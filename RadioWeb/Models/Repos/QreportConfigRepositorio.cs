using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using FirebirdSql.Data.FirebirdClient;
using RadioWeb.Models;
using RadioWeb.Utils;


namespace RadioWeb.Models.Repos
{
    public class QreportConfigRepositorio
    {

        private RadioDBContext db = new RadioDBContext();

        public static QREPORT_CONF Obtener()
        {
            FbConnection oConexion = new FbConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ConexionBD"].ConnectionString);
            oConexion.Open();

            FbCommand oCommand = new FbCommand("select * from qreport_conf where oid = 1", oConexion);
            FbDataReader oReader = oCommand.ExecuteReader();
            QREPORT_CONF oQreportConf = new QREPORT_CONF();

            try
            {
                while (oReader.Read())
                {
                    oQreportConf.OID = DataBase.GetIntFromReader(oReader,"OID");
                    oQreportConf.URL = oReader["URL"].ToString();
                    oQreportConf.CENTRO = oReader["CENTRO"].ToString();
                    oQreportConf.PUBLICKEY_WS = oReader["PUBLICKEY_WS"].ToString();
                    oQreportConf.USER_WS = oReader["USER_WS"].ToString();
                    oQreportConf.PASS_WS = oReader["PASS_WS"].ToString();
                    oQreportConf.TOKEN = oReader["TOKEN"].ToString();
                    oQreportConf.TOKEN_EXPIRATION = DataBase.GetDateTimeFromReader(oReader, "TOKEN_EXPIRATION");
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

            return oQreportConf;

        }

        public static Boolean updateToken(String token, String tokenExpiration) 
        {

            FbConnection oConexion = new FbConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ConexionBD"].ConnectionString);
            FbCommand oCommand = null;
            try
            {
                oConexion.Open();
                string updateStament = "update QREPORT_CONF set TOKEN='"+token+"', TOKEN_EXPIRATION = '"+tokenExpiration+"' ";
                updateStament += "where OID=1 ";
                oCommand = new FbCommand(updateStament, oConexion);
                oCommand.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                return false;

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

            return true;

        }

    }
}