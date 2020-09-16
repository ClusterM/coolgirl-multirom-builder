; INES header stuff
	.inesprg 8   ; 8 banks of PRG = 128kB
	.ineschr 0   ; no CHR, RAM only
	.inesmir 0   ; horizontal mirroring
	.inesmap 2   ; UxROM

	; settings
ENABLE_STARS .equ 1
ENABLE_START_SCROLLING .equ 0
ENABLE_LAST_GAME_SAVING .equ 1
ENABLE_TOP_OFFSET .equ 0
ENABLE_RIGHT_CURSOR .equ 1
GAME_NAMES_OFFSET .equ 2

	; games settings
	.include "games.asm"

	;место под лоадер
;	.rsset $0500
;LOADER .rs 256
	; функции для работы с флешем
;	.rsset $0600
;FLASH_WRITER .rs 256
	; выбираем область памяти для информации о спрайтах
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

	.rsset $6000 ; а это для настроек в SRAM
SRAM_SIG .rs 3
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

	.bank 15   ; последний банк
	.org $FFFA  ; тут у нас хранятся векторы
	.dw NMI    ; NMI вектор
	.dw Start  ; ресет-вектор, указываем на начало программы
	.dw IRQ    ; прерывания

	.bank 15   ; последний банк
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
	
	jsr console_detect

	lda #%00000000 ; disable PPU
	sta $2001
	jsr waitblank_simple

	ldx #$00
.loadloader:
	lda loader+$C000, x ; loading loader and other RAM routines
	sta loader, x
	lda loader+$C100, x
	sta loader+$100, x
	lda loader+$C200, x
	sta loader+$200, x
	inx             
	bne .loadloader
	
	lda #%00001011 ; mirroring, chr-write, enable sram
	sta $5007
	
	jsr load_base_chr
	jsr load_base_pal
	
	; обffуфяем спрайты
	jsr clear_sprites
	jsr sprite_dma_copy

	; устанавливаем выбранную изначально игру и строку, обнуляем переменные
	lda #0
	sta SCROLL_LINES_TARGET
	sta SCROLL_LINES_TARGET+1
	sta SELECTED_GAME
	sta SELECTED_GAME+1	
	sta SCROLL_FINE
	sta KONAMI_CODE_STATE
	sta LAST_STARTED_SAVE
	
	jsr load_state ; загружаем сохранённое состояние
	;lda #13	; test
	;sta LAST_STARTED_SAVE ; test
	;lda #$15	; test
	;sta SCROLL_LINES_TARGET ; test
	;lda #$00	; test
	;sta SCROLL_LINES_TARGET+1 ; test
	;lda #$17	; test
	;sta SELECTED_GAME ; test
	;lda #$00	; test
	;sta SELECTED_GAME+1 ; test
	jsr save_all_saves ;  сохраняем предыдущую сейвку во флеш, если есть
	
	lda #%00001000 ; mirroring, chr-ro, disable sram
	sta $5007
	
	lda SCROLL_LINES_TARGET
	sta SCROLL_LINES
	sta LAST_LINE_GAME
	sta TMP
	lda SCROLL_LINES_TARGET+1
	sta SCROLL_LINES+1
	sta LAST_LINE_GAME+1
	sta TMP+1
	
	; один раз вычисляем остаток от деления на 30
init_modulo:
	lda TMP+1
	bne do_init_modulo
	lda TMP
	cmp #LINES_PER_SCREEN
	bcs do_init_modulo
	
	jmp init_modulo_done
do_init_modulo:
	lda TMP
	sec
	sbc #LINES_PER_SCREEN
	sta TMP
	lda TMP+1
	sbc #0
	sta TMP+1
	jmp init_modulo
	
init_modulo_done:
	lda TMP
	sta SCROLL_LINES_MODULO
	sta LAST_LINE_MODULO

	jsr set_cursor_targets

	; настраиваем нужные нам спрайты
	ldx SPRITE_0_X_TARGET
	stx SPRITE_0_X
	ldx SPRITE_1_X_TARGET
	stx SPRITE_1_X
	ldx SPRITE_0_Y_TARGET
	;ldx #$03
	stx SPRITE_0_Y
	stx SPRITE_1_Y
	ldx #$00
	stx SPRITE_0_TILE
	;ldx #$FF ; скрыть правый указатель
	stx SPRITE_1_TILE
	ldx #%00000000
	stx SPRITE_0_ATTR
	ldx #%01000000
	stx SPRITE_1_ATTR
	
	; обнуляем и этот счётчик
	lda #0
	sta STAR_SPAWN_TIMER
	; инициализируем генератор случайных чисел
	jsr random_init

	jsr read_controller
	lda #%00000100
	cmp BUTTONS
	bne skip_build_info	
	; информация о сборке
	jmp show_build_info
skip_build_info:
	lda #%00010011
	cmp BUTTONS
	bne not_hidden_rom_1
	lda games_count
	sta SELECTED_GAME
	lda games_count+1
	sta SELECTED_GAME+1
	jmp start_game
not_hidden_rom_1:
	lda #%00100011
	cmp BUTTONS
	bne not_hidden_rom_2
	lda games_count
	clc
	adc #1
	sta SELECTED_GAME
	lda games_count+1
	adc #0
	sta SELECTED_GAME+1
	jmp start_game
not_hidden_rom_2:
	lda #%00000111
	cmp BUTTONS
	bne not_tests
	jmp do_tests
not_tests:

	; выводим названия игр
	ldx #15
	jsr print_last_name
print_next_game_at_start:
	inc LAST_LINE_GAME
	lda LAST_LINE_GAME
	bne print_next_game_at_start_last_line_ok
	inc LAST_LINE_GAME+1
print_next_game_at_start_last_line_ok:
	inc LAST_LINE_MODULO
	lda LAST_LINE_MODULO
	cmp #LINES_PER_SCREEN
	bne print_next_game_at_start_modulo_ok
	lda #0
	sta LAST_LINE_MODULO
print_next_game_at_start_modulo_ok:
	jsr print_last_name
	dex
	bne print_next_game_at_start
	
	jsr waitblank_simple
	lda #%00001010  ; сначала у нас base nametable - второй
	sta $2000
	lda #%00001010  ; и спрайты выключены
	sta $2001
	
	; плавно скроллим начальный экран
	; jmp intro_scroll_done ; тут можно сделать пропуск скроллинга для вредных
	lda SELECTED_GAME ; но только если выбрана первая строка... или лучше игра
	bne intro_scroll_done
	lda SELECTED_GAME+1
	bne intro_scroll_done
	ldx #8
intro_scroll:
	bit $2002
	lda #0
	sta $2005
	stx $2005
	jsr waitblank_simple
	jsr read_controller
	ldy #0
	cmp BUTTONS
	bne intro_scroll_done
	inx
	inx
	inx
	inx
	cpx #$f0
	;cpx #$e8 ; для больших картинок сверху
	bne intro_scroll	
intro_scroll_done:

	; скроллим
	jsr scroll_fix
	; обновляем положение спрайтов через DMA
	jsr sprite_dma_copy
	lda #%00001000  ; теперь nametable - первый
	sta $2000
	lda #%00011110  ; и включаем спрайты
	sta $2001
	
	; не держите кнопки!
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

unrom_bank_data:
	.db $00, $01, $02, $03, $04, $05, $06, $07, $08, $09, $0A, $0B, $0C, $0D, $0E, $0F
	.db $10, $11, $12, $13, $14, $15, $16, $17, $18, $19, $1A, $1B, $1C, $1D, $1E, $1F

chr_address: ; чтобы знать, где хранится CHR
	.dw chr_data
	
	.include "banking.asm"
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
	.incbin "menu_pattern0.dat"
	.org $8800 ; some dirty trick
	.incbin "menu_pattern1.dat"
	.org $9000
	.incbin "menu_pattern1.dat"

	.bank 13
	.org $A000
	; background
nametable:
	.incbin "menu_nametable0.dat"
	.org $A200 ; cut part of it to save memory
tilepal: 
	.incbin "menu_palette0.dat" ; palette for background
	.incbin "menu_palette1.dat" ; palette for sprites
	.org tilepal+$14 ; custom palette for stars
	.db $00, $22, $00, $00
	.db $00, $14, $00, $00
	.db $00, $05, $00, $00

	; routines to be executed from RAM
	.bank 14
	.org $0500 ; actually it's $C500 in cartridge memory
	.include "loader.asm"
  .include "flash.asm"	
