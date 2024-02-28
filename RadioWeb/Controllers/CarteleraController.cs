using RadioWeb.Models.Repos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace RadioWeb.Controllers
{
    public class CarteleraController : Controller
    {
        //
        // GET: /Cartelera/
         RadioDBContext db= new RadioDBContext();

        public ActionResult Index()
        {
            
            return View(db.Carteleras.OrderByDescending(s => s.FECHA).Take(10));
        }

        //
        // GET: /Cartelera/Details/5

        public ActionResult Details(int oid)
        {
            
            var model = db.Carteleras.Find(oid);
            
            return View(model);
        }

        //
        // GET: /Cartelera/Create

        public ActionResult Create()
        {
            return View();
        }

        //
        // POST: /Cartelera/Create

        [HttpPost]
        public ActionResult Create(FormCollection collection)
        {
            try
            {
                // TODO: Add insert logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        //
        // GET: /Cartelera/Edit/5

        public ActionResult Edit(int id)
        {
            return View();
        }

        //
        // POST: /Cartelera/Edit/5

        [HttpPost]
        public ActionResult Edit(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add update logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        //
        // GET: /Cartelera/Delete/5

        public ActionResult Delete(int id)
        {
            return View();
        }

        //
        // POST: /Cartelera/Delete/5

        [HttpPost]
        public ActionResult Delete(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add delete logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (db != null)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
