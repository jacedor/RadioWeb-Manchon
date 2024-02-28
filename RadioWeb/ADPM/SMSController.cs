using RadioWeb.Models.Repos;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Security;
using System.Text;
using System.Web.Http;
using System.Xml;

namespace RadioWeb.ADPM
{
    public class SMSController : ApiController
    {
        public static string RutaSMS = System.Configuration.ConfigurationManager.AppSettings["RutaSMS"];

        // GET api/cocentros/5
        public HttpResponseMessage Post(string rutaFichero, int Archivo = 0)
        {
            var response = new HttpResponseMessage();
       
            try
            {
                //Si es igual a 0 se trata de envios de recordatorios de cita
                if (!rutaFichero.ToUpper().StartsWith("CONTEXTO"))
                {
                    //verificamos que massana haya dejado el fichero que dice. Si no es asi borramos el temporal
                    if (File.Exists(RutaSMS + @"\" + rutaFichero))
                    {

                        string TextoDelFichero = System.IO.File.ReadAllText(RutaSMS + @"\" + rutaFichero);                      
                           
                            //llegados a este punto ya tenemos el fichero temporal con toda la información requerida
                            int contador = 0;
                            string line;
                            System.IO.StreamReader file = new System.IO.StreamReader(RutaSMS + @"\" + rutaFichero);
                            while ((line = file.ReadLine()) != null)
                            {

                                String[] valores = line.Split('\t');
                                if (valores.Length > 1)
                                {
                                    String texto = valores[0].ToString() + "#"
                                                       + valores[1].ToString() + "#"
                                                       + valores[2].ToString() + "#"
                                                       + valores[3].ToString() + "#"
                                                       + valores[4].ToString();

                                    contador++;
                                    HttpEnviarArchivoGet(texto, "0");

                                }
                               

                            }
                            HttpEnviarArchivoGet("", "1");
                            file.Close();                
                       
                       
                        response = Request.CreateResponse(HttpStatusCode.OK, "Fichero Procesado");

                    }
                    else
                    {
                        response = Request.CreateResponse(HttpStatusCode.NotAcceptable, "Se ha producido un error");
                       
                    }

                }
                else
                {

                    string line;
                    System.IO.StreamReader file = new System.IO.StreamReader(RutaSMS + @"\" + rutaFichero);
                    while ((line = file.ReadLine()) != null)
                    {
                         String[] valores = line.Split('&');
                        if (valores.Length > 2)
                        {
                            string idMensaje = valores[0].ToString();
                            string TelefonoDestino = valores[1].ToString();
                            string texto = valores[2].ToString();
                            HttpEnviarSMSGet(texto,idMensaje,TelefonoDestino);
                            
                            response = Request.CreateResponse(HttpStatusCode.OK, "Fichero Procesado");

                        }
                        
                    }

                    file.Close();
                }
                    
            }
            catch (Exception)
            {
                response = Request.CreateResponse(HttpStatusCode.NotAcceptable, "Se ha producido un error");             

                return response;
            }
            finally
            {

            }



            return response;
        }



        public void HttpGet(string URI, string parametros)
        {

            try
            {
                WebRequest myWebRequest = WebRequest.Create(URI + parametros);

                // Set the 'Timeout' property in Milliseconds.
                myWebRequest.Timeout = 10000;
                // This request will throw a WebException if it reaches the timeout limit before it is able to fetch the resource.
                WebResponse myWebResponse = myWebRequest.GetResponse();
                myWebResponse.Close();
            }
            catch (WebException e)
            {


            }

        }
       
        
        [HttpPost]
        public void HttpEnviarMail(string destinatario, string cuerpoMail )
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(System.Web.HttpContext.Current.Server.MapPath("/XML/Citaonline.xml"));
            string email = doc.GetElementsByTagName("email")[0].InnerText;
            string emailServer = doc.GetElementsByTagName("smtpServer")[0].InnerText;
            int emailServerPort = int.Parse(doc.GetElementsByTagName("smtpPort")[0].InnerText);
            string emailServerUser = doc.GetElementsByTagName("smtpUser")[0].InnerText;
            string emailServerPass = doc.GetElementsByTagName("smtpPass")[0].InnerText;


            List<string> ldestinatario = new List<string>();
            ldestinatario.Add(destinatario);
            Utils.Varios.EnviarMail(cuerpoMail,ldestinatario, emailServer, emailServerPort, emailServerUser, emailServerPass, email, true, "Confirmación cita", "");       
        }

        public void HttpEnviarSMS(string texto, string Id_Mensaje, string Telefono_Destino)
        {
            ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(delegate { return true; });
            Dictionary<string, string> postParameters = new Dictionary<string, string>();
 WebConfigRepositorio oConfig = new WebConfigRepositorio();
            postParameters.Add("Usuario", oConfig.ObtenerValor("UserSMS"));
            postParameters.Add("Password", oConfig.ObtenerValor("PasswordSMS"));
            postParameters.Add("Id_Mensaje", Id_Mensaje);
            postParameters.Add("Telefono_Destino", Telefono_Destino);            
            postParameters.Add("Texto", texto);           

            string postData = "";

            foreach (string key in postParameters.Keys)
            {
                postData += WebUtility.UrlEncode(key) + "=" + WebUtility.UrlEncode(postParameters[key]) + "&";
            }
           
            string endPoint = oConfig.ObtenerValor("ENDPOINTSMS");
            HttpWebRequest myHttpWebRequest = (HttpWebRequest)HttpWebRequest.Create(endPoint);

            myHttpWebRequest.Method = "POST";

            byte[] data = Encoding.ASCII.GetBytes(postData);

            myHttpWebRequest.ContentType = "application/x-www-form-urlencoded";
            myHttpWebRequest.ContentLength = data.Length;
            Stream requestStream = myHttpWebRequest.GetRequestStream();
            requestStream.Write(data, 0, data.Length);
            requestStream.Close();

            HttpWebResponse myHttpWebResponse = (HttpWebResponse)myHttpWebRequest.GetResponse();

            Stream responseStream = myHttpWebResponse.GetResponseStream();
            StreamReader myStreamReader = new StreamReader(responseStream, Encoding.Default);

            string pageContent = myStreamReader.ReadToEnd();

            myStreamReader.Close();
            responseStream.Close();

            myHttpWebResponse.Close();
        }


        public void HttpEnviarArchivo(string texto, string si_fin = "0")
        {
            ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(delegate { return true; });
            Dictionary<string, string> postParameters = new Dictionary<string, string>();

            WebConfigRepositorio oConfig = new WebConfigRepositorio();
            postParameters.Add("Usuario", oConfig.ObtenerValor("UserSMS"));
            postParameters.Add("Password", oConfig.ObtenerValor("PasswordSMS"));
            postParameters.Add("Texto", texto);
            postParameters.Add("si_Fin", si_fin);

            string postData = "";

            foreach (string key in postParameters.Keys)
            {
                postData += WebUtility.UrlEncode(key) + "=" + WebUtility.UrlEncode(postParameters[key]) + "&";
            }

            string endPoint = oConfig.ObtenerValor("ENDPOINTARCHIVO");
            HttpWebRequest myHttpWebRequest = (HttpWebRequest)HttpWebRequest.Create(endPoint);

            myHttpWebRequest.Method = "POST";

            byte[] data = Encoding.ASCII.GetBytes(postData);

            myHttpWebRequest.ContentType = "application/x-www-form-urlencoded";
            myHttpWebRequest.ContentLength = data.Length;

            Stream requestStream = myHttpWebRequest.GetRequestStream();
            requestStream.Write(data, 0, data.Length);
            requestStream.Close();

            HttpWebResponse myHttpWebResponse = (HttpWebResponse)myHttpWebRequest.GetResponse();

            Stream responseStream = myHttpWebResponse.GetResponseStream();
            StreamReader myStreamReader = new StreamReader(responseStream, Encoding.Default);

            string pageContent = myStreamReader.ReadToEnd();

            myStreamReader.Close();
            responseStream.Close();

            myHttpWebResponse.Close();
        }

        public void HttpEnviarArchivoGet(string texto, string si_fin = "0")
        {
            ServicePointManager.SecurityProtocol = (SecurityProtocolType)3072;

            ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(delegate { return true; });
            Dictionary<string, string> postParameters = new Dictionary<string, string>();
            WebConfigRepositorio oConfig = new WebConfigRepositorio();
            string endPoint = oConfig.ObtenerValor("ENDPOINTARCHIVO") + "?";

            postParameters.Add("Usuario", oConfig.ObtenerValor("UserSMS"));
            postParameters.Add("Password", oConfig.ObtenerValor("PasswordSMS"));
            postParameters.Add("Texto", texto);
            postParameters.Add("si_Fin", si_fin);

            string postData = "";

            foreach (string key in postParameters.Keys)
            {
                postData += WebUtility.UrlEncode(key) + "=" + WebUtility.UrlEncode(postParameters[key]) + "&";
            }

            WebRequest myWebRequest = WebRequest.Create(endPoint + postData);

            // Set the 'Timeout' property in Milliseconds.
            myWebRequest.Timeout = 10000;
            // This request will throw a WebException if it reaches the timeout limit before it is able to fetch the resource.
            WebResponse myWebResponse = myWebRequest.GetResponse();
            myWebResponse.Close();
        }

        public string Get(string phone)
        {
            var rng = new Random();
            int first = rng.Next(10);
            int second = rng.Next(10);
            int third = rng.Next(10);
            int fourth = rng.Next(10);

            string randomUniqueNumbers = first.ToString() + second.ToString() + third.ToString() + fourth.ToString();


            HttpEnviarSMSGet("Para proseguir con su reserva de cita introduzca el siguiente codigo: " + randomUniqueNumbers, randomUniqueNumbers, phone);
            //String parametros = "ia_login=" +
            // "sms1@dr-manchon.com" + "&ia_password="
            //+ System.Web.HttpUtility.UrlEncode("ma21nc") + "&ia_phone1="
            //+ System.Web.HttpUtility.UrlEncode(phone) + "&ia_text1=Para proseguir con su reserva de cita introduzca el siguiente codigo: "
            //+ System.Web.HttpUtility.UrlEncode(randomUniqueNumbers);

            //HttpGet("http://www.infoavisos.com/sms-http/http.exe?", parametros);
            return (randomUniqueNumbers.ToString());
        }

        public void HttpEnviarSMSGet(string texto, string Id_Mensaje, string Telefono_Destino)
        {
            try
            {
                ServicePointManager.SecurityProtocol = (SecurityProtocolType)3072;

                ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(delegate { return true; });
                Dictionary<string, string> postParameters = new Dictionary<string, string>();
                WebConfigRepositorio oConfig = new WebConfigRepositorio();
                string endPoint = oConfig.ObtenerValor("ENDPOINTSMS") + "?";

                postParameters.Add("Usuario", oConfig.ObtenerValor("UserSMS"));
                postParameters.Add("Password", oConfig.ObtenerValor("PasswordSMS"));
                postParameters.Add("Id_Mensaje", Id_Mensaje);
                postParameters.Add("Telefono_Destino", Telefono_Destino);
                postParameters.Add("Texto", Utils.Varios.QuitAccents(texto));

                string postData = "";

                foreach (string key in postParameters.Keys)
                {
                    postData += WebUtility.UrlEncode(key) + "=" + WebUtility.UrlEncode(postParameters[key]) + "&";
                }

                WebRequest myWebRequest = WebRequest.Create(endPoint + postData);

                // Set the 'Timeout' property in Milliseconds.
                myWebRequest.Timeout = 10000;
                // This request will throw a WebException if it reaches the timeout limit before it is able to fetch the resource.
                WebResponse myWebResponse = myWebRequest.GetResponse();
                myWebResponse.Close();
            }
            catch (Exception)
            {
                throw;
            }
        }



    }
}
