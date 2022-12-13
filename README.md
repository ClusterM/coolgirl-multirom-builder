# COOLGIRL Multirom Builder
[![Build test](https://github.com/ClusterM/coolgirl-multirom-builder/actions/workflows/build.yaml/badge.svg)](https://github.com/ClusterM/coolgirl-multirom-builder/actions/workflows/build.yaml)

This is a toolset that allows you to create multirom images for [COOLGIRL Famicom cartridges](https://github.com/ClusterM/coolgirl-famicom-multicart) (mapper 342). This ROM can be run on a emulator or written to a cartridge.

![Loader menu](https://user-images.githubusercontent.com/4236181/205486564-f5cfbe38-adcb-4574-8b9f-16e534052a8d.gif)

It can:
* Automatically combine up to 1536 games into single binary which can be written to a COOLGIRL cartridge
* Create nice menu where you can easily select a game
* Alphabetically sort games if need
* Use your own image for the menu header and other customizations
* Remember last played game and keep up to 255 saves for "battery-backed" games into flash memory
* Run a built-in hardware tests
* Show build and hardware info
* Add up to three hidden ROMs
* Run on Windows (x64), Linux (x64, ARM, ARM64) and macOS (x64)

.NET 6.0 is required. You need to either install the [.NET 6.0 Runtime](https://dotnet.microsoft.com/en-us/download/dotnet/6.0) or to use the self-contained version.

## How to build a ROM
This package contains multiple tools which need to run sequentially. There is Makefile, so you can use [Make](https://www.gnu.org/software/make/) tool to automatize the whole process. This is the most simple way. Windows users can use [msys2](https://www.msys2.org/) to install and run Make or just run build.bat (not customizable, not recommended).

But you need to create game list and save it in the "configs" directory first.

### Game list format
It's just a text file. Lines started with semicolon are comments. Other lines has format:

    <path_to_filename> [| <menu name>]
    
So each line is a path to a ROM with optional name which will be used in the menu. Example:

    roms/Adventure Island (U) [!].nes | ADVENTURE ISLAND
    roms/Adventure Island II (U) [!].nes | ADVENTURE ISLAND 2
    roms/Adventure Island III (U) [!].nes | ADVENTURE ISLAND 3

Use a trailing "/" to add a whole directory:

    roms/
    
If menu name is not specified it will be based on a filename. Maximum length for menu entry is 29 symbols.

You can use "?" symbol as game name to add hidden ROMs:

    spec/sram.nes | ? 
    spec/controller.nes | ? 

First hidden ROM will be started while holding Up+A+B at startup. Second one will be started while holding Down+A+B at startup. I'm using it to add some hardware tests. Also, you can add third hidden ROM, it will be started using the Konami Code in the loader menu :)

All games are alphabetically sorted by default so you don't need to care about game order. But if you are using custom order, you can use "-" symbol to add separators between games:

    roms/filename1.nes
    roms/filename2.nes
    - | SOME TITLE
    roms/filename3.nes
    roms/filename4.nes
    
You can disable sorting and enable custom order using NOSORT=1 option when running Make. Or just add "!NOSORT" line to a game list file.

Check [configs/games.list](configs/games.list) for example.

### How to use Make
Just run:

`make <targets> [options]`

Possible targets:
* **nes20** - build .nes file (NES 2.0)
* **unif** - build .unf file (UNIF)
* **bin** - build raw binary file, can be used with flash memory programmer
* **all** - build .nes, .unf and .bin files at once
* **clean** - remove all temporary and output files

Possible options:
* **GAMES** - use as `GAMES=games.list` to specify the file with game list, default is "games.list"
* **MENU_IMAGE** - use as `MENU_IMAGE=menu_header.png` to specify image for menu header, default is "menu_header.png"
* **LANGUAGE** - use as `LANGUAGE=eng` to specify loader messages (like some warnings) language - "eng" or "rus", default is "eng"
* **SIZE** - use as `SIZE=128` - maximum ROM size in megabytes (flash chip size), builder will throw error in case of ROM overflow, default is 128
* **MAXCHRSIZE** - use as `MAXCHRSIZE=256` - maximum CHR size in kilobytes (CHR RAM chip size), builder will throw error in case if there is game with more CHR size, default is 256
* **OUTPUT_NES20** - use as `OUTPUT_NES20=output.nes` - output .nes file for **nes20** target
* **OUTPUT_UNIF** - use as `OUTPUT_UNIF=output.unf` - output .unf file for **unif** target
* **OUTPUT_BIN** - use as `OUTPUT_BIN=output.bin` - output .bin file for **bin** target
* **CONFIGS_DIR** - use as `CONFIGS_DIR=configs` - directory with game list files, default is "configs"
* **ENABLE_LAST_GAME_SAVING**  - use as `ENABLE_LAST_GAME_SAVING=1` - remember last played game, works only with `ENABLE_SAVES=1` and self-writable flash memory, default is `ENABLE_LAST_GAME_SAVING=1`
* **NOSORT** - use as `NOSORT=1` - disable automatic alphabetically game sorting, default is `NOSORT=0`
* **BADSECTORS** - use as `BADSECTORS=0,5,10` - specify list of bad sectors if you need to write cartridge with bad flash memory, default is none
* **REPORT** - use as `REPORT=report.txt` - specify file for human-readable build report, default is none
* **ENABLE_SOUND** - use as `ENABLE_SOUND=1` - enable or disable sound in the loader menu, default is `ENABLE_SOUND=1`
* **STARS** - use as `STARS=30` - amount of background stars in the loader menu, maximum is `STARS=62`, default is `STARS=30`
* **STARS_DIRECTION** - use as `STARS_DIRECTION=1` - direction of background stars in the loader menu, `STARS_DIRECTION=0` - down to up, `STARS_DIRECTION=1` - up to down, default is up to down
* **STAR_SPAWN_INTERVAL** - use as `STAR_SPAWN_INTERVAL=90` - spawn interval of background stars in the loader menu, default is `STAR_SPAWN_INTERVAL=90`
* **ENABLE_RIGHT_CURSOR** - use as `ENABLE_RIGHT_CURSOR=1` - show or hide right cursor in the loader menu, default is `ENABLE_RIGHT_CURSOR=1`
* **ENABLE_DIM_IN** - use as `ENABLE_DIM_IN=1` - enable dim-in (on startup, etc), default is `ENABLE_DIM_IN=1`
* **DIM_IN_DELAY** - use as `DIM_IN_DELAY=5` - dim-in speed (more - slower), default is `DIM_IN_DELAY=5`
* **ENABLE_DIM_OUT** - use as `ENABLE_DIM_OUT=1` - enable dim-out (before game launch, etc), default is `ENABLE_DIM_OUT=1`
* **DIM_OUT_DELAY** - use as `DIM_OUT_DELAY=1` - dim-out speed (more - slower), default is `DIM_OUT_DELAY=1`

#### Examples
Change header image:

`make nes GAMES=games.list ENABLE_SAVES=1 MENU_HEADER=menu_example.png`

Save output ROM as UNIF file:

`make unif GAMES=games.list OUTPUT_UNIF=output.unf`

## Games compatibility
Games compatibility depends on game's mapper. Supported mapper list is not constant and depends on a [cartridge firmware](https://github.com/ClusterM/coolgirl-famicom-multicart). There is "coolgirl-mappers.json" file which contains register values for supported mappers.

## In-depth info - how it works
First method:
1. Convert images to a NES assets using the [NesTiler](https://github.com/ClusterM/NesTiler) tool.
2. Run **coolgirl-combiner** with "**prepare**" option, it will automatically use the best way to fit games data in the target ROM and create "**games.asm**" file and offsets file. First one contains game names and register values for game loader menu. Second file contains info with addresses of data for every game in the final ROM.
3. Compile assembly files using [nesasm CE](https://github.com/ClusterM/nesasm). It will create .nes file with the loader menu. But it will not contain games data. You can tune loader menu using command line options and defines: header image, background stars behavior, sounds, etc.
4. Combine loader menu and games into one file (.nes, .unf or .bin) using **coolgirl-combiner** with "**combine**" option and offsets file generated by step 2. Done.

Alternative method (easier and faster):
1. Convert images to a NES assets using the [NesTiler](https://github.com/ClusterM/NesTiler) tool.
2. Run **coolgirl-combiner** with "**build**" option, it will automaticaly fit games, compile assembly files using [nesasm CE](https://github.com/ClusterM/nesasm) and combine everything into one file (.nes, .unf or .bin).

## Download
You can always download the latest version at [https://github.com/ClusterM/coolgirl-multirom-builder/releases](https://github.com/ClusterM/coolgirl-multirom-builder/releases).

Also, you can download automatic nightly builds: [http://clusterm.github.io/coolgirl-multirom-builder/](http://clusterm.github.io/coolgirl-multirom-builder/).

## Donate
https://www.donationalerts.com/r/clustermeerkat

https://boosty.to/cluster

