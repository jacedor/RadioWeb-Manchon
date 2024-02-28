using Newtonsoft.Json;
using RadioWeb.Models;
using RadioWeb.Models.Repos;
using RadioWeb.Utils;
using RadioWeb.ViewModels;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;

namespace RadioWeb.Controllers
{
    [Authorize]
    public class KioskoController : Controller
    {
        private RadioDBContext db = new RadioDBContext();


        //GET kiosko/tv
        public ActionResult TV(int? oid)
        {

            //Si no hay OID. Listar tabla.
            if (oid == null)
            {
                return View(db.KioskoTV.OrderBy(p => p.NOMBRE));
            }

            KIOSKO_TV tv = null;

            //Si el oid es 0. Nueva.
            if (oid == 0)
            {
                tv = new KIOSKO_TV();
                return View("TVForm", tv);
            }

            //Si el oid existe. Edit
            tv = db.KioskoTV.Find(oid);
            if (tv == null)
            {
                return HttpNotFound();
            }
            return View("TVForm", tv);
        }


        // POST: kiosko/tv
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult TV(KIOSKO_TV tv)
        {

            if (ModelState.IsValid)
            {

                if (tv.ACTION == "Create")
                {
                    db.KioskoTV.Add(tv);
                    db.SaveChanges();
                }

                else if (tv.ACTION == "Update")
                {
                    db.Entry(tv).State = EntityState.Modified;
                    db.SaveChanges();
                }

            }

            RouteData.Values.Remove("oid");
            return RedirectToAction("TV");
        }


        // GET: kiosko/TVDelete/5
        public ActionResult TVDelete(int oid)
        {
            if (oid == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            KIOSKO_TV tv = db.KioskoTV.Find(oid);
            if (oid != null)
            {
                db.KioskoTV.Remove(tv);
                db.SaveChanges();
            }

            return RedirectToAction("TV");
        }


        //GET: kiosko/agendas
        public ActionResult Agendas() {

            //TVs
            List<KIOSKO_TV> oPantallas = db.KioskoTV.ToList();
            ViewBag.pantallas = oPantallas;

            //Aparatos sin asignar
            List<DAPARATOS> oAparatos = KioskoDaparatoTVRepositorio.DaparatosSinSala();
            ViewBag.aparatos = oAparatos;

            //Aparatos asignados
            List<DAPARATOS> oAparatosAsignados = KioskoDaparatoTVRepositorio.DaparatosConSala();
            ViewBag.aparatosAsignados = oAparatosAsignados;

            return View();
        }

        [HttpPost]
        public ActionResult agendaUpdate(string tvId, string daparatoId) {


            //Buscamos si existe el registro y lo borramos.
            KIOSKO_DAPARATO_TV daparatoTv = db.KioskoDaparatoTV.Where(p => p.DAPARATO_OID.ToString() == daparatoId).FirstOrDefault();
            if(daparatoTv != null) {
                db.KioskoDaparatoTV.Remove(daparatoTv);
                db.SaveChanges();
            }
            daparatoTv = null;

            //Han definido una sala por lo tanto generamos registro.
            if (tvId != null && tvId != "") {
                KIOSKO_DAPARATO_TV newDaparatoTv = new KIOSKO_DAPARATO_TV();
                newDaparatoTv.DAPARATO_OID = Int32.Parse(daparatoId);
                newDaparatoTv.TV_OID = Int32.Parse(tvId);
                db.KioskoDaparatoTV.Add(newDaparatoTv);
                db.SaveChanges();
            }

            return Json("success");
        }

        public ActionResult Notificar(string oid, string action) {

            Dictionary<string, string> response = new Dictionary<string, string>();

            WebConfigRepositorio oConfig = new WebConfigRepositorio();
            string endPoint = oConfig.ObtenerValor("RutaKiosko");
            if (action == "call")
            {
                endPoint = endPoint + "api/viewer/callticket/";
            }
            else if (action == "cancel") 
            {
                endPoint = endPoint + "api/viewer/endticket/";
            }

            Dictionary<string, string>[] queryResult = KioskoDaparatoTVRepositorio.ObtenerPorExploracion(oid);
            if (queryResult == null) {
                response.Add("success", "false");
                response.Add("message", "El aparato no está configurado en ninguna sala.");
                return Json(response);
            }


            String viewerId = queryResult[0]["viewer_id"];
            String deskId = queryResult[0]["desk_id"];
            String deskname = queryResult[0]["deskname"];
            String label = queryResult[0]["label"];

            if (label == null || label == "") {
                response.Add("success", "false");
                response.Add("message", "La exploración no tiene informado el código del ticket.");
                return Json(response);
            }

            Boolean success = true;
            int i = 0;
            while (queryResult[i] != null && success) {
                success = HttpEnviar(endPoint, queryResult[i]["viewer_id"], queryResult[i]["desk_id"], queryResult[i]["deskname"], queryResult[i]["label"]);
                i++;
            }

            
            if (!success) {
                response.Add("success", "false");
                response.Add("message", "Se ha producido un error inesperado al enviar al WebService. Intentelo de nuevo.");
                return Json(response);
            }

            response.Add("success", "true");
            response.Add("message", "");
            return Json(response);
        }


        public Boolean HttpEnviar(String endPoint, string viewer_id, string desk_id, string deskname, string label)
        {
            Boolean success = false;

            /*
            Dictionary<string, string> postParameters = new Dictionary<string, string>();
            postParameters.Add("desk_id", desk_id);
            postParameters.Add("deskname", deskname);
            postParameters.Add("label", label);
            string jsonData = JsonConvert.SerializeObject(postParameters);
            */


            string jsonData = new JavaScriptSerializer().Serialize(new
            {
                desk_id = desk_id,
                deskname = deskname,
                label = label
            });

            LogException.LogMessageToFile("Kiosko", endPoint + viewer_id);
            LogException.LogMessageToFile("Kiosko", jsonData);




            try {                

                var httpWebRequest = (HttpWebRequest)WebRequest.Create(endPoint + viewer_id);
                httpWebRequest.ContentType = "application/json";
                httpWebRequest.Method = "POST";

                using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
                {
                    streamWriter.Write(jsonData);
                }

                var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                LogException.LogMessageToFile("Kiosko", "Response Code: " + httpResponse.StatusDescription);
                if (httpResponse.StatusCode == HttpStatusCode.OK) {
                    success = true;
                }

                //using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                //{
                    //response = streamReader.ReadToEnd();
                //}
            }
            catch (Exception e) {
                success = false;
            }

            return success;

        }

    }
}
