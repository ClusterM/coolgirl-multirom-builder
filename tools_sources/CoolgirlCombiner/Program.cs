using com.clusterrr.Famicom.Containers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;

namespace com.clusterrr.Famicom.CoolGirl
{
    class Program
    {
        const string DEFAULT_MAPPERS_FILE = @"coolgirl-mappers.json";
        const string DEFAULT_FIXES_FILE = @"coolgirl-fixes.json";
        const string DEFAULT_SYMBOLS_FILE = @"coolgirl-symbols.json";

        static int Main(string[] args)
        {
            try
            {
                Console.WriteLine("COOLGIRL UNIF combiner");
                Console.WriteLine("(c) Cluster, 2021");
                Console.WriteLine("http://clusterrr.com");
                Console.WriteLine("clusterrr@clusterrr.com");
                Console.WriteLine();
                bool needShowHelp = false;

                const string commandPrepare = "prepare";
                const string commandCombine = "combine";
                const string commandBuild = "build";

                string command = null;
                string optionMappersFile = Path.Combine(Path.GetDirectoryName(AppContext.BaseDirectory), DEFAULT_MAPPERS_FILE);
                string optionFixesFile = Path.Combine(Path.GetDirectoryName(AppContext.BaseDirectory), DEFAULT_FIXES_FILE);
                string optionSymbolsFile = Path.Combine(Path.GetDirectoryName(AppContext.BaseDirectory), DEFAULT_SYMBOLS_FILE);
                string optionNesAsm = "nesasm";
                string optionNesAsmArgs = "";
                string optionGamesFile = null;
                string optionAsmFile = null;
                string optionOffsetsFile = null;
                string optionReportFile = null;
                string optionLoaderFile = null;
                string optionUnifFile = null;
                string optionNes20File = null;
                string optionBinFile = null;
                string optionLanguage = "eng";
                var badSectors = new List<int>();
                bool optionNoSort = false;
                int optionMaxRomSize = 256;
                int optionMaxChrRamSize = 256;
                bool optionCalculateMd5 = false;
                var jsonOptions = new JsonSerializerOptions()
                {
                    WriteIndented = true,
                    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault,
                    Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
                };

                if (args.Length > 0) command = args[0].ToLower();
                if ((command != commandPrepare) && (command != commandCombine) && (command != commandBuild))
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
                        case "fixes":
                            optionFixesFile = value;
                            i++;
                            break;
                        case "symbols":
                            optionSymbolsFile = value;
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
                        case "nes20":
                            optionNes20File = value;
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
                        case "nesasm":
                            optionNesAsm = value;
                            i++;
                            break;
                        case "nesasm-args":
                        case "nesasmargs":
                            optionNesAsmArgs = value;
                            i++;
                            break;
                        case "md5":
                            optionCalculateMd5 = true;
                            break;
                        default:
                            Console.WriteLine("Unknown parameter: " + param);
                            needShowHelp = true;
                            break;
                    }
                }

                if ((optionGamesFile == null) && ((command == commandPrepare) || (command == commandBuild)))
                {
                    Console.WriteLine("Missing required parameter: --games");
                    needShowHelp = true;
                }
                if ((optionAsmFile == null) && ((command == commandPrepare) || (command == commandBuild)))
                {
                    Console.WriteLine("Missing required parameter: --asm");
                    needShowHelp = true;
                }
                if ((optionOffsetsFile == null) && (command == commandPrepare))
                {
                    Console.WriteLine("Missing required parameter: --offsets");
                    needShowHelp = true;
                }
                if ((optionLoaderFile == null) && (command == commandCombine))
                {
                    Console.WriteLine("Missing required parameter: --loader");
                    needShowHelp = true;
                }
                if ((optionUnifFile == null) && (optionNes20File == null) && (optionBinFile == null) && ((command == commandCombine) || (command == commandBuild)))
                {
                    Console.WriteLine("At least one parameter required: --unif, --nes20 or --bin");
                    needShowHelp = true;
                }

                if (needShowHelp)
                {
                    Console.WriteLine("--- Usage ---");
                    Console.WriteLine("First step:");
                    Console.WriteLine(" CoolgirlCombiner.exe prepare --games <games.txt> --asm <games.asm> --offsets <offsets.json> [--report <report.txt>] [--nosort] [--maxromsize sizemb] [--maxchrsize sizekb] [--language <language>] [--badsectors <sectors>]");
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
                    Console.WriteLine(" CoolgirlCombiner.exe combine --loader <menu.nes> --offsets <offsets.json> [--md5] [--unif <multirom.unf>] [--nes20 multirom.nes] [--bin <multirom.bin>]");
                    Console.WriteLine("  {0,-20}{1}", "--loader", "- loader (compiled using asm file generated by first step)");
                    Console.WriteLine("  {0,-20}{1}", "--offsets", "- input file with offsets for every game (generated by first step)");
                    Console.WriteLine("  {0,-20}{1}", "--md5", "- calculate and show MD5 checksum of ROM");
                    Console.WriteLine("  {0,-20}{1}", "--unif", "- output UNIF file");
                    Console.WriteLine("  {0,-20}{1}", "--nes20", "- output NES 2.0 file");
                    Console.WriteLine("  {0,-20}{1}", "--bin", "- output raw binary file");
                    Console.WriteLine("All at once:");
                    Console.WriteLine(" CoolgirlCombiner.exe build --games <games.txt> --asm <games.asm> [--md5] [--nesasm <nesasm.exe>] [--nesasm-args <args>] [--report <report.txt>] [--nosort] [--maxromsize sizemb] [--maxchrsize sizekb] [--language <language>] [--badsectors <sectors>] [--unif <multirom.unf>] [--nes20 multirom.nes] [--bin <multirom.bin>]");
                    Console.WriteLine("  {0,-20}{1}", "--games", "- input plain text file with list of ROM files");
                    Console.WriteLine("  {0,-20}{1}", "--asm", "- output file for loader");
                    Console.WriteLine("  {0,-20}{1}", "--md5", "- calculate and show MD5 checksum of ROM");
                    Console.WriteLine("  {0,-20}{1}", "--nesasm", "- path to nesasm compiler executable");
                    Console.WriteLine("  {0,-20}{1}", "--nesasm-args", "- additional command line arguments for nesasm");
                    Console.WriteLine("  {0,-20}{1}", "--report", "- output report file (human readable)");
                    Console.WriteLine("  {0,-20}{1}", "--nosort", "- disable automatic sort by name");
                    Console.WriteLine("  {0,-20}{1}", "--maxromsize", "- maximum size for final file (in megabytes)");
                    Console.WriteLine("  {0,-20}{1}", "--maxchrsize", "- maximum CHR RAM size (in kilobytes), default is 256");
                    Console.WriteLine("  {0,-20}{1}", "--language", "- language for system messages: \"eng\" or \"rus\"");
                    Console.WriteLine("  {0,-20}{1}", "--badsectors", "- comma-separated separated list of bad sectors,");
                    Console.WriteLine("  {0,-20}{1}", "--unif", "- output UNIF file");
                    Console.WriteLine("  {0,-20}{1}", "--nes20", "- output NES 2.0 file");
                    Console.WriteLine("  {0,-20}{1}", "--bin", "- output raw binary file");
                    return 1;
                }

                byte?[] result = null;
                if ((command == commandPrepare) || (command == commandBuild))
                {
                    // Loading mappers file
                    var mappersJson = File.ReadAllText(optionMappersFile);
                    var mappers = JsonSerializer.Deserialize<Dictionary<string, Mapper>>(mappersJson, jsonOptions);
                    // Add padding zeros
                    uint t;
                    // Select numeric mappers
                    var mappersNumbers = mappers.Keys.Where(k => uint.TryParse(k, out t)).ToArray();
                    foreach (var mapperNumber in mappersNumbers)
                    {
                        t = uint.Parse(mapperNumber);
                        var padded = $"{t:D3}";
                        if (mapperNumber != padded)
                        {
                            mappers[padded] = mappers[mapperNumber];
                            mappers.Remove(mapperNumber);
                        }
                    }

                    // Loading fixes file
                    Dictionary<uint, GameFix> fixes;
                    if (File.Exists(optionFixesFile))
                    {
                        var fixesJson = File.ReadAllText(optionFixesFile);
                        var fixesStr = JsonSerializer.Deserialize<Dictionary<string, GameFix>>(fixesJson, jsonOptions);
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
                    // Loading symbols table
                    var symbolsJson = File.ReadAllText(optionSymbolsFile);
                    var symbols = JsonSerializer.Deserialize<Dictionary<char, byte>>(symbolsJson, jsonOptions);
                    // Loading games list
                    var lines = File.ReadAllLines(optionGamesFile);
                    var regs = new Dictionary<string, List<String>>();
                    var games = new List<Game>();
                    var report = new List<String>();
                    result = new byte?[128 * 1024];

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
                        if (line.Trim().ToUpper() == "!NOSORT")
                        {
                            optionNoSort = true;
                            continue;
                        }
                        var cols = line.Split(new char[] { '|' }, 2, StringSplitOptions.RemoveEmptyEntries);
                        string fileName = cols[0].Trim();
                        string menuName = cols.Length >= 2 ? cols[1] : null;

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
                                games.Where(g => !g.IsHidden),
                                games.Where(g => g.IsHidden)
                            );
                    }
                    else
                    {
                        // Removing separators
                        var gamesNoSeparators = games.Where(g => !g.IsSeparator);
                        sortedGames =
                            Enumerable.Concat(
                                gamesNoSeparators.Where(g => !g.IsHidden).OrderBy(g => g.MenuName, new ClassicSorter()),
                                gamesNoSeparators.Where(g => g.IsHidden)
                            );
                    }

                    int gamesCount = sortedGames.Count();
                    int hiddenCount = games.Where(g => g.IsHidden).Count();
                    int menuItemsCount = gamesCount - hiddenCount;

                    byte saveId = 0;
                    foreach (var game in sortedGames)
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

                        Console.Write($"Fitting PRG of {Path.GetFileName(game.FileName)} ({game.PrgSize / 1024}KB)... ");
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

                        Console.Write($"Fitting CHR of {Path.GetFileName(game.FileName)} ({game.ChrSize / 1024}KB)... ");
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
                                Console.WriteLine($"address: {pos:X8}");
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
                        if (!game.IsHidden)
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

                    regs["reg_0"] = new List<string>();
                    regs["reg_1"] = new List<string>();
                    regs["reg_2"] = new List<string>();
                    regs["reg_3"] = new List<string>();
                    regs["reg_4"] = new List<string>();
                    regs["reg_5"] = new List<string>();
                    regs["reg_6"] = new List<string>();
                    regs["reg_7"] = new List<string>();
                    regs["chr_start_bank_h"] = new List<string>();
                    regs["chr_start_bank_l"] = new List<string>();
                    regs["chr_start_bank_s"] = new List<string>();
                    regs["chr_count"] = new List<string>();
                    regs["game_save"] = new List<string>();
                    regs["game_flags"] = new List<string>();
                    regs["cursor_pos"] = new List<string>();

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
                        var flags = mapperInfo.Flags;

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
                            case 8 * 1024:
                                prgRamEnabled = true;
                                break;
                            case 16 * 1024:
                                prgRamEnabled = true;
                                flags |= mapperInfo.Flags16kPrgRam;
                                break;
                            case 32 * 1024:
                                throw new NotImplementedException($"{Path.GetFileName(game.FileName)}: 32KB of PRG RAM is not supported yet");
                            default:
                                throw new NotImplementedException($"{Path.GetFileName(game.FileName)}: Weird PRG RAM value: {game.PrgRamSize}");
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
                        regs["reg_6"].Add(string.Format("${0:X2}", (flags << 5) | (mapperInfo.MapperRegister & 0x1F)));                             // flag[2:0], mapper[4:0]
                        regs["reg_7"].Add(string.Format("${0:X2}", @params | ((mapperInfo.MapperRegister & 0x20) << 1)));                           // lockout, mapper[5], four_screen, mirroring[1:0], prg_write_on, chr_write_en, sram_enabled
                        regs["chr_start_bank_h"].Add(string.Format("${0:X2}", ((game.ChrOffset / 0x4000) >> 8) & 0xFF));
                        regs["chr_start_bank_l"].Add(string.Format("${0:X2}", ((game.ChrOffset / 0x4000)) & 0xFF));
                        regs["chr_start_bank_s"].Add(string.Format("${0:X2}", ((game.ChrOffset % 0x4000) >> 8) | 0x80));
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
                    asmResult.AppendLine();
                    asmResult.AppendLine("; Common constants");
                    asmResult.AppendLine($"GAMES_COUNT .equ {menuItemsCount}");
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
                    c = 0;
                    foreach (var game in sortedGames)
                    {
                        if (c % 256 == 0)
                        {
                            asmResult.AppendLine();
                            asmResult.AppendLine($"  .bank {baseBank + c / 256 * 2}");
                            asmResult.AppendLine($"  .org $9000");
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
                        var name = StringToTiles(game.MenuName, symbols);
                        var asm = BytesToAsm(name);
                        asmResult.Append(asm);
                        c++;
                    }

                    // Some strings
                    asmResult.AppendLine();
                    asmResult.AppendLine();
                    asmResult.AppendLine("; Some strings");
                    asmResult.AppendLine("  .bank 14");
                    asmResult.AppendLine("  .org $C800");
                    asmResult.AppendLine();
                    asmResult.AppendLine("string_file:");
                    asmResult.Append(BytesToAsm(StringToTiles("FILE: " + Path.GetFileName(optionGamesFile), symbols)));
                    asmResult.AppendLine("string_build_date:");
                    asmResult.Append(BytesToAsm(StringToTiles("BUILD DATE: " + DateTime.Now.ToString("yyyy-MM-dd"), symbols)));
                    asmResult.AppendLine("string_build_time:");
                    asmResult.Append(BytesToAsm(StringToTiles("BUILD TIME: " + DateTime.Now.ToString("HH:mm:ss"), symbols)));
                    asmResult.AppendLine("string_console_type:");
                    asmResult.Append(BytesToAsm(StringToTiles("CONSOLE TYPE:", symbols)));
                    asmResult.AppendLine("string_ntsc:");
                    asmResult.Append(BytesToAsm(StringToTiles("NTSC", symbols)));
                    asmResult.AppendLine("string_pal:");
                    asmResult.Append(BytesToAsm(StringToTiles("PAL", symbols)));
                    asmResult.AppendLine("string_dendy:");
                    asmResult.Append(BytesToAsm(StringToTiles("DENDY", symbols)));
                    asmResult.AppendLine("string_new:");
                    asmResult.Append(BytesToAsm(StringToTiles("NEW", symbols)));
                    asmResult.AppendLine("string_flash:");
                    asmResult.Append(BytesToAsm(StringToTiles("FLASH:", symbols)));
                    asmResult.AppendLine("string_read_only:");
                    asmResult.Append(BytesToAsm(StringToTiles("READ ONLY", symbols)));
                    asmResult.AppendLine("string_writable:");
                    asmResult.Append(BytesToAsm(StringToTiles("WRITABLE", symbols)));
                    asmResult.AppendLine("flash_sizes:");
                    for (int i = 0; i <= 10; i++)
                        asmResult.AppendLine($"  .dw string_{1 << i}mb");
                    for (int i = 0; i <= 8; i++)
                    {
                        asmResult.AppendLine($"string_{1 << i}mb:");
                        asmResult.Append(BytesToAsm(StringToTiles($"{1 << i}MB", symbols)));
                    }
                    asmResult.AppendLine("string_chr_ram:");
                    asmResult.Append(BytesToAsm(StringToTiles("CHR RAM:", symbols)));
                    asmResult.AppendLine("chr_ram_sizes:");
                    for (int i = 0; i <= 8; i++)
                        asmResult.AppendLine($"  .dw string_{8 * (1 << i)}kb");
                    for (int i = 0; i <= 8; i++)
                    {
                        asmResult.AppendLine($"string_{8 * (1 << i)}kb:");
                        asmResult.Append(BytesToAsm(StringToTiles($"{8 * (1 << i)}KB", symbols)));
                    }
                    asmResult.AppendLine("string_prg_ram:");
                    asmResult.Append(BytesToAsm(StringToTiles("PRG RAM:", symbols)));
                    asmResult.AppendLine("string_present:");
                    asmResult.Append(BytesToAsm(StringToTiles("PRESENT", symbols)));
                    asmResult.AppendLine("string_not_available:");
                    asmResult.Append(BytesToAsm(StringToTiles("NOT AVAILABLE", symbols)));
                    asmResult.AppendLine("string_saving:");
                    if (optionLanguage == "rus")
                        asmResult.Append(BytesToAsm(StringToTiles("  СОХРАНЯЕМСЯ... НЕ ВЫКЛЮЧАЙ!   ", symbols)));
                    else
                        asmResult.Append(BytesToAsm(StringToTiles("   SAVING... DON'T TURN OFF!    ", symbols)));
                    File.WriteAllText(optionAsmFile, asmResult.ToString());
                    asmResult.AppendLine("string_incompatible_console:");
                    if (optionLanguage == "rus")
                        asmResult.Append(BytesToAsm(StringToTiles("     ИЗВИНИТЕ,  ДАННАЯ ИГРА       НЕСОВМЕСТИМА С ЭТОЙ КОНСОЛЬЮ                                        НАЖМИТЕ ЛЮБУЮ КНОПКУ      ", symbols)));
                    else
                        asmResult.Append(BytesToAsm(StringToTiles("    SORRY,  THIS GAME IS NOT      COMPATIBLE WITH THIS CONSOLE                                          PRESS ANY BUTTON        ", symbols)));
                    asmResult.AppendLine("string_calculating_crc:");
                    if (optionLanguage == "rus")
                        asmResult.Append(BytesToAsm(StringToTiles("   СЧИТАЕМ КОНТРОЛЬНУЮ СУММУ,      ПОДОЖДИТЕ НЕСКОЛЬКО ЧАСОВ", symbols)));
                    else
                        asmResult.Append(BytesToAsm(StringToTiles("        CALCULATING CRC,            PLEASE WAIT A FEW HOURS", symbols)));
                    asmResult.AppendLine("string_prg_ram_test:");
                    asmResult.Append(BytesToAsm(StringToTiles("PRG RAM TEST:", symbols)));
                    asmResult.AppendLine("string_chr_ram_test:");
                    asmResult.Append(BytesToAsm(StringToTiles("CHR RAM TEST:", symbols)));
                    asmResult.AppendLine("string_passed:");
                    asmResult.Append(BytesToAsm(StringToTiles("PASSED", symbols)));
                    asmResult.AppendLine("string_failed:");
                    asmResult.Append(BytesToAsm(StringToTiles("FAILED", symbols)));
                    asmResult.AppendLine("string_ok:");
                    asmResult.Append(BytesToAsm(StringToTiles("OK", symbols)));
                    asmResult.AppendLine("string_error:");
                    asmResult.Append(BytesToAsm(StringToTiles("ERROR", symbols)));

                    File.WriteAllText(optionAsmFile, asmResult.ToString());

                    if (command == commandPrepare)
                    {
                        var offsets = new Offsets();
                        offsets.Size = romSize;
                        offsets.RomCount = gamesCount;
                        offsets.GamesFile = Path.GetFileName(optionGamesFile);
                        offsets.Games = sortedGames.Where(g => !g.IsSeparator).ToArray();
                        File.WriteAllText(optionOffsetsFile, JsonSerializer.Serialize(offsets, jsonOptions));
                    }

                    if (command == commandBuild)
                    {
                        Console.Write("Compiling using nesasm... ");
                        Array.Resize(ref result, (int)romSize);
                        var process = new Process();
                        var cp866 = CodePagesEncodingProvider.Instance.GetEncoding(866);
                        process.StartInfo.FileName = optionNesAsm;
                        process.StartInfo.Arguments = $"\"menu.asm\" -r -o - " + optionNesAsmArgs;
                        process.StartInfo.WorkingDirectory = Path.GetDirectoryName(optionAsmFile);
                        process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                        process.StartInfo.UseShellExecute = false;
                        process.StartInfo.CreateNoWindow = true;
                        process.StartInfo.StandardOutputEncoding = cp866;
                        process.StartInfo.StandardErrorEncoding = cp866;
                        process.StartInfo.RedirectStandardInput = true;
                        process.StartInfo.RedirectStandardOutput = true;
                        process.StartInfo.RedirectStandardError = true;
                        process.Start();

                        int b;
                        var stdout = new List<char>();
                        var stderr = new StringBuilder();

                        while (!process.HasExited || !process.StandardOutput.EndOfStream || !process.StandardError.EndOfStream)
                        {
                            while ((b = process.StandardOutput.Read()) >= 0)
                                stdout.Add((char)b);
                            while ((b = process.StandardError.Read()) >= 0)
                                stderr.Append((char)b);
                            Thread.Sleep(10);
                        }

                        if (stderr.Length > 0)
                            Console.WriteLine(stderr);
                        if (process.ExitCode != 0)
                        {
                            Console.WriteLine(string.Join("", stdout));
                            throw new InvalidOperationException($"nesasm returned error code {process.ExitCode}");
                        }

                        var loader = cp866.GetBytes(stdout.ToArray());
                        if (!loader.Any())
                            throw new InvalidDataException("nesasm returned empty data, maybe version is too old?");
                        for (int i = 0; i < loader.Length; i++)
                            result[i] = loader[i];
                        Console.WriteLine("OK");
                    }
                }
                if (command == commandCombine) // Combine
                {
                    var offsetsJson = File.ReadAllText(optionOffsetsFile);
                    var offsets = JsonSerializer.Deserialize<Offsets>(offsetsJson, jsonOptions);
                    result = new byte?[offsets.Size];
                    // Use 0xFF as empty value because it doesn't require writing to flash
                    for (int i = 0; i < offsets.Size; i++)
                        result[i] = 0xFF;

                    Console.Write("Loading loader... ");
                    var loaderFile = new NesFile(optionLoaderFile);
                    var loader = loaderFile.PRG.ToArray();
                    for (int i = 0; i < loader.Length; i++)
                        result[i] = loader[i];
                    Console.WriteLine("OK.");

                    foreach (var game in offsets.Games)
                    {
                        if (!string.IsNullOrEmpty(game.FileName))
                        {
                            Console.Write($"Loading {Path.GetFileName(game.FileName)}... "); ;
                            switch (game.ContainerType)
                            {
                                case Game.NesContainerType.iNES:
                                    {
                                        var nesFile = new NesFile(game.FileName);
                                        var prg = nesFile.PRG.ToArray();
                                        var chr = nesFile.CHR.ToArray();
                                        for (int i = 0; i < prg.Length; i++)
                                            result[game.PrgOffset + i] = prg[i];
                                        for (int i = 0; i < chr.Length; i++)
                                            result[game.ChrOffset + i] = chr[i];
                                    }
                                    break;
                                case Game.NesContainerType.UNIF:
                                    {
                                        var unifFile = new UnifFile(game.FileName);
                                        var prg = unifFile.Fields.Where(k => k.Key.StartsWith("PRG")).OrderBy(k => k.Key).SelectMany(i => i.Value).ToArray();
                                        var chr = unifFile.Fields.Where(k => k.Key.StartsWith("CHR")).OrderBy(k => k.Key).SelectMany(i => i.Value).ToArray();
                                        for (int i = 0; i < prg.Length; i++)
                                            result[game.PrgOffset + i] = prg[i];
                                        for (int i = 0; i < chr.Length; i++)
                                            result[game.ChrOffset + i] = chr[i];
                                    }
                                    break;
                            }
                            Console.WriteLine("OK.");
                        }
                        GC.Collect();
                    }
                }

                if ((command == commandCombine) || (command == commandBuild)) // Combine or build
                {
                    if (!string.IsNullOrEmpty(optionUnifFile))
                    {
                        Console.Write("Saving UNIF file... ");
                        var resultNotNull = result.Select(b => b ?? 0xFF).ToArray();
                        var u = new UnifFile();
                        u.Version = 5;
                        u.Mapper = "COOLGIRL";
                        u.Mirroring = NesFile.MirroringType.MapperControlled;
                        u.Fields["MIRR"] = new byte[] { 5 };
                        u.Fields["PRG0"] = resultNotNull;
                        u.Battery = true;
                        u.Save(optionUnifFile);
                        Console.WriteLine("OK");

                        if (optionCalculateMd5)
                        {
                            // Need to calculate MD5
                            Console.WriteLine("Calculating MD5...");
                            uint sizeFixed = 1;
                            while (sizeFixed < result.Length) sizeFixed <<= 1;
                            var resultSizeFixed = new byte[sizeFixed];
                            Array.Copy(resultNotNull, 0, resultSizeFixed, 0, resultNotNull.Length);
                            for (int i = result.Length; i < sizeFixed; i++)
                                resultSizeFixed[i] = 0xFF;
                            var md5 = System.Security.Cryptography.MD5.Create();
                            var md5hash = md5.ComputeHash(resultSizeFixed);
                            Console.Write("ROM MD5: ");
                            foreach (var b in md5hash)
                                Console.Write("{0:x2}", b);
                            Console.WriteLine();
                        }
                    }
                    if (!string.IsNullOrEmpty(optionNes20File))
                    {
                        Console.Write("Saving iNES file... ");
                        var resultNotNull = result.Select(b => b ?? 0xFF).ToArray();
                        var nes = new NesFile();
                        nes.Version = NesFile.iNesVersion.NES20;
                        nes.PRG = resultNotNull;
                        nes.Mapper = 3913;
                        nes.PrgNvRamSize = 32 * 1024;
                        nes.ChrRamSize = 256 * 1024;
                        nes.Save(optionNes20File);
                        Console.WriteLine("OK");
                        if (optionCalculateMd5)
                        {
                            Console.WriteLine("Calculating MD5...");
                            var md5hash = nes.CalculateMD5();
                            Console.Write("ROM MD5: ");
                            foreach (var b in md5hash)
                                Console.Write("{0:x2}", b);
                            Console.WriteLine();
                        }
                    }
                    if (!string.IsNullOrEmpty(optionBinFile))
                    {
                        Console.Write("Saving BIN file... ");
                        var resultNotNull = result.Select(b => b ?? 0xFF).ToArray();
                        File.WriteAllBytes(optionBinFile, resultNotNull);
                        Console.WriteLine("OK");
                        if (optionCalculateMd5)
                        {
                            Console.WriteLine("Calculating MD5...");
                            var md5 = System.Security.Cryptography.MD5.Create();
                            var md5hash = md5.ComputeHash(resultNotNull);
                            Console.Write("ROM MD5: ");
                            foreach (var b in md5hash)
                                Console.Write("{0:x2}", b);
                            Console.WriteLine();
                        }
                    }
                }
                Console.WriteLine("Done.");
            }
            catch (Exception ex)
            {
#if DEBUG
                Console.WriteLine($"Error: {ex.GetType()}: {ex.Message}{ex.StackTrace}");
#else
                Console.WriteLine($"Error: {ex.GetType()}: {ex.Message}");
#endif
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

        static byte[] StringToTiles(string text, Dictionary<char, byte> symbolsTable)
        {
            text = text.ToUpper();
            var result = new byte[text.Length + 1];
            for (int c = 0; c < result.Length; c++)
            {
                if (c < text.Length)
                {
                    byte charCode;
                    if (symbolsTable.TryGetValue(text[c], out charCode))
                        result[c] = charCode;
                    else
                        result[c] = 0xFF;
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
    }
}
