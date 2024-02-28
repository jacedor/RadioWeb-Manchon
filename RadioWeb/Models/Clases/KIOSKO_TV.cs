using RadioWeb.Models.Repos;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace RadioWeb.Models
{

    [Table("KIOSKO_TV")]
    public partial class KIOSKO_TV
    {

        [Key]
        public int OID { get; set; }
        public string ID { get; set; }
        public string NOMBRE { get; set; }
        public string DESCRIPCION { get; set; }
        public string DESHABILITADA { get; set; }


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