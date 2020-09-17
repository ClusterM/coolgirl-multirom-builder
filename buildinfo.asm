  ; show build info
show_build_info:
  bit $2002
  lda #$21
  sta $2006
  lda #$64
  sta $2006
  ; имя файла
  ldy #0
build_info0_print_next_char:
  lda build_info0, y
  sta $2007
  iny
  cmp #0 ; после завершающего нуля перестаём читать символы
  bne build_info0_print_next_char  

  lda #$21
  sta $2006
  lda #$A4
  sta $2006
  ; дата сборки
  ldy #0
build_info2_print_next_char:
  lda build_info2, y
  sta $2007
  iny
  cmp #0 ; после завершающего нуля перестаём читать символы
  bne build_info2_print_next_char    
  
  lda #$21
  sta $2006
  lda #$E4
  sta $2006
  ; время сборки
  ldy #0
build_info3_print_next_char:
  lda build_info3, y
  sta $2007
  iny
  cmp #0 ; после завершающего нуля перестаём читать символы
  bne build_info3_print_next_char    

  lda #$22
  sta $2006
  lda #$24
  sta $2006
  ; версия консоли
  ldy #0
console_type_print_next_char:
  lda console_type_text, y
  sta $2007
  iny
  cmp #0 ; после завершающего нуля перестаём читать символы
  bne console_type_print_next_char    
  
  lda CONSOLE_TYPE
  and #$08
  beq console_type_no_NEW
  ldy #0
console_type_print_NEW:
  lda console_type_NEW, y
  sta $2007
  iny
  cmp #0 ; после завершающего нуля перестаём читать символы
  bne console_type_print_NEW
console_type_no_NEW:
  lda CONSOLE_TYPE
  and #$01
  beq console_type_no_NTSC
  ldy #0
console_type_print_NTSC:
  lda console_type_NTSC, y
  sta $2007
  iny
  cmp #0 ; после завершающего нуля перестаём читать символы
  bne console_type_print_NTSC  
console_type_no_NTSC:
  lda CONSOLE_TYPE
  and #$02
  beq console_type_no_PAL
  ldy #0
console_type_print_PAL:
  lda console_type_PAL, y
  sta $2007
  iny
  cmp #0 ; после завершающего нуля перестаём читать символы
  bne console_type_print_PAL
console_type_no_PAL:
  lda CONSOLE_TYPE
  and #$04
  beq console_type_no_DENDY
  ldy #0
console_type_print_DENDY:
  lda console_type_DENDY, y
  sta $2007
  iny
  cmp #0 ; после завершающего нуля перестаём читать символы
  bne console_type_print_DENDY
console_type_no_DENDY:

  lda #$23
  sta $2006
  lda #$00
  sta $2006
  jsr draw_footer1
  jsr draw_footer2
  jsr load_text_palette

  lda #$FF
  sta SPRITE_0_Y_TARGET
  sta SPRITE_1_Y_TARGET
  sta SPRITE_0_Y
  sta SPRITE_1_Y
  jsr sprite_dma_copy

  lda #0
  sta SCROLL_LINES_TARGET
  sta SCROLL_LINES_TARGET+1
  sta SCROLL_LINES
  sta SCROLL_LINES+1
  sta SELECTED_GAME
  sta SELECTED_GAME+1  
  sta SCROLL_LINES_MODULO

show_build_info_infin:
  jsr waitblank
  lda #%00011110
  sta $2001
  jmp show_build_info_infin
