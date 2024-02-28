using RadioWeb.Models;
using RadioWeb.Models.Repos;
using RadioWeb.Utils;
using RadioWeb.ViewModels.Informes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace RadioWeb.Controllers
{
    public class P_InformeController : Controller
    {

        private RadioDBContext db = new RadioDBContext();
        // GET: Facturas


        private bool GuardarInforme(VMP_Informe oPInforme)
        {
            bool success = false;
            var PlantillaToUpdate = db.P_Informes.Single(i => i.OID == oPInforme.OID);
                       
            PlantillaToUpdate.FECHA = DateTime.Now;
            PlantillaToUpdate.CANAL = oPInforme.IDIOMA;
            PlantillaToUpdate.TITULO = oPInforme.TITULO;
            PlantillaToUpdate.CID = oPInforme.TIPO;
            db.SaveChanges();

            int idInforme = oPInforme.OID;
            TEXTOS oTexto = new TEXTOS();
            oTexto.TEXTO = oPInforme.TEXTOHTML;
            oTexto.TEXTO = oTexto.TEXTO.Replace("<title>Untitled document</title>", "");
            oTexto.TEXTO = oTexto.TEXTO.Replace("st1", "233");
            oTexto.TEXTO = oTexto.TEXTO.Replace("st2", "433");
            oTexto.OWNER = oPInforme.OID;
            TextosRepositorio.InsertarOrUpdate(oTexto);
            return true;
        }

 
        public ActionResult Admin()
        {
            
            ViewBag.IDIOMAS = DataBase.Idiomas();
            ViewBag.TIPOS = DataBase.TiposPlantillas();
            List<P_INFORMES> oListTemp = db.P_Informes
                                            //.Where(p => p.CID > 0)
                                            .OrderBy(p => p.TITULO).ToList();
            return View("Admin", oListTemp);

        }

        [HttpPost]
        public ActionResult Admin(string Idioma, int Tipo)
        {

           var oListTemp = db.P_Informes.Where(p => p.CANAL == Idioma);

            if (Tipo < 0)
            {
                oListTemp = oListTemp.Where(p => p.CID == null);
            }
            else
            {
                oListTemp = oListTemp.Where(p => p.CID == Tipo);

            }
            oListTemp=oListTemp.OrderBy(p => p.TITULO);
            return PartialView("_Lista", oListTemp.ToList());

        }


        public ActionResult Index()
        {
            List<P_INFORMES> oListTemp = P_InformesRepositorio.Lista();
            USUARIO usuario = (USUARIO)Session["Usuario"];

            if (usuario.DESCPERFILWEB == "RW-MedicoInformante" || usuario.DESCPERFILWEB == "Administrador")
            {
                PERSONAL oPersonal = db.Personal
                    .Where(p => p.LOGIN == usuario.LOGIN).First();
                ViewBag.oidMedico = oPersonal.OID;
               
                oListTemp = oListTemp
                    .Where(p => p.OWNER == oPersonal.OID)
                    .OrderBy(p => p.TITULO)
                    .ToList();
            }

            return PartialView("_Lista", oListTemp);
        }

        [HttpPost]
        public ActionResult Index(int oidPersonal, int oidExploracion)
        {
            //List<P_INFORMES> oListTemp = P_InformesRepositorio.Lista();
            //ViewBag.OidExploracion = oidExploracion;
            //Usuario usuario = (Usuario)Session["Usuario"];
            //oListTemp = oListTemp.Where(p => p.OWNER == oidPersonal).OrderBy(p => p.TITULO).ToList();
            //return PartialView("Index", oListTemp);

            ViewBag.OidExploracion = oidExploracion;
            List<P_INFORMES> oListTemp = db.P_Informes
                                            .Where(p => p.OWNER == oidPersonal)
                                            .OrderBy(p => p.TITULO).ToList();
            return PartialView("_Lista", oListTemp);

        }

        public ActionResult Edit(int OID)
        {
            USUARIO usuario = (USUARIO)Session["Usuario"];
            if (AuthorizeRepositorio.Autorizar(usuario, "/P_Informe/Edit") || usuario.DESCPERFILWEB == "Administrador")
            {
                ViewBag.Title = "Plantillas RW";
                VMP_Informe viewModel = new VMP_Informe(OID);

               
                return View("Editor", viewModel);
            }
            else
            {
                return Content("Acceso no permitido.");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(VMP_Informe viewModel)
        {
            USUARIO usuario = (USUARIO)Session["Usuario"];
            if (AuthorizeRepositorio.Autorizar(usuario, "/Informe/Create") || usuario.DESCPERFILWEB == "Administrador")
            {
                GuardarInforme(viewModel);
                viewModel = new VMP_Informe(viewModel.OID);
                
                return View("Editor", viewModel);


            }
            else
            {
                return Content("Acceso no permitido.");
            }
        }


        public string PrevisualizarPlantilla(int oidPlantilla)
        {
            return P_InformesRepositorio.ObtenerHtmlDelInforme(oidPlantilla);
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
