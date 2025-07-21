using System;

namespace PodWave_Player.Models
{
    public class Episode
    {
        public int EpisodeId { get; set; }
        public string TitleE { get; set; }
        public string DescriptionE { get; set; } //E for Episode-specific description
        public string AudioUrl { get; set; }
        public int DurationInSeconds { get; set; }
    }
}
