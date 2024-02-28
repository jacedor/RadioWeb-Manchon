using RadioWeb.Models;
using RadioWeb.Models.Repos;
using RadioWeb.ViewModels;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace RadioWeb.Controllers
{
    [Authorize]
    public class AparatosComplejosController : Controller
    {
     
        private RadioDBContext db = new RadioDBContext();


        // GET: AparatosComplejos
        public ActionResult Index()
        {
            return View(db.AparatosComplejos.Where(p => p.BORRADO == "F").OrderBy(p => p.APARATO1));
        }


        // GET: AparatosComplejos/Create
        public ActionResult Create()
        {
            AparatosComplejos aparatoComplejo = new AparatosComplejos();
            //aparatoComplejo.OID = 123;
            aparatoComplejo.BORRADO = "F";
            return View("AparatosComplejosForm", aparatoComplejo);
        }

        // POST: AparatosComplejos/Create
        [HttpPost]
        public ActionResult Create(AparatosComplejos aparatoComplejo)
        {
            if (ModelState.IsValid)
            {
                db.AparatosComplejos.Add(aparatoComplejo);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            
            return View("AparatosComplejosForm", aparatoComplejo);
        }



        // GET: AparatosComplejos/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            AparatosComplejos aparatoComplejo = db.AparatosComplejos.Find(id);
            if (aparatoComplejo == null)
            {
                return HttpNotFound();
            }
            return View("AparatosComplejosForm", aparatoComplejo);
        }
               

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(AparatosComplejos aparatoComplejo)
        {
            if (ModelState.IsValid)
            {

                db.Entry(aparatoComplejo).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View("AparatosComplejosForm", aparatoComplejo);
        }


        // GET: AparatosComplejos/Delete/5
        public ActionResult Delete(int id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            AparatosComplejos AparatoComplejo = db.AparatosComplejos.Find(id);
            if (AparatoComplejo == null)
            {
                return HttpNotFound();
            }
            return View(AparatoComplejo);
        }

        // POST: AparatosComplejos/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            AparatosComplejos AparatoComplejo = db.AparatosComplejos.Find(id);
            AparatoComplejo.BORRADO = "T";
            db.SaveChanges();

            return RedirectToAction("Index");
        }


    }
}
