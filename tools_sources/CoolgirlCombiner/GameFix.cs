using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.clusterrr.Famicom.CoolGirl
{
    class GameFix
    {
        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("battery")]
        [DefaultValue(null)]
        public bool? Battery { get; set; }

        [JsonProperty("prg_ram_size")]
        [DefaultValue(null)]
        public uint? PrgRamSize { get; set; }

        //[JsonProperty("prg_nvram_size")]
        //public uint? PrgNvRamSize { get; set; }

        [JsonProperty("chr_ram_size")]
        [DefaultValue(null)]
        public uint? ChrRamSize { get; set; }

        //[JsonProperty("chr_nvram_size")]
        //public uint? ChrNvRamSize { get; set; }
        [JsonProperty("will_not_work_on_pal")]
        [DefaultValue(false)]
        public bool WillNotWorkOnPal { get; set; } = false;
        [JsonProperty("will_not_work_on_ntsc")]
        [DefaultValue(false)]
        public bool WillNotWorkOnNtsc { get; set; } = false;
        [JsonProperty("will_not_work_on_dendy")]
        [DefaultValue(false)]
        public bool WillNotWorkOnDendy { get; set; } = false;
        [JsonProperty("will_not_work_on_new_famiclone")]
        [DefaultValue(false)]
        public bool WillNotWorkOnNewFamiclone { get; set; } = false;
    }
}
