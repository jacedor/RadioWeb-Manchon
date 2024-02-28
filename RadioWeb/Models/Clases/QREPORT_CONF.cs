using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RadioWeb.Models
{
    public class QREPORT_CONF
    {
        public int OID { get; set; }
        public string URL { get; set; }
        public string CENTRO { get; set; }
        public string PUBLICKEY_WS { get; set; }
        public string USER_WS { get; set; }
        public string PASS_WS { get; set; }
        public string TOKEN { get; set; }
        public Nullable<System.DateTime> TOKEN_EXPIRATION { get; set; }
    }
   
}