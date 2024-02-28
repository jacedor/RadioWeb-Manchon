using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;

namespace TuoTempo.Models
{
    public class user
    {
        public string user_lid { get; set; }
        public IdNumber id_number { get; set; }
        public string first_name { get; set; }
        public string second_name { get; set; }
        public string third_name { get; set; }
        public string birthdate { get; set; }
        public string place_of_birth { get; set; }
        public string gender { get; set; }
        public Contact contact { get; set; }
        public Privacy privacy { get; set; }
        public address address { get; set; }
    }

    public class IdNumber
    {
        public string number { get; set; }
        public int type { get; set; }
    }



    public class Privacy
    {
        public CommunicationPreferences communication_preferences { get; set; }
        public bool primary { get; set; }
        public bool promotions { get; set; }
        public bool review { get; set; }
        public bool dossier { get; set; }
    }

    public class CommunicationPreferences
    {
        public bool SMS { get; set; }
        public bool email { get; set; }
        public bool phone { get; set; }
    }
}