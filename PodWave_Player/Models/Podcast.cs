using System;
using System.Collections.Generic;

namespace PodWave_Player.Models
{
    public class Podcast
    {
        public string PodcastId { get; set; }
        public string TitleP { get; set; }
        public string DescriptionP { get; set; } //p for Podcast-specific description
        public string AudioUrl { get; set; }
        public string ImageUrl { get; set; }
        public List<Episode> Episodes { get; set; } = new();
    }

   
}
