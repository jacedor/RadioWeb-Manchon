using RadioWeb.ViewModels;
using RadioWeb.Models.Repos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace RadioWeb.Controllers
{
    public class AgendaGenController : Controller
    {
        public ActionResult Create()
        {
            VWAgendaGen viewModel = null;
            viewModel = new VWAgendaGen {
                            AGENDA = DateTime.Now,
                            APARATOS= DaparatoRepositorio.Lista()};
                
                  

            return View("AgendaGenForm", viewModel);

        }

        [HttpPost] // this action takes the viewModel from the modal
        [ValidateAntiForgeryToken]
        public ActionResult Create(VWAgendaGen viewModel)
        { 
            if (!ModelState.IsValid)
            {

            }
            var colegiado = new VWAgendaGen
            {
               
            };

            
                return View("AgendaGenForm", viewModel);
           
        }



        // GET: AgendaGen/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

     
       

        // GET: AgendaGen/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: AgendaGen/Edit/5
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

        // GET: AgendaGen/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: AgendaGen/Delete/5
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
