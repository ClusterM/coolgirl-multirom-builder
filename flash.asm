	; субрутины для работы с флеш-памятью
	; OMG, это работает!
flash_writer:
sector_erase:
	jsr flash_set_superbank
	lda #%00001111  ; mirroring, flash-write, chr-write, enable sram
	sta $5007 ; to enable flash writes
		
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

	lda #%00001011  ; mirroring, chr-write, enable sram
	sta $5007 ; to disable flash writes
	
wait_for_sector_erase:
	lda $8000
	cmp #$FF
	bne wait_for_sector_erase
	jsr flash_set_superbank_zero
	rts
	
write_flash:
	jsr flash_set_superbank
	lda #%00001111  ; mirroring, flash-write, chr-write, enable sram
	sta $5007		; включаем запись в PRG 
	ldy #$00
	ldx #$20
write_flash_loop:
	lda #$F0
	sta $8000 ; write_prg_flash_command(0x0000, 0xF0);
	lda #$AA
	sta $8AAA ; write_prg_flash_command(0x0AAA, 0xAA);
	lda #$55
	sta $8555 ; write_prg_flash_command(0x0555, 0x55);
	lda #$A0
	sta $8AAA ;	write_prg_flash_command(0x0AAA, 0xA0);	
	lda [COPY_SOURCE_ADDR], y
	sta [COPY_DEST_ADDR], y
write_flash_check1:
	lda [COPY_DEST_ADDR], y
	cmp [COPY_SOURCE_ADDR], y
	bne write_flash_check1
write_flash_check2:
	lda [COPY_DEST_ADDR], y
	cmp [COPY_SOURCE_ADDR], y
	bne write_flash_check2
	iny
	bne write_flash_loop
	inc COPY_SOURCE_ADDR+1
	inc COPY_DEST_ADDR+1
	dex
	bne write_flash_loop	
	lda #%00001011  ; mirroring, chr-write, enable sram
	sta $5007	; to disable flash writes
	jsr flash_set_superbank_zero
	rts

read_flash:
	jsr flash_set_superbank
	ldy #0
	ldx #$20
load_save_again:
	lda [COPY_SOURCE_ADDR], y
	sta [COPY_DEST_ADDR], y
	iny
	bne load_save_again
	inc COPY_SOURCE_ADDR+1
	inc COPY_DEST_ADDR+1
	dex
	bne load_save_again
	jsr flash_set_superbank_zero
	rts
	
flash_set_superbank:
	ldx LOADER_GAME_SAVE_SUPERBANK
	inx
	lda #$FF
	sta $5000
	lda #$00
flash_set_superbank_calc_next:	
	sec
	sbc #$02
	dex
	bne flash_set_superbank_calc_next
	sta $5001
	rts

flash_set_superbank_zero:
	lda #$00
	sta $5000
	sta $5001
	rts
