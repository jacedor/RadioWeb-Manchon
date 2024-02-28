using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace RadioWeb.Models
{
    [Table("CONS_GRUPO")]
    public partial class CONS_GRUPO
    {
        [Key]
        public int OID { get; set; }
        public Nullable<int> VERS { get; set; }
        public Nullable<int> CID{ get; set; }
        public string CANAL { get; set; }
        public Nullable<int> OWNER { get; set; }
        public string USERNAME { get; set; }
        public DateTime MODIF { get; set; }
        public Nullable<int> IOR_GAPARATO { get; set; }
        public Nullable<int> IOR_CONSUMIBLE { get; set; }
        public string BORRADO { get; set; }

    }
}