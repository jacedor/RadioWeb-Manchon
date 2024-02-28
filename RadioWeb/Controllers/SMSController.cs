using RadioWeb.Models.Repos;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Security;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace RadioWeb.Controllers
{
    public class SMSController : Controller
    {

        public void HttpEnviarSMS(string texto, string Id_Mensaje, string Telefono_Destino)
        {
            ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(delegate { return true; });
            Dictionary<string, string> postParameters = new Dictionary<string, string>();

            WebConfigRepositorio oConfig = new WebConfigRepositorio();
            postParameters.Add("Usuario", oConfig.ObtenerValor("UserSMS"));
            postParameters.Add("Password", oConfig.ObtenerValor("PasswordSMS"));
            postParameters.Add("Id_Mensaje", Id_Mensaje);
            postParameters.Add("Telefono_Destino", Telefono_Destino);
            postParameters.Add("Texto",Utils.Varios.QuitAccents( texto));

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
        //
        // GET: /SMS/

        public void Enviar(string phone, string texto, string idMensaje)
        {
            HttpResponseMessage response = new HttpResponseMessage();

            HttpEnviarSMSGet(texto, idMensaje, phone);

           
        }

    }
}
