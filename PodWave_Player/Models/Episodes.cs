using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PodWave_Player.Models
{
    public class Episode
    {
        public string Title { get; set; }
        public string AudioUrl { get; set; }
        public int DurationInSeconds { get; set; }
    }

}
