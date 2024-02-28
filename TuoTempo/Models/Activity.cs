using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TuoTempo.Models
{
    public class Activity
    {
        public string activity_lid { get; set; }
        public string name { get; set; }
        public group group { get; set; }
        public string type { get; set; }
        public string duration { get; set; }
        public string preparation { get; set; }
        public related related { get; set; }
        public bool web_enabled { get; set; }
        public bool active { get; set; }
    }
}