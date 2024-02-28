using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TuoTempo.Models
{
    public class Location
    {
        public string location_lid { get; set; }
        public string name { get; set; }
        public address address { get; set; }
        public Contact contact { get; set; }
        public string notice { get; set; }
        public bool web_enabled { get; set; }
        public bool active { get; set; }
    }
}