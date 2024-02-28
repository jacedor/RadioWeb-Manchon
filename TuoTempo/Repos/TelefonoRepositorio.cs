using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using FirebirdSql.Data.FirebirdClient;
using RadioWeb.Models;
using TuoTempo.Models;

namespace RadioWeb.Models.Repos
{
    public class TelefonoRepositorio
    {

        private static string QuotedString(string valor)
        {

            string retorno = "";
            if (!String.IsNullOrEmpty(valor))
            {
                retorno = "'" + valor.Replace("'", "''") + "'";
            }
            else
            {
                retorno = "''";
            }

            return retorno;


        }
        private static void FormatearDatosParaFireBird(TELEFONO oTelefono)
        {
            if (string.IsNullOrEmpty(oTelefono.LOCALIZACION))
            {
                oTelefono.LOCALIZACION = "''";
            }
            else
            {
                oTelefono.LOCALIZACION = QuotedString(oTelefono.LOCALIZACION);
            }
            if (string.IsNullOrEmpty(oTelefono.NUMERO))
            {
                oTelefono.NUMERO = "''";
            }
            else
            {
                oTelefono.NUMERO = QuotedString(oTelefono.NUMERO);
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
            string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["ConexionBD"].ConnectionString;

            using (FbConnection oConexion = new FbConnection(connectionString))
            using (FbCommand myCommand = new FbCommand())
            {
                try
                {
                    FormatearDatosParaFireBird(oTelefono);

                    string InsertTelefono = "INSERT INTO TELEFONO (OID, USERNAME,OWNER, numero, LOCALIZACION, IOR_TIPO) VALUES (gen_id(GENUID, 1), @OWNER, @NUMERO, @LOCALIZACION, @IOR_TIPO) RETURNING OID";

                    myCommand.Connection = oConexion;
                    myCommand.CommandText = InsertTelefono;

                    // Agregar parámetros
                    myCommand.Parameters.Add(new FbParameter("@OWNER", oTelefono.OWNER));
      
                    myCommand.Parameters.Add(new FbParameter("@NUMERO", oTelefono.NUMERO));
                    myCommand.Parameters.Add(new FbParameter("@LOCALIZACION", oTelefono.LOCALIZACION));
                    myCommand.Parameters.Add(new FbParameter("@IOR_TIPO", 3664933)); // Valor fijo

                    oConexion.Open();
                    return (int)myCommand.ExecuteScalar();
                }
                catch (Exception)
                {
                    throw;
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

       
       
    }
}