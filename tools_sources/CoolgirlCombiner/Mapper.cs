using System.Text.Json.Serialization;

namespace com.clusterrr.Famicom.Multirom
{
    class Mapper
    {
        [JsonPropertyName("description")]
        public string? Description { get; set; }
        [JsonPropertyName("mapper_register")]
        public byte MapperRegister { get; set; } = 0;
        [JsonPropertyName("prg_mode")]
        public byte PrgMode { get; set; } = 0;
        [JsonPropertyName("chr_mode")]
        public byte ChrMode { get; set; } = 0;
        [JsonPropertyName("flags")]
        public byte Flags { get; set; } = 0;
        [JsonPropertyName("flags_for_16k_prg_ram")]
        public byte Flags16kPrgRam { get; set; } = 0;
        [JsonPropertyName("flags_for_32k_prg_ram")]
        public byte Flags32kPrgRam { get; set; } = 0;
        [JsonPropertyName("prg_ram_enabled")]
        public bool PrgRamEnabled { get; set; } = false;
        [JsonPropertyName("prg_bank_a")]
        public byte PrgBankA { get; set; } = 0;
        [JsonPropertyName("chr_ram_banking")]
        public bool ChrRamBanking { get; set; } = false;
    }
}
