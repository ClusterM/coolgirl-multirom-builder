NESASM=tools/nesasm.exe
EMU=fceux
SOURCES=menu.asm
MENU=menu.nes
TILER=tools/NesTiler.exe
COMBINER=tools/CoolgirlCombiner.exe
DUMPER=tools/FamicomDumper.exe 
PORT?=auto
MENU_IMAGE?=menu_header.png
NOSORT?=0
SORT?=
GAMES?=games.list
SIZE?=128
MAXCHRSIZE=256
OFFSETS?=offsets_$(GAMES).json
REPORT?=report_$(GAMES).txt
MENU_ROM?=menu_$(GAMES).nes
UNIF?=multirom_$(GAMES).unf
NES20?=multirom_$(GAMES).nes
LANGUAGE?=rus
NESASM_OPTS+=--symbols=$(UNIF) --symbols-offset=0 -iWss
BADSECTORS?=-1

ifneq ($(NOSORT),0)
SORT=--nosort
endif

ifneq ($(BADSECTORS),-1)
BADS=--badsectors $(BADSECTORS)
endif

all: $(UNIF)
build: $(UNIF)

$(MENU_ROM): $(SOURCES) games.asm header footer symbols sprites
	$(NESASM) $(SOURCES) --output=$(MENU_ROM) $(NESASM_OPTS)

games.asm $(OFFSETS): $(GAMES)
	$(COMBINER) prepare --games $(GAMES) --asm games.asm --maxromsize $(SIZE) --maxchrsize $(MAXCHRSIZE) --offsets $(OFFSETS) --report $(REPORT) $(SORT) --language $(LANGUAGE)

$(UNIF): $(MENU_ROM) $(OFFSETS)
	$(COMBINER) combine --loader $(MENU_ROM) --offsets $(OFFSETS) --unif $(UNIF)

unif: $(UNIF)	

$(NES20): $(MENU_ROM) $(OFFSETS)
	$(COMBINER) combine --loader $(MENU_ROM) --offsets $(OFFSETS) --nes20 $(NES20)

nes20: $(NES20)	

clean:
	rm -f *.bin stdout.txt games.asm *.nl *.lst $(MENU) $(UNIF) $(MENU_ROM) $(REPORT) $(OFFSETS)

run: $(UNIF)
	$(EMU) $(UNIF)

upload: $(UNIF)
	upload.bat $(UNIF)

runmenu: $(MENU_ROM)
	$(EMU) $(MENU_ROM)

flash: clean $(UNIF)
	$(DUMPER) write-coolgirl --file $(UNIF) --port $(PORT) --sound --check --checkpause $(BADS) --ignorebadsectors

header: $(MENU_IMAGE)
	$(TILER) --i0 $(MENU_IMAGE) --enable-palettes 0,1,2 --out-pattern-table0 menu_header_pattern_table.bin --out-name-table0 menu_header_name_table.bin --out-attribute-table0 menu_header_attribute_table.bin --out-palette0 bg_palette0.bin --out-palette1 bg_palette1.bin --out-palette2 bg_palette2.bin --bgcolor #000000

menu_header_pattern_table.bin: header
menu_header_name_table.bin: header
menu_header_attribute_table.bin: header
bg_palette0.bin: header
bg_palette1.bin: header
bg_palette2.bin: header

footer_symbols: menu_symbols.png menu_footer.png
	$(TILER) --i0 menu_symbols.png --i1 menu_footer.png --enable-palettes 3 --pattern-offset0 128 --pattern-offset1 96 --out-pattern-table0 menu_symbols.bin --out-pattern-table1 menu_footer_pattern_table.bin --out-name-table1 menu_footer_name_table.bin --out-palette3 bg_palette3.bin --bgcolor #000000

footer: footer_symbols
symbols: footer_symbols

sprites: menu_sprites.png
	$(TILER) --mode sprites --i0 menu_sprites.png --enable-palettes 0 --out-pattern-table0 menu_sprites.bin --out-palette0 sprites_palette.bin --bgcolor #000000

menu_sprites.bin: sprites
sprites_palette.bin: sprites

menu_symbols.bin: footer_symbols
menu_footer_pattern_table.bin: footer_symbols
menu_footer_name_table.bin: footer_symbols
bg_palette3.bin: footer_symbols

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

badstest:
	$(DUMPER) test-bads-coolgirl --port $(PORT) --sound
