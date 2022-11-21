@echo off

SET GAMES_LIST=configs\games.list
IF NOT "%1"=="" SET GAMES_LIST="%1"

IF EXIST sprites_palette.bin GOTO skip_images

tools\nestiler --colors ./tools/nestiler-colors.json  ^
        --i0 images/menu_header.png ^
        --enable-palettes 0,1,2 ^
        --out-pattern-table0 menu_header_pattern_table_menu_header.png.bin ^
        --out-name-table0 menu_header_name_table_menu_header.png.bin ^
        --out-attribute-table0 menu_header_attribute_table_menu_header.png.bin ^
        --out-palette0 bg_palette0_menu_header.png.bin ^
        --out-palette1 bg_palette1_menu_header.png.bin ^
        --out-palette2 bg_palette2_menu_header.png.bin ^
        --bg-color #000000
IF NOT ERRORLEVEL 0 GOTO error

tools\nestiler --colors ./tools/nestiler-colors.json ^
        --i0 images/menu_symbols.png ^
        --enable-palettes 3 ^
        --pattern-offset0 128 ^
        --pattern-offset1 224 ^
        --out-pattern-table0 menu_symbols.bin ^
        --out-palette3 bg_palette3.bin ^
        --bg-color #000000
IF NOT ERRORLEVEL 0 GOTO error

tools\nestiler --colors ./tools/nestiler-colors.json ^
        --mode sprites ^
        --i0 images/menu_sprites.png ^
        --enable-palettes 0 ^
        --out-pattern-table0 menu_sprites.bin ^
        --out-palette0 sprites_palette.bin ^
        --bg-color #000000
IF NOT ERRORLEVEL 0 GOTO error

:skip_images

IF EXIST multirom.nes DEL multirom.nes
tools\coolgirl-combiner build --games %GAMES_LIST% --asm games_games.list_eng.asm ^
        --maxromsize 128 --maxchrsize 256 --language eng ^
        --nesasm tools\nesasm ^
        --nesasm-args " -C MENU_HEADER_PATTERN_TABLE_BIN=menu_header_pattern_table_menu_header.png.bin -C MENU_HEADER_NAME_TABLE_BIN=menu_header_name_table_menu_header.png.bin -C MENU_HEADER_ATTRIBUTE_TABLE_BIN=menu_header_attribute_table_menu_header.png.bin -C MENU_HEADER_BG_PALETTE_0=bg_palette0_menu_header.png.bin -C MENU_HEADER_BG_PALETTE_1=bg_palette1_menu_header.png.bin -C MENU_HEADER_BG_PALETTE_2=bg_palette2_menu_header.png.bin" ^
        --nes20 multirom.nes
IF NOT ERRORLEVEL 0 GOTO error
IF NOT EXIST multirom.nes GOTO error
echo Success! multirom.nes generated
goto end

:error
echo Oops... error :(

:end
