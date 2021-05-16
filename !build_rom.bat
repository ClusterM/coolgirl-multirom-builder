@echo off
SET GAMES_LIST=configs\games.list
SET OUTPUT_UNIF=multirom.unf
SET OUTPUT_BIN=multirom.bin
SET MENU_IMAGE=images\menu.png
SET MENU_ROM=menu.nes
SET MAX_SIZE=128
SET SORT_GAMES=FALSE
SET TILER=tools\NesTiler.exe
SET COMBINER=tools\CoolgirlCombiner.exe
SET NESASM=tools\nesasm.exe
rem SET NESASM=%NESASM% --symbols=%OUTPUT_UNIF% -iWss 
SET OFFSETS_FILE=offsets.json
SET REPORT_FILE=report.txt
if exist menu.nes del menu.nes
if exist menu_header_pattern_table.bin del menu_header_pattern_table.bin
if exist menu_header_name_table.bin del menu_header_name_table.bin
if exist menu_header_attribute_table.bin del menu_header_attribute_table.bin
if exist bg_palette0.bin del bg_palette0.bin
if exist bg_palette1.bin del bg_palette1.bin
if exist bg_palette2.bin del bg_palette2.bin
if exist bg_palette3.bin del bg_palette3.bin
if exist menu_sprites.bin del menu_sprites.bin
if exist sprites_palette.bin del sprites_palette.bin
if exist menu_symbols.bin del menu_symbols.bin
if exist menu_footer_pattern_table.bin del menu_footer_pattern_table.bin
if exist menu_footer_name_table.bin del menu_footer_name_table.bin
if exist games.asm del games.asm
if exist *.nl del *.nl
if exist %OFFSETS_FILE% del %OFFSETS_FILE%
if exist %REPORT_FILE% del %REPORT_FILE%
if exist %OUTPUT_UNIF% del %OUTPUT_UNIF%
if exist %OUTPUT_BIN% del %OUTPUT_BIN%
if /I %SORT_GAMES% NEQ TRUE SET NOSORTP=--nosort
@echo on
%TILER% --i0 images\menu_header.png --enable-palettes 0,1,2 --out-pattern-table0 menu_header_pattern_table.bin --out-name-table0 menu_header_name_table.bin --out-attribute-table0 menu_header_attribute_table.bin --out-palette0 bg_palette0.bin --out-palette1 bg_palette1.bin --out-palette2 bg_palette2.bin --bgcolor #000000
@if %ERRORLEVEL% neq 0 goto error
%TILER% --i0 images\menu_symbols.png -i1 images\menu_footer.png --enable-palettes 3 --pattern-offset0 128 --pattern-offset1 224 --out-pattern-table0 menu_symbols.bin --out-pattern-table1 menu_footer_pattern_table.bin --out-name-table1 menu_footer_name_table.bin --out-palette3 bg_palette3.bin --bgcolor #000000
@if %ERRORLEVEL% neq 0 goto error
%TILER% --mode sprites --i0 images\menu_sprites.png --enable-palettes 0 --out-pattern-table0 menu_sprites.bin --out-palette0 sprites_palette.bin --bgcolor #000000
@if %ERRORLEVEL% neq 0 goto error
%COMBINER% prepare --games %GAMES_LIST% --asm games.asm --maxromsize %MAX_SIZE% --offsets %OFFSETS_FILE% --report %REPORT_FILE% %NOSORTP%
@if %ERRORLEVEL% neq 0 goto error
%NESASM% menu.asm -o %MENU_ROM%
@if %ERRORLEVEL% neq 0 goto error
%COMBINER% combine --loader %MENU_ROM% --offsets %OFFSETS_FILE% --unif %OUTPUT_UNIF% --bin %OUTPUT_BIN%
@if %ERRORLEVEL% neq 0 goto error
@if exist %OUTPUT_UNIF% echo Seems like everything is fine! %OUTPUT_UNIF% created.
@pause
@exit 0
:error
@echo Oops, something is wrong!
@pause
exit 1
