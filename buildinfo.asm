  ; build info
show_build_info:
  bit $2002
  lda #$21
  sta $2006
  lda #$44
  sta $2006
  ; filename
  lda #LOW(string_file)
  sta <COPY_SOURCE_ADDR
  lda #HIGH(string_file)
  sta <COPY_SOURCE_ADDR+1
  jsr print_text

  lda #$21
  sta $2006
  lda #$84
  sta $2006
  ; build date
  lda #LOW(string_build_date)
  sta <COPY_SOURCE_ADDR
  lda #HIGH(string_build_date)
  sta <COPY_SOURCE_ADDR+1
  jsr print_text
  
  lda #$21
  sta $2006
  lda #$C4
  sta $2006
  ; build time
  lda #LOW(string_build_time)
  sta <COPY_SOURCE_ADDR
  lda #HIGH(string_build_time)
  sta <COPY_SOURCE_ADDR+1
  jsr print_text

  lda #$22
  sta $2006
  lda #$04
  sta $2006
  ; console region/type
  lda #LOW(string_console_type)
  sta <COPY_SOURCE_ADDR
  lda #HIGH(string_console_type)
  sta <COPY_SOURCE_ADDR+1
  jsr print_text
  
  lda <CONSOLE_TYPE
  and #$08
  beq .console_type_no_NEW
  lda #LOW(string_new)
  sta <COPY_SOURCE_ADDR
  lda #HIGH(string_new)
  sta <COPY_SOURCE_ADDR+1
  jsr print_text
.console_type_no_NEW:
  lda <CONSOLE_TYPE
  and #$01
  beq .console_type_no_NTSC
  lda #LOW(string_ntsc)
  sta <COPY_SOURCE_ADDR
  lda #HIGH(string_ntsc)
  sta <COPY_SOURCE_ADDR+1
  jsr print_text
.console_type_no_NTSC:
  lda <CONSOLE_TYPE
  and #$02
  beq .console_type_no_PAL
  lda #LOW(string_pal)
  sta <COPY_SOURCE_ADDR
  lda #HIGH(string_pal)
  sta <COPY_SOURCE_ADDR+1
  jsr print_text
.console_type_no_PAL:
  lda <CONSOLE_TYPE
  and #$04
  beq .console_type_no_DENDY
  lda #LOW(string_dendy)
  sta <COPY_SOURCE_ADDR
  lda #HIGH(string_dendy)
  sta <COPY_SOURCE_ADDR+1
  jsr print_text
.console_type_no_DENDY:

  ; flash memory type and size
print_flash_type:
  lda #$22
  sta $2006
  lda #$44
  sta $2006
  lda #LOW(string_flash)
  sta <COPY_SOURCE_ADDR
  lda #HIGH(string_flash)
  sta <COPY_SOURCE_ADDR+1
  jsr print_text

  ; is it writable?
  lda <FLASH_TYPE
  bne .writable
  lda #LOW(string_error)
  sta <COPY_SOURCE_ADDR
  lda #HIGH(string_error)
  sta <COPY_SOURCE_ADDR+1
  jsr print_text
  jmp .end

  ; yes, it's writable
.writable:
  ; how many memory?
  lda <FLASH_TYPE
  sec
  sbc #20
  asl A
  tay
  lda flash_sizes, y
  sta <COPY_SOURCE_ADDR
  lda flash_sizes+1, y
  sta <COPY_SOURCE_ADDR+1
  jsr print_text

.end:
  lda #$23
  sta $2006
  lda #$00
  sta $2006
  jsr draw_footer1
  jsr draw_footer2
  jsr load_text_palette

  lda #$FF
  sta <SPRITE_1_Y_TARGET
  sta <SPRITE_1_Y_TARGET
  sta SPRITE_0_Y
  sta SPRITE_1_Y
  jsr sprite_dma_copy

  lda #0
  sta <SCROLL_LINES_TARGET
  sta <SCROLL_LINES_TARGET+1
  sta <SCROLL_LINES
  sta <SCROLL_LINES+1
  sta <SELECTED_GAME
  sta <SELECTED_GAME+1  
  sta <SCROLL_LINES_MODULO

show_build_info_infin:
  jsr waitblank
  lda #%00011110
  sta $2001
  jmp show_build_info_infin
