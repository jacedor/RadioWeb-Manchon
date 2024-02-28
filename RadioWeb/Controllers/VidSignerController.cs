using RadioWeb.Models.Repos;
using RadioWeb.Models;
using RadioWeb.Models.VidSigner;
using RadioWeb.ViewModels.VidSigner;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SelectPdf;

namespace RadioWeb.Controllers
{
    public class VidSignerController : Controller
    {
        //  public static string RutaCONS = System.Configuration.ConfigurationManager.AppSettings["RutaCONS"];
        private RadioDBContext db = new RadioDBContext();

        [HttpPost]
        public ActionResult ListaRespuestas(int oid)
        {
            List<VID_RESPUESTAS> oRespuestas = new List<VID_RESPUESTAS>();

            List<VID_DOCUMENTOS> oDocumentos = db.Vid_Documentos.Where(d => d.OWNER == oid && !d.NOMBRE.Contains("LOPD")).ToList();
            //var oDocumento = db.Vid_Documentos.Where(d => d.OWNER == oid && !d.NOMBRE.Contains("LOPD")).FirstOrDefault();

            foreach (VID_DOCUMENTOS oDocumento in oDocumentos)
            {               
                var oRespuestasTmp = db.Vid_Respuestas.Where(r => r.OWNER == oDocumento.OID).ToList();

                foreach (VID_RESPUESTAS oRespuesta in oRespuestasTmp)
                {
                    oRespuestas.Add(oRespuesta);
                }



                
                ViewBag.RUTA_CONSEN = oDocumento.NOMBRE;
            }

            return PartialView("_Respuestas", oRespuestas);
        }

        [HttpPost]
        public ActionResult ListaPartial(int oid, bool esLopd = false, bool esEntrada = false)
        {
            EXPLORACION oExploracion = ExploracionRepositorio.Obtener(oid);
            //en funcion del owner de la exploracion sabremos el centro y rellenaremos las tablet
            //asociadas al mismo
            VWEnviarAFirmar oModel = new VWEnviarAFirmar();
            oModel.OIDEXPLORACION = oid;
            oModel.IOR_PACIENTE = oExploracion.PACIENTE.OID;
            oModel.DNI = oExploracion.PACIENTE.DNI;
            oModel.DNIRESPOSABLE = oExploracion.PACIENTE.DNIRESPONSABLE;
            oModel.RESPONSABLE = oExploracion.PACIENTE.RESPONSABLE;
            oModel.NOMBRE = oExploracion.PACIENTE.PACIENTE1;
            oModel.EDAD = oExploracion.PACIENTE.EDAD;
            //Las plantillas de consentimientos se guardan en la table de plantillas con el cid 1
            // y además tienen en su owner el grupo de aparatos al que perternecen
            //el campo otros4 es el idioma del paciente
            //el campo otros 4 es el idioma de la ficha del paciente
            P_INFORMES documentoLOPD = db.P_Informes
                   .Single(p => p.TITULO.Contains("LOPD")
                   && p.CANAL == oExploracion.PACIENTE.OTROS4);
            //SI HAY ALGUN DOCUMENTO DE LOPD
            if (documentoLOPD != null)
            {
                documentoLOPD.STATUS = (oExploracion.PACIENTE.NUEVA_LOPD == "T" ? "FIRMADO" : "NO FIRMADO");
                documentoLOPD.OIDEXPLORACION = oExploracion.OID;
                if (oExploracion.PACIENTE.NUEVA_LOPD == "T")
                {
                    //SI EL PACIENTE HA FIRMADO LA LOPD DEBERIA ESTAR EN LA TABLA VID DOCUMENTOS
                    //POR TANTO VAMOS A BUSCAR LA RUTA, PONEMOS EL TRY POR SI LO FIRMO EN PAPEL NORMAL
                    try
                    {
                        var oDocumento = db.Vid_Documentos
                     .Single(d => d.IOR_PACIENTE == oExploracion.IOR_PACIENTE && d.CID == 2);
                        documentoLOPD.RUTADOCUMENTO = oDocumento.NOMBRE;
                        documentoLOPD.FECHA =DateTime.Parse( oDocumento.FECHA);
                    }
                    catch (Exception)
                    {

                        documentoLOPD.STATUS = "FIRMADO EN PAPEL";
                    }
                }
                oModel.Documents = new List<P_INFORMES>();
                oModel.Documents.Add(documentoLOPD);
            }

            List<P_INFORMES> oListaConsens = db.P_Informes.Where(p => p.CID == 1 &&
                     p.OWNER == oExploracion.DAPARATO.OWNER &&
                     p.CANAL == oExploracion.PACIENTE.OTROS4).ToList();

            if (oListaConsens.Count > 0)
            {
                foreach (var item in oListaConsens)
                {
                    try
                    {
                        item.OIDEXPLORACION = oExploracion.OID;
                        var documento = db.Vid_Documentos.Where(d => d.OWNER.Value == oid && d.CID == 1).OrderByDescending(d => d.OID).FirstOrDefault();

                        if (documento != null)
                        {
                            item.RUTADOCUMENTO = documento.NOMBRE;
                            item.STATUS = "FIRMADO";
                        }
                        else
                        {
                            item.STATUS = "NO FIRMADO";
                        }
                       
                    }
                    catch (Exception)
                    {

                        item.STATUS = "NO FIRMADO";
                    }
                }
                oModel.Documents.AddRange(oListaConsens);

            }
            //en gamma delfos nos dijeron que un tipo de exploracion en concreto dentro de un grupo 
            //podia tener un consentimiento especifico distinto del general
            P_INFORMES CIEspefico = db.P_Informes.SingleOrDefault(p => p.VERS == oExploracion.IOR_TIPOEXPLORACION
            && p.CANAL == oExploracion.PACIENTE.OTROS4);
            if (CIEspefico != null)
            {
                oModel.Documents.Remove(oModel.Documents.Single(p => p.TITULO.Contains("CI")));
                CIEspefico.OIDEXPLORACION = oExploracion.OID;
                oModel.Documents.Add(CIEspefico);
            }

            foreach (LISTADIA otraExploracion in ListaDiaRepositorio.ObtenerPorPacienteYFecha(oExploracion.OID, oExploracion.PACIENTE.OID, (DateTime)oExploracion.FECHA))
            {
                //Si la exploracion del mismo dia y del mismo paciente pertenece a otro grupo de aparatos
                //agregamos el consentimiento para que los firme todos de una vez
                if (otraExploracion.GRUPOAPA != oExploracion.DAPARATO.OWNER)
                {
                    try
                    {
                            List<P_INFORMES> oOtroConsentimiento = db.P_Informes.Where(p => p.CID == 1 && p.OWNER == otraExploracion.GRUPOAPA && p.CANAL == oExploracion.PACIENTE.OTROS4).ToList();

                        for (int i = 0; i < oOtroConsentimiento.Count; i++)
                        {
                            oOtroConsentimiento[i].OIDEXPLORACION = otraExploracion.OID;
                            var documento = db.Vid_Documentos
                      .Where(d => d.OWNER.Value == otraExploracion.OID && d.CID == 1)
                      .OrderByDescending(d => d.OID)
                      .FirstOrDefault();

                            if (documento != null)
                            {
                                oOtroConsentimiento[i].RUTADOCUMENTO = documento.NOMBRE;
                                oOtroConsentimiento[i].STATUS = "FIRMADO";
                            }
                            else
                            {
                                oOtroConsentimiento[i].STATUS = "NO FIRMADO";
                            }

                            oModel.Documents.Add(oOtroConsentimiento[i]);
                        }
                           
                    }
                    catch (Exception)
                    {


                    }



                }

            }

            List<P_INFORMES> oResultDocuments = new List<P_INFORMES>();
            //LLegados a este punto tenemos todos los documentos de CI Y LOPD 
            for (int i = 0; i < oModel.Documents.Count; i++)
            {
                if (oResultDocuments.Where(p => p.OID == oModel.Documents[i].OID).ToList().Count == 0)
                {
                    GAPARATOS oGrupo = GAparatoRepositorio.Obtener(oModel.Documents[i].OWNER.Value);
                    oModel.Documents[i].GRUPO_APARATO = oGrupo.COD_GRUP;
                    if (oModel.Documents[i].STATUS == "FIRMADO")
                    {
                        oModel.Documents[i].ACCION = "FIRMADO";
                    }
                    else
                    {
                        oModel.Documents[i].ACCION = "FIRMAR";

                    }
                    if (oModel.Documents[i].TITULO.Contains("LOPD"))
                    {
                        oModel.Documents[i].TIPODOCUMENTO = "LOPD";
                    }
                    else
                    {
                        oModel.Documents[i].TIPODOCUMENTO = "CI";

                    }


                    oResultDocuments.Add(oModel.Documents[i]);
                }


            }
            oModel.Documents = oResultDocuments; 

            oModel.Devices = db.Tabletas.Where(p => p.IOR_CENTRO == oExploracion.DAPARATO.CID).ToList();
            //si se está llamando desde la ventana de entrada
            if (esEntrada)
            {
                return PartialView("EnviarATabletEntrada", oModel);
            }
            else
            {

                return PartialView("EnviarATablet", oModel);
            }

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
