using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RadioWeb.Models.Clases
{
    public class ExploracionCitaOnline
    {
        public int ID { get; set; }
        public int OIDAPARATO { get; set; }
        public string OIDEXPLORACION { get; set; }
        public string FILEXPLORACION { get; set; }
        public string NOMBRE { get; set; }
        public string APELLIDOS { get; set; }
        public string DNI { get; set; }
        public string SEXO { get; set; }
        public string TELEFONOMOVIL { get; set; }
        public string EMAIL { get; set; }
        public string FECHANACIMIENTO { get; set; }
        public int OIDMUTUA { get; set; }
        public string TEXTO { get; set; }
    }
}