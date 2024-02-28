using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using RadioWeb.Models.Repos;
using RadioWeb.Models;
using FirebirdSql.Data.FirebirdClient;

namespace RadioWeb.Controllers
{
    public class CentroExternoController : Controller
    {

        [HttpPost]
        public ActionResult GetParaFichaExploracion(CENTROSEXTERNOS oCentro)
        {
            
            return PartialView("CentroExternoFichaExploracion", oCentro);
        }

        [HttpPost]
        public ActionResult GetTexto(int oid)
        {

            return PartialView("InfoCentroExterno", CentrosExternosRepositorio.Obtener(oid));
        }
   
    

    }
}
