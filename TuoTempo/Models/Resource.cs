using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TuoTempo.Models
{
    public class Resource
    {
        public string resource_lid { get; set; }
        public string name { get; set; }
        public id_number id_number { get; set; }
        public Contact contact { get; set; }
        public related related { get; set; }
        public bool web_enabled { get; set; }
        public bool active { get; set; }
    }
}