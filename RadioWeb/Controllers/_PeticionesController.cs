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

namespace RadioWeb.Controllers
{
    public class resultadoPeticiones
    {
        public int total { get; set; }
        public IEnumerable<BOLSA_PRUEBAS> rows { get; set; }
    }
    public class PeticionesController : Controller
    {
        private RadioDBContext db = new RadioDBContext();

        // GET: Peticiones
        public ActionResult Index(string search = "", string sort="OID", string order = "desc", int offset = 0, int limit = 10)
        {
            int usuario = UsuariosRepositorio.Obtener(User.Identity.Name).IOR_COLEGIADO.Value;
            var serializer = new System.Web.Script.Serialization.JavaScriptSerializer();
            resultadoPeticiones oResultado = new resultadoPeticiones();
            switch (sort)
            {
                case "PACIENTE":
                    if (order=="asc")
                    {
                        oResultado.rows = db.BolsaPruebas.Where(p => p.IOR_MEDICOREFERENTE == usuario).OrderBy(p => p.PACIENTE);
                    }
                    else
                    {
                        oResultado.rows = db.BolsaPruebas.Where(p => p.IOR_MEDICOREFERENTE == usuario).OrderByDescending(p => p.PACIENTE);

                    }
                    //students = students.OrderByDescending(s => s.LastName);
                    break;
                case "FECHAENTRADA":
                    if (order == "asc")
                    {
                        oResultado.rows = db.BolsaPruebas.Where(p => p.IOR_MEDICOREFERENTE == usuario).OrderBy(p => p.FECHAENTRADA);
                    }
                    else
                    {
                        oResultado.rows = db.BolsaPruebas.Where(p => p.IOR_MEDICOREFERENTE == usuario).OrderByDescending(p => p.FECHAENTRADA);

                    }
                    //students = students.OrderByDescending(s => s.LastName);
                    break;
                case "EXPLORACION":
                    if (order == "asc")
                    {
                        oResultado.rows = db.BolsaPruebas.Where(p => p.IOR_MEDICOREFERENTE == usuario).OrderBy(p => p.EXPLORACION);
                    }
                    else
                    {
                        oResultado.rows = db.BolsaPruebas.Where(p => p.IOR_MEDICOREFERENTE == usuario).OrderByDescending(p => p.EXPLORACION);

                    }
                    //students = students.OrderByDescending(s => s.LastName);
                    break;
                case "PRIORIDAD":
                    if (order == "asc")
                    {
                        oResultado.rows = db.BolsaPruebas.Where(p => p.IOR_MEDICOREFERENTE == usuario).OrderBy(p => p.PRIORIDAD);
                    }
                    else
                    {
                        oResultado.rows = db.BolsaPruebas.Where(p => p.IOR_MEDICOREFERENTE == usuario).OrderByDescending(p => p.PRIORIDAD);

                    }
                    //students = students.OrderByDescending(s => s.LastName);
                    break;
                case "EXPLORACIONAGENDADA.FECHA":
                    if (order == "asc")
                    {
                        oResultado.rows = db.BolsaPruebas.Where(p => p.IOR_MEDICOREFERENTE == usuario).OrderBy(p => p.EXPLORACIONAGENDADA.FECHA);
                    }
                    else
                    {
                        oResultado.rows = db.BolsaPruebas.Where(p => p.IOR_MEDICOREFERENTE == usuario).OrderByDescending(p => p.EXPLORACIONAGENDADA.FECHA);

                    }
                    //students = students.OrderByDescending(s => s.LastName);
                    break;
                
                default:
                    if (order == "asc")
                    {
                        oResultado.rows = db.BolsaPruebas.Where(p => p.IOR_MEDICOREFERENTE == usuario).OrderBy(p => p.OID);
                    }
                    else
                    {
                        oResultado.rows = db.BolsaPruebas.Where(p => p.IOR_MEDICOREFERENTE == usuario).OrderByDescending(p => p.OID);

                    }

                    break;
            }
         


            if (!String.IsNullOrEmpty(search))
            {
                oResultado.rows = oResultado.rows.Where(p => p.PACIENTE.Contains( search.ToUpper()));
            }


            oResultado.total = oResultado.rows.Count();
            oResultado.rows = oResultado.rows.Skip(offset).Take(limit);

            foreach (var item in oResultado.rows.ToList())
            {
                if (item.IOR_EXPLORACION > 0)
                {
                    item.EXPLORACIONAGENDADA = ListaDiaRepositorio.Obtener(item.IOR_EXPLORACION.Value);
                }
            }

            var json = Json(oResultado, JsonRequestBehavior.AllowGet);
            return json;

        }

        // GET: Peticiones/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            BOLSA_PRUEBAS bOLSA_PRUEBAS = db.BolsaPruebas.Find(id);
            if (bOLSA_PRUEBAS == null)
            {
                return HttpNotFound();
            }
            return View(bOLSA_PRUEBAS);
        }

        // GET: Peticiones/Create

        public ActionResult Create()
        {
            return View(new BOLSA_PRUEBAS());
        }



        // POST: Peticiones/Create
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        public ActionResult Create([Bind(Include = "OID,NOMBRE,APELLIDOS,MAIL,TELEFONO1,TELEFONO2,DIRECCION,POBLACION,PROVINCIA,FECHAPRUEBA,HORAPRUEBA,PRUEBA,COMENTARIO,IOR_TIPOEXPLORACION,IOR_ENTIDADPAGADORA,FECHARESULTADO,PRIORIDAD,IOR_GAPARATO")] BOLSA_PRUEBAS oViewModel)
        {
            if (ModelState.IsValid)
            {
                oViewModel.PACIENTE = oViewModel.APELLIDOS.ToUpper() + ", " + oViewModel.NOMBRE.ToUpper();
                oViewModel.FECHAENTRADA = DateTime.Now;
                oViewModel.IOR_MEDICOREFERENTE = UsuariosRepositorio.Obtener(User.Identity.Name).PERSONAL.OID;
                oViewModel.EXPLORACION = AparatoRepositorio.Obtener(oViewModel.IOR_TIPOEXPLORACION).DES_FIL;// UsuariosRepositorio.Obtener(User.Identity.Name).PERSONAL.OID;
                oViewModel.IOR_GAPARATO = AparatoRepositorio.Obtener(oViewModel.IOR_TIPOEXPLORACION).OWNER.Value;
                oViewModel.IOR_EXPLORACION = -1;
                db.BolsaPruebas.Add(oViewModel);
                db.SaveChanges();
                db.Entry(oViewModel).GetDatabaseValues();
                return RedirectToAction("Create", "Peticiones");
                // return View(new BOLSA_PRUEBAS());

            }
            return new HttpStatusCodeResult(HttpStatusCode.InternalServerError);
            //  return View(oViewModel);
        }

        // GET: Peticiones/Edit/5
        public ActionResult Edit(int oid)
        {
            if (oid == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            BOLSA_PRUEBAS bOLSA_PRUEBAS = db.BolsaPruebas.Find(oid);
            if (bOLSA_PRUEBAS == null)
            {
                return HttpNotFound();
            }
            bOLSA_PRUEBAS.NOMBRE = bOLSA_PRUEBAS.PACIENTE.Split(',')[1];
            bOLSA_PRUEBAS.APELLIDOS = bOLSA_PRUEBAS.PACIENTE.Split(',')[0];
            // bOLSA_PRUEBAS.FECHARESULTADO = bOLSA_PRUEBAS.FECHARESULTADO.ToShortDateString();

            return View("Create", bOLSA_PRUEBAS);
        }

        // POST: Peticiones/Edit/5
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        public ActionResult Edit([Bind(Include = "OID,NOMBRE,APELLIDOS,MAIL,TELEFONO1,TELEFONO2,DIRECCION,POBLACION,PROVINCIA,FECHAPRUEBA,HORAPRUEBA,PRUEBA,COMENTARIO,IOR_TIPOEXPLORACION,IOR_ENTIDADPAGADORA,FECHARESULTADO,PRIORIDAD,IOR_GAPARATO")] BOLSA_PRUEBAS bOLSA_PRUEBAS)
        {
            if (ModelState.IsValid)
            {
                bOLSA_PRUEBAS.PACIENTE = bOLSA_PRUEBAS.APELLIDOS.ToUpper() + ", " + bOLSA_PRUEBAS.NOMBRE.ToUpper().Trim();
                bOLSA_PRUEBAS.FECHAENTRADA = DateTime.Now;
                bOLSA_PRUEBAS.IOR_MEDICOREFERENTE = UsuariosRepositorio.Obtener(User.Identity.Name).PERSONAL.OID;
                bOLSA_PRUEBAS.EXPLORACION = AparatoRepositorio.Obtener(bOLSA_PRUEBAS.IOR_TIPOEXPLORACION).DES_FIL;
                bOLSA_PRUEBAS.IOR_GAPARATO = AparatoRepositorio.Obtener(bOLSA_PRUEBAS.IOR_TIPOEXPLORACION).OWNER.Value;

                bOLSA_PRUEBAS.IOR_EXPLORACION = -1;
                db.Entry(bOLSA_PRUEBAS).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Create", new
                {
                    OID = bOLSA_PRUEBAS.OID

                });
            }
            return View(bOLSA_PRUEBAS);
        }

        // GET: Peticiones/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            BOLSA_PRUEBAS bOLSA_PRUEBAS = db.BolsaPruebas.Find(id);
            if (bOLSA_PRUEBAS == null)
            {
                return HttpNotFound();
            }
            return View(bOLSA_PRUEBAS);
        }

        // POST: Peticiones/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            BOLSA_PRUEBAS bOLSA_PRUEBAS = db.BolsaPruebas.Find(id);
            db.BolsaPruebas.Remove(bOLSA_PRUEBAS);
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
