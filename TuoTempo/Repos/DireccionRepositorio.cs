using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using FirebirdSql.Data.FirebirdClient;
using RadioWeb.Models;


namespace TuoTempo.Models.Repos
{
    public class DireccionRepositorio
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

        private static void FormatearDatosParaFireBird(DIRECCION oDireccion)
        {
            if (string.IsNullOrEmpty(oDireccion.DIRECCION1))
            {
                oDireccion.DIRECCION1 = "''";
            }
            else
            {
                oDireccion.DIRECCION1 =QuotedString(oDireccion.DIRECCION1.ToUpper().Replace("'", "''"));
            }
            if (string.IsNullOrEmpty(oDireccion.CP))
            {
                oDireccion.CP = "''";
            }
            else
            {
                oDireccion.CP = QuotedString(oDireccion.CP);
            }
            if (string.IsNullOrEmpty(oDireccion.POBLACION))
            {
                oDireccion.POBLACION = "''";
            }
            else
            {
                oDireccion.POBLACION = QuotedString(oDireccion.POBLACION.ToUpper().Replace("'", "''"));
            }

            if (string.IsNullOrEmpty(oDireccion.PROVINCIA))
            {
                oDireccion.PROVINCIA = "''";

            }
            else
            {
                oDireccion.PROVINCIA = QuotedString(oDireccion.PROVINCIA.ToUpper().Replace("'", "''"));
            }

            if (string.IsNullOrEmpty(oDireccion.PAIS))
            {
                oDireccion.PAIS = "''";
            }
            else
            {
                oDireccion.PAIS = QuotedString(oDireccion.PAIS);
            }
        }

        public static DIRECCION Editar(DIRECCION oDireccion)
        {
            FbConnection oConexion = new FbConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ConexionBD"].ConnectionString);
            
                FormatearDatosParaFireBird(oDireccion);
                oConexion.Open();
                FbCommand oCommand = new FbCommand("update  DIRECCION  set direccion = " + oDireccion.DIRECCION1 +                   
                    ", POBLACION=" + oDireccion.POBLACION + ", CP=" + oDireccion.CP + ", PROVINCIA=" + oDireccion.PROVINCIA + ", PAIS=" + oDireccion.PAIS + 
                    " WHERE OID = " + oDireccion.OID, oConexion);
            try
            {   
                int result = (int)oCommand.ExecuteNonQuery();

                oDireccion.OID = result;
                return oDireccion;
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

        public static int Insertar(DIRECCION oDireccion)
        {
            string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["ConexionBD"].ConnectionString;

            using (FbConnection oConexion = new FbConnection(connectionString))
            using (FbCommand myCommand = new FbCommand())
            {
                try
                {
                    FormatearDatosParaFireBird(oDireccion);
                    string InsertDireccion = "INSERT INTO DIRECCION (OID, OWNER, DIRECCION, CP, POBLACION, PROVINCIA, PAIS, IOR_TIPO) values (gen_id(GENUID,1), ";
                    InsertDireccion += "@OWNER, @DIRECCION, @CP, @POBLACION, @PROVINCIA,@USERNAME, @PAIS, 0) returning OID";

                    myCommand.Connection = oConexion;
                    myCommand.CommandText = InsertDireccion;

                    // Agregar parámetros
                    myCommand.Parameters.Add(new FbParameter("@USERNAME", "TUOTEMPO"));
                    myCommand.Parameters.Add(new FbParameter("@OWNER", oDireccion.OWNER));
                    myCommand.Parameters.Add(new FbParameter("@DIRECCION", oDireccion.DIRECCION1));
                    myCommand.Parameters.Add(new FbParameter("@CP", oDireccion.CP));
                    myCommand.Parameters.Add(new FbParameter("@POBLACION", oDireccion.POBLACION.Substring(0, Math.Min(23, oDireccion.POBLACION.Length))));
                    myCommand.Parameters.Add(new FbParameter("@PROVINCIA", oDireccion.PROVINCIA.Substring(0, Math.Min(15, oDireccion.PROVINCIA.Length))));
                    myCommand.Parameters.Add(new FbParameter("@PAIS", oDireccion.PAIS));

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
                 oCommand = new FbCommand("DELETE FROM DIRECCION WHERE OID=" + OID, oConexion);
                int result = oCommand.ExecuteNonQuery();
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