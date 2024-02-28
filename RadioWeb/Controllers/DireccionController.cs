using RadioWeb.Models;
using RadioWeb.Models.Logica;
using RadioWeb.Models.Repos;
using RadioWeb.Models.Utilidades;
using RadioWeb.Repositories;
using RadioWeb.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace RadioWeb.Controllers
{
    public class DireccionController : Controller
    {
        private RadioDBContext db = new RadioDBContext();

        private DireccionRepository _direccionRepository;
        private TelefonoRepository _telefonoRepository;

        public DireccionController()
        {
            _direccionRepository = new DireccionRepository(db);
            _telefonoRepository = new TelefonoRepository(db);
        }
        //a pesar de ser un método post
        //este método no actua contra la base de datos,
        //lo que hace es devolver la partial view rellenada con la dirección recibida
        //y se suele usar para inserta una nueva direccion en el edit item template
        //de direcciones
        [HttpPost]
        public ActionResult Post(DIRECCION direccion)
        {
            if (String.IsNullOrEmpty(ViewData.TemplateInfo.HtmlFieldPrefix))
            {
                ViewData.TemplateInfo.HtmlFieldPrefix = "DIRECCIONES";
            }
            return PartialView("DireccionForm", direccion);
        }

        [HttpPost]
        public ActionResult Edit(List<DIRECCION> direcciones)
        {
            ResultadoRequest oResult;
            if (!ModelState.IsValid)
            {
                return PartialView("DireccionForm", direcciones);
            }
            try
            {
                _direccionRepository.Update(direcciones, direcciones.First().OWNER);
                db.SaveChanges();

            }
            catch (Exception ex)
            {
                oResult = new ResultadoRequest()
                {
                    Mensaje = "Se ha producido un error al guarda la historia. " + ex.Message,
                    Resultado = ResultadoRequest.RESULTADO.ERROR
                };
                return PartialView("_ResultRequest", oResult);
            }

            oResult = new ResultadoRequest()
            {
                Mensaje = "Se ha guardado la Dirección Correctamente. ",
                Resultado = ResultadoRequest.RESULTADO.SUCCESS
            };
            return PartialView("_ResultRequest", oResult);
            
           
        }




        //public ActionResult PueblosList(string term)
        //{
        //    PueblosRepositorio oRepo = new PueblosRepositorio();
        //    List<AutoCompleteAvanzado> items = new List<AutoCompleteAvanzado>();

        //    foreach (PUEBLOS oPueblo in oRepo.Obtener(term))
        //    {
        //        items.Add(new AutoCompleteAvanzado
        //        {
        //            value = oPueblo.OID.ToString(),
        //            label = oPueblo.PUEBLO,
        //            desc = oPueblo.CODIGO,
        //            icon = oPueblo.PROVINCIA
        //        });
        //    }


        //    return Json(items, JsonRequestBehavior.AllowGet);
        //}

        //public ActionResult PuebloPorCodigo(string term)
        //{
        //    PueblosRepositorio oRepo = new PueblosRepositorio();
        //    PUEBLOS oPueblo = oRepo.ObtenerPoblacionPorCodigo(term);

        //    return Json(oPueblo, JsonRequestBehavior.AllowGet);
        //}

        //public ActionResult GetItemTemplate()
        //{
        //    DIRECCION oNewDir = new DIRECCION { OID = -1, POBLACION = "" };
        //    return PartialView("_ListItem", oNewDir);
        //}

        [HttpPost]
        public ActionResult EditarCampo(string name, int pk, string value)
        {

            DireccionRepositorio.UpdateCampo(name, value, pk);

            return new HttpStatusCodeResult(200);
        }
    }
}
