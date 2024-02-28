using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TuoTempo.Models
{
    public class address
    {
        public string street_number { get; set; }
        public string street { get; set; }
        public string zipcode { get; set; }
        public string city { get; set; }
        public string province { get; set; }
        public string region { get; set; }
        public string country { get; set; }
    }
}