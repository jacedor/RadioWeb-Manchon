using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using FirebirdSql.Data.FirebirdClient;
using RadioWeb.Models;
using RadioWeb.Utils;


namespace RadioWeb.Models.Repos
{
    public class KioskoDaparatoTVRepositorio
    {

        private RadioDBContext db = new RadioDBContext();

        public static List<DAPARATOS> DaparatosSinSala()
        {
            FbConnection oConexion = new FbConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ConexionBD"].ConnectionString);
            oConexion.Open();

            String sql = "SELECT d.* ";
            sql += "FROM DAPARATOS d ";
            sql += "WHERE d.oid NOT IN(SELECT kdt.DAPARATO_OID FROM KIOSKO_DAPARATO_TV kdt) ";
            sql += "AND d.oid > 0 ";           

            FbCommand oCommand = new FbCommand(sql, oConexion);
            FbDataReader oReader = oCommand.ExecuteReader();
            List <DAPARATOS> listDaparatos = new List<DAPARATOS>();

            try
            {
                while (oReader.Read())
                {

                    DAPARATOS daparato = new DAPARATOS();

                    daparato.OID = DataBase.GetIntFromReader(oReader, "OID");
                    daparato.DES_FIL = DataBase.GetStringFromReader(oReader, "DES_FIL");
                    listDaparatos.Add(daparato);

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

            return listDaparatos;

        }

        public static List<DAPARATOS> DaparatosConSala() {


            FbConnection oConexion = new FbConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ConexionBD"].ConnectionString);
            oConexion.Open();

            String sql = "SELECT d.OID, d.DES_FIL, kdt.TV_OID AS SALA ";
            sql += "FROM KIOSKO_DAPARATO_TV kdt INNER JOIN DAPARATOS d ON(kdt.DAPARATO_OID = d.oid) ";
            FbCommand oCommand = new FbCommand(sql, oConexion);
            FbDataReader oReader = oCommand.ExecuteReader();
            List<DAPARATOS> listDaparatos = new List<DAPARATOS>();

            try
            {
                while (oReader.Read())
                {
                    DAPARATOS daparato = new DAPARATOS();
                    daparato.OID = DataBase.GetIntFromReader(oReader, "OID");
                    daparato.DES_FIL = DataBase.GetStringFromReader(oReader, "DES_FIL");
                    daparato.SALA = DataBase.GetIntFromReader(oReader, "SALA");
                    listDaparatos.Add(daparato);
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

            return listDaparatos;
        }

        public static Dictionary<string, string>[] ObtenerPorExploracion(string oid) {

            FbConnection oConexion = new FbConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ConexionBD"].ConnectionString);
            oConexion.Open();

            String sql = "SELECT kt.ID AS viewer_id, d.OID AS desk_id, d.DES_FIL AS deskname, d.KIOSKO_DES as label ";
            sql += "FROM EXPLORACION e ";
            sql += "CROSS JOIN KIOSKO_TV kt ";
            sql += "INNER JOIN DAPARATOS d on (e.IOR_APARATO = d.OID) ";
            sql += "WHERE e.oid = "+ oid;
            FbCommand oCommand = new FbCommand(sql, oConexion);
            FbDataReader oReader = oCommand.ExecuteReader();

            Dictionary<string, string>[] arrayResult = new Dictionary<string, string>[200];
            Dictionary<string, string> queryResult = null;
            try
            {
                int i = 0;
                while (oReader.Read())
                {
                    queryResult = new Dictionary<string, string>();
                    queryResult.Add("viewer_id", DataBase.GetStringFromReader(oReader, "viewer_id"));
                    queryResult.Add("desk_id", DataBase.GetIntFromReader(oReader, "desk_id").ToString());
                    queryResult.Add("deskname", DataBase.GetStringFromReader(oReader, "deskname"));
                    queryResult.Add("label", DataBase.GetStringFromReader(oReader, "label"));
                    arrayResult[i] = queryResult;
                    i++;
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

            return arrayResult;
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