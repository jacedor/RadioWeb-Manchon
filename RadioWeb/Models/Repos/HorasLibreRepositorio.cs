using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using FirebirdSql.Data.FirebirdClient;
using RadioWeb.Models;
using RadioWeb.Utils;
using System.Xml;
using ADPM.Common;

namespace RadioWeb.Models.Repos
{
    //Repositorio que encapsula las llamadas al stored procedure realizado por massana directamente sobre la base de datos
    //llamado HuecosLibres
    public class HorasLibreRepositorio
    {
        public static List<HUECO> Lista(string EXPLORACION, string FECHA, string HORA, bool CLAUSTRO, bool COLOPERADA, ref string queryHorario, int oidMutua = 0)
        {
            string ClaustrofobiaText = (CLAUSTRO ? "T" : "F");
            string ColOperadaText = (COLOPERADA ? "T" : "F");

            List<HUECO> lHorasHorario = new List<HUECO>();

            string query = "SELECT * FROM HUECOSLIBRES(" + EXPLORACION + ",1," + oidMutua + ", "
                + DateTime.Parse(FECHA).ToString("MM/dd/yyyy").QuotedString() + "," + HORA.QuotedString() + ","
                + ClaustrofobiaText.QuotedString() + "," + ColOperadaText.QuotedString() + ")";

            FbConnection oConexion = new FbConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ConexionBD"].ConnectionString);
            oConexion.Open();
            FbCommand oCommand = new FbCommand(query, oConexion);

            try
            {
                FbDataReader oReaderHoras = oCommand.ExecuteReader();
                while (oReaderHoras.Read())
                {
                    lHorasHorario.Add(new HUECO
                    {
                        IDLISTA = "0",
                        IDHUECO = "1",
                        FIL_EXPLORACION = EXPLORACION.Substring(3),
                        CODIGOAPARATO = DataBase.GetIntFromReader(oReaderHoras, "AAPARATO").ToString(),
                        CENTRO = "CEDIV",
                        FECHA = DataBase.GetStringFromReader(oReaderHoras, "FECHAF"),
                        HORA = DataBase.GetStringFromReader(oReaderHoras, "HORA")
                    });
                }



                queryHorario = query;
                oConexion.Close();
            }
            catch (Exception EX)
            {
                //CARGAMOS VALORES DE CONFIGURACION DEL SERVER DE CORREO
                XmlDocument doc = new XmlDocument();
                doc.Load(System.Web.HttpContext.Current.Server.MapPath("/XML/Citaonline.xml"));
                string email = doc.GetElementsByTagName("email")[0].InnerText;
                string emailServer = doc.GetElementsByTagName("smtpServer")[0].InnerText;
                int emailServerPort = int.Parse(doc.GetElementsByTagName("smtpPort")[0].InnerText);
                string emailServerUser = doc.GetElementsByTagName("smtpUser")[0].InnerText;
                string emailServerPass = doc.GetElementsByTagName("smtpPass")[0].InnerText;

                //Utils.Varios.EnviarMail("Consulta SQL =" + queryHorario,
                //    new List<string>() { "acedo@adpm.es", "massana@adpm.es" },
                //    emailServer,
                //    emailServerPort,
                //    emailServerUser, emailServerPass, email, "Error Citaonline");


            }
            finally
            {
                if (oConexion.State == System.Data.ConnectionState.Open)

                {
                    oConexion.Close();
                    if (oCommand != null)
                    {
                        oCommand.Dispose();
                    }
                }
            }


            return lHorasHorario;
        }

        //LLAMADA DESDE CITA ONLINE
        public static List<HUECO> Lista(string CENTRO, string EXPLORACION, string FECHA, string HORA, bool CLAUSTRO,bool COLOPERADA, ref string queryHorario)
        {
            string ClaustrofobiaText = (CLAUSTRO? "T":"F");
            string ColOperadaText = (COLOPERADA ? "T" : "F");
            
            if (CENTRO == "TIBIDABO")
            {
                CENTRO = "CDPI";
            }

            List<HUECO> lHorasHorario = new List<HUECO>();

            string query = "SELECT * FROM HUECOSLIBRES(" + EXPLORACION.QuotedString() + "," + CENTRO.QuotedString() +
               "," + DateTime.Parse(FECHA).ToString("MM/dd/yyyy").QuotedString() + "," + HORA.QuotedString() + "," + ClaustrofobiaText.QuotedString() + "," + ColOperadaText.QuotedString() + ")";

            FbConnection oConexion = new FbConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ConexionBD"].ConnectionString);
            oConexion.Open();
            FbCommand oCommand = new FbCommand(query, oConexion);

            try
            {
                FbDataReader oReaderHoras = oCommand.ExecuteReader();
                while (oReaderHoras.Read())
                {
                    lHorasHorario.Add(new HUECO
                    {
                        IDLISTA = "0",
                        IDHUECO = "1",
                        FIL_EXPLORACION = EXPLORACION.Substring(3),
                        CODIGOAPARATO = DataBase.GetIntFromReader(oReaderHoras, "AAPARATO").ToString(),
                        CENTRO = CENTRO,
                        FECHA = DataBase.GetStringFromReader(oReaderHoras, "FECHAF"),
                        HORA = DataBase.GetStringFromReader(oReaderHoras, "HORA")
                    });
                }



                queryHorario = query;
                oConexion.Close();
            }
            catch (Exception EX)
            {
                //CARGAMOS VALORES DE CONFIGURACION DEL SERVER DE CORREO
                XmlDocument doc = new XmlDocument();
                doc.Load(System.Web.HttpContext.Current.Server.MapPath("/XML/Citaonline.xml"));
                string email = doc.GetElementsByTagName("email")[0].InnerText;
                string emailServer = doc.GetElementsByTagName("smtpServer")[0].InnerText;
                int emailServerPort = int.Parse(doc.GetElementsByTagName("smtpPort")[0].InnerText);
                string emailServerUser = doc.GetElementsByTagName("smtpUser")[0].InnerText;
                string emailServerPass = doc.GetElementsByTagName("smtpPass")[0].InnerText;

                Utils.Varios.EnviarMail("Consulta SQL =" + queryHorario,
                    new List<string>() { "acedo@adpm.es", "massana@adpm.es" },
                    emailServer,
                    emailServerPort,
                    emailServerUser, emailServerPass, email, true, "Error Citaonline");


            }
            finally
            {
                if (oConexion.State == System.Data.ConnectionState.Open)

                {
                    oConexion.Close();
                    if (oCommand != null)
                    {
                        oCommand.Dispose();
                    }
                }
            }
        

            return lHorasHorario;
        }

        public static List<HUECO> ListaDesdePeticiones(string CENTRO, string EXPLORACION, string FECHA, string HORA, bool CLAUSTRO, bool COLOPERADA,int oidMutua, ref string queryHorario)
        {
            string ClaustrofobiaText = (CLAUSTRO ? "T" : "F");
            string ColOperadaText = (COLOPERADA ? "T" : "F");

            if (CENTRO == "TIBIDABO")
            {
                CENTRO = "CDPI";
            }

            List<HUECO> lHorasHorario = new List<HUECO>();

            string query = "SELECT * FROM HUECOSLIBRESPETIS(" + EXPLORACION.QuotedString() + "," + CENTRO.QuotedString() + "," +  oidMutua +
               "," + DateTime.Parse(FECHA).ToString("MM/dd/yyyy").QuotedString() + "," + HORA.QuotedString() + "," + ClaustrofobiaText.QuotedString() + "," + ColOperadaText.QuotedString() + ")";

            FbConnection oConexion = new FbConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ConexionBD"].ConnectionString);
            oConexion.Open();
            FbCommand oCommand = new FbCommand(query, oConexion);

            try
            {
                FbDataReader oReaderHoras = oCommand.ExecuteReader();
                while (oReaderHoras.Read())
                {
                    lHorasHorario.Add(new HUECO
                    {
                        IDLISTA = "0",
                        IDHUECO = "1",
                        FIL_EXPLORACION = EXPLORACION.Substring(3),
                        CODIGOAPARATO = DataBase.GetIntFromReader(oReaderHoras, "AAPARATO").ToString(),
                        CENTRO = CENTRO,
                        FECHA = DataBase.GetStringFromReader(oReaderHoras, "FECHAF"),
                        HORA = DataBase.GetStringFromReader(oReaderHoras, "HORA")
                    });
                }



                queryHorario = query;
                oConexion.Close();
            }
            catch (Exception EX)
            {
                //CARGAMOS VALORES DE CONFIGURACION DEL SERVER DE CORREO
                XmlDocument doc = new XmlDocument();
                doc.Load(System.Web.HttpContext.Current.Server.MapPath("/XML/Citaonline.xml"));
                string email = doc.GetElementsByTagName("email")[0].InnerText;
                string emailServer = doc.GetElementsByTagName("smtpServer")[0].InnerText;
                int emailServerPort = int.Parse(doc.GetElementsByTagName("smtpPort")[0].InnerText);
                string emailServerUser = doc.GetElementsByTagName("smtpUser")[0].InnerText;
                string emailServerPass = doc.GetElementsByTagName("smtpPass")[0].InnerText;

                Utils.Varios.EnviarMail("Consulta SQL =" + queryHorario,
                    new List<string>() { "acedo@adpm.es", "massana@adpm.es" },
                    emailServer,
                    emailServerPort,
                    emailServerUser, emailServerPass, email, true, "Error Citaonline");


            }
            finally
            {
                if (oConexion.State == System.Data.ConnectionState.Open)

                {
                    oConexion.Close();
                    if (oCommand != null)
                    {
                        oCommand.Dispose();
                    }
                }
            }


            return lHorasHorario;
        }


    }
}