using FastReport.Cloud.OAuth;
using Glimpse.Core.Extensions;
using OfficeOpenXml.FormulaParsing.Excel.Functions.DateTime;
using RadioWeb.Models;
using RadioWeb.Models.Repos;
using RestSharp.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Xml;

namespace RadioWeb.Controllers
{
    public class QReportController : Controller
    {

        private string xmlStudy;
        public string visorUrlWeb;
        public string visorUrlDiagnostico;
        public string visorUrlZip;

        //TOKEN REQUEST
        public String getToken() {

            String token = "";

            if (!this.isTokenExpired())
            {
                //Recuperamos el token de db.
                Models.QREPORT_CONF qc = QreportConfigRepositorio.Obtener();
                token = qc.TOKEN;

            }
            else {
                //Solicitamos un nuevo token.
                token = this.requestToken();
            }
                        
            return token;
        }

        public String requestToken() {

            String token = "";

            Models.QREPORT_CONF qc = QreportConfigRepositorio.Obtener();

            StringBuilder xmlMessage = new StringBuilder();
            xmlMessage.AppendLine("<soapenv:Envelope xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" xmlns:soapenv=\"http://schemas.xmlsoap.org/soap/envelope/\">");
            xmlMessage.AppendLine("<soapenv:Header/>");
            xmlMessage.AppendLine("<soapenv:Body>");
            xmlMessage.AppendLine("<PeticionToken soapenv:encodingStyle=\"http://schemas.xmlsoap.org/soap/encoding/\">");
            xmlMessage.AppendLine("<part xsi:type=\"api:PeticionToken\" xmlns:api=\""+qc.URL+"\">");
            xmlMessage.AppendLine("<PublicKey xsi:type=\"xsd:string\">"+qc.PUBLICKEY_WS+"</PublicKey>");
            xmlMessage.AppendLine("<Usuario xsi:type=\"xsd:string\">"+qc.USER_WS+"</Usuario>");
            xmlMessage.AppendLine("<Password xsi:type=\"xsd:string\">"+qc.PASS_WS+"</Password>");
            xmlMessage.AppendLine("</part>");
            xmlMessage.AppendLine("</PeticionToken>");
            xmlMessage.AppendLine("</soapenv:Body>");
            xmlMessage.AppendLine("</soapenv:Envelope>");
            Int32 LogID = QreportLogRepositorio.Insertar("Token", xmlMessage.ToString());


            String responseString = this.SendSoapMessage(xmlMessage.ToString(), qc.URL);
            QreportLogRepositorio.updateRespuesta(LogID, responseString);

            XmlDocument responseXml = new XmlDocument();
            responseXml.LoadXml(responseString);

            String status = responseXml.GetElementsByTagName("Status")[0].InnerText;
            String xmlString = responseXml.GetElementsByTagName("XML")[0].InnerText;

            if (status.Equals("OK")) 
            {
                QreportLogRepositorio.updateEstado(LogID, "OK");
                XmlDocument responseXml2 = new XmlDocument();
                responseXml2.LoadXml(xmlString);
                token = responseXml2.GetElementsByTagName("token")[0].InnerText;
                String validez = responseXml2.GetElementsByTagName("validez")[0].InnerText;

               
                Boolean updated = QreportConfigRepositorio.updateToken(token, validez);
                if (updated) {
                    return token;
                }

            }
            else 
            {
                QreportLogRepositorio.updateEstado(LogID, "OK");
            }

            return token;

        }

        public Boolean isTokenExpired() {

            Models.QREPORT_CONF qc = QreportConfigRepositorio.Obtener();

            DateTime tomorrow = DateTime.Now;
            tomorrow = tomorrow.AddDays(1);

            //Si la fecha expiración del token está vacia o 
            //la fecha del token está a 1 dia de caducar.
            if (qc.TOKEN_EXPIRATION == null || DateTime.Compare(tomorrow, (DateTime)qc.TOKEN_EXPIRATION)>0)
            {
                return true;
            }

            return false;
        }

        private  string UnescapeXMLValue(string xmlString)
        {
            if (xmlString == null)
                throw new ArgumentNullException("xmlString");
                return xmlString.Replace("&apos;", "'").Replace("&quot;", "\"").Replace("&gt;", ">").Replace("&lt;", "<").Replace("&amp;", "&");
        }

        private  string EscapeXMLValue(string xmlString)
        {

            if (xmlString == null)
                throw new ArgumentNullException("xmlString");       
                return xmlString.Replace("&", "&amp;").Replace("'", "&apos;").Replace("\"", "&quot;").Replace(">", "&gt;").Replace("<", "&lt;");
        }

        public String getStudyUID(String patientId, String accNumber, String sDtVisita) {
            String studyInstanceUID = "";

            Models.QREPORT_CONF qc = QreportConfigRepositorio.Obtener();

            StringBuilder messageXml = new StringBuilder("");
            messageXml.AppendLine("<soapenv:Envelope xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" xmlns:soapenv=\"http://schemas.xmlsoap.org/soap/envelope/\">");
            messageXml.AppendLine("<soapenv:Header/>");
            messageXml.AppendLine("<soapenv:Body>");
            messageXml.AppendLine("<PeticionEstudios soapenv:encodingStyle=\"http://schemas.xmlsoap.org/soap/encoding/\">");
            messageXml.AppendLine("<part xsi:type=\"api:PeticionEstudios\" xmlns:api=\""+qc.URL+"\">");
            messageXml.AppendLine("<PublicKey xsi:type=\"xsd:string\">"+qc.PUBLICKEY_WS+"</PublicKey>");
            messageXml.AppendLine("<Token xsi:type=\"xsd:string\">"+qc.TOKEN+"</Token>");
            messageXml.AppendLine("<PatientID xsi:type=\"xsd:string\">"+patientId+"</PatientID>");
            messageXml.AppendLine("<AccNumber xsi:type=\"xsd:string\">"+accNumber+"</AccNumber>");
            messageXml.AppendLine("<Fecha xsi:type=\"xsd:string\">"+ sDtVisita + "</Fecha>");
            messageXml.AppendLine("</part>");
            messageXml.AppendLine("</PeticionEstudios>");
            messageXml.AppendLine("</soapenv:Body>");
            messageXml.AppendLine("</soapenv:Envelope>");
            Int32 LogID = QreportLogRepositorio.Insertar("PeticionEstudios", messageXml.ToString());
            
            String responseString = this.SendSoapMessage(messageXml.ToString(), qc.URL);
            QreportLogRepositorio.updateRespuesta(LogID, responseString);

            XmlDocument responseXml = new XmlDocument();
            responseXml.LoadXml(responseString);
            this.xmlStudy = responseString;

            String status = responseXml.GetElementsByTagName("Status")[0].InnerText;
            String xmlString = responseXml.GetElementsByTagName("XML")[0].InnerText;

            if (status.Equals("OK"))
            {
                QreportLogRepositorio.updateEstado(LogID, "OK");
                
                //Metemos las URL en cdata. Son unos chapuzas. El xml que devuelven no valida.
                xmlString = xmlString.Replace("<url_visor_web>", "<url_visor_web><![CDATA[");
                xmlString = xmlString.Replace("</url_visor_web>", "]]></url_visor_web>");
                xmlString = xmlString.Replace("<url_visor_clinico>", "<url_visor_clinico><![CDATA[");
                xmlString = xmlString.Replace("</url_visor_clinico>", "]]></url_visor_clinico>");
                xmlString = xmlString.Replace("<url_zip>", "<url_zip><![CDATA[");
                xmlString = xmlString.Replace("</url_zip>", "]]></url_zip>");

                try
                {
                    XmlDocument responseXml2 = new XmlDocument();
                    responseXml2.LoadXml(xmlString.Replace("&", "&amp;"));
                    Int32 estudiosEncontrados = 0;
                    String sEstudiosEncontrados = responseXml2.GetElementsByTagName("estudios_encontrados")[0].InnerText;
                    estudiosEncontrados = Convert.ToInt32(sEstudiosEncontrados);
                    if (estudiosEncontrados > 0)
                    {
                        studyInstanceUID = responseXml2.GetElementsByTagName("studyInstanceUID")[0].InnerText;

                        this.visorUrlDiagnostico= responseXml2.GetElementsByTagName("url_visor_diagnostico")[0].InnerText;
                        this.visorUrlWeb= responseXml2.GetElementsByTagName("url_visor_web")[0].InnerText;
                        this.visorUrlZip= responseXml2.GetElementsByTagName("url_zip")[0].InnerText;
                    }
                }
                catch (Exception ex)
                {

                    throw;
                }
                

               
            }
            else {
                QreportLogRepositorio.updateEstado(LogID, "KO");
            }

            return studyInstanceUID;
        }

        public Boolean sendReport(String patientId, String accNumber, String studyInstanceUID, String rutaPdf) {

            Models.QREPORT_CONF qc = QreportConfigRepositorio.Obtener();

            String nombre = "";
            String fileb64 = "";
            //Recuperar el informe
            try
            {
                nombre = Path.GetFileName(rutaPdf);
                byte[] bytes = System.IO.File.ReadAllBytes(rutaPdf);
                fileb64 = Convert.ToBase64String(bytes);
            }
            catch(Exception e) 
            {
                return false;
            }

            //Mensaje
            StringBuilder messageXml = new StringBuilder("");
            messageXml.AppendLine("<soapenv:Envelope xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" xmlns:soapenv=\"http://schemas.xmlsoap.org/soap/envelope/\">");
            messageXml.AppendLine("<soapenv:Header/>");
            messageXml.AppendLine("<soapenv:Body>");
            messageXml.AppendLine("<AltaInforme soapenv:encodingStyle=\"http://schemas.xmlsoap.org/soap/encoding/\">");
            messageXml.AppendLine("<part xsi:type=\"api:AltaInforme\" xmlns:api=\"https://www.qualitasreport.com/ws/api_externa.php\">");
            messageXml.AppendLine("<PublicKey xsi:type=\"xsd:string\">"+qc.PUBLICKEY_WS+"</PublicKey>");
            messageXml.AppendLine("<Token xsi:type=\"xsd:string\">"+qc.TOKEN+"</Token>");
            messageXml.AppendLine("<CentroID xsi:type=\"xsd:string\">"+qc.CENTRO+"</CentroID>");
            messageXml.AppendLine("<StudyInstanceUID xsi:type=\"xsd:string\">"+studyInstanceUID+"</StudyInstanceUID>");
            messageXml.AppendLine("<PatientID xsi:type=\"xsd:string\">"+patientId+"</PatientID>");
            messageXml.AppendLine("<AccNumber xsi:type=\"xsd:string\">"+accNumber+"</AccNumber>");
            messageXml.AppendLine("<NombrePDF xsi:type=\"xsd:string\">"+nombre+"</NombrePDF>");
            messageXml.AppendLine("<ContenidoB64 xsi:type=\"xsd:string\">"+fileb64+"</ContenidoB64>");
            messageXml.AppendLine("</part>");
            messageXml.AppendLine("</AltaInforme>");
            messageXml.AppendLine("</soapenv:Body>");
            messageXml.AppendLine("</soapenv:Envelope>");

            Int32 LogID = QreportLogRepositorio.Insertar("AltaInforme", messageXml.ToString());

            String responseString = this.SendSoapMessage(messageXml.ToString(), qc.URL);
            QreportLogRepositorio.updateRespuesta(LogID, responseString);

            XmlDocument responseXml = new XmlDocument();
            responseXml.LoadXml(responseString);

            String status = responseXml.GetElementsByTagName("Status")[0].InnerText;

            if (status.Equals("OK")) {
                QreportLogRepositorio.updateEstado(LogID, "OK");
                return true;
            }           
            else{
                QreportLogRepositorio.updateEstado(LogID, "KO");
                return false;
            }

        }

        /// <summary>
        /// Execute a Soap WebService call
        /// </summary>
        /// <returns>Returns the String XML response</returns>
        public String SendSoapMessage(String message, String url)
        {
            string soapResult = "";
            System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12;
            HttpWebRequest request = CreateWebRequest(url);
            XmlDocument soapEnvelopeXml = new XmlDocument();
            soapEnvelopeXml.LoadXml(message);

            using (Stream stream = request.GetRequestStream())
            {
                soapEnvelopeXml.Save(stream);
            }

            using (WebResponse response = request.GetResponse())
            {
                using (StreamReader rd = new StreamReader(response.GetResponseStream()))
                {
                    soapResult = rd.ReadToEnd();
                    //Console.WriteLine(soapResult);
                }
            }

            return soapResult;
        }

        public string getVisor(int tipo)
        {

            String url = "";

            //Tratamos el XML
            XmlDocument responseXml = new XmlDocument();
            
            responseXml.LoadXml(EscapeXMLValue(this.xmlStudy));

            String status = responseXml.GetElementsByTagName("Status")[0].InnerText;
            String xmlString = responseXml.GetElementsByTagName("XML")[0].InnerText;

            if (status.Equals("OK"))
            {

                xmlString = xmlString.Replace("<url_visor_web>", "<url_visor_web><![CDATA[");
                xmlString = xmlString.Replace("</url_visor_web>", "]]></url_visor_web>");
                xmlString = xmlString.Replace("<url_visor_clinico>", "<url_visor_clinico><![CDATA[");
                xmlString = xmlString.Replace("</url_visor_clinico>", "]]></url_visor_clinico>");
                xmlString = xmlString.Replace("<url_zip>", "<url_zip><![CDATA[");
                xmlString = xmlString.Replace("</url_zip>", "]]></url_zip>");

                XmlDocument responseXml2 = new XmlDocument();
                responseXml2.LoadXml(xmlString.Replace("&", "&amp;"));

                switch (tipo)
                {
                    case 1:
                        url = responseXml2.GetElementsByTagName("url_visor_web")[0].InnerText;
                        break;
                    case 2:
                        url = responseXml2.GetElementsByTagName("url_visor_clinico")[0].InnerText;
                        break;
                    case 3:
                        url = responseXml2.GetElementsByTagName("url_zip")[0].InnerText;
                        break;
                }

                return url;
            }
            else
            {
                return url;
            }
        }


        /// <summary>
        /// Create a soap webrequest to [Url]
        /// </summary>
        /// <returns></returns>
        public static HttpWebRequest CreateWebRequest(String url)
        {
            System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12;

            HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(@""+url+"");
            webRequest.Headers.Add(@"SOAP:Action");
            webRequest.ContentType = "text/xml;charset=\"utf-8\"";
            webRequest.Accept = "text/xml";
            webRequest.Method = "POST";
            return webRequest;
        }

    }
}