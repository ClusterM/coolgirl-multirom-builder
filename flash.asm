  ; subroutines for flash memory stuff

FLASH_TYPE .rs 1 ; flash memory type

flash_detect:
  lda #0
  sta <FLASH_TYPE
  jsr enable_flash_write
  ; enter flash CFI mode
  lda #$98
  sta $8AAA ; $98 -> $0AAA
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
  sta $8000 ; $F0 -> $0000
  lda #$AA
  sta $8AAA ; $AA -> $0AAA
  lda #$55
  sta $8555 ; $55 -> $0555
  lda #$80
  sta $8AAA ; $80 -> $0AAA
  lda #$AA
  sta $8AAA ; $AA -> $0AAA
  lda #$55
  sta $8555 ; $55 -> $0555
  lda #$30
  sta $8000 ; $30 -> $0000
  jsr disable_flash_write
.wait:
  lda $8000
  cmp #$FF
  bne .wait
  jsr banking_init
  rts

write_flash:
  jsr enable_flash_write
  jsr flash_set_superbank
  ldy #$00
  ldx #$20
.loop:
  lda #$F0
  sta $8000 ; $F0 -> $0000
  lda #$AA
  sta $8AAA ; $AA -> $0AAA
  lda #$55
  sta $8555 ; $55 -> $0555
  lda #$A0
  sta $8AAA ; $A0 -> $0AAA
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
  inc <COPY_SOURCE_ADDR+1
  inc <COPY_DEST_ADDR+1
  dex
  bne .loop
  jsr disable_flash_write
  jsr banking_init
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
  inc <COPY_SOURCE_ADDR+1
  inc <COPY_DEST_ADDR+1
  dex
  bne .loop
  jsr banking_init
  rts

  ; calculate superbank based on save ID
flash_set_superbank:
  lda #0
  sta <PRG_BANK
  ldx <LOADER_GAME_SAVE_SUPERBANK
  inx
  lda #$00
  sta <PRG_SUPERBANK
  sta <PRG_SUPERBANK+1
.loop:
  sec
  lda <PRG_SUPERBANK
  sbc #$02
  sta <PRG_SUPERBANK
  lda <PRG_SUPERBANK+1
  sbc #0
  sta <PRG_SUPERBANK+1
  dex
  bne .loop
  jsr sync_banks
  rts
