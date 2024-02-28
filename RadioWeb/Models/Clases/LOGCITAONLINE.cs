using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;


namespace RadioWeb.Models
{
    public class LOGCITAONLINE
    {
        public int OID { get; set; }
        public int VERS { get; set; }
        public int CID { get; set; }
        public int OWNER { get; set; }
        public int ORDEN { get; set; }
        public string CANAL { get; set; }
        public string BORRADO { get; set; }
        public string FECHA { get; set; }
        public string HORA { get; set; }
        public string TEXTO { get; set; }
        public string GRUPO { get; set; }
        public string EXPLORACION{ get; set; }
        public string MODIF { get; set; }
        public string USERNAME { get; set; }
        public string APARATO { get; set; }



    }
}