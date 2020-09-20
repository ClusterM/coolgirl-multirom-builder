  ; subroutines for flash memory stuff

FLASH_TYPE .rs 1 ; flash memory type

flash_detect:
  lda #0
  sta <FLASH_TYPE
  jsr enable_flash_write
  ; enter flash CFI mode
  lda #$98
  sta $80AA
  ; check for CFI signature
  lda $8020
  cmp #'Q'
  bne .end
  lda $8022
  cmp #'R'
  bne .end
  lda $8024
  cmp #'Y'
  bne .end
  ; if signature is ok read flash size
  lda $804E
  sta <FLASH_TYPE
.end:
  ; exit CFI mode
  lda #$F0
  sta $8000
  jsr disable_flash_write
  rts

sector_erase:
  jsr enable_flash_write
  jsr flash_set_superbank
  lda #$F0
  sta $8000 ; write_prg_flash_command(0x0000, 0xF0);
  lda #$AA
  sta $8AAA ; write_prg_flash_command(0x0AAA, 0xAA);
  lda #$55
  sta $8555 ; write_prg_flash_command(0x0555, 0x55);
  lda #$80
  sta $8AAA ; write_prg_flash_command(0x0AAA, 0x80);
  lda #$AA
  sta $8AAA ; write_prg_flash_command(0x0AAA, 0xAA);
  lda #$55
  sta $8555 ; write_prg_flash_command(0x0555, 0x55);
  lda #$30
  sta $8000 ; write_prg_flash_command(0x0000, 0x30);
  jsr disable_flash_write
.wait:
  lda $8000
  cmp #$FF
  bne .wait
  jsr flash_set_superbank_zero
  rts
  
write_flash:
  jsr enable_flash_write
  jsr flash_set_superbank
  ldy #$00
  ldx #$20
.loop:
  lda #$F0
  sta $8000 ; write_prg_flash_command(0x0000, 0xF0);
  lda #$AA
  sta $8AAA ; write_prg_flash_command(0x0AAA, 0xAA);
  lda #$55
  sta $8555 ; write_prg_flash_command(0x0555, 0x55);
  lda #$A0
  sta $8AAA ; write_prg_flash_command(0x0AAA, 0xA0);  
  lda [COPY_SOURCE_ADDR], y
  sta [COPY_DEST_ADDR], y
.check1:
  lda [COPY_DEST_ADDR], y
  cmp [COPY_SOURCE_ADDR], y
  bne .check1
.check2:
  lda [COPY_DEST_ADDR], y
  cmp [COPY_SOURCE_ADDR], y
  bne .check2
  iny
  bne .loop
  inc COPY_SOURCE_ADDR+1
  inc COPY_DEST_ADDR+1
  dex
  bne .loop
  jsr disable_flash_write
  jsr flash_set_superbank_zero
  rts

read_flash:
  jsr flash_set_superbank
  ldy #0
  ldx #$20
.loop:
  lda [COPY_SOURCE_ADDR], y
  sta [COPY_DEST_ADDR], y
  iny
  bne .loop
  inc COPY_SOURCE_ADDR+1
  inc COPY_DEST_ADDR+1
  dex
  bne .loop
  jsr flash_set_superbank_zero
  rts
  
flash_set_superbank:
  lda #0
  jsr select_prg_bank
  ldx LOADER_GAME_SAVE_SUPERBANK
  inx
  lda #$FF
  sta $5000
  lda #$00
.loop:
  sec
  sbc #$02
  dex
  bne .loop
  sta $5001
  rts

flash_set_superbank_zero:
  lda #$00
  sta $5000
  sta $5001
  lda #%11111000
  sta $5002
  rts
