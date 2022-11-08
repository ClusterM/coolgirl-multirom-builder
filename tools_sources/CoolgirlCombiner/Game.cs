﻿using com.clusterrr.Famicom.Containers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Text.Json.Serialization;

namespace com.clusterrr.Famicom.CoolGirl
{
    class Game
    {
        public enum NesContainerType { iNES = 1, UNIF = 2 };

        [JsonPropertyName("file_name")]
        public string FileName { get; set; } = String.Empty;

        [JsonPropertyName("menu_name")]
        public string MenuName { get; set; } = String.Empty;

        [JsonIgnore]
        public readonly byte[] PRG = Array.Empty<byte>();

        [JsonPropertyName("prg_offset")]
        public int PrgOffset { get; set; } = 0;

        [JsonIgnore]
        public readonly byte[] CHR = Array.Empty<byte>();

        [JsonPropertyName("chr_offset")]
        public int ChrOffset { get; set; } = 0;

        [JsonIgnore]
        public int? PrgRamSize { get; set; } = null;

        [JsonIgnore]
        public int? ChrRamSize { get; set; } = null;

        [JsonPropertyName("mapper")]
        public string Mapper { get; set; } = String.Empty;

        [JsonPropertyName("save_id")]
        public byte SaveId { get; set; }

        [JsonIgnore]
        public GameFlags Flags { get; set; }

        [JsonPropertyName("battery")]
        public bool Battery { get; set; }

        [JsonIgnore]
        public MirroringType Mirroring { get; set; }

        [JsonPropertyName("trained")]
        public bool Trained { get; set; }

        [JsonPropertyName("container_type")]
        public NesContainerType ContainerType { get; set; }

        [Flags]
        public enum GameFlags
        {
            WillNotWorkOnNtsc = 0x01,
            WillNotWorkOnPal = 0x02,
            WillNotWorkOnDendy = 0x04,
            WillNotWorkOnNewFamiclone = 0x08,
            Hidden = 0x10,
            Separator = 0x80,
        };

        public Game()
        {
        }

        public Game(string fileName, string? menuName = null, Dictionary<string, GameFix>? fixes = null)
        {
            // Separators
            if (fileName == "-")
            {
                MenuName = (string.IsNullOrWhiteSpace(menuName) || menuName == "-") ? "" : menuName;
                FileName = "";
                Flags |= GameFlags.Separator;
            }
            else
            {
                Console.WriteLine($"Loading {Path.GetFileName(fileName)}...");
                FileName = fileName;
                if (string.IsNullOrWhiteSpace(menuName))
                {
                    // Menu name based on filename
                    MenuName = Limit(Regex.Replace(Path.GetFileNameWithoutExtension(fileName), @"( ?\[.*?\])|( \(.\))", string.Empty).Replace("_", " ").ToUpper().Replace(", THE", "").Trim());
                }
                else
                {
                    MenuName = Limit(menuName.Trim());
                    if (MenuName == "?") Flags |= GameFlags.Hidden;
                }
                string crc32;
                string md5;
                try
                {
                    var nesFile = new NesFile(fileName);
                    PRG = nesFile.PRG;
                    CHR = nesFile.CHR;
                    Battery = nesFile.Battery;
                    Mapper = $"{nesFile.Mapper:D3}" + ((nesFile.Submapper > 0) ? $":{nesFile.Submapper}" : "");
                    Mirroring = nesFile.Mirroring;
                    ContainerType = NesContainerType.iNES;
                    Trained = nesFile.Trainer != null && nesFile.Trainer.Length > 0;
                    if (nesFile.Version == NesFile.iNesVersion.NES20)
                    {
                        PrgRamSize = (int?)(nesFile.PrgRamSize + nesFile.PrgNvRamSize);
                        ChrRamSize = (int?)(nesFile.ChrRamSize + nesFile.ChrNvRamSize);
                    }
                    crc32 = $"{nesFile.CalculateCRC32():x08}";
                    var md5full = nesFile.CalculateMD5();
                    md5 = $"{md5full[8]:x02}{md5full[9]:x02}{md5full[10]:x02}{md5full[11]:x02}{md5full[12]:x02}{md5full[13]:x02}{md5full[14]:x02}{md5full[15]:x02}"; // lower 8 bytes of MD5
                }
                catch (InvalidDataException)
                {
                    var unifFile = new UnifFile(fileName);
                    PRG = unifFile.Where(k => k.Key.StartsWith("PRG")).OrderBy(k => k.Key).SelectMany(i => i.Value).ToArray();
                    CHR = unifFile.Where(k => k.Key.StartsWith("CHR")).OrderBy(k => k.Key).SelectMany(i => i.Value).ToArray();
                    Battery = unifFile.Battery ?? false;
                    var mapper = unifFile.Mapper;
                    if (string.IsNullOrEmpty(mapper))
                        throw new InvalidDataException($"Mapper is not set in {Path.GetFileName(fileName)}");
                    if (mapper.StartsWith("NES-") || mapper.StartsWith("UNL-") || mapper.StartsWith("HVC-") || mapper.StartsWith("BTL-") || mapper.StartsWith("BMC-"))
                        mapper = mapper.Substring(4);
                    Mapper = mapper;
                    Mirroring = unifFile.Mirroring ?? MirroringType.MapperControlled;
                    ContainerType = NesContainerType.UNIF;
                    crc32 = $"{unifFile.CalculateCRC32():x08}";
                    var md5full = unifFile.CalculateMD5();
                    md5 = $"{md5full[8]:x02}{md5full[9]:x02}{md5full[10]:x02}{md5full[11]:x02}{md5full[12]:x02}{md5full[13]:x02}{md5full[14]:x02}{md5full[15]:x02}"; // lower 8 bytes of MD5
                }
                // Check for fixes database
                if (fixes != null)
                {
                    GameFix? fix;
                    if (fixes.TryGetValue(crc32, out fix) || fixes.TryGetValue(md5, out fix))
                    {
                        if (!string.IsNullOrEmpty(fix.Mapper) && Mapper != fix.Mapper)
                        {
                            Mapper = fix.Mapper;
                            Console.WriteLine($"Fix based on checksum: {Path.GetFileName(fileName)} has {fix.Mapper} mapper");
                        }
                        if (!string.IsNullOrEmpty(fix.Mirroring) && (Mapper.ToString() != fix.Mirroring))
                        {
                            Mirroring = Enum.Parse<MirroringType>(fix.Mirroring);
                            Console.WriteLine($"Fix based on checksum: {Path.GetFileName(fileName)} has {fix.Mirroring} mirroring type");
                        }
                        if (fix.PrgRamSize.HasValue && (PrgRamSize != fix.PrgRamSize * 1024))
                        {
                            PrgRamSize = (int?)(fix.PrgRamSize * 1024);
                            Console.WriteLine($"Fix based on checksum: {Path.GetFileName(fileName)} has {fix.PrgRamSize}KB PRG RAM");
                        }
                        if (fix.ChrRamSize.HasValue && (ChrRamSize != fix.ChrRamSize * 1024))
                        {
                            ChrRamSize = (int?)(fix.ChrRamSize * 1024);
                            Console.WriteLine($"Fix based on checksum: {Path.GetFileName(fileName)} has {fix.ChrRamSize}KB CHR RAM");
                        }
                        if (fix.Battery.HasValue && (Battery != fix.Battery.Value))
                        {
                            Battery = fix.Battery.Value;
                            Console.WriteLine($"Fix based on checksum: {Path.GetFileName(fileName)} battery saves = {fix.Battery}");
                        }
                        if (fix.WillNotWorkOnPal)
                        {
                            Flags |= GameFlags.WillNotWorkOnPal;
                        }
                        if (fix.WillNotWorkOnNtsc)
                        {
                            Flags |= GameFlags.WillNotWorkOnNtsc;
                        }
                        if (fix.WillNotWorkOnDendy)
                        {
                            Flags |= GameFlags.WillNotWorkOnDendy;
                        }
                        if (fix.WillNotWorkOnNewFamiclone)
                        {
                            Flags |= GameFlags.WillNotWorkOnNewFamiclone;
                        }
                    }
                }
                // External NTRAM is not supported on new famiclones
                if (Mirroring == MirroringType.FourScreenVram)
                    Flags |= GameFlags.WillNotWorkOnNewFamiclone;
                // Check for round sizes and add padding
                if (PRG.Length > 0)
                {
                    uint roundSize = 1;
                    while (roundSize < PRG.Length)
                        roundSize <<= 1;
                    if (roundSize > PRG.Length)
                    {
                        PRG = Enumerable.Concat(PRG, Enumerable.Repeat(byte.MaxValue, (int)(roundSize - PRG.Length))).ToArray();
                    }
                }
                if (CHR.Length > 0)
                {
                    uint roundSize = 1;
                    while (roundSize < CHR.Length)
                        roundSize <<= 1;
                    if (roundSize > CHR.Length)
                        CHR = Enumerable.Concat(CHR, Enumerable.Repeat(byte.MaxValue, (int)(roundSize - CHR.Length))).ToArray();
                }
            }
        }

        public bool IsHidden { get => (Flags & GameFlags.Hidden) != 0; }

        public bool IsSeparator { get => (Flags & GameFlags.Separator) != 0; }

        public override string ToString() => MenuName;

        public static string Limit(string name)
        {
            name = name.Trim();
            if (name.Length > 29) name = name.Substring(0, 26).Trim() + "...";
            return name.Trim();
        }
    }
}
