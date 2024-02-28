using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace RadioWeb.Models
{
    public class FORMULARIO_TIPO_ELEMENTO
    {
        [Key]
        public int OID { get; set; }
        public string TIPO { get; set; }
        public string ACTIVO { get; set; }
    }
}