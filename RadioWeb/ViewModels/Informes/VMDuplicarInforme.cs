using RadioWeb.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RadioWeb.ViewModels.Informes
{
    public class VMDuplicarInforme
    {
        public string URL_PACS { get; set; }
        public string DESFIL { get; set; }
        public string UrlPreviaDuplicar { get; set; }
        public EXPLORACION EXPLORACINOAINFORMAR{ get; set; }

        public USUARIO UsuarioLogeado { get; set; }

        public IEnumerable<INFORMES>  INFORMESPACIENTE{ get; set; }
        public IEnumerable<P_INFORMES> PLANTILLASINFORMES { get; set; }
    }
}