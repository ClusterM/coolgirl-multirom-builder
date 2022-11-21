using System.ComponentModel;
using System.Text.Json.Serialization;

namespace com.clusterrr.Famicom.Multirom
{
    class GameFix
    {
        [JsonPropertyName("description")]
        public string? Description { get; set; }

        [JsonPropertyName("mapper")]
        [DefaultValue(null)]
        public string? Mapper { get; set; }

        [JsonPropertyName("mirroring")]
        [DefaultValue(null)]
        public string? Mirroring { get; set; }

        [JsonPropertyName("battery")]
        [DefaultValue(null)]
        public bool? Battery { get; set; }

        [JsonPropertyName("prg_ram_size")]
        [DefaultValue(null)]
        public uint? PrgRamSize { get; set; }

        //[JsonProperty("prg_nvram_size")]
        //public uint? PrgNvRamSize { get; set; }

        [JsonPropertyName("chr_ram_size")]
        [DefaultValue(null)]
        public uint? ChrRamSize { get; set; }

        //[JsonProperty("chr_nvram_size")]
        //public uint? ChrNvRamSize { get; set; }
        [JsonPropertyName("will_not_work_on_pal")]
        [DefaultValue(false)]
        public bool WillNotWorkOnPal { get; set; } = false;
        [JsonPropertyName("will_not_work_on_ntsc")]
        [DefaultValue(false)]
        public bool WillNotWorkOnNtsc { get; set; } = false;
        [JsonPropertyName("will_not_work_on_dendy")]
        [DefaultValue(false)]
        public bool WillNotWorkOnDendy { get; set; } = false;
        [JsonPropertyName("will_not_work_on_new_famiclone")]
        [DefaultValue(false)]
        public bool WillNotWorkOnNewFamiclone { get; set; } = false;
    }
}
