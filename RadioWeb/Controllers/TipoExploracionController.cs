using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using RadioWeb.Models;
using RadioWeb.Models.Repos;
using RadioWeb.Repositories;


namespace RadioWeb.Controllers
{
    public class TipoExploracionController : Controller
    {
        private RadioDBContext db = new RadioDBContext();
    
        /*
         * 
         * Tipos de exploracion (APARATOS)
         * 
         */
        public ActionResult CreateTipoExploracion(int groupOid)
        {
            APARATOS tipoExploracion = new APARATOS();
            tipoExploracion.BORRADO = "F";
            tipoExploracion.IOR_EMPRESA = 4;
            tipoExploracion.OWNER = groupOid;
            return View("TiposExploraciones", tipoExploracion);
        }

        [HttpPost]
        public ActionResult CreateTipoExploracion(APARATOS tipoExploracion)
        {
            if (ModelState.IsValid)
            {
                db.Aparatos.Add(tipoExploracion);
                db.SaveChanges();
                int groupOid = tipoExploracion.OWNER.GetValueOrDefault();

                return RedirectToAction("Index", "Grupos", new { id = groupOid, tab = "tabTexploracion" });
            }

            return View("TiposExploracioines", tipoExploracion);
        }

        public ActionResult EditTipoExploracion(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }


            APARATOS tipoExploracion = db.Aparatos.Find(id);
            if (tipoExploracion == null)
            {
                return HttpNotFound();
            }
            return View("TiposExploraciones", tipoExploracion);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditTipoExploracion(APARATOS tipoExploracion)
        {
            if (ModelState.IsValid)
            {
                db.Entry(tipoExploracion).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index", "Grupos", new { id = tipoExploracion.OWNER, tab = "tabTexploracion" });
            }
            return View("TiposExploraciones", tipoExploracion);
        }


        public ActionResult DeleteTipoExploracion(int id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            ForeignkeyCheckController fkcc = new ForeignkeyCheckController();
            ViewBag.fkBorrable = fkcc.isBorrable("APARATOS", id);

            APARATOS tipoExploracion = db.Aparatos.Find(id);
            if (tipoExploracion == null)
            {
                return HttpNotFound();
            }
            return View(tipoExploracion);
        }

        [HttpPost, ActionName("DeleteTipoExploracion")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteTipoExploracionConfirmed(int id)
        {
            APARATOS tipoExploracion = db.Aparatos.Find(id);

            ForeignkeyCheckController fkcc = new ForeignkeyCheckController();
            if (fkcc.isBorrable("APARATOS", id))
            {
                tipoExploracion.BORRADO = "T";
                db.SaveChanges();
            }

            return RedirectToAction("Index", "Grupos", new { id = tipoExploracion.OWNER, tab = "tabTexploracion" });
        }


        [HttpPost]
        public ActionResult ListaTiposExploraciones(int oidGrupo)
        {
            List<APARATOS> oListaTiposExploraciones = AparatoRepositorio.ListaPorGrupoAparatos(oidGrupo);
            ViewBag.groupOid = oidGrupo;
            return PartialView("_ListaTiposExploraciones", oListaTiposExploraciones);
        }

    }
}