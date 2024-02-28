using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace RadioWeb.Models.Clases
{
    public class PDF_INFORME
    {
     
        [NotMapped]
        public string RUTA { get; set; }
        [NotMapped]
        public string NOMBRE { get; set; }
        [NotMapped]
        public string TIPO { get; set; }
        [NotMapped]
        public int IOR_INFORME { get; set; }
    }
    }
