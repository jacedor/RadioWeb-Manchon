using FirebirdSql.Data.FirebirdClient;
using Newtonsoft.Json;
using RadioWeb.Filters;
using RadioWeb.Models;
using RadioWeb.Models.Repos;
using RadioWeb.Models.VidSigner;
using RadioWeb.Utils;
using RadioWeb.ViewModels.VidSigner;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Web;

using System.Web.Http;
using System.Web.Mvc;
using VidCloudLib.DTOEntities;
using VidCloudLib.DTOs;
using ViDRestPubDTOs.DTOs.Forms;

namespace RadioWeb.ADPM
{

    public class docstatusController : ApiController
    {


        private string endPoint = "";
        private string rutaVidSigner = "";

        private Dictionary<string, string> oSubscripciones = new Dictionary<string, string>();
        private Dictionary<string, string> oSubscripcionesRuta = new Dictionary<string, string>();
        private static string sSubscriptionNameOK = "";
        private static string sSubscriptionPassOK = "";
        private static string sSubscriptionFolder = "";

        public docstatusController()
        {
            WebConfigRepositorio oConfig = new WebConfigRepositorio();
            endPoint = oConfig.ObtenerValor("EndPointVidSigner");
            rutaVidSigner = oConfig.ObtenerValor("RutaDocsVidSigner");

            string centrosVid = oConfig.ObtenerValor("CentrosVidSigner");
            string[] centrosArray = centrosVid.Split(',');

            foreach (string i in centrosArray)
            {
                try
                {
                    sSubscriptionNameOK = oConfig.ObtenerValor("UserNameVidSigner" + i);
                    sSubscriptionPassOK = oConfig.ObtenerValor("PasswordVidSigner" + i);
                    sSubscriptionFolder = oConfig.ObtenerValor("RutaCentroVidSigner" + i);
                    oSubscripciones.Add(sSubscriptionNameOK, sSubscriptionPassOK);
                    //oSubscripcionesRuta.Add(sSubscriptionNameOK, rutaVidSigner + @"\" +  sSubscriptionFolder);
                }
                catch (Exception)
                {

                }
            }

        }

        #region "Metodos VidSigner"

        public sealed class EstadoDocumento
        {
            public static readonly string All = "all";
            public static readonly string Signed = "signed";
            public static readonly string UnSigned = "unsigned";
            public static readonly string SignedDownloaded = "signeddownloaded";
            public static readonly string SignedNotDownloaded = "signednotdownloaded";
        }

        private string ObtenerCarpetaGuardarPDF(int centro)
        {
            WebConfigRepositorio oConfig = new WebConfigRepositorio();
            string rutaBase = oConfig.ObtenerValor("RutaDocsVidSigner") + @"\" +  oConfig.ObtenerValor("RutaCentroVidSigner" + centro.ToString());
           
          
            if (!System.IO.Directory.Exists(rutaBase))
            {
                System.IO.Directory.CreateDirectory(rutaBase);
            }
            string rutaBaseAnyo = rutaBase + @"\" + DateTime.Now.Year.ToString();
            string rutaBaseMes = rutaBase + @"\" + DateTime.Now.Year.ToString() + @"\" + DateTime.Now.Month.ToString("d2") + @"\";
            //Si no existe el directorio del año actual lo creamos
            if (!System.IO.Directory.Exists(rutaBaseAnyo))
            {
                System.IO.Directory.CreateDirectory(rutaBaseAnyo);
            }
            //Si no existe el directorio del año actual lo creamos
            if (!System.IO.Directory.Exists(rutaBaseMes))
            {
                System.IO.Directory.CreateDirectory(rutaBaseMes);
            }

            return rutaBaseMes;
        }

        private void DeleteSignedDocument(string DocGUI, string rutadocumento)
        {
            ServicePointManager.SecurityProtocol = (SecurityProtocolType)3072;

            HttpClient httpClient = new HttpClient();
            httpClient.BaseAddress = new Uri(endPoint);
            //Send
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", 
                Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(sSubscriptionNameOK + ":" + sSubscriptionPassOK)));
          //  httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            var response = httpClient.DeleteAsync("/api/signeddocuments/" + DocGUI).Result;
        }

        private static string getPDFB64(string sPDFpath)
        {
            byte[] pdfBytes = File.ReadAllBytes(sPDFpath);
            return Convert.ToBase64String(pdfBytes);
        }

        public void printDocumentInfoDTO(DocumentInfoDTO aDocInfoDTO, int oidDocumento)
        {

            foreach (SignerInfoDTO aSigner in aDocInfoDTO.Signers)
            {
                printSignerInfoDTO(aSigner, oidDocumento);
            }

        }
        public void printSignerInfoDTO(SignerInfoDTO aSignerInfoDTO, int oidDocumento)
        {

            if (aSignerInfoDTO.FormInfo != null)
                printFormInfoDTO(aSignerInfoDTO.FormInfo, oidDocumento);
        }

        public static bool AnularExploracion(int oidDocumento)
        {
            bool resultado = false;
            FbConnection oConexion = new FbConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ConexionBD"].ConnectionString);

            oConexion.Open();

            FbCommand oSelectDocumento = new FbCommand("Select * from VID_DOCUMENTOS WHERE OID=" + oidDocumento, oConexion);
            FbCommand oCommand = null;
            FbCommand oCommandTexto = null;
            try
            {
                FbDataReader oReader = oSelectDocumento.ExecuteReader();
                int oidExploracion = -1;
                //OBTENEMOS EL OID DE LA EXPLORACION QUE ESTA EN LA TABLA VID_DOCUMENTOS
                while (oReader.Read())
                {
                    oidExploracion = int.Parse(oReader["OWNER"].ToString());
                }

                if (oidExploracion > 0)
                {
                    //MARCAMOS LA EXPLORACION COMO BORRADA
                    string updateStament = "update exploracion set CID=14, estado=0,borrado='A' where oid = " + oidExploracion;
                    oCommand = new FbCommand(updateStament, oConexion);
                    oCommand.ExecuteNonQuery();

                    oCommandTexto = new FbCommand("UPDATE OR INSERT INTO TEXTOS (OWNER,TEXTO) VALUES ("
                         + oidExploracion + ",'ANULADA POR RESPUESTA EN CONSENTIMIENTO')MATCHING  (OWNER)", oConexion);
                    int result = oCommandTexto.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
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
                    };
					if (oSelectDocumento != null)
                    {
                        oSelectDocumento.Dispose();
                    };
					 if (oCommandTexto != null)
                    {
                        oCommandTexto.Dispose();
                    };
                }
                if (oConexion.State == System.Data.ConnectionState.Open)
                {
                    oConexion.Close();

                }

            }

            return resultado;
        }

        public static void InsertarRespuesta(int oidConsentimiento, string pregunta, string respuesta,string idPregunta, int tipo = 1)
        {
            string respuestaTexto = "null";
            if (tipo != 1)
            {
                respuestaTexto = respuesta;
                respuesta = "null";
            }
            else
            {

                if (respuesta.ToUpper() == "TRUE")
                {
                    respuesta = "T";
                }
                else
                {
                    respuesta = "F";
                }
            }
            FbConnection oConexion = new FbConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ConexionBD"].ConnectionString);

            oConexion.Open();
            try
            {
                string macroRespuesta = "Select MACRO FROM VID_PREGUNTAS WHERE OID=" + idPregunta;
                FbCommand oCommandPregunta = new FbCommand(macroRespuesta, oConexion);
                FbDataReader oReader = oCommandPregunta.ExecuteReader();
                string macroPregunta = "";
                while (oReader.Read())
                {
                    macroPregunta = DataBase.GetStringFromReader(oReader, "MACRO");
                }

                FbCommand oInsert = new FbCommand("INSERT INTO VID_RESPUESTAS(OID, CID, OWNER, PREGUNTA, RESPUESTABOOL, RESPUESTATEXTO,MACRO) values (gen_id(GENUID,1)," +
                    1 + "," + oidConsentimiento + ",'" + pregunta.Replace("'", "''") + "','" + respuesta + "','" + respuestaTexto.Replace("'", "''") + "','" + macroPregunta + "')", oConexion);
                oInsert.CommandText = oInsert.CommandText.Replace("'null'", "null");
                oInsert.ExecuteNonQuery();
               
            }
            catch (Exception ex)
            {

                LogException.LogMessageToFile(ex.Message);
            }
            finally
            {
 oConexion.Close();
            }
          
        }

        public bool PreguntaAnulaPrueba(int oidPregunta)
        {
            bool resultado = false;
            FbConnection oConexion = new FbConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ConexionBD"].ConnectionString);

            oConexion.Open();

            FbCommand oSelectPregunta = new FbCommand("Select * from VID_PREGUNTAS WHERE OID=" + oidPregunta, oConexion);

            FbDataReader oReader = oSelectPregunta.ExecuteReader();
            while (oReader.Read())
            {
                if (oReader["ANULAPRUEBA"].ToString() == "T")
                {
                    resultado = true;
                }
            }
            oConexion.Close();
            return resultado;
        }

        public void printFormInfoDTO(FormInfoDTO aFormInfoDTO, int oidDocumento)
        {
            bool AlgunaPreguntaAnulaExploracion = false;
            if (aFormInfoDTO.Sections != null && aFormInfoDTO.Sections.Count > 0)
            {
                foreach (FormSectionInfoDTO aFormSectionInfo in aFormInfoDTO.Sections)
                {
                    if (aFormSectionInfo.Groups != null && aFormSectionInfo.Groups.Count > 0)
                    {
                        foreach (FormGroupInfoDTO aFormGroupInfoDTO in aFormSectionInfo.Groups)
                        {
                            if (!String.IsNullOrEmpty(aFormGroupInfoDTO.Title))
                            {
                                Console.WriteLine("GroupTitle=" + aFormGroupInfoDTO.Title);
                            }

                            if (aFormGroupInfoDTO.CheckBoxes != null && aFormGroupInfoDTO.CheckBoxes.Count > 0)
                            {
                                foreach (FormCheckBoxInfoDTO aFormCheckBoxInfoDTO in aFormGroupInfoDTO.CheckBoxes)
                                {
                                    int idPregunta = int.Parse(aFormCheckBoxInfoDTO.Id);
                                    //si ninguna de las pregunta ha provocado anular la exploracion
                                    if (!AlgunaPreguntaAnulaExploracion)
                                    {

                                        AlgunaPreguntaAnulaExploracion = PreguntaAnulaPrueba(idPregunta) && aFormCheckBoxInfoDTO.Response.ToUpper() == "TRUE";
                                    }

                                    InsertarRespuesta(oidDocumento, aFormCheckBoxInfoDTO.Title, aFormCheckBoxInfoDTO.Response,aFormCheckBoxInfoDTO.Id, 1);

                                }
                            }
                            if (aFormGroupInfoDTO.RadioButtons != null && aFormGroupInfoDTO.RadioButtons.Count > 0)
                            {
                                foreach (FormRadioButtonInfoDTO aFormRadioButtonInfoDTO in aFormGroupInfoDTO.RadioButtons)
                                {
                                    if (!String.IsNullOrEmpty(aFormRadioButtonInfoDTO.Title))
                                    {
                                        Console.WriteLine("RadioButton Title=" + aFormRadioButtonInfoDTO.Title);
                                    }
                                    Console.WriteLine("RadioButton ID=" + aFormRadioButtonInfoDTO.Id);
                                    Console.WriteLine("RadioButton Response=" + aFormRadioButtonInfoDTO.Response);

                                    InsertarRespuesta(oidDocumento, aFormRadioButtonInfoDTO.Title, aFormRadioButtonInfoDTO.Response, aFormRadioButtonInfoDTO.Id, 3);
                                }
                            }
                            if (aFormGroupInfoDTO.TextBoxes != null && aFormGroupInfoDTO.TextBoxes.Count > 0)
                            {
                                foreach (FormTextBoxInfoDTO aFormTextBoxInfoDTO in aFormGroupInfoDTO.TextBoxes)
                                {
                                    if (!String.IsNullOrEmpty(aFormTextBoxInfoDTO.Title))
                                    {
                                        Console.WriteLine("TextBox Title=" + aFormTextBoxInfoDTO.Title);
                                    }
                                    InsertarRespuesta(oidDocumento, aFormTextBoxInfoDTO.Title, aFormTextBoxInfoDTO.Response, aFormTextBoxInfoDTO.Id,2);
                                }
                            }
                        }
                    }
                }


                if (AlgunaPreguntaAnulaExploracion)
                {
                    AnularExploracion(oidDocumento);
                }
            }
        }

        public void GetDocumentInfo(String aDocGUI, int oidDocumento)
        {

            ServicePointManager.SecurityProtocol = (SecurityProtocolType)3072;

            HttpClient httpClient = new HttpClient();
            httpClient.BaseAddress = new Uri(endPoint);
            //Send
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(sSubscriptionNameOK + ":" + sSubscriptionPassOK)));
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            var response = httpClient.GetAsync("/api/documentinfo/" + aDocGUI).Result;
            if (response.IsSuccessStatusCode)
            {
                DocumentInfoDTO aDocInfoDTO = response.Content.ReadAsAsync<DocumentInfoDTO>().Result;
                //System.Threading.Thread.Sleep(1000);
                printDocumentInfoDTO(aDocInfoDTO, oidDocumento);
            }
            else
            {
                Console.WriteLine("Error GetDocumentInfo: " + response.StatusCode);
            }
        }

        public Document GetSignedDocument(string DocUI, string filename)
        {

            ServicePointManager.SecurityProtocol =  (SecurityProtocolType)3072;

            Document oDocumentoRetornado = new Document();
            try
            {
                //si el fichero es un consentimiento informado o una LOPD, 
                //la información de a qué exploracion pertenece esta aqui
                //PARAMETROS[1];
                string OIDPACIENTE = "";
                string OID_EXPLORACION = "";
                bool esDocumentoLOPDCI = false;
                string nombreFicheroDestino = "";
                if (filename.Contains("_"))
                {
                    string[] PARAMETROS = filename.Split('_');
                    OIDPACIENTE = PARAMETROS[2].Replace(".pdf", "");
                    OID_EXPLORACION = PARAMETROS[1];
                    esDocumentoLOPDCI = true;
                }
                else
                {
                    //si no es un documento de LOPD o CI es porque esta en la tabla imágenes. 
                    
                    IMAGENES oDocumento = ImagenesRepositorio.Obtener(int.Parse(filename.Substring(0, filename.IndexOf("."))));

                    if (oDocumento.OID<0)
                    {
                         oDocumento = ImagenesRepositorio.Obtener(DocUI);
                    }
                    OIDPACIENTE = oDocumento.IOR_PACIENTE.Value.ToString();
                    OID_EXPLORACION = oDocumento.IOR_EXPLORACION.Value.ToString();
                    nombreFicheroDestino = oDocumento.PATH + oDocumento.NOMBRE + "." + oDocumento.EXT;
                }

                LISTADIA oExplo = ListaDiaRepositorio.Obtener(int.Parse(OID_EXPLORACION));
                WebConfigRepositorio oConfig = new WebConfigRepositorio();
                sSubscriptionNameOK = oConfig.ObtenerValor("UserNameVidSigner" + oExplo.CENTRO);
                sSubscriptionPassOK = oConfig.ObtenerValor("PasswordVidSigner" + oExplo.CENTRO);
                DocumentDTO resultFile = new DocumentDTO();
                ServicePointManager.SecurityProtocol = (SecurityProtocolType)3072;
                HttpClient httpClient = new HttpClient();
                httpClient.BaseAddress = new Uri(endPoint);
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic",
                    Convert.ToBase64String(Encoding.UTF8.GetBytes(sSubscriptionNameOK + ":" + sSubscriptionPassOK)));
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                var response = httpClient.GetAsync("/api/signeddocuments/" + DocUI).Result;
                if (response.IsSuccessStatusCode)
                {
                    var responseBody = response.Content.ReadAsStringAsync().Result;
                    resultFile = JsonConvert.DeserializeObject<DocumentDTO>(responseBody);
                }
                else
                {
                    throw new Exception();
                }
                string textoLog = "";
                oDocumentoRetornado.DocGui = DocUI;
                if (esDocumentoLOPDCI)
                {
                    string rutaEscrituroConAnyoMes = ObtenerCarpetaGuardarPDF( oExplo.CENTRO);
                    FileInfo oFileInfo = new FileInfo(resultFile.FileName);
                    nombreFicheroDestino = rutaEscrituroConAnyoMes + oFileInfo.Name;
                    System.IO.File.WriteAllBytes(nombreFicheroDestino, Convert.FromBase64String(resultFile.DocContent));
                
                    FbConnection oConexion = new FbConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ConexionBD"].ConnectionString);

                    oConexion.Open();

                    int tipoDocumento = 1;
                    if (oFileInfo.Name.ToUpper().Contains("LOPD"))
                    {
                        tipoDocumento = 2;

                    }
                    LOGVIDSIGNER oLog = new LOGVIDSIGNER
                    {
                        IOR_EXPLORACION = oExplo.OID,
                        FECHA = DateTime.Now.ToString("dd/MM/yyyy HH:mm"),
                        TEXTO = "Descargando " + oFileInfo.Name,
                        PLANTILLA =(tipoDocumento==1?"CI":"LOPD"),
                        USUARIO = "VIDSIGNER",
                        ACCION = "DESCARGAR",
                        DOCGUID = DocUI,
                        IOR_PACIENTE = oExplo.IOR_PACIENTE
                    };
                    LogVidSignerRepositorio.Insertar(oLog);

                    FbCommand oInsert = new FbCommand("INSERT INTO VID_DOCUMENTOS (OID,CID,IOR_PACIENTE, NOMBRE, OWNER, FECHA,DOCGUI) values (gen_id(GENUID,1)," +
                        tipoDocumento + "," + OIDPACIENTE + ",'" + nombreFicheroDestino.Replace("'", "''") + "'," + OID_EXPLORACION + ",'" +
                        DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "','" + DocUI + "') returning oid", oConexion);

                    oDocumentoRetornado.OID = (int)oInsert.ExecuteScalar();
                    oDocumentoRetornado.FileName = nombreFicheroDestino;
                    oDocumentoRetornado.OIDEXPLORACION = int.Parse(OID_EXPLORACION);
                    oDocumentoRetornado.OIDPACIENTE = int.Parse(OIDPACIENTE);

                    //SI EL NOMBRE DEL FICHERO DESCARGADO NO CONTIENE LA PALABRA LOPD ES QUE SE TRATA DE UN CONSENTIMIENTO INFORMADO
                    if (!oFileInfo.Name.ToUpper().Contains("LOPD"))
                    {
                        FbCommand oUpdate = new FbCommand("UPDATE EXPLORACION SET FIRMA_CONSEN='T' WHERE OID= " + OID_EXPLORACION, oConexion);
                        oUpdate.ExecuteNonQuery();
                        oConexion.Close();
                        oLog.TEXTO = oLog.TEXTO + " Marcar Firma_Consen";
                        if (oUpdate != null)
                        {
                            oUpdate.Dispose();
                        }
                    }
                    else
                    {
                        FbCommand oUpdate = new FbCommand("UPDATE PACIENTE SET NUEVA_LOPD ='T' WHERE OID= " + OIDPACIENTE, oConexion);
                        oUpdate.ExecuteNonQuery();
                        oConexion.Close();
                        if (oUpdate != null)
                        {
                            oUpdate.Dispose();
                        }
                        oLog.TEXTO = oLog.TEXTO +  " Marcar LOPD";

                    }
                   
                }
                else
                {
                    if (File.Exists(nombreFicheroDestino))
                    {
                        File.Delete(nombreFicheroDestino);
                    }
                    System.IO.File.WriteAllBytes(nombreFicheroDestino, Convert.FromBase64String(resultFile.DocContent));
                }

                //System.Text.UTF8Encoding encoding = new System.Text.UTF8Encoding();



                return oDocumentoRetornado;


            }
            catch (Exception ex)
            {


                return oDocumentoRetornado;
            }


        }

        #endregion



        /*
        //este métoddo mira si el documento especificado ya ha sido descargado
        public HttpResponseMessage Get(string docGuid)
        {
           
            string idDoc = docGuid.Split(':')[1].Substring(1, 36);
            var oDocumento = VidDocumentosRepositorio.Obtener(idDoc);
            HttpResponseMessage response = null;
            if (oDocumento == null)
            {
               // response = Request.CreateResponse(HttpStatusCode.NotFound);
               response = Request.CreateResponse(HttpStatusCode.OK, "NO FIRMADO");

            }
            else {
               response= Request.CreateResponse(HttpStatusCode.OK, "FIRMADO");
            }
            return response;
        }
        */


        public HttpResponseMessage Get(string docGuid)
        {

            //GetDocumentInfo(docGuid, 0);
            Document pDocumentoDescargado = GetSignedDocument(docGuid, "GAYET,NOEMI_22496306_18054115_LOPD_20210608125340.pdf");

            HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.OK, "FIRMADO");
            return response;
        }



        [VidSignerBasicAuthentication]
        public HttpResponseMessage Post(string id, [FromBody]Document Document)
        {
            LogException.LogMessageToFile("Recibida llamada API VIDSIGNER: "+ Document.DocGui);

            if (Document.DocStatus.ToUpper() == EstadoDocumento.Signed.ToUpper())
            {
                Document pDocumentoDescargado = GetSignedDocument(Document.DocGui, Document.FileName);
                if (File.Exists(pDocumentoDescargado.FileName))
                {
                    try
                    {
                        GetDocumentInfo(Document.DocGui, pDocumentoDescargado.OID);
                        ////Borramos el documento
                        DeleteSignedDocument(pDocumentoDescargado.DocGui, pDocumentoDescargado.FileName);

                    }
                    catch (Exception ex)
                    {

                        throw ex;
                    }

                }
            }

            HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.OK);
            return response;
        }
    }
}