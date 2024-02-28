using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;


namespace RadioWeb.Models
{
    public class LOGUSUARIOS
    {
        public int OWNER { get; set; }
        public string FECHA { get; set; }
        public string USUARIO { get; set; }
        public string TEXTO { get; set; }
        public int OID { get; set; }
        public string MUTUA { get; set; }
        public string COD_FIL { get; set; }
        public string DATA { get; set; }     


      
    }
}