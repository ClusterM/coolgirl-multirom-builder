LOADER_REG_0 .rs 1
LOADER_REG_1 .rs 1
LOADER_REG_2 .rs 1
LOADER_REG_3 .rs 1
LOADER_REG_4 .rs 1
LOADER_REG_5 .rs 1
LOADER_REG_6 .rs 1
LOADER_REG_7 .rs 1
LOADER_CHR_START_H .rs 1
LOADER_CHR_START_L .rs 1
LOADER_CHR_START_S .rs 1
LOADER_CHR_LEFT .rs 1
LOADER_GAME_SAVE .rs 1
LOADER_GAME_SAVE_BANK .rs 1
LOADER_GAME_SAVE_SUPERBANK .rs 1

loader:
  ; loading game
  ; setting all registers
  lda LOADER_REG_0
  sta $5000
  lda LOADER_REG_1
  sta $5001
  lda LOADER_REG_2
  sta $5002
  lda LOADER_REG_3
  sta $5003  
  lda LOADER_REG_4
  sta $5004
  lda LOADER_REG_5
  sta $5005
  lda LOADER_REG_6
  sta $5006
  lda LOADER_REG_7
  sta $5007
  ; jumping to cleaner
  jmp loader_clean_and_start

  ; dirty trick :)
loader_end:
  .org $07E0
loader_clean_and_start:
  ; clean memory
  lda #$00
  sta <COPY_SOURCE_ADDR
  sta <COPY_SOURCE_ADDR+1
  ldy #$02
  ldx #$07
.loop:
  sta [COPY_SOURCE_ADDR], y
  iny
  bne .loop
  inc <COPY_SOURCE_ADDR+1
  dex
  bne .loop
.loop2:
  sta [COPY_SOURCE_ADDR], y
  iny
  cpy #LOW(.loop2) ; to the very end
  bne .loop2  
  ; Start game!
  jmp [$FFFC]
  .org loader_end

load_all_chr_banks:
  ; loading tiles to CHR RAM for all banks
  ; how many 8KB parts left?
  lda #0
  sta <CHR_BANK
  sta <PRG_BANK
  sta <COPY_SOURCE_ADDR
  lda <LOADER_CHR_START_L
  sta <PRG_SUPERBANK
  lda <LOADER_CHR_START_H
  sta <PRG_SUPERBANK+1
.loop
  lda <LOADER_CHR_LEFT
  beq .done
  dec <LOADER_CHR_LEFT
  jsr sync_banks
  ; source address
  lda <LOADER_CHR_START_S
  sta <COPY_SOURCE_ADDR+1
  ; load 8KB of CHR
  jsr load_chr
  ; calculating next source address
  lda <LOADER_CHR_START_S
  clc
  adc #$20
  sta <LOADER_CHR_START_S
  cmp #$C0
  ; is it overflowed?
  bne .chr_s_not_inc
  ; yes
  lda #$80
  sta <LOADER_CHR_START_S
  lda <PRG_SUPERBANK  
  clc
  adc #1
  sta <PRG_SUPERBANK
  lda <PRG_SUPERBANK+1
  adc #0
  sta <PRG_SUPERBANK+1  
.chr_s_not_inc:
  ; increase target CHR bank number
  inc <CHR_BANK
  jmp .loop
.done:
  jsr banking_init
  rts

  ; loading tiles to CHR RAM
load_chr:
  jsr enable_chr_write
  lda #$00
  sta $2006
  sta $2006
  ldy #$00
  ldx #$20
.loop:
  lda [COPY_SOURCE_ADDR], y
  sta $2007
  iny
  bne .loop
  inc <COPY_SOURCE_ADDR+1
  dex
  bne .loop
  jsr disable_chr_write
  rts
