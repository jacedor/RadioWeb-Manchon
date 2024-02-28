using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using FirebirdSql.Data.FirebirdClient;
using RadioWeb.Models;
using RadioWeb.Utils;

namespace RadioWeb.Models.Repos
{
    public class DireccionRepositorio
    {

        private static void FormatearDatosParaFireBird(DIRECCION oDireccion)
        {
            if (string.IsNullOrEmpty(oDireccion.DIRECCION1))
            {
                oDireccion.DIRECCION1 = "''";
            }
            else
            {
                oDireccion.DIRECCION1 = DataBase.QuotedString(oDireccion.DIRECCION1.ToUpper().Replace("'", "''"));
            }
            if (string.IsNullOrEmpty(oDireccion.CP))
            {
                oDireccion.CP = "''";
            }
            else
            {
                oDireccion.CP = DataBase.QuotedString(oDireccion.CP);
            }
            if (string.IsNullOrEmpty(oDireccion.POBLACION))
            {
                oDireccion.POBLACION = "''";
            }
            else
            {
                oDireccion.POBLACION = DataBase.QuotedString(oDireccion.POBLACION.ToUpper().Replace("'", "''"));
            }

            if (string.IsNullOrEmpty(oDireccion.PROVINCIA))
            {
                oDireccion.PROVINCIA = "''";

            }
            else
            {
                oDireccion.PROVINCIA = DataBase.QuotedString(oDireccion.PROVINCIA.ToUpper().Replace("'", "''"));
            }

            if (string.IsNullOrEmpty(oDireccion.PAIS))
            {
                oDireccion.PAIS = "''";
            }
            else
            {
                oDireccion.PAIS = DataBase.QuotedString(oDireccion.PAIS);
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
            FbConnection oConexion = new FbConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ConexionBD"].ConnectionString);
              FbCommand myCommand = new FbCommand();
            try
            {

                FormatearDatosParaFireBird(oDireccion);
                string InsertDireccion = "INSERT INTO DIRECCION  (OID,OWNER,DIRECCION,CP,POBLACION,PROVINCIA,PAIS,IOR_TIPO) values (gen_id(GENUID,1), ";               

                InsertDireccion += oDireccion.OWNER + "," + oDireccion.DIRECCION1;
                InsertDireccion += "," + oDireccion.CP;
                if (oDireccion.POBLACION.Length > 22) {
                    oDireccion.POBLACION=oDireccion.POBLACION.Substring(0, 23); 
                }
                if (oDireccion.PROVINCIA.Length > 15)
                {
                    oDireccion.PROVINCIA = oDireccion.PROVINCIA.Substring(0, 15);
                }
                InsertDireccion += "," + oDireccion.POBLACION;
                InsertDireccion += "," + oDireccion.PROVINCIA;
                InsertDireccion += "," + oDireccion.PAIS;
                InsertDireccion += ",0" ;

                InsertDireccion += ") returning OID";

                oConexion.Open();
              
                myCommand.Connection = oConexion;
                myCommand.CommandText = InsertDireccion;
                return (int)myCommand.ExecuteScalar();

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
                    if (myCommand != null)
                    {
                        myCommand.Dispose();
                    }
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
        public static List<DIRECCION> Obtener(int Oid_Paciente)
        {
            FbConnection oConexion = new FbConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ConexionBD"].ConnectionString);

           
                oConexion.Open();
                FbCommand oCommand = new FbCommand("select * FROM DIRECCION WHERE OWNER=" + Oid_Paciente, oConexion);
                FbDataReader oReader = oCommand.ExecuteReader();
                List<DIRECCION> lDireccion = new List<DIRECCION>();
            try
            {
                while (oReader.Read())
                {
                    DIRECCION oDireccion = new DIRECCION();

                    oDireccion.OID = DataBase.GetIntFromReader(oReader, "OID");
                    oDireccion.DIRECCION1 = DataBase.GetStringFromReader(oReader, "DIRECCION");
                    oDireccion.CP = DataBase.GetStringFromReader(oReader, "CP");
                    oDireccion.BORRADO = DataBase.GetStringFromReader(oReader, "BORRADO");
                    oDireccion.CANAL = DataBase.GetStringFromReader(oReader, "CANAL");
                    oDireccion.CID = DataBase.GetIntFromReader(oReader, "CID");
                    oDireccion.IOR_EMPRESA = DataBase.GetIntFromReader(oReader, "IOR_EMPRESA");
                    oDireccion.IOR_TIPO = DataBase.GetIntFromReader(oReader, "IOR_TIPO");
                    oDireccion.OWNER = DataBase.GetIntFromReader(oReader, "OWNER");
                    
                    oDireccion.PAIS = DataBase.GetStringFromReader(oReader, "PAIS");
                    oDireccion.POBLACION = DataBase.GetStringFromReader(oReader, "POBLACION");
                    oDireccion.PROVINCIA = DataBase.GetStringFromReader(oReader, "PROVINCIA");
                    oDireccion.USERNAME = DataBase.GetStringFromReader(oReader, "USERNAME");
                    lDireccion.Add(oDireccion);
                }


                return lDireccion;
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


        public static void UpdateCampo(string campo, string valor, int oid)
        {

           
            FbConnection oConexion = new FbConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ConexionBD"].ConnectionString);
            FbCommand oCommand = null;
            try
            {
                oConexion.Open();
                string updateStament = "update DIRECCION set " + campo + "='" + valor.ToUpper() + "'";
                updateStament += " where oid= " + oid;
                oCommand = new FbCommand(updateStament, oConexion);
                oCommand.ExecuteNonQuery();
            }
            catch (Exception)
            {


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


        public static DIRECCION Get(int Oid_Direccion)
        {
            FbConnection oConexion = new FbConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ConexionBD"].ConnectionString);

            
                oConexion.Open();
                FbCommand oCommand = new FbCommand("select * FROM DIRECCION WHERE oid=" + Oid_Direccion, oConexion);
                FbDataReader oReader = oCommand.ExecuteReader();
            try
            {
                oReader.Read();
               
                    DIRECCION oDireccion = new DIRECCION();

                    oDireccion.OID = DataBase.GetIntFromReader(oReader, "OID");
                    oDireccion.DIRECCION1 = DataBase.GetStringFromReader(oReader, "DIRECCION");
                    oDireccion.CP = DataBase.GetStringFromReader(oReader, "CP");
                    oDireccion.BORRADO = DataBase.GetStringFromReader(oReader, "BORRADO");
                    oDireccion.CANAL = DataBase.GetStringFromReader(oReader, "CANAL");
                    oDireccion.CID = DataBase.GetIntFromReader(oReader, "CID");
                    oDireccion.IOR_EMPRESA = DataBase.GetIntFromReader(oReader, "IOR_EMPRESA");
                    oDireccion.IOR_TIPO = DataBase.GetIntFromReader(oReader, "IOR_TIPO");
                    oDireccion.OWNER = DataBase.GetIntFromReader(oReader, "OWNER");

                    oDireccion.PAIS = DataBase.GetStringFromReader(oReader, "PAIS");
                    oDireccion.POBLACION = DataBase.GetStringFromReader(oReader, "POBLACION");
                    oDireccion.PROVINCIA = DataBase.GetStringFromReader(oReader, "PROVINCIA");
                    oDireccion.USERNAME = DataBase.GetStringFromReader(oReader, "USERNAME");



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
    
    }
}