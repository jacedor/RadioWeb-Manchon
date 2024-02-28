using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using FirebirdSql.Data.FirebirdClient;
using RadioWeb.Models;
using RadioWeb.Utils;


namespace RadioWeb.Models.Repos
{
    public class ConsumibleRepositorio
    {

      

        public static CONSUMIBLES Obtener(int OID)
        {
            FbConnection oConexion = new FbConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ConexionBD"].ConnectionString);
            oConexion.Open();

            FbCommand oCommand = new FbCommand("select * from CONSUMIBLES where oid= " + OID, oConexion);
            FbDataReader oReader = oCommand.ExecuteReader();
            CONSUMIBLES oConsumible = new CONSUMIBLES();
            try
            {
                while (oReader.Read())
                {


                    oConsumible.OID = DataBase.GetIntFromReader(oReader, "OID");
                    oConsumible.VERS = DataBase.GetIntFromReader(oReader, "VERS");
                    oConsumible.CID = DataBase.GetIntFromReader(oReader, "CID");
                    oConsumible.CANAL = DataBase.GetStringFromReader(oReader, "CANAL");
                    oConsumible.OWNER = DataBase.GetIntFromReader(oReader, "OWNER");
                    oConsumible.USERNAME = DataBase.GetStringFromReader(oReader, "USERNAME");
                    oConsumible.MODIF = DataBase.GetDateTimeFromReader(oReader, "MODIF");
                    oConsumible.IOR_EMPRESA = DataBase.GetIntFromReader(oReader, "IOR_EMPRESA");
                    oConsumible.COD_CONSUM = DataBase.GetStringFromReader(oReader, "COD_CONSUM");
                    oConsumible.DES_CONSUM = DataBase.GetStringFromReader(oReader, "DES_CONSUM");
                    oConsumible.TOT_CONSUM = DataBase.GetIntFromReader(oReader, "TOT_CONSUM");
                    oConsumible.BORRADO = DataBase.GetStringFromReader(oReader, "BORRADO");

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
            return oConsumible;
        }

        public static int delete(int  oid)
        {
            FbConnection oConexion = new FbConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ConexionBD"].ConnectionString);
            oConexion.Open();
            FbCommand oCommand = null;
            int result = -1;
            try
            {
                 oCommand = new FbCommand("delete from  PAGOS where owner =" + oid, oConexion);
                 result = oCommand.ExecuteNonQuery();
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
          
           
            return result;
        }


        public static List<CONSUMIBLES> ListaPorGrupoAparatos(int oidGrupo)
        {
            FbConnection oConexion = new FbConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ConexionBD"].ConnectionString);
            oConexion.Open();

            String query = "select a.* from CONSUMIBLES a INNER JOIN CONS_GRUPO b ON a.oid = b.IOR_CONSUMIBLE ";
            query += " where IOR_EMPRESA = 4 and (a.BORRADO != 'T' or a.BORRADO is null) ";
            if (oidGrupo != -1)
            {
                query += "and b.IOR_GAPARATO=" + oidGrupo;
            }
            query += " order by cod_consum";

            FbCommand oCommand = new FbCommand(query, oConexion);
            FbDataReader oReader = oCommand.ExecuteReader();

            List<CONSUMIBLES> lConsumible = new List<CONSUMIBLES>();
            try
            {
                while (oReader.Read())
                {
                    CONSUMIBLES oConsumible = new CONSUMIBLES();
                    oConsumible.OID = DataBase.GetIntFromReader(oReader, "OID");
                    oConsumible.VERS = DataBase.GetIntFromReader(oReader, "VERS");
                    oConsumible.CID = DataBase.GetIntFromReader(oReader, "CID");
                    oConsumible.CANAL = DataBase.GetStringFromReader(oReader, "CANAL");
                    oConsumible.OWNER = DataBase.GetIntFromReader(oReader, "OWNER");
                    oConsumible.USERNAME = DataBase.GetStringFromReader(oReader, "USERNAME");
                    oConsumible.MODIF = DataBase.GetDateTimeFromReader(oReader, "MODIF");
                    oConsumible.IOR_EMPRESA = DataBase.GetIntFromReader(oReader, "IOR_EMPRESA");
                    oConsumible.COD_CONSUM = DataBase.GetStringFromReader(oReader, "COD_CONSUM");
                    oConsumible.DES_CONSUM = DataBase.GetStringFromReader(oReader, "DES_CONSUM");
                    oConsumible.TOT_CONSUM = DataBase.GetIntFromReader(oReader, "TOT_CONSUM");
                    oConsumible.BORRADO = DataBase.GetStringFromReader(oReader, "BORRADO");
                    lConsumible.Add(oConsumible);
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
            return lConsumible;
        }



    }
}
