GAMES?=games.list
MENU_IMAGE?=menu_header.png
LANGUAGE?=rus
SIZE?=128
MAXCHRSIZE?=256
DUMPER_OPTS?=--port auto
NESASM_EXTRA_OPTS?=

OFFSETS?=offsets_$(GAMES).json
UNIF?=multirom_$(GAMES).unf
NES20?=multirom_$(GAMES).nes
BIN?=multirom_$(GAMES).bin
NESASM_OPTS+=$(NESASM_EXTRA_OPTS)
NESASM_OPTS+=--symbols=$(UNIF)

SOURCES=menu.asm banking.asm buildinfo.asm buttons.asm flash.asm loader.asm misc.asm preloader.asm saves.asm sounds.asm tests.asm video.asm
CONFIGS_DIR=configs
IMAGES_DIR=images

# tools
TILER=tools/nestiler
COMBINER=tools/coolgirl-combiner
NESASM=tools/nesasm
EMU=fceux64
DUMPER=tools/famicom-dumper

NOSORT?=0
ifneq ($(NOSORT),0)
SORT_OPTION=--nosort
endif

BADSECTORS?=-1
ifneq ($(BADSECTORS),-1)
BADS_OPTION=--badsectors $(BADSECTORS)
endif

REPORT?=
ifneq ($(REPORT),)
REPORT_OPTION=--report $(REPORT)
endif

LOCK_OPTION=--lock
NOLOCK?=0
ifneq ($(NOLOCK),0)
LOCK_OPTION=
endif

GAMES_DB=games_$(GAMES)_$(LANGUAGE).asm
MENU_ROM?=menu_$(GAMES_DB).nes
MENU_HEADER_PATTERN_TABLE_BIN=menu_header_pattern_table_$(MENU_IMAGE).bin
MENU_HEADER_NAME_TABLE_BIN=menu_header_name_table_$(MENU_IMAGE).bin
MENU_HEADER_ATTRIBUTE_TABLE_BIN=menu_header_attribute_table_$(MENU_IMAGE).bin
MENU_HEADER_BG_PALETTE_0=bg_palette0_$(MENU_IMAGE).bin
MENU_HEADER_BG_PALETTE_1=bg_palette1_$(MENU_IMAGE).bin
MENU_HEADER_BG_PALETTE_2=bg_palette2_$(MENU_IMAGE).bin
HEADER_FILES=$(MENU_HEADER_PATTERN_TABLE_BIN) $(MENU_HEADER_NAME_TABLE_BIN) $(MENU_HEADER_ATTRIBUTE_TABLE_BIN) $(MENU_HEADER_BG_PALETTE_0) $(MENU_HEADER_BG_PALETTE_1) $(MENU_HEADER_BG_PALETTE_2)
FOOTER_FILES=menu_footer_pattern_table.bin menu_footer_name_table.bin bg_palette3.bin
SYMBOL_FILES=menu_symbols.bin
SPRITE_FILES=menu_sprites.bin sprites_palette.bin
NESASM_OPTS+=-C MENU_HEADER_PATTERN_TABLE_BIN=$(MENU_HEADER_PATTERN_TABLE_BIN) -C MENU_HEADER_NAME_TABLE_BIN=$(MENU_HEADER_NAME_TABLE_BIN) -C MENU_HEADER_ATTRIBUTE_TABLE_BIN=$(MENU_HEADER_ATTRIBUTE_TABLE_BIN) -C MENU_HEADER_BG_PALETTE_0=$(MENU_HEADER_BG_PALETTE_0) -C MENU_HEADER_BG_PALETTE_1=$(MENU_HEADER_BG_PALETTE_1) -C MENU_HEADER_BG_PALETTE_2=$(MENU_HEADER_BG_PALETTE_2) -C GAMES_DB=$(GAMES_DB)

all: $(UNIF) $(NES20)	$(BIN)
build: $(UNIF)

$(MENU_ROM): $(SOURCES) $(GAMES_DB) $(HEADER_FILES) $(FOOTER_FILES) $(SYMBOL_FILES) $(SPRITE_FILES)
	$(NESASM) menu.asm --output=$(MENU_ROM) $(NESASM_OPTS)

menu: $(MENU_ROM)

$(GAMES_DB) $(OFFSETS): $(CONFIGS_DIR)/$(GAMES)
	$(COMBINER) prepare --games $(CONFIGS_DIR)/$(GAMES) --asm $(GAMES_DB) \
		--maxromsize $(SIZE) --maxchrsize $(MAXCHRSIZE) \
		--offsets $(OFFSETS) $(REPORT_OPTION) $(SORT_OPTION) \
		--language $(LANGUAGE) $(BADS_OPTION)

#$(UNIF): $(SOURCES) $(HEADER_FILES) $(FOOTER_FILES) $(SYMBOL_FILES) $(SPRITE_FILES) $(MENU_ROM) $(OFFSETS)
#	$(COMBINER) combine --loader $(MENU_ROM) --offsets $(OFFSETS) --unif $(UNIF)

$(UNIF): $(SOURCES) $(HEADER_FILES) $(FOOTER_FILES) $(SYMBOL_FILES) $(SPRITE_FILES) $(CONFIGS_DIR)/$(GAMES)
	$(COMBINER) build --games $(CONFIGS_DIR)/$(GAMES) --asm $(GAMES_DB) \
		--maxromsize $(SIZE) --maxchrsize $(MAXCHRSIZE) $(REPORT_OPTION) $(SORT_OPTION) --language $(LANGUAGE) \
		--nesasm $(NESASM) --nesasm-args "$(NESASM_OPTS)"  $(BADS_OPTION) \
		--unif $(UNIF)

unif: $(UNIF)	

#$(NES20): $(SOURCES) $(HEADER_FILES) $(FOOTER_FILES) $(SYMBOL_FILES) $(SPRITE_FILES) $(MENU_ROM) $(OFFSETS)
#	$(COMBINER) combine --loader $(MENU_ROM) --offsets $(OFFSETS) --nes20 $(NES20)

$(NES20): $(SOURCES) $(HEADER_FILES) $(FOOTER_FILES) $(SYMBOL_FILES) $(SPRITE_FILES) $(CONFIGS_DIR)/$(GAMES)
	$(COMBINER) build --games $(CONFIGS_DIR)/$(GAMES) --asm $(GAMES_DB) \
		--maxromsize $(SIZE) --maxchrsize $(MAXCHRSIZE) $(REPORT_OPTION) $(SORT_OPTION) --language $(LANGUAGE) \
		--nesasm $(NESASM) --nesasm-args "$(NESASM_OPTS)"  $(BADS_OPTION) \
		--nes20 $(NES20)

nes20: $(NES20)	

#$(BIN): $(SOURCES) $(HEADER_FILES) $(FOOTER_FILES) $(SYMBOL_FILES) $(SPRITE_FILES) $(MENU_ROM) $(OFFSETS)
#	$(COMBINER) combine --loader $(MENU_ROM) --offsets $(OFFSETS) --bin $(BIN)

$(BIN): $(SOURCES) $(HEADER_FILES) $(FOOTER_FILES) $(SYMBOL_FILES) $(SPRITE_FILES) $(CONFIGS_DIR)/$(GAMES)
	$(COMBINER) build --games $(CONFIGS_DIR)/$(GAMES) --asm $(GAMES_DB) \
		--maxromsize $(SIZE) --maxchrsize $(MAXCHRSIZE) $(REPORT_OPTION) $(SORT_OPTION) --language $(LANGUAGE) \
		--nesasm $(NESASM) --nesasm-args "$(NESASM_OPTS)"  $(BADS_OPTION) \
		--bin $(BIN)

bin: $(BIN)

clean:
	rm -f stdout.txt *.nl *.lst *.bin *.txt games_*.asm menu_*.nes multirom_*.unf  offsets_*.json

run: $(UNIF)
	$(EMU) $(UNIF)

upload: $(UNIF)
	./upload.bat $(UNIF)

runmenu: $(MENU_ROM)
	$(EMU) $(MENU_ROM)

write: $(UNIF)
	$(DUMPER) write-coolgirl --file $(UNIF) --sound --check $(BADS_OPTION) $(LOCK_OPTION) $(DUMPER_OPTS)

$(HEADER_FILES): $(IMAGES_DIR)/$(MENU_IMAGE)
	$(TILER) --i0 $(IMAGES_DIR)/$(MENU_IMAGE) \
		--enable-palettes 0,1,2 \
		--out-pattern-table0 $(MENU_HEADER_PATTERN_TABLE_BIN) \
		--out-name-table0 $(MENU_HEADER_NAME_TABLE_BIN) \
		--out-attribute-table0 $(MENU_HEADER_ATTRIBUTE_TABLE_BIN) \
		--out-palette0 $(MENU_HEADER_BG_PALETTE_0) \
		--out-palette1 $(MENU_HEADER_BG_PALETTE_1) \
		--out-palette2 $(MENU_HEADER_BG_PALETTE_2) \
		--bgcolor \#000000

$(SYMBOL_FILES) $(FOOTER_FILES): $(IMAGES_DIR)/menu_symbols.png $(IMAGES_DIR)/menu_footer.png
	$(TILER) \
		--i0 $(IMAGES_DIR)/menu_symbols.png \
		--i1 $(IMAGES_DIR)/menu_footer.png \
		--enable-palettes 3 \
		--pattern-offset0 128 \
		--pattern-offset1 224 \
		--out-pattern-table0 menu_symbols.bin \
		--out-pattern-table1 menu_footer_pattern_table.bin \
		--out-name-table1 menu_footer_name_table.bin \
		--out-palette3 bg_palette3.bin --bgcolor \#000000

$(SPRITE_FILES): $(IMAGES_DIR)/menu_sprites.png
	$(TILER) \
		--mode sprites \
		--i0 $(IMAGES_DIR)/menu_sprites.png \
		--enable-palettes 0 \
		--out-pattern-table0 menu_sprites.bin \
		--out-palette0 sprites_palette.bin \
		--bgcolor \#000000

fulltest:
	$(DUMPER) script --cs-file tools/scripts/CoolgirlTests.cs --sound $(DUMPER_OPTS) - full

fulltest1:
	$(DUMPER) script --cs-file tools/scripts/CoolgirlTests.cs --sound $(DUMPER_OPTS) - full 1

fulltestwrite: fulltest1 write

prgramtest:
	$(DUMPER) script --cs-file tools/scripts/CoolgirlTests.cs --sound $(DUMPER_OPTS) - prg-ram

chrtest:
	$(DUMPER) script --cs-file tools/scripts/CoolgirlTests.cs --sound $(DUMPER_OPTS) - chr-ram

batterytest:
	$(DUMPER) script --cs-file tools/scripts/BatteryTest.cs --mapper coolgirl --sound $(DUMPER_OPTS)
