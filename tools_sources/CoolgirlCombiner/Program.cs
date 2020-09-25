﻿using com.clusterrr.Famicom;
using com.clusterrr.Famicom.Containers;
using com.clusterrr.Famicom.Containers.HeaderFixer;
using Newtonsoft.Json;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.XPath;

namespace com.clusterrr.Famicom.CoolGirl
{
    partial class Program
    {
        static int Main(string[] args)
        {
            //mappers["0"] = new Mapper { MapperRegister = 0x00, PrgMode = 0, ChrMode = 0, PrgRamEnabled = false, Description = "NROM" };
            //mappers["2"] = new Mapper { MapperRegister = 0x01, PrgMode = 0, ChrMode = 0, PrgRamEnabled = false, Description = "UxROM"};
            //mappers["71"] = new Mapper { MapperRegister = 0x01, PrgMode = 0, ChrMode = 0, PrgRamEnabled = false, Flags = 1, Description = "Camerica" };
            //mappers["30"] = new Mapper { MapperRegister = 0x01, PrgMode = 0, ChrMode = 0, PrgRamEnabled = false, Flags = 2, ChrRamBanking = true, Description = "UNROM512" };
            //mappers["3"] = new Mapper { MapperRegister = 0x02, PrgMode = 0, ChrMode = 0, PrgRamEnabled = false, Description = "CNROM" }; 
            //mappers["78"] = new Mapper { MapperRegister = 0x03, PrgMode = 0, ChrMode = 0, PrgRamEnabled = false, Description = "Irem: Holy Diver and Uchuusen - Cosmo Carrier" };
            //mappers["97"] = new Mapper { MapperRegister = 0x04, PrgMode = 1, ChrMode = 0, PrgRamEnabled = false, Description = "Irem's TAM-S1: only Kaiketsu Yanchamaru" };
            //mappers["93"] = new Mapper { MapperRegister = 0x05, PrgMode = 0, ChrMode = 0, PrgRamEnabled = false, Description  = "Sunsoft-2: Shanghai, Fantasy Zone" }; 
            //mappers["163"] = new Mapper { MapperRegister = 0x06, PrgMode = 7, ChrMode = 0, PrgRamEnabled = true, Description = "Final Fantasy VII and some other weird games" };
            //mappers["18"] = new Mapper { MapperRegister = 0x07, PrgMode = 4, ChrMode = 7, PrgRamEnabled = false, Description = "Jaleco SS88006" };
            //mappers["7"] = new Mapper { MapperRegister = 0x08, PrgMode = 7, ChrMode = 0, PrgRamEnabled = false, Description = "AxROM" };
            //mappers["34"] = new Mapper { MapperRegister = 0x08, PrgMode = 7, ChrMode = 0, PrgRamEnabled = false, Flags = 1, Description = "BxROM (but not NINA-001!)" }; // 
            //mappers["241"] = new Mapper { MapperRegister = 0x08, PrgMode = 7, ChrMode = 0, PrgRamEnabled = true, Flags = 1, Description = "BxROM with PRG RAM" };
            //mappers["228"] = new Mapper { MapperRegister = 0x09, PrgMode = 7, ChrMode = 0, PrgRamEnabled = false, Description = "Cheetahmen 2" };
            //mappers["11"] = new Mapper { MapperRegister = 0x0A, PrgMode = 7, ChrMode = 0, PrgRamEnabled = false, Description = "Color Dreams" };
            //mappers["66"] = new Mapper { MapperRegister = 0x0B, PrgMode = 7, ChrMode = 0, PrgRamEnabled = false, Description = "GxROM" };
            //mappers["87"] = new Mapper { MapperRegister = 0x0C, PrgMode = 0, ChrMode = 0, PrgRamEnabled = false, Description = "Jaleco" };
            //mappers["90"] = new Mapper { MapperRegister = 0x0D, PrgMode = 4, ChrMode = 7, PrgRamEnabled = false, Description = "JY (partical support): Aladdin only" };
            //mappers["65"] = new Mapper { MapperRegister = 0x0E, PrgMode = 4, ChrMode = 7, PrgRamEnabled = false, Description = "Irem's H3001" };
            //mappers["5"] = new Mapper { MapperRegister = 0x0F, PrgMode = 4, ChrMode = 7, PrgRamEnabled = true, Description = "MMC5 (partical support): Castlevania 3 only" };
            //mappers["1"] = new Mapper { MapperRegister = 0x10, PrgMode = 0, ChrMode = 0, PrgRamEnabled = true, Flags16kPrgRam = 1, Description = "MMC1" };
            //mappers["9"] = new Mapper { MapperRegister = 0x11, PrgMode = 4, ChrMode = 5, PrgRamEnabled = true , Description = "MMC2"};
            //mappers["10"] = new Mapper { MapperRegister = 0x11, PrgMode = 0, ChrMode = 5, PrgRamEnabled = true, Flags = 1, Description = "MMC4" };
            //mappers["152"] = new Mapper { MapperRegister = 0x12, PrgMode = 0, ChrMode = 0, PrgRamEnabled = false, Description = "Bandai" };
            //mappers["73"] = new Mapper { MapperRegister = 0x13, PrgMode = 0, ChrMode = 0, PrgRamEnabled = true, Description = "VRC3" };
            //mappers["4"] = new Mapper { MapperRegister = 0x14, PrgMode = 4, ChrMode = 2, PrgRamEnabled = true, Description = "MMC3" };
            //mappers["118"] = new Mapper { MapperRegister = 0x14, PrgMode = 4, ChrMode = 2, PrgRamEnabled = true, Flags = 1, Description = "TxSROM (MMC3 with weird wiring)" };
            //mappers["189"] = new Mapper { MapperRegister = 0x14, PrgMode = 7, ChrMode = 2, PrgRamEnabled = false, Flags = 2, Description = "TXC" };
            //mappers["206"] = new Mapper { MapperRegister = 0x14, PrgMode = 4, ChrMode = 2, PrgRamEnabled = false, Flags = 4, Description = "The simpler predecessor of the MMC3" };
            //mappers["112"] = new Mapper { MapperRegister = 0x15, PrgMode = 4, ChrMode = 2, PrgRamEnabled = true, Description = "NTDEC" };
            //mappers["33"] = new Mapper { MapperRegister = 0x16, PrgMode = 4, ChrMode = 2, PrgRamEnabled = true, Description = "Taito" };
            //mappers["48"] = new Mapper { MapperRegister = 0x16, PrgMode = 4, ChrMode = 2, PrgRamEnabled = true, Flags = 1, Description = "Taito" };
            //mappers["42"] = new Mapper { MapperRegister = 0x17, PrgMode = 7, ChrMode = 0, PrgRamEnabled = false, PrgBankA = 0xFF, Description = "FDS conversions" };
            //mappers["21"] = new Mapper { MapperRegister = 0x18, PrgMode = 4, ChrMode = 7, PrgRamEnabled = true, Flags = 1, Description = "VRC4a" };
            //mappers["22"] = new Mapper { MapperRegister = 0x18, PrgMode = 4, ChrMode = 7, PrgRamEnabled = true, Flags = 1 | 2, Description = "VRC2a" };
            //mappers["23"] = new Mapper { MapperRegister = 0x18, PrgMode = 4, ChrMode = 7, PrgRamEnabled = true, Description = "VRC2b" };
            //mappers["25"] = new Mapper { MapperRegister = 0x18, PrgMode = 4, ChrMode = 7, PrgRamEnabled = true, Flags = 1, Description = "VRC2c, VRC4" };
            //mappers["69"] = new Mapper { MapperRegister = 0x19, PrgMode = 4, ChrMode = 7, PrgRamEnabled = true, Description = "Sunsoft FME-7" };
            //mappers["32"] = new Mapper { MapperRegister = 0x1A, PrgMode = 4, ChrMode = 7, PrgRamEnabled = true, Description = "Irem's G-101" };
            //mappers["113"] = new Mapper { MapperRegister = 0x1B, PrgMode = 7, ChrMode = 0, PrgRamEnabled = false, Description = "NINA-03/06" };
            //mappers["133"] = new Mapper { MapperRegister = 0x1C, PrgMode = 7, ChrMode = 0, PrgRamEnabled = false, Description = "Sachen" };
            //mappers["36"] = new Mapper { MapperRegister = 0x1D, PrgMode = 7, ChrMode = 0, PrgRamEnabled = false, Description = "TXC's PCB 01-22000-400" };
            //mappers["70"] = new Mapper { MapperRegister = 0x1E, PrgMode = 0, ChrMode = 0, PrgRamEnabled = false, Description = "Bandai: Family Trainer, Kamen Rider Club, Space Shadow" };
            //mappers["184"] = new Mapper { MapperRegister = 0x1F, PrgMode = 0, ChrMode = 4, PrgRamEnabled = false }; // Mapper #184
            //mappers["38"] = new Mapper { MapperRegister = 0x20, PrgMode = 7, ChrMode = 0, PrgRamEnabled = false, Description = "Crime Busters" }; // Mapper #38
            //mappers["AC08"] = new Mapper { MapperRegister = 0x21, PrgMode = 7, ChrMode = 0, PrgRamEnabled = false, PrgBankA = 8, Description = "Green Beret FDS conversion" }; // Mapper AC08
            //File.WriteAllText("mappers.json", JsonConvert.SerializeObject(mappers, Newtonsoft.Json.Formatting.Indented));
            //var fixes = new Dictionary<string,GameFix>()
            //{
            //    {"0xc6182024", new GameFix() { Description = "Romance of the 3 Kingdoms", PrgRamSize = 16 } },
            //    {"0xabbf7217", new GameFix() { Description = "??? (J) (PRG0)", PrgRamSize = 16 } },
            //    {"0xccf35c02", new GameFix() { Description = "??? (J) (PRG1)", PrgRamSize = 16 } },
            //    {"0x2225c20f", new GameFix() { Description = "Genghis Khan", PrgRamSize = 16 } },
            //    {"0xfb69743a", new GameFix() { Description = "??? (J)", PrgRamSize = 16 } },
            //    {"0x4642dda6", new GameFix() { Description = "Nobunaga's Ambition", PrgRamSize = 16 } },
            //    {"0x3f7ad415", new GameFix() { Description = "??? (J) (PRG0)", PrgRamSize = 16 } },
            //    {"0x2b11e0b0", new GameFix() { Description = "??? (J) (PRG1)", PrgRamSize = 16 } },

            //    {"0xb8747abf", new GameFix() { Description = "Best Play Pro Yakyuu Special (J) (PRG0)", PrgRamSize = 32 } },
            //    {"0xc3de7c69", new GameFix() { Description = "??? (J) (PRG1)", PrgRamSize = 32 } },
            //    {"0xc9556b36", new GameFix() { Description = "Final Fantasy I & II (J) [!]", PrgRamSize = 32 } },

            //    {"0x93991433", new GameFix() { Description = "Low-G-Man", PrgRamSize = 0 } },
            //    {"0xaf65aa84", new GameFix() { Description = "Low-G-Man", PrgRamSize = 0 } },

            //    {"0x78b657ac", new GameFix() { Description = "Armadillo (J) [!]", WillNotWorkOnDendy = true } },
            //    {"0xb3d92e78", new GameFix() { Description = "Armadillo (J) [T+Eng1.01_Vice Translations]", WillNotWorkOnDendy = true } },
            //    {"0x0fe6e6a5", new GameFix() { Description = "Armadillo (J) [T+Rus1.00 Chief-Net (23.05.2012)]", WillNotWorkOnDendy = true } },
            //    {"0xe62e3382", new GameFix() { Description = "MiG 29 - Soviet Fighter (Camerica) [!]", WillNotWorkOnDendy = true } },
            //    {"0x1bc686a8", new GameFix() { Description = "Fire Hawk (Camerica) [!]", WillNotWorkOnDendy = true } },
            //};
            //File.WriteAllText("fixes.json", JsonConvert.SerializeObject(fixes, Newtonsoft.Json.Formatting.Indented, new JsonSerializerSettings { DefaultValueHandling = DefaultValueHandling.Ignore }));

            Console.WriteLine("COOLGIRL UNIF combiner");
            Console.WriteLine("(c) Cluster, 2020");
            Console.WriteLine("http://clusterrr.com");
            Console.WriteLine("clusterrr@clusterrr.com");
            Console.WriteLine();
            bool needShowHelp = false;

            string command = null;
            string optionMappersFile = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"mappers.json");
            string optionFixesFile = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"fixes.json");
            string optionGamesFile = null;
            string optionAsmFile = null;
            string optionOffsetsFile = null;
            string optionReportFile = null;
            string optionLoaderFile = null;
            string optionUnifFile = null;
            string optionBinFile = null;
            string optionLanguage = "eng";
            var badSectors = new List<int>();
            bool optionNoSort = false;
            int optionMaxRomSize = 256;
            int optionMaxChrRamSize = 256;
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
                    case "mappers":
                        optionMappersFile = value;
                        i++;
                        break;
                    case "games":
                        optionGamesFile = value;
                        i++;
                        break;
                    case "asm":
                        optionAsmFile = value;
                        i++;
                        break;
                    case "offsets":
                        optionOffsetsFile = value;
                        i++;
                        break;
                    case "report":
                        optionReportFile = value;
                        i++;
                        break;
                    case "loader":
                        optionLoaderFile = value;
                        i++;
                        break;
                    case "unif":
                        optionUnifFile = value;
                        i++;
                        break;
                    case "bin":
                        optionBinFile = value;
                        i++;
                        break;
                    case "nosort":
                        optionNoSort = true;
                        break;
                    case "maxromsize":
                        optionMaxRomSize = int.Parse(value);
                        i++;
                        break;
                    case "maxchrsize":
                        optionMaxChrRamSize = int.Parse(value);
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
                if (optionGamesFile == null)
                {
                    Console.WriteLine("Missing required parameter: --games");
                    needShowHelp = true;
                }
                if (optionAsmFile == null)
                {
                    Console.WriteLine("Missing required parameter: --asm");
                    needShowHelp = true;
                }
                if (optionOffsetsFile == null)
                {
                    Console.WriteLine("Missing required parameter: --offsets");
                    needShowHelp = true;
                }
            }
            else if (command == "combine")
            {
                if (optionLoaderFile == null)
                {
                    Console.WriteLine("Missing required parameter: --loader");
                    needShowHelp = true;
                }
                if (optionUnifFile == null && optionBinFile == null)
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
                Console.WriteLine(" CoolgirlCombiner.exe prepare --games <games.txt> --asm <games.asm> --offsets <offsets.xml> [--report <report.txt>] [--nosort] [--maxromsize sizemb] [--language <language>] [--badsectors <sectors>]");
                Console.WriteLine("  {0,-20}{1}", "--games", "- input plain text file with list of ROM files");
                Console.WriteLine("  {0,-20}{1}", "--asm", "- output file for loader");
                Console.WriteLine("  {0,-20}{1}", "--offsets", "- output file with offsets for every game");
                Console.WriteLine("  {0,-20}{1}", "--report", "- output report file (human readable)");
                Console.WriteLine("  {0,-20}{1}", "--nosort", "- disable automatic sort by name");
                Console.WriteLine("  {0,-20}{1}", "--maxromsize", "- maximum size for final file (in megabytes)");
                Console.WriteLine("  {0,-20}{1}", "--maxchrsize", "- maximum CHR RAM size (in kilobytes), default is 256");
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
                    // Loading mappers file
                    var mappersJson = File.ReadAllText(optionMappersFile);
                    var mappers = JsonConvert.DeserializeObject<Dictionary<string, Mapper>>(mappersJson);
                    // Loading fixes file
                    Dictionary<uint, GameFix> fixes;
                    if (File.Exists(optionFixesFile))
                    {
                        var fixesJson = File.ReadAllText(optionFixesFile);
                        var fixesStr = JsonConvert.DeserializeObject<Dictionary<string, GameFix>>(fixesJson);
                        // Convert string CRC32 to uint
                        fixes = fixesStr.Select(kv =>
                        {
                            return new KeyValuePair<uint, GameFix>(
                                // Check for hexademical values
                                kv.Key.ToLower().StartsWith("0x")
                                    ? Convert.ToUInt32(kv.Key.Substring(2), 16)
                                    : uint.Parse(kv.Key),
                                kv.Value);
                        }).ToDictionary(kv => kv.Key, kv => kv.Value);
                    }
                    else
                    {
                        Console.WriteLine("WARNING! Fixes file not found, fixes database will not be used");
                        fixes = null;
                    }
                    // Loading games list
                    var lines = File.ReadAllLines(optionGamesFile);
                    var result = new byte?[128 * 1024];
                    var regs = new Dictionary<string, List<String>>();
                    var games = new List<Game>();
                    var report = new List<String>();

                    // Reserved for loader
                    for (int a = 0; a < 128 * 1024; a++)
                        result[a] = 0xFF;

                    // Bad sectors :(
                    foreach (var bad in badSectors)
                    {
                        for (int a = bad * 4 * 0x8000; a < bad * 4 * 0x8000 + 128 * 1024; a++)
                        {
                            if (a >= result.Length)
                                Array.Resize(ref result, a + 16 * 1024 * 1024);
                            result[a] = 0xFF;
                        }
                    }

                    // Building list of ROMs
                    foreach (var line in lines)
                    {
                        // Skip empty lines
                        if (string.IsNullOrWhiteSpace(line)) continue;
                        // Skip comments
                        if (line.StartsWith(";")) continue;
                        var cols = line.Split(new char[] { '|' }, 2, StringSplitOptions.RemoveEmptyEntries);
                        string fileName = cols[0].Trim();
                        string menuName = cols.Length >= 2 ? cols[1].Trim() : fileName;

                        // Is it a directory?
                        if (fileName.EndsWith("/") || fileName.EndsWith("\\"))
                        {
                            Console.WriteLine("Loading directory: {0}", fileName);
                            var files = Enumerable.Concat(Enumerable.Concat(Directory.GetFiles(fileName, "*.nes"), Directory.GetFiles(fileName, "*.unf")), Directory.GetFiles(fileName, "*.unif"));
                            foreach (var file in files)
                            {
                                games.Add(new Game(file, fixes: fixes));
                            }
                        }
                        else
                        {
                            // No, it's a file
                            games.Add(new Game(fileName, menuName, fixes: fixes));
                        }
                    }

                    // Sorting
                    IEnumerable<Game> sortedGames;
                    if (optionNoSort)
                    {
                        sortedGames =
                            Enumerable.Concat(
                                games.Where(g => !g.MenuName.StartsWith("?")),
                                games.Where(g => g.MenuName.StartsWith("?"))
                            );
                    }
                    else
                    {
                        // Removing separators
                        var gamesNoSeparators = games.Where(g => (g.Flags & Game.GameFlags.Separator) == 0);
                        sortedGames =
                            Enumerable.Concat(
                                gamesNoSeparators.Where(g => !g.MenuName.StartsWith("?")).OrderBy(g => g.MenuName),
                                gamesNoSeparators.Where(g => g.MenuName.StartsWith("?"))
                            );
                    }

                    int gamesCount = sortedGames.Count();
                    int hiddenCount = games.Where(g => g.ToString().StartsWith("?")).Count();
                    int menuItemsCount = gamesCount - hiddenCount;

                    byte saveId = 0;
                    foreach (var game in games)
                    {
                        if (game.Battery)
                        {
                            saveId++;
                            game.SaveId = saveId;
                        }
                    }

                    uint usedSpace = 0;
                    var sortedPrgs = games.OrderByDescending(g => g.PrgSize).Where(g => g.PrgSize > 0);
                    foreach (var game in sortedPrgs)
                    {
                        var prg = game.PRG.ToArray();

                        Console.Write($"Fitting PRG of {Path.GetFileName(game.FileName)} ({game.PrgSize}KB)... ");
                        bool fitted = false;
                        for (uint pos = 0; pos < optionMaxRomSize * 1024 * 1024; pos += game.PrgSize)
                        {
                            if (WillFit(result, pos, prg))
                            {
                                game.PrgOffset = pos;
                                for (var i = 0; i < prg.Length; i++)
                                {
                                    if (pos + i >= result.Length)
                                        Array.Resize(ref result, (int)(pos + i + 16 * 1024 * 1024));
                                    result[pos + i] = prg[i];
                                }
                                usedSpace = Math.Max(usedSpace, (uint)(pos + prg.Length));
                                fitted = true;
                                Console.WriteLine($"offset: {pos:X8}");
                                break;
                            }
                        }
                        if (!fitted) throw new OutOfMemoryException("Can't fit " + Path.GetFileName(game.FileName));
                        GC.Collect();
                    }

                    var sortedChrs = games.OrderByDescending(g => g.ChrSize).Where(g => g.ChrSize > 0);
                    foreach (var game in sortedChrs)
                    {
                        var chr = game.CHR.ToArray();

                        Console.Write($"Fitting PRG of {Path.GetFileName(game.FileName)} ({game.ChrSize}KB)... ");
                        bool fitted = false;
                        for (uint pos = 0; pos < optionMaxRomSize * 1024 * 1024; pos += 0x2000)
                        {
                            if (WillFit(result, pos, chr))
                            {
                                game.ChrOffset = pos;
                                for (var i = 0; i < chr.Length; i++)
                                {
                                    if (pos + i >= result.Length)
                                        Array.Resize(ref result, (int)(pos + i + 16 * 1024 * 1024));
                                    result[pos + i] = chr[i];
                                }
                                fitted = true;
                                usedSpace = Math.Max(usedSpace, (uint)(pos + chr.Length));
                                Console.WriteLine($"Address: {pos:X8}");
                                break;
                            }
                        }
                        if (!fitted) throw new OutOfMemoryException("Can't fit " + Path.GetFileName(game.FileName));
                        GC.Collect();
                    }

                    // Calculate output ROM size
                    while (usedSpace % 0x8000 != 0)
                        usedSpace++;
                    uint romSize = usedSpace;
                    usedSpace += (uint)(128 * 1024 * (int)Math.Ceiling(saveId / 4.0));

                    uint totalSize = 0;
                    uint maxChrSize = 0;
                    report.Add(string.Format("{0,-33} {1,-15} {2,-10} {3,-10} {4,0}", "Game name", "Mapper", "Save ID", "Size", "Total size"));
                    report.Add(string.Format("{0,-33} {1,-15} {2,-10} {3,-10} {4,0}", "------------", "-------", "-------", "-------", "--------------"));
                    var mapperStats = new Dictionary<string, int>();
                    foreach (var game in sortedGames)
                    {
                        if (!game.ToString().StartsWith("?"))
                        {
                            totalSize += game.PrgSize;
                            totalSize += game.ChrSize;
                            report.Add(string.Format("{0,-33} {1,-15} {2,-10} {3,-10} {4,0}", FirstCharToUpper(game.ToString().Replace("_", " ").Replace("+", "")), game.Mapper, game.SaveId == 0 ? "-" : game.SaveId.ToString(),
                                $"{(game.PrgSize + game.ChrSize) / 1024}KB", $"{totalSize / 1024}KB total"));
                            if (!string.IsNullOrEmpty(game.Mapper))
                            {
                                if (!mapperStats.ContainsKey(game.Mapper)) mapperStats[game.Mapper] = 0;
                                mapperStats[game.Mapper]++;
                            }
                        }
                        if (game.ChrSize > maxChrSize)
                            maxChrSize = game.ChrSize;
                    }
                    report.Add("");
                    report.Add(string.Format("{0,-15} {1,0}", "Mapper", "Count"));
                    report.Add(string.Format("{0,-15} {1,0}", "------", "-----"));
                    foreach (var mapper in from m in mapperStats.Keys orderby m ascending select m)
                    {
                        report.Add(string.Format("{0,-15} {1,0}", mapper, mapperStats[mapper]));
                    }
                    report.Add("");
                    report.Add($"Total games: {sortedGames.Count() - hiddenCount}");
                    report.Add($"Final ROM size: {Math.Round(usedSpace / 1024.0 / 1024.0, 3)}MB");
                    report.Add($"Maximum CHR size: {maxChrSize / 1024}KB");
                    report.Add($"Battery-backed games: {saveId}");

                    // Print some stats
                    Console.WriteLine($"Total games: {sortedGames.Count() - hiddenCount}");
                    Console.WriteLine($"Final ROM size: {Math.Round(usedSpace / 1024.0 / 1024.0, 3)}MB");
                    Console.WriteLine($"Maximum CHR size: {maxChrSize / 1024}KB");
                    Console.WriteLine($"Battery-backed games: {saveId}");

                    // Write report file if need
                    if (optionReportFile != null)
                        File.WriteAllLines(optionReportFile, report.ToArray());

                    if (games.Count - hiddenCount == 0)
                        throw new InvalidOperationException("Games list is empty");

                    if (usedSpace > optionMaxRomSize * 1024 * 1024) // This should not happen
                        throw new OutOfMemoryException($"ROM is too big: {Math.Round(usedSpace / 1024.0 / 1024.0, 3)}MB");
                    if (games.Count > 768)
                        throw new ArgumentOutOfRangeException("games", $"Too many ROMs: {games.Count}");
                    if (saveId > 128)
                        throw new ArgumentOutOfRangeException("saves", $"Too many battery backed games: {saveId}");

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
                    regs["game_flags"] = new List<String>();
                    regs["cursor_pos"] = new List<String>();

                    int c = 0;
                    foreach (var game in sortedGames)
                    {
                        Mapper mapperInfo;
                        if (!string.IsNullOrEmpty(game.Mapper))
                        {
                            if (!mappers.TryGetValue(game.Mapper, out mapperInfo))
                                throw new NotSupportedException($"Unknown mapper \"{game.Mapper}\" for {Path.GetFileName(game.FileName)}");
                        }
                        else mapperInfo = new Mapper();
                        if (game.ChrSize > optionMaxChrRamSize * 1024)
                            throw new Exception($"CHR is too big in {game.FileName}");
                        if (game.Mirroring == NesFile.MirroringType.FourScreenVram && game.ChrSize > 256 * 1024 - 0x1000)
                            throw new Exception($"Four-screen and such big CHR is not supported for {game.FileName}");
                        bool prgRamEnabled;
                        var mapperFlags = mapperInfo.Flags;

                        // Some unusual games
                        //if (game.Mapper == "1") // MMC1 ?
                        switch (game.PrgRamSize)
                        {
                            case null:
                                prgRamEnabled = mapperInfo.PrgRamEnabled;
                                // default value
                                break;
                            case 0:
                                prgRamEnabled = false;
                                break;
                            case 8:
                                prgRamEnabled = true;
                                break;
                            case 16:
                                prgRamEnabled = true;
                                mapperFlags |= mapperInfo.Flags16kPrgRam;
                                break;
                            case 32:
                                throw new NotImplementedException("32KB of PRG RAM is not supported yet");
                            default:
                                throw new NotImplementedException($"Weird PRG RAM value: {game.PrgRamSize}KB");
                        }
                        if ((game.Flags & Game.GameFlags.WillNotWorkOnDendy) != 0)
                            Console.WriteLine($"WARNING! {Path.GetFileName(game.FileName)} is not compatible with Dendy");
                        if ((game.Flags & Game.GameFlags.WillNotWorkOnNtsc) != 0)
                            Console.WriteLine($"WARNING! {Path.GetFileName(game.FileName)} is not compatible with NTSC consoles");
                        if ((game.Flags & Game.GameFlags.WillNotWorkOnPal) != 0)
                            Console.WriteLine($"WARNING! {Path.GetFileName(game.FileName)} is not compatible with PAL consoles");
                        if ((game.Flags & Game.GameFlags.WillNotWorkOnNewFamiclone) != 0)
                            Console.WriteLine($"WARNING! {Path.GetFileName(game.FileName)} is not compatible with new Famiclones");

                        uint chrBankingSize = game.ChrSize;
                        // if using CHR RAM...
                        if (chrBankingSize == 0)
                        {
                            if (!game.ChrRamSize.HasValue)
                            {
                                // CHR RAM size is unknown
                                // if CHR RAM banking is supported by mapper
                                // set maximum size
                                if (mapperInfo.ChrRamBanking)
                                    chrBankingSize = 512 * 1024;
                                else // else banking is disabled
                                    chrBankingSize = 0x2000;
                            }
                            else
                            {
                                // CHR RAM size is specified by NES 2.0 or fixes.json file
                                chrBankingSize = game.ChrRamSize.Value;
                            }
                        }
                        uint prgMask = ~(game.PrgSize / 0x4000 - 1);
                        uint chrMask = ~(chrBankingSize / 0x2000 - 1);

                        byte @params = 0;
                        if (prgRamEnabled) @params |= (1 << 0); // enable SRAM
                        if (game.ChrSize == 0) @params |= (1 << 1); // enable CHR write
                        if (game.Mirroring == NesFile.MirroringType.Horizontal) @params |= (1 << 3); // default mirroring
                        if (game.Mirroring == NesFile.MirroringType.FourScreenVram) @params |= (1 << 5); // four-screen mirroring
                        @params |= (1 << 7); // lockout

                        regs["reg_0"].Add(string.Format("${0:X2}", ((game.PrgOffset / 0x4000) >> 8) & 0xFF));                                       // none[7:5], prg_base[26:22]
                        regs["reg_1"].Add(string.Format("${0:X2}", (game.PrgOffset / 0x4000) & 0xFF));                                              // prg_base[21:14]
                        regs["reg_2"].Add(string.Format("${0:X2}", ((chrMask & 0x20) << 2) | (prgMask & 0x7F)));                                    // chr_mask[18], prg_mask[20:14]
                        regs["reg_3"].Add(string.Format("${0:X2}", (mapperInfo.PrgMode << 5) | 0));                                                 // prg_mode[2:0], chr_bank_a[7:3]
                        regs["reg_4"].Add(string.Format("${0:X2}", (byte)(mapperInfo.ChrMode << 5) | (chrMask & 0x1F)));                            // chr_mode[2:0], chr_mask[17:13]
                        regs["reg_5"].Add(string.Format("${0:X2}", (((mapperInfo.PrgBankA & 0x1F) << 2) | (game.Battery ? 0x02 : 0x01)) & 0xFF));   // chr_bank[8], prg_bank_a[5:1], sram_page[1:0]
                        regs["reg_6"].Add(string.Format("${0:X2}", (mapperInfo.Flags << 5) | (mapperInfo.MapperRegister & 0x1F)));                  // flag[2:0], mapper[4:0]
                        regs["reg_7"].Add(string.Format("${0:X2}", @params | ((mapperInfo.MapperRegister & 0x20) << 1)));                           // lockout, mapper[5], four_screen, mirroring[1:0], prg_write_on, chr_write_en, sram_enabled
                        regs["chr_start_bank_h"].Add(string.Format("${0:X2}", ((game.ChrOffset / 0x8000) >> 7) & 0xFF));
                        regs["chr_start_bank_l"].Add(string.Format("${0:X2}", ((game.ChrOffset / 0x8000) << 1) & 0xFF));
                        regs["chr_start_bank_s"].Add(string.Format("${0:X2}", ((game.ChrOffset % 0x8000) >> 8) | 0x80));
                        regs["chr_count"].Add(string.Format("${0:X2}", game.ChrSize / 0x2000));
                        regs["game_save"].Add(string.Format("${0:X2}", !game.Battery ? 0 : game.SaveId));
                        regs["game_flags"].Add(string.Format("${0:X2}", (byte)game.Flags));
                        regs["cursor_pos"].Add(string.Format("${0:X2}", game.ToString().Length /*+ (++c).ToString().Length*/));
                    }

                    // It's time to generate assembly file
                    const byte baseBank = 0;
                    var asmResult = new StringBuilder();
                    asmResult.AppendLine("; Games database");
                    asmResult.AppendLine();
                    asmResult.AppendLine("; Number of secret ROMs");
                    asmResult.AppendLine($"SECRETS .equ {hiddenCount}");
                    asmResult.AppendLine();
                    asmResult.AppendLine();
                    asmResult.AppendLine("; Registers to start games");
                    int regCount = 0;
                    foreach (var reg in regs.Keys)
                    {
                        c = 0;
                        foreach (var r in regs[reg])
                        {
                            if (c % 256 == 0)
                            {
                                asmResult.AppendLine();
                                asmResult.AppendLine($"  .bank {baseBank + c / 256 * 2}");
                                asmResult.AppendLine($"  .org ${0x8000 + regCount * 0x100:X4}");
                                asmResult.Append($"loader_data_{reg}{(c == 0 ? "" : $"_{c}")}:");
                            }
                            if (c % 16 == 0)
                            {
                                asmResult.AppendLine();
                                asmResult.Append("  .db");
                            }
                            asmResult.Append(((c % 16 != 0) ? ", " : " ") + r);
                            c++;
                        }
                        asmResult.AppendLine();
                        regCount++;
                    }

                    asmResult.AppendLine();
                    asmResult.AppendLine();
                    asmResult.AppendLine("; Game names");
                    asmResult.AppendLine();
                    c = 0;
                    foreach (var game in sortedGames)
                    {
                        if (c % 256 == 0)
                        {
                            asmResult.AppendLine();
                            asmResult.AppendLine($"  .bank {baseBank + c / 256 * 2}");
                            asmResult.AppendLine($"  .org $9000");
                            asmResult.AppendLine($"game_names_list{(c == 0 ? "" : $"_{c}")}:");
                            asmResult.AppendLine($"game_names{(c == 0 ? "" : $"_{c}")}:");
                        }
                        asmResult.AppendLine($"  .dw game_name_{c}");
                        c++;
                    }

                    c = 0;
                    foreach (var game in sortedGames)
                    {
                        asmResult.AppendLine();
                        if (c % 256 == 0)
                        {
                            asmResult.AppendLine();
                            asmResult.AppendLine("  .bank " + (baseBank + c / 256 * 2 + 1));
                            if (baseBank + c / 256 * 2 + 1 >= 62) throw new Exception("Bank overflow! Too many games?");
                            asmResult.AppendLine("  .org $A000");
                        }
                        asmResult.AppendLine("; " + game.ToString());
                        asmResult.AppendLine("game_name_" + c + ":");
                        var name = StringToTiles(game.MenuName);
                        var asm = BytesToAsm(name);
                        asmResult.Append(asm);
                        c++;
                    }

                    // Common values
                    asmResult.AppendLine();
                    asmResult.AppendLine();
                    asmResult.AppendLine("; Common values");
                    asmResult.AppendLine();
                    asmResult.AppendLine("  .bank 14");
                    asmResult.AppendLine("  .org $C800");
                    asmResult.AppendLine();
                    asmResult.AppendLine("games_count:");
                    asmResult.AppendLine($"  .dw {menuItemsCount}");
                    asmResult.AppendLine();
                    asmResult.AppendLine();
                    asmResult.AppendLine("games_offset:");
                    asmResult.AppendLine($"  .db {(menuItemsCount > 10 ? 0 : 5 - menuItemsCount / 2)}");
                    asmResult.AppendLine();
                    asmResult.AppendLine();
                    asmResult.AppendLine("maximum_scroll:");
                    asmResult.AppendLine($"  .dw {Math.Max(0, menuItemsCount - 11)}");

                    // Some strings
                    asmResult.AppendLine();
                    asmResult.AppendLine();
                    asmResult.AppendLine("; Some strings");
                    asmResult.AppendLine();
                    asmResult.AppendLine("string_file:");
                    asmResult.Append(BytesToAsm(StringToTiles("FILE: " + Path.GetFileName(optionGamesFile))));
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
                    File.WriteAllText(optionAsmFile, asmResult.ToString());
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

                    File.WriteAllText(optionAsmFile, asmResult.ToString());

                    XmlWriterSettings xmlSettings = new XmlWriterSettings();
                    xmlSettings.Indent = true;
                    StreamWriter str = new StreamWriter(optionOffsetsFile);
                    XmlWriter offsetsXml = XmlWriter.Create(str, xmlSettings);
                    offsetsXml.WriteStartDocument();
                    offsetsXml.WriteStartElement("Offsets");

                    offsetsXml.WriteStartElement("Info");
                    offsetsXml.WriteElementString("Size", romSize.ToString());
                    offsetsXml.WriteElementString("RomCount", games.Count.ToString());
                    offsetsXml.WriteElementString("GamesFile", Path.GetFileName(optionGamesFile));
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
                        offsetsXml.WriteElementString("PrgOffset", string.Format("{0:X8}", game.PrgOffset));
                        if (game.ChrSize > 0)
                            offsetsXml.WriteElementString("ChrOffset", string.Format("{0:X8}", game.ChrOffset));
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
                    using (var xmlFile = File.OpenRead(optionOffsetsFile))
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
                        var loaderFile = new NesFile(optionLoaderFile);
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

                        if (!string.IsNullOrEmpty(optionUnifFile))
                        {
                            var u = new UnifFile();
                            u.Mapper = "COOLGIRL";
                            u.Fields["MIRR"] = new byte[] { 5 };
                            u.Fields["PRG0"] = result;
                            u.Fields["BATR"] = new byte[] { 1 };
                            u.Version = 5;
                            u.Save(optionUnifFile);

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
                        if (!string.IsNullOrEmpty(optionBinFile))
                        {
                            File.WriteAllBytes(optionBinFile, result);
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

        static bool WillFit(byte?[] dest, uint pos, byte[] source)
        {
            for (uint addr = pos; addr < pos + source.Length; addr++)
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

        class Game
        {
            public enum NesContainerType { iNES, UNIF };
            [JsonProperty("file_name")]
            public string FileName { get; set; }
            [JsonProperty("menu_name")]
            public string MenuName { get; set; }
            public readonly IEnumerable<byte> PRG = null;
            [JsonProperty("prg_size")]
            public uint PrgSize { get; set; }
            public readonly IEnumerable<byte> CHR = null;
            [JsonProperty("prg_offset")]
            public uint PrgOffset;
            [JsonProperty("chr_size")]
            public uint ChrSize { get; set; }
            [JsonProperty("chr_offset")]
            public uint ChrOffset;
            public uint? PrgRamSize { get; set; } = null;
            [JsonIgnore]
            public uint? ChrRamSize { get; set; } = null;
            [JsonProperty("mapper")]
            public string Mapper { get; set; }
            [JsonIgnore]
            public byte SaveId { get; set; }
            [JsonIgnore]
            public GameFlags Flags { get; set; }
            [JsonIgnore]
            public bool Battery { get; set; }
            [JsonIgnore]
            public NesFile.MirroringType Mirroring { get; set; }
            [JsonIgnore]
            public NesContainerType ContainerType { get; set; }

            [Flags]
            public enum GameFlags
            {
                Separator = 0x80,
                WillNotWorkOnNtsc = 0x01,
                WillNotWorkOnPal = 0x02,
                WillNotWorkOnDendy = 0x04,
                WillNotWorkOnNewFamiclone = 0x08
            };

            public Game(string fileName, string menuName = null, Dictionary<uint, GameFix> fixes = null)
            {
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
                    if (string.IsNullOrWhiteSpace(menuName))
                    {
                        // Menu name based on filename
                        MenuName = Regex.Replace(Path.GetFileNameWithoutExtension(fileName), @"( ?\[.*?\])|( \(.\))", string.Empty).Replace("_", " ").ToUpper().Replace(", THE", "").Trim();
                    }
                    else
                    {
                        MenuName = menuName;
                    }
                    // Strip long names
                    if (MenuName.Length > 28)
                        MenuName = MenuName.Substring(0, 25).Trim() + "...";
                    uint crc;
                    try
                    {
                        var nesFile = new NesFile(fileName);
                        var fixResult = nesFile.CorrectRom();
                        if (fixResult != 0)
                            Console.WriteLine(" Invalid header. Fix: " + fixResult);
                        PRG = nesFile.PRG;
                        PrgSize = (uint)nesFile.PRG.Length;
                        CHR = nesFile.CHR;
                        ChrSize = (uint)nesFile.CHR.Length;
                        Battery = nesFile.Battery;
                        Mapper = $"{nesFile.Mapper}" + ((nesFile.Submapper > 0) ? $":{nesFile.Submapper}" : "");
                        Mirroring = nesFile.Mirroring;
                        ContainerType = NesContainerType.iNES;
                        if (nesFile.Trainer != null && nesFile.Trainer.Length > 0)
                            throw new NotImplementedException(string.Format("{0} - trained games are not supported yet", Path.GetFileName(fileName)));
                        if (nesFile.Version == NesFile.iNesVersion.NES20)
                        {
                            PrgRamSize = nesFile.PrgRamSize + nesFile.PrgNvRamSize;
                            ChrRamSize = nesFile.ChrRamSize + nesFile.ChrNvRamSize;
                        }
                        crc = nesFile.CalculateCRC32();
                    }
                    catch (InvalidDataException)
                    {
                        var unifFile = new UnifFile(fileName);
                        PRG = unifFile.Fields.Where(k => k.Key.StartsWith("PRG")).OrderBy(k => k.Key).SelectMany(i => i.Value);
                        PrgSize = (uint)PRG.Count();
                        CHR = unifFile.Fields.Where(k => k.Key.StartsWith("CHR")).OrderBy(k => k.Key).SelectMany(i => i.Value);
                        ChrSize = (uint)CHR.Count();
                        Battery = unifFile.Battery;
                        var mapper = unifFile.Mapper;
                        if (mapper.StartsWith("NES-") || mapper.StartsWith("UNL-") || mapper.StartsWith("HVC-") || mapper.StartsWith("BTL-") || mapper.StartsWith("BMC-"))
                            mapper = mapper.Substring(4);
                        Mapper = mapper;
                        Mirroring = unifFile.Mirroring;
                        ContainerType = NesContainerType.UNIF;
                        crc = unifFile.CalculateCRC32();
                    }
                    // Check for fixes database
                    if (fixes != null)
                    {
                        GameFix fix = null;
                        if (fixes.TryGetValue(crc, out fix))
                        {
                            if (fix.PrgRamSize.HasValue)
                                PrgRamSize = fix.PrgRamSize;
                            if (fix.ChrRamSize.HasValue)
                                ChrRamSize = fix.ChrRamSize;
                            if (fix.Battery.HasValue)
                                Battery = fix.Battery.Value;
                            if (fix.WillNotWorkOnPal)
                                Flags |= GameFlags.WillNotWorkOnPal;
                            if (fix.WillNotWorkOnNtsc)
                                Flags |= GameFlags.WillNotWorkOnNtsc;
                            if (fix.WillNotWorkOnDendy)
                                Flags |= GameFlags.WillNotWorkOnDendy;
                            if (fix.WillNotWorkOnNewFamiclone)
                                Flags |= GameFlags.WillNotWorkOnNewFamiclone;
                        }
                    }
                    // External NTRAM is not supported on new famiclones
                    if (Mirroring == NesFile.MirroringType.FourScreenVram)
                        Flags |= GameFlags.WillNotWorkOnNewFamiclone;
                    // Check for round sizes
                    if (PrgSize > 0)
                    {
                        uint roundSize = 1;
                        while (roundSize < PrgSize)
                            roundSize <<= 1;
                        if (roundSize > PrgSize)
                        {
                            var newPrg = new byte[roundSize];
                            for (uint i = PrgSize; i < roundSize; i++) newPrg[i] = 0xFF;
                            Array.Copy(PRG.ToArray(), newPrg, PrgSize);
                            PRG = newPrg;
                            PrgSize = roundSize;
                        }
                    }
                    if (ChrSize > 0)
                    {
                        uint roundSize = 1;
                        while (roundSize < ChrSize)
                            roundSize <<= 1;
                        if (roundSize > ChrSize)
                        {
                            var newChr = new byte[roundSize];
                            for (uint i = ChrSize; i < roundSize; i++) newChr[i] = 0xFF;
                            Array.Copy(CHR.ToArray(), newChr, PrgSize);
                            CHR = newChr;
                            ChrSize = roundSize;
                        }
                    }
                }
            }

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
