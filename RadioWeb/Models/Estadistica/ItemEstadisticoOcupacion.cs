using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace RadioWeb.Models.Estadistica
{
    public class ItemEstadisticoOcupacion
    {
        public string Anyo { get; set; }
        public string Fecha { get; set; }
        public string Fecha_Inicio { get; set; }
        public string Fecha_Fin{ get; set; }
        public string Mes { get; set; }
        [DataType("GAPARATO")]
        [DisplayName("GRUPO")]
        public int Grupo { get; set; }
        public string Aparato { get; set; }
        public string CodFil { get; set; }
        public int IOR_APARATO { get; set; }
        public int HuecosHorario { get; set; }
        public int HuecosLibre { get; set; }
        public int PorcentageLibre { get; set; }
        public int HuecosOcupados { get; set; }
        public int PorcentageOcupados { get; set; }
        public int Dia { get; set; }


    }
}