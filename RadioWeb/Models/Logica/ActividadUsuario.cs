using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RadioWeb.Models.Logica
{
    public class ActividadUsuario
    {
        public string OID { get; set; }
        public string DESCRIPCION { get; set; }
        public DateTime CUANDO { get; set; }
        public string TIPO { get; set; }
        //LA URL SE CONSTRUYE EN EL LADO DEL SERVIDOR EN FUNCIÓN DEL TIPO
        public string URL { get; set; }
        
      
    }
}