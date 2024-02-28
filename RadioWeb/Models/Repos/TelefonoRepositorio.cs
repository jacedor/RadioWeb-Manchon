using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using FirebirdSql.Data.FirebirdClient;
using RadioWeb.Models;
using RadioWeb.Utils;

namespace RadioWeb.Models.Repos
{
    public class TelefonoRepositorio
    {

        private static void FormatearDatosParaFireBird(TELEFONO oTelefono)
        {
            if (string.IsNullOrEmpty(oTelefono.LOCALIZACION))
            {
                oTelefono.LOCALIZACION = "''";
            }
            else
            {
                oTelefono.LOCALIZACION = DataBase.QuotedString(oTelefono.LOCALIZACION);
            }
            if (string.IsNullOrEmpty(oTelefono.NUMERO))
            {
                oTelefono.NUMERO = "''";
            }
            else
            {
                oTelefono.NUMERO = DataBase.QuotedString(oTelefono.NUMERO);
            }
           
        }

        public static TELEFONO Editar(TELEFONO oTelefono)
        {
            FbConnection oConexion = new FbConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ConexionBD"].ConnectionString);
            FbCommand oCommand = null;
            try
            {
                FormatearDatosParaFireBird(oTelefono);
                oConexion.Open();
                 oCommand = new FbCommand("update  TELEFONO  set LOCALIZACION = " + oTelefono.LOCALIZACION + 
                     ", NUMERO=" + oTelefono.NUMERO  + " WHERE OID = " + oTelefono.OID, oConexion);

                int result = (int)oCommand.ExecuteNonQuery();

                oTelefono.OID = result;
                return oTelefono;
            }
            catch (Exception)
            {

                throw;
            }
            finally
            {
                oConexion.Close();
                if (oCommand != null)
                {
                    oCommand.Dispose();
                }               
            }
        }

        public static int Insertar(TELEFONO oTelefono)
        {
            FbConnection oConexion = new FbConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ConexionBD"].ConnectionString);
            FbCommand myCommand = null;
            try
            {

                FormatearDatosParaFireBird(oTelefono);
                    string InsertTelefono = "INSERT INTO TELEFONO  (OID,OWNER,numero,LOCALIZACION,IOR_TIPO) values (gen_id(GENUID,1), ";

                    InsertTelefono += oTelefono.OWNER + "," + oTelefono.NUMERO;
                    InsertTelefono += "," + oTelefono.LOCALIZACION;
                    InsertTelefono += ",3664933" ;
                InsertTelefono += ") returning OID";

                    oConexion.Open();
                     myCommand = new FbCommand();
                    myCommand.Connection = oConexion;
                    myCommand.CommandText = InsertTelefono;
                return (int)myCommand.ExecuteScalar();

            }
            catch (Exception)
            {

                throw;
            }
            finally
            {
                oConexion.Close();
                if (myCommand != null)
                {
                    myCommand.Dispose();
                }

            }
        }

        public static void Delete(string OID)
        {
            FbConnection oConexion = new FbConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ConexionBD"].ConnectionString);
            FbCommand oCommand = null;
            try
            {
                oConexion.Open();
                 oCommand = new FbCommand("DELETE FROM TELEFONO WHERE OID=" + OID, oConexion);
                int result = oCommand.ExecuteNonQuery();
            }
            catch (Exception)
            {

                throw;
            }
            finally
            {
                oConexion.Close();
                if (oCommand != null)
                {
                    oCommand.Dispose();
                }
            }
        }

        public static TELEFONO Get(int oidTelefono)
        {
            FbConnection oConexion = new FbConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ConexionBD"].ConnectionString);
            FbCommand oCommand = null;
            try
            {
                oConexion.Open();
                 oCommand = new FbCommand("select * FROM TELEFONO WHERE oid=" + oidTelefono, oConexion);
                FbDataReader oReader = oCommand.ExecuteReader();
                
                TELEFONO oTelefono = new TELEFONO();

                while (oReader.Read())
                {
                 
                    oTelefono.OID = DataBase.GetIntFromReader(oReader, "OID");
                    oTelefono.TELEFONO1 = DataBase.GetStringFromReader(oReader, "TELEFONO");
                    oTelefono.PREFIJO = DataBase.GetShortFromReader(oReader, "PREFIJO");
                    oTelefono.BORRADO = DataBase.GetStringFromReader(oReader, "BORRADO");
                    oTelefono.CANAL = DataBase.GetStringFromReader(oReader, "CANAL");
                    oTelefono.CID = DataBase.GetIntFromReader(oReader, "CID");
                    oTelefono.IOR_EMPRESA = DataBase.GetIntFromReader(oReader, "IOR_EMPRESA");
                    oTelefono.IOR_TIPO = DataBase.GetIntFromReader(oReader, "IOR_TIPO");
                    oTelefono.LOCALIZACION = DataBase.GetStringFromReader(oReader, "LOCALIZACION");
                    oTelefono.OWNER = DataBase.GetIntFromReader(oReader, "OWNER");
                    oTelefono.NUMERO = DataBase.GetStringFromReader(oReader, "NUMERO");

                  
                }


                return oTelefono;
            }
            catch (Exception)
            {

                throw;
            }
            finally
            {
                oConexion.Close();
                if (oCommand != null)
                {
                    oCommand.Dispose();
                }
            }

        }


        public static List<TELEFONO> Obtener(int Oid_Paciente)
        {
            FbConnection oConexion = new FbConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ConexionBD"].ConnectionString);
            FbCommand oCommand = null;
            try
            {
                oConexion.Open();
                oCommand = new FbCommand("select * FROM TELEFONO WHERE OWNER=" + Oid_Paciente, oConexion);
                FbDataReader oReader = oCommand.ExecuteReader();
                List<TELEFONO> lTelefonos = new List<TELEFONO>();

                while (oReader.Read())
                {
                    TELEFONO oTelefono = new TELEFONO();

                    oTelefono.OID = DataBase.GetIntFromReader(oReader, "OID");
                    oTelefono.TELEFONO1 = DataBase.GetStringFromReader(oReader, "TELEFONO");
                    oTelefono.PREFIJO = DataBase.GetShortFromReader(oReader, "PREFIJO");
                    oTelefono.BORRADO = DataBase.GetStringFromReader(oReader, "BORRADO");
                    oTelefono.CANAL = DataBase.GetStringFromReader(oReader, "CANAL");
                    oTelefono.CID = DataBase.GetIntFromReader(oReader, "CID");
                    oTelefono.IOR_EMPRESA = DataBase.GetIntFromReader(oReader, "IOR_EMPRESA");
                    oTelefono.IOR_TIPO = DataBase.GetIntFromReader(oReader, "IOR_TIPO");
                    oTelefono.LOCALIZACION = DataBase.GetStringFromReader(oReader, "LOCALIZACION");
                    oTelefono.OWNER = DataBase.GetIntFromReader(oReader, "OWNER");
                    oTelefono.NUMERO = DataBase.GetStringFromReader(oReader, "NUMERO").Replace(" ","");

                    lTelefonos.Add(oTelefono);
                }


                return lTelefonos;
            }
            catch (Exception)
            {

                throw;
            }
            finally
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