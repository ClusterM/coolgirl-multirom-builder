using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.clusterrr.Famicom.CoolGirl
{
    class Offsets
    {
        [JsonProperty("size")]
        public uint Size { get; set; }
        [JsonProperty("rom_count")]
        public uint RomCount { get; set; }
        [JsonProperty("games_file")]
        public string GamesFile { get; set; }
        [JsonProperty("games")]
        public Game[] Games { get; set; }
    }
}
