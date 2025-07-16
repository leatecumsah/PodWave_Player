using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PodWave_Player.Models
{
    class Podcast
    {
        public string PodcastId { get; set; }
        public string Title { get; set; }
        public string DescriptionP { get; set; }
        public string AudioUrl { get; set; }
        public string ImageUrl { get; set; }
        public List<Episode> Episodes { get; set; } = new();
    }
}
