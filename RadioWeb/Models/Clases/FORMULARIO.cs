using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace RadioWeb.Models
{
    [Table("FORMULARIO")]
    public class FORMULARIO
    {
        [Key]
        public int OID { get; set; }
        public Nullable<int> VERS { get; set; }        
        public string USERNAME { get; set; }
        public string DESCRIPCION { get; set; }
        public string ACTIVO { get; set; }
        public DateTime FECHA { get; set; }
    }
}