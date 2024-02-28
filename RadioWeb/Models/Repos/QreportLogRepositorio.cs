using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using FirebirdSql.Data.FirebirdClient;
using RadioWeb.Models;
using RadioWeb.Utils;


namespace RadioWeb.Models.Repos
{
    public class QreportLogRepositorio
    {

        public static int Insertar(String tipoMensaje, String envio)
        {
            FbConnection oConexion = new FbConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ConexionBD"].ConnectionString);
            FbCommand oCommand = null;
            try
            {
                if (envio.Length > 3999) {
                    envio = envio.Substring(0, 3999);
                }            

                oConexion.Open();
                string InsertComand = "insert into QREPORT_LOG (OID, WS, DT, ENVIO) VALUES (gen_id(GENUID,1), @ws, @dt, @envio) RETURNING OID ";
                oCommand = new FbCommand(InsertComand, oConexion);
                oCommand.Parameters.Add("@ws", tipoMensaje);
                oCommand.Parameters.Add("@dt", DateTime.Now);
                oCommand.Parameters.Add("@envio", envio);
                //int result = (int)oCommand.ExecuteNonQuery();
                int oid = Convert.ToInt32(oCommand.ExecuteScalar());

                return oid;

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


        public static Boolean updateRespuesta(Int32 OID, String respuesta)
        {

            FbConnection oConexion = new FbConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ConexionBD"].ConnectionString);
            FbCommand oCommand = null;
            try
            {

                if (respuesta.Length > 3999)
                {
                    respuesta = respuesta.Substring(0, 3999);
                }

                oConexion.Open();
                string updateStament = "update QREPORT_LOG set RESPUESTA = @respuesta where OID=@OID ";
                oCommand = new FbCommand(updateStament, oConexion);
                oCommand.Parameters.Add("@respuesta", respuesta);
                oCommand.Parameters.Add("@OID", OID);
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


        public static Boolean updateEstado(Int32 OID, String estado)
        {

            FbConnection oConexion = new FbConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ConexionBD"].ConnectionString);
            FbCommand oCommand = null;
            try
            {
                oConexion.Open();
                string updateStament = "update QREPORT_LOG set ESTADO=@estado where OID=@OID ";
                oCommand = new FbCommand(updateStament, oConexion);
                oCommand.Parameters.Add("@estado", estado);
                oCommand.Parameters.Add("@OID", OID);
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