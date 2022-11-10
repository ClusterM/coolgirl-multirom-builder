using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.clusterrr.Famicom.CoolGirl
{
    internal class Config
    {
        const string DEFAULT_MAPPERS_FILE = @"coolgirl-mappers.json";
        const string DEFAULT_FIXES_FILE = @"coolgirl-fixes.json";
        const string DEFAULT_SYMBOLS_FILE = @"coolgirl-symbols.json";
        public const string commandPrepare = "prepare";
        public const string commandCombine = "combine";
        public const string commandBuild = "build";

        public enum CombinerCommand
        {
            Prepare,
            Combine,
            Build,
        }

        public enum CombinerLanguage
        {
            English,
            Russian
        }

        public CombinerCommand Command { get; private set; } = CombinerCommand.Prepare;
        public string MappersFile { get; private set; }
        public string FixesFile { get; private set; }
        public string SymbolsFile { get; private set; }
        public string NesAsm { get; private set; } = "nesasm";
        public string NesAsmArgs { get; private set; } = "";
        public string SourcesDir { get; private set; } = ".";
        public string? GamesFile { get; private set; } = null;
        public string? AsmFile { get; private set; } = null;
        public string OffsetsFile { get; private set; } = "offsets.json";
        public string? ReportFile { get; private set; } = null;
        public string? LoaderFile { get; private set; } = null;
        public string? UnifFile { get; private set; } = null;
        public string? Nes20File { get; private set; } = null;
        public string? BinFile { get; private set; } = null;
        public CombinerLanguage Language { get; private set; } = CombinerLanguage.English;
        public HashSet<int> BadSectors { get; private set; } = new();
        public bool NoSort { get; private set; } = false;
        public uint MaxRomSizeMB { get; private set; } = 256;
        public uint MaxChrRamSizeKB { get; private set; } = 256;

        private Config()
        {
            MappersFile = Path.Combine(Path.GetDirectoryName(AppContext.BaseDirectory) ?? String.Empty, DEFAULT_MAPPERS_FILE);
            if (!File.Exists(MappersFile) && !OperatingSystem.IsWindows())
                MappersFile = Path.Combine("/etc", DEFAULT_MAPPERS_FILE);
            FixesFile = Path.Combine(Path.GetDirectoryName(AppContext.BaseDirectory) ?? String.Empty, DEFAULT_FIXES_FILE);
            if (!File.Exists(FixesFile) && !OperatingSystem.IsWindows())
                FixesFile = Path.Combine("/etc", DEFAULT_FIXES_FILE);
            SymbolsFile
             = Path.Combine(Path.GetDirectoryName(AppContext.BaseDirectory) ?? String.Empty, DEFAULT_SYMBOLS_FILE);
            if (!File.Exists(SymbolsFile) && !OperatingSystem.IsWindows())
                SymbolsFile = Path.Combine("/etc", DEFAULT_SYMBOLS_FILE);
        }

        // TODO: Replace magic strings with constants
        public static Config? Parse(string[] args)
        {
            Config config = new Config();

            if (args.Length > 0)
            {
                string command = args[0].ToLower();
                switch (command)
                {
                    case commandPrepare:
                        config.Command = CombinerCommand.Prepare;
                        break;
                    case commandCombine:
                        config.Command = CombinerCommand.Combine;
                        break;
                    case commandBuild:
                        config.Command = CombinerCommand.Build;
                        break;
                    default:
                        if (!string.IsNullOrEmpty(command))
                            Console.WriteLine("Unknown command: " + command);
                        return null;
                }
            }
            else return null;
            for (int i = 1; i < args.Length; i++)
            {
                string param = args[i];
                while (param.StartsWith("-")) param = param.Substring(1);
                string value = i < args.Length - 1 ? args[i + 1] : "";
                switch (param.ToLower())
                {
                    case "mappers":
                        config.MappersFile = value;
                        i++;
                        break;
                    case "fixes":
                        config.FixesFile = value;
                        i++;
                        break;
                    case "symbols":
                        config.SymbolsFile = value;
                        i++;
                        break;
                    case "games":
                        config.GamesFile = value;
                        i++;
                        break;
                    case "asm":
                        config.AsmFile = value;
                        i++;
                        break;
                    case "offsets":
                        config.OffsetsFile = value;
                        i++;
                        break;
                    case "report":
                        config.ReportFile = value;
                        i++;
                        break;
                    case "loader":
                        config.LoaderFile = value;
                        i++;
                        break;
                    case "unif":
                        config.UnifFile = value;
                        i++;
                        break;
                    case "nes20":
                        config.Nes20File = value;
                        i++;
                        break;
                    case "bin":
                        config.BinFile = value;
                        i++;
                        break;
                    case "nosort":
                        config.NoSort = true;
                        break;
                    case "maxromsize":
                        config.MaxRomSizeMB = uint.Parse(value);
                        i++;
                        break;
                    case "maxchrsize":
                        config.MaxChrRamSizeKB = uint.Parse(value);
                        i++;
                        break;
                    case "language":
                        switch (value.ToLower())
                        {
                            case "eng":
                                config.Language = CombinerLanguage.English;
                                break;
                            case "rus":
                                config.Language = CombinerLanguage.Russian;
                                break;
                            default:
                                throw new InvalidDataException($"Invalid language: {value}");
                        }
                        i++;
                        break;
                    case "badsectors":
                        foreach (var v in value.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                            config.BadSectors.Add(int.Parse(v));
                        i++;
                        break;
                    case "nesasm":
                        config.NesAsm = value;
                        i++;
                        break;
                    case "nesasm-args":
                    case "nesasmargs":
                        config.NesAsmArgs = value;
                        i++;
                        break;
                    case "sources":
                        config.SourcesDir = value;
                        i++;
                        break;
                    default:
                        Console.WriteLine("Unknown parameter: " + param);
                        return null;
                }
            }

            if ((config.GamesFile == null) && ((config.Command == CombinerCommand.Prepare) || (config.Command == CombinerCommand.Build)))
            {
                Console.WriteLine("Missing required parameter: --games");
                return null;
            }
            if ((config.AsmFile == null) && (config.Command == CombinerCommand.Prepare))
            {
                Console.WriteLine("Missing required parameter: --asm");
                return null;
            }
            if ((config.OffsetsFile == null) && ((config.Command == CombinerCommand.Prepare) || (config.Command == CombinerCommand.Combine)))
            {
                Console.WriteLine("Missing required parameter: --offsets");
                return null;
            }
            if ((config.LoaderFile == null) && (config.Command == CombinerCommand.Combine))
            {
                Console.WriteLine("Missing required parameter: --loader");
                return null;
            }
            if ((config.UnifFile == null) && (config.Nes20File == null) && (config.BinFile == null)
                && ((config.Command == CombinerCommand.Combine) || (config.Command == CombinerCommand.Build)))
            {
                Console.WriteLine("At least one parameter required: --unif, --nes20 or --bin");
                return null;
            }

            if (string.IsNullOrEmpty(config.AsmFile))
                config.AsmFile = Path.Combine(config.SourcesDir, "games.asm");

            return config;
        }

        public static void PrintHelp()
        {
            var exename = Path.GetFileName(Process.GetCurrentProcess()?.MainModule?.FileName);
            Console.WriteLine("--- Usage ---");
            Console.WriteLine("First step:");
            Console.WriteLine($" {exename} prepare --games <games.txt> --asm <games.asm> --offsets <offsets.json> [--report <report.txt>] [--nosort] [--maxromsize <size_mb>] [--maxchrsize <size_kb>] [--language <eng|rus>] [--badsectors <sectors>]");
            Console.WriteLine("  {0,-20}{1}", "--games", "- input plain text file with a list of ROM files");
            Console.WriteLine("  {0,-20}{1}", "--asm", "- output file for the loader");
            Console.WriteLine("  {0,-20}{1}", "--offsets", "- output file with offsets for every game");
            Console.WriteLine("  {0,-20}{1}", "--report", "- output report file (human readable)");
            Console.WriteLine("  {0,-20}{1}", "--nosort", "- disable automatic sort by name");
            Console.WriteLine("  {0,-20}{1}", "--maxromsize", "- maximum size for final file (in megabytes)");
            Console.WriteLine("  {0,-20}{1}", "--maxchrsize", "- maximum CHR RAM size (in kilobytes), default is 256");
            Console.WriteLine("  {0,-20}{1}", "--language", "- language for system messages: \"eng\" or \"rus\"");
            Console.WriteLine("  {0,-20}{1}", "--badsectors", "- comma-separated list of bad sectors,");
            Console.WriteLine("Second step:");
            Console.WriteLine($" {exename} combine --loader <menu.nes> --offsets <offsets.json> [--unif <multirom.unf>] [--nes20 multirom.nes] [--bin <multirom.bin>]");
            Console.WriteLine("  {0,-20}{1}", "--loader", "- loader (compiled using the asm file generated by the first step)");
            Console.WriteLine("  {0,-20}{1}", "--offsets", "- input file with offsets for every game (generated by the first step)");
            Console.WriteLine("  {0,-20}{1}", "--unif", "- output UNIF file");
            Console.WriteLine("  {0,-20}{1}", "--nes20", "- output NES 2.0 file");
            Console.WriteLine("  {0,-20}{1}", "--bin", "- output raw binary file");
            Console.WriteLine("All at once:");
            Console.WriteLine($" {exename} build --games <games.txt> --asm <games.asm> [--nesasm <nesasm>] [--nesasm-args <args>] [--sources <path>] [--report <report.txt>] [--nosort] [--maxromsize <size_mb>] [--maxchrsize <size_kb>] [--language <language>] [--badsectors <sectors>] [--unif <multirom.unf>] [--nes20 <multirom.nes>] [--bin <multirom.bin>]");
            Console.WriteLine("  {0,-20}{1}", "--games", "- input plain text file with list of ROM files");
            Console.WriteLine("  {0,-20}{1}", "--asm", "- output file for the loader");
            Console.WriteLine("  {0,-20}{1}", "--nesasm", "- path to the nesasm compiler executable");
            Console.WriteLine("  {0,-20}{1}", "--nesasm-args", "- additional command-line arguments for nesasm");
            Console.WriteLine("  {0,-20}{1}", "--sources", "- directory with loader source files, default is current directory");
            Console.WriteLine("  {0,-20}{1}", "--report", "- output report file (human readable)");
            Console.WriteLine("  {0,-20}{1}", "--nosort", "- disable automatic sort by name");
            Console.WriteLine("  {0,-20}{1}", "--maxromsize", "- maximum size for final file (in megabytes)");
            Console.WriteLine("  {0,-20}{1}", "--maxchrsize", "- maximum CHR RAM size (in kilobytes), default is 256");
            Console.WriteLine("  {0,-20}{1}", "--language", "- language for system messages: \"eng\" or \"rus\", default is \"eng\"");
            Console.WriteLine("  {0,-20}{1}", "--badsectors", "- comma-separated list of bad sectors,");
            Console.WriteLine("  {0,-20}{1}", "--unif", "- output UNIF file");
            Console.WriteLine("  {0,-20}{1}", "--nes20", "- output NES 2.0 file");
            Console.WriteLine("  {0,-20}{1}", "--bin", "- output raw binary file");
        }
    }
}
