using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RadioWeb.Models.Estadistica
{
    public class ItemEstadisticoExploraciones
    {
        public string Centro { get; set; }
        public string Color { get; set; }
        public string Grupo { get; set; }
        public string Aparato { get; set; }
        public string TipoExploracion { get; set; }
        public double Valor { get; set; }
    }
}