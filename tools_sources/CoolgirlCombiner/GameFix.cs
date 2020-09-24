using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.clusterrr.Famicom.CoolGirl
{
    class GameFix
    {
        [JsonProperty("description")]
        string Description { get; set; }

        [JsonProperty("battery")]
        bool? Battery { get; set; }

        [JsonProperty("prg_ram_size")]
        uint? PrgRamSize { get; set; }

        [JsonProperty("prg_nvram_size")]
        uint? PrgNvRamSize { get; set; }

        [JsonProperty("chr_ram_size")]
        uint? ChrRamSize { get; set; }

        [JsonProperty("chr_nvram_size")]
        uint? ChrNvRamSize { get; set; }
    }
}
