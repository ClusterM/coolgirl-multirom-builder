NESASM=tools/nesasm.exe
EMU=fceux
SOURCES=menu.asm
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
BIN?=multirom_$(GAMES).bin
LANGUAGE?=rus
NESASM_OPTS+=--symbols=$(UNIF) -iWss
BADSECTORS?=-1
LOCK=--lock

ifneq ($(NOSORT),0)
SORT=--nosort
endif

ifneq ($(BADSECTORS),-1)
BADS=--badsectors $(BADSECTORS)
endif

ifneq ($(NOLOCK),0)
LOCK=
endif

all: $(UNIF) $(NES20)	$(BIN)
build: $(UNIF)

$(MENU_ROM): $(SOURCES) games.asm header footer symbols sprites
	$(NESASM) $(SOURCES) --output=$(MENU_ROM) $(NESASM_OPTS)

menu: $(MENU_ROM)

games.asm $(OFFSETS): $(GAMES)
	$(COMBINER) prepare --games $(GAMES) --asm games.asm --maxromsize $(SIZE) --maxchrsize $(MAXCHRSIZE) --offsets $(OFFSETS) --report $(REPORT) $(SORT) --language $(LANGUAGE) $(BADS)

$(UNIF): $(SOURCES) header footer symbols sprites
	$(COMBINER) build --games $(GAMES) --asm games.asm --maxromsize $(SIZE) --maxchrsize $(MAXCHRSIZE) --report $(REPORT) $(SORT) --language $(LANGUAGE) --nesasm $(NESASM) --unif $(UNIF) $(BADS)
#	$(COMBINER) combine --loader $(MENU_ROM) --offsets $(OFFSETS) --unif $(UNIF)

unif: $(UNIF)	

$(NES20): $(SOURCES) header footer symbols sprites
	$(COMBINER) build --games $(GAMES) --asm games.asm --maxromsize $(SIZE) --maxchrsize $(MAXCHRSIZE) --report $(REPORT) $(SORT) --language $(LANGUAGE) --nesasm $(NESASM) --nes20 $(NES20) $(BADS)
#	$(COMBINER) combine --loader $(MENU_ROM) --offsets $(OFFSETS) --nes20 $(NES20)

nes20: $(NES20)	

$(BIN): $(SOURCES) header footer symbols sprites
	$(COMBINER) build --games $(GAMES) --asm games.asm --maxromsize $(SIZE) --maxchrsize $(MAXCHRSIZE) --report $(REPORT) $(SORT) --language $(LANGUAGE) --nesasm $(NESASM) --bin $(BIN) $(BADS)
#	$(COMBINER) combine --loader $(MENU_ROM) --offsets $(OFFSETS) --bin $(BIN)

bin: $(BIN)

clean:
	rm -f stdout.txt games.asm *.nl *.lst $(UNIF) $(NES20) $(BIN) $(MENU_ROM) $(REPORT) $(OFFSETS)
	rm -f menu_header_pattern_table.bin menu_header_name_table.bin menu_header_attribute_table.bin
	rm -f bg_palette0.bin bg_palette1.bin bg_palette2.bin bg_palette3.bin menu_sprites.bin sprites_palette.bin menu_symbols.bin menu_footer_pattern_table.bin menu_footer_name_table.bin

run: $(UNIF)
	$(EMU) $(UNIF)

upload: $(UNIF)
	./upload.bat $(UNIF)

runmenu: $(MENU_ROM)
	$(EMU) $(MENU_ROM)

write: clean $(UNIF)
	$(DUMPER) write-coolgirl --file $(UNIF) --port $(PORT) --sound --check $(BADS) --ignorebadsectors $(LOCK)

header: $(MENU_IMAGE)
	$(TILER) --i0 $(MENU_IMAGE) --enable-palettes 0,1,2 --out-pattern-table0 menu_header_pattern_table.bin --out-name-table0 menu_header_name_table.bin --out-attribute-table0 menu_header_attribute_table.bin --out-palette0 bg_palette0.bin --out-palette1 bg_palette1.bin --out-palette2 bg_palette2.bin --bgcolor \#000000

menu_header_pattern_table.bin: header
menu_header_name_table.bin: header
menu_header_attribute_table.bin: header
bg_palette0.bin: header
bg_palette1.bin: header
bg_palette2.bin: header

footer_symbols: menu_symbols.png menu_footer.png
	$(TILER) --i0 menu_symbols.png --i1 menu_footer.png --enable-palettes 3 --pattern-offset0 128 --pattern-offset1 224 --out-pattern-table0 menu_symbols.bin --out-pattern-table1 menu_footer_pattern_table.bin --out-name-table1 menu_footer_name_table.bin --out-palette3 bg_palette3.bin --bgcolor \#000000

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

fulltestwrite: fulltest1 clean write

prgramtest:
	$(DUMPER) test-prg-ram-coolgirl -p $(PORT) --sound

batterytest:
	$(DUMPER) test-battery --port $(PORT) --mapper coolgirl --sound

chrtest:
	$(DUMPER) test-chr-coolgirl --port $(PORT) --sound

badstest:
	$(DUMPER) test-bads-coolgirl --port $(PORT) --sound
