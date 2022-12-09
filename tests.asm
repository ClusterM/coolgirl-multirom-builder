TEST_RW .rs 1
TEST_XOR .rs 1
TEST_PRG_RAM_FAILED .rs 1
TEST_CHR_RAM_FAILED .rs 1
TEST_BANK .rs 1

do_tests:
  ; detect CHR RAM size
  jsr detect_chr_ram_size
  ; check presense of PRG RAM
  jsr prg_ram_detect
do_tests_again:
  ; invert TEST_XOR
  lda <TEST_XOR
  eor #$FF
  sta <TEST_XOR
  lda #%00000000 ; disable PPU
  sta PPUCTRL
  sta PPUMASK
  jsr waitblank_simple

  lda #$00
  ; writing
  sta <TEST_RW
  ; reset result
  sta <TEST_PRG_RAM_FAILED
  sta <TEST_CHR_RAM_FAILED
  
  lda PRG_RAM_PRESENT
  beq .chr
  jsr enable_prg_ram
.prg_ram:
  jsr random_init
  ; select bank
  lda #(PRG_RAM_BANKS-1)
  sta <TEST_BANK
.prg_ram_test_loop_bank:
  jsr beep
  lda <TEST_BANK
  jsr select_prg_ram_bank
  lda #$00
  sta <COPY_DEST_ADDR
  lda #$60
  sta <COPY_DEST_ADDR+1
  ldy #$00
  ldx #$20
.prg_ram_test_loop:
  jsr random ; generate next random number
  lda <TEST_RW ; reading or writing?
  bne .prg_ram_test_read
  lda <RANDOM
  eor <TEST_XOR
  sta [COPY_DEST_ADDR], y
  jmp .prg_ram_test_next
.prg_ram_test_read:
  lda <RANDOM
  eor <TEST_XOR
  cmp [COPY_DEST_ADDR], y
  beq .prg_ram_test_next
  lda #1
  sta TEST_PRG_RAM_FAILED
.prg_ram_test_next:
  iny
  bne .prg_ram_test_loop
  inc COPY_SOURCE_ADDR+1
  inc COPY_DEST_ADDR+1
  dex
  bne .prg_ram_test_loop
  ; test for corruption on wrong /CE delay
  lda #00
  sta $E000
  sta $F000
  dec <TEST_BANK
  bpl .prg_ram_test_loop_bank
  lda <TEST_RW
  bne .chr
  inc <TEST_RW
  jmp .prg_ram

.chr:
  jsr disable_prg_ram
  lda #$00
  sta TEST_RW ; writing
.chr_again:
  jsr random_init
  lda #1
  ldx CHR_RAM_SIZE
  ; shift 1 to the left CHR_RAM_SIZE times
.shift_loop:
  dex
  bmi .shift_done
  asl A
  jmp .shift_loop
.shift_done:
  ; minus 1
  sec
  sbc #1
  sta <TEST_BANK
  jsr enable_chr_write
.chr_test_loop_bank:
  jsr beep
  lda <TEST_BANK
  jsr select_chr_bank
  lda #$00
  sta PPUADDR
  sta PPUADDR
  ldy #$00
  ldx #$20
  lda <TEST_RW
  beq .chr_test_loop
  lda PPUDATA ; need to discard first read
.chr_test_loop:
  jsr random ; generate next random number
  lda <TEST_RW ; reading or writing?
  bne .chr_test_read
  lda <RANDOM
  eor <TEST_XOR
  sta PPUDATA
  jmp .chr_test_next
.chr_test_read:
  lda <RANDOM
  eor <TEST_XOR
  cmp PPUDATA
  beq .chr_test_next
  lda #1
  sta <TEST_CHR_RAM_FAILED
.chr_test_next:
  iny
  bne .chr_test_loop
  dex
  bne .chr_test_loop
  dec <TEST_BANK
  bpl .chr_test_loop_bank
  lda <TEST_RW
  bne .tests_end
  inc <TEST_RW
  jmp .chr_again

  ; results
.tests_end:
  jsr load_base_chr
  jsr clear_screen
  jsr load_text_attributes
  jsr preload_base_palette
  jsr load_palette

  ; PRG RAM test result  
  lda #$21
  sta PPUADDR
  lda #$A4
  sta PPUADDR
  lda #LOW(string_prg_ram_test)
  sta <COPY_SOURCE_ADDR
  lda #HIGH(string_prg_ram_test)
  sta <COPY_SOURCE_ADDR+1
  jsr print_text
  ; is it present?
  lda PRG_RAM_PRESENT
  bne .prg_ram_test_result
  ; PRG RAM is not present
  lda #LOW(string_not_available)
  sta <COPY_SOURCE_ADDR
  lda #HIGH(string_not_available)
  sta <COPY_SOURCE_ADDR+1
  jmp .prg_ram_test_result_print
.prg_ram_test_result:
  ldx TEST_PRG_RAM_FAILED
  bne .prg_ram_test_result_fail
  ; PRG RAM test OK
  lda #LOW(string_passed)
  sta <COPY_SOURCE_ADDR
  lda #HIGH(string_passed)
  sta <COPY_SOURCE_ADDR+1
  jmp .prg_ram_test_result_print
.prg_ram_test_result_fail:
  ; PRG RAM test failed
  lda #LOW(string_failed)
  sta <COPY_SOURCE_ADDR
  lda #HIGH(string_failed)
  sta <COPY_SOURCE_ADDR+1
  jmp .prg_ram_test_result_print
.prg_ram_test_result_print:
  jsr print_text

  ; CHR RAM test result
  lda #$21
  sta PPUADDR
  lda #$E4
  sta PPUADDR
  lda #LOW(string_chr_ram_test)
  sta <COPY_SOURCE_ADDR
  lda #HIGH(string_chr_ram_test)
  sta <COPY_SOURCE_ADDR+1
  jsr print_text
  lda <CHR_RAM_SIZE
  asl A
  tay
  lda chr_ram_sizes, y
  sta <COPY_SOURCE_ADDR
  lda chr_ram_sizes+1, y
  sta <COPY_SOURCE_ADDR+1
  jsr print_text
  ldx TEST_CHR_RAM_FAILED
  bne .chr_test_result_fail
  lda #LOW(string_passed)
  sta <COPY_SOURCE_ADDR
  lda #HIGH(string_passed)
  sta <COPY_SOURCE_ADDR+1
  jmp .chr_test_result_print
.chr_test_result_fail:
  lda #LOW(string_failed)
  sta <COPY_SOURCE_ADDR
  lda #HIGH(string_failed)
  sta <COPY_SOURCE_ADDR+1
.chr_test_result_print:
  jsr print_text
  lda #0
  sta PPUSCROLL
  sta PPUSCROLL
  jsr waitblank_simple
  lda #%00001010 ; enable PPU and show result
  sta PPUMASK
  ldx #$FF
.do_tests_wait:
  jsr waitblank_simple
  dex
  bne .do_tests_wait
  lda #0
  ora TEST_PRG_RAM_FAILED
  ora TEST_CHR_RAM_FAILED
  beq .do_tests_ok
  jsr error_sound
.do_tests_stop:
  jmp .do_tests_wait
.do_tests_ok:
  jmp do_tests_again
