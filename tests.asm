TEST_STATE .rs 1
TEST_MODE .rs 1
TEST_SRAM_FAILED .rs 1
TEST_CHR_RAM_FAILED .rs 1

do_tests:
  lda TEST_MODE
  eor #$FF
  sta TEST_MODE
  lda #%00000000 ; disable PPU
  sta $2000
  sta $2001
  jsr enable_chr_write
  jsr enable_prg_ram
  lda #$00
  sta TEST_STATE ; writing
  sta TEST_SRAM_FAILED
  sta TEST_CHR_RAM_FAILED
.sram:
  jsr random_init
  lda #$03 ; init SRAM bank
  sta LOADER_GAME_SAVE_BANK
.sram_test_loop_bank:
  jsr beep
  lda LOADER_GAME_SAVE_BANK
  sta $5005 ; SRAM bank
  lda #$00
  sta COPY_DEST_ADDR
  lda #$60
  sta COPY_DEST_ADDR+1
  ldy #$00
  ldx #$20
.sram_test_loop:
  lda TEST_STATE ; reading or writing?
  bne .sram_test_read
  jsr random ; writing
  lda RANDOM
  eor TEST_MODE
  sta [COPY_DEST_ADDR], y
  jmp .sram_test_next
.sram_test_read:
  jsr random ; reading
  lda RANDOM
  eor TEST_MODE
  cmp [COPY_DEST_ADDR], y  
  beq .sram_test_next
  lda #1
  sta TEST_SRAM_FAILED
.sram_test_next:
  iny
  bne .sram_test_loop
  inc COPY_SOURCE_ADDR+1
  inc COPY_DEST_ADDR+1
  dex
  bne .sram_test_loop
  lda #$00 ; some bus tests
  sta $E000
  lda #$FF
  sta $E001
  dec LOADER_GAME_SAVE_BANK
  bpl .sram_test_loop_bank
  lda TEST_STATE
  bne .chr
  inc TEST_STATE
  jmp .sram

.chr:  
  lda #$00
  sta TEST_STATE ; writing
.chr_again:  
  jsr random_init
  lda #31
  sta LOADER_CHR_LEFT  
  
.chr_test_loop_bank:
  jsr beep
  lda LOADER_CHR_LEFT
  sta $5003
  lda #$00
  sta $2006  
  sta $2006
  ldy #$00
  ldx #$20
  lda TEST_STATE ; need to discard first read
  beq .chr_test_loop
  lda $2007
.chr_test_loop:
  lda TEST_STATE ; reading or writing?
  bne .chr_test_read
  jsr random ; writing
  lda RANDOM
  eor TEST_MODE
  sta $2007
  jmp .chr_test_next
.chr_test_read:
  jsr random ; reading
  lda RANDOM
  eor TEST_MODE
  cmp $2007
  beq .chr_test_next
  lda #1
  sta TEST_CHR_RAM_FAILED
.chr_test_next:
  iny
  bne .chr_test_loop
  dex
  bne .chr_test_loop
  dec LOADER_CHR_LEFT
  bpl .chr_test_loop_bank
  lda TEST_STATE
  bne .tests_end
  inc TEST_STATE
  jmp .chr_again

  ; results
.tests_end:
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
  ldx TEST_SRAM_FAILED
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
  ora TEST_SRAM_FAILED
  ora TEST_CHR_RAM_FAILED
  beq .do_tests_ok
  jsr error_sound
.do_tests_stop:
  jmp .do_tests_wait
.do_tests_ok:
  jmp do_tests
