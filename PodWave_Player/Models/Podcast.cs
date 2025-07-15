using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PodWave_Player.Models
{
    class Podcast
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public string AudioUrl { get; set; }
        public List<Episode> Episodes { get; set; } = new();
    }
}
