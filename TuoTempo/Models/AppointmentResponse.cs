using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TuoTempo.Models
{
    public class AppointmentResponse
    {
        public string app_lid { get; set; }
        public string created { get; set; }
        public string cancelled { get; set; }
        public string modified { get; set; }
        public string status { get; set; }
        public string checkedin { get; set; }
        public string start_visit { get; set; }
        public string end_visit { get; set; }
        public string notes { get; set; }
        public string tags { get; set; }
        public Availabilities availability { get; set; }
        public user user { get; set; }

    }
}