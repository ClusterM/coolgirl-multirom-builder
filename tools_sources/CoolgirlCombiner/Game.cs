using com.clusterrr.Famicom.Containers;
using com.clusterrr.Famicom.Containers.HeaderFixer;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace com.clusterrr.Famicom.CoolGirl
{
    class Game
    {
        public enum NesContainerType { iNES, UNIF };

        [JsonProperty("file_name")]
        public string FileName { get; set; }

        [JsonProperty("menu_name")]
        public string MenuName { get; set; }

        [JsonIgnore]
        public readonly IEnumerable<byte> PRG = null;

        [JsonProperty("prg_size")]
        public uint PrgSize { get; set; }

        [JsonIgnore]
        public readonly IEnumerable<byte> CHR = null;

        [JsonProperty("prg_offset")]
        public uint PrgOffset;

        [JsonProperty("chr_size")]
        public uint ChrSize { get; set; }

        [JsonProperty("chr_offset")]
        public uint ChrOffset;

        [JsonIgnore]
        public uint? PrgRamSize { get; set; } = null;

        [JsonIgnore]
        public uint? ChrRamSize { get; set; } = null;

        [JsonProperty("mapper")]
        public string Mapper { get; set; }

        [JsonProperty("save_id")]
        public byte SaveId { get; set; }

        [JsonIgnore]
        public GameFlags Flags { get; set; }

        [JsonProperty("battery")]
        public bool Battery { get; set; }

        [JsonIgnore]
        public NesFile.MirroringType Mirroring { get; set; }

        [JsonProperty("container_type")]
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
                            PrgRamSize = fix.PrgRamSize * 1024;
                        if (fix.ChrRamSize.HasValue)
                            ChrRamSize = fix.ChrRamSize * 1024;
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
                        Array.Copy(CHR.ToArray(), newChr, ChrSize);
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
