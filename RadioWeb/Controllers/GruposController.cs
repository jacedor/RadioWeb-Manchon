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
    [Authorize]
    public class GruposController : Controller
    {
        private RadioDBContext db = new RadioDBContext();

        public GruposController()
        {
        }


        public ActionResult Index()
        {
            return View(db.Gaparatos.Where(p => p.IOR_EMPRESA == 4 && p.OID > 0).OrderBy(p => p.COD_GRUP));
        }


        /*
         * 
         * GRUPOS (GAPARATOS)
         * 
         */

        public ActionResult Create()
        {
            GAPARATOS gaparato = new GAPARATOS();
            gaparato.BORRADO = "F";
            gaparato.IOR_EMPRESA = 4;
            return View("Grupos", gaparato);
        }

        [HttpPost]
        public ActionResult Create(GAPARATOS gaparato)
        {
            if (ModelState.IsValid)
            {
                db.Gaparatos.Add(gaparato);
                db.SaveChanges();
                int groupOid = gaparato.OID;

                return RedirectToAction("Index", new { id = groupOid});
            }

            return View("Grupos", gaparato);
        }


        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            GAPARATOS grupo = db.Gaparatos.Find(id);
            if (grupo == null)
            {
                return HttpNotFound();
            }
            return View("Grupos", grupo);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(GAPARATOS gaparato)
        {
            if (ModelState.IsValid)
            {
                db.Entry(gaparato).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index", new { id = gaparato.OID });
            }
            return View("Grupos", gaparato);
        }



        public ActionResult Delete(int id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            ForeignkeyCheckController fkcc = new ForeignkeyCheckController();
            ViewBag.fkBorrable = fkcc.isBorrable("GAPARATOS", id);

            GAPARATOS grupo = db.Gaparatos.Find(id);
            if (grupo == null)
            {
                return HttpNotFound();
            }
            return View(grupo);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            GAPARATOS grupo = db.Gaparatos.Find(id);

            ForeignkeyCheckController fkcc = new ForeignkeyCheckController();
            if (fkcc.isBorrable("GAPARATOS", id))
            {
                grupo.BORRADO = "T";
                db.SaveChanges();
            }

            return RedirectToAction("Index", new { id = grupo.OWNER, tab = "tabAparatos" });
        }

    }

}
