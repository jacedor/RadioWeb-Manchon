using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ADPM.Common;
using FirebirdSql.Data.FirebirdClient;
using RadioWeb.Models;
using RadioWeb.Utils;


namespace RadioWeb.Models.Repos
{
    public class LogVidSignerRepositorio
    {

        public static int Insertar(LOGVIDSIGNER oLog)
        {
            FbConnection oConexion = new FbConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ConexionUsuarios"].ConnectionString);
            FbCommand oCommand = null;
            string InsertComand = "";
            try
            {
                 InsertComand = "insert into LOGVIDSIGNER (FECHA, USUARIO, DOCGUID, PLANTILLA, TEXTO, ACCION, IOR_PACIENTE, IOR_EXPLORACION) VALUES (";
                if (oLog.TEXTO.Length > 100)
                {
                    oLog.TEXTO = oLog.TEXTO.Substring(0, 99);
                }
                InsertComand +=  DateTime.Now.ToString("dd-MM-yyyy HH:mm").QuotedString() + "," + oLog.USUARIO.QuotedString() + "," + oLog.DOCGUID.QuotedString()
                          + "," + oLog.PLANTILLA.QuotedString() + "," + oLog.TEXTO.QuotedString() + "," + oLog.ACCION.QuotedString() + "," + oLog.IOR_PACIENTE + "," + oLog.IOR_EXPLORACION
                           + ")";
                oConexion.Open();
                 oCommand = new FbCommand(InsertComand, oConexion);
                int result = (int)oCommand.ExecuteNonQuery();
                return result;

            }
            catch (Exception ex)
            {

                LogException.LogMessageToFile("Error logear documento" + ex.Message + " " + InsertComand) ;
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
        public static List<LOGVIDSIGNER> Obtener(int oidExploracion)
        {
            FbConnection oConexion = new FbConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ConexionUsuarios"].ConnectionString);
            FbCommand oCommand = null;
            try
            {
                List<LOGVIDSIGNER> oLogUsuarios = new List<LOGVIDSIGNER>();
                oConexion.Open();
                 oCommand = new FbCommand("select * FROM LOGVIDSiGNER  where IOR_EXPLORACION=" + oidExploracion, oConexion);
                FbDataReader oReader = oCommand.ExecuteReader();

                LOGVIDSIGNER oTemp = new LOGVIDSIGNER();
                while (oReader.Read())
                {
                    oTemp = new LOGVIDSIGNER
                    {
                        FECHA = DataBase.GetStringFromReader(oReader, "FECHA"),
                        USUARIO = DataBase.GetStringFromReader(oReader, "USUARIO"),
                        TEXTO = DataBase.GetStringFromReader(oReader, "TEXTO"),
                        OID = DataBase.GetIntFromReader(oReader, "OID"),
                        ACCION = DataBase.GetStringFromReader(oReader, "ACCION"),
                        DOCGUID = DataBase.GetStringFromReader(oReader, "DOCGUID"),
                        IOR_EXPLORACION = DataBase.GetIntFromReader(oReader, "IOR_EXPLORACION"),
                        IOR_PACIENTE = DataBase.GetIntFromReader(oReader, "IOR_PACIENTE"),
                        PLANTILLA = DataBase.GetStringFromReader(oReader, "PLANTILLA")

                    };
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