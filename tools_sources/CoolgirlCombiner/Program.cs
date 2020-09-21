﻿using com.clusterrr.Famicom;
using com.clusterrr.Famicom.Containers;
using com.clusterrr.Famicom.Containers.HeaderFixer;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.XPath;

namespace Cluster.Famicom
{
    class Program
    {
        static int Main(string[] args)
        {
            var mappers = new Dictionary<string, MapperInfo>();
            mappers["0"] = new MapperInfo { MapperReg = 0x00, PrgMode = 0, ChrMode = 0, WramEnabled = false }; // NROM
            mappers["2"] = new MapperInfo { MapperReg = 0x01, PrgMode = 0, ChrMode = 0, WramEnabled = false }; // UxROM
            mappers["71"] = new MapperInfo { MapperReg = 0x01, PrgMode = 0, ChrMode = 0, WramEnabled = false, MapperFlags = 1 }; // Codemasters
            mappers["30"] = new MapperInfo { MapperReg = 0x01, PrgMode = 0, ChrMode = 0, WramEnabled = false, MapperFlags = 2 }; // UNROM512
            mappers["3"] = new MapperInfo { MapperReg = 0x02, PrgMode = 0, ChrMode = 0, WramEnabled = false }; // CNROM
            mappers["78"] = new MapperInfo { MapperReg = 0x03, PrgMode = 0, ChrMode = 0, WramEnabled = false }; // Holy Diver
            mappers["97"] = new MapperInfo { MapperReg = 0x04, PrgMode = 1, ChrMode = 0, WramEnabled = false }; // Irem's TAM-S1
            mappers["93"] = new MapperInfo { MapperReg = 0x05, PrgMode = 0, ChrMode = 0, WramEnabled = false }; // Sunsoft-2
            mappers["163"] = new MapperInfo { MapperReg = 0x06, PrgMode = 7, ChrMode = 0, WramEnabled = true }; // Mapper 163 (Final Fantasy VII and some other weird games)
            mappers["18"] = new MapperInfo { MapperReg = 0x07, PrgMode = 4, ChrMode = 7, WramEnabled = false }; // Jaleco SS88006
            mappers["7"] = new MapperInfo { MapperReg = 0x08, PrgMode = 7, ChrMode = 0, WramEnabled = false }; // AxROM
            mappers["34"] = new MapperInfo { MapperReg = 0x08, PrgMode = 7, ChrMode = 0, WramEnabled = false, MapperFlags = 1 }; // BxROM (but not NINA-001!)
            mappers["241"] = new MapperInfo { MapperReg = 0x08, PrgMode = 7, ChrMode = 0, WramEnabled = true, MapperFlags = 1 }; // BxROM with PRG RAM
            mappers["228"] = new MapperInfo { MapperReg = 0x09, PrgMode = 7, ChrMode = 0, WramEnabled = false }; // Cheetahmen 2
            mappers["11"] = new MapperInfo { MapperReg = 0x0A, PrgMode = 7, ChrMode = 0, WramEnabled = false }; // Color Dreams
            mappers["66"] = new MapperInfo { MapperReg = 0x0B, PrgMode = 7, ChrMode = 0, WramEnabled = false }; // GxROM
            mappers["87"] = new MapperInfo { MapperReg = 0x0C, PrgMode = 0, ChrMode = 0, WramEnabled = false }; // Mapper #87
            mappers["90"] = new MapperInfo { MapperReg = 0x0D, PrgMode = 4, ChrMode = 7, WramEnabled = false }; // Mapper #90
            mappers["65"] = new MapperInfo { MapperReg = 0x0E, PrgMode = 4, ChrMode = 7, WramEnabled = false }; // Mapper #65 - Irem's H3001
            mappers["5"] = new MapperInfo { MapperReg = 0x0F, PrgMode = 4, ChrMode = 7, WramEnabled = true }; // MMC5
            mappers["1"] = new MapperInfo { MapperReg = 0x10, PrgMode = 0, ChrMode = 0, WramEnabled = true }; // MMC1
            mappers["9"] = new MapperInfo { MapperReg = 0x11, PrgMode = 4, ChrMode = 5, WramEnabled = true }; // MMC2
            mappers["10"] = new MapperInfo { MapperReg = 0x11, PrgMode = 0, ChrMode = 5, WramEnabled = true, MapperFlags = 1 }; // MMC4
            mappers["152"] = new MapperInfo { MapperReg = 0x12, PrgMode = 0, ChrMode = 0, WramEnabled = false }; // Mapper #152
            mappers["73"] = new MapperInfo { MapperReg = 0x13, PrgMode = 0, ChrMode = 0, WramEnabled = true }; // VRC3
            mappers["4"] = new MapperInfo { MapperReg = 0x14, PrgMode = 4, ChrMode = 2, WramEnabled = true }; // MMC3
            mappers["118"] = new MapperInfo { MapperReg = 0x14, PrgMode = 4, ChrMode = 2, WramEnabled = true, MapperFlags = 1 }; // TxSROM (MMC3 with flag)
            mappers["189"] = new MapperInfo { MapperReg = 0x14, PrgMode = 7, ChrMode = 2, WramEnabled = false, MapperFlags = 2 }; // Mapper #189
            mappers["206"] = new MapperInfo { MapperReg = 0x14, PrgMode = 4, ChrMode = 2, WramEnabled = false, MapperFlags = 4 }; // Mapper #206
            mappers["112"] = new MapperInfo { MapperReg = 0x15, PrgMode = 4, ChrMode = 2, WramEnabled = true }; // Mapper #112
            mappers["33"] = new MapperInfo { MapperReg = 0x16, PrgMode = 4, ChrMode = 2, WramEnabled = true }; // Taito
            mappers["48"] = new MapperInfo { MapperReg = 0x16, PrgMode = 4, ChrMode = 2, WramEnabled = true, MapperFlags = 1 }; // Taito
            mappers["42"] = new MapperInfo { MapperReg = 0x17, PrgMode = 7, ChrMode = 0, WramEnabled = false, PrgBankA = 0xFF };
            mappers["21"] = new MapperInfo { MapperReg = 0x18, PrgMode = 4, ChrMode = 7, WramEnabled = true, MapperFlags = 1 }; // VRC4a
            mappers["22"] = new MapperInfo { MapperReg = 0x18, PrgMode = 4, ChrMode = 7, WramEnabled = true, MapperFlags = 1 | 2 }; // VRC2a
            mappers["23"] = new MapperInfo { MapperReg = 0x18, PrgMode = 4, ChrMode = 7, WramEnabled = true }; // VRC2b
            mappers["25"] = new MapperInfo { MapperReg = 0x18, PrgMode = 4, ChrMode = 7, WramEnabled = true, MapperFlags = 1 }; // VRC2c, VRC4
            mappers["69"] = new MapperInfo { MapperReg = 0x19, PrgMode = 4, ChrMode = 7, WramEnabled = true }; // Sunsoft FME-7
            mappers["32"] = new MapperInfo { MapperReg = 0x1A, PrgMode = 4, ChrMode = 7, WramEnabled = true }; // Irem's G-101
            mappers["113"] = new MapperInfo { MapperReg = 0x1B, PrgMode = 7, ChrMode = 0, WramEnabled = false }; // NINA-03/06
            mappers["133"] = new MapperInfo { MapperReg = 0x1C, PrgMode = 7, ChrMode = 0, WramEnabled = false }; // Sachen, 72-pin version only
            mappers["36"] = new MapperInfo { MapperReg = 0x1D, PrgMode = 7, ChrMode = 0, WramEnabled = false }; // Mapper #36 is assigned to TXC's PCB 01-22000-400
            mappers["70"] = new MapperInfo { MapperReg = 0x1E, PrgMode = 0, ChrMode = 0, WramEnabled = false }; // Mapper #70
            mappers["184"] = new MapperInfo { MapperReg = 0x1F, PrgMode = 0, ChrMode = 4, WramEnabled = false }; // Mapper #184
            mappers["38"] = new MapperInfo { MapperReg = 0x20, PrgMode = 7, ChrMode = 0, WramEnabled = false }; // Mapper #38
            mappers["AC08"] = new MapperInfo { MapperReg = 0x21, PrgMode = 7, ChrMode = 0, WramEnabled = false, PrgBankA = 8 }; // Mapper AC08
            Console.WriteLine("COOLGIRL UNIF combiner");
            Console.WriteLine("(c) Cluster, 2020");
            Console.WriteLine("http://clusterrr.com");
            Console.WriteLine("clusterrr@clusterrr.com");
            Console.WriteLine();
            bool needShowHelp = false;

            string command = null;
            string optionGames = null;
            string optionAsm = null;
            string optionOffsets = null;
            string optionReport = null;
            string optionLoader = null;
            string optionUnif = null;
            string optionBin = null;
            string optionLanguage = "eng";
            var badSectors = new List<int>();
            bool optionNoSort = false;
            int optionMaxSize = 256;
            if (args.Length > 0) command = args[0].ToLower();
            if (command != "prepare" && command != "combine")
            {
                if (!string.IsNullOrEmpty(command))
                    Console.WriteLine("Unknown command: " + command);
                needShowHelp = true;
            }
            for (int i = 1; i < args.Length; i++)
            {
                string param = args[i];
                while (param.StartsWith("-")) param = param.Substring(1);
                string value = i < args.Length - 1 ? args[i + 1] : "";
                switch (param.ToLower())
                {
                    case "games":
                        optionGames = value;
                        i++;
                        break;
                    case "asm":
                        optionAsm = value;
                        i++;
                        break;
                    case "offsets":
                        optionOffsets = value;
                        i++;
                        break;
                    case "report":
                        optionReport = value;
                        i++;
                        break;
                    case "loader":
                        optionLoader = value;
                        i++;
                        break;
                    case "unif":
                        optionUnif = value;
                        i++;
                        break;
                    case "bin":
                        optionBin = value;
                        i++;
                        break;
                    case "nosort":
                        optionNoSort = true;
                        break;
                    case "maxsize":
                        optionMaxSize = int.Parse(value);
                        i++;
                        break;
                    case "language":
                        optionLanguage = value.ToLower();
                        i++;
                        break;
                    case "badsectors":
                        foreach (var v in value.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                            badSectors.Add(int.Parse(v));
                        i++;
                        break;
                    default:
                        Console.WriteLine("Unknown parameter: " + param);
                        needShowHelp = true;
                        break;
                }
            }

            if (command == "prepare")
            {
                if (optionGames == null)
                {
                    Console.WriteLine("Missing required parameter: --games");
                    needShowHelp = true;
                }
                if (optionAsm == null)
                {
                    Console.WriteLine("Missing required parameter: --asm");
                    needShowHelp = true;
                }
                if (optionOffsets == null)
                {
                    Console.WriteLine("Missing required parameter: --offsets");
                    needShowHelp = true;
                }
            }
            else if (command == "combine")
            {
                if (optionLoader == null)
                {
                    Console.WriteLine("Missing required parameter: --loader");
                    needShowHelp = true;
                }
                if (optionUnif == null && optionBin == null)
                {
                    Console.WriteLine("At least one parameter required: --unif or --bin");
                    needShowHelp = true;
                }
            }

            if (needShowHelp)
            {
                Console.WriteLine("");
                Console.WriteLine("--- Usage ---");
                Console.WriteLine("First step:");
                Console.WriteLine(" CoolgirlCombiner.exe prepare --games <games.txt> --asm <games.asm> --offsets <offsets.xml> [--report <report.txt>] [--nosort] [--maxsize sizemb] [--language <language>] [--badsectors <sectors>]");
                Console.WriteLine("  {0,-20}{1}", "--games", "- input plain text file with list of ROM files");
                Console.WriteLine("  {0,-20}{1}", "--asm", "- output file for loader");
                Console.WriteLine("  {0,-20}{1}", "--offsets", "- output file with offsets for every game");
                Console.WriteLine("  {0,-20}{1}", "--report", "- output report file (human readable)");
                Console.WriteLine("  {0,-20}{1}", "--nosort", "- disable automatic sort by name");
                Console.WriteLine("  {0,-20}{1}", "--maxsize", "- maximum size for final file (in megabytes)");
                Console.WriteLine("  {0,-20}{1}", "--language", "- language for system messages: \"eng\" or \"rus\"");
                Console.WriteLine("  {0,-20}{1}", "--badsectors", "- comma-separated separated list of bad sectors,");
                Console.WriteLine("Second step:");
                Console.WriteLine(" CoolgirlCombiner.exe combine --loader <menu.nes> --offsets <offsets.xml> [--unif <multirom.unf>] [--bin <multirom.bin>]");
                Console.WriteLine("  {0,-20}{1}", "--loader", "- loader (compiled using asm file generated by first step)");
                Console.WriteLine("  {0,-20}{1}", "--offsets", "- input file with offsets for every game (generated by first step)");
                Console.WriteLine("  {0,-20}{1}", "--unif", "- output UNIF file");
                Console.WriteLine("  {0,-20}{1}", "--bin", "- output raw binary file");
                return 1;
            }


            try
            {
                if (command == "prepare")
                {
                    var lines = File.ReadAllLines(optionGames);
                    var result = new byte?[128 * 1024];
                    var regs = new Dictionary<string, List<String>>();
                    var games = new List<Game>();
                    var namesIncluded = new List<String>();

                    // Reserved for loader
                    for (int a = 0; a < 128 * 1024; a++)
                        result[a] = 0xff;

                    // Bad sectors :(
                    foreach (var bad in badSectors)
                    {
                        for (int a = bad * 4 * 0x8000; a < bad * 4 * 0x8000 + 128 * 1024; a++)
                        {
                            if (a >= result.Length)
                                Array.Resize(ref result, a + 16 * 1024 * 1024);
                            result[a] = 0xff;
                        }
                    }

                    // Building list of ROMs
                    foreach (var line in lines)
                    {
                        if (string.IsNullOrEmpty(line.Trim())) continue;
                        if (line.StartsWith(";")) continue;
                        int sepPos = line.TrimEnd().IndexOf('|');

                        string fileName;
                        string menuName = null;
                        if (sepPos < 0)
                        {
                            fileName = line.Trim();
                        }
                        else
                        {
                            fileName = line.Substring(0, sepPos).Trim();
                            menuName = line.Substring(sepPos + 1).Trim();
                        }

                        if (fileName.EndsWith("/") || fileName.EndsWith("\\"))
                        {
                            Console.WriteLine("Loading directory: {0}", fileName);
                            var files = Enumerable.Concat(Enumerable.Concat(Directory.GetFiles(fileName, "*.nes"), Directory.GetFiles(fileName, "*.unf")), Directory.GetFiles(fileName, "*.unif"));
                            foreach (var file in files)
                            {
                                Game.LoadGames(games, file);
                            }
                        }
                        else
                        {
                            Game.LoadGames(games, fileName, menuName);
                        }
                    }

                    // Removing separators
                    if (!optionNoSort)
                        games = new List<Game>((from game in games where !(string.IsNullOrEmpty(game.FileName) || game.FileName == "-") select game).ToArray());
                    // Sorting
                    if (optionNoSort)
                    {
                        games = new List<Game>((from game in games where !game.ToString().StartsWith("?") select game)
                        .Union(from game in games where game.ToString().StartsWith("?") orderby game.ToString().ToUpper() ascending select game)
                        .ToArray());
                    }
                    else
                    {
                        games = new List<Game>((from game in games where !game.ToString().StartsWith("?") orderby game.ToString().ToUpper() ascending select game)
                            .Union(from game in games where game.ToString().StartsWith("?") orderby game.ToString().ToUpper() ascending select game)
                            .ToArray());
                    }
                    int hiddenCount = games.Where(game => game.ToString().StartsWith("?")).Count();

                    byte saveId = 0;
                    foreach (var game in games)
                    {
                        if (game.Battery)
                        {
                            saveId++;
                            game.SaveId = saveId;
                        }
                    }

                    int usedSpace = 0;
                    var sortedPrgs = from game in games orderby game.PrgSize descending select game;
                    foreach (var game in sortedPrgs)
                    {
                        int prgRoundSize = 1;
                        while (prgRoundSize < game.PrgSize) prgRoundSize *= 2;
                        var prg = game.PRG;

                        Console.WriteLine("Fitting PRG for {0} ({1}kbytes)...", game, prgRoundSize / 1024);
                        for (int pos = 0; pos < optionMaxSize * 1024 * 1024; pos += prgRoundSize)
                        {
                            if (WillFit(result, pos, prgRoundSize, prg))
                            {
                                game.PrgPos = pos;
                                for (var i = 0; i < prg.Length; i++)
                                {
                                    if (pos + i >= result.Length)
                                        Array.Resize(ref result, pos + i + 16 * 1024 * 1024);
                                    result[pos + i] = prg[i];
                                }
                                usedSpace = Math.Max(usedSpace, pos + prg.Length);
                                Console.WriteLine("Address: {0:X8}", pos);
                                break;
                            }
                        }
                        if (game.PrgPos < 0) throw new Exception("Can't fit " + game);
                        GC.Collect();
                    }

                    var sortedChrs = from game in games orderby game.ChrSize descending select game;
                    foreach (var game in sortedChrs)
                    {
                        if (game.ChrSize == 0) continue;
                        var chr = game.CHR;

                        Console.WriteLine("Fitting CHR for {0} ({1}kbytes)...", game, game.ChrSize / 1024);
                        for (int pos = 0; pos < optionMaxSize * 1024 * 1024; pos += 0x2000)
                        {
                            if (WillFit(result, pos, game.ChrSize, chr))
                            {
                                game.ChrPos = pos;
                                for (var i = 0; i < chr.Length; i++)
                                {
                                    if (pos + i >= result.Length)
                                        Array.Resize(ref result, pos + i + 16 * 1024 * 1024);
                                    result[pos + i] = chr[i];
                                }
                                usedSpace = Math.Max(usedSpace, pos + chr.Length);
                                Console.WriteLine("Address: {0:X8}", pos);
                                break;
                            }
                        }
                        if (game.ChrPos < 0) throw new Exception("Can't fit " + game.FileName);
                        GC.Collect();
                    }
                    while (usedSpace % 0x8000 != 0)
                        usedSpace++;
                    int romSize = usedSpace;
                    usedSpace += 128 * 1024 * (int)Math.Ceiling(saveId / 4.0);

                    int totalSize = 0;
                    int maxChrSize = 0;
                    namesIncluded.Add(string.Format("{0,-33} {1,-10} {2,-10} {3,-10} {4,0}", "Game name", "Mapper", "Save ID", "Size", "Total size"));
                    namesIncluded.Add(string.Format("{0,-33} {1,-10} {2,-10} {3,-10} {4,0}", "------------", "-------", "-------", "-------", "--------------"));
                    var mapperStats = new Dictionary<string, int>();
                    foreach (var game in games)
                    {
                        if (!game.ToString().StartsWith("?"))
                        {
                            totalSize += game.PrgSize;
                            totalSize += game.ChrSize;
                            namesIncluded.Add(string.Format("{0,-33} {1,-10} {2,-10} {3,-10} {4,0}", FirstCharToUpper(game.ToString().Replace("_", " ").Replace("+", "")), game.Mapper, game.SaveId == 0 ? "-" : game.SaveId.ToString(),
                                ((game.PrgSize + game.ChrSize) / 1024) + " KB", (totalSize / 1024) + " KB total"));
                            if (!string.IsNullOrEmpty(game.Mapper))
                            {
                                if (!mapperStats.ContainsKey(game.Mapper)) mapperStats[game.Mapper] = 0;
                                mapperStats[game.Mapper]++;
                            }
                        }
                        if (game.ChrSize > maxChrSize)
                            maxChrSize = game.ChrSize;
                    }
                    namesIncluded.Add("");
                    namesIncluded.Add(string.Format("{0,-10} {1,0}", "Mapper", "Count"));
                    namesIncluded.Add(string.Format("{0,-10} {1,0}", "------", "-----"));
                    foreach (var mapper in from m in mapperStats.Keys orderby m ascending select m)
                    {
                        namesIncluded.Add(string.Format("{0,-10} {1,0}", mapper, mapperStats[mapper]));
                    }

                    namesIncluded.Add("");
                    namesIncluded.Add("Total games: " + (games.Count - hiddenCount));
                    namesIncluded.Add("Final ROM size: " + Math.Round(usedSpace / 1024.0 / 1024.0, 3) + "MB");
                    namesIncluded.Add("Maximum CHR size: " + maxChrSize / 1024 + "KB");
                    namesIncluded.Add("Battery-backed games: " + saveId);

                    Console.WriteLine("Total games: " + (games.Count - hiddenCount));
                    Console.WriteLine("Final ROM size: " + Math.Round(usedSpace / 1024.0 / 1024.0, 3) + "MB");
                    Console.WriteLine("Maximum CHR size: " + maxChrSize / 1024 + "KB");
                    Console.WriteLine("Battery-backed games: " + saveId);

                    if (optionReport != null)
                        File.WriteAllLines(optionReport, namesIncluded.ToArray());

                    if (games.Count - hiddenCount == 0)
                        throw new Exception("Games list is empty");

                    if (usedSpace > optionMaxSize * 1024 * 1024)
                        throw new Exception(string.Format("ROM is too big: {0} MB", Math.Round(usedSpace / 1024.0 / 1024.0, 3)));
                    if (games.Count > 768)
                        throw new Exception("Too many ROMs: " + games.Count);
                    if (saveId > 128)
                        throw new Exception("Too many battery backed games: " + saveId);

                    regs["reg_0"] = new List<String>();
                    regs["reg_1"] = new List<String>();
                    regs["reg_2"] = new List<String>();
                    regs["reg_3"] = new List<String>();
                    regs["reg_4"] = new List<String>();
                    regs["reg_5"] = new List<String>();
                    regs["reg_6"] = new List<String>();
                    regs["reg_7"] = new List<String>();
                    regs["chr_start_bank_h"] = new List<String>();
                    regs["chr_start_bank_l"] = new List<String>();
                    regs["chr_start_bank_s"] = new List<String>();
                    regs["chr_count"] = new List<String>();
                    regs["game_save"] = new List<String>();
                    regs["game_type"] = new List<String>();
                    regs["cursor_pos"] = new List<String>();

                    int c = 0;
                    foreach (var game in games)
                    {
                        int prgPos = game.PrgPos;
                        int chrPos = Math.Max(game.ChrPos, 0);
                        int chrBase = (chrPos / 0x2000) >> 4;
                        int prgBase = (prgPos / 0x2000) >> 4;
                        int prgRoundSize = 1;
                        while (prgRoundSize < game.PrgSize) prgRoundSize *= 2;
                        int chrRoundSize;
                        if (game.ChrSize > 0)
                        {
                            chrRoundSize = 1;
                            while (chrRoundSize < game.ChrSize || chrRoundSize < 0x2000) chrRoundSize *= 2;
                        } else
                        {
                            chrRoundSize = 0;
                        }

                        MapperInfo mapperInfo;
                        if (!string.IsNullOrEmpty(game.Mapper))
                        {
                            if (!mappers.TryGetValue(game.Mapper, out mapperInfo))
                                throw new Exception(string.Format("Unknowm mapper #{0} for {1} ", game.Mapper, game.FileName));
                        }
                        else mapperInfo = new MapperInfo();
                        if (game.ChrSize > 256 * 1024)
                            throw new Exception(string.Format("CHR is too big in {0} ", game.FileName));
                        if (game.Mirroring == NesFile.MirroringType.FourScreenVram && game.ChrSize > 256 * 1024 - 0x1000)
                            throw new Exception(string.Format("Four-screen and such big CHR is not supported for {0} ", game.FileName));

                        // Some unusual games
                        if (game.Mapper == "1") // MMC1 ?
                        {
                            switch (game.CRC32)
                            {
                                case 0xc6182024:	// Romance of the 3 Kingdoms
                                case 0x2225c20f:	// Genghis Khan
                                case 0x4642dda6:	// Nobunaga's Ambition
                                case 0x29449ba9:	// ""        "" (J)
                                case 0x2b11e0b0:	// ""        "" (J)
                                case 0xb8747abf:	// Best Play Pro Yakyuu Special (J)
                                case 0xc9556b36:	// Final Fantasy I & II (J) [!]
                                    Console.WriteLine("WARNING! {0} uses 16KB of WRAM", game.FileName);
                                    mapperInfo.MapperFlags |= 1; // flag to support 16KB of WRAM
                                    break;
                            }
                        }
                        if (game.Mapper == "4") // MMC3 ?
                        {
                            switch (game.CRC32)
                            {
                                case 0x93991433:	// Low-G-Man
                                case 0xaf65aa84:	// Low-G-Man
                                    Console.WriteLine("WARNING! WRAM will be disabled for {0}", Path.GetFileName(game.FileName));
                                    mapperInfo.WramEnabled = false; // disable WRAM
                                    break;
                            }
                        }
                        switch (game.CRC32)
                        {
                            case 0x78b657ac: // "Armadillo (J) [!].nes"
                            case 0xb3d92e78: // "Armadillo (J) [T+Eng1.01_Vice Translations].nes" 
                            case 0x0fe6e6a5: // "Armadillo (J) [T+Rus1.00 Chief-Net (23.05.2012)].nes" 
                            case 0xe62e3382: // "MiG 29 - Soviet Fighter (Camerica) [!].nes" 
                            case 0x1bc686a8: // "Fire Hawk (Camerica) [!].nes" 
                                Console.WriteLine("WARNING! {0} is not compatible with Dendy", Path.GetFileName(game.FileName));
                                game.Flags |= GameFlags.WillNotWorkOnDendy;
                                break;
                        }
                        if (string.IsNullOrEmpty(game.ToString()))
                            game.Flags = GameFlags.Separator;

                        uint prgMask = (uint)~(prgRoundSize / 0x4000 - 1);
                        uint chrMask = (uint)~(chrRoundSize / 0x2000 - 1);

                        byte @params = 0;
                        if (mapperInfo.WramEnabled) @params |= 1; // enable SRAM
                        if (game.ChrSize == 0) @params |= 2; // enable CHR write
                        if (game.Mirroring == NesFile.MirroringType.Horizontal) @params |= 8; // default mirroring
                        if (game.Mirroring == NesFile.MirroringType.FourScreenVram)
                        {
                            @params |= 32; // four screen
                            game.Flags |= GameFlags.WillNotWorkOnNewDendy; // not external NTRAM on new famiclones :(
                        }
                        @params |= 0x80; // lockout

                        regs["reg_0"].Add(string.Format("${0:X2}", ((prgPos / 0x4000) >> 8) & 0xFF));                                               // none[7:5], prg_base[26:22]
                        regs["reg_1"].Add(string.Format("${0:X2}", (prgPos / 0x4000) & 0xFF));                                                      // prg_base[21:14]
                        regs["reg_2"].Add(string.Format("${0:X2}", ((chrMask & 0x20) << 2) | (prgMask & 0x7F)));                                   // chr_mask[18], prg_mask[20:14]
                        regs["reg_3"].Add(string.Format("${0:X2}", (mapperInfo.PrgMode << 5) | 0));                                                 // prg_mode[2:0], chr_bank_a[7:3]
                        regs["reg_4"].Add(string.Format("${0:X2}", (byte)(mapperInfo.ChrMode << 5) | (chrMask & 0x1F)));                                  // chr_mode[2:0], chr_mask[17:13]
                        regs["reg_5"].Add(string.Format("${0:X2}", (((mapperInfo.PrgBankA & 0x1F) << 2) | (game.Battery ? 0x02 : 0x01)) & 0xFF));   // chr_bank[8], prg_bank_a[5:1], sram_page[1:0]
                        regs["reg_6"].Add(string.Format("${0:X2}", (mapperInfo.MapperFlags << 5) | (mapperInfo.MapperReg & 0x1F)));                 // flag[2:0], mapper[4:0]
                        regs["reg_7"].Add(string.Format("${0:X2}", @params | ((mapperInfo.MapperReg & 0x20) << 1)));                                // lockout, mapper[5], four_screen, mirroring[1:0], prg_write_on, chr_write_en, sram_enabled
                        regs["chr_start_bank_h"].Add(string.Format("${0:X2}", ((chrPos / 0x8000) >> 7) & 0xFF));
                        regs["chr_start_bank_l"].Add(string.Format("${0:X2}", ((chrPos / 0x8000) << 1) & 0xFF));
                        regs["chr_start_bank_s"].Add(string.Format("${0:X2}", ((chrPos % 0x8000) >> 8) | 0x80));
                        regs["chr_count"].Add(string.Format("${0:X2}", game.ChrSize / 0x2000));
                        regs["game_save"].Add(string.Format("${0:X2}", !game.Battery ? 0 : game.SaveId));
                        regs["game_type"].Add(string.Format("${0:X2}", (byte)game.Flags));
                        regs["cursor_pos"].Add(string.Format("${0:X2}", game.ToString().Length /*+ (++c).ToString().Length*/));
                    }

                    byte baseBank = 0;
                    var asmResult = new StringBuilder();
                    asmResult.AppendLine("; Games database");
                    int regCount = 0;
                    foreach (var reg in regs.Keys)
                    {
                        c = 0;
                        foreach (var r in regs[reg])
                        {
                            if (c % 256 == 0)
                            {
                                asmResult.AppendLine();
                                asmResult.AppendLine("  .bank " + (baseBank + c / 256 * 2));
                                asmResult.AppendLine(string.Format("  .org ${0:X4}", 0x8000 + regCount * 0x100));
                                asmResult.Append("loader_data_" + reg + (c == 0 ? "" : "_" + c.ToString()) + ":");
                            }
                            //asmResult.AppendLine("  .db " + string.Join(", ", regs[reg]));
                            if (c % 16 == 0)
                            {
                                asmResult.AppendLine();
                                asmResult.Append("  .db");
                            }
                            asmResult.AppendFormat(((c % 16 != 0) ? "," : "") + " {0}", r);
                            c++;
                        }
                        asmResult.AppendLine();
                        regCount++;
                    }

                    asmResult.AppendLine();
                    asmResult.AppendLine();
                    //asmResult.Append("  .dw");
                    c = 0;
                    foreach (var game in games)
                    {
                        if (c % 256 == 0)
                        {
                            asmResult.AppendLine();
                            asmResult.AppendLine("  .bank " + (baseBank + c / 256 * 2));
                            asmResult.AppendLine("  .org $9000");
                            asmResult.AppendLine("game_names_list" + (c == 0 ? "" : "_" + c.ToString()) + ":");
                            asmResult.AppendLine("  .dw game_names" + (c == 0 ? "" : "_" + c.ToString()));
                            asmResult.AppendLine("game_names" + (c == 0 ? "" : "_" + c.ToString()) + ":");
                        }
                        //asmResult.AppendFormat(((c > 0) ? "," : "") + " game_name_" + c);
                        asmResult.AppendLine("  .dw game_name_" + c);
                        c++;
                    }

                    asmResult.AppendLine();
                    asmResult.AppendLine();
                    asmResult.AppendLine("; Game names");
                    c = 0;
                    foreach (var game in games)
                    {
                        asmResult.AppendLine();
                        if (c % 256 == 0)
                        {
                            asmResult.AppendLine("  .bank " + (baseBank + c / 256 * 2 + 1));
                            if (baseBank + c / 256 * 2 + 1 >= 62) throw new Exception("Bank overflow! Too many games?");
                            asmResult.AppendLine("  .org $A000");
                        }
                        asmResult.AppendLine("; " + game.ToString());
                        asmResult.AppendLine("game_name_" + c + ":");
                        var name = StringToTiles(string.Format(/*"{0}. "+*/"{1}", c + 1, game.ToString()));
                        var asm = BytesToAsm(name);
                        asmResult.Append(asm);
                        c++;
                    }

                    asmResult.AppendLine();
                    asmResult.AppendLine("  .bank 14");
                    asmResult.AppendLine("  .org $C800");
                    asmResult.AppendLine();
                    asmResult.AppendLine("games_count:");
                    asmResult.AppendLine("  .dw " + (games.Count - hiddenCount));
                    asmResult.AppendLine();
                    asmResult.AppendLine();
                    asmResult.AppendLine("games_offset:");
                    asmResult.AppendLine("  .db " + ((games.Count - hiddenCount) > 10 ? 0 : 5 - (games.Count - hiddenCount) / 2));
                    asmResult.AppendLine();
                    asmResult.AppendLine();
                    asmResult.AppendLine("maximum_scroll:");
                    asmResult.AppendLine("  .dw " + Math.Max(0, games.Count - 11 - hiddenCount));
                    asmResult.AppendLine();
                    asmResult.AppendLine();
                    asmResult.AppendLine("string_file:");
                    asmResult.Append(BytesToAsm(StringToTiles("FILE: " + Path.GetFileName(optionGames))));
                    asmResult.AppendLine("string_build_date:");
                    asmResult.Append(BytesToAsm(StringToTiles("BUILD DATE: " + DateTime.Now.ToString("yyyy-MM-dd"))));
                    asmResult.AppendLine("string_build_time:");
                    asmResult.Append(BytesToAsm(StringToTiles("BUILD TIME: " + DateTime.Now.ToString("HH:mm:ss"))));
                    asmResult.AppendLine("string_console_type:");
                    asmResult.Append(BytesToAsm(StringToTiles("CONSOLE TYPE:")));
                    asmResult.AppendLine("string_ntsc:");
                    asmResult.Append(BytesToAsm(StringToTiles("NTSC")));
                    asmResult.AppendLine("string_pal:");
                    asmResult.Append(BytesToAsm(StringToTiles("PAL")));
                    asmResult.AppendLine("string_dendy:");
                    asmResult.Append(BytesToAsm(StringToTiles("DENDY")));
                    asmResult.AppendLine("string_new:");
                    asmResult.Append(BytesToAsm(StringToTiles("NEW")));
                    asmResult.AppendLine("string_flash:");
                    asmResult.Append(BytesToAsm(StringToTiles("FLASH:")));
                    asmResult.AppendLine("string_read_only:");
                    asmResult.Append(BytesToAsm(StringToTiles("READ ONLY")));
                    asmResult.AppendLine("string_writable:");
                    asmResult.Append(BytesToAsm(StringToTiles("WRITABLE")));
                    asmResult.AppendLine("flash_sizes:");
                    for (int i = 0; i <= 8; i++)
                        asmResult.AppendLine($"  .dw string_{1 << i}mb");
                    for (int i = 0; i <= 8; i++)
                    {
                        asmResult.AppendLine($"string_{1 << i}mb:");
                        asmResult.Append(BytesToAsm(StringToTiles($"{1 << i}MB")));
                    }
                    asmResult.AppendLine("string_chr_ram:");
                    asmResult.Append(BytesToAsm(StringToTiles("CHR RAM:")));
                    asmResult.AppendLine("chr_ram_sizes:");
                    for (int i = 0; i <= 8; i++)
                        asmResult.AppendLine($"  .dw string_{8 * (1 << i)}kb");
                    for (int i = 0; i <= 8; i++)
                    {
                        asmResult.AppendLine($"string_{8 * (1 << i)}kb:");
                        asmResult.Append(BytesToAsm(StringToTiles($"{8 * (1 << i)}KB")));
                    }
                    asmResult.AppendLine("string_prg_ram:");
                    asmResult.Append(BytesToAsm(StringToTiles("PRG RAM:")));
                    asmResult.AppendLine("string_present:");
                    asmResult.Append(BytesToAsm(StringToTiles("PRESENT")));
                    asmResult.AppendLine("string_not_available:");
                    asmResult.Append(BytesToAsm(StringToTiles("NOT AVAILABLE")));
                    asmResult.AppendLine("string_saving:");
                    if (optionLanguage == "rus")
                        asmResult.Append(BytesToAsm(StringToTiles("  СОХРАНЯЕМСЯ... НЕ ВЫКЛЮЧАЙ!   ")));
                    else
                        asmResult.Append(BytesToAsm(StringToTiles("   SAVING... DON'T TURN OFF!    ")));
                    File.WriteAllText(optionAsm, asmResult.ToString());
                    asmResult.AppendLine("string_incompatible_console:");
                    if (optionLanguage == "rus")
                        asmResult.Append(BytesToAsm(StringToTiles("     ИЗВИНИТЕ,  ДАННАЯ ИГРА       НЕСОВМЕСТИМА С ЭТОЙ КОНСОЛЬЮ                                        НАЖМИТЕ ЛЮБУЮ КНОПКУ      ")));
                    else
                        asmResult.Append(BytesToAsm(StringToTiles("    SORRY,  THIS GAME IS NOT      COMPATIBLE WITH THIS CONSOLE                                          PRESS ANY BUTTON        ")));
                    asmResult.AppendLine("string_prg_ram_test:");
                    asmResult.Append(BytesToAsm(StringToTiles("PRG RAM TEST:")));
                    asmResult.AppendLine("string_chr_ram_test:");
                    asmResult.Append(BytesToAsm(StringToTiles("CHR RAM TEST:")));
                    asmResult.AppendLine("string_passed:");
                    asmResult.Append(BytesToAsm(StringToTiles("PASSED")));
                    asmResult.AppendLine("string_failed:");
                    asmResult.Append(BytesToAsm(StringToTiles("FAILED")));
                    asmResult.AppendLine("string_ok:");
                    asmResult.Append(BytesToAsm(StringToTiles("OK")));
                    asmResult.AppendLine("string_error:");
                    asmResult.Append(BytesToAsm(StringToTiles("ERROR")));

                    asmResult.AppendLine();
                    asmResult.AppendLine();
                    asmResult.AppendLine("SECRETS .equ " + hiddenCount);
                    File.WriteAllText(optionAsm, asmResult.ToString());

                    XmlWriterSettings xmlSettings = new XmlWriterSettings();
                    xmlSettings.Indent = true;
                    StreamWriter str = new StreamWriter(optionOffsets);
                    XmlWriter offsetsXml = XmlWriter.Create(str, xmlSettings);
                    offsetsXml.WriteStartDocument();
                    offsetsXml.WriteStartElement("Offsets");

                    offsetsXml.WriteStartElement("Info");
                    offsetsXml.WriteElementString("Size", romSize.ToString());
                    offsetsXml.WriteElementString("RomCount", games.Count.ToString());
                    offsetsXml.WriteElementString("GamesFile", Path.GetFileName(optionGames));
                    offsetsXml.WriteEndElement();

                    offsetsXml.WriteStartElement("ROMs");
                    foreach (var game in games)
                    {
                        if (game.FileName == "-") continue;
                        offsetsXml.WriteStartElement("ROM");
                        offsetsXml.WriteElementString("FileName", game.FileName);
                        offsetsXml.WriteElementString("ContainerType", game.ContainerType.ToString());
                        if (!game.ToString().StartsWith("?"))
                            offsetsXml.WriteElementString("MenuName", game.ToString());
                        offsetsXml.WriteElementString("PrgOffset", string.Format("{0:X8}", game.PrgPos));
                        if (game.ChrSize > 0)
                            offsetsXml.WriteElementString("ChrOffset", string.Format("{0:X8}", game.ChrPos));
                        offsetsXml.WriteElementString("Mapper", game.Mapper?.ToString());
                        if (game.SaveId > 0)
                            offsetsXml.WriteElementString("SaveId", game.SaveId.ToString());
                        offsetsXml.WriteEndElement();
                    }
                    offsetsXml.WriteEndElement();

                    offsetsXml.WriteEndElement();
                    offsetsXml.Close();
                }
                else
                {
                    using (var xmlFile = File.OpenRead(optionOffsets))
                    {
                        var offsetsXml = new XPathDocument(xmlFile);
                        XPathNavigator offsetsNavigator = offsetsXml.CreateNavigator();
                        XPathNodeIterator offsetsIterator = offsetsNavigator.Select("/Offsets/Info/Size");
                        int size = -1;
                        while (offsetsIterator.MoveNext())
                        {
                            size = offsetsIterator.Current.ValueAsInt;
                        }
                        //size = 64 * 1024 * 1024;

                        if (size < 0) throw new Exception("Invalid offsets file");
                        var result = new byte[size];

                        Console.Write("Loading loader... ");
                        var loaderFile = new NesFile(optionLoader);
                        Array.Copy(loaderFile.PRG, 0, result, 0, loaderFile.PRG.Length);
                        Console.WriteLine("OK.");

                        offsetsIterator = offsetsNavigator.Select("/Offsets/ROMs/ROM");
                        while (offsetsIterator.MoveNext())
                        {
                            var currentRom = offsetsIterator.Current;
                            string filename = null;
                            Game.NesContainerType containerType = Game.NesContainerType.iNES;
                            int prgOffset = -1;
                            int chrOffset = -1;
                            var descs = currentRom.SelectDescendants(XPathNodeType.Element, false);
                            while (descs.MoveNext())
                            {
                                var param = descs.Current;
                                switch (param.Name.ToLower())
                                {
                                    case "filename":
                                        filename = param.Value;
                                        break;
                                    case "containertype":
                                        if (param.Value.ToLower() == "unif")
                                            containerType = Game.NesContainerType.UNIF;
                                        break;
                                    case "prgoffset":
                                        prgOffset = int.Parse(param.Value, System.Globalization.NumberStyles.HexNumber);
                                        break;
                                    case "chroffset":
                                        chrOffset = int.Parse(param.Value, System.Globalization.NumberStyles.HexNumber);
                                        break;
                                }
                            }

                            if (!string.IsNullOrEmpty(filename))
                            {
                                Console.Write("Loading {0}... ", filename);
                                switch (containerType)
                                {
                                    case Game.NesContainerType.iNES:
                                        {
                                            var nesFile = new NesFile(filename);
                                            if (prgOffset >= 0)
                                                Array.Copy(nesFile.PRG, 0, result, prgOffset, nesFile.PRG.Length);
                                            if (chrOffset >= 0)
                                                Array.Copy(nesFile.CHR, 0, result, chrOffset, nesFile.CHR.Length);
                                        }
                                        break;
                                    case Game.NesContainerType.UNIF:
                                        {
                                            var unifFile = new UnifFile(filename);
                                            var prg = unifFile.Fields.Where(k => k.Key.StartsWith("PRG")).OrderBy(k => k.Key).SelectMany(i => i.Value).ToArray();
                                            var chr = unifFile.Fields.Where(k => k.Key.StartsWith("CHR")).OrderBy(k => k.Key).SelectMany(i => i.Value).ToArray();
                                            if (prgOffset >= 0)
                                                Array.Copy(prg, 0, result, prgOffset, prg.Length);
                                            if (chrOffset >= 0)
                                                Array.Copy(chr, 0, result, chrOffset, chr.Length);
                                        }
                                        break;
                                }
                                Console.WriteLine("OK.");
                            }
                            GC.Collect();
                        }

                        if (!string.IsNullOrEmpty(optionUnif))
                        {
                            var u = new UnifFile();
                            u.Mapper = "COOLGIRL";
                            u.Fields["MIRR"] = new byte[] { 5 };
                            u.Fields["PRG0"] = result;
                            u.Fields["BATR"] = new byte[] { 1 };
                            u.Version = 5;
                            u.Save(optionUnif);

                            uint sizeFixed = 1;
                            while (sizeFixed < result.Length) sizeFixed <<= 1;
                            var resultSizeFixed = new byte[sizeFixed];
                            Array.Copy(result, 0, resultSizeFixed, 0, result.Length);
                            for (int i = result.Length; i < sizeFixed; i++)
                                resultSizeFixed[i] = 0xFF;
                            var md5 = System.Security.Cryptography.MD5.Create();
                            var md5hash = md5.ComputeHash(resultSizeFixed);
                            Console.Write("ROM MD5: ");
                            foreach (var b in md5hash)
                                Console.Write("{0:x2}", b);
                            Console.WriteLine();
                        }
                        if (!string.IsNullOrEmpty(optionBin))
                        {
                            File.WriteAllBytes(optionBin, result);
                        }
                    }
                }
                Console.WriteLine("Done.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message + ex.StackTrace);
                return 2;
            }
            return 0;
        }

        static bool WillFit(byte?[] dest, int pos, int size, byte[] source)
        {
            for (int addr = pos; addr < pos + size; addr++)
            {
                if (addr >= dest.Length) return true;
                if (dest[addr] != null && ((addr - pos >= source.Length) || dest[addr] != source[addr - pos]))
                    return false;
            }
            return true;
        }

        static byte[] StringToTiles(string text)
        {
            //text = text.ToUpper();
            var result = new byte[text.Length + 1];
            for (int c = 0; c < result.Length; c++)
            {

                if (c < text.Length)
                {
                    //int charCode = Encoding.GetEncoding(1251).GetBytes(text[c].ToString())[0]; // =Oo=
                    if (text[c] >= 'A' && text[c] <= 'Z')
                        result[c] = (byte)(text[c] - 'A' + 0x01);
                    else if (text[c] >= 'a' && text[c] <= 'z')
                        result[c] = (byte)(text[c].ToString().ToUpper()[0] - 'A' + 0x01);
                    else if (text[c] >= '1' && text[c] <= '9')
                        result[c] = (byte)(text[c] - '1' + 0x1B);
                    else if (text[c] >= 'А' && text[c] <= 'Я')
                        result[c] = (byte)(text[c] - 'А' + 0x31);
                    else if (text[c] >= 'а' && text[c] <= 'я')
                        result[c] = (byte)(text[c] - 'а' + 0x51);
                    else
                        switch (text[c])
                        {
                            case '0':
                                result[c] = 0x0F;
                                break;
                            case '.':
                                result[c] = 0x24;
                                break;
                            case ',':
                                result[c] = 0x25;
                                break;
                            case '?':
                                result[c] = 0x26;
                                break;
                            case ':':
                                result[c] = 0x27;
                                break;
                            case '-':
                                result[c] = 0x28;
                                break;
                            case '&':
                                result[c] = 0x29;
                                break;
                            case '!':
                                result[c] = 0x2A;
                                break;
                            case '(':
                                result[c] = 0x2B;
                                break;
                            case ')':
                                result[c] = 0x2C;
                                break;
                            case '\'':
                                result[c] = 0x2D;
                                break;
                            case '#':
                                result[c] = 0x2E;
                                break;
                            case '_':
                                result[c] = 0x2F;
                                break;
                            default:
                                result[c] = 0x30;
                                break;
                        }
                    result[c] += 0x80;
                }
            }
            return result;
        }

        static string BytesToAsm(byte[] name)
        {
            var asmResult = new StringBuilder();
            for (int ch = 0; ch < name.Length; ch++)
            {
                if (ch % 15 == 0)
                {
                    if (ch > 0) asmResult.AppendLine();
                    asmResult.Append("  .db");
                }
                asmResult.AppendFormat(((ch % 15 > 0) ? "," : "") + " ${0:X2}", name[ch]);
            }
            asmResult.AppendLine();
            return asmResult.ToString();
        }
        static string FirstCharToUpper(string input)
        {
            if (String.IsNullOrEmpty(input)) return "";
            return input.First().ToString().ToUpper() + input.Substring(1);
        }

        struct MapperInfo
        {
            public byte MapperReg;
            public byte MapperFlags;
            public byte PrgMode;
            public byte ChrMode;
            public bool WramEnabled;
            public byte PrgBankA;
        }

        enum GameFlags
        {
            Separator = 0x80,
            WillNotWorkOnNtsc = 0x01,
            WillNotWorkOnPal = 0x02,
            WillNotWorkOnDendy = 0x04,
            WillNotWorkOnNewDendy = 0x08
        };

        class Game
        {
            public enum NesContainerType { iNES, UNIF };
            public string FileName;
            public string MenuName;
            public readonly byte[] PRG = new byte[0];
            public int PrgSize { get => PRG.Length; }
            public readonly byte[] CHR = new byte[0];
            public int ChrSize { get => CHR.Length; }
            public int PrgPos;
            public int ChrPos;
            public byte SaveId;
            public GameFlags Flags;
            public bool Battery;
            public string Mapper;
            public NesFile.MirroringType Mirroring;
            public uint CRC32;
            public NesContainerType ContainerType;

            public static void LoadGames(List<Game> games, string fileName, string menuName = null)
            {
                var game = new Game(fileName, menuName);
                games.Add(game);
                GC.Collect();
            }

            public Game(string fileName, string menuName = null)
            {
                PrgPos = -1;
                ChrPos = -1;
                // Separators
                if (fileName == "-")
                {
                    MenuName = "";
                    FileName = "";
                    Flags |= GameFlags.Separator;
                }
                else
                {
                    Console.WriteLine("Loading {0}...", Path.GetFileName(fileName));
                    FileName = fileName;
                    try
                    {
                        var nesFile = new NesFile(fileName);
                        var fix = nesFile.CorrectRom();
                        if (fix != 0)
                            Console.WriteLine(" Invalid header. Fix: " + fix);
                        PRG = nesFile.PRG;
                        CHR = nesFile.CHR;
                        Battery = nesFile.Battery;
                        Mapper = $"{nesFile.Mapper}";
                        if (nesFile.Submapper > 0)
                            Mapper += $":{nesFile.Submapper}";

                        Mirroring = nesFile.Mirroring;
                        CRC32 = nesFile.CalculateCRC32();
                        ContainerType = NesContainerType.iNES;
                        if (nesFile.Trainer != null && nesFile.Trainer.Length > 0)
                            throw new NotImplementedException(string.Format("{0} - trained games are not supported yet", Path.GetFileName(fileName)));
                    }
                    catch (InvalidDataException)
                    {
                        var unifFile = new UnifFile(fileName);
                        PRG = unifFile.Fields.Where(k => k.Key.StartsWith("PRG")).OrderBy(k => k.Key).SelectMany(i => i.Value).ToArray();
                        CHR = unifFile.Fields.Where(k => k.Key.StartsWith("CHR")).OrderBy(k => k.Key).SelectMany(i => i.Value).ToArray();
                        Battery = unifFile.Fields.ContainsKey("BATR") && unifFile.Fields["BATR"].Length > 0 && unifFile.Fields["BATR"][0] != 0;
                        Mapper = unifFile.Mapper;
                        if (Mapper.StartsWith("NES-") || Mapper.StartsWith("UNL-") || Mapper.StartsWith("HVC-") || Mapper.StartsWith("BTL-") || Mapper.StartsWith("BMC-"))
                            Mapper = Mapper.Substring(4);
                        Mirroring = unifFile.Mirroring;
                        ContainerType = NesContainerType.UNIF;
                    }
                }
                MenuName = menuName;
            }

            /*
            public NesFile ROM
            {
                get
                {
                    if (string.IsNullOrEmpty(FileName))
                    {
                        var nesFile = new NesFile();
                        nesFile.PRG = new byte[0];
                        nesFile.CHR = new byte[0];
                        return nesFile;
                    }
                    else
                    {
                        var nesFile = new NesFile(FileName);
                        var fix = nesFile.CorrectRom();
                        return nesFile;
                    }
                }
            }
            */

            public override string ToString()
            {
                string name;
                if (MenuName != null)
                {
                    if (MenuName.StartsWith("+"))
                        name = MenuName.Substring(1).Trim();
                    else
                        name = MenuName.Trim();
                }
                else
                {
                    name = Regex.Replace(Regex.Replace(Path.GetFileNameWithoutExtension(FileName), @" ?\(.{1,3}[0-9]?\)", string.Empty), @" ?\[.*?\]", string.Empty).Trim().Replace("_", " ").Replace(", The", "");
                }
                name = name.Trim();
                if (name.Length > 28) name = name.Substring(0, 25).Trim() + "...";
                return name.Trim();
            }
        }
    }
}
