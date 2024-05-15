using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using FirebirdSql.Data.FirebirdClient;
using RadioWeb.Models;



namespace TuoTempo.Models.Repos
{
    public class LogUsuariosRepositorio
    {

        public static int Insertar(LOGUSUARIOS oLog)
        {
            FbConnection oConexion = new FbConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ConexionUsuarios"].ConnectionString);
            FbCommand oCommand = null;
            try
            {
                string InsertComand = "insert into LOGUSUARIOS (OWNER,FECHA,USUARIO,TEXTO,MUTUA,COD_FIL,DATA) VALUES (";
                oLog.TEXTO = "RW-" + oLog.TEXTO;
                if (oLog.TEXTO.Length > 29)
                {
                    oLog.TEXTO = oLog.TEXTO.Substring(0, 29);
                }
                InsertComand += oLog.OWNER + ",'" + DateTime.Now.ToString("dd-MM-yyyy HH:mm") + "','" + oLog.USUARIO + "','" + oLog.TEXTO + "','";
                InsertComand += oLog.MUTUA + "','" + oLog.COD_FIL + "','" + DateTime.Now.ToString("MM/dd/yyyy") + "')";
                oConexion.Open();
                 oCommand = new FbCommand(InsertComand, oConexion);
                int result = (int)oCommand.ExecuteNonQuery();
                return result;

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