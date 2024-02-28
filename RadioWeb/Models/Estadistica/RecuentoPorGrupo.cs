using RadioWeb.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RadioWeb.Utils
{
    public class RecuentoPorCentro
    {
        public int CuentaTotal { get; set; }        
        public List<GAPARATOS> RecuentoPorGrupo { get; set; }
        public List<DAPARATOS> RecuentoPorAparato { get; set; }
        public List<APARATOS> RecuentoPorTipoExploracion { get; set; }
    }
}