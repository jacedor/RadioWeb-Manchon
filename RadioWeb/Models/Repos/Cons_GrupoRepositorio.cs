using FirebirdSql.Data.FirebirdClient;
using RadioWeb.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RadioWeb.Models.Repos
{
    public class Cons_GrupoRepositorio
    {

        public static int[] FindByConsumibleOid(int? consumibleOid) {

            int[] result = new int[99];            
            FbConnection oConexion = new FbConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ConexionBD"].ConnectionString);
            oConexion.Open();
            string query = "SELECT a.IOR_GAPARATO FROM CONS_GRUPO a where a.IOR_CONSUMIBLE = " + consumibleOid;
            FbCommand oCommand = new FbCommand(query, oConexion);
            try
            {
                FbDataReader oReader = oCommand.ExecuteReader();
                int i = 0;
                while (oReader.Read())
                {
                    result[i] = DataBase.GetIntFromReader(oReader, "IOR_GAPARATO");
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

            return result;
        }


        public static int DeleteByConsumibleOid(int? consumibleOid)
        {

            FbConnection oConexion = new FbConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ConexionBD"].ConnectionString);
            FbCommand oCommand = null;
            int result = -1;

            try
            {
                oConexion.Open();               
                oCommand = new FbCommand("DELETE FROM CONS_GRUPO WHERE IOR_CONSUMIBLE = " + consumibleOid, oConexion);
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

    }
}