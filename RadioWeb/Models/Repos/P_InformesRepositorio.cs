using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using FirebirdSql.Data.FirebirdClient;
using RadioWeb.Models;
using RadioWeb.Utils;

namespace RadioWeb.Models.Repos
{

    public class P_InformesRepositorio
    {
        public static string ObtenerHtmlDelInforme(int oidInforme, bool esParaTablet=false)
        {
            FbConnection oConexion = new FbConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ConexionBD"].ConnectionString);


            FbCommand oCommand = null;

            FbCommand oCommandTextos = null;

            oConexion.Open();


            try
            {
                string result = "";
                string queryInforme = "SELECT a.TEXTO";
                queryInforme += " FROM TEXTOS a where a.OWNER=" + oidInforme;


                FbDataReader oReader = null;

                oCommand = new FbCommand(queryInforme, oConexion);
                oReader = oCommand.ExecuteReader();

                string rtfInforme = "";
                while (oReader.Read())
                {
                    rtfInforme = DataBase.GetStringFromReader(oReader, "TEXTO");
                    if (rtfInforme.StartsWith("{\\rtf1"))
                    {
                        //result= result.Replace()
                        //result = DataBase.convertRtfToHtml(rtfInforme,false);

                        if (esParaTablet)
                        {
                            result = DataBase.convertRtfToHtml(rtfInforme, false);
                        }
                        else
                        {
                            result = DataBase.convertRtfToText(rtfInforme);
                        }

                       
                        result = result.Replace("<p style=\"margin:0pt 0pt 0pt 0pt;line-height:normal;\"><span class=\"st1\">&nbsp;</span></p>", "<br/>");
                    }
                    else
                    {
                        result = rtfInforme;
                    }


                 }
                result=result.Replace("size=3", "size=\"3\"");
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

            }
        }
        public static P_INFORMES Obtener(int oid)
        {
            FbConnection oConexion = new FbConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ConexionBD"].ConnectionString);
            try
            {
                string queryInformes = "select *  from P_INFORMES p where oid=" + oid;
                FbDataReader oReader = DataBase.EjecutarQuery(oConexion, queryInformes);
                P_INFORMES oPlantillaInforme = new P_INFORMES();
                while (oReader.Read())
                {
                    DateTime fechaInforme;
                    
                    oPlantillaInforme.OID = DataBase.GetIntFromReader(oReader, "OID");
                    //fecha del informe
                    if (DateTime.TryParse(oReader["FECHA"].ToString(), out fechaInforme))
                    {
                        oPlantillaInforme.FECHA = (DateTime)oReader["FECHA"];

                    }
                    oPlantillaInforme.OWNER = DataBase.GetIntFromReader(oReader, "owner");
                    oPlantillaInforme.TITULO = DataBase.GetStringFromReader(oReader, "TITULO");
                   
                   
                }


                return oPlantillaInforme;
            }
            catch (Exception)
            {

                throw;
            }
            finally
            {
                if (oConexion.State == System.Data.ConnectionState.Open)
                    oConexion.Close();
            }
        }
        public static List<P_INFORMES> Lista()
        {
            FbConnection oConexion = new FbConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ConexionBD"].ConnectionString);
            try
            {
                string queryInformes = "select m.COD,M.descripcion,P.FECHA, P.OWNER, P.CID, P.OID, p.titulo  from P_INFORMES p JOIN PERSONAL m on m.OID=p.OWNER";
                queryInformes += " ORDER by  m.COD,M.descripcion, fecha DESC ";
                FbDataReader oReader = DataBase.EjecutarQuery(oConexion, queryInformes);
                List<P_INFORMES> lPlantillasInformes = new List<P_INFORMES>();
                while (oReader.Read())
                {
                    DateTime fechaInforme;

                    P_INFORMES oPlantillaInforme = new P_INFORMES();
                    oPlantillaInforme.OID = DataBase.GetIntFromReader(oReader, "OID");
                    //fecha del informe
                    if (DateTime.TryParse(oReader["FECHA"].ToString(), out fechaInforme))
                    {
                        oPlantillaInforme.FECHA = (DateTime)oReader["FECHA"];

                    }
                    oPlantillaInforme.OWNER = DataBase.GetIntFromReader(oReader, "owner");

                    oPlantillaInforme.TITULO = DataBase.GetStringFromReader(oReader, "TITULO");
                    oPlantillaInforme.DESC_MEDICO = DataBase.GetStringFromReader(oReader, "COD") + " - " + DataBase.GetStringFromReader(oReader, "descripcion");

                    lPlantillasInformes.Add(oPlantillaInforme);
                }


                return lPlantillasInformes;
            }
            catch (Exception)
            {

                throw;
            }
            finally
            {
                if (oConexion.State == System.Data.ConnectionState.Open)
                    oConexion.Close();
            }
        }

    }
}