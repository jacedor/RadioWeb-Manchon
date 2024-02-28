using RadioWeb.ViewModels.Estadistica;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace RadioWeb.Controllers
{
    public class ActividadController : Controller
    {
        //
        // GET: /Actividad/

        public ActionResult Index()
        {
            VWFiltrosResumenFacturacion oViewModel = new VWFiltrosResumenFacturacion();
            oViewModel.FILTROS = new ViewModels.VWFiltros();
            oViewModel.FILTROS.FECHA_INICIO = DateTime.Now.AddDays(-30).ToShortDateString();
            oViewModel.FILTROS.FECHA_FIN = DateTime.Now.ToShortDateString();
            return View(oViewModel);
        }

        //
        // GET: /Actividad/Details/5

        public ActionResult Details(int id)
        {
            return View();
        }

        //
        // GET: /Actividad/Create

        public ActionResult Create()
        {
            return View();
        }

        //
        // POST: /Actividad/Create

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
        // GET: /Actividad/Edit/5

        public ActionResult Edit(int id)
        {
            return View();
        }

        //
        // POST: /Actividad/Edit/5

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
        // GET: /Actividad/Delete/5

        public ActionResult Delete(int id)
        {
            return View();
        }

        //
        // POST: /Actividad/Delete/5

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
    }
}
