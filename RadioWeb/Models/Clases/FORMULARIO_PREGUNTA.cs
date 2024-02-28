using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace RadioWeb.Models
{
    public class FORMULARIO_PREGUNTA
    {
        [Key]
        public int OID { get; set; }
        public int IOR_FORMULARIO { get; set; }        
        public int IOR_TIPO { get; set; }
        public string DESCRIPCION { get; set; }
        public int  ORDEN { get; set; }
        public string OBLIGATORIO { get; set; }
        public string ACTIVO { get; set; }

        [NotMapped]
        public List<FORMULARIO_RESPUESTAS> RESPUESTAS{ get; set; }

        [NotMapped]
        public string[] RESPUESTASFORM { get; set; }

        [NotMapped]
        public FORMULARIO_TIPO_ELEMENTO TIPO { get; set; }
    }
}