@echo off
SET GAMES_LIST=games.list
SET OUTPUT_UNIF=multirom.unf
SET OUTPUT_BIN=multirom.bin
SET MENU_IMAGE=menu.png
SET MAX_SIZE=64
@rem SET NOSORT=TRUE
SET CONVERTER=tools\TilesConverter.exe
SET COMBINER=tools\CoolgirlCombiner.exe
SET NESASM=tools\nesasm.exe
SET OFFSETS_FILE=offsets.xml
SET REPORT_FILE=report.txt
if exist menu.nes del menu.nes
if exist menu_pattern0.dat del menu_pattern0.dat
if exist menu_nametable0.dat del menu_nametable0.dat
if exist menu_palette0.dat del menu_palette0.dat
if exist menu_pattern1.dat del menu_pattern1.dat
if exist menu_palette1.dat del menu_palette1.dat
if exist games.asm del games.asm
if exist %OFFSETS_FILE% del %OFFSETS_FILE%
if exist %REPORT_FILE% del %REPORT_FILE%
if exist %OUTPUT_UNIF% del %OUTPUT_UNIF%
if exist %OUTPUT_BIN% del %OUTPUT_BIN%
if [%NOSORT%] NEQ [] else if %NOSORT% NEQ 0 else if /I %NOSORT% NEQ false SET NOSORT=--nosort
@echo on
%CONVERTER% %MENU_IMAGE% menu_pattern0.dat menu_nametable0.dat menu_palette0.dat
%CONVERTER% menu_sprites.png menu_pattern1.dat menu_nametable1.dat menu_palette1.dat
%COMBINER% prepare --games %GAMES_LIST% --asm games.asm --maxsize %MAX_SIZE% --offsets %OFFSETS_FILE% --report %REPORT_FILE% %NOSORT%
%NESASM% menu.asm
%COMBINER% combine --loader menu.nes --offsets %OFFSETS_FILE% --unif %OUTPUT_UNIF% --bin %OUTPUT_BIN%
@if exist %OUTPUT_UNIF% echo Seems like everything is fine! %OUTPUT_UNIF% created.
@pause
