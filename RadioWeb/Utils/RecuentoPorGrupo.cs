using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RadioWeb.Models.Estadistica
{
    public class RecuentoPorCentro
    {
        public int CuentaTotal { get; set; }        
        public Dictionary<string, int> RecuentoPorGrupo { get; set; }
        public Dictionary<string, int> RecuentoPorAparato { get; set; }
        public Dictionary<string, int> RecuentoPorTipoExploracion { get; set; }
    }
}