using OfficeOpenXml.FormulaParsing.Excel.Functions.Text;
using RadioWeb.Models.Repos;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mail;

namespace RadioWeb.Utils
{

   
    public class JsonResponseModel
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public string Data { get; set; }
    }
    public class Varios
    {
        public static string ObtenerCarpetaImagen()
        {
            WebConfigRepositorio oConfig = new WebConfigRepositorio();
            string RutaImagenes = oConfig.ObtenerValor("RutaWebCam");

            string trimestre = DateTime.Now.Year.ToString();

            switch (DateTime.Now.Month)
            {
                case 1:
                case 2:
                case 3:
                    trimestre += "T1";
                    break;
                case 4:
                case 5:
                case 6:
                    trimestre += "T2";
                    break;
                case 7:
                case 8:
                case 9:
                    trimestre += "T3";
                    break;
                case 10:
                case 11:
                case 12:
                    trimestre += "T4";
                    break;


            }
            if (!Directory.Exists(RutaImagenes + @"\" + trimestre + @"\"))
            {
                Directory.CreateDirectory(RutaImagenes + @"\" + trimestre + @"\");
            }
            return RutaImagenes + @"\" + trimestre + @"\";

        }

        public static string ObtenerCarpetaDocumentosEscaneados()
        {
            WebConfigRepositorio oConfig = new WebConfigRepositorio();
            string RutaImagenes =oConfig.ObtenerValor("RutaDocumentosEscaneados");

            string trimestre = DateTime.Now.Year.ToString();

            switch (DateTime.Now.Month)
            {
                case 1:
                case 2:
                case 3:
                    trimestre += "T1";
                    break;
                case 4:
                case 5:
                case 6:
                    trimestre += "T2";
                    break;
                case 7:
                case 8:
                case 9:
                    trimestre += "T3";
                    break;
                case 10:
                case 11:
                case 12:
                    trimestre += "T4";
                    break;


            }
            if (!Directory.Exists(RutaImagenes + @"\" + trimestre + @"\"))
            {
                Directory.CreateDirectory(RutaImagenes + @"\" + trimestre + @"\");
            }
            return RutaImagenes + @"\" + trimestre + @"\";

        }

        public static string QuitAccents(string inputString)
        {
            Regex a = new Regex("[á|à|ä|â]", RegexOptions.Compiled);
            Regex e = new Regex("[é|è|ë|ê]", RegexOptions.Compiled);
            Regex i = new Regex("[í|ì|ï|î]", RegexOptions.Compiled);
            Regex o = new Regex("[ó|ò|ö|ô]", RegexOptions.Compiled);
            Regex u = new Regex("[ú|ù|ü|û]", RegexOptions.Compiled);
            Regex n = new Regex("[ñ|Ñ]", RegexOptions.Compiled);
            inputString = a.Replace(inputString, "a");
            inputString = e.Replace(inputString, "e");
            inputString = i.Replace(inputString, "i");
            inputString = o.Replace(inputString, "o");
            inputString = u.Replace(inputString, "u");
            inputString = n.Replace(inputString, "n");
            return inputString;
        }


        public static void EnviarMail(string bodyMail,
          List<string> Destinatarios,
          string smtpServer, int smtpPort, string user, string password, string mail, bool usarSsl,
          string subject = "Confirmación cita", string adjunto = "")
        {
            try
            {
                SmtpClient server = new SmtpClient(smtpServer, smtpPort);
               
                server.Credentials = new System.Net.NetworkCredential(user, password);
                server.EnableSsl = usarSsl;
                server.DeliveryMethod = SmtpDeliveryMethod.Network;
                System.Net.Mail.MailMessage oMail = new System.Net.Mail.MailMessage();
                ServicePointManager.Expect100Continue = true;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                Guid objGuid = new Guid();
                objGuid = Guid.NewGuid();
                String MessageID = "<" + objGuid.ToString() + ">";

                oMail.Headers.Add("Message-Id",
                         String.Format("<{0}@{1}>",
                         Guid.NewGuid().ToString(),
                       "no-reply@grupmanchon.com"));
               
                oMail.IsBodyHtml = true;
                oMail.From = new MailAddress(mail, "Grup Manchon");
                //Enviamos en copia oculta mail a todos menos al paciente que es la ultima posicion del array
                for (int i = 0; i < Destinatarios.Count - 1; i++)
                {
                    oMail.Bcc.Add(new MailAddress(Destinatarios[i]));
                }
                //Enviamos email al paciente
                oMail.To.Add(new MailAddress(Destinatarios[Destinatarios.Count - 1]));


                oMail.Subject = subject;
                if (adjunto.Length > 0)
                {
                    oMail.Attachments.Add(new Attachment(adjunto));
                }

                oMail.Body = bodyMail;

                server.Send(oMail);
                oMail.Dispose();

            }
            catch (Exception ex)
            {

                System.Threading.Thread.Sleep(1);

            }


        }
        //public static void EnviarMail(string bodyMail, List<string> Destinatarios, 
        //    string smtpServer, int smtpPort, string user, string password, string mail, 
        //    string subject = "Confirmación cita", string adjunto = "",bool usarSsl=false)
        //{
        //    MailMessage oMail = new MailMessage();
        //    try
        //    {
        //        SmtpClient server = new SmtpClient(smtpServer, smtpPort);
        //        server.Credentials = new System.Net.NetworkCredential(user, password);
        //        server.EnableSsl = usarSsl;
        //        server.UseDefaultCredentials = true;


        //        oMail.IsBodyHtml = true;
        //        oMail.From = new MailAddress(mail,mail);
        //        //Enviamos en copia oculta mail a todos menos al paciente que es la ultima posicion del array
        //        for (int i = 0; i < Destinatarios.Count - 1; i++)
        //        {
        //            oMail.Bcc.Add(new MailAddress(Destinatarios[i]));
        //        }
        //        //Enviamos email al paciente
        //        oMail.To.Add(new MailAddress(Destinatarios[Destinatarios.Count - 1]));


        //        oMail.Subject = subject;
        //        if (adjunto.Length > 0)
        //        {
        //            oMail.Attachments.Add(new Attachment(adjunto));
        //        }

        //        oMail.Body = bodyMail;

        //        server.Send(oMail);

        //    }
        //    catch (Exception ex)
        //    {



        //    }
        //    finally
        //    {
        //        oMail.Dispose();

        //    }


        //}
    }
}