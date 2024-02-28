using RadioWeb.Models;
using RadioWeb.Models.Repos;
using RadioWeb.Models.VidSigner;
using RadioWeb.ViewModels.VidSigner;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace RadioWeb.ADPM
{
    public class VidSignerController : ApiController
    {

        public HttpResponseMessage Post([FromBody]VWEnviarAFirmar oFirmar)
        {
            try
            {
                LISTADIA oExploracion = ListaDiaRepositorio.Obtener(oFirmar.OIDEXPLORACION);
                PACIENTE oPaciente = PacienteRepositorio.Obtener(oExploracion.IOR_PACIENTE);

                if (oPaciente.DNI != oFirmar.DNI && !String.IsNullOrEmpty(oFirmar.DNI))
                {
                    PacienteRepositorio.UpdateCampo("DNI", oFirmar.DNI, oPaciente.OID);
                }

                if (oPaciente.DNIRESPONSABLE != oFirmar.DNIRESPOSABLE && !String.IsNullOrEmpty(oFirmar.DNIRESPOSABLE))
                {
                    PacienteRepositorio.UpdateCampo("RDNI", oFirmar.DNIRESPOSABLE, oPaciente.OID);
                }
                if (oPaciente.RESPONSABLE != oFirmar.RESPONSABLE && !String.IsNullOrEmpty(oFirmar.RESPONSABLE))
                {
                    PacienteRepositorio.UpdateCampo("RNOMBRE", oFirmar.RESPONSABLE, oPaciente.OID);
                }

                WebConfigRepositorio oConfig = new WebConfigRepositorio();
                string userVid = oConfig.ObtenerValor("UserNameVidSigner" + oExploracion.CENTRO);
                string passVid = oConfig.ObtenerValor("PasswordVidSigner" + oExploracion.CENTRO);
                VidSignerClient oClientVid = new VidSignerClient(userVid, passVid);
                //llegados este punto pude ser que queramos enviar un documento ya generado
                //entramos en el if o un documento sobre plantilla, entrariamos por el Else
                if (oFirmar.ESDOCUMENTO)
                {
                    if (oClientVid.EnviarDocumento(oFirmar.DocumentSelected, oFirmar.OIDEXPLORACION, oFirmar.DeviceSelected))
                    {
                        HttpResponseMessage response = Request
                            .CreateResponse(HttpStatusCode.Created, oClientVid.DocUID);
                        
                        LOGVIDSIGNER oLog = new LOGVIDSIGNER
                        {
                            IOR_EXPLORACION = oExploracion.OID,
                            FECHA = DateTime.Now.ToString("dd/MM/yyyy HH:mm"),
                            TEXTO = "Enviado a Firmar " + oExploracion.COD_FIL + ' ' + oExploracion.FIL,
                            PLANTILLA = "Report " + oFirmar.DocumentSelected,
                            USUARIO = User.Identity.Name,
                            ACCION = "ENVIAR",
                            DOCGUID = oClientVid.DocUID,
                            IOR_PACIENTE = oExploracion.IOR_PACIENTE

                        };
                        LogVidSignerRepositorio.Insertar(oLog);
                        return response;
                    }
                    else
                    {
                        HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.InternalServerError, oClientVid.ErrorMessage);
                        return response;
                    }
                }
                else
                {
                    if (oClientVid.EnviarConFormulario(oFirmar.DocumentSelected,
                        oFirmar.OIDEXPLORACION, oFirmar.DeviceSelected))
                    {
                        HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.Created, oClientVid.DocUID);
                        P_INFORMES oPlantilla = P_InformesRepositorio.Obtener (oFirmar.DocumentSelected);
                        LOGVIDSIGNER oLog = new LOGVIDSIGNER
                        {
                            IOR_EXPLORACION = oExploracion.OID,
                            FECHA = DateTime.Now.ToString("dd/MM/yyyy HH:mm"),
                            TEXTO = "Enviado a Firmar " + oExploracion.COD_FIL + ' ' + oExploracion.FIL,
                            PLANTILLA = oPlantilla.TITULO,
                            USUARIO = User.Identity.Name,
                            ACCION = "ENVIAR",
                            DOCGUID = oClientVid.DocUID.Split(':')[1].Substring(1, 36),
                            IOR_PACIENTE = oExploracion.IOR_PACIENTE

                        };
                        LogVidSignerRepositorio.Insertar(oLog);
                        return response;
                    }
                    else
                    {
                        HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.InternalServerError, oClientVid.ErrorMessage);
                        return response;
                    }
                }
            }
            catch (Exception ex)
            {
                HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
                return response;

            }

        }

        // POST: Este es el POST que usa massana
        public HttpResponseMessage Post(string plantilla)
        {
            try
            {
                string[] valoresMassana = plantilla.Split(new string[] { "---" }, StringSplitOptions.None);
                LISTADIA oExploracion = ListaDiaRepositorio.Obtener(int.Parse(valoresMassana[1]));

                WebConfigRepositorio oConfig = new WebConfigRepositorio();
                string userVid = oConfig.ObtenerValor("UserNameVidSigner" + oExploracion.CENTRO);
                string passVid = oConfig.ObtenerValor("PasswordVidSigner" + oExploracion.CENTRO);


                VidSignerClient oClientVid = new VidSignerClient(userVid, passVid);



                if (oClientVid.EnviarConFormulario(int.Parse(valoresMassana[0]), int.Parse(valoresMassana[1]), valoresMassana[2]))
                {
                    HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.Created, oClientVid.DocUID);
                    P_INFORMES oPlantilla = P_InformesRepositorio.Obtener(int.Parse (valoresMassana[0]));

                    LOGVIDSIGNER oLog = new LOGVIDSIGNER
                    {
                        IOR_EXPLORACION = oExploracion.OID,
                        FECHA = DateTime.Now.ToString("dd/MM/yyyy HH:mm"),
                        TEXTO = "RadIB - Enviado a Firmar " + oExploracion.COD_FIL + ' ' + oExploracion.FIL,
                        PLANTILLA = "Report " + oPlantilla.TITULO ,
                        USUARIO = User.Identity.Name,
                        ACCION = "ENVIAR",
                        DOCGUID = oClientVid.DocUID.Split(':')[1].Substring(1, 36),
                        IOR_PACIENTE = oExploracion.IOR_PACIENTE

                    };
                    LogVidSignerRepositorio.Insertar(oLog);
                    return response;
                }
                else
                {
                    HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.InternalServerError, oClientVid.ErrorMessage);
                    return response;
                }
            }
            catch (Exception ex)
            {
                HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
                return response;

            }

        }


    }
}
