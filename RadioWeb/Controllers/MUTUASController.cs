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
    public class MUTUASController : Controller
    {
        private RadioDBContext db = new RadioDBContext();
        private DireccionRepository _direccionRepository;
        private TelefonoRepository _telefonoRepository;
        public MUTUASController()
        {
            _direccionRepository = new DireccionRepository(db);
            _telefonoRepository = new TelefonoRepository(db);
        }

        [HttpPost]
        public JsonResult Buscar(string query, bool conDirecciones = false)
        {
            return Json(MutuasRepositorio.Lista(query), JsonRequestBehavior.AllowGet);
        }
        // GET: MUTUAS
        public ActionResult Index()
        {
            return View(db.Mutuas.ToList());
        }

        // GET: MUTUAS/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            MUTUAS mUTUAS = db.Mutuas.Find(id);
            if (mUTUAS == null)
            {
                return HttpNotFound();
            }
            return View(mUTUAS);
        }

        // GET: MUTUAS/Create
        public ActionResult Create()
        {
            MUTUAS mutua = new MUTUAS();
            return View("MutuaForm", mutua);
        }

        // POST: MUTUAS/Create
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "OID,OWNER,NOMBRE,NIF,CONTACTO,COD_DAP,NUM_CLI,CODMUT,IOR_CENTRAL,TIPOPAGO,DIASCARENCIA,MAILING,TEXTO,DIRECCIONES")] MUTUAS mUTUAS)
        {

            if (ModelState.IsValid)
            {
                _direccionRepository.Update(mUTUAS.DIRECCIONES, mUTUAS.OID);
                _telefonoRepository.Update(mUTUAS.TELEFONOS, mUTUAS.OID);
                db.Mutuas.Add(mUTUAS);
                db.SaveChanges();
                var texto = TextosRepositorio.Obtener(mUTUAS.OID);
                if (texto != null)
                {
                    texto.OWNER = mUTUAS.OID;
                    texto.TEXTO = mUTUAS.TEXTO;
                    TextosRepositorio.InsertarOrUpdate(texto);

                }

                return RedirectToAction("Index");
            }


            return View("MutuaForm",mUTUAS);
        }

        // GET: MUTUAS/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            MUTUAS mUTUAS = db.Mutuas.Find(id);
            if (mUTUAS == null)
            {
                return HttpNotFound();
            }
            mUTUAS.TEXTO = TextosRepositorio.Obtener(id.Value).TEXTO;
            if (mUTUAS.IOR_CENTRAL > 0)
            {
                mUTUAS.CENTRAL = db.Mutuas.Find(mUTUAS.IOR_CENTRAL.Value).NOMBRE;
            }
            return View("MutuaForm", mUTUAS);
        }

        // POST: MUTUAS/Edit/5
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "OID,OWNER,NOMBRE,NIF,CONTACTO,COD_DAP,NUM_CLI,CODMUT,IOR_CENTRAL,TIPOPAGO,DIASCARENCIA,MAILING,TEXTO,DIRECCIONES,TELEFONOS")] MUTUAS mUTUAS)
        {
            if (ModelState.IsValid)
            {
                _direccionRepository.Update(mUTUAS.DIRECCIONES, mUTUAS.OID);
                _telefonoRepository.Update(mUTUAS.TELEFONOS, mUTUAS.OID);

                db.Entry(mUTUAS).State = EntityState.Modified;
                db.SaveChanges();
                var texto = TextosRepositorio.Obtener(mUTUAS.OID);
                if (texto != null)
                {
                    texto.OWNER = mUTUAS.OID;
                    texto.TEXTO = mUTUAS.TEXTO;
                    TextosRepositorio.InsertarOrUpdate(texto);
                }

                return RedirectToAction("Index");
            }
            return View("MutuaForm",mUTUAS);
        }

        // GET: MUTUAS/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            MUTUAS mUTUAS = db.Mutuas.Find(id);
            if (mUTUAS == null)
            {
                return HttpNotFound();
            }
            return View(mUTUAS);
        }

        // POST: MUTUAS/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            MUTUAS mUTUAS = db.Mutuas.Find(id);
            mUTUAS.BORRADO = "T";
           
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
