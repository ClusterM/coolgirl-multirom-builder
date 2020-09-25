NESASM=tools/nesasm.exe
EMU=/D/Emulators/fceux/fceux.exe
SOURCES=menu.asm
MENU=menu.nes
CONVERTER=tools/TilesConverter.exe
COMBINER=tools/CoolgirlCombiner.exe
DUMPER=tools/FamicomDumper.exe 
PORT?=auto
MENU_IMAGE?=menu.png
NOSORT?=0
SORT?=
GAMES?=games.list
SIZE?=128
MAXCHRSIZE=256
OFFSETS?=offsets_$(GAMES).json
REPORT?=report_$(GAMES).txt
EXECUTABLE?=menu_$(GAMES).nes
UNIF?=multirom_$(GAMES).unf
NES20?=multirom_$(GAMES).nes
LANGUAGE?=rus
#NESASM_OPTS+=--symbols=$(UNIF) --symbols-offset=0 -iWss

ifneq ($(NOSORT),0)
SORT=--nosort
endif

all: $(UNIF)
build: $(UNIF)

$(EXECUTABLE): $(SOURCES) menu_pattern0.dat menu_nametable0.dat menu_palette0.dat menu_pattern1.dat menu_palette1.dat games.asm
	$(NESASM) $(SOURCES) --output=$(EXECUTABLE) $(NESASM_OPTS)

games.asm $(OFFSETS): $(GAMES)
	$(COMBINER) prepare --games $(GAMES) --asm games.asm --maxromsize $(SIZE) --maxchrsize $(MAXCHRSIZE) --offsets $(OFFSETS) --report $(REPORT) $(SORT) --language $(LANGUAGE)

$(UNIF): $(EXECUTABLE) $(OFFSETS)
	$(COMBINER) combine --loader $(EXECUTABLE) --offsets $(OFFSETS) --unif $(UNIF)

unif: $(UNIF)	

$(NES20): $(EXECUTABLE) $(OFFSETS)
	$(COMBINER) combine --loader $(EXECUTABLE) --offsets $(OFFSETS) --nes20 $(NES20)

nes20: $(NES20)	

clean:
	rm -f *.dat stdout.txt games.asm menu.bin $(MENU) $(UNIF) $(EXECUTABLE) $(REPORT) $(OFFSETS)

run: $(UNIF)
	$(EMU) $(UNIF)

upload: $(UNIF)
	upload.bat $(UNIF)

runmenu: $(EXECUTABLE)
	$(EMU) $(EXECUTABLE)

flash: clean $(UNIF)
	$(DUMPER) write-coolgirl --file $(UNIF) --port $(PORT) --sound --check

menu_pattern0.dat: menu_bg
menu_nametable0.dat: menu_bg
menu_palette0.dat: menu_bg

menu_pattern1.dat: menu_sprites
menu_nametable1.dat: menu_sprites
menu_palette1.dat: menu_sprites

logo_pattern.dat: logo
logo_nametable.dat: logo
logo_palette.dat: logo

menu_bg: $(MENU_IMAGE)
	$(CONVERTER) $(MENU_IMAGE) menu_pattern0.dat menu_nametable0.dat menu_palette0.dat

menu_sprites: menu_sprites.png
	$(CONVERTER) menu_sprites.png menu_pattern1.dat menu_nametable1.dat menu_palette1.dat

fulltest:
	$(DUMPER) test-coolgirl --port $(PORT) --sound

fulltest1:
	$(DUMPER) test-coolgirl --port $(PORT) --sound --testcount 1

fulltestflash: fulltest1 clean flash	

sramtest:
	$(DUMPER) test-sram-coolgirl -p $(PORT) --sound

batterytest:
	$(DUMPER) test-battery --port $(PORT) --mapper coolgirl --sound

chrtest:
	$(DUMPER) test-chr-coolgirl --port $(PORT) --sound
