using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace RadioWeb.Models
{
    public class FORMULARIO_RESPUESTAS
    {
        [Key]
        public int OID { get; set; }
        public Nullable<int> VERS { get; set; }
        public string VALOR { get; set; }
        public string ACTIVO { get; set; }
      
    }
}