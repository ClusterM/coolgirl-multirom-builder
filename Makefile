NESASM=tools/nesasm.exe
EMU=tools/fceux/fceux.exe
SOURCES=menu.asm
MENU=menu.nes
CONVERTER=tools/TilesConverter.exe
COMBINER=tools/CoolgirlCombiner.exe
DUMPER=tools/famicom-dumper.exe 
PORT?=auto
MENU_IMAGE?=menu.png
NOSORT?=0
SORT?=
GAMES?=games.test
SIZE?=64
OFFSETS?=offsets.xml
REPORT?=report_$(GAMES).txt
EXECUTABLE?=menu_$(GAMES).nes
UNIF?=multirom_$(GAMES).unf

ifneq ($(NOSORT),0)
SORT=--nosort
endif

all: $(UNIF)
build: $(UNIF)

$(EXECUTABLE): $(SOURCES) menu_pattern0.dat menu_nametable0.dat menu_palette0.dat menu_pattern1.dat menu_palette1.dat games.asm
	rm -f $(MENU) && rm -f $(EXECUTABLE) && $(NESASM) $(SOURCES) && mv -f $(MENU) $(EXECUTABLE)

games.asm $(OFFSETS): $(GAMES)
	$(COMBINER) prepare --games $(GAMES) --asm games.asm --maxsize $(SIZE) --offsets $(OFFSETS) --report $(REPORT) $(SORT)

$(UNIF): $(EXECUTABLE) $(OFFSETS)
	$(COMBINER) combine --loader $(EXECUTABLE) --offsets $(OFFSETS) --unif $(UNIF)

clean:
	rm -f *.dat stdout.txt games.asm menu.bin $(MENU) $(UNIF) $(EXECUTABLE) $(REPORT)

run: $(UNIF)
	$(EMU) $(UNIF)

upload: $(UNIF)
	upload.bat $(UNIF)

runmenu: $(EXECUTABLE)
	$(EMU) $(EXECUTABLE)

flash: clean $(UNIF)
	$(DUMPER) write-coolgirl --file $(UNIF) --port $(PORT) --sound

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

sramtest:
	$(DUMPER) test-sram-coolgirl -p $(PORT) --sound

batterytest:
	$(DUMPER) test-battery --port $(PORT) --mapper coolgirl --sound

chrtest:
	$(DUMPER) test-chr-coolgirl --port $(PORT) --sound
