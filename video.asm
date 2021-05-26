PALETTE_CACHE     .rs 32 ; temporary memory for palette, for dimming
  ; cursors target coordinates
SPRITE_0_X_TARGET .rs 1
SPRITE_1_X_TARGET .rs 1
SPRITE_Y_TARGET .rs 1
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
  ; for build info
CHR_RAM_SIZE .rs 1 ; CHR RAM size 8*2^xKB
LAST_ATTRIBUTE_ADDRESS .rs 1 ; to prevent duplicate writes
SCHEDULE_PRINT_FIRST .rs 1
SCHEDULE_PRINT_LAST .rs 1

  ; constants
CHARS_PER_LINE .equ 32
LINES_PER_SCREEN .equ 15

waitblank:
  pha
  tya
  pha
  txa
  pha
  bit PPUSTATUS ; reset vblank bit
.loop:
  lda PPUSTATUS ; load A with value at location PPUSTATUS
  bpl .loop ; if bit 7 is not set (not VBlank) keep checking

  ; updating sprites
  jsr sprite_dma_copy
  lda SCHEDULE_PRINT_FIRST
  beq .first_not_scheduled
  jsr print_first_name
  lda #0
  sta SCHEDULE_PRINT_FIRST
.first_not_scheduled:
  lda SCHEDULE_PRINT_LAST
  beq .last_not_scheduled
  jsr print_last_name
  lda #0
  sta SCHEDULE_PRINT_LAST
.last_not_scheduled:
  jsr scroll_fix
  ; scrolling
  jsr move_scrolling
  ; moving cursors
  jsr move_cursors
  ; reading controller
  jsr read_controller
  ; stars on the background
  .if ENABLE_STARS!=0
  jsr stars
  .endif

  pla
  tax
  pla
  tay
  pla
  rts

waitblank_simple:
  pha
  bit PPUSTATUS
.loop:
  lda PPUSTATUS  ; load A with value at location PPUSTATUS
  bpl .loop  ; if bit 7 is not set (not VBlank) keep checking
  pla
  rts

waitblank_x:
  ; for for v-blank X times
  cpy #0
  bne .loop
  rts
.loop:
  jsr waitblank
  dex
  bne .loop
  rts

scroll_fix:
  pha
  tya
  pha
  ; scrolling reset
  bit PPUSTATUS
  ; X coordinate always 0
  lda #0
  sta PPUSCROLL
  lda <SCROLL_LINES_MODULO
  cmp #LINES_PER_SCREEN
  bcc .first_screen
  sec
  sbc #LINES_PER_SCREEN ; substracting otherwise
  ldy #%00001010 ; second nametable
  jmp .really
.first_screen:
  ldy #%00001000 ; first nametable
.really:
  sty PPUCTRL ; set base nametable
  ; calculating Y coordinate
  asl A
  asl A
  asl A
  asl A
  clc
  adc <SCROLL_FINE
  sta PPUSCROLL
  pla
  tay
  pla
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
  cmp #LINES_PER_SCREEN * 2
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
  cmp #LINES_PER_SCREEN * 2
  bcc .modulo_ok2
  lda #0
  sta <LAST_LINE_MODULO
.modulo_ok2:
  ;jsr print_last_name
  inc SCHEDULE_PRINT_LAST
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
  bpl .modulo_ok
  lda #LINES_PER_SCREEN * 2 - 1
  sta <SCROLL_LINES_MODULO
.modulo_ok:
  lda <LAST_LINE_GAME
  sec
  sbc #1
  sta <LAST_LINE_GAME
  lda <LAST_LINE_GAME+1
  sbc #0
  sta <LAST_LINE_GAME+1
  dec <LAST_LINE_MODULO
  bpl .modulo_ok2
  lda #LINES_PER_SCREEN * 2 - 1
  sta <LAST_LINE_MODULO
.modulo_ok2:
  ;jsr print_first_name
  inc SCHEDULE_PRINT_FIRST
  rts

screen_wrap_down:
  jsr wait_scroll_done
  lda #0
  sta <SELECTED_GAME
  sta <SELECTED_GAME+1
  lda #0
  sta <SCROLL_LINES_TARGET
  sta <SCROLL_LINES_TARGET+1
  sta <SCROLL_LINES
  sta <SCROLL_LINES+1
  sta <LAST_LINE_GAME
  sta <LAST_LINE_GAME+1
  jsr set_cursor_targets
  ldx #LINES_PER_SCREEN
.lines_loop
  ; draw lines and update screen
  jsr waitblank_simple
  jsr print_last_name
  jsr sprite_dma_copy
  jsr scroll_fix
  jsr move_cursors
  .if ENABLE_STARS!=0
  jsr stars
  .endif
  cpx #0
  beq .end
  ; increment everything
  inc <SCROLL_LINES_MODULO
  lda <SCROLL_LINES_MODULO
  cmp #LINES_PER_SCREEN * 2
  bcc .modulo_ok
  lda #0
  sta <SCROLL_LINES_MODULO
.modulo_ok:
  inc <LAST_LINE_MODULO
  lda <LAST_LINE_MODULO
  cmp #LINES_PER_SCREEN * 2
  bcc .modulo_ok2
  lda #0
  sta <LAST_LINE_MODULO
.modulo_ok2:
  inc <LAST_LINE_GAME
  ; next line?
  dex
  jmp .lines_loop
.end:
  rts

screen_wrap_up:
  jsr wait_scroll_done
  ; some weird math
  lda #(GAMES_COUNT - 1) & $FF
  sta <SELECTED_GAME
  lda #((GAMES_COUNT - 1) >> 8) & $FF
  sta <SELECTED_GAME+1
  lda #(GAMES_COUNT + 4) & $FF
  sta <SCROLL_LINES
  lda #((GAMES_COUNT + 4) >> 8) & $FF
  sta <SCROLL_LINES+1
  lda #(GAMES_COUNT - 11) & $FF
  sta <SCROLL_LINES_TARGET
  lda #((GAMES_COUNT - 11) >> 8) & $FF
  sta <SCROLL_LINES_TARGET+1
  lda #(GAMES_COUNT + 4) & $FF
  sta <LAST_LINE_GAME
  lda #((GAMES_COUNT + 4) >> 8) & $FF
  sta <LAST_LINE_GAME+1
  jsr set_cursor_targets
  ldx #LINES_PER_SCREEN
.lines_loop:
  ; decrement everything
  dec <SCROLL_LINES_MODULO
  bpl .modulo_ok
  lda #LINES_PER_SCREEN * 2 - 1
  sta <SCROLL_LINES_MODULO
.modulo_ok:
  dec <LAST_LINE_MODULO
  bpl .modulo_ok2
  lda #LINES_PER_SCREEN * 2 - 1
  sta <LAST_LINE_MODULO
.modulo_ok2:
  lda <SCROLL_LINES
  sec
  sbc #1
  sta <SCROLL_LINES
  lda <SCROLL_LINES+1
  sbc #0
  sta <SCROLL_LINES+1
  ; draw lines and update screen
  jsr waitblank_simple
  jsr print_first_name
  jsr sprite_dma_copy
  jsr scroll_fix
  jsr move_cursors
  .if ENABLE_STARS!=0
  jsr stars
  .endif
  ; next line?
  dex
  bne .lines_loop
  rts

load_base_chr:
  ; loading CHR
  lda #BANK(chr_data) / 2 ; bank with CHR
  jsr select_prg_bank
  lda #LOW(chr_data)
  sta COPY_SOURCE_ADDR
  lda #HIGH(chr_data)
  sta COPY_SOURCE_ADDR+1
  jsr load_chr
  rts

preload_base_palette:
  ; loading palette into palette cache
  lda #BANK(tilepal) / 2 ; bank with palette
  jsr select_prg_bank
  ldx #$00
.loop:
  lda tilepal, x
  sta PALETTE_CACHE, x
  inx
  cpx #32
  bne .loop
  rts

  ; loading palette from cache to PPU
load_palette:
  jsr waitblank_simple
  lda #LOW(PALETTE_CACHE)
  sta <COPY_SOURCE_ADDR
  lda #HIGH(PALETTE_CACHE)
  sta <COPY_SOURCE_ADDR+1
  lda #$3F
  sta $2006
  lda #$00
  sta $2006
  ldy #$00
  ldx #32
.loop:
  lda [COPY_SOURCE_ADDR], y
  sta $2007
  iny
  dex
  bne .loop
  jsr scroll_fix
  rts

;load_base_pal:
  ; loading palette into $3F00 of PPU
  ;lda #$3F
  ;sta PPUADDR
  ;lda #$00
  ;sta PPUADDR
  ;ldx #$00
;.loop:
  ;lda tilepal, x
  ;sta PPUDATA
  ;inx
  ;cpx #32
  ;bne .loop
  ;rts

dim:
  ; dimming preloaded palette
  ldx #0
.loop:
  lda PALETTE_CACHE, x
  sec
  sbc #$10
  bpl .not_minus
  lda #$1D  
.not_minus:
  cmp #$0D
  bne .not_very_black
  lda #$1D
.not_very_black:
  sta PALETTE_CACHE, x
  inx
  cpx #32
  bne .loop
  rts

  ; dimming base palette in
dim_base_palette_in:
  ;lda BUTTONS
  ;bne .done ; skip if any button pressed
  .if ENABLE_DIM_IN!=0
  jsr preload_base_palette
  jsr dim
  jsr dim
  jsr dim
  jsr load_palette
  ldx #DIM_IN_DELAY
  jsr waitblank_x
  ;lda BUTTONS
  ;bne .done ; skip if any button pressed
  jsr preload_base_palette
  jsr dim
  jsr dim
  jsr load_palette
  ldx #DIM_IN_DELAY
  jsr waitblank_x
  ;lda BUTTONS
  ;bne .done ; skip if any button pressed
  jsr preload_base_palette
  jsr dim
  jsr load_palette
  ldx #DIM_IN_DELAY
  jsr waitblank_x
  .endif
.done:
  jsr preload_base_palette
  jsr load_palette
  jsr waitblank
  rts

  ; dimming base palette out in
dim_base_palette_out:
  .if ENABLE_DIM_OUT!=0
  jsr preload_base_palette
  jsr dim
  jsr load_palette
  ldx #DIM_OUT_DELAY
  jsr waitblank_x
  jsr preload_base_palette
  jsr dim
  jsr dim
  jsr load_palette
  ldx #DIM_OUT_DELAY
  jsr waitblank_x
  jsr preload_base_palette
  jsr dim
  jsr dim
  jsr dim
  jsr load_palette
  ldx #DIM_OUT_DELAY
  jsr waitblank_x
  .endif
  jsr load_black
  jsr waitblank
  rts

  ; loading empty black palette into $3F00 of PPU
load_black:
  ; waiting for vblank
  ; need even if rendering is disabled
  ; to prevent lines on black screen
  jsr waitblank_simple
  lda #$3F
  sta PPUADDR
  lda #$00
  sta PPUADDR
  ldx #$00
  lda #$3F ; color
.loop:
  sta PPUDATA
  inx
  cpx #32
  bne .loop
  rts

  ; nametable cleanup
clear_screen:
  lda #$20
  sta PPUADDR
  lda #$00
  sta PPUADDR
  lda #$00
  ldx #0
  ldy #$10
.loop:
  sta PPUDATA
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
  pha
  lda #0
  sta OAMADDR
  lda #HIGH(SPRITES)
  sta OAMDMA
  pla
  rts

  ; loading header (image on the top), first part
draw_header1:
  lda #BANK(nametable_header) / 2 ; bank with header
  jsr select_prg_bank
  ldx #0
  ldy #$40
.loop:
  lda nametable_header, x
  sta PPUDATA
  inx
  dey
  bne .loop
  rts

  ; loading header (image on the top), second part
draw_header2:
  lda #BANK(nametable_header) / 2 ; bank with header
  jsr select_prg_bank
  ldx #$40
  ldy #$40
.loop:
  lda nametable_header, x
  sta PPUDATA
  inx
  dey
  bne .loop
  rts

  ; loading footer (image on the bottom), first part
draw_footer1:
  lda #$06
  jsr select_prg_bank
  ldx #0
  ldy #$40
.loop:
  lda nametable_footer, x
  sta PPUDATA
  inx
  dey
  bne .loop
  rts

  ; loading footer (image on the bottom), second part
draw_footer2:
  lda #$06
  jsr select_prg_bank
  ldx #$40
  ldy #$40
.loop:
  lda nametable_footer, x
  sta PPUDATA
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
  ; when there are not so many games we need offset
  .if GAMES_COUNT <= 10
  lda TEXT_DRAW_GAME
  cmp #2
  bcc .padding_end
  cmp #(GAMES_COUNT + 2)
  beq .padding_footer_1
  cmp #(GAMES_COUNT + 3)
  beq .padding_footer_2
  lda <TEXT_DRAW_ROW
  clc
  adc #(6 - GAMES_COUNT / 2 - GAMES_COUNT % 2)
  sta <TEXT_DRAW_ROW
  jmp .padding_end
.padding_footer_1:
  lda #13
  sta <TEXT_DRAW_ROW
  jmp .padding_end
.padding_footer_2:
  lda #14
  sta <TEXT_DRAW_ROW
.padding_end:
  .endif
  lda <TEXT_DRAW_ROW
  ; detecting target nametable and PPU address
  cmp #LINES_PER_SCREEN
  bcc .first_screen
  ; second
  sec
  sbc #LINES_PER_SCREEN
  lsr A
  lsr A
  clc
  adc #$2C
  bit PPUSTATUS
  sta PPUADDR
  lda <TEXT_DRAW_ROW
  sec
  sbc #LINES_PER_SCREEN
  asl A
  asl A
  asl A
  asl A
  asl A
  asl A
  sta PPUADDR
  jmp .print_start
  ; first
.first_screen:
  lsr A
  lsr A
  clc
  adc #$20
  bit PPUSTATUS
  sta PPUADDR
  lda <TEXT_DRAW_ROW
  asl A
  asl A
  asl A
  asl A
  asl A
  asl A
  sta PPUADDR
.print_start:
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
  jsr set_line_attributes
  jmp .end
.header2:
  jsr draw_header2
  jsr set_line_attributes
  jmp .end
.not_header:
  ; we need to substract 2 from game number
  lda <TEXT_DRAW_GAME
  sec
  sbc #2
  ; store game number to TMP and TMP+1
  sta <TMP
  lda <TEXT_DRAW_GAME+1
  sbc #0
  sta <TMP+1
  ; is it footer?
  lda <TMP+1
  cmp #(GAMES_COUNT >> 8) & $FF
  bne .not_footer_1
  lda <TMP
  cmp #GAMES_COUNT & $FF
  bne .not_footer_1
  jsr draw_footer1
  jsr set_line_attributes
  jmp .end
.not_footer_1:
  lda <TMP+1
  cmp #((GAMES_COUNT + 1) >> 8) & $FF
  bne .not_footer_2
  lda <TMP
  cmp #(GAMES_COUNT + 1) & $FF
  bne .not_footer_2
  jsr draw_footer2
  jsr set_line_attributes
  jmp .end
.not_footer_2:
  lda <TMP
  sec
  sbc #GAMES_COUNT & $FF
  lda <TMP+1
  sbc #(GAMES_COUNT >> 8) & $FF
  bcs .end
  lda <TMP+1
  jsr select_prg_bank
  lda #LOW(game_names)
  clc
  adc <TMP
  sta <COPY_SOURCE_ADDR
  lda #HIGH(game_names)
  adc #0
  sta <COPY_SOURCE_ADDR+1
  ; x2 (because address two bytes length)
  lda <COPY_SOURCE_ADDR
  clc
  adc <TMP
  sta <COPY_SOURCE_ADDR
  lda <COPY_SOURCE_ADDR+1
  adc #0
  sta <COPY_SOURCE_ADDR+1
  ldy #0
  lda [COPY_SOURCE_ADDR], y
  sta <TMP
  iny
  lda [COPY_SOURCE_ADDR], y
  sta <COPY_SOURCE_ADDR+1
  lda <TMP
  sta <COPY_SOURCE_ADDR
  ; spaces on the left
  ldx #GAME_NAMES_OFFSET+1
.print_blank:
  lda #$00
  sta PPUDATA
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
  sta PPUDATA
  iny
  inx
  cpx #CHARS_PER_LINE
  bne .next_char
.print_name_end:
  ; clear second line
  ldy #CHARS_PER_LINE
  lda #0
.clear_2nd_line_loop:
  sta PPUDATA
  dey
  bne .clear_2nd_line_loop
  jsr set_line_attributes
.end:
  pla
  tax
  pla
  tay
  pla
  rts

set_line_attributes:
  ; maybe this line already drawned?
  lda <TEXT_DRAW_ROW
  and #%11111110
  cmp <LAST_ATTRIBUTE_ADDRESS
  bne .set_attribute_address
  rts
.set_attribute_address:
  ; calculating attributes address
  lda <TEXT_DRAW_ROW
  cmp #LINES_PER_SCREEN
  bcc .first_screen
  ; second nametable
  lda #1
  sta TMP ; remember nametable #
  lda #$2F
  sta PPUADDR
  lda <TEXT_DRAW_ROW
  sec
  sbc #LINES_PER_SCREEN
  jmp .nametable_detect_end
  ; first nametable
.first_screen:
  lda #0
  sta TMP ; remember nametable #
  lda #$23
  sta PPUADDR
  lda <TEXT_DRAW_ROW
.nametable_detect_end:
  ; one byte for 4 rows
  and #%11111110
  asl A
  asl A
  clc
  adc #$C0
  sta <LAST_ATTRIBUTE_ADDRESS
  sta PPUADDR
  ; now writing attributes, need to calculate them too
  ldx #8
  ldy #0
  lda <TEXT_DRAW_GAME+1
  cmp #((GAMES_COUNT + 3) >> 8) & $FF
  bne .not_footer
  lda <TEXT_DRAW_GAME
  cmp #(GAMES_COUNT + 3) & $FF
  beq .footer
  jmp .maybe_header_or_game_0
.not_footer:
  cmp #0
  bne .only_text_attributes
.maybe_header_or_game_0:
  lda <TEXT_DRAW_GAME+1
  bne .only_text_attributes
  lda <TEXT_DRAW_GAME
  cmp #0
  beq .header_0
  cmp #1
  beq .header_1
  cmp #2
  beq .game_0
  jmp .only_text_attributes
.header_0
  lda TEXT_DRAW_ROW
  eor TMP
  lsr A
  bcc .only_header_attributes
  lda #BANK(header_attribute_table) / 2 ; bank with header attribute table
  jsr select_prg_bank
.header_0_loop:
  lda header_attribute_table, y
  asl A
  asl A
  asl A
  asl A
  ora #$0F
  sta PPUDATA
  iny
  dex
  bne .header_0_loop
  rts
.header_1
  lda TEXT_DRAW_ROW
  eor TMP
  lsr A
  bcs .only_header_attributes
  lda #BANK(header_attribute_table) / 2
  jsr select_prg_bank
.header_1_loop:
  lda header_attribute_table, y
  lsr A
  lsr A
  lsr A
  lsr A
  ora #$F0
  sta PPUDATA
  iny
  dex
  bne .header_1_loop
  rts
.game_0:
  lda TEXT_DRAW_ROW
  eor TMP
  lsr A
  bcc .only_text_attributes
  jmp .header_1_loop
.footer:
  lda TEXT_DRAW_ROW
  eor TMP
  lsr A
  bcs .only_text_attributes
  jmp .header_0_loop
.only_header_attributes:
  lda header_attribute_table, y
  sta PPUDATA
  iny
  dex
  bne .only_header_attributes
  rts
.only_text_attributes:
  lda #$FF
.only_text_attributes_loop:
  sta PPUDATA
  dex
  bne .only_text_attributes_loop
  rts

move_cursors:
  pha
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
  lda <SPRITE_Y_TARGET
  cmp SPRITE_0_Y
  beq .sprite_0y_target_end
  bcs .sprite_0y_target_plus
  lda SPRITE_0_Y
  sec
  sbc #4
  sta SPRITE_0_Y
  .if ENABLE_RIGHT_CURSOR!=0
  sta SPRITE_1_Y
  .endif
  jmp .sprite_0y_target_end
.sprite_0y_target_plus:
  lda SPRITE_0_Y
  clc
  adc #4
  sta SPRITE_0_Y
  .if ENABLE_RIGHT_CURSOR!=0
  sta SPRITE_1_Y
  .endif
.sprite_0y_target_end:
  pla
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

set_scroll_targets:
  ; set scroll targets first
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
  bcs .not_down
  lda <SCROLL_LINES_TARGET
  clc
  adc #1
  sta <SCROLL_LINES_TARGET
  lda <SCROLL_LINES_TARGET+1
  adc #0
  sta <SCROLL_LINES_TARGET+1
  jmp set_scroll_targets
.not_down:
  lda <SELECTED_GAME
  sec
  sbc <SCROLL_LINES_TARGET
  lda <SELECTED_GAME+1
  sbc <SCROLL_LINES_TARGET+1
  bcs set_cursor_targets
  lda <SCROLL_LINES_TARGET
  sec
  sbc #1
  sta <SCROLL_LINES_TARGET
  lda <SCROLL_LINES_TARGET+1
  sbc #0
  sta <SCROLL_LINES_TARGET+1
  jmp .not_down

set_cursor_targets:
  ; set cursor targets depending on selected game number
  ; left cursor, X
  ldx <SELECTED_GAME
  lda #GAME_NAMES_OFFSET * 8
  sta <SPRITE_0_X_TARGET
  ; right cursor, X
  lda <SELECTED_GAME + 1
  jsr select_prg_bank
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
  .if GAMES_COUNT <= 10
  clc
  adc #(6 - GAMES_COUNT / 2 - GAMES_COUNT % 2)
  .endif
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
  sta <SPRITE_Y_TARGET
  rts

wait_scroll_done:
  ; just to make sure that screen drawing done
  jsr waitblank
  lda <SCROLL_LINES
  cmp <SCROLL_LINES_TARGET
  bne wait_scroll_done
  lda <SCROLL_LINES+1
  cmp <SCROLL_LINES_TARGET+1
  bne wait_scroll_done
  rts

  .if ENABLE_STARS!=0
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
  adc #1
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
  adc #1
  sta SPRITES+1, y
  jsr random ; random X
  sta SPRITES+3, y
  jsr random ; random attributes
  and #%00000011 ; palette - lowest two bits
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
  .endif

load_text_attributes:
  lda #$23
  sta PPUADDR
  lda #$C8
  sta PPUADDR
  lda #$FF
  ldy #$38
.loop:
  sta PPUDATA
  dey
  bne .loop
  rts

  ; print null-terminated string from [COPY_SOURCE_ADDR]
print_text:
  ldy #0
.loop:
  lda [COPY_SOURCE_ADDR], y
  sta PPUDATA
  iny
  cmp #0 ; stop at zero
  bne .loop
  rts

  ; show "saving... keep power on" message
saving_warning_show:
  ; disable PPU
  lda #%00000000
  sta PPUCTRL
  sta PPUMASK
  jsr waitblank_simple
  ; print text
  jsr clear_screen
  lda #$21
  sta PPUADDR
  lda #$C0
  sta PPUADDR
  lda #LOW(string_saving)
  sta COPY_SOURCE_ADDR
  lda #HIGH(string_saving)
  sta COPY_SOURCE_ADDR+1
  jsr print_text
  jsr load_text_attributes
  ; enable PPU
  lda #%00001000
  sta PPUCTRL
  lda #%00001010
  sta PPUMASK
  jsr dim_base_palette_in
  rts

  ; hide this message (clear screen)
saving_warning_hide:
  jsr dim_base_palette_out
  lda #%00000000 ; disable PPU
  sta PPUCTRL
  sta PPUMASK
  rts

detect_chr_ram_size:
  ; disable PPU
  lda #%00000000
  sta PPUCTRL
  sta PPUMASK
  jsr waitblank_simple
  jsr enable_chr_write
  lda #$00
  sta PPUADDR
  sta PPUADDR
  ; store $AA to zero bank
  sta <CHR_RAM_SIZE
  lda #$AA
  sta PPUDATA
  ; calculate bank number
.next_size:
  lda #1
  ldx CHR_RAM_SIZE
  ; shift 1 to the left CHR_RAM_SIZE times
.shift_loop:
  dex
  bmi .shift_done
  asl A
  beq .end ; overflow check
  jmp .shift_loop
.shift_done:
  ; select this bank
  jsr select_chr_bank
  ; store $AA
  ldx #$00
  stx PPUADDR
  stx PPUADDR
  lda #$AA
  sta PPUDATA
  ; check for $AA
  stx PPUADDR
  stx PPUADDR
  ldy PPUDATA ; dump read
  cmp PPUDATA
  bne .end ; check failed
  ; store $55
  stx PPUADDR
  stx PPUADDR
  lda #$55
  ; check for $55
  sta PPUDATA
  stx PPUADDR
  stx PPUADDR
  ldy PPUDATA ; dump read
  cmp PPUDATA
  bne .end ; check failed
  ; select zero bank
  lda #0
  jsr select_chr_bank
  ; check that $AA is not overwrited
  stx PPUADDR
  stx PPUADDR
  lda #$AA
  ldy PPUDATA ; dump read
  cmp PPUDATA
  bne .end ; check failed
  ; OK! Let's check next bank
  inc <CHR_RAM_SIZE
  jmp .next_size
.end:
  lda #0
  jsr select_chr_bank
  lda #0
  sta PPUADDR
  sta PPUADDR
  sta PPUDATA
  jsr disable_chr_write
  rts
