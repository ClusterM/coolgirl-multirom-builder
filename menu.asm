; INES header stuff
	.inesprg 32   ; 32 банка кода - 512KB
	.ineschr 0   ; нет CHR
	.inesmir 0   ; мирроринг
	.inesmap 2   ; UxROM

	.rsset $0000
COPY_SOURCE_ADDR .rs 2
TMP .rs 2

	;место под лоадер
	.rsset $0400
LOADER .rs 256

	; выбираем область памяти для информации о спрайтах
	.rsset $0600
SPRITES .rs 0
SPRITE_0_Y .rs 1
SPRITE_0_TILE .rs 1
SPRITE_0_ATTR .rs 1
SPRITE_0_X .rs 1
SPRITE_1_Y .rs 1
SPRITE_1_TILE .rs 1
SPRITE_1_ATTR .rs 1
SPRITE_1_X .rs 1

	; область памяти для переменных
	.rsset $0500
BUTTONS .rs 1 ; текущие нажатия кнопок
BUTTONS_TMP .rs 1 ; временная переменная для кнопок
	; цели стремления курсоров
SPRITE_0_X_TARGET .rs 1
SPRITE_0_Y_TARGET .rs 1
SPRITE_1_X_TARGET .rs 1
SPRITE_1_Y_TARGET .rs 1
	; выбранная игра
SELECTED_GAME .rs 2
	; переменные для отрисовки названий игр
TEXT_DRAW_GAME .rs 2
TEXT_DRAW_ROW .rs 1

SCROLL_LINES .rs 2 ; текущая строка скроллинга	
SCROLL_LINES_MODULO .rs 1 ; остаток от деления текущей строки на 30
LAST_LINE_MODULO .rs 1
LAST_LINE_GAME .rs 2
SCROLL_FINE .rs 1 ; точное положение строки	
SCROLL_LINES_TARGET .rs 2 ; строка, куда стремится скроллинг	
STAR_SPAWN_TIMER .rs 1 ; таймер спауна звёзд на фоне
RANDOM .rs 1 ; случайные числа
KONAMI_CODE_STATE .rs 1 ; состояние KONAMI кода
	; тут задаются параметры для запуска лоадера

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

	; а это временные переменные лоадера
;LOADER_CHR_BANK_SOURCE_COUNTER .rs 1
;LOADER_CHR_BANK_TARGET_COUNTER .rs 1
;LOADER_CHR_BANK_COUNTER .rs 1

	.bank 63   ; последний банк
	.org $FFFA  ; тут у нас хранятся векторы
	.dw NMI    ; NMI вектор
	.dw Start  ; ресет-вектор, указываем на начало программы
	.dw IRQ    ; прерывания

	.bank 63   ; последний банк
	.org $E000

Start:
	sei ; сразу же отключаем любые прерывания
	ldx #$FF
	txs
	
	lda #%00000000 ; выключаем пока что PPU
	sta $2000
	sta $2001
	
	jsr waitblank_simple

	; очистка памяти
clean_start:
	lda #$00
	sta COPY_SOURCE_ADDR
	;lda #$02 ; для БРО
	sta COPY_SOURCE_ADDR+1
	ldy #$02
	ldx #$08
	;ldx #$06 ; для БРО
clean_start_loop:
	sta [COPY_SOURCE_ADDR], y
	iny
	bne clean_start_loop
	inc COPY_SOURCE_ADDR+1
	dex
	bne clean_start_loop

	jsr load_black
	jsr load_blank
	
	; включаем PPU и гордо демонстрируем чёрный экран четверть секунды
	lda #%00001010
	sta $2001
	
	; ждём 15-30 кадров после включения, иначе всё повиснет
	; потому что китайцы пидорасы
	ldx #30
start_wait:
	jsr waitblank_simple
	dex
	bne start_wait	
	
	lda #%00000000 ; выключаем пока что PPU
	sta $2001
	jsr waitblank_simple

	ldx #$00
loadloader:
	lda loader+$C000, x ; копируем наш лоадер в оперативную память
	sta loader, x
	inx             
	bne loadloader
	
	lda #$0A
	sta $5007
	
;	jmp skip_single_game
	
	; если у нас только одна игра, то запускаем её
;	lda #1
;	cmp games_count
;	bne skip_single_game
	;jsr read_controller
	; пропуск автозапуска
	;lda #%00000100
	;cmp BUTTONS
	;beq skip_single_game
	;lda #0
	;sta SELECTED_GAME
	;jmp start_game
skip_single_game:
	lda #$1E
	jsr select_bank
	lda chr_address ;#$00	
	sta COPY_SOURCE_ADDR ; #$00
	lda chr_address+1 ;#$C0
	sta COPY_SOURCE_ADDR+1 ; #$A0
	jsr load_chr
	lda #0
	jsr select_bank

	; загружаем палитру по адресу $3F00 в PPU
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

	; цвета для букв
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
	
	; обffуфяем спрайты
	ldx #0
	lda #$ff
clear_sprites:
	sta SPRITE_0_Y, x
	inx
	bne clear_sprites

	; устанавливаем выбранную изначально игру и строку, обнуляем переменные
	ldx #0
	
	stx SELECTED_GAME
	stx SELECTED_GAME+1
	stx SCROLL_LINES_TARGET
	stx SCROLL_LINES_TARGET+1

	stx SCROLL_LINES
	stx SCROLL_LINES+1
	stx SCROLL_LINES_MODULO
	stx SCROLL_FINE
	
	stx LAST_LINE_MODULO
	stx LAST_LINE_GAME
	stx LAST_LINE_GAME+1
	
	stx KONAMI_CODE_STATE

	jsr set_cursor_targets

	; настраиваем нужные нам спрайты
	ldx SPRITE_0_X_TARGET
	stx SPRITE_0_X
	ldx SPRITE_1_X_TARGET
	stx SPRITE_1_X
	;ldx SPRITE_0_Y_TARGET
	ldx #$03
	stx SPRITE_0_Y
	stx SPRITE_1_Y
	ldx #$00
	stx SPRITE_0_TILE
	stx SPRITE_1_TILE
	ldx #%00000000
	stx SPRITE_0_ATTR
	ldx #%01000000
	stx SPRITE_1_ATTR
	
	; обнуляем и этот счётчик
	lda #0
	sta STAR_SPAWN_TIMER
	; инициализируем генератор случайных чисел
	lda #$FF
	sta RANDOM
	
	; выводим заголовок
	;jsr draw_header1
	;jsr draw_header2

	jsr read_controller
	lda #%00000100
	cmp BUTTONS
	bne skip_build_info	
	; информация о сборке
	jmp show_build_info
skip_build_info:

	; обновляем положение спрайтов через DMA
	jsr sprite_dma_copy

	; выводим названия игр
	ldx #14
	jsr print_last_name
print_next_game_at_start:
	inc LAST_LINE_GAME
	inc LAST_LINE_MODULO
	jsr print_last_name
	dex
	bne print_next_game_at_start
	
	jsr waitblank_simple
	bit $2002
	lda #0
	sta $2005
	sta $2005
	lda #%00001010  ; сначала у нас base nametable - второй
	sta $2000
	lda #%00001010  ; и спрайты выключены
	sta $2001
	
	; плавно скролим начальный экран
	ldx #0
	;jmp intro_scroll_done
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

	; дорисовываем ещё одну строку за пределами экрана
	inc LAST_LINE_GAME
	inc LAST_LINE_MODULO
	jsr print_last_name
	bit $2002
	lda #0
	sta $2005
	;lda #248 ; для больших картинок сверху
	sta $2005	
	lda #%00001000  ; теперь nametable - первый
	sta $2000
	lda #%00011110  ; и включаем спрайты
	sta $2001
	
	; не держите кнопки!
	jsr wait_buttons_not_pressed

	; основной бесконечный цикл
infin:	
	jsr waitblank

	; в большинстве случаев кнопки не нажаты, зачем тратить время на проверку?
buttons_check:
	lda BUTTONS
	cmp #$00
	beq infin
	
	jsr konami_code_check

button_a:
	lda BUTTONS
	and #%00000001	
	beq button_b
	jsr start_sound
	jmp start_game
	
button_b:
	lda BUTTONS
	and #%00000010	
	beq button_start	
	; nothing to do
	jmp button_done
	
button_start:
	lda BUTTONS
	and #%00001000
	beq button_up
	jsr start_sound
	jmp start_game

button_up:
	lda BUTTONS
	and #%00010000
	beq button_down
	jsr bleep
	lda SELECTED_GAME
	sec
	sbc #1
	sta SELECTED_GAME
	lda SELECTED_GAME+1
	sbc #0
	sta SELECTED_GAME+1
	bmi button_up_ovf
	jsr check_separator_up
	jmp button_done
button_up_ovf:
	lda games_count
	sec
	sbc #1
	sta SELECTED_GAME
	lda games_count+1
	sbc #0
	sta SELECTED_GAME+1
	jmp button_done

button_down:
	lda BUTTONS
	and #%00100000
	beq button_left
	jsr bleep
	lda SELECTED_GAME
	clc
	adc #1
	sta SELECTED_GAME
	lda SELECTED_GAME+1
	adc #0
	sta SELECTED_GAME+1
	cmp games_count+1
	bne button_down_not_ovf
	lda SELECTED_GAME
	cmp games_count
	beq button_down_ovf	
button_down_not_ovf:
	jsr check_separator_down
	jmp button_done
button_down_ovf:
	lda #0
	sta SELECTED_GAME
	sta SELECTED_GAME+1
	jmp button_done
	
button_left:
	lda BUTTONS
	and #%01000000
	beq button_right
	jsr bleep
	lda SCROLL_LINES_TARGET
	sec
	sbc #10
	sta SCROLL_LINES_TARGET
	lda SCROLL_LINES_TARGET+1
	sbc #0
	sta SCROLL_LINES_TARGET+1
	bmi button_left_ovf
	jmp button_left2
button_left_ovf:
	lda #0
	sta SCROLL_LINES_TARGET
	sta SCROLL_LINES_TARGET+1
button_left2:
	lda SELECTED_GAME
	sec
	sbc #10
	sta SELECTED_GAME
	lda SELECTED_GAME+1
	sbc #0
	sta SELECTED_GAME+1
	bmi button_left_ovf2
	jsr check_separator_up
	jmp button_done
button_left_ovf2:
	; TODO
	;lda SELECTED_GAME
	;beq button_left_ovf3
	lda #0
	sta SELECTED_GAME
	sta SELECTED_GAME+1
	jmp button_done
;button_left_ovf3:
;	lda games_count
;	sec
;	sbc #1
;	sta SELECTED_GAME
;	lda games_count+1
;	sbc #0
;	sta SELECTED_GAME+1
;	jmp button_done

button_right:
	lda BUTTONS
	and #%10000000
	beq button_none
	jsr bleep
	lda SCROLL_LINES_TARGET
	clc
	adc #10
	sta SCROLL_LINES_TARGET
	lda SCROLL_LINES_TARGET+1
	adc #0
	sta SCROLL_LINES_TARGET+1
	; проверка на переполнение скроллинга
	lda SCROLL_LINES_TARGET
	sec
	sbc maximum_scroll
	lda SCROLL_LINES_TARGET+1
	sbc maximum_scroll+1
	bcs button_right_ovf
button_right_not_ovf:
	jmp button_right2
button_right_ovf:
	lda maximum_scroll
	sta SCROLL_LINES_TARGET
	lda maximum_scroll+1
	sta SCROLL_LINES_TARGET+1
button_right2:
	lda SELECTED_GAME
	clc
	adc #10
	sta SELECTED_GAME
	lda SELECTED_GAME+1
	adc #0
	sta SELECTED_GAME+1
	; проверка на переполнение выбранной игры
	lda SELECTED_GAME
	sec
	sbc games_count
	lda SELECTED_GAME+1
	sbc games_count+1
	bcs button_right_ovf2
button_right_not_ovf2:
	jsr check_separator_down
	jmp button_done
button_right_ovf2:
	; TODO
	;ldx SELECTED_GAME
	;inx
	;cpx games_count
	;beq button_right_ovf3
	lda games_count
	sec
	sbc #1
	sta SELECTED_GAME
	lda games_count+1
	sbc #0
	sta SELECTED_GAME+1
	jmp button_done
;button_right_ovf3:
;	lda #0
;	sta SELECTED_GAME
;	jmp button_done

button_none:
	; это никогда не должно выполняться, ведь других кнопок нет, а что-то нажато
	jmp infin ; и так вечно
	
button_done:
	jsr set_cursor_targets ; самое время обновить цели
	jsr wait_buttons_not_pressed
	jmp infin ; и так вечно

; пропускаем разделители при прокрутке вверх
check_separator_down:
	lda SELECTED_GAME+1
	jsr select_bank
	ldx SELECTED_GAME
	lda game_types, x
	bne check_separator_down_done
	lda SELECTED_GAME
	clc
	adc #1
	sta SELECTED_GAME
	lda SELECTED_GAME+1
	adc #0
	sta SELECTED_GAME+1
	jmp check_separator_down	
check_separator_down_done:
	rts

; пропускаем разделители при прокрутке вниз
check_separator_up:
	lda SELECTED_GAME+1
	jsr select_bank
	ldx SELECTED_GAME
	lda game_types, x
	bne check_separator_up_done
	lda SELECTED_GAME
	sec
	sbc #1
	sta SELECTED_GAME
	lda SELECTED_GAME+1
	sbc #0
	sta SELECTED_GAME+1
	jmp check_separator_up	
check_separator_up_done:
	rts
	
	; ждём, пока игрок не отпустит кнопку
wait_buttons_not_pressed:
	jsr waitblank ; ждём, пока дорисуется экран
	lda BUTTONS
	and #$FF
	bne wait_buttons_not_pressed
	rts

NMI: ; прерывание, срабатывает при дорисовке экрана, но нам это не нужно
	rti

IRQ: ; не можем использовать тут IRQ... потому что китайцы пидорасы
	rti
	
random:
	; генератор типа случайных чисел, лол, помещает число в регистр A
	lda RANDOM
	lsr A
	bcc random_noeor
	eor #$B4
random_noeor:
	sta RANDOM
	rts	
	
waitblank: 
	;php
	pha
	tya
	pha
	txa
	pha

	bit $2002 ; обнуляем vblank бит, если уже vblank, ибо нефиг
waitblank1:
	lda $2002  ; load A with value at location $2002
	bpl waitblank1  ; if bit 7 is not set (not VBlank) keep checking
	
	; скроллим
	jsr move_scrolling
	jsr scroll_fix
	
	; обновляем положение спрайтов через DMA
	jsr sprite_dma_copy

	; идёт отрисовка, есть время заниматься всяким
	; двигаем курсоры к их целям
	jsr move_cursors

	; читаем данные с контроллера
	jsr read_controller
	; занимаемся звёздами на фоне
	jsr stars
	
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
waitblank_simple1:
	lda $2002  ; load A with value at location $2002
	bpl waitblank_simple1  ; if bit 7 is not set (not VBlank) keep checking
	pla
	rts

scroll_fix:
	; обнуляем скроллинг
	bit $2002	
scroll_x_zero:
	lda #0
scroll_x:
	sta $2005

;	lda SCROLL_LINES
;	sta TMP
;	lda SCROLL_LINES+1
;	sta TMP+1
;scroll_round_lines:
;	bne scroll_round_lines_do_it
;	lda TMP
;	cmp lines_per_screen ; остаток от деления на 30 (строки на двух экранах)
;	bcs scroll_round_lines_do_it
;	jmp start_scroll
;scroll_round_lines_do_it:
;	lda TMP	
;	sec
;	sbc lines_per_screen
;	sta TMP
;	lda TMP+1
;	sbc #0
;	sta TMP+1
;	jmp scroll_round_lines	
;start_scroll: ; определяем base nametable
	lda SCROLL_LINES_MODULO
	cmp lines_per_visible_screen ; видимые строки на экране
	bcc start_scroll_first_screen ; менее 15? Тогда далее
	sec
	sbc lines_per_visible_screen ; уменьшаем на 15?
	ldy #%10001010 ; второй nametable
	jmp start_scroll_really
	
start_scroll_first_screen:
	ldy #%10001000 ; первый nametable
	
start_scroll_really:
	sty $2000
	asl A
	asl A
	asl A
	asl A
	clc
	adc SCROLL_FINE
	;sec    ; для больших картинок сверху
	;sbc #8 ; для больших картинок сверху
	sta $2005	
	rts

scroll_line_down:
	lda SCROLL_LINES
	clc
	adc #1
	sta SCROLL_LINES
	lda SCROLL_LINES+1
	adc #0
	sta SCROLL_LINES+1

	inc SCROLL_LINES_MODULO
	lda SCROLL_LINES_MODULO
	cmp lines_per_screen
	bcc scroll_line_down_modulo_ok
	lda #0
	sta SCROLL_LINES_MODULO
scroll_line_down_modulo_ok:

	lda LAST_LINE_GAME
	clc
	adc #1
	sta LAST_LINE_GAME
	lda LAST_LINE_GAME+1
	adc #0
	sta LAST_LINE_GAME+1

	inc LAST_LINE_MODULO
	lda LAST_LINE_MODULO
	cmp lines_per_screen
	bcc scroll_line_down_modulo_ok2
	lda #0
	sta LAST_LINE_MODULO
scroll_line_down_modulo_ok2:	
	
	jsr print_last_name
	rts

scroll_line_up:
	lda SCROLL_LINES
	sec
	sbc #1
	sta SCROLL_LINES
	lda SCROLL_LINES+1
	sbc #0
	sta SCROLL_LINES+1

	dec SCROLL_LINES_MODULO
	lda SCROLL_LINES_MODULO
	bpl scroll_line_up_modulo_ok
	lda lines_per_screen ; может просто вписать 29?
	sta SCROLL_LINES_MODULO
	dec SCROLL_LINES_MODULO
scroll_line_up_modulo_ok:

	lda LAST_LINE_GAME
	sec
	sbc #1
	sta LAST_LINE_GAME
	lda LAST_LINE_GAME+1
	sbc #0
	sta LAST_LINE_GAME+1

	dec LAST_LINE_MODULO
	lda LAST_LINE_MODULO
	bpl scroll_line_up_modulo_ok2
	lda lines_per_screen
	sta LAST_LINE_MODULO
	dec LAST_LINE_MODULO
scroll_line_up_modulo_ok2:
	
	jsr print_first_name
	rts

load_black:
	; загружаем пустую палитру по адресу $3F00 в PPU
	lda #$3F
	sta $2006
	lda #$00
	sta $2006
	ldx #$00
	lda #$3F ; цвет
load_black_pal:
	sta $2007
	inx
	cpx #16
	bne load_black_pal
	rts

	; очищаем nametable
load_blank:
	lda #$20
	sta $2006
	lda #$00
	sta $2006
	lda #$00
	ldx #0
	ldy #$10
clear_screen1_next:
	sta $2007
	inx
	bne clear_screen1_next
	dey
	bne clear_screen1_next	
	rts

reset_sound:
	lda #0
	sta $4000
	sta $4001
	sta $4002
	sta $4003
	sta $4004
	sta $4005
	sta $4006
	sta $4007
	sta $4008
	sta $4009
	sta $400A
	sta $4010
	sta $4011
	sta $4012
	sta $4013
	rts

sprite_dma_copy:
	; обновляем положение спрайтов через DMA
	ldx #0
	stx $2003
	
	ldx #$06 ; тут задаётся адрес xx00
	stx $4014
	rts
	
	; Загружает заголовок вверху, верхняя часть
draw_header1:
	bit $2002
	lda #$20
	sta $2006
	lda #$00
	sta $2006
	ldx #0
	ldy #$40
load_header_next1:
	lda nametable, x
	sta $2007
	inx
	dey
	bne load_header_next1
	rts
	
	; Загружает заголовок вверху, нижняя часть
draw_header2:
	bit $2002
	lda #$20
	sta $2006
	lda #$40
	sta $2006
	ldx #$40
	ldy #$40
load_header_next2:
	lda nametable, x
	sta $2007
	inx
	dey
	bne load_header_next2
	
	lda #$23
	sta $2006
	lda #$C0
	sta $2006
	ldx #0
	ldy #8
load_header_palette_next:
	lda nametable+$3C0, x
	sta $2007
	inx
	dey
	bne load_header_palette_next
	rts

	; Рисуем нижнюю часть экрана, копирайт, адрес сайта, все дела
draw_footer1:
	ldx #0
	ldy #$40
load_footer_next1:
	lda nametable+$340, x
	sta $2007
	inx
	dey
	bne load_footer_next1	
	rts

	; Вторая половина нижней части
draw_footer2:
	ldx #0
	ldy #$40
load_footer_next2:
	lda nametable+$380, x
	sta $2007
	inx
	dey
	bne load_footer_next2
	rts

	; Вывод названий игры
print_first_name:
	pha
	lda SCROLL_LINES
	sta TEXT_DRAW_GAME
	lda SCROLL_LINES+1
	sta TEXT_DRAW_GAME+1
	lda SCROLL_LINES_MODULO
	sta TEXT_DRAW_ROW
	jsr print_name
	pla
	rts
	
print_last_name:
	pha
	lda LAST_LINE_GAME
	sta TEXT_DRAW_GAME
	lda LAST_LINE_GAME+1
	sta TEXT_DRAW_GAME+1
	lda LAST_LINE_MODULO
	sta TEXT_DRAW_ROW
	jsr print_name
	pla
	rts

print_name:
	;php
	pha
	tya
	pha
	txa
	pha	

	; А не заголовок ли нам надо напечатать?
	lda TEXT_DRAW_GAME+1
	bne print_name_not_header ; явно нет
	lda TEXT_DRAW_GAME
	beq print_name_header1
	cmp #1
	beq print_name_header2
	jmp print_name_not_header	
print_name_header1:
	jsr draw_header1
	jmp print_done
print_name_header2:
	jsr draw_header2
	jmp print_done	
print_name_not_header:	

	; когда мало игр...
	lda TEXT_DRAW_ROW
	clc
	adc games_offset
	sta TEXT_DRAW_ROW

print_name_start:
	asl TEXT_DRAW_ROW ; умножаем на два
	lda TEXT_DRAW_ROW
	; Определяем в какой nametable печатать
	cmp lines_per_screen
	bcc print_addr_first_screen
	; второй
	sec
	sbc lines_per_screen
	lsr A
	lsr A
	lsr A
	clc
	adc #$2C
	bit $2002
	sta $2006
	lda TEXT_DRAW_ROW
	sec
	sbc lines_per_screen
	asl A
	asl A
	asl A
	asl A
	asl A
	sta $2006
	jmp print_addr_select_done
	; первый
print_addr_first_screen:
	lsr A
	lsr A
	lsr A
	clc
	adc #$20
	bit $2002
	sta $2006
	lda TEXT_DRAW_ROW
	asl A
	asl A
	asl A
	asl A
	asl A
	sta $2006
	;jmp print_addr_select_done
print_addr_select_done:

	; Название игры должно быть на два меньше, если это не заголовок
	lda TEXT_DRAW_GAME
	sec
	sbc #2
	sta TEXT_DRAW_GAME
	lda TEXT_DRAW_GAME+1
	sbc #0
	sta TEXT_DRAW_GAME+1

	; А не футер ли нам писать?
	lda TEXT_DRAW_GAME
	sec
	sbc games_count
	lda TEXT_DRAW_GAME+1
	sbc games_count+1
	bcc print_text_line
	ldx TEXT_DRAW_GAME
	cpx games_count
	beq print_addr_footer1
	dex
	cpx games_count
	beq print_addr_footer2
	jmp print_name_done
print_addr_footer2:
	jsr draw_footer2
	; Пусть у футера будет та же палитра, что и у текста
	; В моём случае это по абсолютно СЛУЧАЙНЫМ обстоятельствам красиво
	jmp print_name_done
print_addr_footer1:
	jsr draw_footer1
	jmp print_name_done

print_text_line:
	lda TEXT_DRAW_GAME+1
	jsr select_bank
	lda game_names_list
	clc
	adc TEXT_DRAW_GAME
	sta TMP
	lda game_names_list+1
	adc #0 ;TEXT_DRAW_GAME+1
	sta TMP+1
	lda TMP
	clc 
	adc TEXT_DRAW_GAME
	sta TMP
	lda TMP+1
	adc #0 ;TEXT_DRAW_GAME+1
	sta TMP+1
	
	ldy #0
	lda [TMP], y
	sta COPY_SOURCE_ADDR
	iny
	lda [TMP], y
	sta COPY_SOURCE_ADDR+1

	; сначала пустые пробелы, ведь китайцы пидорасы
	; и нормальное центрирование скроллингом по прерываниям сделать не позволяют
;	ldy TEXT_DRAW_GAME
;	ldx game_names_pos, y
	ldx #3
print_blank:
	lda #$00
	sta $2007
	dex
	bne print_blank
	
	; сам текст...
	;ldx game_names_pos, y
	ldx #3
	ldy #0
	lda #1
print_name_next_char:
	cmp #0 ; после завершающего нуля перестаём читать символы, но выводим нули (пустые тайлы)
	beq print_name_next_char_end_of_line
	lda [COPY_SOURCE_ADDR], y
print_name_next_char_end_of_line:
	sta $2007	
	iny
	inx
	cpx chars_per_line
	bne print_name_next_char
print_name_done:
	; но если это верхняя часть экрана, надо стереть две строки
	lda TEXT_DRAW_ROW
	cmp #4
	bcs print_name_done_really
	ldy chars_per_line
	lda #0
print_name_clear_2nd_line:
	sta $2007
	dey
	bne print_name_clear_2nd_line
	
print_name_done_really:

	; фиксим палитру для текста
	lda TEXT_DRAW_ROW
	cmp lines_per_screen
	bcc print_palette_addr_first_screen
	; второй
	lda #$2F
	sta $2006
	lda TEXT_DRAW_ROW
	sec
	sbc lines_per_screen
	jmp print_palette_addr_select_done
	; первый
print_palette_addr_first_screen:
	lda #$23
	sta $2006
	lda TEXT_DRAW_ROW
print_palette_addr_select_done:
	; один байт палитры распространяется сразу на 4 строки, так что округляем
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
	
print_done:
	pla
	tax
	pla
	tay
	pla
	;plp
	rts

	; Просто константы
chars_per_line:
	.db 32
lines_per_screen:
	.db 30
lines_per_visible_screen:
	.db 15
	
move_cursors:
	; плавно двигаем курсоры к их целям, чтобы было красиво!
	jmp sprite_0y_target_done ; теперь не нужно
	lda SPRITE_0_X_TARGET
	cmp SPRITE_0_X
	beq sprite_0x_target_done
	bcs sprite_0x_target_plus
	lda SPRITE_0_X
	sec
	sbc #4
	sta SPRITE_0_X
	jmp sprite_0x_target_done
sprite_0x_target_plus:
	lda SPRITE_0_X
	clc
	adc #4
	sta SPRITE_0_X
sprite_0x_target_done:
	lda SPRITE_0_Y_TARGET
	cmp SPRITE_0_Y
	beq sprite_0y_target_done
	bcs sprite_0y_target_plus
	lda SPRITE_0_Y
	sec
	sbc #4
	sta SPRITE_0_Y
	jmp sprite_0y_target_done
sprite_0y_target_plus:
	lda SPRITE_0_Y
	clc
	adc #4
	sta SPRITE_0_Y
sprite_0y_target_done:
	lda SPRITE_1_X_TARGET
	cmp SPRITE_1_X
	beq sprite_1x_target_done
	bcs sprite_1x_target_plus
	lda SPRITE_1_X
	sec
	sbc #4
	sta SPRITE_1_X
	jmp sprite_1x_target_done
sprite_1x_target_plus:
	lda SPRITE_1_X
	clc
	adc #4
	sta SPRITE_1_X
sprite_1x_target_done:
	lda SPRITE_1_Y_TARGET
	cmp SPRITE_1_Y
	beq sprite_1y_target_done
	bcs sprite_1y_target_plus
	lda SPRITE_1_Y
	sec
	sbc #4
	sta SPRITE_0_Y ; так всегда и для 0го
	sta SPRITE_1_Y
	jmp sprite_1y_target_done
sprite_1y_target_plus:
	lda SPRITE_1_Y
	clc
	adc #4
	sta SPRITE_0_Y ; так всегда и для 0го
	sta SPRITE_1_Y
sprite_1y_target_done:
	rts

	; Плавно скроллим текст к заданной строке
move_scrolling:
	; Смотрим, с какой скоростью скроллить
	; в зависимости от того, как далека заданная строка от текущей
	lda SCROLL_LINES
	clc
	adc #2
	sta TMP
	lda SCROLL_LINES+1
	adc #0
	sta TMP+1
	lda SCROLL_LINES_TARGET
	sec
	sbc TMP
	lda SCROLL_LINES_TARGET+1
	sbc TMP+1
	bcs move_scrolling_fast_down
	lda SCROLL_LINES_TARGET
	clc
	adc #2
	sta TMP
	lda SCROLL_LINES_TARGET+1
	adc #0
	sta TMP+1
	lda SCROLL_LINES
	sec
	sbc TMP
	lda SCROLL_LINES+1
	sbc TMP+1
	bcs move_scrolling_fast_up
	
	; медленно
	jsr move_scrolling_real
	rts	
	
	; быстро - 4 раза за раз
move_scrolling_fast:
	jsr move_scrolling_real
	jsr move_scrolling_real
	jsr move_scrolling_real
	jsr move_scrolling_real
	rts

move_scrolling_fast_up:
	jmp scroll_line_up

move_scrolling_fast_down:
	jmp scroll_line_down

move_scrolling_real:
	; двигаем экран собственно... а нужно ли? проверяем
	ldx SCROLL_LINES_TARGET
	cpx SCROLL_LINES
	bne move_scrolling_need
	ldx SCROLL_LINES_TARGET+1
	cpx SCROLL_LINES+1
	bne move_scrolling_need
	ldx SCROLL_FINE
	bne move_scrolling_need
	rts	
	
	; нужно
move_scrolling_need:
	lda SCROLL_LINES
	sec
	sbc SCROLL_LINES_TARGET
	lda SCROLL_LINES+1
	sbc SCROLL_LINES_TARGET+1
	bcc scroll_lines_target_plus

	; скроллим вверх
	lda SCROLL_FINE
	sec
	sbc #4
	bmi scroll_lines_target_minus
	sta SCROLL_FINE
	rts	
	
scroll_lines_target_minus:
	and #$0f
	sta SCROLL_FINE
	jsr scroll_line_up
	rts	
	
scroll_lines_target_plus:
	; скроллим вниз
	lda SCROLL_FINE
	; прибавляем fine
	clc
	adc #4
	sta SCROLL_FINE
	cmp #16
	bne scroll_lines_target_done
	; вниз, если fine достиг 16
	lda #0
	sta SCROLL_FINE	
	jsr scroll_line_down
scroll_lines_target_done:
	rts

set_cursor_targets:
	; сначала задаём скроллинг, если нужно
set_scroll_target_not_ok1:
	lda SCROLL_LINES_TARGET
	clc
	adc #10
	sta TMP
	lda SCROLL_LINES_TARGET+1
	adc #0
	sta TMP+1
	lda TMP
	sec
	sbc SELECTED_GAME
	lda TMP+1
	sbc SELECTED_GAME+1
	bcs set_scroll_target_ok1 ; надо скроллить вниз
	lda SCROLL_LINES_TARGET
	clc
	adc #1
	sta SCROLL_LINES_TARGET
	lda SCROLL_LINES_TARGET+1
	adc #0
	sta SCROLL_LINES_TARGET+1
	jmp set_scroll_target_not_ok1	
set_scroll_target_ok1:

set_scroll_target_not_ok2:
	lda SELECTED_GAME
	sec
	sbc SCROLL_LINES_TARGET
	lda SELECTED_GAME+1
	sbc SCROLL_LINES_TARGET+1
	bcs set_scroll_target_ok2 ; надо скроллить вверх
	lda SCROLL_LINES_TARGET
	sec
	sbc #1
	sta SCROLL_LINES_TARGET
	lda SCROLL_LINES_TARGET+1
	sbc #0
	sta SCROLL_LINES_TARGET+1
	jmp set_scroll_target_not_ok2
set_scroll_target_ok2:

	; устанавливаем цели курсоров согласно выбранной игре
	; левый курсор
	ldx SELECTED_GAME
	;ldy game_names_pos, x
	;dey
	;tya
	;asl A
	;asl A
	;asl A
	lda #16
	sta SPRITE_0_X_TARGET
	
	; правый курсор
	lda SELECTED_GAME+1
	jsr select_bank
	ldx SELECTED_GAME
	ldy game_names_pos2, x
	dey
	tya
	asl A
	asl A
	asl A
	sta SPRITE_1_X_TARGET
	
	; их Y координата, всегда одинаковая
	lda SELECTED_GAME
	
	; когда мало игр...
	clc
	adc games_offset
	
	sec 
	sbc SCROLL_LINES_TARGET
	clc
	adc #2
	asl A
	asl A
	asl A
	asl A
	sec
	sbc #1
	;clc	   ; для больших картинок сверху
	;adc #8 ; для больших картинок сверху
	sta SPRITE_0_Y_TARGET
	sta SPRITE_1_Y_TARGET
	rts
	
stars:
	lda STAR_SPAWN_TIMER
	cmp #$E0 ; кол-во звёзд, на котором останавливаемся
	beq stars_spawn_end	
	inc STAR_SPAWN_TIMER
	lda STAR_SPAWN_TIMER
	and #$0f
	cmp #0
	bne stars_spawn_end
	lda STAR_SPAWN_TIMER
	lsr A
	lsr A
	tay
	lda STAR_SPAWN_TIMER
	lda #$FC ; Y, за пределами экрана
	sta SPRITES+4, y
	iny
	jsr random ; рандомный тайл
	and #$03
	clc
	adc #$71
	sta SPRITES+4, y
	iny
	jsr random  ; атрибуты, рандомная палитра
	and #%00000011 ; палитра - это младшие два бита
	ora #%00100000 ; и принудительно выставляем бит низкого приоритета
	sta SPRITES+4, y
	iny
	jsr random
	sta SPRITES+4, y

stars_spawn_end:
	ldy #8
stars_move_next:
	lda SPRITES, y
	cmp #$FF
	beq stars_move_done
	tya
	lsr A
	lsr A
	and #$07
	cmp #$00 ; быстрые звёзды
	beq stars_move_fast
	cmp #$01 ; быстрые звёзды
	beq stars_move_fast
	cmp #$02 ; быстрые звёзды
	beq stars_move_fast
	cmp #$03 ; средние звёзды
	beq stars_move_medium
	cmp #$04 ; средние звёзды
	beq stars_move_medium
	cmp #$05 ; медленные звёзды
	beq stars_move_slow
	cmp #$06 ; медленные звёзды
	beq stars_move_slow
	; сверхмедленные, по дефолту
	lda SPRITES, y
	sbc #1
	jmp stars_moved
stars_move_slow:
	lda SPRITES, y
	sec
	sbc #2
	jmp stars_moved
stars_move_medium:
	lda SPRITES, y
	sec
	sbc #3
	jmp stars_moved
stars_move_fast:
	lda SPRITES, y
	sec
	sbc #4
stars_moved:
	sta SPRITES, y
	cmp #$0A
	bcs stars_move_next1
	lda #$FC
	sta SPRITES, y ; опускаем Y
	jsr random  ; рандомный тайл
	and #$03
	clc
	adc #$71
	sta SPRITES+1, y
	jsr random
	sta SPRITES+3, y ; рандомный X
	jsr random  ; атрибуты, рандомная палитра
	and #%00000011 ; палитра - это младшие два бита
	ora #%00100000 ; и принудительно выставляем бит низкого приоритета
	sta SPRITES+2, y ; рандомная палитра
stars_move_next1:
	iny
	iny
	iny
	iny
	bne stars_move_next	
stars_move_done:
	rts

	; чтение контроллера, два раза
read_controller:
	pha
	tya
	pha
	txa
	pha
	jsr read_controller_real
	ldx BUTTONS_TMP
	jsr read_controller_real
	cpx BUTTONS_TMP
	bne read_controller_fail
	stx BUTTONS
read_controller_fail:
	pla
	tax
	pla
	tay
	pla
	rts
	
	; чтение контроллера, настоящее
read_controller_real:
	;php
	lda #1
	sta $4016
	lda #0
	sta $4016
	ldy #8
read_button:
	lda $4016
	and #$03
	cmp #$01
	ror BUTTONS_TMP
	dey
	bne read_button
	rts
	
konami_code_check:
	ldy KONAMI_CODE_STATE
	lda konami_code, y
	cmp BUTTONS
	bne konami_code_check_fail
	iny
	jmp konami_code_check_done
konami_code_check_fail:
	ldy #0
	lda konami_code ; на случай если неверная кнопка - начало верной последовательности
	cmp BUTTONS
	bne konami_code_check_done
	iny
konami_code_check_done:
	sty KONAMI_CODE_STATE
	rts

konami_code:
	.db $10, $10, $20, $20, $40, $80, $40, $80, $02, $01
konami_code_length:
	.db 10
	
	; звук перемещения курсора
bleep:
	lda #%00000001
	sta $4015
	;square 1
	;lda #%10011111  ; 50% duty, max volume
	lda #$87
	sta $4000
	;lda #%10011010  ; sweep 
	lda #$89
	sta $4001
	;lda #40
	lda #$F0
	sta $4002
	lda #%00000000
	sta $4003
	rts
	
	; звук запуска игры
start_sound:
	lda KONAMI_CODE_STATE
	cmp konami_code_length
	beq start_sound_alt

	lda #%00000001
	sta $4015 ;enable channel(s)	
	;square 1
	;lda #%10011111  ; 50% duty, max volume
	lda #%00111111
	sta $4000
	;lda #%10100010  ; sweep 
	lda #$9A
	sta $4001
	;lda #20
	lda #$FF
	sta $4002
	;lda #%11111000
	lda #$00
	sta $4003
	rts
	
	; звук запуска игры при вводе конами кода
start_sound_alt:
	lda #%00000001
	sta $4015 ;enable channel(s)	
	;square 1
	lda #%10011111  ; 50% duty, max volume
	sta $4000
	lda #%10000011  ; sweep 
	sta $4001
	lda #20
	sta $4002
	lda #%11000000
	sta $4003
	rts
	
	; сам запуск игры
start_game:
	sei ; больше никаких прерываний
	lda #%00000000 ; выключаем экран, чтобы не смотреть на глюки
	sta $2000
	lda #%00000000
	sta $2001
	jsr waitblank_simple ; ждём vblank
	jsr load_black ; чёрный цвет
	jsr load_blank ; очищаем nametable

	ldx #15
start_game_wait_sound:
	;lda $4015 ; ждём, пока доиграет звук
	;and #$01
	;bne start_game_wait_sound	
	jsr waitblank_simple
	dex
	bne start_game_wait_sound	

	; проверяем, не вводился ли konami code
	lda KONAMI_CODE_STATE
	cmp konami_code_length
	bne no_konami_code
	lda games_count
	clc
	adc #2
	sta SELECTED_GAME	
no_konami_code:

	; нужно обнулить назад регистры звука, второй мегамен без этого глючит :(
	jsr reset_sound

	; очистка памяти перед запуском игры
clean:
	lda #$00
	sta COPY_SOURCE_ADDR
	lda #$00
	sta COPY_SOURCE_ADDR+1
	ldy #$02
	ldx #$04
	lda #$00
clean_loop:
	sta [COPY_SOURCE_ADDR], y
	iny
	bne clean_loop
	inc COPY_SOURCE_ADDR+1
	dex
	bne clean_loop
	
	lda SELECTED_GAME+1
	jsr select_bank
	
	; запускаем лоадер согласно выбранной игре	
	ldx SELECTED_GAME
	lda loader_data_reg_0, x
	sta LOADER_REG_0
	lda loader_data_reg_1, x
	sta LOADER_REG_1
	lda loader_data_reg_2, x
	sta LOADER_REG_2
	lda loader_data_reg_3, x
	sta LOADER_REG_3
	lda loader_data_reg_4, x
	sta LOADER_REG_4
	lda loader_data_reg_5, x
	sta LOADER_REG_5
	lda loader_data_reg_6, x
	sta LOADER_REG_6
	lda loader_data_reg_7, x
	sta LOADER_REG_7	
	lda loader_data_chr_start_bank_h, x
	sta LOADER_CHR_START_H
	lda loader_data_chr_start_bank_l, x
	sta LOADER_CHR_START_L
	lda loader_data_chr_start_bank_s, x
	sta LOADER_CHR_START_S
	lda loader_data_chr_count, x
	sta LOADER_CHR_LEFT
	lda #0
	sta LOADER_CHR_COUNT
	
	lda #0
	jsr select_bank
	
	jmp loader
	
select_bank:
	tax
	sta unrom_bank_data, x
	asl A
	asl A
	sta $5005
	rts
	
unrom_bank_data:
	.db $00, $01, $02, $03, $04, $05, $06, $07, $08, $09, $0A, $0B, $0C, $0D, $0E, $0F
	.db $10, $11, $12, $13, $14, $15, $16, $17, $18, $19, $1A, $1B, $1C, $1D, $1E, $1F
	
	; информация о сборке
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

	lda #$23
	sta $2006
	lda #$00
	sta $2006
	jsr draw_footer1
	jsr draw_footer2

	lda #$23
	sta $2006
	lda #$C8
	sta $2006
	lda #$FF
	ldy #$38
build_info_palette:
	sta $2007
	dey
	bne build_info_palette

	lda #$FF
	sta SPRITE_0_Y_TARGET
	sta SPRITE_1_Y_TARGET
	sta SPRITE_0_Y
	sta SPRITE_1_Y
	jsr sprite_dma_copy

	; включаем рендер
	jsr waitblank
	lda #%00001000  ; nametable - первый
	sta $2000
	lda #%00011110
	sta $2001

show_build_info_infin:
	jsr waitblank
	jmp show_build_info_infin

chr_address: ; чтобы знать, где хранится CHR
	.dw chr_data
	
	; паттерны
	.bank 61
	.org $A000
chr_data:
	.incbin "menu_pattern0.dat"
	.org $A800 ; небольшой чит
	.incbin "menu_pattern1.dat"
	.org $B000
	.incbin "menu_pattern1.dat" ; тут его конец можно смело обрезать, он пустой и не испольузется

	.bank 62
	.org $C000 ; Перед лоадером
	; фон меню
nametable:
	.incbin "menu_nametable0.dat"
	; палитра
	.org $C200
tilepal: 
	.incbin "menu_palette0.dat" ; палитра фона меню
	.incbin "menu_palette1.dat" ; палитра спрайтов меню
	.org tilepal+$14 ; кастомная палитра для звёзд на фоне
	.db $00, $22, $00, $00
	.db $00, $14, $00, $00
	.db $00, $05, $00, $00
	; это место в памяти чуть раньше $E400, далее начинается лоадер

	; лоадер
	.bank 62
	.org $0400 ; на самом деле это $E400, но мы будем вызывать код из оперативки
loader:
	; запуск игры!	
	; загружаем тайлы в CHR RAM
	; сколько осталось блоков по 8кб?
	ldx LOADER_CHR_LEFT
	beq chr_loading_done
	dec LOADER_CHR_LEFT
	; старший байт адреса
	ldx LOADER_CHR_START_H
	stx $5000
	; младший
	lda LOADER_CHR_START_L
	sta $5001
	; маска
	ldx #$FE
	stx $5002
	; CHR банк
	ldx LOADER_CHR_COUNT
	stx $5003
	; адрес - откуда берём
	ldx #$00
	stx COPY_SOURCE_ADDR
	ldx LOADER_CHR_START_S
	bne loader_chr_not_null
	ldx #$80
	stx LOADER_CHR_START_S
loader_chr_not_null:
	stx COPY_SOURCE_ADDR+1
	jsr load_chr
	lda LOADER_CHR_START_S
	clc
	adc #$20
	sta LOADER_CHR_START_S
	bcc loader_chr_s_not_inc
	lda LOADER_CHR_START_L	
	clc
	adc #2
	sta LOADER_CHR_START_L
	lda LOADER_CHR_START_H
	adc #0
	sta LOADER_CHR_START_H	
loader_chr_s_not_inc:
	inc LOADER_CHR_COUNT
	jmp loader	
chr_loading_done:

	; выставляем регистры согласно заданным параметрам
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

	; Запуск!
	jmp [$FFFC]

	; Загружаем тайлы в CHR RAM
load_chr:
	lda #$00
	sta $2006
	sta $2006
	ldy #$00
	ldx #$20
load_chr_loop:
	lda [COPY_SOURCE_ADDR], y
	sta $2007
	iny
	bne load_chr_loop
	inc COPY_SOURCE_ADDR+1
	dex
	bne load_chr_loop
	rts

	; настройки игр
	.bank 0
	.org $8000
	.include "games.asm"
