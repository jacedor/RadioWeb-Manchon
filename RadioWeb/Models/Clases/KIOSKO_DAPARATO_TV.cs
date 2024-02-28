using RadioWeb.Models.Repos;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace RadioWeb.Models
{

    [Table("KIOSKO_DAPARATO_TV")]
    public partial class KIOSKO_DAPARATO_TV
    {

        [Key]
        public int OID { get; set; }
        public int DAPARATO_OID { get; set; }
        public int TV_OID { get; set; }

        [NotMapped]
        public string ACTION
        {
            get
            {
                return (OID > 0) ? "Update" : "Create";
            }

        }

    }
}