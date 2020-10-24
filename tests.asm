TEST_RW .rs 1
TEST_XOR .rs 1
TEST_PRG_RAM_FAILED .rs 1
TEST_CHR_RAM_FAILED .rs 1
TEST_BANK .rs 1

do_tests:
  ; detect CHR RAM size
  jsr detect_chr_ram_size
do_tests_again:
  ; invert TEST_XOR
  lda <TEST_XOR
  eor #$FF
  sta <TEST_XOR
  lda #%00000000 ; disable PPU
  sta $2000
  sta $2001
  jsr waitblank_simple
  jsr enable_prg_ram
  lda #$00
  ; writing
  sta <TEST_RW
  ; reset result
  sta <TEST_PRG_RAM_FAILED
  sta <TEST_CHR_RAM_FAILED
.sram:
  jsr random_init
  ; select bank
  lda #(PRG_RAM_BANKS-1)
  sta <TEST_BANK
.sram_test_loop_bank:
  jsr beep
  lda <TEST_BANK
  jsr select_prg_ram_bank
  lda #$00
  sta <COPY_DEST_ADDR
  lda #$60
  sta <COPY_DEST_ADDR+1
  ldy #$00
  ldx #$20
.sram_test_loop:
  jsr random ; generate next random number
  lda <TEST_RW ; reading or writing?
  bne .sram_test_read
  lda <RANDOM
  eor <TEST_XOR
  sta [COPY_DEST_ADDR], y
  jmp .sram_test_next
.sram_test_read:
  lda <RANDOM
  eor <TEST_XOR
  cmp [COPY_DEST_ADDR], y  
  beq .sram_test_next
  lda #1
  sta TEST_PRG_RAM_FAILED
.sram_test_next:
  iny
  bne .sram_test_loop
  inc COPY_SOURCE_ADDR+1
  inc COPY_DEST_ADDR+1
  dex
  bne .sram_test_loop
  dec <TEST_BANK
  bpl .sram_test_loop_bank
  lda <TEST_RW
  bne .chr
  inc <TEST_RW
  jmp .sram

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
  sta $2006  
  sta $2006
  ldy #$00
  ldx #$20
  lda <TEST_RW
  beq .chr_test_loop
  lda $2007 ; need to discard first read
.chr_test_loop:
  jsr random ; generate next random number
  lda <TEST_RW ; reading or writing?
  bne .chr_test_read
  lda <RANDOM
  eor <TEST_XOR
  sta $2007
  jmp .chr_test_next
.chr_test_read:
  lda <RANDOM
  eor <TEST_XOR
  cmp $2007
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
  lda #0
  jsr select_chr_bank
  jsr load_base_chr
  jsr clear_screen
  jsr load_text_palette
  lda #$21
  sta $2006
  lda #$A4
  sta $2006
  lda #LOW(string_prg_ram_test)
  sta <COPY_SOURCE_ADDR
  lda #HIGH(string_prg_ram_test)
  sta <COPY_SOURCE_ADDR+1
  jsr print_text
  ldx TEST_PRG_RAM_FAILED
  bne .sram_test_result_fail
  lda #LOW(string_passed)
  sta <COPY_SOURCE_ADDR
  lda #HIGH(string_passed)
  sta <COPY_SOURCE_ADDR+1
  jmp .sram_test_result_print
.sram_test_result_fail:
  lda #LOW(string_failed)
  sta <COPY_SOURCE_ADDR
  lda #HIGH(string_failed)
  sta <COPY_SOURCE_ADDR+1
.sram_test_result_print:
  jsr print_text
  
  lda #$21
  sta $2006
  lda #$E4
  sta $2006
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
  sta $2005
  sta $2005    
  jsr waitblank_simple
  lda #%00001010 ; enable PPU and show result
  sta $2001
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

crc_tests:
  ; disable PPU
  lda #%00000000
  sta $2000
  sta $2001
  jsr waitblank_simple
  jsr clear_screen
  lda #$21
  sta $2006
  lda #$C0
  sta $2006
  lda #LOW(string_calculating_crc)
  sta COPY_SOURCE_ADDR
  lda #HIGH(string_calculating_crc)
  sta COPY_SOURCE_ADDR+1
  jsr print_text
  jsr load_text_palette
  jsr waitblank_simple
  bit $2002
  lda #0
  sta $2005
  sta $2005
  lda #%00001000
  sta $2000
  lda #%00001010
  sta $2001
  jsr waitblank_simple
  jsr crc_calc_128m
  jsr start_sound
  ; disable PPU
  lda #%00000000
  sta $2000
  sta $2001
  jsr waitblank_simple
  rts
