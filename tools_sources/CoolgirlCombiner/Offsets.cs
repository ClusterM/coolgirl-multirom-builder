using System.Text.Json.Serialization;

namespace com.clusterrr.Famicom.CoolGirl
{
    class Offsets
    {
        [JsonPropertyName("size")]
        public int Size { get; set; }
        [JsonPropertyName("rom_count")]
        public int RomCount { get; set; }
        [JsonPropertyName("games_file")]
        public string? GamesFile { get; set; }
        [JsonPropertyName("games")]
        public Game[]? Games { get; set; }
    }
}
