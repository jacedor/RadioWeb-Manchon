using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using FirebirdSql.Data.FirebirdClient;
using RadioWeb.Models;
using RadioWeb.Utils;


namespace RadioWeb.Models.Repos
{
    public class LogCitaOnlineRepositorio
    {

        public static int Insertar(LOGCITAONLINE oLog)
        {
            FbConnection oConexion = new FbConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ConexionUsuarios"].ConnectionString);
            string InsertComand = "";
            try
            {

                 InsertComand = "insert into LOGCITAONLINE (OID, CID, FECHA,HORA,TEXTO,GRUPO,EXPLORACION,MODIF,USERNAME,APARATO,OWNER,ORDEN,CANAL) VALUES (gen_id(GENUID,1),";
              
                InsertComand += oLog.CID + "," + DataBase.QuotedString(oLog.FECHA) + "," + DataBase.QuotedString(oLog.HORA) + ",'" + oLog.TEXTO.Replace("'","") + "','";
                InsertComand += oLog.GRUPO + "','" + oLog.EXPLORACION + "',"  + DataBase.QuotedString(oLog.MODIF) + "," + DataBase.QuotedString(oLog.USERNAME) + ",";
                InsertComand += DataBase.QuotedString(oLog.APARATO) + ","  +  oLog.OWNER + ",-1,-1)";
                
                oConexion.Open();
                FbCommand oCommand = new FbCommand(InsertComand, oConexion);
                int result = (int)oCommand.ExecuteNonQuery();
                oCommand.Dispose();
                return result;

            }
            catch (Exception ex)
            {

                throw;
            }
            finally
            {
                if (oConexion.State == System.Data.ConnectionState.Open)                    
                    oConexion.Close();
            }



        }


        /// <summary>
        /// Devuelve lo LOGS de la tabla LOGUSUARIOS
        /// </summary>   
        /// <param name="oidOwner">Oid del la exploracion</param>
        public static List<LOGUSUARIOS> Obtener(int oidExploracion)
        {
            FbConnection oConexion = new FbConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ConexionUsuarios"].ConnectionString);

            try
            {
                List<LOGUSUARIOS> oLogUsuarios = new List<LOGUSUARIOS>();
                oConexion.Open();
                FbCommand oCommand = new FbCommand("select * FROM LOGUSUARIOS  where OWNER=" + oidExploracion, oConexion);
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
                oCommand.Dispose();
                return oLogUsuarios;

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