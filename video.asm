	; cursors target coordinates
SPRITE_0_X_TARGET .rs 1
SPRITE_0_Y_TARGET .rs 1
SPRITE_1_X_TARGET .rs 1
SPRITE_1_Y_TARGET .rs 1
	; variables for game name drawing
TEXT_DRAW_GAME .rs 2
TEXT_DRAW_ROW .rs 1
SCROLL_LINES .rs 2 ; current scroll line
SCROLL_LINES_MODULO .rs 1 ; current scroll line % 30
LAST_LINE_MODULO .rs 1
LAST_LINE_GAME .rs 2
SCROLL_FINE .rs 1 ; fine scroll position
SCROLL_LINES_TARGET .rs 2 ; scrolling target
STAR_SPAWN_TIMER .rs 1 ; stars spawn timer

	; constants
CHARS_PER_LINE .equ 32
LINES_PER_SCREEN .equ 30
LINES_PER_VISIBLE_SCREEN .equ 15

waitblank: 
	pha
	tya
	pha
	txa
	pha

	bit $2002 ; reset vblank bit
.loop:
	lda $2002 ; load A with value at location $2002
	bpl .loop  ; if bit 7 is not set (not VBlank) keep checking
	
	; scrolling
	jsr move_scrolling
	jsr scroll_fix	
	; updating sprites
	jsr sprite_dma_copy
	; moving cursors
	jsr move_cursors
	; reading controller 
	jsr read_controller
	; stars on the background
	.if	ENABLE_STARS!=0
	jsr stars
	.endif
	
	pla
	tax
	pla
	tay
	pla
	;plp
	rts
	
waitblank_simple:
	pha
	bit $2002
.loop:
	lda $2002  ; load A with value at location $2002
	bpl .loop  ; if bit 7 is not set (not VBlank) keep checking
	pla
	rts

scroll_fix:
	; scrollong reset
	bit $2002	
	lda #0
	sta $2005
	lda <SCROLL_LINES_MODULO
	cmp #LINES_PER_VISIBLE_SCREEN
	bcc .first_screen ; <?
	sec
	sbc #LINES_PER_VISIBLE_SCREEN ; substracting otherwise
	ldy #%00001010 ; second nametable
	jmp .really	
.first_screen:
	ldy #%00001000 ; first nametable	
.really:
	sty $2000
	asl A
	asl A
	asl A
	asl A
	clc
	adc <SCROLL_FINE
	.if ENABLE_TOP_OFFSET!=0
	sec    ; для больших картинок сверху
	sbc #8 ; для больших картинок сверху
	.endif
	sta $2005
	rts

scroll_line_down:
	lda <SCROLL_LINES
	clc
	adc #1
	sta <SCROLL_LINES
	lda <SCROLL_LINES+1
	adc #0
	sta <SCROLL_LINES+1
	inc <SCROLL_LINES_MODULO
	lda <SCROLL_LINES_MODULO
	cmp #LINES_PER_SCREEN
	bcc .modulo_ok
	lda #0
	sta <SCROLL_LINES_MODULO
.modulo_ok:
	lda <LAST_LINE_GAME
	clc
	adc #1
	sta <LAST_LINE_GAME
	lda <LAST_LINE_GAME+1
	adc #0
	sta <LAST_LINE_GAME+1
	inc <LAST_LINE_MODULO
	lda <LAST_LINE_MODULO
	cmp #LINES_PER_SCREEN
	bcc .modulo_ok2
	lda #0
	sta <LAST_LINE_MODULO
.modulo_ok2:	
	jsr print_last_name
	rts

scroll_line_up:
	lda <SCROLL_LINES
	sec
	sbc #1
	sta <SCROLL_LINES
	lda <SCROLL_LINES+1
	sbc #0
	sta <SCROLL_LINES+1
	dec <SCROLL_LINES_MODULO
	lda <SCROLL_LINES_MODULO
	bpl .modulo_ok
	lda #LINES_PER_SCREEN
	sta <SCROLL_LINES_MODULO
	dec <SCROLL_LINES_MODULO
.modulo_ok:
	lda <LAST_LINE_GAME
	sec
	sbc #1
	sta <LAST_LINE_GAME
	lda <LAST_LINE_GAME+1
	sbc #0
	sta <LAST_LINE_GAME+1
	dec <LAST_LINE_MODULO
	lda <LAST_LINE_MODULO
	bpl .modulo_ok2
	lda #LINES_PER_SCREEN
	sta <LAST_LINE_MODULO
	dec <LAST_LINE_MODULO
.modulo_ok2:	
	jsr print_first_name
	rts

load_base_chr:
	; loading CHR
	lda #$06
	jsr select_bank
	lda #LOW(chr_data)
	sta COPY_SOURCE_ADDR
	lda #HIGH(chr_data)
	sta COPY_SOURCE_ADDR+1
	jsr load_chr
	rts

load_base_pal:
	; loading palette into $3F00 of PPU
	lda #$3F
	sta $2006
	lda #$00
	sta $2006
	ldx #$00
loadpal:
	lda tilepal, x
	sta $2007
	inx
	cpx #32
	bne loadpal

	; letters colors
	lda #$3F
	sta $2006
	lda #$0D
	sta $2006
	ldx #17
loadpal2:
	lda tilepal, x
	sta $2007
	inx
	cpx #20
	bne loadpal2
	rts

load_black:
	; loading empty black palette into $3F00 of PPU
	lda #$3F
	sta $2006
	lda #$00
	sta $2006
	ldx #$00
	lda #$3F ; color
load_black_pal:
	sta $2007
	inx
	cpx #32
	bne load_black_pal
	rts

	; nametable cleanup
clear_screen:
	lda #$20
	sta $2006
	lda #$00
	sta $2006
	lda #$00
	ldx #0
	ldy #$10
.loop:
	sta $2007
	inx
	bne .loop
	dey
	bne .loop
	rts
	
	; clear all sprites data
clear_sprites:
	lda #$FF
	ldx #0
.loop:
	sta SPRITES, x
	inx
	bne .loop
	rts

	; DMA sprites loading
sprite_dma_copy:
	ldx #0
	stx $2003	
	ldx #HIGH(SPRITES)
	stx $4014
	rts
	
	; loading header (image on the top), first part
draw_header1:
	lda #$06
	jsr select_bank
	bit $2002
	lda #$20
	sta $2006
	lda #$00
	sta $2006
	ldx #0
	ldy #$40
.loop:
	lda nametable, x
	sta $2007
	inx
	dey
	bne .loop
	rts
	
	; loading header (image on the top), second part
draw_header2:
	lda #$06
	jsr select_bank
	bit $2002
	lda #$20
	sta $2006
	lda #$40
	sta $2006
	ldx #$40
	ldy #$40
.loop:
	lda nametable, x
	sta $2007
	inx
	dey
	bne .loop
	; loading attributes for it
	lda #$23
	sta $2006
	lda #$C0
	sta $2006
	ldx #0
	ldy #8
.palette_loop:
	lda nametable+$3C0, x
	sta $2007
	inx
	dey
	bne .palette_loop
	rts

	; loading footer (image on the bottom), first part
draw_footer1:
	lda #$06
	jsr select_bank
	ldx #0
	ldy #$40
.loop:
	lda nametable+$340, x
	sta $2007
	inx
	dey
	bne .loop
	rts

	; loading footer (image on the bottom), second part
draw_footer2:
	lda #$06
	jsr select_bank
	ldx #0
	ldy #$40
.loop:
	lda nametable+$380, x
	sta $2007
	inx
	dey
	bne .loop
	rts

	; printing game name on the top
print_first_name:
	pha
	lda <SCROLL_LINES
	sta <TEXT_DRAW_GAME
	lda <SCROLL_LINES+1
	sta <TEXT_DRAW_GAME+1
	lda <SCROLL_LINES_MODULO
	sta <TEXT_DRAW_ROW
	jsr print_name
	pla
	rts

	; printing game name on the bottom
print_last_name:
	pha
	lda <LAST_LINE_GAME
	sta <TEXT_DRAW_GAME
	lda <LAST_LINE_GAME+1
	sta <TEXT_DRAW_GAME+1
	lda <LAST_LINE_MODULO
	sta <TEXT_DRAW_ROW
	jsr print_name
	pla
	rts

print_name:
	pha
	tya
	pha
	txa
	pha	
	; is it header lines?
	lda <TEXT_DRAW_GAME+1
	bne .not_header ; no
	lda <TEXT_DRAW_GAME
	beq .header1
	cmp #1
	beq .header2
	jmp .not_header	
.header1:
	jsr draw_header1
	jmp .end_really_really
.header2:
	jsr draw_header2
	jmp .end_really_really
.not_header:
	; when there are not so many games we need so offset
	lda <TEXT_DRAW_ROW
	clc
	adc games_offset
	sta <TEXT_DRAW_ROW
	asl <TEXT_DRAW_ROW ; x2
	lda <TEXT_DRAW_ROW
	; detecting target nametable
	cmp #LINES_PER_SCREEN
	bcc .first_screen
	; second
	sec
	sbc #LINES_PER_SCREEN
	lsr A
	lsr A
	lsr A
	clc
	adc #$2C
	bit $2002
	sta $2006
	lda <TEXT_DRAW_ROW
	sec
	sbc #LINES_PER_SCREEN
	asl A
	asl A
	asl A
	asl A
	asl A
	sta $2006
	jmp .print_end
	; first
.first_screen:
	lsr A
	lsr A
	lsr A
	clc
	adc #$20
	bit $2002
	sta $2006
	lda <TEXT_DRAW_ROW
	asl A
	asl A
	asl A
	asl A
	asl A
	sta $2006
.print_end:
	; we need to substract 2 from game number
	lda <TEXT_DRAW_GAME
	sec
	sbc #2
	sta <TEXT_DRAW_GAME
	lda <TEXT_DRAW_GAME+1
	sbc #0
	sta <TEXT_DRAW_GAME+1
	; is it footer?
	lda <TEXT_DRAW_GAME
	sec
	sbc games_count
	lda <TEXT_DRAW_GAME+1
	sbc games_count+1
	bcc .print_text_line
	ldx <TEXT_DRAW_GAME
	cpx games_count
	beq .footer1
	dex
	cpx games_count
	beq .footer2
	jmp .end
.footer2:
	jsr draw_footer2
	; lets keep for footer text palette, i like how it looks
	jmp .end
.footer1:
	jsr draw_footer1
	jmp .end
.print_text_line:
	lda <TEXT_DRAW_GAME+1
	jsr select_bank
	lda #LOW(game_names)
	clc
	adc <TEXT_DRAW_GAME
	sta <TMP
	lda #HIGH(game_names)
	adc #0
	sta <TMP+1
	lda <TMP
	clc 
	adc <TEXT_DRAW_GAME
	sta <TMP
	lda <TMP+1
	adc #0
	sta <TMP+1	
	ldy #0
	lda [TMP], y
	sta COPY_SOURCE_ADDR
	iny
	lda [TMP], y
	sta COPY_SOURCE_ADDR+1
	; spaces on the left
	ldx #GAME_NAMES_OFFSET+1
.print_blank:
	lda #$00
	sta $2007
	dex
	bne .print_blank	
	; text
	ldx #GAME_NAMES_OFFSET+1
	ldy #0
	lda #1
.next_char:
	cmp #0 ; after zero we are not printing letters but printing spaces
	beq .end_of_line
	lda [COPY_SOURCE_ADDR], y
.end_of_line:
	sta $2007	
	iny
	inx
	cpx #CHARS_PER_LINE
	bne .next_char
.end:
	; but if it upper part, we need to erase two lines
	lda <TEXT_DRAW_ROW
	cmp #4
	bcs .end_really
	ldy #CHARS_PER_LINE
	lda #0
.clear_2nd_line:
	sta $2007
	dey
	bne .clear_2nd_line
.end_really:
	; attributes for text
	lda <TEXT_DRAW_ROW
	cmp #LINES_PER_SCREEN
	bcc .print_palette_addr_first_screen
	; second nametable
	lda #$2F
	sta $2006
	lda <TEXT_DRAW_ROW
	sec
	sbc #LINES_PER_SCREEN
	jmp .print_palette_addr_select_end
	; first nametable
.print_palette_addr_first_screen:
	lda #$23
	sta $2006
	lda <TEXT_DRAW_ROW
.print_palette_addr_select_end:
	; one byte for 4 rows
	lsr A
	lsr A
	asl A
	asl A
	asl A
	clc
	adc #$C0
	sta $2006
	lda #$FF
	sta $2007
	sta $2007
	sta $2007
	sta $2007
	sta $2007
	sta $2007
	sta $2007
	sta $2007	
.end_really_really:
	pla
	tax
	pla
	tay
	pla
	rts

move_cursors:
	; fine cursor scrolling
	lda <SPRITE_1_X_TARGET
	cmp SPRITE_1_X
	beq .sprite_1x_target_end
	bcs .sprite_1x_target_plus
	lda SPRITE_1_X
	sec
	sbc #4
	sta SPRITE_1_X
	jmp .sprite_1x_target_end
.sprite_1x_target_plus:
	lda SPRITE_1_X
	clc
	adc #4
	sta SPRITE_1_X
.sprite_1x_target_end:
	lda <SPRITE_1_Y_TARGET
	cmp SPRITE_1_Y
	beq .sprite_1y_target_end
	bcs .sprite_1y_target_plus
	lda SPRITE_1_Y
	sec
	sbc #4
	sta SPRITE_0_Y
	sta SPRITE_1_Y
	jmp .sprite_1y_target_end
.sprite_1y_target_plus:
	lda SPRITE_1_Y
	clc
	adc #4
	sta SPRITE_0_Y
	sta SPRITE_1_Y
.sprite_1y_target_end:
	rts

	; fine scrolling to target line
move_scrolling:
	; determining scroll speed
	lda <SCROLL_LINES
	clc
	adc #2
	sta <TMP
	lda <SCROLL_LINES+1
	adc #0
	sta <TMP+1
	lda <SCROLL_LINES_TARGET
	sec
	sbc <TMP
	lda <SCROLL_LINES_TARGET+1
	sbc <TMP+1
	bcs .move_scrolling_fast_down
	lda <SCROLL_LINES_TARGET
	clc
	adc #2
	sta <TMP
	lda <SCROLL_LINES_TARGET+1
	adc #0
	sta <TMP+1
	lda <SCROLL_LINES
	sec
	sbc <TMP
	lda <SCROLL_LINES+1
	sbc <TMP+1
	bcs .move_scrolling_fast_up	
	; slow scrolling
	jsr .move_scrolling_real
	rts	
	; fast scrolling
.move_scrolling_fast:
	jsr .move_scrolling_real
	jsr .move_scrolling_real
	jsr .move_scrolling_real
	jsr .move_scrolling_real
	rts
.move_scrolling_fast_up:
	jmp scroll_line_up
.move_scrolling_fast_down:
	jmp scroll_line_down
.move_scrolling_real:
	; do we need to move screen?
	ldx <SCROLL_LINES_TARGET
	cpx <SCROLL_LINES
	bne .need
	ldx <SCROLL_LINES_TARGET+1
	cpx <SCROLL_LINES+1
	bne .need
	ldx <SCROLL_FINE
	bne .need
	rts	
	; we need it
.need:
	lda <SCROLL_LINES
	sec
	sbc <SCROLL_LINES_TARGET
	lda <SCROLL_LINES+1
	sbc <SCROLL_LINES_TARGET+1
	bcc .target_plus
	; scrolling up
	lda <SCROLL_FINE
	sec
	sbc #4
	bmi .target_minus
	sta <SCROLL_FINE
	rts	
.target_minus:
	and #$0f
	sta <SCROLL_FINE
	jsr scroll_line_up
	rts		
.target_plus:
	; scrolling down
	lda <SCROLL_FINE
	; adding fine value
	clc
	adc #4
	sta <SCROLL_FINE
	cmp #16
	bne .end
	; down if fine >=16
	lda #0
	sta <SCROLL_FINE	
	jsr scroll_line_down
.end:
	rts

set_cursor_targets:
	; set scroll targets first
.not_ok1:
	lda <SCROLL_LINES_TARGET
	clc
	adc #10
	sta <TMP
	lda <SCROLL_LINES_TARGET+1
	adc #0
	sta <TMP+1
	lda <TMP
	sec
	sbc <SELECTED_GAME
	lda <TMP+1
	sbc <SELECTED_GAME+1
	bcs .ok1 ; надо скроллить вниз
	lda <SCROLL_LINES_TARGET
	clc
	adc #1
	sta <SCROLL_LINES_TARGET
	lda <SCROLL_LINES_TARGET+1
	adc #0
	sta <SCROLL_LINES_TARGET+1
	jmp .not_ok1	
.ok1:
.not_ok2:
	lda <SELECTED_GAME
	sec
	sbc <SCROLL_LINES_TARGET
	lda <SELECTED_GAME+1
	sbc <SCROLL_LINES_TARGET+1
	bcs .ok2 ; надо скроллить вверх
	lda <SCROLL_LINES_TARGET
	sec
	sbc #1
	sta <SCROLL_LINES_TARGET
	lda <SCROLL_LINES_TARGET+1
	sbc #0
	sta <SCROLL_LINES_TARGET+1
	jmp .not_ok2
.ok2:
	; set cursor targets depending on selected game number
	; left cursor, X
	ldx <SELECTED_GAME
	lda #GAME_NAMES_OFFSET*8
	sta <SPRITE_0_X_TARGET	
	; right cursor, X
	lda <SELECTED_GAME+1
	jsr select_bank
	ldx <SELECTED_GAME
	ldy loader_data_cursor_pos, x
	dey
	tya
	clc
	adc #GAME_NAMES_OFFSET+2
	asl A
	asl A
	asl A
	sta <SPRITE_1_X_TARGET	
	; Y coordinate it the same for both
	lda <SELECTED_GAME	
	; when there are not so many games
	clc
	adc games_offset	
	sec 
	sbc <SCROLL_LINES_TARGET
	clc
	adc #2
	asl A
	asl A
	asl A
	asl A
	sec
	sbc #1
	.if ENABLE_TOP_OFFSET!=0 ; for large images on top
	clc
	adc #8
	.endif
	sta <SPRITE_0_Y_TARGET
	sta <SPRITE_1_Y_TARGET
	rts
	
stars:
	lda <STAR_SPAWN_TIMER
	cmp #$E0 ; stars count
	beq .spawn_end	
	inc <STAR_SPAWN_TIMER
	lda <STAR_SPAWN_TIMER
	and #$0F
	cmp #0
	bne .spawn_end
	lda <STAR_SPAWN_TIMER
	lsr A
	lsr A
	tay
	lda <STAR_SPAWN_TIMER
	lda #$FC ; Y, below screen
	sta SPRITES+4, y
	iny
	jsr random ; random tile
	and #$03
	clc
	adc #$71
	sta SPRITES+4, y
	iny
	jsr random  ; attributes, random palette
	and #%00000011 ; palette - tho lowest bits
	ora #%00100000 ; low priority bit
	sta SPRITES+4, y
	iny
	jsr random
	sta SPRITES+4, y
.spawn_end:
	ldy #8
.move_next:
	lda SPRITES, y
	cmp #$FF
	beq .move_end
	tya
	lsr A
	lsr A
	and #$07
	cmp #$00
	beq .move_fast
	cmp #$01
	beq .move_fast
	cmp #$02
	beq .move_fast
	cmp #$03
	beq .move_medium
	cmp #$04
	beq .move_medium
	cmp #$05
	beq .move_slow
	cmp #$06
	beq .move_slow	
.move_very_slow: ; default
	lda SPRITES, y
	sbc #1
	jmp .moved
.move_slow:
	lda SPRITES, y
	sec
	sbc #2
	jmp .moved
.move_medium:
	lda SPRITES, y
	sec
	sbc #3
	jmp .moved
.move_fast:
	lda SPRITES, y
	sec
	sbc #4
.moved:
	sta SPRITES, y
	cmp #$0A
	bcs .move_next1
	lda #$FC
	sta SPRITES, y ; reset Y
	jsr random  ; random tile
	and #$03
	clc
	adc #$71
	sta SPRITES+1, y
	jsr random ; random X
	sta SPRITES+3, y
	jsr random ; random attributes
	and #%00000011 ; palette - lowest tho bits
	ora #%00100000 ; priority bit
	sta SPRITES+2, y
.move_next1:
	iny
	iny
	iny
	iny
	bne .move_next	
.move_end:
	rts

saving_warning_show:
	lda #%00000000 ; disable PPU
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
	cmp #0 ; stop at zero
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
	rts

saving_warning_hide:
	lda #%00000000 ; disable PPU
	sta $2000
	sta $2001
	jsr waitblank_simple
	jsr clear_screen
	rts
