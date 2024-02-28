using RadioWeb.Models;
using RadioWeb.Models.Repos;
using RadioWeb.ViewModels;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace RadioWeb.Controllers
{
    public class FormularioController : Controller
    {

        private RadioDBContext db = new RadioDBContext();       

        //paso 1 realización de infomes
        public ActionResult Index(int oid = -1)
        {
            VWFormulario oviewModel = new VWFormulario(oid == -1 ? db.Formulario.First().OID : oid); 
            return View(oviewModel);
        }

       
        public ActionResult EditPregunta(int oid)
        {
            FORMULARIO_PREGUNTA oModel = db.Formulario_Pregunta.Single(p=>p.OID== oid);
            ViewBag.RESPUESTAS = db.Formulario_Respuestas.ToList();
            List<FORMULARIO_PREGUNTAS_RESPUESTAS> oRespuestas = db.Formulario_Pregunta_Respuestas
                                                                  .Where(p => p.IOR_PREGUNTA == oid)
                                                                   .ToList();

            oModel.RESPUESTAS = new List<FORMULARIO_RESPUESTAS>();
            foreach (FORMULARIO_PREGUNTAS_RESPUESTAS respuesta in oRespuestas)
            {
                oModel.RESPUESTAS.Add(db.Formulario_Respuestas.Single(r => r.OID == respuesta.IOR_RESPUESTA));
            }

            ViewBag.TIPOS = db.Formulario_Tipo_Elemento.ToList();
            return PartialView(oModel);
        }

       [HttpPost]
        public ActionResult EditPregunta(FORMULARIO_PREGUNTA oviewModel)
        {
 
            if (!ModelState.IsValid)
            {
                return PartialView(oviewModel);
            }
            
            //Primero eliminamos todas las respuestas relacionadas con la pregunta
            foreach (var respuesta in db.Formulario_Pregunta_Respuestas.Where(p=>p.IOR_PREGUNTA ==oviewModel.OID))
            {
               
                db.Formulario_Pregunta_Respuestas.Remove(respuesta);
                
            }

            foreach (string respuesta in oviewModel.RESPUESTASFORM)
            {
                FORMULARIO_PREGUNTAS_RESPUESTAS p = new FORMULARIO_PREGUNTAS_RESPUESTAS
                {
                    IOR_PREGUNTA = oviewModel.OID,
                    IOR_RESPUESTA = int.Parse(respuesta)
                };
                db.Formulario_Pregunta_Respuestas.Add(p);

            }




            db.Entry(oviewModel).State = EntityState.Modified;
            db.SaveChanges();


            return RedirectToAction("Index", new
            {
                oid = oviewModel.IOR_FORMULARIO,
                tabActivo = "tab-preguntas"
            });
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}