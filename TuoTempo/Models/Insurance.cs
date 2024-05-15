using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TuoTempo.Models
{
    public class Insurance
    {
        public string insurance_id { get; set; }
        public string name { get; set; }
        public PatientNotice patient_notice { get; set; }       
        public string preparation { get; set; }
        public bool web_enabled { get; set; }
        public bool active { get; set; }
    }
}