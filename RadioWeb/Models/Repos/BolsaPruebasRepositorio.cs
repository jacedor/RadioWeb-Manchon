using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using FirebirdSql.Data.FirebirdClient;
using RadioWeb.Models;
using RadioWeb.Utils;
using ADPM.Common;
namespace RadioWeb.Models.Repos

{
    
    public class BolsaPruebasRepositorio
    {
        public static void Anular(int oid, int motivo)
        {
            FbConnection oConexion = new FbConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ConexionBD"].ConnectionString);
            FbCommand oCommand = null;
            try
            {
                oConexion.Open();
                string InsertComand = "UPDATE BOLSA_PRUEBAS SET IOR_MOTDESPROG = @motivo WHERE OID = @OID";
                oCommand = new FbCommand(InsertComand, oConexion);
                oCommand.Parameters.Add("@motivo", motivo);
                oCommand.Parameters.Add("@oid", oid);
                oCommand.ExecuteNonQuery();
            }
            catch (Exception ex)
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
        
    }
}