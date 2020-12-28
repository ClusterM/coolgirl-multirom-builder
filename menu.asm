; INES header stuff
  .inesprg 128 * 1024   ; 8 banks of PRG = 128kB
  .ineschr 0   ; no CHR, RAM only
  .inesmir 0   ; horizontal mirroring
  .inesmap 2   ; UxROM

  ; settings
ENABLE_STARS .equ 1
ENABLE_START_SCROLLING .equ 1
ENABLE_LAST_GAME_SAVING .equ 1
ENABLE_TOP_OFFSET .equ 0
ENABLE_RIGHT_CURSOR .equ 1
GAME_NAMES_OFFSET .equ 2
BUTTON_REPEAT_FRAMES .equ 30

  ; games settings
  .include "games.asm"

  ; sprites data
  .rsset $0400
SPRITES .rs 0
SPRITE_0_Y .rs 1
SPRITE_0_TILE .rs 1
SPRITE_0_ATTR .rs 1
SPRITE_0_X .rs 1
SPRITE_1_Y .rs 1
SPRITE_1_TILE .rs 1
SPRITE_1_ATTR .rs 1
SPRITE_1_X .rs 1

  ; non-volatile PRG-RAM
  .rsset $6000
SRAM_SIGNATURE .rs 8
SRAM_LAST_STARTED_GAME .rs 2
SRAM_LAST_STARTED_LINE .rs 2
SRAM_LAST_STARTED_SAVE .rs 1

  .rsset $0000
  ; zero page variables
  ; some common variables
COPY_SOURCE_ADDR .rs 2
COPY_DEST_ADDR .rs 2
TMP .rs 2
  ; selected game
SELECTED_GAME .rs 2

  ; переменные для отрисовки названий игр
LAST_STARTED_SAVE .rs 1 ; последнее использованное сохранение

  .bank 15   ; last bank
  .org $FFFA ; vectors
  .dw NMI    ; NMI vector
  .dw Start  ; reset vector
  .dw IRQ    ; interrupts

  .org $FFE0
unrom_bank_data:
  ; for compatibility with UNROM and UNROM's bus conflicts
  .db $00, $01, $02, $03, $04, $05, $06, $07, $08, $09, $0A, $0B, $0C, $0D, $0E, $0F

  .org $E000
Start:
  sei ; no interrupts

  ; reset stack
  ldx #$ff
  txs 
  
  lda #%00000000 ; PPU disabled
  sta $2000
  sta $2001
  
  jsr waitblank_simple

  ; clean memory
  lda #$00
  sta <COPY_SOURCE_ADDR
  sta <COPY_SOURCE_ADDR+1
  ldy #$02
  ldx #$08
.loop:
  sta [COPY_SOURCE_ADDR], y
  iny
  bne .loop
  inc COPY_SOURCE_ADDR+1
  dex
  bne .loop

  jsr clear_screen
  jsr load_black
  
  ; enable PPU to show black screen
  lda #%00001010
  sta $2001
  
  ; wait some time
  ldx #15
.start_wait:
  jsr waitblank_simple
  dex
  bne .start_wait
  
  lda #%00000000 ; disable PPU
  sta $2001
  jsr waitblank_simple

  ; loading loader and other RAM routines
  ldx #$00
.load_ram_routines:
  lda ram_routines+$C000, x
  sta ram_routines, x
  lda ram_routines+$C100, x
  sta ram_routines+$100, x
  lda ram_routines+$C200, x
  sta ram_routines+$200, x
  inx             
  bne .load_ram_routines
  
  ; init banks and other cart stuff
  jsr banking_init
  ; detect console type
  jsr console_detect
  ; load CHR data
  jsr load_base_chr
  ; palette
  jsr load_base_pal 
  ; clear all sprites data
  jsr clear_sprites
  ; load this empty sprites data
  jsr sprite_dma_copy 
  
  jsr read_controller ; read buttons
  jsr load_state ; loading saved cursor position and other data
  jsr save_all_saves ;  сохраняем предыдущую сейвку во флеш, если есть
  
  lda <SCROLL_LINES_TARGET
  sta <SCROLL_LINES
  sta <LAST_LINE_GAME
  sta <TMP
  lda <SCROLL_LINES_TARGET+1
  sta <SCROLL_LINES+1
  sta <LAST_LINE_GAME+1
  sta <TMP+1
  
  ; calculate modulo
.init_modulo:
  lda <TMP+1
  bne .do_init_modulo
  lda <TMP
  cmp #LINES_PER_SCREEN
  bcs .do_init_modulo  
  jmp .init_modulo_end
.do_init_modulo:
  lda <TMP
  sec
  sbc #LINES_PER_SCREEN
  sta <TMP
  lda <TMP+1
  sbc #0
  sta <TMP+1
  jmp .init_modulo  
.init_modulo_end:
  lda <TMP
  sta <SCROLL_LINES_MODULO
  sta <LAST_LINE_MODULO

  jsr set_cursor_targets

  ; sprites init
  ldx <SPRITE_0_X_TARGET
  stx SPRITE_0_X
  ldx <SPRITE_1_X_TARGET
  stx SPRITE_1_X
  ldx <SPRITE_0_Y_TARGET
  stx SPRITE_0_Y
  stx SPRITE_1_Y
  ldx #$00
  stx SPRITE_0_TILE
  .if ENABLE_RIGHT_CURSOR=0
  ldx #$FF ;hide right cursor
  .endif
  stx SPRITE_1_TILE
  ldx #%00000000
  stx SPRITE_0_ATTR
  ldx #%01000000
  stx SPRITE_1_ATTR
  
  ; init random number generator
  jsr random_init

  lda #%00000100
  cmp <BUTTONS
  bne .skip_build_info  
  ; build and hardware info
  jmp show_build_info
.skip_build_info:
  ldx games_count
  dex
  bne .not_single_game
  ldx games_count+1
  bne .not_single_game
  stx <SELECTED_GAME
  stx <SELECTED_GAME+1
  jmp start_game
.not_single_game:
  .if SECRETS>=1
  lda #%00010011
  cmp <BUTTONS
  bne .not_hidden_rom_1
  lda games_count
  sta <SELECTED_GAME
  lda games_count+1
  sta <SELECTED_GAME+1
  jmp start_game
.not_hidden_rom_1:
  .endif
  .if SECRETS>=2
  lda #%00100011
  cmp <BUTTONS
  bne .not_hidden_rom_2
  lda games_count
  clc
  adc #1
  sta <SELECTED_GAME
  lda games_count+1
  adc #0
  sta <SELECTED_GAME+1
  jmp start_game
.not_hidden_rom_2:
  .endif
  lda #%00000111
  cmp <BUTTONS
  bne .not_tests
  jmp do_tests
.not_tests:
  lda #%00001011
  cmp <BUTTONS
  bne .not_crc
  jsr crc_tests
.not_crc:

  ; printing game names
  ldx #15
  jsr print_last_name
.print_next_game_at_start:
  inc <LAST_LINE_GAME
  lda <LAST_LINE_GAME
  bne .last_line_ok
  inc <LAST_LINE_GAME+1
.last_line_ok:
  inc <LAST_LINE_MODULO
  lda <LAST_LINE_MODULO
  cmp #LINES_PER_SCREEN
  bne .modulo_ok
  lda #0
  sta <LAST_LINE_MODULO
.modulo_ok:
  jsr print_last_name
  dex
  bne .print_next_game_at_start
  
  jsr waitblank_simple
  lda #%00001010 ; second nametable
  sta $2000
  lda #%00001010 ; disabled sprites
  sta $2001
  
  ; start scrolling
  .if ENABLE_START_SCROLLING!=0
  lda <SELECTED_GAME ; but only if first game selected
  bne .intro_scroll_end
  lda <SELECTED_GAME+1
  bne .intro_scroll_end
  ldx #8
.intro_scroll:
  bit $2002
  lda #0
  sta $2005
  stx $2005
  jsr waitblank_simple
  jsr read_controller
  ldy #0
  cmp <BUTTONS
  bne .intro_scroll_end
  inx
  inx
  inx
  inx
  .if ENABLE_TOP_OFFSET=0
  cpx #$f0
  .else
  cpx #$e8 ; for large images on the top
  .endif
  bne .intro_scroll  
  .intro_scroll_end:
  .endif

  jsr scroll_fix
  ; updating sprites
  jsr sprite_dma_copy
  lda #%00001000  ; switch to first nametable
  sta $2000
  lda #%00011110  ; enable sprites
  sta $2001
  
  ; do not hold buttons!
  jsr wait_buttons_not_pressed

  ; main loop
infin:  
  jsr waitblank
  jsr buttons_check
  jmp infin

NMI: ; not used
  rti

IRQ: ; not used
  rti

  .include "misc.asm"
  .include "buttons.asm"
  .include "video.asm"
  .include "sounds.asm"
  .include "tests.asm"
  .include "buildinfo.asm"
  .include "saves.asm"
  .include "preloader.asm"

  ; patterns
  .bank 12
  .org $8000
chr_data:
  .incbin "menu_header_pattern_table.bin"
  .org $8000 + 224 * 16
  .incbin "menu_footer_pattern_table.bin"
  .org $8800
  .incbin "menu_symbols.bin"
  .org $9000
  .incbin "menu_sprites.bin"

  .bank 13
  .org $A000
  ; background
nametable_header:
  .incbin "menu_header_name_table.bin"
nametable_footer:
  .incbin "menu_footer_name_table.bin"
tilepal: 
  ; palette for background
  .incbin "bg_palette0.bin"
  .incbin "bg_palette1.bin"
  .incbin "bg_palette2.bin"
  .incbin "bg_palette3.bin"
  .incbin "sprites_palette.bin" ; palette for sprites
  .org tilepal+$14 ; custom palette for stars
  .db $00, $22, $00, $00
  .db $00, $14, $00, $00
  .db $00, $05, $00, $00

header_attribute_table:
  .incbin "menu_header_attribute_table.bin"

  ; routines to be executed from RAM
  .bank 14
  .org $0500 ; actually it's $C500 in cartridge memory
ram_routines:
  .include "banking.asm"
  .include "flash.asm"  
  .include "loader.asm"
