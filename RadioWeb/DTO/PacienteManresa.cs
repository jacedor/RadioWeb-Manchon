using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RadioWeb.DTO
{
    public class PacienteRest
    {
        public string oid { get; set; }
        public string nombre{ get; set; }
        public string apellidos { get; set; }
        public string dni { get; set; }
        public string sexo { get; set; }
        public string fechaNacimiento { get; set; }
        public int idMutua { get; set; }
        public ExploracionRest exploracion { get; set; }
    }
}