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
using RadioWeb.ViewModels;
using System.IO;
using ADPM.Common;
using RadioWeb.Repositories;
using RadioWeb.Filters;

namespace RadioWeb.Controllers
{
    [Authorize]
    public class PreciosController : Controller
    {
        private RadioDBContext db = new RadioDBContext();


       
        public ActionResult Index()
        {
            return View();
        }

        ////método utilizado por x-editable para editar campos sueltos
        [HttpPost]
        public ActionResult EditarCampo(string name, int pk, string value)
        {
           PRECIOS oPrecio= db.Precios.Single(p => p.OID == pk);
            oPrecio.CANTIDAD =  double.Parse( value.Replace('.',','));
            db.Entry(oPrecio).State = EntityState.Modified;
            db.SaveChanges();


           
            return new HttpStatusCodeResult(200);
        }



        public ActionResult Buscar(int ior_entidadPagadora,int ior_gaparato,string search="",string order="asc")
        {
            try
            {
                var serializer = new System.Web.Script.Serialization.JavaScriptSerializer();
                serializer.MaxJsonLength = 500000000;
                var listaPrecios = db.Precios.Where(p => p.IOR_ENTIDADPAGADORA == ior_entidadPagadora && p.IOR_GAPARATO == ior_gaparato)
                       .Select(c => new { Descripcion = c.APARATO.DES_FIL, Precio = c.CANTIDAD, Oid= c.OID })
                       .ToList();
                var json = Json(listaPrecios, JsonRequestBehavior.AllowGet);
                json.MaxJsonLength = 500000000;


                return json;
            }
            catch (Exception)
            {

                throw;
            }




           
        }


        // GET: Facturas/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            FACTURAS fACTURAS = db.Facturas.Find(id);
            if (fACTURAS == null)
            {
                return HttpNotFound();
            }
            return View(fACTURAS);
        }

        // POST: Facturas/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            FACTURAS fACTURAS = db.Facturas.Find(id);
            db.Facturas.Remove(fACTURAS);
            db.SaveChanges();
            return RedirectToAction("Index");
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
