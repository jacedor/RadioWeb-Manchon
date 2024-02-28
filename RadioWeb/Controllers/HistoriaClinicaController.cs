using RadioWeb.Models;
using RadioWeb.Models.Logica;
using RadioWeb.Models.Repos;
using RadioWeb.Utils;
using RadioWeb.ViewModels.Paciente;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace RadioWeb.Controllers
{
    public class HistoriaClinicaController : Controller
    {
        private RadioDBContext db = new RadioDBContext();

        [HttpGet]
        public ActionResult HistoriaEnEditor(int oid)
        {
            HISTORIAS oHistoria = db.Historias.SingleOrDefault(h => h.IOR_PACIENTE == oid);
            VMHistoriaClinica oViewModel = new VMHistoriaClinica();
            oViewModel.OIDPACIENTE = oid;
            //Si ya existe una historia clinica
            if (oHistoria != null)
            {
                oViewModel.OID = oHistoria.OID;
                oViewModel.CID = 1;
                oViewModel.MODIF = oHistoria.MODIF.Value;
                oViewModel.TEXTOHTML = TextosRepositorio.Obtener(oHistoria.OID).TEXTO;
                if (oViewModel.TEXTOHTML.StartsWith("{\\rtf1"))
                {
                    oViewModel.TEXTOHTML = DataBase.convertRtfToHtml(TextosRepositorio.Obtener(oHistoria.OID).TEXTO);
                    oViewModel.TEXTOHTML = oViewModel.TEXTOHTML.Replace("<p style=\"margin:0pt 0pt 0pt 0pt;line-height:normal;\"><span class=\"st1\">&nbsp;</span></p>", "");
                    oViewModel.TEXTOHTML = oViewModel.TEXTOHTML.Replace("The trial version of RTF-to-HTML DLL .Net can convert up to 10000 symbols.", "");
                    oViewModel.TEXTOHTML = oViewModel.TEXTOHTML.Replace("Get the full featured version!", "");
                }

            }
            return PartialView("HistoriaEditor", oViewModel);
        }

        [HttpGet]
        public ActionResult IndexNew(int oid)
        {
            HISTORIAS oHistoria = db.Historias.SingleOrDefault(h => h.IOR_PACIENTE == oid);
            VMHistoriaClinica oViewModel = new VMHistoriaClinica();
            oViewModel.OIDPACIENTE = oid;
            //Si ya existe una historia clinica
            if (oHistoria != null)
            {
                oViewModel.OID = oHistoria.OID;
                oViewModel.CID = 1;
                oViewModel.MODIF = (oHistoria.MODIF.HasValue ? oHistoria.MODIF.Value : DateTime.MinValue);
                oViewModel.TEXTOHTML = TextosRepositorio.Obtener(oHistoria.OID).TEXTO;
                if (oViewModel.TEXTOHTML.StartsWith("{\\rtf1"))
                {
                    oViewModel.TEXTOHTML = DataBase.convertRtfToHtml(TextosRepositorio.Obtener(oHistoria.OID).TEXTO);
                    oViewModel.TEXTOHTML = oViewModel.TEXTOHTML.Replace("<p style=\"margin:0pt 0pt 0pt 0pt;line-height:normal;\"><span class=\"st1\">&nbsp;</span></p>", "");
                    oViewModel.TEXTOHTML = oViewModel.TEXTOHTML.Replace("The trial version of RTF-to-HTML DLL .Net can convert up to 10000 symbols.", "");
                    oViewModel.TEXTOHTML = oViewModel.TEXTOHTML.Replace("Get the full featured version!", "");
                }

            }
            return PartialView("HistoriaNew", oViewModel);
        }

        [HttpGet]
        public ActionResult Index(int oid)
        {
            HISTORIAS oHistoria = db.Historias.SingleOrDefault(h => h.IOR_PACIENTE == oid);
            VMHistoriaClinica oViewModel = new VMHistoriaClinica();
            oViewModel.OIDPACIENTE = oid;
            //Si ya existe una historia clinica
            if (oHistoria != null)
            {
                oViewModel.OID = oHistoria.OID;
                oViewModel.CID = 1;               
                oViewModel.MODIF = (oHistoria.MODIF.HasValue? oHistoria.MODIF.Value: DateTime.MinValue);
                oViewModel.TEXTOHTML = TextosRepositorio.Obtener(oHistoria.OID).TEXTO;
                if (oViewModel.TEXTOHTML.StartsWith("{\\rtf1"))
                    {
                        oViewModel.TEXTOHTML = DataBase.convertRtfToHtml(TextosRepositorio.Obtener(oHistoria.OID).TEXTO);
                        oViewModel.TEXTOHTML = oViewModel.TEXTOHTML.Replace("<p style=\"margin:0pt 0pt 0pt 0pt;line-height:normal;\"><span class=\"st1\">&nbsp;</span></p>", "");
                        oViewModel.TEXTOHTML = oViewModel.TEXTOHTML.Replace("The trial version of RTF-to-HTML DLL .Net can convert up to 10000 symbols.", "");
                        oViewModel.TEXTOHTML = oViewModel.TEXTOHTML.Replace("Get the full featured version!", "");
                    }
                   
            }
            return PartialView("Historia",oViewModel);
        }

        [HttpPost]
        public ActionResult GuardarHc(VMHistoriaClinica viewModel)
        {
            ResultadoRequest oResult;
            try
            {
                var hc = db.Historias.SingleOrDefault(i => i.IOR_PACIENTE == viewModel.OIDPACIENTE);

                if (hc != null)
                {
                    hc.MODIF = DateTime.Now;
                    //ponemos el oid de la hc para luego actualizar la tabla textos mas abajo
                    viewModel.OID = hc.OID;
                    db.SaveChanges();
                }
                else
                {
                    HISTORIAS oNuevaHistoria = new HISTORIAS
                    {
                        CID = 1,
                        CANAL = "F",
                        IOR_PACIENTE = viewModel.OIDPACIENTE,
                        MODIF = DateTime.Now
                    };
                    db.Historias.Add(oNuevaHistoria);
                    db.SaveChanges();
                    viewModel.OID = oNuevaHistoria.OID;
                }

                TEXTOS oTexto = new TEXTOS
                {
                    TEXTO = viewModel.TEXTOHTML,
                    OWNER = viewModel.OID,
                    CANAL= "HISTOR"
                };

                oTexto.TEXTO = oTexto.TEXTO.Replace("<title>Untitled document</title>", "");
                TextosRepositorio.InsertarOrUpdate(oTexto);
            }
            catch (Exception ex)
            {
                 oResult = new ResultadoRequest() {
                    Mensaje= "Se ha producido un error al guarda la historia. " + ex.Message,
                    Resultado= ResultadoRequest.RESULTADO.ERROR
                };
                return PartialView("_ResultRequest", oResult);
            }

            oResult = new ResultadoRequest()
            {
                Mensaje = "Se ha guardado la Historia Clinica. ",
                Resultado = ResultadoRequest.RESULTADO.SUCCESS
            };
            return PartialView("_ResultRequest",oResult);
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