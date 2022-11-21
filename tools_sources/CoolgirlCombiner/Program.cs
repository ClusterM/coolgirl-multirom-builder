using com.clusterrr.Famicom.Containers;
using com.clusterrr.Famicom.Multirom;
using com.clusterrr.Tools;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;

namespace com.clusterrr.Famicom.CoolGirl
{
    public class Program
    {
        const string REPO_PATH = "https://github.com/ClusterM/coolgirl-multirom-builder";
        static DateTime BUILD_TIME = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc).AddSeconds(long.Parse(Properties.Resources.buildtime.Trim()));
        const int LOADER_OFFSET = 0;
        const int LOADER_SIZE = 128 * 1024;
        const int FLASH_SECTOR_SIZE = 128 * 1024;
        const int MAX_GAME_COUNT = 256 * 6;
        const int MAX_SAVE_COUNT = byte.MaxValue;

        public static int Main(string[] args)
        {
            try
            {
                Console.WriteLine($"COOLGIRL Combiner v{Assembly.GetExecutingAssembly()?.GetName()?.Version?.Major}.{Assembly.GetExecutingAssembly()?.GetName()?.Version?.Minor}");
                Console.WriteLine($"  Commit {Properties.Resources.gitCommit} @ {REPO_PATH}");
#if DEBUG
                Console.WriteLine($"  Debug version, build time: {BUILD_TIME.ToLocalTime()}");
#endif
                Console.WriteLine("  (c) Alexey 'Cluster' Avdyukhin / https://clusterrr.com / clusterrr@clusterrr.com");
                Console.WriteLine("");

                var config = Config.Parse(args);

                if (config == null)
                {
                    Config.PrintHelp();
                    return 1;
                }

                var jsonOptions = new JsonSerializerOptions()
                {
                    WriteIndented = true,
                    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault,
                    Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
                };

                // Reserved for loader
                byte[]? result = null;

                // Step one: load ROMs, allocate space for them and generate config for loader
                if ((config.Command == Config.CombinerCommand.Prepare) || (config.Command == Config.CombinerCommand.Build))
                {
                    // Use 0xFF as empty value because it doesn't require writing to flash
                    result = Enumerable.Repeat(byte.MaxValue, (int)(config.MaxRomSizeMB * 1024 * 1024)).ToArray();
                    // Loading mappers file
                    var mappersJson = File.ReadAllText(config.MappersFile);
                    var mappers = JsonSerializer.Deserialize<Dictionary<string, Mapper>>(mappersJson, jsonOptions);
                    if (mappers == null) throw new InvalidDataException("Can't read mappers file");
                    // Add padding zeros
                    // Select numeric mappers
                    var mapperNumbers = mappers.Keys.Where(k => uint.TryParse(k, out uint t)).ToArray();
                    foreach (var mapperNumber in mapperNumbers)
                    {
                        var t = uint.Parse(mapperNumber);
                        var padded = $"{t:D3}";
                        if (mapperNumber != padded)
                        {
                            mappers[padded] = mappers[mapperNumber];
                            mappers.Remove(mapperNumber);
                        }
                    }

                    // Loading fixes file
                    Dictionary<string, GameFix>? fixes;
                    if (File.Exists(config.FixesFile))
                    {
                        var fixesJson = File.ReadAllText(config.FixesFile);
                        var fixesStr = JsonSerializer.Deserialize<Dictionary<string, GameFix>>(fixesJson, jsonOptions);
                        if (fixesStr == null) throw new InvalidDataException("Can't read fixes file");
                        // Convert string CRC32 to uint
                        fixes = fixesStr.ToDictionary(
                                    // Check for hexademical values
                                    kv => kv.Key.ToLower().StartsWith("0x")
                                        ? kv.Key[2..].ToLower()
                                        : kv.Key.ToLower(),
                                    kv => kv.Value);
                    }
                    else
                    {
                        Console.WriteLine("WARNING! Fixes file not found, fixes database will not be used");
                        fixes = null;
                    }

                    // Loading symbols table
                    var symbolsJson = File.ReadAllText(config.SymbolsFile);
                    var symbols = JsonSerializer.Deserialize<Dictionary<char, byte>>(symbolsJson, jsonOptions);
                    if (symbols == null) throw new InvalidDataException("Can't load symbols file");

                    // Loading games list
                    var lines = File.ReadAllLines(config.GamesFile!);
                    var regs = new Dictionary<string, List<String>>();
                    var games = new List<Game>();
                    var report = new List<String>();
                    bool nosort = config.NoSort;

                    // Building list of ROMs
                    foreach (var line in lines)
                    {
                        // Skip empty lines
                        if (string.IsNullOrWhiteSpace(line)) continue;
                        // Skip comments
                        if (line.Trim().StartsWith(";")) continue;
                        if (line.Trim().ToUpper() == "!NOSORT")
                        {
                            nosort = true;
                            continue;
                        }
                        var cols = line.Split(new char[] { '|' }, 2, StringSplitOptions.RemoveEmptyEntries);
                        string fileName = cols[0].Trim();
                        string? menuName = cols.Length >= 2 ? cols[1] : null;

                        // Is it a directory?
                        if (fileName.EndsWith("/") || fileName.EndsWith("\\"))
                        {
                            Console.WriteLine($"Loading directory: {fileName}");
                            var files =
                                Directory.GetFiles(fileName, "*.nes").Concat(
                                Directory.GetFiles(fileName, "*.unf")).Concat(
                                Directory.GetFiles(fileName, "*.unif"));
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
                    if (nosort)
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

                    int gameCount = sortedGames.Count();
                    int hiddenCount = games.Where(g => g.IsHidden).Count();
                    int menuItemsCount = gameCount - hiddenCount;

                    int saveId = 0;
                    foreach (var game in sortedGames)
                    {
                        if (game.Battery)
                        {
                            saveId++;
                            game.SaveId = (byte)saveId;
                        }
                    }

                    int usedSpace = LOADER_SIZE;
                    int notFittedSize = 0;
                    var sortedPrgs = games.OrderByDescending(g => g.PRG.Length).Where(g => g.PRG.Length > 0);
                    foreach (var game in sortedPrgs)
                    {
                        Console.Write($"Fitting PRG of {Path.GetFileName(game.FileName)} ({game.PRG.Length / 1024}KB)... ");
                        bool fitted = false;
                        for (int pos = 0; pos < config.MaxRomSizeMB * 1024 * 1024; pos += game.PRG.Length)
                        {
                            if (WillFit(result, pos, game.PRG, config.BadSectors))
                            {
                                game.PrgOffset = pos;
                                Array.Copy(game.PRG, 0, result, pos, game.PRG.Length);
                                usedSpace = Math.Max(LOADER_OFFSET + LOADER_SIZE, Math.Max(usedSpace, pos + game.PRG.Length));
                                fitted = true;
                                Console.WriteLine($"offset: 0x{pos:X8}");
                                break;
                            }
                        }
                        if (!fitted)
                        {
                            Console.WriteLine("Failed.");
                            notFittedSize += game.PRG.Length;
                        }
                        GC.Collect();
                    }

                    var sortedChrs = games.OrderByDescending(g => g.CHR.Length).Where(g => g.CHR.Length > 0);
                    foreach (var game in sortedChrs)
                    {
                        Console.Write($"Fitting CHR of {Path.GetFileName(game.FileName)} ({game.CHR.Length / 1024}KB)... ");
                        bool fitted = false;
                        for (int pos = 0; pos < config.MaxRomSizeMB * 1024 * 1024; pos += 0x2000)
                        {
                            if (WillFit(result, pos, game.CHR, config.BadSectors))
                            {
                                game.ChrOffset = pos;
                                Array.Copy(game.CHR, 0, result, pos, game.CHR.Length);
                                usedSpace = Math.Max(LOADER_OFFSET + LOADER_SIZE, Math.Max(usedSpace, pos + game.CHR.Length));
                                fitted = true;
                                Console.WriteLine($"offset: 0x{pos:X8}");
                                break;
                            }
                        }
                        if (!fitted)
                        {
                            Console.WriteLine("Failed.");
                            notFittedSize += game.CHR.Length;
                        }
                        GC.Collect();
                    }

                    // Calculate output ROM size
                    usedSpace += notFittedSize;
                    // Round up to minimum PRG bank size
                    usedSpace = 0x4000 * (int)Math.Ceiling((float)usedSpace / (float)0x4000);
                    int romSize = usedSpace;
                    // Round up to sector size
                    usedSpace = FLASH_SECTOR_SIZE * (int)Math.Ceiling((float)usedSpace / (float)FLASH_SECTOR_SIZE);
                    // Space for saves
                    usedSpace += FLASH_SECTOR_SIZE * (int)Math.Ceiling(saveId / 4.0);

                    int totalSize = 0;
                    int maxChrSize = 0;
                    report.Add(string.Format("{0,-33} {1,-15} {2,-10} {3}", "Game name", "Mapper", "Save ID", "Size"));
                    report.Add(string.Format("{0,-33} {1,-15} {2,-10} {3}", "------------", "-------", "-------", "-------"));
                    var mapperStats = new Dictionary<string, int>();
                    foreach (var game in sortedGames)
                    {
                        if (!game.IsHidden)
                        {
                            totalSize += game.PRG.Length;
                            totalSize += game.CHR.Length;
                            report.Add(string.Format("{0,-33} {1,-15} {2,-10} {3}",
                                FirstCharToUpper(game.ToString().Replace("_", " ")),
                                game.Mapper,
                                game.SaveId == 0 ? "-" : game.SaveId.ToString(),
                                $"{(game.PRG.Length + game.CHR.Length) / 1024}KB"));
                            if (!string.IsNullOrEmpty(game.Mapper))
                            {
                                if (!mapperStats.ContainsKey(game.Mapper)) mapperStats[game.Mapper] = 0;
                                mapperStats[game.Mapper]++;
                            }
                        }
                        if (game.CHR.Length > maxChrSize)
                            maxChrSize = game.CHR.Length;
                    }
                    report.Add("");
                    report.Add(string.Format("{0,-15} {1,0}", "Mapper", "Count"));
                    report.Add(string.Format("{0,-15} {1,0}", "------", "-----"));
                    foreach (var mapper in mapperStats.Keys.OrderBy(k => k))
                    {
                        report.Add(string.Format("{0,-15} {1,0}", mapper, mapperStats[mapper]));
                    }
                    report.Add("");
                    report.Add($"Total games: {sortedGames.Count() - hiddenCount}");
                    report.Add($"Total flash memoy space used: {Math.Round(usedSpace / 1024.0 / 1024.0, 3)}MB");
                    report.Add($"Maximum CHR size: {maxChrSize / 1024}KB");
                    report.Add($"Battery-backed games: {saveId}");

                    // Print some stats
                    Console.WriteLine($"Total games: {sortedGames.Count() - hiddenCount}");
                    Console.WriteLine($"Final ROM size: {Math.Round(usedSpace / 1024.0 / 1024.0, 3)}MB");
                    Console.WriteLine($"Maximum CHR size: {maxChrSize / 1024}KB");
                    Console.WriteLine($"Battery-backed games: {saveId}");

                    // Write report file if need
                    if (config.ReportFile != null)
                        File.WriteAllLines(config.ReportFile, report.ToArray());

                    if (games.Count - hiddenCount == 0)
                        throw new InvalidOperationException("Games list is empty");

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

                    // Error collection
                    var problems = new List<Exception>();

                    if ((notFittedSize > 0) && (usedSpace > config.MaxRomSizeMB * 1024 * 1024))
                        problems.Add(new OutOfMemoryException($"ROM is too big: {Math.Round(usedSpace / 1024.0 / 1024.0, 3)}MB"));
                    if (games.Count > MAX_GAME_COUNT)
                        problems.Add(new InvalidDataException($"Too many ROMs: {games.Count} (maximum {MAX_GAME_COUNT})"));
                    if (saveId > MAX_SAVE_COUNT)
                        problems.Add(new InvalidDataException($"Too many battery backed games: {saveId} (maximum {byte.MaxValue})"));

                    int c = 0;
                    foreach (var game in sortedGames)
                    {
                        Mapper? mapperInfo;
                        if (!string.IsNullOrEmpty(game.Mapper))
                        {
                            if (!mappers.TryGetValue(game.Mapper, out mapperInfo))
                            {
                                problems.Add(new NotSupportedException($"Unknown mapper \"{game.Mapper}\" for \"{Path.GetFileName(game.FileName)}\""));
                                continue;
                            }
                        }
                        else mapperInfo = new Mapper();
                        if (game.CHR.Length > config.MaxChrRamSizeKB * 1024)
                        {
                            problems.Add(new InvalidDataException($"CHR size is too big in \"{Path.GetFileName(game.FileName)}\""));
                            continue;
                        }
                        if ((game.Mirroring == MirroringType.FourScreenVram) && (game.CHR.Length > config.MaxChrRamSizeKB * 1024 - 0x1000))
                        {
                            problems.Add(new InvalidDataException($"Four-screen mode and such big CHR ({config.MaxChrRamSizeKB}KB) is not supported for \"{Path.GetFileName(game.FileName)}\""));
                            continue;
                        }
                        if (game.Trained)
                        {
                            problems.Add(new NotImplementedException($"Trained games are not supported for \"{game.FileName}\""));
                            continue;
                        }

                        bool prgRamEnabled;
                        var flags = mapperInfo.Flags;

                        // Some unusual games
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
                                problems.Add(new NotImplementedException($"32KB of PRG RAM is not supported for \"{Path.GetFileName(game.FileName)}\""));
                                continue;
                            default:
                                problems.Add(new NotImplementedException($"Weird PRG RAM value {game.PrgRamSize} for \"{Path.GetFileName(game.FileName)}\""));
                                continue;
                        }
                        if ((game.Flags & Game.GameFlags.WillNotWorkOnDendy) != 0)
                            Console.WriteLine($"WARNING! \"{Path.GetFileName(game.FileName)}\" is not compatible with Dendy");
                        if ((game.Flags & Game.GameFlags.WillNotWorkOnNtsc) != 0)
                            Console.WriteLine($"WARNING! \"{Path.GetFileName(game.FileName)}\" is not compatible with NTSC consoles");
                        if ((game.Flags & Game.GameFlags.WillNotWorkOnPal) != 0)
                            Console.WriteLine($"WARNING! \"{Path.GetFileName(game.FileName)}\" is not compatible with PAL consoles");
                        if ((game.Flags & Game.GameFlags.WillNotWorkOnNewFamiclone) != 0)
                            Console.WriteLine($"WARNING! \"{Path.GetFileName(game.FileName)}\" is not compatible with new Famiclones");

                        int chrBankingSize = game.CHR.Length;
                        // if using CHR RAM...
                        if (chrBankingSize == 0)
                        {
                            if (!game.ChrRamSize.HasValue)
                            {
                                // CHR RAM size is unknown
                                // if CHR RAM banking is supported by the mapper
                                // set the maximum size
                                if (mapperInfo.ChrRamBanking)
                                    chrBankingSize = (int)config.MaxChrRamSizeKB * 1024;
                                else // else banking is disabled
                                    chrBankingSize = 0x2000;
                            }
                            else
                            {
                                // CHR RAM size is specified by NES 2.0 header or fixes.json file
                                chrBankingSize = game.ChrRamSize.Value;
                            }
                        }
                        int prgMask = ~(game.PRG.Length / 0x4000 - 1);
                        int chrMask = ~(chrBankingSize / 0x2000 - 1);

                        byte @params = 0;
                        if (prgRamEnabled || game.Battery) @params |= (1 << 0); // enable SRAM
                        if (game.CHR.Length == 0) @params |= (1 << 1); // enable CHR write
                        if (game.Mirroring == MirroringType.Horizontal) @params |= (1 << 3); // default mirroring
                        if (game.Mirroring == MirroringType.FourScreenVram) @params |= (1 << 5); // four-screen mirroring
                        @params |= (1 << 7); // lockout

                        // TODO: replace magic numbers
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
                        regs["chr_count"].Add(string.Format("${0:X2}", game.CHR.Length / 0x2000));
                        regs["game_save"].Add(string.Format("${0:X2}", !game.Battery ? 0 : game.SaveId));
                        regs["game_flags"].Add(string.Format("${0:X2}", (byte)game.Flags));
                        regs["cursor_pos"].Add(string.Format("${0:X2}", game.ToString().Length));
                    }

                    // Handle collected errors
                    if (problems.Any()) throw new AggregateException(problems);

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
                            if (baseBank + c / 256 * 2 + 1 >= 62) throw new OutOfMemoryException("Bank overflow! Too many games?");
                            asmResult.AppendLine("  .org $A000");
                        }
                        asmResult.AppendLine("; " + Path.GetFileName(game.FileName));
                        asmResult.AppendLine("game_name_" + c + ":");
                        var name = StringToTiles(game.MenuName, symbols);
                        var asm = BytesToAsm(name);
                        asmResult.Append(asm);
                        c++;
                    }

                    // Some strings
                    // TODO: replace magic strings with constants
                    asmResult.AppendLine();
                    asmResult.AppendLine();
                    asmResult.AppendLine("; Some strings");
                    asmResult.AppendLine("  .bank 14");
                    asmResult.AppendLine("  .org $C800");
                    asmResult.AppendLine();
                    asmResult.AppendLine("string_file:");
                    asmResult.Append(BytesToAsm(StringToTiles("FILE: " + Path.GetFileName(config.GamesFile), symbols)));
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
                    for (int i = 0; i <= 10; i++)
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
                    asmResult.AppendLine("string_version:");
                    asmResult.Append(BytesToAsm(StringToTiles($"VERSION: {Assembly.GetExecutingAssembly()?.GetName()?.Version?.Major}.{Assembly.GetExecutingAssembly()?.GetName()?.Version?.Minor}", symbols)));
                    asmResult.AppendLine("string_commit:");
                    asmResult.Append(BytesToAsm(StringToTiles($"COMMIT: {Properties.Resources.gitCommit}", symbols)));

                    switch (config.Language)
                    {
                        case Config.CombinerLanguage.English:
                            asmResult.AppendLine("string_saving:");
                            asmResult.Append(BytesToAsm(StringToTiles("   SAVING... DON'T TURN OFF!    ", symbols)));
                            asmResult.AppendLine("string_incompatible_console:");
                            asmResult.Append(BytesToAsm(StringToTiles("    SORRY,  THIS GAME IS NOT      COMPATIBLE WITH THIS CONSOLE                                          PRESS ANY BUTTON        ", symbols)));
                            break;
                        case Config.CombinerLanguage.Russian:
                            asmResult.AppendLine("string_saving:");
                            asmResult.Append(BytesToAsm(StringToTiles("  СОХРАНЯЕМСЯ... НЕ ВЫКЛЮЧАЙ!   ", symbols)));
                            asmResult.AppendLine("string_incompatible_console:");
                            asmResult.Append(BytesToAsm(StringToTiles("     ИЗВИНИТЕ,  ДАННАЯ ИГРА       НЕСОВМЕСТИМА С ЭТОЙ КОНСОЛЬЮ                                        НАЖМИТЕ ЛЮБУЮ КНОПКУ      ", symbols)));
                            break;
                    }

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

                    File.WriteAllText(config.AsmFile!, asmResult.ToString());

                    if (config.Command == Config.CombinerCommand.Prepare)
                    {
                        var offsets = new Offsets();
                        offsets.Size = romSize;
                        offsets.RomCount = gameCount;
                        offsets.GamesFile = Path.GetFileName(config.GamesFile);
                        offsets.Games = sortedGames.Where(g => !g.IsSeparator).ToArray();
                        File.WriteAllText(config.OffsetsFile!, JsonSerializer.Serialize(offsets, jsonOptions));
                    }

                    if (config.Command == Config.CombinerCommand.Build)
                    {
                        Console.Write("Compiling using nesasm... ");
                        if (romSize < result.Length) Array.Resize(ref result, romSize);
                        var process = new Process();
                        var cp866 = CodePagesEncodingProvider.Instance.GetEncoding(866) ?? Encoding.ASCII;
                        process.StartInfo.FileName = config.NesAsm;
                        process.StartInfo.Arguments = $"\"menu.asm\" -r -o - -C \"GAMES_DB={config.AsmFile}\" " + config.NesAsmArgs;
                        process.StartInfo.WorkingDirectory = config.SourcesDir;
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
                        Array.Copy(loader, 0, result, LOADER_OFFSET, loader.Length);
                        Console.WriteLine("OK");
                    }
                }

                // Step two (in case of separate steps): load ROMs, menu ROM and merge them in one multirom
                if (config.Command == Config.CombinerCommand.Combine) // Combine
                {
                    var offsetsJson = File.ReadAllText(config.OffsetsFile);
                    var offsets = JsonSerializer.Deserialize<Offsets>(offsetsJson, jsonOptions);
                    if (offsets == null) throw new InvalidDataException("Can't load offsets file");
                    // Use 0xFF as empty value because it doesn't require writing to flash
                    result = Enumerable.Repeat(byte.MaxValue, offsets.Size).ToArray();

                    Console.Write("Loading loader... ");
                    var loaderFile = new NesFile(config.LoaderFile!);
                    var loader = loaderFile.PRG.ToArray();
                    Array.Copy(loader, 0, result, LOADER_OFFSET, loader.Length);
                    Console.WriteLine("OK.");

                    foreach (var game in offsets.Games ?? Array.Empty<Game>())
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
                                        var prg = unifFile.Where(k => k.Key.StartsWith("PRG")).OrderBy(k => k.Key).SelectMany(i => i.Value).ToArray();
                                        var chr = unifFile.Where(k => k.Key.StartsWith("CHR")).OrderBy(k => k.Key).SelectMany(i => i.Value).ToArray();
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

                // Step three: save result
                if ((config.Command == Config.CombinerCommand.Combine) || (config.Command == Config.CombinerCommand.Build)) // Combine or build
                {
                    if (!string.IsNullOrEmpty(config.UnifFile))
                    {
                        Console.Write("Saving UNIF file... ");
                        var u = new UnifFile();
                        u.Version = 5;
                        u.Mapper = "COOLGIRL";
                        u.Mirroring = MirroringType.MapperControlled;
                        u.PRG0 = result!;
                        u.Battery = true;
                        u.Save(config.UnifFile);
                        Console.WriteLine("OK");
                    }
                    if (!string.IsNullOrEmpty(config.Nes20File))
                    {
                        Console.Write("Saving NES file... ");
                        var nes = new NesFile();
                        nes.Version = NesFile.iNesVersion.NES20;
                        nes.PRG = result!;
                        nes.CHR = Array.Empty<byte>();
                        nes.Mapper = 342;
                        nes.PrgNvRamSize = 32 * 1024;
                        nes.ChrRamSize = config.MaxChrRamSizeKB * 1024;
                        nes.Battery = true;
                        nes.Save(config.Nes20File);
                        Console.WriteLine("OK");
                    }
                    if (!string.IsNullOrEmpty(config.BinFile))
                    {
                        Console.Write("Saving BIN file... ");
                        File.WriteAllBytes(config.BinFile, result!);
                        Console.WriteLine("OK");
                    }
                }
                Console.WriteLine("Done.");
            }
            catch (AggregateException ae)
            {
                if (ae.InnerExceptions.Count > 1)
                    Console.WriteLine($"{ae.InnerExceptions.Count} errors.");
                foreach (var ex in ae.InnerExceptions)
                {
#if DEBUG
                    Console.WriteLine($"Error {ex.GetType()}: {ex.Message}{ex.StackTrace}");
#else
                    Console.WriteLine($"Error: {ex.Message}");
#endif
                }
                return 2;
            }
            catch (Exception ex)
            {
#if DEBUG
                Console.WriteLine($"Error {ex.GetType()}: {ex.Message}{ex.StackTrace}");
#else
                Console.WriteLine($"Error: {ex.Message}");
#endif
                return 2;
            }
            return 0;
        }

        static bool WillFit(byte[] dest, int pos, byte[] source, HashSet<int> badSectors)
        {
            for (int addr = pos; addr < pos + source.Length; addr++)
            {
                if (addr % 0x2000 == 0)
                {
                    if ((addr >= LOADER_OFFSET) && (addr < LOADER_OFFSET + LOADER_SIZE))
                        return false;
                    if ((badSectors != null) && badSectors.Contains(addr / FLASH_SECTOR_SIZE))
                        return false;
                }
                if (addr >= dest.Length)
                    return false;
                if ((dest[addr] != byte.MaxValue) && (dest[addr] != source[addr - pos]))
                    return false;
            }
            return true;
        }

        static byte[] StringToTiles(string text, Dictionary<char, byte> symbolTable)
        {
            text = text.ToUpper();
            var result = new byte[text.Length + 1];
            for (int c = 0; c < result.Length; c++)
            {
                if (c < text.Length)
                {
                    byte charCode;
                    if (symbolTable.TryGetValue(text[c], out charCode))
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
            if (string.IsNullOrEmpty(input)) return "";
            return input.First().ToString().ToUpper() + input[1..];
        }
    }
}
