using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TuoTempo.Models
{
    public class Availabilities
    {
        public string availability_lid { get; set; }
        public string date { get; set; }
        public string start_time { get; set; }
        public string end_time { get; set; }
        public string location_lid { get; set; }
        public string resource_lid { get; set; }
        public string activity_lid { get; set; }
        public string insurance_lid { get; set; }
        public string price { get; set; }
    }
}