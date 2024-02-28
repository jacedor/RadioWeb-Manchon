using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RadioWeb.DTO
{
    public class ExploracionRest
    {
        public string fecha { get; set; }
        public string hora { get; set; }
        public string texto { get; set; }
        public int owner { get; set; }
        public string idcitaonline { get; set; }
    }
}