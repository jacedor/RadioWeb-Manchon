using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TuoTempo.Models
{
    public class AppointmentRequest
    {

        public Int64 app_lid { get; set; }
        public Availabilities availability { get; set; }
        public user user { get; set; }
        public string notes { get; set; }
        public string tags { get; set; }
        public string referral_doctor { get; set; }
        public communication communication { get; set; }
        public string type { get; set; }
        public string groupid { get; set; }
        public insurance_card insurance_card { get; set; }
        public custom_fields custom_fields { get; set; }
       
    }
}