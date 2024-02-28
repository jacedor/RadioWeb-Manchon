using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using FirebirdSql.Data.FirebirdClient;
using RadioWeb.Models;
using RadioWeb.Utils;


namespace RadioWeb.Models.Repos
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
                InsertComand += oLog.MUTUA + "','" + oLog.COD_FIL + "'," + DataBase.QuotedString(DateTime.Now.ToString("MM/dd/yyyy")) + ")";
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


        /// <summary>
        /// Devuelve lo LOGS de la tabla LOGUSUARIOS
        /// </summary>   
        /// <param name="oidOwner">Oid del la exploracion</param>
        public static List<LOGUSUARIOS> Obtener(int oidExploracion)
        {
            FbConnection oConexion = new FbConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ConexionUsuarios"].ConnectionString);
            FbCommand oCommand = null;
            try
            {
                List<LOGUSUARIOS> oLogUsuarios = new List<LOGUSUARIOS>();
                oConexion.Open();
                 oCommand = new FbCommand("select * FROM LOGUSUARIOS  where OWNER=" + oidExploracion, oConexion);
                FbDataReader oReader = oCommand.ExecuteReader();

                LOGUSUARIOS oTemp = new LOGUSUARIOS();
                while (oReader.Read())
                {
                    oTemp = new LOGUSUARIOS();
                    oTemp.OWNER = DataBase.GetIntFromReader(oReader, "OWNER");
                    oTemp.FECHA = DataBase.GetStringFromReader(oReader, "FECHA");
                    oTemp.USUARIO = DataBase.GetStringFromReader(oReader, "USUARIO");
                    oTemp.TEXTO = DataBase.GetStringFromReader(oReader, "TEXTO");
                    oTemp.OID = DataBase.GetIntFromReader(oReader, "OID");
                    oTemp.MUTUA = DataBase.GetStringFromReader(oReader, "MUTUA");
                    oTemp.COD_FIL = DataBase.GetStringFromReader(oReader, "COD_FIL");


                    oLogUsuarios.Add(oTemp);

                }

                return oLogUsuarios;

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
    }
}