  ; subroutines for flash memory stuff

FLASH_TYPE .rs 1 ; flash memory type
CRC .rs 2

flash_detect:
  lda #0
  sta <FLASH_TYPE
  jsr enable_flash_write
  ; enter flash CFI mode
  lda #$98
  sta $8AAA
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
  jsr banking_init
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
  lda #$FF
  sta <PRG_SUPERBANK+1
  lda #$00
.loop:
  sec
  sbc #$02
  dex
  bne .loop
  sta PRG_SUPERBANK
  jsr sync_banks
  rts

  ; calculate next CRC based on value in A
crc_calc:
  eor <CRC
  sta <CRC
  txa
  pha
  ldx #8
.loop:
  clc
  ror <CRC+1
  ror <CRC
  bcc .noxor
  lda #$01
  eor <CRC
  sta <CRC
  lda #$A0
  eor <CRC+1
  sta <CRC+1
.noxor:
  dex
  bne .loop
  pla
  tax
  rts

  ; calculate CRC of $8000-$BFFF
crc_calc_16k:
  txa
  pha
  tya
  pha
  lda #0
  sta <COPY_SOURCE_ADDR
  lda #$80
  sta <COPY_SOURCE_ADDR+1
  ldx #$40
  ldy #0
.loop:
  lda [COPY_SOURCE_ADDR], y
  jsr crc_calc
  iny
  bne .loop
  inc <COPY_SOURCE_ADDR+1
  dex
  bne .loop
  pla
  tay
  pla
  tax
  rts

  ; calculcate CRC for current 128K superbank
crc_calc_128k:
  txa
  pha
  tya
  pha
  ; reset CRC
  lda #0
  sta <CRC
  sta <CRC+1
  ldx #8
.loop:
  jsr sync_banks
  jsr crc_calc_16k
  clc
  lda <PRG_SUPERBANK
  adc #1
  sta <PRG_SUPERBANK
  lda <PRG_SUPERBANK+1
  adc #0
  sta <PRG_SUPERBANK+1
  dex
  bne .loop
  pla
  tay
  pla
  tax
  rts

  ; calculcate CRCs for every 128K superbank
crc_calc_128m:
  jsr enable_prg_ram
  lda #3
  sta <PRG_RAM_BANK
  lda #0
  sta <PRG_BANK
  sta <PRG_SUPERBANK
  sta <PRG_SUPERBANK+1
  sta <COPY_DEST_ADDR
  lda #$60
  sta <COPY_DEST_ADDR+1
.loop:
  jsr crc_calc_128k
  ldy #0
  lda <CRC
  sta [COPY_DEST_ADDR], y
  inc <COPY_DEST_ADDR
  lda <CRC+1
  sta [COPY_DEST_ADDR], y
  inc <COPY_DEST_ADDR
  bne .loop
  inc <COPY_DEST_ADDR+1
  lda <COPY_DEST_ADDR+1
  cmp #$68
  bne .loop
  jsr disable_prg_ram
  lda #0
  sta <PRG_BANK
  sta <PRG_SUPERBANK
  sta <PRG_SUPERBANK+1
  jsr sync_banks
  rts
