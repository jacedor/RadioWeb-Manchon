using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace RadioWeb.Models
{
    
    public class FORMULARIO_PREGUNTAS_RESPUESTAS
    {
        [Key]
        [Column(Order = 1)]
        public int IOR_PREGUNTA { get; set; }
        [Key]
        [Column(Order = 2)]
        public int IOR_RESPUESTA { get; set; }
        public int? OIDHABILITA { get; set; }
      
    }
}