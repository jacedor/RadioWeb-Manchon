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
    public class AparatoController : Controller
    {
        private RadioDBContext db = new RadioDBContext();

        public JsonResult GetAparatosPorCentro(int oidCentro)
        {
            return Json(RadioWeb.Models.Repos.DaparatoRepositorio.ListaPorCentro(oidCentro), JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetAparatosPorGrupo(int oidGrupo)
        {
            return Json(RadioWeb.Models.Repos.DaparatoRepositorio.ListaPorGrupoAparatos(oidGrupo), JsonRequestBehavior.AllowGet);
        }



        /*
         * 
         * APARATOS (DAPARATOS)
         * 
         * */
        public ActionResult CreateAparato(int groupOid)
        {
            DAPARATOS daparato = new DAPARATOS();
            daparato.BORRADO = "F";
            daparato.IOR_EMPRESA = 4;
            daparato.OWNER = groupOid;
            return View("Aparatos", daparato);
        }
        [HttpPost]
        public ActionResult CreateAparato(DAPARATOS daparato)
        {
            if (ModelState.IsValid)
            {
                db.Daparatos.Add(daparato);
                db.SaveChanges();
                int groupOid = daparato.OWNER;

                return RedirectToAction("Index", "Grupos", new { id = groupOid, tab = "tabAparatos" });
            }

            return View("Aparatos", daparato);
        }


        public ActionResult EditAparato(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }


            DAPARATOS aparato = db.Daparatos.Find(id);
            if (aparato == null)
            {
                return HttpNotFound();
            }
            return View("Aparatos", aparato);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditAparato(DAPARATOS aparato)
        {
            if (ModelState.IsValid)
            {
                db.Entry(aparato).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index", "Grupos", new { id = aparato.OWNER, tab = "tabAparatos" });
            }
            return View("Aparatos", aparato);
        }


        public ActionResult DeleteAparato(int id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            ForeignkeyCheckController fkcc = new ForeignkeyCheckController();
            ViewBag.fkBorrable = fkcc.isBorrable("DAPARATOS", id);

            DAPARATOS aparato = db.Daparatos.Find(id);
            if (aparato == null)
            {
                return HttpNotFound();
            }
            return View(aparato);
        }

        [HttpPost, ActionName("DeleteAparato")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteAparatoConfirmed(int id)
        {
            DAPARATOS aparato = db.Daparatos.Find(id);

            ForeignkeyCheckController fkcc = new ForeignkeyCheckController();
            if (fkcc.isBorrable("DAPARATOS", id))
            {
                aparato.BORRADO = "T";
                db.SaveChanges();
            }

            return RedirectToAction("Index", "Grupos", new { id = aparato.OWNER, tab = "tabAparatos" });
        }

        [HttpPost]
        public ActionResult ListaAparatos(int oidGrupo)
        {
            List<DAPARATOS> oListaDaparatos = DaparatoRepositorio.ListaPorGrupoAparatosV2(oidGrupo);
            ViewBag.groupOid = oidGrupo;
            return PartialView("_ListaAparatos", oListaDaparatos);
        }


    }
}
