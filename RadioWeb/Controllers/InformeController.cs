using RadioWeb.Models;
using RadioWeb.Models.Repos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using FastReport;
using FastReport.Web;
using FastReport.Export.Pdf;
using FirebirdSql.Data.FirebirdClient;
using System.IO;
using System.Web.UI.WebControls;
using MvcRazorToPdf;
using iTextSharp.text;
using System.Web.Hosting;
using iTextSharp.text.pdf;
using SelectPdf;
using System.Net.Http;
using System.Net;
using RadioWeb.ViewModels.Informes;
using ADPM.Common;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Diagnostics;
using System.Configuration;
using RadioWeb.ViewModels.Exploracion;
using RadioWeb.Utils;
using RadioWeb.Filters;
using RadioWeb.Models.Clases;
using RadioWeb.ViewModels.Paciente;
using FastReport.Cloud.OAuth;

namespace RadioWeb.Controllers
{
    [Authorize]
    public class InformeController : Controller
    {

        private RadioDBContext db = new RadioDBContext();

        private List<P_INFORMES> GetPlantillas(USUARIO oUsuario, string urlPrevia = "")
        {
            //obtenemos las plantillas de informes
            List<P_INFORMES> oListTemp = new List<P_INFORMES>();
            PERSONAL oPersonal = db.Personal.Where(p => p.LOGIN == oUsuario.LOGIN).FirstOrDefault();
            if (oPersonal == null)
            {
                oPersonal = db.Personal.Where(p => p.OID <= 0).FirstOrDefault();
            }
            ViewBag.urlPrevia = urlPrevia;
            ViewBag.oidMedico = oPersonal.OID;
            oListTemp = P_InformesRepositorio.Lista().Where(p => p.OWNER == oPersonal.OID).OrderBy(p => p.TITULO).ToList();
            return oListTemp;
        }

        private List<P_INFORMES> GetPlantillas(string sLogin, string urlPrevia = "")
        {
            //obtenemos las plantillas de informes
            List<P_INFORMES> oListTemp = new List<P_INFORMES>();
            ViewBag.urlPrevia = urlPrevia;
            PERSONAL oPersonal = db.Personal.Where(p => p.LOGIN == sLogin).FirstOrDefault();
            if (oPersonal == null)
            {
                oPersonal = db.Personal.Where(p => p.OID <= 0).FirstOrDefault();
            }
            ViewBag.oidMedico = oPersonal.OID;
            oListTemp = P_InformesRepositorio.Lista().Where(p => p.OWNER == oPersonal.OID).OrderBy(p => p.TITULO).ToList();
            return oListTemp;
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AutoGuardarTexto(VMInforme viewModel, string UrlPrevia = "")
        {
            var informeUpdated = db.Informes.Single(i => i.OID == viewModel.OID);
            informeUpdated.DURACION = viewModel.DURACION;
            db.SaveChanges();
            TEXTOS oTexto = new TEXTOS
            {
                TEXTO = viewModel.TEXTOHTML,
                OWNER = viewModel.OID
            };

            oTexto.TEXTO = oTexto.TEXTO.Replace("<title>Untitled document</title>", "");
            TextosRepositorio.InsertarOrUpdate(oTexto);
            return Json(true, JsonRequestBehavior.AllowGet);
        }

        
        public ActionResult ListaPaciente(int oidPaciente)
        {

            return PartialView("_InformesPaciente", InformesRepositorio.ObtenerPorPaciente(oidPaciente));
        }

        public ActionResult ListaExploracion(int oid)
        {

            return PartialView("_InformesPaciente", InformesRepositorio.ObtenerPorExploracion(oid));
        }


        public ActionResult PendientesRevisar(int oidPersonal)
        {
            List<VMInformesNoRevisados> oResult = new List<VMInformesNoRevisados>();
            var listaInformesRevisarBD = db.Informes
                                        .Where(i => i.IOR_MEDREVISION == oidPersonal
                                        && i.REVISADO == "F" && i.VALIDACION == "F").ToList();

            foreach (var item in listaInformesRevisarBD)
            {
                VMInformesNoRevisados oTemp = new VMInformesNoRevisados();
                LISTADIA oexplo = ListaDiaRepositorio.Obtener(item.OWNER.Value);
                PERSONAL oPersonalInforme = db.Personal.Where(p => p.OID == item.IOR_MEDINFORME).SingleOrDefault();

                oTemp.FECHA = item.FECHA;
                oTemp.MEDINFO = oPersonalInforme.NOMBRE;
                oTemp.PACIENTE = oexplo.PACIENTE;
                oTemp.CENTRO = CentrosRepositorio.Obtener(oexplo.CENTRO).NOMBRE.ToUpper();
                oTemp.OID = item.OID;
                oTemp.HORA = item.HORA;
                oTemp.TITULO = item.TITULO;
                oTemp.VALIDACION = item.VALIDACION;
                oTemp.IOR_EXPLORACION = item.OWNER.Value;
                oResult.Add(oTemp);
            }
            return PartialView("_PendientesRevisar", oResult);
        }


        //paso 1 realización de infomes
        public ActionResult ExploracionesPendientes(string filter = "")
        {
            USUARIO usuario = UsuariosRepositorio.Obtener(User.Identity.Name);
            PERSONAL oPersonal = db.Personal.Where(p => p.LOGIN == usuario.LOGIN).FirstOrDefault();
            WebConfigRepositorio oConfig = new WebConfigRepositorio();
            ViewBag.CampoMutua = oConfig.ObtenerValor("ComboMutuas").ToUpper();

            ViewBag.oidMedico = oPersonal.OID;

            List<VMExploNoInformadas> listaInformesPendientes = new List<VMExploNoInformadas>(); ;
            List<VMExploNoInformadas> listaInformesTemp = null;
            //INFORMES PENDIENTES
            if (usuario.PERFIL == 10 || usuario.PERFIL == 2)
            {
                VMExploNoInformadas.ULTIMOSINFORMES = new List<INFORMES>();
                int personalInformante = oPersonal.OID;
                if (filter == "all")
                {
                    personalInformante = 0;
                }
                //Obtenemos los informes pendientes 
                listaInformesPendientes = ExploracionRepositorio.ObtenerMedicoInformante(personalInformante);
            }
            else
            {
                //Si hemos llegado a esta pantalla y no somos medico informante enseñamos todos los                 
                listaInformesPendientes = ExploracionRepositorio.ObtenerMedicoInformante(0);
            }

            if (usuario.ESMEDICO)
            {
                VMExploNoInformadas.ULTIMOSINFORMES = db.Informes
                        .Where(t => t.IOR_MEDINFORME == oPersonal.OID)
                        .OrderByDescending(t => t.OID)
                        .Take(20);

                foreach (var item in VMExploNoInformadas.ULTIMOSINFORMES)
                {
                    LISTADIA oExplo = ListaDiaRepositorio.Obtener(item.OWNER.Value);
                    item.PACIENTE = oExplo.PACIENTE;
                    if (item.IOR_MEDREVISION == 0)
                    {
                        item.IOR_MEDREVISION = -1;
                    }
                    PERSONAL oPersonalRevisor = db.Personal.Where(p => p.OID == item.IOR_MEDREVISION)
                                                .FirstOrDefault();
                    item.MEDICOREVISOR = (oPersonalRevisor != null ? oPersonalRevisor.LOGIN : "");
                    item.FECHAEXPLORACION = oExplo.FECHA;
                    if (oExplo.FECHAMAXENTREGA.HasValue)
                    {
                        item.FECHAMAXIMAENTREGA = oExplo.FECHAMAXENTREGA.Value.ToShortDateString();
                    }

                }
            }

            return View(listaInformesPendientes);
        }

        //paso 2 realización de informes
        public ActionResult Duplicar(int OID, string ReturnUrl = "")
        {
            //Obtener(OID);


            ViewBag.OidExploracion = OID;
            VMDuplicarInforme oModel = new VMDuplicarInforme();
            WebConfigRepositorio oConfig = new WebConfigRepositorio();
            oModel.EXPLORACINOAINFORMAR = ExploracionRepositorio.Obtener(OID);// ListaDiaRepositorio.
            //Obtenemos los informes anteriores del paciente
            oModel.INFORMESPACIENTE = InformesRepositorio.ObtenerPorPaciente(oModel.EXPLORACINOAINFORMAR.IOR_PACIENTE);
            oModel.UsuarioLogeado = UsuariosRepositorio.Obtener(User.Identity.Name);
            oModel.UrlPreviaDuplicar = ReturnUrl;
            try
            {
                oModel.URL_PACS = oConfig.ObtenerValor("RUTAPACS" + oModel.EXPLORACINOAINFORMAR.OWNER).ToString()
                                   .Replace("@IOR_PAC", oModel.EXPLORACINOAINFORMAR.IOR_PACIENTE.ToString())
                                   .Replace("@OID_EXP", oModel.EXPLORACINOAINFORMAR.OID.ToString());

                if (oModel.URL_PACS.Contains("@COD_PAC"))
                {
                    PACIENTE oPaciente = PacienteRepositorio.Obtener(oModel.EXPLORACINOAINFORMAR.IOR_PACIENTE);
                    oModel.URL_PACS = oModel.URL_PACS.Replace("@COD_PAC", oPaciente.COD_PAC)
                        .Replace("@OID_EXP", oModel.EXPLORACINOAINFORMAR.OID.ToString());
                }
            }
            catch (Exception)
            {

                oModel.URL_PACS = "";
            }

            //obtenemos las plantillas de informes
            List<P_INFORMES> oListTemp = new List<P_INFORMES>();
            USUARIO usuario = (USUARIO)Session["Usuario"];

            //LOS PERFILES 10 Y 2 SON DE MEDICOS QUE INFORMAN, POR ESO CARGAMOS SUS PANTILLAS
            if (usuario.ESMEDICO || usuario.LOGIN.ToUpper() == "ADMIN")
            {
                PERSONAL oPersonal = db.Personal.SingleOrDefault(p => p.LOGIN == usuario.LOGIN);
                if (oPersonal==null)
                {
                    oPersonal = db.Personal.Where(p => p.OID <= 0).FirstOrDefault();
                }
                ViewBag.oidMedico = oPersonal.OID;
                oListTemp = GetPlantillas(usuario, ReturnUrl);

                if (oListTemp.Count == 0)
                {
                    //EXPLORACION oExplo = ExploracionRepositorio.Obtener(oExploracion.OID);
                    if (oModel.EXPLORACINOAINFORMAR.EMPRESA.NOMBRE.Contains("DELFOS"))
                    {
                        oListTemp = GetPlantillas("Roca", ReturnUrl);
                    }
                }

            }
            oModel.PLANTILLASINFORMES = oListTemp;


            return View(oModel);
        }


        public ActionResult Plantilla(int oid)
        {

            USUARIO usuario = UsuariosRepositorio.Obtener(User.Identity.Name);
            P_INFORMES oInforme = InformesRepositorio.ObtenerPlantilla(oid);
            PERSONAL oPersonal = db.Personal.Where(p => p.LOGIN == usuario.LOGIN).FirstOrDefault();
            if (oPersonal == null)
            {
                oPersonal = db.Personal.Where(p => p.OID <= 0).FirstOrDefault();
            }

            if (oInforme.TEXTOHTML.Contains("Firma M&eacute;dico") || oInforme.TEXTOHTML.Contains("Firma Médico") || oInforme.TEXTOHTML.Contains("Firma Medico"))
            {

                oInforme.TEXTOHTML.Replace("Firma M&eacute;dico", "<img src='/img/" + usuario.LOGIN + ".jpg>");
                oInforme.TEXTOHTML.Replace("Firma Médico", " < img src='/img/" + usuario.LOGIN + ".jpg>");
                oInforme.TEXTOHTML.Replace("Firma Médico", " < img src='/img/" + usuario.LOGIN + ".jpg>");
            }
            oInforme.TEXTOHTML = oInforme.TEXTOHTML.Replace("@MI", oPersonal.DESCRIPCION);
            oInforme.TEXTOHTML = oInforme.TEXTOHTML.Replace("@NU", oPersonal.NUMERO);

            return Json(oInforme, JsonRequestBehavior.AllowGet);

        }



        //devuelve vista parcial que incrustamos en la ventana modal para enviar un mail con el PDF del informe adjunto
        [HttpPost]
        public ActionResult GetParaEnvioMail(EMAIL email)
        {

            //Obtenemos el texto de la plantailla que tienen en el programa de massana
            EMAIL emailTemp = EmailRepositorio.Obtener(15517848);

            PACIENTE oPaciente = PacienteRepositorio.Obtener((int)email.IOR_PACIENTE);


            foreach (TELEFONO item in oPaciente.TELEFONOS)
            {
                if (item.NUMERO.StartsWith("6"))
                {
                    email.CANAL = item.NUMERO;
                }
            }
            EXPLORACION oExplo = ExploracionRepositorio.Obtener((int)email.CID);
            string descGrupo = GAparatoRepositorio.Obtener((int)oExplo.APARATO.OWNER).COD_GRUP;
            email.ASUNTO = "Informe  " + descGrupo;

            email.USERNAME = oPaciente.EMAIL;
            email.TEXTO = emailTemp.TEXTO;

            WebConfigRepositorio oConfig = new WebConfigRepositorio();
            string RutaDOCS = oConfig.ObtenerValor("RUTADOCS");
            string RutaMacrosCabecera = oConfig.ObtenerValor("RutaMacroCabeceraInformes");
            if (email.OWNER <= 0)
            {
                INFORMES oInforme = db.Informes.SingleOrDefault(i => i.OWNER == oExplo.OID && i.VALIDACION=="T");
                email.OWNER = oInforme.OID;
            }

            string fichero = InformesRepositorio.GenerarPDF(RutaDOCS, true, (int)email.OWNER,
                RutaMacrosCabecera, "OIDINFORME");
            email.ADJUNTO1 = fichero;
            email.ADJUNTO2 = new FileInfo(fichero).CreationTime.ToShortDateString();

            //Asignamos las configuraciones de LOPD al mail, para que se vean des de el modal de enviamiento de Email.
            

            //VMLOPDPaciente LOPDsettings = new VMLOPDPaciente {
            //    ENVIO_MEDICO = oPaciente.ENVIO_MEDICO,
            //    ENVIO_RESULTADOS = oPaciente.ENVIO_RESULTADOS,
            //    ENVIO_MAIL = oPaciente.ENVIO_MAIL,
            //    ENVIO_SMS = oPaciente.ENVIO_SMS,
            //    ENVIO_PROPAGANDA = oPaciente.ENVIO_PROPAGANDA
            //};
            

            return PartialView("InformeEnvioMail", email);
        }



        [Autorization]
        public ActionResult Create(int oidOrigen, int oidExploracion, int oidInforme = 0, string titulo = "", string urlPrevia = "")
        {
            USUARIO usuario = UsuariosRepositorio.Obtener(User.Identity.Name);

            INFORMES oInforme = new INFORMES();
            LISTADIA oExplo = ListaDiaRepositorio.Obtener(oidExploracion);// ExploracionRepositorio.Obtener(oidExploracion);
            PACIENTE oPaciente = PacienteRepositorio.Obtener(oExplo.IOR_PACIENTE);
            PERSONAL oPersonal = db.Personal.Where(p => p.LOGIN == usuario.LOGIN).FirstOrDefault();
            if (oPersonal == null)
            {
                oPersonal = db.Personal.Where(p => p.OID <= 0).FirstOrDefault();
            }

            if (oidOrigen > 0)
            {
                //Primero miramos si el oid origen es un informe anterior del paciente
                INFORMES oInformeDuplicar = InformesRepositorio.Obtener(oidOrigen);
                //Si el origen no es un informe anterior del paciente significa que el origen es una plantilla de informes
                if (string.IsNullOrEmpty(oInformeDuplicar.TEXTOHTML))
                {
                    oInforme.TEXTOHTML = P_InformesRepositorio.ObtenerHtmlDelInforme(oidOrigen);
                    oInforme.TITULO = titulo;
                    if (String.IsNullOrEmpty(titulo))
                    {
                        oInforme.TITULO = oInformeDuplicar.TITULO;
                    }
                }
                else
                {
                    oInforme.TEXTOHTML = oInformeDuplicar.TEXTOHTML;
                    oInforme.TITULO = oInformeDuplicar.TITULO;
                }
            }
            else
            {
                oInforme.TITULO = "Nuevo Informe";
                oInforme.TEXTOHTML = "";

            }
            oInforme.TEXTOHTML = oInforme.TEXTOHTML.Replace("@MI", oPersonal.DESCRIPCION);
            oInforme.TEXTOHTML = oInforme.TEXTOHTML.Replace("@NU", oPersonal.NUMERO);
            oInforme.OWNER = oidExploracion;
            oInforme.FECHA = DateTime.Now;
            oInforme.HORA = DateTime.Now.ToString("HH:mm");
            oInforme.COD_PAC = oPaciente.COD_PAC;
            oInforme.FECHAEXPLORACION = oExplo.FECHA;
            oInforme.IOR_MEDINFORME = oPersonal.OID;
            oInforme.MEDICOINFORMANTE = oPersonal.NOMBRE;
            oInforme.LOGINMEDICOINFORMANTE = oPersonal.LOGIN;
            oInforme.IOR_PAC = oExplo.IOR_PACIENTE;
            oInforme.PACIENTE = oPaciente.PACIENTE1;
            oInforme.PATOLOGICO = " ";
            oInforme.IOR_MEDREVISION = 0;
            oInforme.CID = 1;
            oInforme.USERNAME = usuario.LOGIN;
            oInforme.ALFAS = "F";
            oInforme.VALIDACION = "F";
            oInforme.REVISADO = "F";
            oInforme.DURACION = "00:00:00";
            oInforme.CANAL = "INFO";

            try
            {
                db.Informes.Add(oInforme);
                db.SaveChanges();
                db.Entry(oInforme).GetDatabaseValues();

                TEXTOS oTexto = new TEXTOS();
                oTexto.TEXTO = oInforme.TEXTOHTML;
                oTexto.TEXTO = oTexto.TEXTO.Replace("<title>Untitled document</title>", "");
                oTexto.OWNER = oInforme.OID;

                TextosRepositorio.InsertarOrUpdate(oTexto);
            }
            catch (DbEntityValidationException ex)
            {
                Debug.WriteLine(ex.Message);
            }

            RadioWeb.Utils.LogLopd.Insertar("Nuevo informe de "
                + oInforme.PACIENTE + " - " + oInforme.FECHA + " IP: " + Request.UserHostAddress + " OID: " + oInforme.OID
                , "1");

            return RedirectToAction("Edit", "Informe", new
            {
                OID = oInforme.OID,
                UrlPrevia = urlPrevia
            });



            //}
            //else
            //{

            //    return Content("Acceso no permitido.");
            //}
        }

        [Autorization]
        public ActionResult Edit(int OID, string UrlPrevia = "")
        {
            USUARIO usuario = UsuariosRepositorio.Obtener(User.Identity.Name);
            ViewBag.Title = "Informes RW";
            VMInforme viewModel = new VMInforme(OID);

            viewModel.URLPREVIA = "";
            //LOS PERFILES 10 Y 2 SON DE MEDICOS QUE INFORMAN, POR ESO CARGAMOS SUS PANTILLAS
            if (usuario.PERFIL == 10 || usuario.PERFIL == 2)
            {
                viewModel.PLANTILLASINFORMES = GetPlantillas(usuario);
                if (viewModel.PLANTILLASINFORMES.Count() == 0)
                {
                    if (viewModel.EXPLORACION.EMPRESA.NOMBRE.Contains("DELFOS"))
                    {
                        viewModel.PLANTILLASINFORMES = GetPlantillas("Roca");
                    }
                }
            }
            viewModel.URLPREVIA = UrlPrevia;
            RadioWeb.Utils.LogLopd.Insertar("Ve informe de "
                + viewModel.PACIENTE + " - " + viewModel.FECHA + " - " + viewModel.EXPLORACION.APARATO.DES_FIL
                + " IP: " + Request.UserHostAddress + " OID: " + viewModel.OID
                , "1");

            return View("Editor", viewModel);

        }

        [HttpPost]
        [ValidateAntiForgeryToken, Autorization]
        public ActionResult Edit(VMInforme viewModel)
        {
            USUARIO usuario = UsuariosRepositorio.Obtener(User.Identity.Name);

            GuardarTextoInforme(viewModel);

            if (usuario.PRIVILEGIADO == -1)
            {
                INFORMES oInforme = db.Informes.Single(i => i.OID == viewModel.OID);
                oInforme.IOR_TECNICO = viewModel.IOR_TECNICO;
                oInforme.IOR_MEDREVISION = viewModel.IOR_MEDREVISION;
                oInforme.IOR_MEDINFORME = viewModel.IOR_MEDINFORME;
                //oInforme.FECHA = DateTime.Now;
                //oInforme.HORAMOD = DateTime.Now.ToString("HH:mm");
                oInforme.DURACION = oInforme.DURACION;

                //Actualizamos la ficha de la exploración asignando el medico que hace el informe y el que lo revisa
                ExploracionRepositorio.UpdateCampo("IOR_MEDICO", viewModel.IOR_MEDINFORME.ToString(), oInforme.OWNER.Value, "INTEGER");
                ExploracionRepositorio.UpdateCampo("IOR_MEDREVISION", viewModel.IOR_MEDREVISION.ToString(), oInforme.OWNER.Value, "INTEGER");

                db.Entry(oInforme).State = EntityState.Modified;
                db.SaveChanges();
                viewModel = new VMInforme(viewModel.OID);
                RadioWeb.Utils.LogLopd.Insertar("Modifica datos cabecera informe  "
              + viewModel.PACIENTE + " - " + viewModel.FECHA + " - " + viewModel.EXPLORACION.APARATO.DES_FIL
              + " IP: " + Request.UserHostAddress + " OID: " + viewModel.OID
              , "1");
            }
            if (usuario.ESMEDICO && usuario.PRIVILEGIADO != -1)
            {
                INFORMES oInforme = db.Informes.Single(i => i.OID == viewModel.OID);
                oInforme.IOR_MEDREVISION = viewModel.IOR_MEDREVISION;
                oInforme.IOR_TECNICO = viewModel.IOR_TECNICO;
                 oInforme.MODIF = DateTime.Now;
                oInforme.HORAMOD = DateTime.Now.ToString("HH:mm");
                oInforme.DURACION = oInforme.DURACION;
                oInforme.IOR_MEDINFORME = viewModel.IOR_MEDINFORME;

                //Actualizamos la ficha de la exploración asignando el medico que hace el informe y el que lo revisa
                ExploracionRepositorio.UpdateCampo("IOR_MEDICO", viewModel.IOR_MEDINFORME.ToString(), oInforme.OWNER.Value, "INTEGER");
                ExploracionRepositorio.UpdateCampo("IOR_MEDREVISION", viewModel.IOR_MEDREVISION.ToString(), oInforme.OWNER.Value, "INTEGER");

                db.SaveChanges();
                viewModel = new VMInforme(viewModel.OID);
            }


            //LOS PERFILES 10 Y 2 SON DE MEDICOS QUE INFORMAN, POR ESO CARGAMOS SUS PANTILLAS
            if (usuario.PERFIL == 10 || usuario.PERFIL == 2)
            {
                viewModel.PLANTILLASINFORMES = GetPlantillas(usuario);
            }

            return View("Editor", viewModel);



        }


        private bool GuardarTextoInforme(VMInforme oInforme)
        {

            int idInforme = oInforme.OID;
            TEXTOS oTexto = new TEXTOS();
            oTexto.TEXTO = oInforme.TEXTOHTML;
            oTexto.TEXTO = oTexto.TEXTO.Replace("<title>Untitled document</title>", "");
            oTexto.TEXTO = oTexto.TEXTO.Replace("st1", "233");
            oTexto.TEXTO = oTexto.TEXTO.Replace("st2", "433");
            oTexto.OWNER = oInforme.OID;
            TextosRepositorio.InsertarOrUpdate(oTexto);
            return true;
        }


        public string Previsualizar(int oid)
        {
            return InformesRepositorio.ObtenerHtmlDelInforme(oid);
        }

        [HttpDelete]
        [Autorization]
        public ActionResult Delete(int oid)
        {
            INFORMES informe = db.Informes.Single(i => i.OID == oid);
            db.Entry(informe).State = EntityState.Modified;
            informe.BORRADO = "T";
            db.SaveChanges();
            return new HttpStatusCodeResult(HttpStatusCode.OK);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Validar(int oid)
        {
            USUARIO usuario = (USUARIO)Session["Usuario"];
            WebConfigRepositorio oConfig = new WebConfigRepositorio();
            INFORMES oInforme = db.Informes.Single(i => i.OID == oid);
            PERSONAL oValidadorInforme = null;
            PERSONAL oCreadorInforme = null;

            bool validaMedicoRevisor = oConfig.ObtenerValor("MEDICOQUEVALIDA") == "REVISOR";
            bool moduloExportacionBadalona = oConfig.ObtenerValor("MODULOEXPORTACIONBADALONA") == "T";
            bool moduloGenerarVersionesPDFs = oConfig.ObtenerValor("MODULOGENERAVERSIONES") == "T";

            //En funcion del parametro MedicoValidador de la confiuración global
            //Generamos una instancia del tipo personal con los datos del validador del informe
            //que puede ser o bien el creador del informe (Manchon) o bien el medico revisor (Corachan)
            if (validaMedicoRevisor)
            {
                oValidadorInforme = db.Personal.Where(p => p.OID == oInforme.IOR_MEDREVISION).SingleOrDefault();
                oCreadorInforme = db.Personal.Where(p => p.OID == oInforme.IOR_MEDINFORME).SingleOrDefault();
            }
            else
            {
                oValidadorInforme = db.Personal.Where(p => p.OID == oInforme.IOR_MEDINFORME).FirstOrDefault();
                if (oValidadorInforme == null)
                {
                    oValidadorInforme = db.Personal.Where(p => p.OID <= 0).FirstOrDefault();
                }
                oCreadorInforme = oValidadorInforme;
            }


            try
            {
                oInforme.USERNAME = usuario.LOGIN;
                //Si el que valida el informe es el mismo que lo crea
                if ((usuario.LOGIN == oValidadorInforme.LOGIN) || (usuario.PRIVILEGIADO == -1)
                    || ((usuario.LOGIN == "TELEPACS") && (usuario.PERFIL == 7 || usuario.PERFIL == 9 ||
                    usuario.PERFIL == 10 || usuario.PERFIL == 76 || usuario.PERFIL == 129)))
                {

                    //SI ES EL MISMO MEDICO O ES PRIVILEGIADO O ES TELEPACS CON USUARIO TECNICO, MEDICO, SUPERVISION...
                    if (oInforme.VALIDACION == "F")
                    {
                        //ANTES DE VALIDAR MIRAMOS QUE NO EXISTA YA UNO VALIDADO. SI YA HAY ALGUNO PONEMOS EL INFORME COMO ANULADO
                        int existeUnoValidado = db.Informes
                                    .Count(i => i.OWNER == oInforme.OWNER
                                            && i.VALIDACION == "T");

                        if (existeUnoValidado > 0)
                        {
                            oInforme.VALIDACION = "A";
                            db.SaveChanges();
                            //DEVOLVEMOS UNA X PARA PODER NOTIFICAR AL USUARIO EL MOTIVO POR EL QUE ANULAMOS
                            //EXISTE OTRO VALIDADO
                            return Json("X", JsonRequestBehavior.AllowGet);
                        }
                        else
                        {
                            oInforme.VALIDACION = "T";
                            oInforme.REVISADO = "T";
                            if (validaMedicoRevisor)
                            {
                                oInforme.IOR_MEDREVISION = oValidadorInforme.OID;
                            }

                            oInforme.FECHAREVISION = DateTime.Now;
                            oInforme.HORAREV = DateTime.Now.ToString("HH:mm");
                            EXPLORACION oExploracion = ExploracionRepositorio.Obtener(oInforme.OWNER.Value);
                            ExploracionRepositorio.UpdateCampo("INFORMADA", "T", oInforme.OWNER.Value);
                            ExploracionRepositorio.UpdateCampo("IOR_MEDICO", oInforme.IOR_MEDINFORME.ToString(), oInforme.OWNER.Value, "INTEGER");
                            ExploracionRepositorio.UpdateCampo("IOR_MEDREVISION", oInforme.IOR_MEDREVISION.ToString(), oInforme.OWNER.Value, "INTEGER");
                            db.SaveChanges();
                            List<EXPLORACION> oListHijas = ExploracionRepositorio.ObtenerHijas(oInforme.OWNER.Value);
                            foreach (var hija in oListHijas)
                            {
                                ExploracionRepositorio.UpdateCampo("INFORMADA", "T", hija.OID);
                                ExploracionRepositorio.UpdateCampo("IOR_MEDICO", oInforme.IOR_MEDINFORME.ToString(), hija.OID, "INTEGER");
                                ExploracionRepositorio.UpdateCampo("IOR_MEDREVISION", oInforme.IOR_MEDREVISION.ToString(), hija.OID, "INTEGER");
                            }
                         
                            if (moduloGenerarVersionesPDFs)
                            {
                               
                                try
                                {
                                    InformesRepositorio.GenerarVersionInforme(oInforme.OID);
                                }
                                catch (Exception ex)
                                {

                                    LogException.LogMessageToFile(ex.Message);
                                }
                            }
                            //Badalona y MEDIFIATC
                            if ( oExploracion.IOR_ENTIDADPAGADORA == 11042
                                && !String.IsNullOrEmpty(oExploracion.ARCHIVOBADALONA) && moduloExportacionBadalona)
                            {
                                try
                                {
                                    InformesRepositorio.GenerarFicherosBadalona(oInforme.OID);
                                }
                                catch (Exception)
                                {

                                    throw;
                                }

                            }
                            if ((oExploracion.IOR_ENTIDADPAGADORA == 139)
                                  && !String.IsNullOrEmpty(oExploracion.ARCHIVOBADALONA) && moduloExportacionBadalona)
                            {
                                try
                                {
                                    InformesRepositorio.GenerarFicherosFiatc(oInforme.OID);
                                }
                                catch (Exception)
                                {

                                    throw;
                                }
                            }
                            if (Session["FiltrosBusquedaAvanzada"] != null)
                            {
                                VWBusquedaAvanzada oFiltros = new VWBusquedaAvanzada();
                                oFiltros = (VWBusquedaAvanzada)Session["FiltrosBusquedaAvanzada"];
                                var exploracionesBuscadas = oFiltros.Resultados;
                                exploracionesBuscadas = exploracionesBuscadas.Where(a => a.OID != oInforme.OWNER).ToList();
                                oFiltros.Resultados = exploracionesBuscadas;
                                Session["FiltrosBusquedaAvanzada"] = oFiltros;
                            }
                        }
                    }
                    else if (oInforme.VALIDACION == "T")
                    {
                        oInforme.VALIDACION = "A";
                        oInforme.REVISADO = "F";
                        oInforme.FECHAREVISION = null;
                        oInforme.HORAREV = null;
                        db.SaveChanges();
                        ExploracionRepositorio.UpdateCampo("INFORMADA", "F", oInforme.OWNER.Value);
                        ExploracionRepositorio.UpdateCampo("IOR_MEDICO", "-1", oInforme.OWNER.Value, "INTEGER");
                        ExploracionRepositorio.UpdateCampo("IOR_MEDREVISION", "-1", oInforme.OWNER.Value, "INTEGER");
                    }
                    else if (oInforme.VALIDACION == "A")
                    {
                        oInforme.VALIDACION = "F";
                        oInforme.REVISADO = "F";
                        oInforme.FECHAREVISION = null;
                        oInforme.HORAREV = null;
                        db.SaveChanges();
                        ExploracionRepositorio.UpdateCampo("INFORMADA", "F", oInforme.OWNER.Value);
                        ExploracionRepositorio.UpdateCampo("IOR_MEDICO", "-1", oInforme.OWNER.Value, "INTEGER");
                        ExploracionRepositorio.UpdateCampo("IOR_MEDREVISION", "-1", oInforme.OWNER.Value, "INTEGER");

                    }
                }
                else if (validaMedicoRevisor && usuario.LOGIN == oCreadorInforme.LOGIN)
                {
                    //este # le indica al cliente (navegador) que enseñe un mensaje indicando que
                    // el medico que valida es es de la revisión
                    return Json("#", JsonRequestBehavior.AllowGet);

                }
                else if (usuario.PERFIL == 7 || usuario.PERFIL == 9 || usuario.PERFIL == 10 || usuario.PERFIL == 76 || usuario.PERFIL == 129)
                {
                    if (oInforme.VALIDACION == "F")
                    {
                        oInforme.VALIDACION = "A";
                        db.SaveChanges();
                        ExploracionRepositorio.UpdateCampo("INFORMADA", "F", oInforme.OWNER.Value);
                    }
                    else if (oInforme.VALIDACION == "A")
                    {
                        oInforme.VALIDACION = "F";
                        db.SaveChanges();
                        ExploracionRepositorio.UpdateCampo("INFORMADA", "F", oInforme.OWNER.Value);
                    }
                    else if (oInforme.VALIDACION == "T")
                    {
                        oInforme.VALIDACION = "A";
                        db.SaveChanges();
                        ExploracionRepositorio.UpdateCampo("INFORMADA", "F", oInforme.OWNER.Value);
                        ExploracionRepositorio.UpdateCampo("IOR_MEDICO", "-1", oInforme.OWNER.Value, "INTEGER");
                        ExploracionRepositorio.UpdateCampo("IOR_MEDREVISION", "-1", oInforme.OWNER.Value, "INTEGER");

                    }

                }

                return Json(oInforme.VALIDACION, JsonRequestBehavior.AllowGet);

            }
            catch (Exception)
            {

                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
        }

        [HttpPost]
        public ActionResult Valorar(VMInforme viewModel)
        {
            INFORMES oInforme = db.Informes.Single(i => i.OID == viewModel.OID);
            VALORACION oValoracion = null;
            oValoracion = db.Valoracion.Where(i => i.OWNER == viewModel.OID).FirstOrDefault();

            if (oValoracion == null)
            {
                oValoracion = new VALORACION
                {
                    OWNER = oInforme.OID,
                    V_IMAGEN = viewModel.VALORACIONIMAGENES.ToString(),
                    V_RADIOLOGO = viewModel.VALORACIONINFORME.ToString(),
                    FECHA = DateTime.Now,
                    FECHA_VALORACION = DateTime.Now,
                    IOR_PAC = oInforme.IOR_PAC,
                    IOR_MEDINFORME = oInforme.IOR_MEDINFORME,
                    IOR_TECNICO = oInforme.IOR_TECNICO,
                    IOR_TECNICA = oInforme.IOR_TECNICA,
                    ERROR = "F",
                    TEXTO_RADIOLOGO = viewModel.VALORACIONINFORMETEXTO,
                    TEXTO_IMAGEN = viewModel.VALORACIONIMAGENESTEXTO
                };
                db.Valoracion.Add(oValoracion);
            }
            else
            {
                USUARIO usuario = (USUARIO)Session["Usuario"];
                oValoracion.V_IMAGEN = viewModel.VALORACIONIMAGENES.ToString();
                oValoracion.V_RADIOLOGO = viewModel.VALORACIONINFORME.ToString();
                oValoracion.IOR_MEDREVISION = usuario.PERSONAL.OID;
                oValoracion.FECHA_VALORACION = DateTime.Now;
                oValoracion.TEXTO_RADIOLOGO = viewModel.VALORACIONINFORMETEXTO;
                oValoracion.TEXTO_IMAGEN = viewModel.VALORACIONIMAGENESTEXTO;
                db.Entry(oValoracion).State = EntityState.Modified;
            }


            db.SaveChanges();

            return RedirectToAction("Edit", "Informe", new
            {
                OID = oInforme.OID
            });
        }

        public ActionResult QReport(int oid, bool esOidExploracion = false)
        {
            WebConfigRepositorio oConfig = new WebConfigRepositorio();
            string RutaQReport = oConfig.ObtenerValor("QReport");
            INFORMES oInforme = null;
            EXPLORACION oExploracion = null;
            //al solicitar que podamos enviar a qreport desde la exploracion
            if (esOidExploracion)
            {
                int oidExploracion = 0;
                oExploracion = ExploracionRepositorio.Obtener(oid);
                if (oExploracion.IOR_MASTER > 0)
                {
                    oidExploracion = oExploracion.IOR_MASTER.Value;
                }
                else
                {
                    oidExploracion = oExploracion.OID;

                }

                oInforme = db.Informes.Single(i => i.OWNER == oidExploracion && i.VALIDACION == "T");
            }
            //si recibimos directamente el oid del informe
            else
            {
                oInforme = db.Informes.Single(i => i.OID == oid);
                oExploracion = ExploracionRepositorio.Obtener(oInforme.OWNER.Value);
            }


            string RutaMacrosCabecera = oConfig.ObtenerValor("RutaMacroCabeceraInformes");
            try
            {

                string RutaDOCS = oConfig.ObtenerValor("RUTADOCS");
                string rutaPdfGenerado = InformesRepositorio.GenerarPDF(RutaDOCS, true, oInforme.OID, RutaMacrosCabecera,"");

                QReportController qr = new QReportController();


                //Recuperamos el token, si no lo tenemos o está caducado. Lo peticionamos.
                String token = qr.getToken();
                String patientId = oExploracion.IOR_PACIENTE.ToString();
                String accNumber = oExploracion.OID.ToString();
                DateTime dtVisita = (DateTime)oExploracion.FECHA;
                String sDtVisita = dtVisita.ToString("yyyy-MM-dd");

                //Recuperamos el UID de la petición
                String studyInstanceUID = qr.getStudyUID(patientId, accNumber, sDtVisita);

                if (studyInstanceUID == "") {
                    return Json(new { success = false, message = "No enviado. El paciente no tiene estudio asociado" }, JsonRequestBehavior.AllowGet);
                }

                //Enviamos el informe a Qreport
                Boolean success = qr.sendReport(patientId, accNumber, studyInstanceUID, rutaPdfGenerado);
                if (!success) {
                    return Json(new { success = false, message = "No enviado. Se ha producido un error." }, JsonRequestBehavior.AllowGet);
                }

                //Si ha ido bien actualizamos el valor del campo en la base de datos
                ExploracionRepositorio.updateQreportEnviado(oid, "T");

            }
            catch (Exception)
            {
                throw;
                return Json(new { success = false, message = "No enviado. Se ha producido un error." }, JsonRequestBehavior.AllowGet);
            }

            return Json(new { success = true, message = "" }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult VisorQReport(int oid, int tipo)
        {
            String url = "";

            //Obtenemos la exploracion
            int oidExploracion = 0;
            EXPLORACION oExploracion = null;
            oExploracion = ExploracionRepositorio.Obtener(oid);
            if (oExploracion.IOR_MASTER > 0)
            {
                oidExploracion = oExploracion.IOR_MASTER.Value;
            }
            else
            {
                oidExploracion = oExploracion.OID;
            }

            try
            {

                QReportController qr = new QReportController();

                //Recuperamos el token, si no lo tenemos o está caducado. Lo peticionamos.
                String token = qr.getToken();
                String patientId = oExploracion.IOR_PACIENTE.ToString();
                String accNumber = oExploracion.OID.ToString();
                DateTime dtVisita = (DateTime)oExploracion.FECHA;
                String sDtVisita = dtVisita.ToString("yyyy-MM-dd");

                //Recuperamos el UID de la petición
                String studyInstanceUID = qr.getStudyUID(patientId, accNumber, sDtVisita);

                if (studyInstanceUID == "")
                {
                    return Json(new { success = false, message = "El paciente no tiene estudio asociado" }, JsonRequestBehavior.AllowGet);
                }

                //Obtenemos la url del visor
                switch (tipo)
                {
                    case 1:
                        url = qr.visorUrlWeb.Replace("&amp;", "&");//qr.getVisor(tipo);

                        break;
                    case 2:
                        url = qr.visorUrlDiagnostico.Replace("&amp;", "&");//qr.getVisor(tipo);

                        break;
                    case 3:
                        url = qr.visorUrlZip.Replace("&amp;", "&");//qr.getVisor(tipo);

                        break;
                    default:
                        break;
                }
             //   url = qr.xmlStudyURL.Replace("&amp;","&");//qr.getVisor(tipo);

                if (url != "")
                {
                    return Json(new { success = true, message = url }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { success = false, message = "No se ha podido abrir el visor" }, JsonRequestBehavior.AllowGet);
                }

            }
            catch (Exception)
            {
                throw;
                return Json(new { success = false, message = "Se ha producido un error." }, JsonRequestBehavior.AllowGet);
            }

            return Json(new { success = true, message = "" }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult comprobarEstadoInformeQReport(int oid)
        {
            //Obtenemos la exploracion
            int oidExploracion = 0;
            EXPLORACION oExploracion = null;
            oExploracion = ExploracionRepositorio.Obtener(oid);

            //Comprobamos si la exploracion se ha enviado al QREPORT
            string qreport_enviado = oExploracion.QREPORT_ENVIADO;

            if (qreport_enviado == null || qreport_enviado == "F")
            {
                return Json(new { success = false, message = "El informe no se ha subido al QReport" }, JsonRequestBehavior.AllowGet);
            }

            return Json(new { success = true, message = "" }, JsonRequestBehavior.AllowGet);
        }


        public ActionResult XeroxDelfos(int oid)
        {
            USUARIO usuario = (USUARIO)Session["Usuario"];
            WebConfigRepositorio oConfig = new WebConfigRepositorio();
            string RutaDOCS = oConfig.ObtenerValor("RUTADOCS");
            string RutaMacrosCabecera = oConfig.ObtenerValor("RutaMacroCabeceraInformes");

            INFORMES oInforme = db.Informes.Single(i => i.OID == oid);
            EXPLORACION oExploracion = ExploracionRepositorio.Obtener(oInforme.OWNER.Value);
            PERSONAL oCreadorInforme = db.Personal.Where(p => p.OID == oInforme.IOR_MEDINFORME).First();
            PACIENTE oPaciente = PacienteRepositorio.Obtener(oInforme.IOR_PAC.Value);
            string RutaXerox = oConfig.ObtenerValor("RutaXerox");
            try
            {
                string rutaPdfGenerado = InformesRepositorio.GenerarPDF(RutaDOCS, true, oid, RutaMacrosCabecera);
                System.IO.File.Copy(rutaPdfGenerado, RutaXerox + @"\" + oPaciente.PACIENTE1 + "^" + oExploracion.OID + ".pdf");
                try
                {
                    System.IO.File.Delete(rutaPdfGenerado);
                }
                catch (Exception)
                {


                }
            }
            catch (Exception ex)
            {

                return new HttpStatusCodeResult(HttpStatusCode.InternalServerError, ex.Message);
            }

            return new HttpStatusCodeResult(HttpStatusCode.OK, "Enviado a Xerox");

        }

        public ActionResult Xerox(int oid)
        {
            USUARIO usuario = (USUARIO)Session["Usuario"];
            WebConfigRepositorio oConfig = new WebConfigRepositorio();


            INFORMES oInforme = db.Informes.Single(i => i.OID == oid);
            EXPLORACION oExploracion = ExploracionRepositorio.Obtener(oInforme.OWNER.Value);
            PERSONAL oCreadorInforme = db.Personal.Where(p => p.OID == oInforme.IOR_MEDINFORME).First();
            PACIENTE oPaciente = PacienteRepositorio.Obtener(oInforme.IOR_PAC.Value);
            string RutaXerox = oConfig.ObtenerValor("RutaXerox");
            string RutaXerox2 = oConfig.ObtenerValor("RutaXerox2");
            string RutaXerox3 = oConfig.ObtenerValor("RutaXerox3");
            string RutaXerox4 = oConfig.ObtenerValor("RutaXerox4");
            string RutaXerox5 = oConfig.ObtenerValor("RutaXerox5");
            string RutaXerox6 = oConfig.ObtenerValor("RutaXerox6");
            string RutaXerox7 = oConfig.ObtenerValor("RutaXerox7");
            string RutaClinicum = oConfig.ObtenerValor("RutaClinicum");
            string RutaLesMoreres = oConfig.ObtenerValor("RutaLesMoreres");
            string RutaEBA = oConfig.ObtenerValor("RutaEBA");
            string RutaDifusio = oConfig.ObtenerValor("RutaDifusio");
            string RutaManresa = oConfig.ObtenerValor("RutaManresa");
            try
            {
                oInforme.USERNAME = usuario.LOGIN;
                //Estas son todas las condiciones para poder enviar a la Xerox
                if ((usuario.LOGIN == oCreadorInforme.LOGIN) || (usuario.PRIVILEGIADO == -1)
                    || ((usuario.LOGIN == "TELEPACS") && (usuario.PERFIL == 7 || usuario.PERFIL == 9 || usuario.PERFIL == 10 || usuario.PERFIL == 76 || usuario.PERFIL == 129)))
                {
                    //clinicum
                    if (oExploracion.DAPARATO.CID == 9)
                    {
                        string nombreArchivo = oExploracion.IDCITAONLINE;
                        if (nombreArchivo.Contains("-"))
                        {
                            nombreArchivo = nombreArchivo.Substring(0, nombreArchivo.IndexOf("-") - 1);
                        }

                        //GenerarPDFXerox(RutaClinicum + @"\" + nombreArchivo + ".pdf", "CabeceraInformes_CLI.html", oInforme.OID);
                        string rutaPdfGenerado = InformesRepositorio.GenerarPDF(RutaClinicum, true, oid, Server.MapPath("~/Reports/pdf/templates/CabeceraInformes_CLI.html"));
                        if (System.IO.File.Exists(RutaClinicum + @"\" + nombreArchivo + ".pdf"))
                        {
                            System.IO.File.Delete(RutaClinicum + @"\" + nombreArchivo + ".pdf");
                        }
                        System.IO.File.Move(rutaPdfGenerado, RutaClinicum + @"\" + nombreArchivo + ".pdf");
                    }
                    //les moreres
                    else if (oExploracion.DAPARATO.CID == 7 ||
                        oExploracion.IOR_CENTROEXTERNO == 12810117 ||
                        oExploracion.IOR_CENTROEXTERNO == 12810149)
                    {
                        //Si la exploracion viene de estos dos centros externos y el nhcap es nulo no hacemos nada
                        if (!(oExploracion.IOR_CENTROEXTERNO == 12810117 || oExploracion.IOR_CENTROEXTERNO == 12810149)
                            && string.IsNullOrEmpty(oExploracion.NHCAP))
                        {
                            string fechaNacimiento = (String.IsNullOrEmpty(oPaciente.FECHAN.Value.ToString()) ? "AAAAMMDD" : oPaciente.FECHAN.Value.ToString("yyyyMMdd"));
                            string nombreMoreres = RutaLesMoreres + @"\" + oExploracion.PACIENTE.PACIENTE1.Replace(", ", "#").Replace(",", "#").Replace(",", "").Replace("  ", "").QuitAccents()
                                + (String.IsNullOrEmpty(oExploracion.NHCAP) ? "_" + fechaNacimiento + "_" + oExploracion.FECHA.Value.ToString("yyyyMMdd") : "_" + oExploracion.NHCAP + "_" + oExploracion.FECHA.Value.ToString("yyyyMMdd"));

                            //GenerarPDFXerox(nombreMoreres + ".pdf", "CabeceraInformesLogo_CDPI.html", oInforme.OID);
                            string rutaPdfGenerado = InformesRepositorio.GenerarPDF(RutaLesMoreres, true, oid, oConfig.ObtenerValor("RutaMacroCabeceraInformes"));
                            if (System.IO.File.Exists(nombreMoreres + ".pdf"))
                            {
                                System.IO.File.Delete(nombreMoreres + ".pdf");
                            }
                            System.IO.File.Move(rutaPdfGenerado, nombreMoreres + ".pdf");
                        }

                    }
                    else if (oExploracion.IOR_ENTIDADPAGADORA == 16726546)//EBA
                    {
                        string grupoAparatos = GAparatoRepositorio.Obtener(oExploracion.IOR_GRUPO.Value).COD_GRUP;
                        switch (grupoAparatos)
                        {
                            case "RM":
                                grupoAparatos = "RMN";
                                break;
                            case "TAC":
                                grupoAparatos = "TC";
                                break;
                            case "DEN":
                                grupoAparatos = "DMO";
                                break;
                            case "MAM":
                                grupoAparatos = "Mx";
                                break;
                            default:
                                break;
                        }

                        string nombreEBA = oPaciente.CIP;
                        if (String.IsNullOrEmpty(nombreEBA))
                        {
                            nombreEBA = oPaciente.PACIENTE1;
                            nombreEBA = nombreEBA.Replace(',', ' ').Replace('.', ' ').Replace("  ", " ").QuitAccents();
                        }
                        nombreEBA = grupoAparatos + "_" + nombreEBA + "_" + oExploracion.FECHA.Value.ToString("yyyyMMdd");
                        // GenerarPDFXerox(RutaEBA + @"\" + nombreEBA + ".pdf", "CabeceraInformesLogo_CDPI.html", oInforme.OID);
                        string rutaPdfGenerado = InformesRepositorio.GenerarPDF(RutaEBA, true, oid, Server.MapPath("~/Reports/pdf/templates/CabeceraInformesLogo_CDPINew.html"));
                        System.IO.File.Move(rutaPdfGenerado, nombreEBA + ".pdf");
                    }
                    else
                    {
                        int xerox = DaparatoRepositorio.Obtener(oExploracion.IOR_APARATO.Value).XEROX.Value;
                        string nombreFichero = "";
                        string rutaPdfGenerado = "";
                        switch (xerox)
                        {
                            case 1:
                                //Deberiamos enviar por SistemX
                                nombreFichero = RutaXerox + @"\" + oPaciente.PACIENTE1.Replace(",", "").Replace(" ", " ").QuitAccents() + "^" + oExploracion.OID + ".pdf";

                                rutaPdfGenerado = InformesRepositorio.GenerarPDF(RutaXerox, true, oid, Server.MapPath("~/Reports/pdf/templates/CabeceraInformesLogo_CDPINew.html"));
                                if (System.IO.File.Exists(nombreFichero))
                                {
                                    System.IO.File.Delete(nombreFichero);
                                }
                                System.IO.File.Move(rutaPdfGenerado, nombreFichero);
                                if (oPaciente.OTROS3 == "T")
                                {
                                    int i = 0;
                                    string difusioString = RutaDifusio + @"\" + oPaciente.PACIENTE1.Replace(",", "").Replace(" ", "^").QuitAccents() + "_"
                                        + oPaciente.OID + "_" + oExploracion.OID + "_3_" + i.ToString().PadLeft(2, '0');

                                    while (System.IO.File.Exists(difusioString + i.ToString().PadLeft(2, '0') + ".pdf"))
                                    {
                                        i = i + 1;
                                        difusioString = difusioString + i.ToString().PadLeft(2, '0');

                                    }
                                    System.IO.File.Copy(nombreFichero, difusioString + ".pdf", true);

                                }
                                break;
                            case 2:
                                //Deberiamos enviar por SistemX
                                nombreFichero = RutaXerox2 + @"\" + oExploracion.OID + ".pdf";
                                rutaPdfGenerado = InformesRepositorio.GenerarPDF(RutaXerox2, true, oid, Server.MapPath("~/Reports/pdf/templates/CabeceraInformesLogo_CDPINew.html"));
                                if (System.IO.File.Exists(nombreFichero))
                                {
                                    System.IO.File.Delete(nombreFichero);
                                }
                                System.IO.File.Move(rutaPdfGenerado, nombreFichero);


                                break;
                            case 3:
                                //Deberiamos enviar por SistemX
                                nombreFichero = RutaXerox3 + @"\" + oExploracion.OID + ".pdf";
                                rutaPdfGenerado = InformesRepositorio.GenerarPDF(RutaXerox3, true, oid, Server.MapPath("~/Reports/pdf/templates/CabeceraInformesLogo_CDPINew.html"));
                                if (System.IO.File.Exists(nombreFichero))
                                {
                                    System.IO.File.Delete(nombreFichero);
                                }
                                System.IO.File.Move(rutaPdfGenerado, nombreFichero);
                                if (oPaciente.OTROS3 == "T")
                                {
                                    int i = 0;
                                    string difusioString = RutaDifusio + @"\" + oPaciente.PACIENTE1.Replace(",", "").Replace(" ", "^").QuitAccents() + "_"
                                        + oPaciente.OID + "_" + oExploracion.OID + "_3_" + i.ToString().PadLeft(2, '0');

                                    while (System.IO.File.Exists(difusioString + i.ToString().PadLeft(2, '0') + ".pdf"))
                                    {
                                        i = i + 1;
                                        difusioString = difusioString + i.ToString().PadLeft(2, '0');

                                    }

                                    System.IO.File.Copy(nombreFichero, difusioString + ".pdf", true);
                                }
                                break;
                            case 4:
                                //Deberiamos enviar por SistemX
                                nombreFichero = @"\\pc20\informes\" + oExploracion.OID + ".pdf";
                                rutaPdfGenerado = InformesRepositorio.GenerarPDF(@"\\pc20\informes", true, oid, Server.MapPath("~/Reports/pdf/templates/CabeceraInformesLogo_CDPINew.html"));
                                if (System.IO.File.Exists(nombreFichero))
                                {
                                    System.IO.File.Delete(nombreFichero);
                                }
                                System.IO.File.Move(rutaPdfGenerado, nombreFichero);
                                // GenerarPDFXerox(nombreFichero, "CabeceraInformesLogo_CDPI.html", oInforme.OID);
                                break;
                            case 5:
                                //Deberiamos enviar por SistemX
                                nombreFichero = RutaXerox5 + @"\" + oExploracion.OID + ".pdf";
                                rutaPdfGenerado = InformesRepositorio.GenerarPDF(RutaXerox5, true, oid, Server.MapPath("~/Reports/pdf/templates/CabeceraInformesLogo_CDPINew.html"));
                                if (System.IO.File.Exists(nombreFichero))
                                {
                                    System.IO.File.Delete(nombreFichero);
                                }
                                System.IO.File.Move(rutaPdfGenerado, nombreFichero);
                                if (oPaciente.OTROS3 == "T")
                                {
                                    int i = 0;
                                    string difusioString = RutaDifusio + @"\" + oPaciente.PACIENTE1.Replace(",", "").Replace(" ", "^").QuitAccents() + "_"
                                        + oPaciente.OID + "_" + oExploracion.OID + "_63_" + i.ToString().PadLeft(2, '0');

                                    while (System.IO.File.Exists(difusioString + i.ToString().PadLeft(2, '0') + ".pdf"))
                                    {
                                        i = i + 1;
                                        difusioString = difusioString + i.ToString().PadLeft(2, '0');

                                    }
                                    System.IO.File.Copy(nombreFichero, difusioString + ".pdf", true);
                                }
                                break;
                            case 6:
                                //Deberiamos enviar por SistemX    
                                nombreFichero = RutaXerox6 + @"\" + oExploracion.OID + ".pdf";
                                if (System.IO.File.Exists(nombreFichero))
                                {
                                    System.IO.File.Delete(nombreFichero);
                                }
                                rutaPdfGenerado = InformesRepositorio.GenerarPDF(RutaXerox6, true, oid, Server.MapPath("~/Reports/pdf/templates/CabeceraInformesLogo_CDPINew.html"));
                                System.IO.File.Move(rutaPdfGenerado, nombreFichero);
                                //GenerarPDFXerox(nombreFichero, "CabeceraInformesLogo_CDPI.html", oInforme.OID);
                                string idManresa = (oExploracion.IDCITAONLINE.StartsWith("MAN_") ? oExploracion.IDCITAONLINE : "MAN_" + oExploracion.IDCITAONLINE);
                                System.IO.File.Copy(nombreFichero, RutaManresa + @"\" + idManresa + "_" + oExploracion.OID + ".pdf", true);
                                break;
                            case 7:
                                //Deberiamos enviar por SistemX    
                                nombreFichero = RutaXerox7 + @"\" + oExploracion.OID + "^" + oPaciente.PACIENTE1.Replace(",", "").Replace(" ", " ").QuitAccents() + ".pdf";
                                if (System.IO.File.Exists(nombreFichero))
                                {
                                    System.IO.File.Delete(nombreFichero);
                                }
                                rutaPdfGenerado = InformesRepositorio.GenerarPDF(RutaXerox7, true, oid, Server.MapPath("~/Reports/pdf/templates/CabeceraInformesLogo_CDPINew.html"));
                                System.IO.File.Move(rutaPdfGenerado, nombreFichero);
                                //   GenerarPDFXerox(nombreFichero, "CabeceraInformesLogo_CDPI.html", oInforme.OID);

                                break;
                            default:
                                break;
                        }

                    }
                }
            }
            catch (Exception ex)
            {

                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }


            return Json(oInforme.VALIDACION, JsonRequestBehavior.AllowGet);
        }


        [HttpPost]
        [ValidateInput(false)]
        public ActionResult EnviarInformeMail(EMAIL email)
        {

            if (ModelState.IsValid)
            {
                try
                {

                    List<string> oMail = new List<string>();
                   
                    EXPLORACION oExploracion = ExploracionRepositorio.Obtener((int)email.CID);
                    if (!oExploracion.EMPRESA.NOMBRE.Contains("DELFOS"))
                    {
                        CENTROS oCentro = CentrosRepositorio.Obtener(oExploracion.OWNER.Value);
                        if (!oCentro.NOMBRE.ToUpper().Contains("DELFOS"))
                        {
                            switch (oExploracion.DAPARATO.CID)
                            {
                                case 1:
                                    oMail.Add("noemi.gayete@affidea.com");
                                    break;
                                case 4:
                                    oMail.Add("zaidamonge@dr-manchon.com");
                                    break;
                                default:
                                    oMail.Add("noemi.gayete@affidea.com");
                                    break;
                            }
                        }

                    }

                //email destino del paciente
                    oMail.Add(email.USERNAME);

                    TEXTOS oTextoExplo = new TEXTOS();
                    oTextoExplo.TEXTO = oExploracion.TEXTO;
                    USUARIO usuario = (USUARIO)Session["Usuario"];
                    string textoInformandoEnvio = " Enviat Informe (" + usuario.LOGIN + ") ";


                    if (email.ADJUNTO2 == "True" || email.ADJUNTO2 == "T" || email.ADJUNTO2 == "on")
                    {
                        textoInformandoEnvio = textoInformandoEnvio + " + sms ";
                    }

                    textoInformandoEnvio = textoInformandoEnvio + "(" + DateTime.Now.ToShortDateString() + "-" + DateTime.Now.ToString("HH:mm") + ")";
                    oTextoExplo.TEXTO = textoInformandoEnvio + oTextoExplo.TEXTO;
                    oTextoExplo.OWNER = oExploracion.OID;
                    TextosRepositorio.InsertarOrUpdate(oTextoExplo);

                    WebConfigRepositorio oConfig = new WebConfigRepositorio();
                    string smtpServer = oConfig.ObtenerValor("smtpServer");
                    string smtpUser = oConfig.ObtenerValor("smtpUser");
                    string smtpPassword = oConfig.ObtenerValor("smtpPassword");
                    string smtpPort = oConfig.ObtenerValor("smtpPort");
                    string emailFrom = oConfig.ObtenerValor("smtpMail");
                    Boolean usarSsl = oConfig.ObtenerValor("smtpusessl").ToUpper() == "TRUE" ? true : false;
                    if (!oExploracion.EMPRESA.NOMBRE.Contains("DELFOS"))
                    {

                        Utils.Varios.EnviarMail(email.TEXTO, oMail, smtpServer, int.Parse(smtpPort), smtpUser, smtpPassword, emailFrom, usarSsl, email.ASUNTO, email.ADJUNTO1);

                        USUARIO usuarioAutenticated = (USUARIO)System.Web.HttpContext.Current.Session["Usuario"];
                        LOGUSUARIOS oLogTecnico = new LOGUSUARIOS
                        {
                            OWNER = oExploracion.OID,
                            FECHA = DateTime.Now.ToString("dd/MM/yyyy HH:mm"),
                            TEXTO = "Informe Enviado Mail",
                            USUARIO = usuarioAutenticated.NOME,
                            DATA = DateTime.Now.ToString("dd/MM/yyyy"),
                            COD_FIL = oExploracion.DAPARATO.COD_FIL,
                            MUTUA = oExploracion.ENTIDAD_PAGADORA.CODMUT
                        };
                        LogUsuariosRepositorio.Insertar(oLogTecnico);

                    }
                    //Utils.Varios.EnviarMail(email.TEXTO, oMail, smtpServer, int.Parse(smtpPort),
                    //   smtpUser, smtpPassword, emailFrom, email.ASUNTO, email.ADJUNTO1,usarSsl);
                    return new HttpStatusCodeResult(HttpStatusCode.OK);
                }
                catch (Exception)
                {

                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
            }
            else
            {

                return new HttpStatusCodeResult(HttpStatusCode.NotAcceptable);
            }





        }


        
        public FileContentResult Imprimir(int oid, string password = "")
        {
            WebConfigRepositorio oConfig = new WebConfigRepositorio();
            string RutaDOCS = oConfig.ObtenerValor("RUTADOCS");
            string nombreDocumento = "Documento.pdf";
            string RutaMacrosCabecera = oConfig.ObtenerValor("RutaMacroCabeceraInformes");

            string file = InformesRepositorio.GenerarPDF(RutaDOCS,
                true, oid,
               RutaMacrosCabecera,
                password);

            if (System.IO.File.Exists(file))
            {
                nombreDocumento = new System.IO.FileInfo(file).Name;
            }
            var fileBytes = System.IO.File.ReadAllBytes(file);
            //   return File(Server.MapPath(file), "application/pdf");
            return File(fileBytes, "application/pdf");

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
