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
    public class CentrosExternosController : Controller
    {
        private RadioDBContext db = new RadioDBContext();
        private DireccionRepository _direccionRepository;
        private TelefonoRepository _telefonoRepository;
        public CentrosExternosController()
        {
            _direccionRepository = new DireccionRepository(db);
            _telefonoRepository = new TelefonoRepository(db);
        }

     


        // GET: MUTUAS
        public ActionResult Index()
        {
            return View(db.CentrosExternos.ToList());
        }

        // GET: MUTUAS/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            CENTROSEXTERNOS centroEx = db.CentrosExternos.Find(id);
            if (centroEx == null)
            {
                return HttpNotFound();
            }
            return View(centroEx);
        }

        // GET: MUTUAS/Create
        public ActionResult Create()
        {
            CENTROSEXTERNOS oCentro = new CENTROSEXTERNOS();
           
            var listaMutuas= MutuasRepositorio.Lista(false);
            var mutuaPrivados = MutuasRepositorio.Obtener(3820080);
            mutuaPrivados.NOMBRE = "PRIVADO";
            listaMutuas.Add(mutuaPrivados);
            ViewBag.Mutuas = listaMutuas;// db.Mutuas.ToList();

            return View("CentroExternoForm", oCentro);
        }

        // POST: MUTUAS/Create
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(CENTROSEXTERNOS oCentro)
        {

            if (ModelState.IsValid)
            {
                _direccionRepository.Update(oCentro.DIRECCIONES, oCentro.OID);
                _telefonoRepository.Update(oCentro.TELEFONOS, oCentro.OID);
                db.CentrosExternos.Add(oCentro);
                db.SaveChanges();
                var texto = TextosRepositorio.Obtener(oCentro.OID);
                if (texto != null)
                {
                    texto.OWNER = oCentro.OID;
                    texto.TEXTO = oCentro.TEXTO;
                    TextosRepositorio.InsertarOrUpdate(texto);

                }

                return RedirectToAction("Index");
            }


            return View("CentroExternoForm",oCentro);
        }

        // GET: MUTUAS/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            CENTROSEXTERNOS oCentro = db.CentrosExternos.Find(id);
            if (oCentro == null)
            {
                return HttpNotFound();
            }
            oCentro.TEXTO = TextosRepositorio.Obtener(id.Value).TEXTO;
            var mutuaPrivados = MutuasRepositorio.Obtener(3820080);
            mutuaPrivados.NOMBRE = "PRIVADO";
            var listaMutuas= MutuasRepositorio.Lista(false);
            listaMutuas.Add(mutuaPrivados);
            ViewBag.Mutuas = listaMutuas;// db.Mutuas.ToList();
            ViewBag.CentrosExternos = db.CentrosExternos.Where(p => p.BORRADO != "T" || p.BORRADO == null);



            return View("CentroExternoForm", oCentro);
        }

        [HttpPost]
        public JsonResult CopiarConfiguracion(int centroActual, int centroSeleccionado)
        {
            // Obtener mutuas y colegiados del centro seleccionado
            var mutuasSeleccionadas = db.CE_MUTUAS.Where(m => m.IOR_CENTROEXTERNO == centroSeleccionado).ToList();
            var colegiadosSeleccionados = db.CE_MEDICOS.Where(c => c.IOR_CENTROEXTERNO == centroSeleccionado).ToList();

            // Crear nuevas mutuas asociadas al centro actual
            var nuevasMutuas = mutuasSeleccionadas.Select(mutua => new CE_MUTUAS
            {
                IOR_CENTROEXTERNO = centroActual // Asignar el centro actual
                
            }).ToList();

            // Crear nuevos colegiados asociados al centro actual
            var nuevosColegiados = colegiadosSeleccionados.Select(colegiado => new CE_MEDICOS
            {
                IOR_CENTROEXTERNO = centroActual
            }).ToList();

            // Guardar los nuevos registros en la base de datos
            db.CE_MUTUAS.AddRange(nuevasMutuas);
            db.CE_MEDICOS.AddRange(nuevosColegiados);
            db.SaveChanges();

           

            // Devolver éxito
            return Json(new { success = true });
        }




        // POST: MUTUAS/Edit/5
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(CENTROSEXTERNOS oCentro)
        {
            if (ModelState.IsValid)
            {
                // Verificar y procesar direcciones
                // Verificar si existen direcciones antes de intentar actualizar
                if (oCentro.DIRECCIONES != null && oCentro.DIRECCIONES.Any() )
                {
                    _direccionRepository.Update(oCentro.DIRECCIONES, oCentro.OID);
                }

                // Verificar si existen teléfonos antes de intentar actualizar
                if (oCentro.TELEFONOS != null && oCentro.TELEFONOS.Any())
                {
                    _telefonoRepository.Update(oCentro.TELEFONOS, oCentro.OID);
                }

                // Actualizar datos del centro
                db.Entry(oCentro).State = EntityState.Modified;
                db.SaveChanges();

                // Procesar texto relacionado
                var texto = TextosRepositorio.Obtener(oCentro.OID);
                if (texto != null)
                {
                    texto.OWNER = oCentro.OID;
                    texto.TEXTO = oCentro.TEXTO;
                    TextosRepositorio.InsertarOrUpdate(texto);
                }

                return RedirectToAction("Index");
            }

            return View("CentroExternoForm", oCentro);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult GuardarRelaciones(int OID, List<int> MutuasSeleccionadas, List<int> ColegiadosSeleccionados)
        {
            var centro = db.CentrosExternos.Include("MutuasRelacionadas").Include("ColegiadosRelacionados").FirstOrDefault(c => c.OID == OID);
            if (centro == null)
            {
                return HttpNotFound();
            }

            // Actualizar mutuas relacionadas
            centro.MutuasRelacionadas.Clear();
            if (MutuasSeleccionadas != null && MutuasSeleccionadas.Any())
            {
                var mutuas = db.Mutuas.Where(m => MutuasSeleccionadas.Contains(m.OID)).ToList();
                centro.MutuasRelacionadas.AddRange(mutuas);
            }

            // Actualizar colegiados relacionados
            centro.ColegiadosRelacionados.Clear();
            if (ColegiadosSeleccionados != null && ColegiadosSeleccionados.Any())
            {
                var colegiados = db.Colegiados.Where(c => ColegiadosSeleccionados.Contains(c.OID)).ToList();
                centro.ColegiadosRelacionados.AddRange(colegiados);
            }

            db.SaveChanges();

            return RedirectToAction("Edit", new { id = OID });
        }



        // GET: MUTUAS/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            CENTROSEXTERNOS oCentro = db.CentrosExternos.Find(id);
            if (oCentro == null)
            {
                return HttpNotFound();
            }
            return View(oCentro);
        }

        // POST: MUTUAS/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            CENTROSEXTERNOS mUTUAS = db.CentrosExternos.Find(id);
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
