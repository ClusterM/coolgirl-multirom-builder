using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace com.clusterrr.Famicom.CoolGirl
{
    partial class Program
    {
        class Mapper
        {
            [JsonProperty("description")]
            public string Description { get; set; }
            [JsonProperty("mapper_register")]
            public byte MapperRegister { get; set; } = 0;
            [JsonProperty("prg_mode")]
            public byte PrgMode { get; set; } = 0;
            [JsonProperty("chr_mode")]
            public byte ChrMode { get; set; } = 0;
            [JsonProperty("flags")]
            public byte Flags { get; set; } = 0;
            [JsonProperty("flags_for_16k_prg_ram")]
            public byte Flags16kPrgRam { get; set; } = 0;
            [JsonProperty("flags_for_32k_prg_ram")]
            public byte Flags32kPrgRam { get; set; } = 0;
            [JsonProperty("prg_ram_enabled")]
            public bool PrgRamEnabled { get; set; } = false;
            [JsonProperty("prg_bank_a")]
            public byte PrgBankA { get; set; } = 0;
            [JsonProperty("chr_ram_banking")]
            public bool ChrRamBanking { get; set; } = false;
        }
    }
}
