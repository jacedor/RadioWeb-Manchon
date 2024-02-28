using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;


namespace RadioWeb.Models
{
    public class LOGERRORS
    {
        [Key]
        public int OID { get; set; }
        public int VERS { get; set; }
        public int CID { get; set; }
        public string CANAL { get; set; }
        public int OWNER { get; set; }
        public string USERNAME { get; set; }
        public string BORRADO { get; set; }
        public string ERROR { get; set; }
        public string FECHA { get; set; }
      
    }
}