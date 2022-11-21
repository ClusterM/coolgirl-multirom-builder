GAMES ?=        games.list
MENU_IMAGE ?=   menu_header.png
LANGUAGE ?=     eng
SIZE ?=         128
MAXCHRSIZE?=    256
DUMPER_OPTS ?=  --port auto
NESASM_EXTRA_OPTS ?=

OFFSETS ?=      offsets_$(GAMES).json
OUTPUT_UNIF ?=  multirom_$(GAMES).unf
OUTPUT_NES20 ?= multirom_$(GAMES).nes
OUTPUT_BIN ?=   multirom_$(GAMES).bin
NESASM_OPTS +=  $(NESASM_EXTRA_OPTS)

SOURCES =       menu.asm banking.asm buildinfo.asm buttons.asm flash.asm loader.asm misc.asm preloader.asm saves.asm sounds.asm tests.asm video.asm
CONFIGS_DIR =   configs
IMAGES_DIR =    images

# tools
TILER =         ./tools/nestiler
COMBINER =      ./tools/coolgirl-combiner
NESASM =        ./tools/nesasm
COLORS =        ./tools/nestiler-colors.json 
EMU =           fceux64
DUMPER =        famicom-dumper

MINDKIDS ?= 0
ifneq ($(MINDKIDS),0)
MINDKIDS_OPTION=--mindkids
endif

SAVES ?= 0
ifneq ($(SAVES),0)
SAVES_OPTION=--saves
endif

NOSORT ?= 0
ifneq ($(NOSORT),0)
SORT_OPTION=--nosort
endif

BADSECTORS ?= -1
ifneq ($(BADSECTORS),-1)
BADS_OPTION=--badsectors $(BADSECTORS)
endif

REPORT ?=
ifneq ($(REPORT),)
REPORT_OPTION=--report $(REPORT)
endif

NOLOCK ?= 0
LOCK_OPTION = --lock
ifneq ($(NOLOCK),0)
LOCK_OPTION=
endif

ENABLE_SOUND ?= -1
ifneq ($(ENABLE_SOUND),-1)
NESASM_OPTS += -D ENABLE_SOUND = $(ENABLE_SOUND)
endif

STARS ?= -1
ifneq ($(STARS),-1)
NESASM_OPTS += -D STARS=$(STARS)
endif

STARS_DIRECTION ?= -1
ifneq ($(STARS_DIRECTION),-1)
NESASM_OPTS += -D STARS_DIRECTION=$(STARS_DIRECTION)
endif

STAR_SPAWN_INTERVAL ?= -1
ifneq ($(STAR_SPAWN_INTERVAL),-1)
NESASM_OPTS += -D STAR_SPAWN_INTERVAL=$(STAR_SPAWN_INTERVAL)
endif

ENABLE_LAST_GAME_SAVING ?= -1
ifneq ($(ENABLE_LAST_GAME_SAVING),-1)
NESASM_OPTS += -D ENABLE_LAST_GAME_SAVING=$(ENABLE_LAST_GAME_SAVING)
endif

ENABLE_RIGHT_CURSOR ?= -1
ifneq ($(ENABLE_RIGHT_CURSOR),-1)
NESASM_OPTS += -D ENABLE_RIGHT_CURSOR=$(ENABLE_RIGHT_CURSOR)
endif

ENABLE_DIM_IN ?= -1
ifneq ($(ENABLE_DIM_IN),-1)
NESASM_OPTS += -D ENABLE_DIM_IN=$(ENABLE_DIM_IN)
endif

DIM_IN_DELAY ?= -1
ifneq ($(DIM_IN_DELAY),-1)
NESASM_OPTS += -D DIM_IN_DELAY=$(DIM_IN_DELAY)
endif

ENABLE_DIM_OUT ?= -1
ifneq ($(ENABLE_DIM_OUT),-1)
NESASM_OPTS += -D ENABLE_DIM_OUT=$(ENABLE_DIM_OUT)
endif

DIM_OUT_DELAY ?= -1
ifneq ($(DIM_OUT_DELAY),-1)
NESASM_OPTS += -D DIM_OUT_DELAY=$(DIM_OUT_DELAY)
endif

GAMES_DB =                        games_$(GAMES)_$(LANGUAGE).asm
MENU_ROM ?=                       menu_$(GAMES_DB).nes
MENU_HEADER_PATTERN_TABLE_BIN =   menu_header_pattern_table_$(MENU_IMAGE).bin
MENU_HEADER_NAME_TABLE_BIN =      menu_header_name_table_$(MENU_IMAGE).bin
MENU_HEADER_ATTRIBUTE_TABLE_BIN = menu_header_attribute_table_$(MENU_IMAGE).bin
MENU_HEADER_BG_PALETTE_0 =        bg_palette0_$(MENU_IMAGE).bin
MENU_HEADER_BG_PALETTE_1 =        bg_palette1_$(MENU_IMAGE).bin
MENU_HEADER_BG_PALETTE_2 =        bg_palette2_$(MENU_IMAGE).bin
HEADER_FILES =                    $(MENU_HEADER_PATTERN_TABLE_BIN) $(MENU_HEADER_NAME_TABLE_BIN) \
                                      $(MENU_HEADER_ATTRIBUTE_TABLE_BIN) $(MENU_HEADER_BG_PALETTE_0) \
                                      $(MENU_HEADER_BG_PALETTE_1) $(MENU_HEADER_BG_PALETTE_2)
FOOTER_FILES =                    footer.pt footer.nt
SYMBOL_FILES =                    menu_symbols.bin
SPRITE_FILES =                    menu_sprites.bin sprites_palette.bin

NESASM_OPTS +=                    -C MENU_HEADER_PATTERN_TABLE_BIN=$(MENU_HEADER_PATTERN_TABLE_BIN) \
                                      -C MENU_HEADER_NAME_TABLE_BIN=$(MENU_HEADER_NAME_TABLE_BIN) \
                                      -C MENU_HEADER_ATTRIBUTE_TABLE_BIN=$(MENU_HEADER_ATTRIBUTE_TABLE_BIN) \
                                      -C MENU_HEADER_BG_PALETTE_0=$(MENU_HEADER_BG_PALETTE_0) \
                                      -C MENU_HEADER_BG_PALETTE_1=$(MENU_HEADER_BG_PALETTE_1) \
                                      -C MENU_HEADER_BG_PALETTE_2=$(MENU_HEADER_BG_PALETTE_2)

default: $(OUTPUT_NES20)
build: $(OUTPUT_NES20)
all: $(OUTPUT_UNIF) $(OUTPUT_NES20) $(OUTPUT_BIN)

$(MENU_ROM): $(SOURCES) $(GAMES_DB) $(HEADER_FILES) $(FOOTER_FILES) $(SYMBOL_FILES) $(SPRITE_FILES)
	$(NESASM) menu.asm --output=$(MENU_ROM) $(NESASM_OPTS)

menu: $(MENU_ROM)

$(GAMES_DB) $(OFFSETS): $(CONFIGS_DIR)/$(GAMES)
	$(COMBINER) prepare --games $(CONFIGS_DIR)/$(GAMES) --asm $(GAMES_DB) \
		--maxromsize $(SIZE) --maxchrsize $(MAXCHRSIZE) \
		--offsets $(OFFSETS) $(REPORT_OPTION) $(SORT_OPTION) \
		--language $(LANGUAGE) $(BADS_OPTION) \
        $(SAVES_OPTION)

#$(OUTPUT_UNIF): $(SOURCES) $(HEADER_FILES) $(FOOTER_FILES) $(SYMBOL_FILES) $(SPRITE_FILES) $(MENU_ROM) $(OFFSETS)
#	$(COMBINER) combine --loader $(MENU_ROM) --offsets $(OFFSETS) --unif $(OUTPUT_UNIF) $(MINDKIDS_OPTION) 

$(OUTPUT_UNIF): $(SOURCES) $(HEADER_FILES) $(FOOTER_FILES) $(SYMBOL_FILES) $(SPRITE_FILES) $(CONFIGS_DIR)/$(GAMES)
	$(COMBINER) build --games $(CONFIGS_DIR)/$(GAMES) --asm $(GAMES_DB) \
		--maxromsize $(SIZE) --maxchrsize $(MAXCHRSIZE) $(REPORT_OPTION) $(MINDKIDS_OPTION) $(SORT_OPTION) --language $(LANGUAGE) \
		--nesasm $(NESASM) --nesasm-args "$(NESASM_OPTS)" $(BADS_OPTION) $(SAVES_OPTION) \
		--unif $(OUTPUT_UNIF)

unif: $(OUTPUT_UNIF)	

#$(OUTPUT_NES20): $(SOURCES) $(HEADER_FILES) $(FOOTER_FILES) $(SYMBOL_FILES) $(SPRITE_FILES) $(MENU_ROM) $(OFFSETS)
#	$(COMBINER) combine --loader $(MENU_ROM) --offsets $(OFFSETS) --nes20 $(OUTPUT_NES20) $(MINDKIDS_OPTION) 

$(OUTPUT_NES20): $(SOURCES) $(HEADER_FILES) $(FOOTER_FILES) $(SYMBOL_FILES) $(SPRITE_FILES) $(CONFIGS_DIR)/$(GAMES)
	$(COMBINER) build --games $(CONFIGS_DIR)/$(GAMES) --asm $(GAMES_DB) \
		--maxromsize $(SIZE) --maxchrsize $(MAXCHRSIZE) $(REPORT_OPTION) $(MINDKIDS_OPTION) $(SORT_OPTION) --language $(LANGUAGE) \
		--nesasm $(NESASM) --nesasm-args "$(NESASM_OPTS)" $(BADS_OPTION) $(SAVES_OPTION) \
		--nes20 $(OUTPUT_NES20)

nes20: $(OUTPUT_NES20)
nes: nes20

#$(OUTPUT_BIN): $(SOURCES) $(HEADER_FILES) $(FOOTER_FILES) $(SYMBOL_FILES) $(SPRITE_FILES) $(MENU_ROM) $(OFFSETS)
#	$(COMBINER) combine --loader $(MENU_ROM) --offsets $(OFFSETS) --bin $(OUTPUT_BIN) $(MINDKIDS_OPTION)

$(OUTPUT_BIN): $(SOURCES) $(HEADER_FILES) $(FOOTER_FILES) $(SYMBOL_FILES) $(SPRITE_FILES) $(CONFIGS_DIR)/$(GAMES)
	$(COMBINER) build --games $(CONFIGS_DIR)/$(GAMES) --asm $(GAMES_DB) \
		--maxromsize $(SIZE) --maxchrsize $(MAXCHRSIZE) $(REPORT_OPTION)  $(MINDKIDS_OPTION) $(SORT_OPTION) --language $(LANGUAGE) \
		--nesasm $(NESASM) --nesasm-args "$(NESASM_OPTS)" $(BADS_OPTION) $(SAVES_OPTION) \
		--bin $(OUTPUT_BIN)

bin: $(OUTPUT_BIN)

clean:
	rm -f stdout.txt *.nl *.lst *.bin *.txt games_*.asm menu_*.nes multirom_*.unf multirom_*.nes multirom_*.bin offsets*.json

run: $(OUTPUT_NES20)
	$(EMU) $(OUTPUT_NES20)

upload: $(OUTPUT_NES20)
	./upload.bat $(OUTPUT_NES20)

runmenu: $(MENU_ROM)
	$(EMU) $(MENU_ROM)

write: $(OUTPUT_NES20)
	$(DUMPER) write-coolgirl --file $(OUTPUT_NES20) --sound --check $(BADS_OPTION) $(LOCK_OPTION) $(DUMPER_OPTS)

$(HEADER_FILES): $(IMAGES_DIR)/$(MENU_IMAGE)
	$(TILER) --colors $(COLORS) \
		--i0 $(IMAGES_DIR)/$(MENU_IMAGE) \
		--enable-palettes 0,1,2 \
		--out-pattern-table0 $(MENU_HEADER_PATTERN_TABLE_BIN) \
		--out-name-table0 $(MENU_HEADER_NAME_TABLE_BIN) \
		--out-attribute-table0 $(MENU_HEADER_ATTRIBUTE_TABLE_BIN) \
		--out-palette0 $(MENU_HEADER_BG_PALETTE_0) \
		--out-palette1 $(MENU_HEADER_BG_PALETTE_1) \
		--out-palette2 $(MENU_HEADER_BG_PALETTE_2) \
		--bg-color \#000000

$(SYMBOL_FILES): $(IMAGES_DIR)/menu_symbols.png
	$(TILER) --colors $(COLORS) \
		--i0 $(IMAGES_DIR)/menu_symbols.png \
		--enable-palettes 3 \
		--pattern-offset0 128 \
		--pattern-offset1 224 \
		--out-pattern-table0 menu_symbols.bin \
		--out-palette3 bg_palette3.bin \
        --bg-color \#000000

$(SPRITE_FILES): $(IMAGES_DIR)/menu_sprites.png
	$(TILER) --colors $(COLORS) \
		--mode sprites \
		--i0 $(IMAGES_DIR)/menu_sprites.png \
		--enable-palettes 0 \
		--out-pattern-table0 menu_sprites.bin \
		--out-palette0 sprites_palette.bin \
		--bg-color \#000000
