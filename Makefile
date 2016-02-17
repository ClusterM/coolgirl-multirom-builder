NESASM=tools/nesasm.exe
EMU=tools/fceux/fceux.exe
EMU_ALT=tools/fceux/fceux.exe
EMU_ALT_ALT=tools/nestopia/nestopia.exe
SOURCES=menu.asm
EXECUTABLE=menu.nes
UNIF?=multirom.unf
CONVERTER=tools/TilesConverter.exe
COMBINER=tools/CoolgirlCombiner.exe
DUMPER=tools/famicom-dumper.exe 
PORT?=auto
MENU_IMAGE?=menu.png
SORT?=sort
GAMES?=games.test
SIZE?=64

all: $(SOURCES) $(EXECUTABLE)

$(EXECUTABLE): $(SOURCES) menu_pattern0.dat menu_nametable0.dat menu_palette0.dat menu_pattern1.dat menu_palette1.dat games.asm
	rm -f $(EXECUTABLE) && $(NESASM) $(SOURCES)

games.asm: $(GAMES)
	$(COMBINER) $(GAMES) games.asm $(SORT) $(SIZE)

$(UNIF): $(EXECUTABLE) $(GAMES)
	$(COMBINER) $(GAMES) $(UNIF) $(SORT) $(SIZE)
#	cp $(EXECUTABLE) $(UNIF)

build: $(UNIF)

clean:
	rm -f *.dat stdout.txt games.asm menu.bin $(UNIF) $(EXECUTABLE)

run: $(UNIF)
	$(EMU) $(UNIF)

runmenu: $(EXECUTABLE)
	$(EMU_ALT) $(EXECUTABLE)
#	$(EMU_ALT_ALT) .\$(EXECUTABLE)

flash: clean $(UNIF)
	$(DUMPER) write-coolgirl -f $(UNIF) -p $(PORT)

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

sramtest:
	$(DUMPER) test-sram -p $(PORT)

batterytest:
	$(DUMPER) test-battery -p $(PORT)

chrtest:
	$(DUMPER) test-chr -p $(PORT)

chrtestfull:
	$(DUMPER) test-chr-coolboy -p $(PORT)
