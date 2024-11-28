using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Hosting;
using FirebirdSql.Data.FirebirdClient;
using RadioWeb.Models;
using RadioWeb.Utils;
using HtmlAgilityPack;
using ADPM.Common;
using System.Text.RegularExpressions;
using RadioWeb.Models.HTMLToPDF;
using System.Text;
using System.Collections.Specialized;
using iTextSharp.tool.xml.html;

namespace RadioWeb.Models.Repos
{

    public class InformesRepositorio
    {
        private static string ObtenerCarpetaGuardarPDF(string anyo, string mes)
        {
            WebConfigRepositorio oConfig = new WebConfigRepositorio();
            string rutaBase = oConfig.ObtenerValor("DIRINFORMES") + @"\INFORMES_";


            if (!System.IO.Directory.Exists(rutaBase))
            {
                System.IO.Directory.CreateDirectory(rutaBase);
            }

            string rutaBaseMes = rutaBase + anyo + "_" + mes + @"\";
            //Si no existe el directorio del año actual lo creamos

            //Si no existe el directorio del año actual lo creamos
            if (!System.IO.Directory.Exists(rutaBaseMes))
            {
                System.IO.Directory.CreateDirectory(rutaBaseMes);
            }

            return rutaBaseMes;
        }

        public static INFORMES Obtener(int oid)
        {

            FbConnection oConexion = new FbConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ConexionBD"].ConnectionString);
            oConexion.Open();
            INFORMES oInforme = new INFORMES();

            FbCommand oCommand = new FbCommand("select * from INFORMES where oid=" + oid, oConexion);
            FbDataReader oReader = oCommand.ExecuteReader();
            try
            {

                while (oReader.Read())
                {

                    oInforme.OID = DataBase.GetIntFromReader(oReader, "OID");
                    oInforme.VERS = DataBase.GetIntFromReader(oReader, "VERS");
                    oInforme.CID = DataBase.GetIntFromReader(oReader, "CID");
                    oInforme.CANAL = DataBase.GetStringFromReader(oReader, "CANAL");
                    oInforme.OWNER = DataBase.GetIntFromReader(oReader, "OWNER");
                    oInforme.USERNAME = DataBase.GetStringFromReader(oReader, "USERNAME");
                    oInforme.MODIF = DataBase.GetDateTimeFromReader(oReader, "MODIF");
                    oInforme.IOR_EMPRESA = DataBase.GetIntFromReader(oReader, "IOR_EMPRESA");
                    oInforme.BORRADO = DataBase.GetStringFromReader(oReader, "BORRADO");
                    oInforme.TITULO = DataBase.GetStringFromReader(oReader, "TITULO");
                    oInforme.FECHA = DataBase.GetDateTimeFromReader(oReader, "FECHA").Value;
                    oInforme.COD_PAC = DataBase.GetStringFromReader(oReader, "COD_PAC");
                    oInforme.ALFAS = DataBase.GetStringFromReader(oReader, "ALFAS");
                    oInforme.ALFAS2 = DataBase.GetStringFromReader(oReader, "ALFAS2");
                    oInforme.VISITA = DataBase.GetDoubleFromReader(oReader, "VISITA");
                    oInforme.FECHAREVISION = DataBase.GetDateTimeFromReader(oReader, "FECHAREVISION");
                    oInforme.HORAMOD = DataBase.GetStringFromReader(oReader, "HORAMOD");
                    oInforme.HORA = DataBase.GetStringFromReader(oReader, "HORA");
                    oInforme.IOR_PAC = DataBase.GetIntFromReader(oReader, "IOR_PAC");
                    oInforme.PACIENTE = PacienteRepositorio.Obtener((int)oInforme.IOR_PAC).PACIENTE1;
                    oInforme.IOR_MEDINFORME = DataBase.GetIntFromReader(oReader, "IOR_MEDINFORME");
                    oInforme.IOR_MEDREVISION = DataBase.GetIntFromReader(oReader, "IOR_MEDREVISION");
                    oInforme.PATOLOGICO = DataBase.GetStringFromReader(oReader, "PATOLOGICO");
                    oInforme.IOR_TECNICO = DataBase.GetIntFromReader(oReader, "IOR_TECNICO");
                    oInforme.IOR_MODALIDAD = DataBase.GetIntFromReader(oReader, "IOR_MODALIDAD");
                    oInforme.IOR_SITUACION = DataBase.GetIntFromReader(oReader, "IOR_SITUACION");
                    oInforme.IOR_TECNICA = DataBase.GetIntFromReader(oReader, "IOR_TECNICA");
                    oInforme.VALIDACION = DataBase.GetStringFromReader(oReader, "VALIDACION");
                    oInforme.TEXTOHTML = ObtenerHtmlDelInforme(DataBase.GetIntFromReader(oReader, "OID"));

                }
                return oInforme;
            }
            catch (Exception)
            {

                throw;
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



        }

        public static P_INFORMES ObtenerPlantilla(int oidPlantilla)
        {

            FbConnection oConexion = new FbConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ConexionBD"].ConnectionString);
            oConexion.Open();
            P_INFORMES oInforme = new P_INFORMES();

            FbCommand oCommand = new FbCommand("select * from TEXTOS where owner=" + oidPlantilla, oConexion);
            FbDataReader oReader = oCommand.ExecuteReader();

            try
            {
                while (oReader.Read())
                {

                    oInforme.OID = DataBase.GetIntFromReader(oReader, "OID");
                    oInforme.OWNER = DataBase.GetIntFromReader(oReader, "OWNER");
                    oInforme.USERNAME = DataBase.GetStringFromReader(oReader, "USERNAME");
                    oInforme.MODIF = DataBase.GetDateTimeFromReader(oReader, "MODIF");
                    oInforme.IOR_EMPRESA = DataBase.GetIntFromReader(oReader, "IOR_EMPRESA");

                    oInforme.TEXTOHTML = ObtenerHtmlDelPlantilla(oidPlantilla);

                }
                return oInforme;
            }
            catch (Exception)
            {

                throw;
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

        }

        public static List<INFORMES> ObtenerDeExploracion(int oidExploracion)
        {

            FbConnection oConexion = new FbConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ConexionBD"].ConnectionString);
            oConexion.Open();
            INFORMES oInforme = new INFORMES();

            FbCommand oCommand = new FbCommand("select * from INFORMES where owner=" + oidExploracion, oConexion);
            FbDataReader oReader = oCommand.ExecuteReader();
            try
            {
                List<INFORMES> lListaInformes = new List<INFORMES>();
                while (oReader.Read())
                {
                    oInforme = new INFORMES();
                    oInforme.OID = DataBase.GetIntFromReader(oReader, "OID");
                    oInforme.VERS = DataBase.GetIntFromReader(oReader, "VERS");
                    oInforme.CID = DataBase.GetIntFromReader(oReader, "CID");
                    oInforme.CANAL = DataBase.GetStringFromReader(oReader, "CANAL");
                    oInforme.OWNER = DataBase.GetIntFromReader(oReader, "OWNER");
                    oInforme.USERNAME = DataBase.GetStringFromReader(oReader, "USERNAME");
                    oInforme.MODIF = DataBase.GetDateTimeFromReader(oReader, "MODIF");
                    oInforme.IOR_EMPRESA = DataBase.GetIntFromReader(oReader, "IOR_EMPRESA");
                    oInforme.BORRADO = DataBase.GetStringFromReader(oReader, "BORRADO");
                    oInforme.TITULO = DataBase.GetStringFromReader(oReader, "TITULO");
                    oInforme.FECHA = DataBase.GetDateTimeFromReader(oReader, "FECHA").Value;

                    oInforme.COD_PAC = DataBase.GetStringFromReader(oReader, "COD_PAC");
                    oInforme.ALFAS = DataBase.GetStringFromReader(oReader, "ALFAS");
                    oInforme.ALFAS2 = DataBase.GetStringFromReader(oReader, "ALFAS2");
                    oInforme.VISITA = DataBase.GetDoubleFromReader(oReader, "VISITA");
                    oInforme.HORAMOD = DataBase.GetStringFromReader(oReader, "HORAMOD");
                    oInforme.HORA = DataBase.GetStringFromReader(oReader, "HORA");
                    oInforme.IOR_PAC = DataBase.GetIntFromReader(oReader, "IOR_PAC");
                    oInforme.PACIENTE = PacienteRepositorio.Obtener((int)oInforme.IOR_PAC).PACIENTE1;
                    oInforme.IOR_MEDINFORME = DataBase.GetIntFromReader(oReader, "IOR_MEDINFORME");
                    oInforme.IOR_MEDREVISION = DataBase.GetIntFromReader(oReader, "IOR_MEDREVISION");
                    oInforme.PATOLOGICO = DataBase.GetStringFromReader(oReader, "PATOLOGICO");
                    oInforme.IOR_TECNICO = DataBase.GetIntFromReader(oReader, "IOR_TECNICO");
                    oInforme.IOR_MODALIDAD = DataBase.GetIntFromReader(oReader, "IOR_MODALIDAD");
                    oInforme.IOR_SITUACION = DataBase.GetIntFromReader(oReader, "IOR_SITUACION");
                    oInforme.IOR_TECNICA = DataBase.GetIntFromReader(oReader, "IOR_TECNICA");
                    oInforme.VALIDACION = DataBase.GetStringFromReader(oReader, "VALIDACION");
                    oInforme.TEXTOHTML = ObtenerHtmlDelInforme(DataBase.GetIntFromReader(oReader, "OID"));
                    lListaInformes.Add(oInforme);
                }
                return lListaInformes;
            }
            catch (Exception)
            {

                throw;
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



        }

        public static INFORMES ObtenerDeExploracionValidado(int oidExploracion)
        {

            FbConnection oConexion = new FbConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ConexionBD"].ConnectionString);
            oConexion.Open();
            INFORMES oInforme = new INFORMES();

            FbCommand oCommand = new FbCommand("select * from INFORMES where owner=" + oidExploracion + " AND VALIDACION = 'T'", oConexion);
            FbDataReader oReader = oCommand.ExecuteReader();
            try
            {
                while (oReader.Read())
                {

                    oInforme.OID = DataBase.GetIntFromReader(oReader, "OID");
                    oInforme.VERS = DataBase.GetIntFromReader(oReader, "VERS");
                    oInforme.CID = DataBase.GetIntFromReader(oReader, "CID");
                    oInforme.CANAL = DataBase.GetStringFromReader(oReader, "CANAL");
                    oInforme.OWNER = DataBase.GetIntFromReader(oReader, "OWNER");
                    oInforme.USERNAME = DataBase.GetStringFromReader(oReader, "USERNAME");
                  //  oInforme.USERDESVALIDA = DataBase.GetStringFromReader(oReader, "USERDESVALIDA");
                  //  oInforme.FECHADESVALIDA = DataBase.GetDateTimeFromReader(oReader, "FECHADESVALIDA");
                    oInforme.MODIF = DataBase.GetDateTimeFromReader(oReader, "MODIF");
                    oInforme.IOR_EMPRESA = DataBase.GetIntFromReader(oReader, "IOR_EMPRESA");
                    oInforme.BORRADO = DataBase.GetStringFromReader(oReader, "BORRADO");
                    oInforme.TITULO = DataBase.GetStringFromReader(oReader, "TITULO");
                    oInforme.FECHA = DataBase.GetDateTimeFromReader(oReader, "FECHA").Value;

                    oInforme.COD_PAC = DataBase.GetStringFromReader(oReader, "COD_PAC");
                    oInforme.ALFAS = DataBase.GetStringFromReader(oReader, "ALFAS");
                    oInforme.ALFAS2 = DataBase.GetStringFromReader(oReader, "ALFAS2");
                    oInforme.VISITA = DataBase.GetDoubleFromReader(oReader, "VISITA");
                    oInforme.HORAMOD = DataBase.GetStringFromReader(oReader, "HORAMOD");
                    oInforme.HORA = DataBase.GetStringFromReader(oReader, "HORA");
                    oInforme.IOR_PAC = DataBase.GetIntFromReader(oReader, "IOR_PAC");
                    oInforme.PACIENTE = PacienteRepositorio.Obtener((int)oInforme.IOR_PAC).PACIENTE1;
                    oInforme.IOR_MEDINFORME = DataBase.GetIntFromReader(oReader, "IOR_MEDINFORME");
                    oInforme.IOR_MEDREVISION = DataBase.GetIntFromReader(oReader, "IOR_MEDREVISION");
                    oInforme.PATOLOGICO = DataBase.GetStringFromReader(oReader, "PATOLOGICO");
                    oInforme.IOR_TECNICO = DataBase.GetIntFromReader(oReader, "IOR_TECNICO");
                    oInforme.IOR_MODALIDAD = DataBase.GetIntFromReader(oReader, "IOR_MODALIDAD");
                    oInforme.IOR_SITUACION = DataBase.GetIntFromReader(oReader, "IOR_SITUACION");
                    oInforme.IOR_TECNICA = DataBase.GetIntFromReader(oReader, "IOR_TECNICA");
                    oInforme.VALIDACION = DataBase.GetStringFromReader(oReader, "VALIDACION");
                    oInforme.TEXTOHTML = ObtenerHtmlDelInforme(DataBase.GetIntFromReader(oReader, "OID"));
                }
                return oInforme;
            }
            catch (Exception)
            {
                throw;
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
        }

        private static string ObtenerHtmlDelPlantilla(int oidInforme)
        {
            FbConnection oConexion = new FbConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ConexionBD"].ConnectionString);


            string result = "";
            string queryInforme = "";

            queryInforme = "SELECT a.TEXTO";
            queryInforme += " FROM TEXTOS a where a.OWNER=" + oidInforme;
            oConexion.Open();

            FbCommand oCommand = new FbCommand(queryInforme, oConexion);
            FbDataReader oReader = oCommand.ExecuteReader();

            try
            {

                string rtfInforme = "";
                while (oReader.Read())
                {

                    rtfInforme = DataBase.GetStringFromReader(oReader, "TEXTO");
                    if (rtfInforme.StartsWith("{\\rtf1"))
                    {
                        result = DataBase.convertRtfToHtml(rtfInforme);
                        HtmlDocument oDocHtml = new HtmlDocument();

                        result = result.Replace("<p style=\"margin:0pt 0pt 0pt 0pt;line-height:normal;\"><span class=\"st1\">&nbsp;</span></p>", "");
                        result = result.Replace("The trial version of RTF-to-HTML DLL .Net can convert up to 10000 symbols.", "");
                        result = result.Replace("Get the full featured version!", "");
                        oDocHtml.LoadHtml(result);
                        result = oDocHtml.DocumentNode.SelectSingleNode("//body").InnerText;

                    }
                    else
                    {
                        result = rtfInforme;
                    }



                }
                if (result.IndexOf("<style") > 0)
                {


                    //return result.Substring(result.IndexOf("<STYLE"), result.ToUpper().IndexOf("</BODY>") - result.ToUpper().IndexOf("<STYLE"));
                    return result;
                }
                else
                {
                    if (result.IndexOf("<body>") > 0)
                    {
                        return result.Substring(result.IndexOf("<body>"), result.IndexOf("</body>") - result.IndexOf("<body>"));
                    }
                    //si entramos por este else quiere decir que es texto plano
                    else
                    {
                        return result;
                    }

                }



            }
            catch (Exception)
            {

                throw;
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
        }

        public static string ObtenerHtmlDelInforme(int oidInforme)
        {
            FbConnection oConexion = new FbConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ConexionBD"].ConnectionString);
            FbConnection oConexionTextos = new FbConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ConexionTextos"].ConnectionString);

            FbCommand oCommand = null;

            FbCommand oCommandTextos = null;

            oConexion.Open();


            try
            {
                string result = "";
                string queryInforme = "";
                FbDataReader oReader = null;
                if (System.Configuration.ConfigurationManager.AppSettings["NombreEmpresa"].ToString().Contains("Manchon"))
                {
                    if (oidInforme > 12000000)
                    {
                        queryInforme = "SELECT a.TEXTO";
                        queryInforme += " FROM TEXTOS a where a.OWNER=" + oidInforme;
                    }
                    else
                    {
                        queryInforme = "SELECT a.TEXTO";
                        queryInforme += " FROM TEXTOS2 a where a.OWNER=" + oidInforme;
                    }


                    if (oidInforme > 12000000)
                    {
                        oCommand = new FbCommand(queryInforme, oConexion);
                        oReader = oCommand.ExecuteReader();
                    }
                    else
                    {
                        oConexionTextos.Open();
                        oCommandTextos = new FbCommand(queryInforme, oConexionTextos);
                        oReader = oCommandTextos.ExecuteReader();
                    }
                }
                else
                {
                    queryInforme = "SELECT a.TEXTO";
                    queryInforme += " FROM TEXTOS a where a.OWNER=" + oidInforme;
                    oCommand = new FbCommand(queryInforme, oConexion);
                    oReader = oCommand.ExecuteReader();
                }

                string rtfInforme = "";
                while (oReader.Read())
                {

                    rtfInforme = DataBase.GetStringFromReader(oReader, "TEXTO");
                    if (rtfInforme.StartsWith("{\\rtf1"))
                    {
                        //result= result.Replace()
                        //result = DataBase.convertRtfToHtml(rtfInforme);
                        result = DataBase.convertRtfToText(rtfInforme);
                    }
                    else
                    {
                        result = rtfInforme;
                        result = HttpUtility.HtmlDecode(result);

                    }



                }
                if (result.IndexOf("<style") > 0)
                {
                    return result;
                }
                else
                {
                    if (result.IndexOf("<body>") > 0)
                    {
                        return result.Substring(result.IndexOf("<body>"), result.IndexOf("</body>") - result.IndexOf("<body>"));
                    }
                    //si entramos por este else quiere decir que es texto plano
                    else
                    {
                        return result;
                    }

                }



            }
            catch (Exception)
            {

                throw;
            }
            finally

            {
                if (oConexion.State == System.Data.ConnectionState.Open)
                {
                    if (oCommand != null)
                    {
                        oCommand.Dispose();
                    }
                    if (oCommandTextos != null)
                    {
                        oCommandTextos.Dispose();
                    }


                    oConexion.Close();
                }
                if (oConexionTextos.State == System.Data.ConnectionState.Open)
                {
                    oConexionTextos.Close();
                    if (oCommandTextos != null)
                    {
                        oCommandTextos.Dispose();
                    }
                }
            }
        }

        public static List<INFORMES> ObtenerPorPaciente(int oidOwner)
        {
            FbConnection oConexion = new FbConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ConexionBD"].ConnectionString);

            string queryInformes = "select centros.nombre as nombrecentro, centros.oid as oidcentro, i.*,";
            queryInformes += "m.cod as lkp_medico,l.cod as lkp_medrev,m.login as medInforme, l.login as medRevision, e.fecha as FECHAEXPLORACION,e.hora as HORAEXPLORACION from informes i";
            queryInformes += " left join personal m on m.oid = i.ior_medinforme left join personal l on l.oid=i.ior_medrevision";
            queryInformes += " left join exploracion e on e.oid=i.owner ";
            queryInformes += " left join daparatos dapa on dapa.oid=e.ior_aparato";
            queryInformes += " left join centros on centros.oid=dapa.cid";
            queryInformes += " where i.IOR_PAC=" + oidOwner + " order by i.fecha desc";

            oConexion.Open();
            FbCommand oCommand = new FbCommand(queryInformes, oConexion);
            FbDataReader oReader = oCommand.ExecuteReader();

            try
            {
                List<INFORMES> lListaInformes = new List<INFORMES>();
                while (oReader.Read())
                {
                    try
                    {
                        DateTime fechaInforme;
                        DateTime fechaRevision;
                        DateTime fechaModifica;
                        DateTime fechaInformeExploracion;
                        DateTime fechaMaxEntrega;
                        INFORMES oInforme = new INFORMES();
                        oInforme.OID = DataBase.GetIntFromReader(oReader, "OID");
                        //fecha del informe
                        if (DateTime.TryParse(oReader["FECHA"].ToString(), out fechaInforme))
                        {
                            oInforme.FECHA = (DateTime)oReader["FECHA"];


                        }

                        if (DateTime.TryParse(oReader["fecharevision"].ToString(), out fechaRevision))
                        {
                            oInforme.FECHAREVISION = (DateTime)oReader["fecharevision"];
                        }
                        if (DateTime.TryParse(oReader["FECHAEXPLORACION"].ToString(), out fechaInformeExploracion))
                        {
                            oInforme.FECHAEXPLORACION = DateTime.Parse(DataBase.GetDateTimeFromReader(oReader, "FECHAEXPLORACION").Value.ToString("dd/MM/yyyy") + " " + DataBase.GetStringFromReader(oReader, "HORAEXPLORACION"));

                        }

                        if (DateTime.TryParse(oReader["MODIF"].ToString(), out fechaModifica))
                        {
                            oInforme.MODIF = fechaModifica;


                        }
                        oInforme.HORAMOD = DataBase.GetStringFromReader(oReader, "HORAMOD");
                        oInforme.OWNER = DataBase.GetIntFromReader(oReader, "owner");
                        oInforme.IOR_PAC = DataBase.GetIntFromReader(oReader, "IOR_PAC");
                        oInforme.HORA = DataBase.GetStringFromReader(oReader, "HORA");
                        oInforme.HORAREV = DataBase.GetStringFromReader(oReader, "HORAREV");
                        oInforme.IOR_MEDINFORME = DataBase.GetIntFromReader(oReader, "IOR_MEDINFORME");
                        oInforme.IOR_MEDREVISION = DataBase.GetIntFromReader(oReader, "IOR_MEDREVISION");
                        oInforme.LOGINMEDICOREVISOR = DataBase.GetStringFromReader(oReader, "medRevision");
                        oInforme.LOGINMEDICOINFORMANTE = DataBase.GetStringFromReader(oReader, "medInforme");
                        oInforme.BORRADO = DataBase.GetStringFromReader(oReader, "BORRADO");
                        oInforme.TITULO = DataBase.GetStringFromReader(oReader, "TITULO");
                        oInforme.PATOLOGICO = DataBase.GetStringFromReader(oReader, "PATOLOGICO");
                        oInforme.VALIDACION = DataBase.GetStringFromReader(oReader, "VALIDACION");
                        oInforme.CENTRO = DataBase.GetStringFromReader(oReader, "nombrecentro");
                        oInforme.CENTROOID = DataBase.GetIntFromReader(oReader, "oidcentro");
                        lListaInformes.Add(oInforme);
                    }
                    catch (Exception ex)
                    {


                    }

                }


                return lListaInformes;
            }
            catch (Exception)
            {

                throw;
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
        }

        public static List<INFORMES> ObtenerPorExploracion(int oid)
        {
            FbConnection oConexion = new FbConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ConexionBD"].ConnectionString);

            string queryInformes = "select centros.nombre as nombrecentro, centros.oid as oidcentro, i.borrado, i.oid,i.IOR_PAC,i.fecha,i.fecharevision, i.horarev,i.titulo,i.owner, i.borrado,i.ior_medinforme,";
            queryInformes += "m.cod as lkp_medico,l.cod as lkp_medrev,m.login as medInforme, l.login as medRevision, e.fecha as FECHAEXPLORACION,i.patologico,i.ior_medrevision,i.validacion,i.hora from informes i";
            queryInformes += " left join personal m on m.oid = i.ior_medinforme left join personal l on l.oid=i.ior_medrevision";
            queryInformes += " left join exploracion e on e.oid=i.owner ";
            queryInformes += " left join daparatos dapa on dapa.oid=e.ior_aparato";
            queryInformes += " left join centros on centros.oid=dapa.cid";
            queryInformes += " where i.OWNER=" + oid + " order by i.fecha desc";

            oConexion.Open();
            FbCommand oCommand = new FbCommand(queryInformes, oConexion);
            FbDataReader oReader = oCommand.ExecuteReader();

            try
            {
                List<INFORMES> lListaInformes = new List<INFORMES>();
                while (oReader.Read())
                {
                    try
                    {
                        DateTime fechaInforme;
                        DateTime fechaRevision;
                        DateTime fechaInformeExploracion;
                        DateTime fechaMaxEntrega;
                        INFORMES oInforme = new INFORMES();
                        oInforme.OID = DataBase.GetIntFromReader(oReader, "OID");
                        //fecha del informe
                        if (DateTime.TryParse(oReader["FECHA"].ToString(), out fechaInforme))
                        {
                            oInforme.FECHA = (DateTime)oReader["FECHA"];


                        }

                        if (DateTime.TryParse(oReader["fecharevision"].ToString(), out fechaRevision))
                        {
                            oInforme.FECHAREVISION = (DateTime)oReader["fecharevision"];
                        }
                        if (DateTime.TryParse(oReader["FECHAEXPLORACION"].ToString(), out fechaInformeExploracion))
                        {
                            oInforme.FECHAEXPLORACION = (DateTime)DataBase.GetDateTimeFromReader(oReader, "FECHAEXPLORACION");

                        }

                        oInforme.OWNER = DataBase.GetIntFromReader(oReader, "owner");
                        oInforme.IOR_PAC = DataBase.GetIntFromReader(oReader, "IOR_PAC");
                        oInforme.HORA = DataBase.GetStringFromReader(oReader, "HORA");
                        oInforme.HORAREV = DataBase.GetStringFromReader(oReader, "HORAREV");
                        oInforme.IOR_MEDINFORME = DataBase.GetIntFromReader(oReader, "IOR_MEDINFORME");
                        oInforme.IOR_MEDREVISION = DataBase.GetIntFromReader(oReader, "IOR_MEDREVISION");
                        oInforme.LOGINMEDICOREVISOR = DataBase.GetStringFromReader(oReader, "medRevision");
                        oInforme.LOGINMEDICOINFORMANTE = DataBase.GetStringFromReader(oReader, "medInforme");
                        oInforme.BORRADO = DataBase.GetStringFromReader(oReader, "BORRADO");
                        oInforme.TITULO = DataBase.GetStringFromReader(oReader, "TITULO");
                        oInforme.PATOLOGICO = DataBase.GetStringFromReader(oReader, "PATOLOGICO");
                        oInforme.VALIDACION = DataBase.GetStringFromReader(oReader, "VALIDACION");
                        oInforme.CENTRO = DataBase.GetStringFromReader(oReader, "nombrecentro");
                        oInforme.CENTROOID = DataBase.GetIntFromReader(oReader, "oidcentro");
                        lListaInformes.Add(oInforme);
                    }
                    catch (Exception ex)
                    {


                    }

                }


                return lListaInformes;
            }
            catch (Exception)
            {

                throw;
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
        }

        private static string SetDefaultTextSize(string inputHtml, int fontSize, string fontName)
        {
            HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
            doc.LoadHtml(inputHtml);
            var elementsWithStyleAttribute = doc.DocumentNode.SelectNodes(string.Concat("//", "span"));

            if (null == elementsWithStyleAttribute)
            {
                return inputHtml;
            }

            foreach (var element in elementsWithStyleAttribute)
            {
                var classElement = element.GetAttributeValue("class", null);

                if (!string.IsNullOrWhiteSpace(classElement))
                {
                    // Remove class attribute.
                    element.Attributes["class"].Remove();
                }

                var styles = element.GetAttributeValue("style", null);

                if (!string.IsNullOrWhiteSpace(styles)) //&& (styles.ToUpper().Contains("FONT-FAMILY:") || styles.ToUpper().Contains("FONT-SIZE:")))
                {
                    element.Attributes["style"].Remove();

                    string[] splitter = { ";" };
                    string[] styleClasses = styles.Split(splitter, StringSplitOptions.None);

                    StringBuilder sbStyles = new StringBuilder("font-family:Arial; font-size:10pt;");

                    if (null != styleClasses && styleClasses.Length > 0)
                    {
                        foreach (var item in styleClasses)
                        {
                            if (!string.IsNullOrWhiteSpace(item) && !item.ToUpper().Contains("FONT-FAMILY:")
                                && !item.ToUpper().Contains("FONT-SIZE:") && !item.ToUpper().Contains("FONT:"))
                            {
                                // Add existing styles except font size and font family styles.
                                sbStyles.Append(item.Trim());
                                sbStyles.Append(";");
                            }
                        }
                    }

                    element.SetAttributeValue("style", sbStyles.ToString());
                }
                else
                {
                    element.SetAttributeValue("style", "font-family:Arial; font-size:10pt;");
                }
            }

            return doc.DocumentNode.InnerHtml;
        }

        public static string GenerarPDF(string Carpeta, bool conCabecera, int oid, string rutaCabecera, string password = "")
        {
            RadioDBContext db = new RadioDBContext();
            WebConfigRepositorio oConfig = new WebConfigRepositorio();
            string RutaDOCS = oConfig.ObtenerValor("RUTADOCS");
            INFORMES oInforme = InformesRepositorio.Obtener(oid);
            //Si por el oid que nos han pasado no obtenemos ningun objeto
            //tal vez sea porque nos han enviado el oid de la exploración
            //en cuyo caso debemos buscar un informe con el owner igual al OID que nos han enviado como parametro
            if (oInforme.OID <= 0)
            {
                oInforme = InformesRepositorio.ObtenerDeExploracion(oid).Single(i => i.VALIDACION == "T");
            }

            // oInforme.TEXTOHTML = SetDefaultTextSize(oInforme.TEXTOHTML, 12, "Arial");

            EXPLORACION oExploracion2 = ExploracionRepositorio.Obtener((int)oInforme.OWNER);
            DAPARATOS oDaparato = oExploracion2.DAPARATO;
         
            string rutaInformeDefi = RutaDOCS + @"\RW_" + oExploracion2.OID + "_" + oExploracion2.PACIENTE.PACIENTE1.Replace("'", "") + ".pdf";
            if (System.IO.File.Exists(rutaInformeDefi))
            {
                System.IO.File.Delete(rutaInformeDefi);
            }

            string rutaFicheroCabeceraTemp = "";
            string htmlCabecera = "";
            string htmlPie = "";
            if (conCabecera)
            {
                string headerUrl = ""; 

                try
                {
                    headerUrl= HostingEnvironment.MapPath(rutaCabecera);
                }
                catch (Exception)
                {

                    headerUrl = rutaCabecera;
                }
              
                htmlCabecera = System.IO.File.ReadAllText(headerUrl);

                htmlCabecera = htmlCabecera.Replace("@PA", oExploracion2.PACIENTE.PACIENTE1);
                htmlCabecera = htmlCabecera.Replace("@FNAC", oExploracion2.PACIENTE.FECHAN.Value.ToShortDateString());
                htmlCabecera = htmlCabecera.Replace("@HC", oExploracion2.PACIENTE.OID.ToString());

                if (oExploracion2.COLEGIADO.OID > 0)
                {
                    htmlCabecera = htmlCabecera.Replace("@ME", oExploracion2.COLEGIADO.TRATA.ToUpper() + " " + oExploracion2.COLEGIADO.NOMBRE);
                }
                else
                {
                    htmlCabecera = htmlCabecera.Replace("@ME", "");
                }

                List<EXPLORACION> oExploracionesInformadas = new List<EXPLORACION>();
                List<EXPLORACION> oExploracionesInformadasTemp = ExploracionRepositorio.ObtenerHijas(oExploracion2.OID);
                oExploracionesInformadasTemp.Add(oExploracion2);
                for (int i = 0; i <= oExploracionesInformadasTemp.Count - 1; i++)
                {

                    oExploracionesInformadas.Add(oExploracionesInformadasTemp[i]);

                }
                string IniciofilasCabecera = "<table border='0' style='width: 100 %;font-family:Verdana; font-size:10px;'>";
                string filasCabecera = "";
                string FinfilasCabecera = "</table>";
                foreach (var item in oExploracionesInformadas)
                {
                    string htmlInfoExploracion = oConfig.ObtenerValor("InfoExploracionHeader");
                    //si en un mismo dia hay la misma exploración con hijas relacionada
                    //significa que están ocupando tiempo de aparato y no debe aparecer enl a cabecera del informe                    
                    if (item.CABINF_EXPLO == "T")
                    {
                        htmlInfoExploracion = htmlInfoExploracion.Replace("@EX", item.APARATO.DES_FIL);
                        htmlInfoExploracion = htmlInfoExploracion.Replace("@FECHEX", item.FECHA.Value.ToShortDateString());
                    }
                    else
                    {
                        htmlInfoExploracion = htmlInfoExploracion.Replace("@EX", "");
                        htmlInfoExploracion = htmlInfoExploracion.Replace("@FECHEX", "");
                    }

                    var consumiblesAsociados = db.Exp_Consum
                        .Where(c => c.IOR_EXPLORACION == item.OID).ToList();
                    bool imprmirConsumible = false;
                    if (consumiblesAsociados.Count > 0 && consumiblesAsociados.First().CABINF_DOSIS == "T")
                    {
                        EXP_CONSUM oExpConsum = db.Exp_Consum.Where(c => c.IOR_EXPLORACION == item.OID).First();
                        CONSUMIBLES oConsumible = ConsumibleRepositorio.Obtener(oExpConsum.IOR_CONSUM.Value);
                        imprmirConsumible = true;
                        htmlInfoExploracion = htmlInfoExploracion.Replace("@TRAZADOR", oConsumible.DES_CONSUM);
                        htmlInfoExploracion = htmlInfoExploracion.Replace("@DOSIS", oExpConsum.DOSIS);
                        if (oExpConsum.MCI > 0)
                        {
                            if (oConsumible.DES_CONSUM == "DLP")
                            {
                                htmlInfoExploracion = htmlInfoExploracion.Replace("@MCI@", oExpConsum.MCI.ToString() + "mGy/cm");
                            }
                            else
                            {
                                htmlInfoExploracion = htmlInfoExploracion.Replace("@MCI@", oExpConsum.MCI.ToString() + "mCi (" + oExpConsum.MCI * 37 + "MBq)");

                            }
                        }
                        else
                        {
                            htmlInfoExploracion = htmlInfoExploracion.Replace("@MCI@", "");

                        }
                        if (oExpConsum.DLP > 0)
                        {
                            htmlInfoExploracion = htmlInfoExploracion.Replace("@DLP@", oExpConsum.DLP.ToString() + "mGy/cm");
                        }
                        else
                        {
                            if (consumiblesAsociados.Any(c => c.DLP > 0))
                            {
                                var dlp = consumiblesAsociados.Single(c => c.DLP > 0);
                                dlp.CONSUMIBLE = ConsumibleRepositorio.Obtener(dlp.IOR_CONSUM.Value);
                                htmlInfoExploracion = htmlInfoExploracion.Replace("@DLP@", " / " + dlp.CONSUMIBLE.DES_CONSUM + "-" + dlp.DLP.ToString() + " mGy/cm");
                            }
                            htmlInfoExploracion = htmlInfoExploracion.Replace("@DLP@", "");
                            //htmlInfoExploracion = htmlInfoExploracion.Replace("@DLP@", "");

                        }
                        //htmlInfoExploracion = htmlInfoExploracion.Replace("@DLP@", oExpConsum.DLP.ToString());
                    }
                    else
                    {
                        htmlInfoExploracion = htmlInfoExploracion.Replace("@TRAZADOR", "");
                        htmlInfoExploracion = htmlInfoExploracion.Replace("@DOSIS", "");
                        htmlInfoExploracion = htmlInfoExploracion.Replace("@DLP@", "");
                        htmlInfoExploracion = htmlInfoExploracion.Replace("@MCI@", "");
                    }
                    if (item.CABINF_EXPLO == "T" || imprmirConsumible)
                    {
                        filasCabecera = filasCabecera + htmlInfoExploracion;
                        // htmlCabecera = htmlCabecera.Replace("@INFOEXPLORACION", "@INFOEXPLORACION " + htmlInfoExploracion);
                    }

                }
                if (!String.IsNullOrEmpty(filasCabecera))
                {
                    htmlCabecera = htmlCabecera.Replace("@INFOEXPLORACION", IniciofilasCabecera + filasCabecera + FinfilasCabecera);

                }
                else
                {
                    htmlCabecera = htmlCabecera.Replace("@INFOEXPLORACION ", "");
                    htmlCabecera = htmlCabecera.Replace("@INFOEXPLORACION ", "");
                    htmlCabecera = htmlCabecera.Replace("@INFOEXPLORACION ", "");
                    htmlCabecera = htmlCabecera.Replace("@INFOEXPLORACION ", "");

                }

                if (oExploracion2.PACIENTE.DIRECCIONES.Count > 0)
                {
                    htmlCabecera = htmlCabecera.Replace("@DIR", oExploracion2.PACIENTE.DIRECCIONES.First().DIRECCION1);
                    htmlCabecera = htmlCabecera.Replace("@CP", oExploracion2.PACIENTE.DIRECCIONES.First().CP);
                    htmlCabecera = htmlCabecera.Replace("@POB", oExploracion2.PACIENTE.DIRECCIONES.First().POBLACION);
                }

                htmlCabecera = htmlCabecera.Replace("@EX", oExploracion2.APARATO.DES_FIL);
                htmlCabecera = htmlCabecera.Replace("@FE", oInforme.FECHA.ToShortDateString());
                htmlCabecera = htmlCabecera.Replace("@MU", "Mutua:" + oExploracion2.ENTIDAD_PAGADORA.CODMUT);
                rutaFicheroCabeceraTemp = HostingEnvironment.MapPath("~/Reports/pdf/templates/cabecera_" + oid.ToString() + ".html");
                System.IO.File.WriteAllText(rutaFicheroCabeceraTemp, htmlCabecera);

                string RutaMacrosPie = "";
                try
                {
                    RutaMacrosPie = oConfig.ObtenerValor("RutaMacroPieInformes");
                }
                catch (Exception)
                {
                    System.Threading.Thread.Sleep(1);

                }
                if (!String.IsNullOrEmpty(RutaMacrosPie))
                {
                    string pierUrl = HostingEnvironment.MapPath(RutaMacrosPie);

                    htmlPie = System.IO.File.ReadAllText(pierUrl);
                    PERSONAL oMedicoInformante = db.Personal
                        .Where(p => p.OID == oInforme.IOR_MEDINFORME).SingleOrDefault();
                    PERSONAL oMedicoRevisor = db.Personal
                        .Where(p => p.OID == oInforme.IOR_MEDREVISION).SingleOrDefault();
                    if (oMedicoInformante != null)
                    {
                        var request = HttpContext.Current.Request; // Asegúrate de tener 'using System.Web;' para acceder a HttpContext
                        string baseUrl = request.Url.GetLeftPart(UriPartial.Authority);

                        // Ahora construyes la cadena de la imagen usando la URL base dinámica
                        string cadenaImagen = $"{baseUrl}/img/firmas/{oMedicoInformante.LOGIN}.jpg";

                        htmlPie = htmlPie.Replace("Firma M&eacute;dico", "<img border='0' src='" + cadenaImagen + "' width='100'/>");
                        htmlPie = htmlPie.Replace("Firma Médico", "<img border='0' src='" + cadenaImagen + "' width='100'/>");
                        htmlPie = htmlPie.Replace("Firma Medico", "<img border='0' src='" + cadenaImagen + "' width='100'/>");
                        htmlPie = htmlPie.Replace("FirmaMedico", "<img border='0' src='" + cadenaImagen + "' width='100'/><p>" + oMedicoInformante.DESCRIPCION + "</p><p>" + oMedicoInformante.NUMERO + "</p>");
                        htmlPie = htmlPie.Replace("@MEDICO1", oMedicoInformante.DESCRIPCION);
                        if (oMedicoInformante.NUMERO != null)
                        {
                            htmlPie = htmlPie.Replace("@NUMCOLMEDICO1", oMedicoInformante.NUMERO);
                        }
                        else
                        {
                            htmlPie = htmlPie.Replace("@NUMCOLMEDICO1", "");
                        }
                    }
                    if (oMedicoRevisor != null)
                    {
                        //si el revisor y el que informa es el mismo
                        if (oMedicoInformante.DESCRIPCION == oMedicoRevisor.DESCRIPCION || oInforme.USERNAME != oMedicoRevisor.LOGIN)
                        {
                            //Puede ser que haya validado el informe un usuario privilegiado que tendremos en la tabla personal
                            PERSONAL oMedicoPrivilegiadoQuevalida = db.Personal
                                .Where(p => p.LOGIN == oInforme.USERNAME).SingleOrDefault();
                            if (oMedicoPrivilegiadoQuevalida != null
                                && oMedicoPrivilegiadoQuevalida.DESCRIPCION != oMedicoRevisor.DESCRIPCION
                                && oMedicoPrivilegiadoQuevalida.DESCRIPCION != oMedicoInformante.DESCRIPCION)
                            {
                                htmlPie = htmlPie.Replace("@MEDICO2", oMedicoPrivilegiadoQuevalida.DESCRIPCION);
                                if (oMedicoInformante.NUMERO != null)
                                {
                                    htmlPie = htmlPie.Replace("@NUMCOLMEDICO2", oMedicoPrivilegiadoQuevalida.NUMERO);
                                }
                                else
                                {
                                    htmlPie = htmlPie.Replace("@NUMCOLMEDICO2", "");
                                }
                            }
                            else
                            {
                                htmlPie = htmlPie.Replace("@MEDICO2", "");
                                htmlPie = htmlPie.Replace("N. Col: @NUMCOLMEDICO2", "");
                            }
                        }
                        else
                        {
                            htmlPie = htmlPie.Replace("@MEDICO2", oMedicoRevisor.DESCRIPCION);
                            if (oMedicoInformante.NUMERO != null)
                            {
                                htmlPie = htmlPie.Replace("@NUMCOLMEDICO2", oMedicoRevisor.NUMERO);
                            }
                            else
                            {
                                htmlPie = htmlPie.Replace("@NUMCOLMEDICO2", "");
                            }
                        }

                    }

                }

            }
            // Expresión regular para detectar múltiples <div> con contenido "&nbsp;" o un espacio no rompible
            // Expresión regular para detectar múltiples <div> con contenido "&nbsp;" o un espacio no rompible
            string pattern = @"(<div>(?:&nbsp;|\u00A0)</div>\s*){2,}";

            // Reemplaza los bloques consecutivos por una sola etiqueta <br/>
            string result = Regex.Replace(oInforme.TEXTOHTML, pattern, "<br/>", RegexOptions.IgnoreCase);

            // Asigna el resultado procesado de nuevo al campo TEXTOHTML
            oInforme.TEXTOHTML = result;

            // Opcionalmente, puedes usar el texto procesado en otra variable para debug o uso adicional
            string htmlText = oInforme.TEXTOHTML;

            if (!htmlText.TrimStart().StartsWith("<html>") || oExploracion2.EMPRESA.NOMBRE.Contains("FORASTÉ"))
            {
                //Si el texto no empieza por html puede ser debido a que viene de un informe rtf en cuyo caso tiene head y style
                if (htmlText.Contains("<style"))
                {
                    HtmlDocument htmlDoc = new HtmlDocument();
                    htmlDoc.LoadHtml(htmlText);
                    htmlDoc.DocumentNode.SelectNodes("//style|//script")
                        .ToList()
                        .ForEach(n => n.Remove());
                    htmlDoc.DocumentNode.SelectNodes("//br").ToList().ForEach(n => n.Remove());

                    // Get all Nodes: script
                    HtmlAgilityPack.HtmlNodeCollection nodes = htmlDoc.DocumentNode.SelectNodes("//span");
                    

                    nodes = htmlDoc.DocumentNode.SelectNodes("//meta");
                    // Remove all Nodes:
                    if (nodes != null)
                    {
                        foreach (HtmlNode node in nodes)
                        {
                            node.Remove();
                        }
                    }  


                    nodes = htmlDoc.DocumentNode.SelectNodes("//p");
                    if (nodes != null)
                    {
                        foreach (HtmlNode p in nodes)
                        {

                            p.Attributes.Remove("style");
                            p.Attributes.Remove("class");
                            p.InnerHtml = p.InnerText ;
                        }
                    }

                    nodes = htmlDoc.DocumentNode.SelectNodes("//span");
                    if (nodes != null)
                    {
                        foreach (HtmlNode p in nodes)
                        {

                            p.Attributes.Remove("style");
                            p.Attributes.Remove("class");
                            p.InnerHtml = p.InnerText;
                        }
                    }

                    nodes = htmlDoc.DocumentNode.SelectNodes("//div");
                    if (nodes != null)
                    {
                        foreach (HtmlNode p in nodes)
                        {
                            p.Attributes.Remove("style");
                            p.Attributes.Remove("class");
                        }
                    }



                    htmlText = htmlDoc.DocumentNode.InnerHtml;
                    //htmlText = "<html>" + oInforme.TEXTOHTML.Replace("</style>", "</style><body>") + "</body></html>";
                    try
                    {
                        htmlText = htmlText.Substring(htmlText.IndexOf("</style>"));
                    }
                    catch
                    {

                    }


                    htmlText = htmlText.Replace("<style type=\"text/css\">", "");

                    if (htmlText.Contains("<style"))
                    {
                        //htmlText = htmlText.Replace("<style", "<head><style");
                        //htmlText = htmlText.Replace("</style>", "</style><body>");
                        htmlText = "<html>" + htmlText + "</body></html>";
                    }
                    else
                    {
                        if (!htmlText.Contains("<html>"))
                        {
 htmlText = "<html><head></head><body>" + htmlText + "</body></html>";
                        }
                           

                    }


                }
                //o puede ser un html que proviene directamente del editor SummerNote en cuyo caso no tiene nada
                else
                {
                    htmlText = htmlText.Replace("<body>", "");
                    htmlText = "<html><head></head><body>" + htmlText + "</body></html>";
                }
                //  htmlText = SetDefaultTextSize(oInforme.TEXTOHTML, 12, "Arial");

            }

            if (htmlText.Contains("FirmaMedico") ||htmlText.Contains("Firma M&eacute;dico") || htmlText.Contains("Firma Médico") || htmlText.Contains("Firma Medico"))
            {
                PERSONAL oPersonal = PersonalRepositorio.Obtener(oInforme.IOR_MEDINFORME);
                //  string cadenaImagen = "http://172.30.229.6/img/firmas/" + oPersonal.LOGIN + ".jpg";
                var request = HttpContext.Current.Request; // Asegúrate de tener 'using System.Web;' para acceder a HttpContext
                string baseUrl = request.Url.GetLeftPart(UriPartial.Authority);

                // Ahora construyes la cadena de la imagen usando la URL base dinámica
                string cadenaImagen = $"{baseUrl}/img/firmas/{oPersonal.LOGIN}.jpg";

                htmlText = htmlText.Replace("Firma M&eacute;dico", "<img border='0' src='" + cadenaImagen + "' width='100'/>");
                htmlText = htmlText.Replace("Firma Médico", "<img border='0' src='" + cadenaImagen + "' width='100'/>");
                htmlText = htmlText.Replace("Firma Medico", "<img border='0' src='" + cadenaImagen + "' width='100'/>");
                htmlText = htmlText.Replace("FirmaMedico", "<img border='0' src='" + cadenaImagen + "' width='100'/><p>" + oPersonal.DESCRIPCION + "</p><p>" + oPersonal.NUMERO + "</p>");
                htmlText = htmlText.Replace("@MI", oPersonal.DESCRIPCION);
                htmlText = htmlText.Replace("@NU", oPersonal.NUMERO);
      
                htmlText = htmlText.Replace("<table style=\"", "<table border=\"0\" style=\"border-color:white;");
                htmlText = htmlText.Replace("<td style=\"", "<td style=\"border-color:white;");
            }
            else
            {
                htmlText = htmlText.Replace("</body>",  htmlPie +  "</body>");

            }
            string htmlString = htmlText.Replace("<body>", "<body>" + htmlCabecera);

           
            htmlString = htmlString.Replace("o:p", "p");
            htmlString = htmlString.Replace("<br>", "<br/>");
            htmlString = htmlString.Replace("<br>", "<br/>");


          

            if (System.IO.File.Exists(rutaInformeDefi))
            {
                System.IO.File.Delete(rutaInformeDefi);
            }
            StreamWriter Swr = new StreamWriter(rutaInformeDefi);
            Swr.Write(htmlString);
            Swr.Close();
            Swr.Dispose();

            CENTROS oCentro = db.Centros.Single(c => c.OID == oExploracion2.OWNER);
            byte[] aPDF = null;
            if (oExploracion2.EMPRESA.NOMBRE.Contains("DELFOS"))
            {
                //integración con Badalona para Delfos
                if (oExploracion2.IOR_ENTIDADPAGADORA.Value == 11042)
                {
                    oCentro.LOGO_URL = "/img/11042LogoDelfos.png";
                }
                aPDF = Models.HTMLToPDF.Converter2.InformeFromHtml(rutaInformeDefi,
                            oCentro.LOGO_URL,
                            oCentro.LOGO_HEIGHT,
                            oCentro.LOGO_WIDTH);
            }
            else
            {
                aPDF = Models.HTMLToPDF.Converter.InformeFromHtml2(rutaInformeDefi,
                        oCentro.LOGO_URL,
                        oCentro.LOGO_HEIGHT,
                        oCentro.LOGO_WIDTH);

            }


            if (System.IO.File.Exists(rutaInformeDefi))
            {
                System.IO.File.Delete(rutaInformeDefi);
            }

            System.IO.File.WriteAllBytes(rutaInformeDefi, aPDF);

            if (conCabecera)
            {
                System.IO.File.Delete(rutaFicheroCabeceraTemp);
            }

            if (password.Length > 0)
            {
                Models.HTMLToPDF.Converter.addPassword(rutaInformeDefi, rutaInformeDefi.Replace(".pdf", "10.pdf"), oInforme.OID.ToString());
                rutaInformeDefi = rutaInformeDefi.Replace(".pdf", "10.pdf");
            }


            return rutaInformeDefi;

        }

        public static string StripHTML(string html)
        {
            var regex = new Regex("<[^>]+>", RegexOptions.IgnoreCase);
            return System.Web.HttpUtility.HtmlDecode((regex.Replace(html, "")));
        }



        public static void GenerarVersionInforme(int oid)
        {
            RadioDBContext db = new RadioDBContext();
            WebConfigRepositorio oConfig = new WebConfigRepositorio();
            //obtenermos el informe de la tabla informes
            INFORMES oInforme = db.Informes.Single(inf => inf.OID == oid);

            PERSONAL oCreadorInforme = db.Personal
                .Single(p => p.OID == oInforme.IOR_MEDINFORME);
            EXPLORACION oExploracion = ExploracionRepositorio.Obtener(oInforme.OWNER.Value);
            //Si el tipo de exploracion tiene un valor de 1 vamos a buscar
            //un fichero con el identificador de paciente a la carpeta de importación de pruebas

            //Miramos cuantos ficheros existen del informe en la tabla de PDFS
            IEnumerable<INFORMESPDF> lInformesExploracion = db.InformesPDF.Where(inf => inf.IOR_EXPLORACION == oExploracion.OID);

            //BASANDONOS EN LA FECHA DE LA EXPLORACION OBTENEMOS LA CARPETA DESTINO
            string rutaFicheropdf = ObtenerCarpetaGuardarPDF(oExploracion.FECHA.Value.Year.ToString(),
                oExploracion.FECHA.Value.Month.ToString().PadLeft(2, '0'));

            PACIENTE oPaciente = PacienteRepositorio.Obtener(oInforme.IOR_PAC.Value);

            string nombreFichero = oPaciente.PACIENTE1.Replace(",", " ").Replace(".", " ");
            nombreFichero = nombreFichero.Replace("  ", " ");
            nombreFichero = nombreFichero.QuitAccents().Replace(" ", "^");
            nombreFichero = nombreFichero + "_" + oExploracion.FECHA.Value.ToString("yyyyMMdd") + '_' + oExploracion.OID + '_'
                + (lInformesExploracion.Count() + 1).ToString().PadLeft(2, '0') + ".pdf";
            //creamos el registro de BASE DE DATOS en la tabla INFORMESPDF
            INFORMESPDF oInfoPDF = new INFORMESPDF();
            oInfoPDF.IOR_PACIENTE = oPaciente.OID;
            oInfoPDF.IOR_EXPLORACION = oExploracion.OID;
            oInfoPDF.IOR_INFORME = oInforme.OID;
            oInfoPDF.PATH = rutaFicheropdf;
            oInfoPDF.USERNAME = HttpContext.Current.User.Identity.Name;
            oInfoPDF.NOMBRE = nombreFichero;
            oInfoPDF.TIPOEXPLORACION = oExploracion.TIPOEXPLORACIONDESC;
            oInfoPDF.FECHA = oExploracion.FECHA;
            oInfoPDF.MEDINFORME = oCreadorInforme.DESCRIPCION;
            oInfoPDF.MEDVALIDACION = oInforme.USERNAME;
            oInfoPDF.MODIF = DateTime.Now;
            oInforme.VALIDACION = oInforme.VALIDACION;
            db.InformesPDF.Add(oInfoPDF);
            db.SaveChanges();


            string RutaDOCS = oConfig.ObtenerValor("RUTADOCS");
            string RutaMacrosCabecera = oConfig.ObtenerValor("RutaMacroCabeceraInformes");
            //GENEREMOS EL FICHERO
            string rutaPdfGenerado = GenerarPDF(RutaDOCS, true, oInforme.OID, RutaMacrosCabecera);
            System.IO.File.Copy(rutaPdfGenerado, rutaFicheropdf + nombreFichero);
            try
            {
                System.IO.File.Delete(rutaPdfGenerado);
            }
            catch (Exception EX)
            {

            }
        }

        public static void GenerarFicherosFiatc(int oid)
        {
            RadioDBContext db = new RadioDBContext();
            INFORMES oInforme = InformesRepositorio.Obtener(oid);
            PERSONAL oCreadorInforme = db.Personal
                .Single(p => p.OID == oInforme.IOR_MEDINFORME);
            EXPLORACION oExploracion = ExploracionRepositorio.Obtener(oInforme.OWNER.Value);
            int i = 0;
#if DEBUG

            string rutaFicheroHl7 = @"C:\INFORMES\ASP" + oExploracion.IOR_ENTIDADPAGADORA + @"\" + oExploracion.IDCITAONLINE + "_" + oExploracion.PACIENTE.MAILING4 + "_" + DateTime.Now.ToString("yyyyMMddhhmmss") + ".hl7";


#else
           
                                string rutaFicheroHl7 = @"E:\01_imaging\04_Data\INFORMES\ASP" + oExploracion.IOR_ENTIDADPAGADORA + @"\" + oExploracion.IDCITAONLINE + "_" + oExploracion.PACIENTE.MAILING4 + "_" + DateTime.Now.ToString("yyyyMMddhhmmss") + ".hl7";

                               
#endif

            try
            {
                PERSONAL oMedicoInforme = PersonalRepositorio.Obtener(oInforme.IOR_MEDINFORME);
                PERSONAL oMedicoRevisor = PersonalRepositorio.Obtener(oInforme.IOR_MEDREVISION);
                string readTextHL7 = System.IO.File.ReadAllText(HostingEnvironment.MapPath("~//fiatc.hl7"));
                readTextHL7 = readTextHL7.Replace("@INFORMEHTML", HtmlToText.ConvertHtml(oInforme.TEXTOHTML));
                readTextHL7 = readTextHL7.Replace("@FECHAGEN", DateTime.Now.ToString("yyyyMMddHHmmss"));// 2018 09 02 205041);
                readTextHL7 = readTextHL7.Replace("@MAILING4", oExploracion.PACIENTE.MAILING4);
                readTextHL7 = readTextHL7.Replace("@MAILING4", oExploracion.PACIENTE.MAILING4);
                readTextHL7 = readTextHL7.Replace("@PACIENTENOMBRE", oExploracion.PACIENTE.PACIENTE1);
                readTextHL7 = readTextHL7.Replace("@FECHANAC", oExploracion.PACIENTE.FECHAN.Value.ToString("yyyyMMdd"));
                readTextHL7 = readTextHL7.Replace("@SEXO", oExploracion.PACIENTE.SEXO);
                readTextHL7 = readTextHL7.Replace("@ESPECIALIDAD", oExploracion.GAPARATO.COD_GRUP);
                readTextHL7 = readTextHL7.Replace("@ACCESSIONNUMBER", oExploracion.IDCITAONLINE);
                readTextHL7 = readTextHL7.Replace("@FECHAHORAVALIDACION", oInforme.FECHAREVISION.Value.ToString("yyyyMMddHHmmss"));
                readTextHL7 = readTextHL7.Replace("@NOMCOL", oMedicoInforme.DESCRIPCION);
                readTextHL7 = readTextHL7.Replace("@NUMCOL", oMedicoInforme.NUMERO);
                readTextHL7 = readTextHL7.Replace("@NOMCOREV", oMedicoRevisor.DESCRIPCION);
                readTextHL7 = readTextHL7.Replace("@NUMCOREV", oMedicoRevisor.NUMERO);


                readTextHL7 = readTextHL7.Replace("<http://www.sautin.com/order.php#RTOHNET>", "");
                string ficheroSIUOrigen = oExploracion.ARCHIVOBADALONA.Split(';')[0];
                // string valorBusqueda = oExploracion.ARCHIVOBADALONA.Split(';')[1];
                string carpeta = "badalona";
                if (oExploracion.IOR_ENTIDADPAGADORA != 11042)
                {
                    carpeta = "fiatc";
                }
                //string readTextSiu = System.IO.File.ReadAllText(@"C:\derivaciones\" + carpeta + @"\ficheros\Procesados\" + ficheroSIUOrigen, System.Text.Encoding.ASCII);
                // readTextSiu = readTextSiu.Replace(valorBusqueda, oExploracion.FECHA.Value.ToString("yyyyMMdd") + oExploracion.HORA.Replace(":", ""));


                System.IO.File.WriteAllText(rutaFicheroHl7, readTextHL7);
            }
            catch (Exception ex)
            {
                LogException.LogMessageToFile(ex.Message);
            }

        }




        public static void GenerarFicherosBadalona(int oid)
        {
            RadioDBContext db = new RadioDBContext();
            INFORMES oInforme = db.Informes.Single(inf => inf.OID == oid);
            PERSONAL oCreadorInforme = db.Personal
                .Single(p => p.OID == oInforme.IOR_MEDINFORME);
            EXPLORACION oExploracion = ExploracionRepositorio.Obtener(oInforme.OWNER.Value);
            int i = 0;
#if DEBUG
            string rutaFicheropdf = @"C:\INFORMES\ASP" + oExploracion.IOR_ENTIDADPAGADORA + @"\" + oExploracion.NHCAP + i.ToString().PadLeft(2, '0') + ".pdf";
            string rutaFicheroXml = @"C:\INFORMES\ASP" + oExploracion.IOR_ENTIDADPAGADORA + @"\" + oExploracion.PACIENTE.MAILING4 + "_" + DateTime.Now.ToString("yyyyMMdd") + i.ToString().PadLeft(2, '0') + ".xml";
            string rutaFicheroHl7 = @"C:\INFORMES\ASP" + oExploracion.IOR_ENTIDADPAGADORA + @"\" + oExploracion.PACIENTE.MAILING4 + ".hl7";

            while (System.IO.File.Exists(rutaFicheropdf))
            {
                i = i + 1;
                rutaFicheropdf = @"C:\INFORMES\ASP" + oExploracion.IOR_ENTIDADPAGADORA + @"\" + oExploracion.NHCAP + i.ToString().PadLeft(2, '0') + ".pdf";
                rutaFicheroXml = @"C:\INFORMES\ASP" + oExploracion.IOR_ENTIDADPAGADORA + @"\" + oExploracion.PACIENTE.MAILING4 + "_" + DateTime.Now.ToString("yyyyMMdd") + i.ToString().PadLeft(2, '0') + ".xml";
                rutaFicheroHl7 = @"C:\INFORMES\ASP" + oExploracion.IOR_ENTIDADPAGADORA + @"\" + oExploracion.PACIENTE.MAILING4 + i.ToString().PadLeft(2, '0') + ".hl7";

            }
#else
                                //string rutaFicheropdf = @"E:\01_imaging\04_Data\INFORMES\ASP" + oExploracion.IOR_ENTIDADPAGADORA + @"\" + oExploracion.NHCAP + i.ToString().PadLeft(2, '0') + ".pdf";
                                string rutaFicheroXml = @"E:\01_imaging\04_Data\INFORMES\ASP" + oExploracion.IOR_ENTIDADPAGADORA + @"\"  + oExploracion.PACIENTE.MAILING4 + "_" + DateTime.Now.ToString("yyyyMMdd") + i.ToString().PadLeft(2, '0') + ".xml";
                                string rutaFicheroHl7 = @"E:\01_imaging\04_Data\INFORMES\ASP" + oExploracion.IOR_ENTIDADPAGADORA + @"\" + oExploracion.PACIENTE.MAILING4 + ".hl7";

                                string rutaFicheropdf = @"E:\01_imaging\04_Data\INFORMES\ASP" + oExploracion.IOR_ENTIDADPAGADORA + @"\" + oExploracion.NHCAP + i.ToString().PadLeft(2, '0') + ".pdf";
                                while (System.IO.File.Exists(rutaFicheropdf))
                                {
                                    i = i + 1;
                                    rutaFicheropdf = @"E:\01_imaging\04_Data\INFORMES\ASP" + oExploracion.IOR_ENTIDADPAGADORA + @"\" + oExploracion.NHCAP + i.ToString().PadLeft(2, '0') + ".pdf";
                                    rutaFicheroXml = @"E:\01_imaging\04_Data\INFORMES\ASP" + oExploracion.IOR_ENTIDADPAGADORA + @"\" + oExploracion.PACIENTE.MAILING4 + "_" + DateTime.Now.ToString("yyyyMMdd") + i.ToString().PadLeft(2, '0') + ".xml";
                                 rutaFicheroHl7 = @"E:\01_imaging\04_Data\INFORMES\ASP" + oExploracion.IOR_ENTIDADPAGADORA + @"\" + oExploracion.PACIENTE.MAILING4 + i.ToString().PadLeft(2, '0') + ".hl7";

                                }
#endif
            WebConfigRepositorio oConfig = new WebConfigRepositorio();
            string RutaDOCS = oConfig.ObtenerValor("RUTADOCS");
            string RutaMacrosCabecera = oConfig.ObtenerValor("RutaMacroCabeceraInformes");
            string rutaPdfGenerado = GenerarPDF(RutaDOCS, true, oInforme.OID, RutaMacrosCabecera);
            System.IO.File.Copy(rutaPdfGenerado, rutaFicheropdf, true);
            try
            {
                System.IO.File.Delete(rutaPdfGenerado);
            }
            catch (Exception)
            {

            }

            //GENERAMOS EL XML CON EL FORMATO DE BADALONA
            string readTextXml = System.IO.File.ReadAllText(HostingEnvironment.MapPath("~//badalona.xml"));
            readTextXml = readTextXml.Replace("@FECHA", DateTime.Now.ToString("yyyy-MM-dd:HH:mm:ss"));
            readTextXml = readTextXml.Replace("@NHCPACIENTEBADALONA", oExploracion.PACIENTE.MAILING4);
            readTextXml = readTextXml.Replace("@NOMBREMEDICOINFORME", oCreadorInforme.NOMBRE.Split(',')[1].Trim());
            readTextXml = readTextXml.Replace("@APELLIDOMEDICOINFORME", oCreadorInforme.NOMBRE.Split(' ')[0]);
            readTextXml = readTextXml.Replace("@APELLIDO2MEDICOINFORME", "");
            readTextXml = readTextXml.Replace("@NUMCOL", oCreadorInforme.NUMERO);
            readTextXml = readTextXml.Replace("@DNI", oCreadorInforme.DNI);
            Byte[] bytes = System.IO.File.ReadAllBytes(rutaFicheropdf);
            String file = Convert.ToBase64String(bytes);
            readTextXml = readTextXml.Replace("@CONTENTPDF", file);
            System.IO.File.WriteAllText(rutaFicheroXml, readTextXml);

            LOGUSUARIOS oLogBadalona = new LOGUSUARIOS
            {
                OWNER = oExploracion.OID,
                FECHA = DateTime.Now.ToString("dd/MM/yyyy HH:mm"),
                TEXTO = "Envio INFORME BADALONA",
                USUARIO = HttpContext.Current.User.Identity.Name.ToString(),
                DATA = DateTime.Now.ToString("dd/MM/yyyy"),
                COD_FIL = oExploracion.APARATO.FIL,
                MUTUA = oExploracion.ENTIDAD_PAGADORA.CODMUT
            };

            LogUsuariosRepositorio.Insertar(oLogBadalona);
            //GENERAMOS EL FICHERO SIU DE BADALONA FECHA CITACION EN FORMATO yyyyMMddHHmmss
            try
            {
                string ficheroSIUOrigen = oExploracion.ARCHIVOBADALONA.Split(';')[0];
                string valorBusqueda = oExploracion.ARCHIVOBADALONA.Split(';')[1];
                string carpeta = "badalona";
                if (oExploracion.IOR_ENTIDADPAGADORA != 11042)
                {
                    carpeta = "fiatc";
                }
                string readTextSiu = System.IO.File.ReadAllText(@"C:\derivaciones\" + carpeta + @"\ficheros\Procesados\" + ficheroSIUOrigen, System.Text.Encoding.ASCII);
                readTextSiu = readTextSiu.Replace(valorBusqueda, oExploracion.FECHA.Value.ToString("yyyyMMdd") + oExploracion.HORA.Replace(":", ""));

                readTextSiu = readTextSiu.Replace("RGS|1|A", "RGS|1|U");
                readTextSiu = readTextSiu.Replace("AIG|1|A", "AIG|1|U");
                readTextSiu = readTextSiu.Replace("SIU^S12", "SIU^S13");
                System.IO.File.WriteAllText(rutaFicheroHl7, readTextSiu);
            }
            catch (Exception ex)
            {
                LogException.LogMessageToFile(ex.Message);
            }

        }

    }
}