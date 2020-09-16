SAVES .rs 4				; ��� ����� ����������

save_state:
	; saving last started game
	lda #0 ; first SRAM bank
	sta $5005
	; storing signature
	lda #'C'
	sta SRAM_SIG
	lda #'L'
	sta SRAM_SIG+1
	lda #'U'
	sta SRAM_SIG+2
  ; storing selected game
	lda SELECTED_GAME
	sta SRAM_LAST_STARTED_GAME
	lda SELECTED_GAME+1
	sta SRAM_LAST_STARTED_GAME+1
  ; storing scrolling state
	lda SCROLL_LINES_TARGET
	sta SRAM_LAST_STARTED_LINE
	lda SCROLL_LINES_TARGET+1
	sta SRAM_LAST_STARTED_LINE+1
  ; storing save ID of last started game
	lda LAST_STARTED_SAVE
	sta SRAM_LAST_STARTED_SAVE 
	rts
	
load_state:
  ; loading saved state
	lda #0 ; first SRAM bank
	sta $5005
	; check for signature
	lda SRAM_SIG
	cmp #'C'
	bne .end
	lda SRAM_SIG+1
	cmp #'L'
	bne .end
	lda SRAM_SIG+2
	cmp #'U'
	bne .end
  ; loading last started game
	lda SRAM_LAST_STARTED_GAME
	sta SELECTED_GAME
	lda SRAM_LAST_STARTED_GAME+1
	sta SELECTED_GAME+1
	; check for invalid value
	lda SELECTED_GAME
	sec
	sbc games_count
	lda SELECTED_GAME+1
	sbc games_count+1	
	bcs .ovf
  ; loading scrolling state
	lda SRAM_LAST_STARTED_LINE
	sta SCROLL_LINES_TARGET
	lda SRAM_LAST_STARTED_LINE+1
	sta SCROLL_LINES_TARGET+1
  ; loading last save ID
	lda SRAM_LAST_STARTED_SAVE
	sta LAST_STARTED_SAVE
.end:
	rts
.ovf:
	; reset values
	lda #0
	sta SELECTED_GAME
	sta SELECTED_GAME+1
	sta SCROLL_LINES_TARGET
	sta SCROLL_LINES_TARGET+1
	rts
	
load_save:
  ; loading battery backed save for game if any
	pha
	tya
	pha
	txa
	pha
	
	lda	LOADER_GAME_SAVE
	beq .done ; game has not battery backed saves
	; superbank number
	sta LOADER_GAME_SAVE_SUPERBANK
	dec LOADER_GAME_SAVE_SUPERBANK
	lda LOADER_GAME_SAVE_BANK
	; � �������
	sta $5005
	lda #0
	sta COPY_SOURCE_ADDR
	sta COPY_DEST_ADDR
	lda #$80
	sta COPY_SOURCE_ADDR+1
	lda #$60
	sta COPY_DEST_ADDR+1
	jsr read_flash
.done:
	pla
	tax
	pla
	tay
	pla
	rts
	
	; �� �� �� �����, ������ � �������� �������
save_save:
	pha
	tya
	pha
	txa
	pha

	lda	LOADER_GAME_SAVE
	beq .done ; ���� ���� �� ���������� �����, �� ��
	; ����� ����������
	sta LOADER_GAME_SAVE_SUPERBANK
	dec LOADER_GAME_SAVE_SUPERBANK
	lda LOADER_GAME_SAVE_BANK
	; � �������
	sta $5005
	lda #0
	sta COPY_SOURCE_ADDR
	sta COPY_DEST_ADDR
	lda #$60
	sta COPY_SOURCE_ADDR+1
	lda #$80
	sta COPY_DEST_ADDR+1
	jsr write_flash
.done:
	pla
	tax
	pla
	tay
	pla
	rts

save_all_saves:
	ldx LAST_STARTED_SAVE
	bne .there_is_save
	jmp .done
.there_is_save:	
	; ������ �����
	lda #%00000000 ; ��������� ���� ��� PPU
	sta $2000
	sta $2001
	jsr waitblank_simple
	jsr clear_screen
	lda #$21
	sta $2006
	lda #$C0
	sta $2006
	ldy #0
.print_warning:
	lda saving_text, y
	sta $2007
	iny
	cmp #0 ; ����� ������������ ���� �������� ������ �������
	bne .print_warning
	lda #$23
	sta $2006
	lda #$C8
	sta $2006
	lda #$FF
	ldy #$38
.print_warning_palette:
	sta $2007
	dey
	bne .print_warning_palette
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

	ldx LAST_STARTED_SAVE
	dex
	txa
	and #%11111100 ; ����� ������� ���������� � ������
	ora #1 ; ���� ����
	sta LOADER_GAME_SAVE
	lda #0
	sta LOADER_GAME_SAVE_BANK
	
	; �������� ��� �����
	ldx #3
.load_all_saves:
	; ���� ��� � ���� ��������� ���������, �� ����������
	lda LOADER_GAME_SAVE
	cmp LAST_STARTED_SAVE
	bne .load_all_saves_skip1
	inc LOADER_GAME_SAVE	
.load_all_saves_skip1:
	; ���� ��� ������ ����, �� ���� �� �������
	lda LOADER_GAME_SAVE_BANK
	cmp #2
	bne .load_all_saves_skip2
	inc LOADER_GAME_SAVE_BANK
.load_all_saves_skip2:
	; ���������� � ������ - ��� ����� ���������
	lda LOADER_GAME_SAVE
	ldy LOADER_GAME_SAVE_BANK
	sta SAVES, y
	jsr load_save
	inc LOADER_GAME_SAVE	
	inc LOADER_GAME_SAVE_BANK	
	dex
	bne .load_all_saves
	
	; � �� ������ ����� � ��� ������ ��������� ���������
	ldx LAST_STARTED_SAVE
	txa
	ldy #2
	sta SAVES, y
	dex ; ���������� ������ �������
	txa
	ora #%00000011
	sta LOADER_GAME_SAVE_SUPERBANK ; ����� ����������	
	lda #0
	sta $5005 ; ������� ����

	; ������� ������
	jsr sector_erase
	
	; � ������ ���������� ������ ����� �����
	ldy #0
.write:
	lda SAVES, y
	sta LOADER_GAME_SAVE
	sty LOADER_GAME_SAVE_BANK
	jsr save_save
	iny
	cpy #4
	bne .write

.done:
	lda #0
	sta LAST_STARTED_SAVE ; ������� ���������, ��, �� ��� ���� ����� ������� � SRAM
	jsr save_state
	lda #%00000000 ; ��������� ���� ��� PPU
	sta $2000
	sta $2001
	jsr waitblank_simple
	jsr clear_screen
	rts
