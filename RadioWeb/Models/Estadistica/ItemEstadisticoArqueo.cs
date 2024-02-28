using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RadioWeb.Models.Estadistica
{
    public class ItemEstadisticoArqueo
    {
        public string Anyo { get; set; }
        public string    Mes { get; set; }
        public string Grupo { get; set; }
        public int Cuenta { get; set; }
        public double Ventas { get; set; }
        public double Media { get; set; }
    }
}