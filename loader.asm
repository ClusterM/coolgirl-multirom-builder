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
LOADER_CHR_COUNT .rs 1
LOADER_GAME_SAVE .rs 1
LOADER_GAME_SAVE_BANK .rs 1
LOADER_GAME_SAVE_SUPERBANK .rs 1

loader:
  ; loading game
  jsr load_all_chr_banks ; load tiles
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

load_all_chr_banks:
	; loading tiles to CHR RAM for all banks
	; how many 8KB parts left?
	ldx <LOADER_CHR_LEFT
	beq .done
	dec <LOADER_CHR_LEFT
	; high address byte
	ldx <LOADER_CHR_START_H
	stx $5000
	; low address byte
	lda <LOADER_CHR_START_L
	sta $5001
	; mask for 8KB banks
	ldx #$FE
	stx $5002
	; target CHR bank
	ldx <LOADER_CHR_COUNT
	stx $5003
	; source address
	ldx #$00
	stx <COPY_SOURCE_ADDR
  ; source address overflow check
	ldx <LOADER_CHR_START_S
	bne .chr_not_null
	ldx #$80
	stx <LOADER_CHR_START_S
.chr_not_null:
	stx <COPY_SOURCE_ADDR+1
  ; load 8KB of CHR
	jsr load_chr
  ; calculating next source address
	lda <LOADER_CHR_START_S
	clc
	adc #$20
	sta <LOADER_CHR_START_S
  ; is it overflowed?
	bcc .chr_s_not_inc
  ; yes, increase source address for two banks
	lda <LOADER_CHR_START_L	
	clc
	adc #2
	sta <LOADER_CHR_START_L
	lda <LOADER_CHR_START_H
	adc #0
	sta <LOADER_CHR_START_H	
.chr_s_not_inc:
  ; increase target CHR bank number
	inc <LOADER_CHR_COUNT
	jmp load_all_chr_banks
.done:
  rts

	; loading tiles to CHR RAM
load_chr:
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
	rts

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
