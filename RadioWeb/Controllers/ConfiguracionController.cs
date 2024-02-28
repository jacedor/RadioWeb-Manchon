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
using FirebirdSql.Data.FirebirdClient;

namespace RadioWeb.Controllers
{
    public class ConfiguracionController : Controller
    {
        private RadioDBContext db = new RadioDBContext();

        // GET: Configuracion
        public ActionResult Global()
        {

            ViewBag.Secciones = db.WebConfig
                .Where(x => x.SECCION != "GENERAL")
                .ToList()
                .Select(x => x.SECCION)
                .Distinct();
#if DEBUG
            var parametros = db.WebConfig
                .Where(p => p.VERS.Value == 1 && p.SECCION.ToString().ToUpper() == "GENERAL").ToList();
#else
           var parametros = db.WebConfig
               .Where(p => p.VERS.Value == 1 && p.SECCION.ToString().ToUpper() == "GENERAL").ToList();
#endif
            return View(parametros);
        }

        [HttpPost]
        public ActionResult Global(string seccion)
        {
            ViewBag.Seccion = seccion;


#if DEBUG
            var parametros = db.WebConfig
                .Where(p => p.VERS.Value == 1
                        && p.SECCION == seccion.ToUpper())
                        .ToList();
#else
            var parametros = db.WebConfig
                .Where(p => p.VERS.Value == 0 
                        && p.SECCION.ToString().ToUpper()==seccion.ToUpper() )
                        .ToList();
#endif

            return PartialView("_List", parametros);
        }

        [HttpPost]
        public ActionResult EditarCampo(string name, int pk, string value)
        {

            FbConnection oConexion = new FbConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ConexionBD"].ConnectionString);
            FbCommand oCommand = null;
            try
            {
                oConexion.Open();
                string updateStament = "update WEBCONFIG set " + name + "='" + value + "'";
                updateStament += " where oid= " + pk;
                oCommand = new FbCommand(updateStament, oConexion);
                oCommand.ExecuteNonQuery();
            }
            catch (Exception)
            {


            }
            finally
            {

                if (oConexion.State == System.Data.ConnectionState.Open)
                {
                    oConexion.Close();
                    if (oCommand != null)
                    {
                        oCommand.Dispose();
                    }

                }

            }

            return new HttpStatusCodeResult(200);
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
