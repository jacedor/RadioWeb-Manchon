using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;


namespace RadioWeb.Models
{
    public class LOGVIDSIGNER
    {
        public string FECHA { get; set; }
        public string USUARIO { get; set; }
        public string DOCGUID { get; set; }
        public string PLANTILLA { get; set; }
        public string ACCION { get; set; }
        public string TEXTO { get; set; }
        public int OID { get; set; }
        public int IOR_PACIENTE { get; set; }
        public int IOR_EXPLORACION { get; set; }


      
    }
}