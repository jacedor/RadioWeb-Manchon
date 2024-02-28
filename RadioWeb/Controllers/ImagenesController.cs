using RadioWeb.Models;
using RadioWeb.Models.Repos;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace RadioWeb.Controllers
{
    public class ImagenesController : Controller
    {

        private RadioDBContext db = new RadioDBContext();


        [HttpPost]
        public JsonResult GuardarImagenWebCam(string data_uri, int oid)
        {

            if (data_uri != null)
            {                

                int ior_paciente = ExploracionRepositorio.Obtener(oid).IOR_PACIENTE;

                IMAGENES oImagen = new IMAGENES { IOR_PACIENTE = ior_paciente,
                    IOR_EXPLORACION = oid,
                    EXT = "jpg",
                    PATH = Utils.Varios.ObtenerCarpetaImagen() };

                ImagenesRepositorio.Insertar(oImagen);
                

                byte[] data = Convert.FromBase64String(data_uri);
                using (var imageFile = new FileStream(oImagen.PATH + oImagen.NOMBRE + ".jpg", FileMode.Create))
                {
                    imageFile.Write(data, 0, data.Length);
                    imageFile.Flush();
                }


            }
            return Json(new { success = true }, JsonRequestBehavior.AllowGet);

        }

        [HttpPost]
        public ActionResult ListaPartial(int oid)
        {
            return PartialView("Carrousel", ImagenesRepositorio.Obtener(oid));
        }

        [HttpPost]
        public ActionResult ListaPartialImagenes(int oid)
        {
            return PartialView("Carrousel", ImagenesRepositorio.Obtener(oid,false));
        }

        public ActionResult Create(int OID)
        {
            EXPLORACION oExploracion = ExploracionRepositorio.Obtener(OID);
            IMAGENES viewModel = new IMAGENES();
            viewModel.IOR_EXPLORACION = oExploracion.OID;
            viewModel.IOR_PACIENTE = oExploracion.IOR_PACIENTE;
            viewModel.FECHAEXPLORACION = oExploracion.FECHA.Value.ToShortDateString();
            viewModel.HORA = oExploracion.HORA;
            viewModel.NOMBREPACIENTE = oExploracion.PACIENTE.PACIENTE1;            
            return View(viewModel);
        }

        [HttpPost]
        public ActionResult Create(IMAGENES img, HttpPostedFileBase file)
        {
            if (ModelState.IsValid)
            {
                if (file != null)
                {
                    System.IO.FileInfo fInfo = new System.IO.FileInfo(file.FileName);
                    
                    file.SaveAs(Utils.Varios.ObtenerCarpetaImagen() + img.NOMBRE  + fInfo.Extension.ToString().ToLower());
                   
                    IMAGENES oImagen = new IMAGENES {NOMBRE= img.NOMBRE,
                        IOR_PACIENTE = img.IOR_PACIENTE,
                        IOR_EXPLORACION = img.IOR_EXPLORACION, 
                    EXT = fInfo.Extension, PATH = Utils.Varios.ObtenerCarpetaImagen() };

                    oImagen.OID = int.Parse( ImagenesRepositorio.Insertar(oImagen));

                }

            }
            return View(img);
        }
        private string GetMimeType(string fileName)
        {
            string mimeType = "application/unknown";
            string ext = System.IO.Path.GetExtension(fileName).ToLower();
            Microsoft.Win32.RegistryKey regKey = Microsoft.Win32.Registry.ClassesRoot.OpenSubKey(ext);
            if (regKey != null && regKey.GetValue("Content Type") != null)
                mimeType = regKey.GetValue("Content Type").ToString();
            return mimeType;
        }

        public FileContentResult Imprimir(int oid)
        {
            IMAGENES oDocumento = db.Imagenes.Where(p => p.OID == oid).SingleOrDefault();

            byte[] fileBytes = null;
            string nombreDocumento = "";

            if (oDocumento == null)
            {
                VID_DOCUMENTOS oDocumentoFirmado = db.Vid_Documentos.Where(p => p.OID == oid).SingleOrDefault();
                fileBytes = System.IO.File.ReadAllBytes(oDocumentoFirmado.NOMBRE);
                nombreDocumento = oDocumentoFirmado.NOMBRE;
            }
            else
            {
                nombreDocumento = oDocumento.PATH + oDocumento.NOMBRE + "." + oDocumento.EXT;

                fileBytes = null;
                if (System.IO.File.Exists(nombreDocumento))
                {
                    fileBytes = System.IO.File.ReadAllBytes(nombreDocumento);
                }
            }
      
            return File(fileBytes,GetMimeType(nombreDocumento));
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
