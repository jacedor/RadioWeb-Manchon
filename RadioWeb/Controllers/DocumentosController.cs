using RadioWeb.Models;
using RadioWeb.Models.Repos;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Net;
using RadioWeb.ViewModels.Documentos;
using ADPM.Common.Web.DataAnnotations;
using RadioWeb.Models.Logica;
using System.Data.Entity;

namespace RadioWeb.Controllers
{
   
    public class DocumentosController : Controller
    {
        private RadioDBContext db = new RadioDBContext();

        
        [HttpPost]
        public ActionResult UploadAudio(HttpPostedFileBase file, int OIDEXPLORACIONDOCS)
        {
            IMAGENES oImagen=new IMAGENES();
            oImagen.OID = -1;
            if (file != null && file.ContentLength > 0)
                try
                {
                    EXPLORACION oExplo = ExploracionRepositorio.Obtener(OIDEXPLORACIONDOCS);

                   
                     oImagen = new IMAGENES
                    {
                        IOR_PACIENTE = oExplo.IOR_PACIENTE,
                        IOR_EXPLORACION = oExplo.OID,
                        EXT ="wav",
                        OWNER=20,
                        PATH = Utils.Varios.ObtenerCarpetaDocumentosEscaneados(),
                        USERNAME= User.Identity.Name
                        
                        //OWNER = TipoDocumento
                    };

                    oImagen.OID = int.Parse(ImagenesRepositorio.Insertar(oImagen));

                    file.SaveAs(Utils.Varios.ObtenerCarpetaDocumentosEscaneados() + oImagen.NOMBRE +"."+ oImagen.EXT);

                    ExploracionRepositorio.UpdateCampo("RECORDED", "T", oExplo.OID);
                    ViewBag.Message = "Fichero asociado a Exploracion";
                }
                catch (Exception ex)
                {
                    ViewBag.Message = "ERROR:" + ex.Message.ToString();
                }
            else
            {
                ViewBag.Message = "No has seleccionado fichero.";
            }
            //   ImagenesRepositorio.Obtener()
            return Json(oImagen.OID.ToString() + ".wav" + "(" + oImagen.USERNAME + ")", JsonRequestBehavior.AllowGet);
            //return new HttpStatusCodeResult(HttpStatusCode.OK, ));
        }

        [HttpPost]
        public ActionResult Upload(HttpPostedFileBase file, int OIDEXPLORACIONDOCS)
        {
            if (file != null && file.ContentLength > 0)
                try
                {
                    EXPLORACION oExplo = ExploracionRepositorio.Obtener(OIDEXPLORACIONDOCS);

                    FileInfo oFileInfo = new FileInfo(file.FileName);

                    string nombreDocumento = Path.GetFileNameWithoutExtension(oFileInfo.Name);
                    if(nombreDocumento.Length >= 50)
                    {
                        nombreDocumento = nombreDocumento.Substring(0, 30);
                    }

                    IMAGENES oImagen = new IMAGENES
                    {
                        IOR_PACIENTE = oExplo.IOR_PACIENTE,
                        IOR_EXPLORACION = oExplo.OID,
                        EXT = oFileInfo.Extension,
                        PATH = Utils.Varios.ObtenerCarpetaDocumentosEscaneados(),
                        NOMBRE = nombreDocumento
                        //OWNER = TipoDocumento
                    };

                    ImagenesRepositorio.Insertar(oImagen);

                    oImagen.OID = db.Imagenes.Where(p => p.IOR_PACIENTE == oExplo.IOR_PACIENTE
                                    && p.IOR_EXPLORACION == oExplo.OID).OrderByDescending(p => p.FECHA).FirstOrDefault().OID;

                    file.SaveAs( Utils.Varios.ObtenerCarpetaDocumentosEscaneados() + oImagen.NOMBRE + oImagen.EXT);
                    
                    ViewBag.Message = "Fichero asociado a Exploracion";
                }
                catch (Exception ex)
                {
                    ViewBag.Message = "ERROR:" + ex.Message.ToString();
                }
            else
            {
                ViewBag.Message = "No has seleccionado fichero.";
            }
            return new HttpStatusCodeResult(HttpStatusCode.OK);
        }

        public ActionResult Buscar(int oidExploracion, int tipoDocumento)
        {
            IMAGENES oDocumento = null;
            try
            {
                oDocumento = db.Imagenes.SingleOrDefault( p => p.OWNER == tipoDocumento && p.BORRADO!="T" &&
                           p.IOR_EXPLORACION == oidExploracion);
                if (oDocumento == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.NotFound);
                }
            }
            catch (Exception ex)
            {

                return new HttpStatusCodeResult(HttpStatusCode.InternalServerError, ex.Message);
            }
            //Si el documento ya ha sido firmado en la tablet. 
            if (oDocumento.CID == 1)
            {
                return new HttpStatusCodeResult(HttpStatusCode.OK, "FIRMADO");
            }

            return new HttpStatusCodeResult(HttpStatusCode.OK, oDocumento.OID.ToString());
        }

        [HttpDelete]
        public ActionResult Delete(int oid)
        {
            IMAGENES oDocumento = null;
            try
            {

                oDocumento = db.Imagenes.SingleOrDefault(p => p.OID == oid);

                if (oDocumento != null)
                {
                    //Puede ser que al intentar borrar el fichero este abierto y no se pueda borrar, 
                    //en cuyo caso solo eliminariamos el registro de base de datos de este fichero
                    try
                    {
                        //para no perder el documento decidimos no borrar el fichero físico. 
                        if (System.IO.File.Exists((oDocumento.PATH + oDocumento.NOMBRE + "." + oDocumento.EXT)))
                        {
                            System.IO.File.Delete(oDocumento.PATH + oDocumento.NOMBRE + "." + oDocumento.EXT);
                        }
                    }
                    catch (Exception)
                    {


                    }                    

                    oDocumento.BORRADO = "T";
                    //db.Imagenes.Remove(oDocumento);
                    db.Entry(oDocumento).State = EntityState.Modified;
                    db.SaveChanges();

                    int QuedanAudiosAsociadosAInforme = db.Imagenes.Where(p => p.IOR_EXPLORACION == oDocumento.IOR_EXPLORACION
                    && p.BORRADO != "T" && p.OWNER == 20).Count();
                    if (QuedanAudiosAsociadosAInforme == 0)
                    {
                        ExploracionRepositorio.UpdateCampo("RECORDED", "F", oDocumento.IOR_EXPLORACION.Value);
                    }

                }
            }
            catch (Exception ex)
            {

                return new HttpStatusCodeResult(HttpStatusCode.InternalServerError, ex.Message);
            }

            return new HttpStatusCodeResult(HttpStatusCode.OK);
        }

        [HttpPost]
        public ActionResult Edit(List<IMAGENES> DOCUMENTOS)
        {
            ResultadoRequest oResult;
           
            try
            {
                foreach (var item in DOCUMENTOS)
                {
                    IMAGENES odoc = db.Imagenes.Single(i => i.OID == item.OID);
                    db.Entry(odoc).State = EntityState.Modified;
                    odoc.OWNER = item.OWNER;
                   
                }
                db.SaveChanges();

            }
            catch (Exception ex)
            {
                oResult = new ResultadoRequest()
                {
                    Mensaje = "Se ha producido un error al guardar los documentos. " + ex.Message,
                    Resultado = ResultadoRequest.RESULTADO.ERROR
                };
                return PartialView("_ResultRequest", oResult);
            }

            oResult = new ResultadoRequest()
            {
                Mensaje = "Se ha guardado los Documentos Correctamente. ",
                Resultado = ResultadoRequest.RESULTADO.SUCCESS
            };
            return PartialView("_ResultRequest", oResult);


        }

      
        public ActionResult List(int OID)
        {
            List<IMAGENES> oViewModel = new List<IMAGENES>();
            ViewBag.OIDEXPLORACIONDOCS = OID;
            WebConfigRepositorio oConfig = new WebConfigRepositorio();
            bool hayDocumentos = false;

            foreach (IMAGENES item in ImagenesRepositorio.Obtener(OID, true))
            {
                hayDocumentos = true;
                item.SOLOLECTURA = false;
                oViewModel.Add(item);
            }

            foreach (var firmados in db.Vid_Documentos.Where(d=>d.OWNER== OID))
            {
                hayDocumentos = true;

               
                if (System.IO.File.Exists(firmados.NOMBRE))
                {
                    FileInfo oFile = new FileInfo(firmados.NOMBRE);
                    oViewModel.Add(new IMAGENES
                    {
                        OID = firmados.OID,
                        SOLOLECTURA = true,
                        NOMBRE = (firmados.NOMBRE.Contains("LOPD") ? "L.O.P.D." : oFile.Name),
                        PATH= firmados.NOMBRE,
                        FECHA= DateTime.Parse( firmados.FECHA),
                        TIPO = (firmados.NOMBRE.Contains("LOPD") ? "L.O.P.D." : "CONSENTIMIENTO")

                    });
                }
                
            }


            if (hayDocumentos)
            {
                LISTADIA oExplo2 = ListaDiaRepositorio.Obtener(OID);
                //EXPLORACION oExplo = ExploracionRepositorio.Obtener(oViewModel.First().IOR_EXPLORACION.Value);
                ViewBag.Tablets = db.Tabletas.Where(p => p.IOR_CENTRO == oExplo2.CENTRO).ToList();
            } 

            return PartialView(oViewModel);
        }

        public ActionResult ListaPaciente(int oid)
        {
            VMDocumentosPaciente oViewModel = new VMDocumentosPaciente();
            oViewModel.DOCUMENTOS = db.Imagenes
                .Where(p => p.IOR_PACIENTE == oid && p.BORRADO != "T")
                .ToList();
            foreach (var firmados in db.Vid_Documentos.Where(d => d.IOR_PACIENTE == oid))
            {



                if (System.IO.File.Exists(firmados.NOMBRE))
                {
                    FileInfo oFile = new FileInfo(firmados.NOMBRE);
                    oViewModel.DOCUMENTOS.Add(new IMAGENES
                    {
                        OID = firmados.OID,
                        SOLOLECTURA = true,
                        NOMBRE = (firmados.NOMBRE.Contains("LOPD") ? "L.O.P.D." : oFile.Name),
                        PATH = firmados.NOMBRE,
                        FECHA = DateTime.Parse(firmados.FECHA),
                        TIPO = (firmados.NOMBRE.Contains("LOPD") ? "L.O.P.D." : "CONSENTIMIENTO")

                    });
                }

            }
            return PartialView("List", oViewModel.DOCUMENTOS);
        }


        // GET: Documentos en la carpeta temporal de escaneo
        public ActionResult Index()
        {
            List<IMAGENES> oViewModel = new List<IMAGENES>();
            WebConfigRepositorio oConfig = new WebConfigRepositorio();

            USUARIO usuarioAutenticated = (USUARIO)System.Web.HttpContext.Current.Session["Usuario"];

            string RutaEscaneo = oConfig.ObtenerValor("RutaDocumentosEscaneadosTemp") + @"\" + usuarioAutenticated.LOGIN;
            if (!Directory.Exists(RutaEscaneo))
            {
                Directory.CreateDirectory(RutaEscaneo);
            }

            foreach (string item in Directory.GetFiles(RutaEscaneo))
            {
                FileInfo oFileInfo = new FileInfo(item);
                oViewModel.Add(new IMAGENES
                {
                    FECHA = oFileInfo.CreationTime,
                    NOMBRE = oFileInfo.Name,
                    EXT = oFileInfo.Extension,
                    PATH = oFileInfo.FullName
                });
            }
            var tiposDocumento = from s in db.Refractometros
                                 where s.CID == 6 // if you want to filer the results
                                 select s;
            ViewBag.TiposDocumento = new SelectList(tiposDocumento, "OID", "NOMBRE");
            return PartialView(oViewModel);
        }


       
        [HttpPost]
        public ActionResult Index(int oidExploracion, string NombreDocumento, string Accion, int TipoDocumento)
        {
            string result = "";
            EXPLORACION oExplo = ExploracionRepositorio.Obtener(oidExploracion);

            switch (Accion.ToUpper())
            {
                case "ELIMINAR":
                    if (System.IO.File.Exists(NombreDocumento))
                    {
                        try
                        {
                            System.IO.File.Delete(NombreDocumento);
                            result = "Fichero Eliminado";
                        }
                        catch (Exception)
                        {

                            return new HttpStatusCodeResult(HttpStatusCode.InternalServerError, "No se ha podido eliminar el Documento");
                        }
                    }
                    break;
                case "ASOCIAR":
                    FileInfo oFileInfo = new FileInfo(NombreDocumento);
                    IMAGENES oImagen = new IMAGENES
                    {
                        IOR_PACIENTE = oExplo.IOR_PACIENTE,
                        IOR_EXPLORACION = oExplo.OID,
                        EXT = oFileInfo.Extension,
                        PATH = Utils.Varios.ObtenerCarpetaDocumentosEscaneados(),
                        OWNER = TipoDocumento
                    };

                    oImagen.OID = int.Parse(ImagenesRepositorio.Insertar(oImagen));

                    System.IO.File.Copy(NombreDocumento, Utils.Varios.ObtenerCarpetaDocumentosEscaneados() + oImagen.NOMBRE + oImagen.EXT);


                    result = "Fichero Asociado a " + oExplo.PACIENTE.PACIENTE1;
                    try
                    {
                        System.IO.File.Delete(NombreDocumento);
                    }
                    catch (Exception)
                    {


                    }
                    break;
                default:
                    break;
            }


            return new HttpStatusCodeResult(HttpStatusCode.OK, result);
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