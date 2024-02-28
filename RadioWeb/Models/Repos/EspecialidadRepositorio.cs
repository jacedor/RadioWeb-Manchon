using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using FirebirdSql.Data.FirebirdClient;
using RadioWeb.Models;
using RadioWeb.Utils;

namespace RadioWeb.Models.Repos
{
    public class EspecialidadRepositorio
    {

        public static ESPECIALIDADES Obtener(int OID_Especialidad)
        {
            FbConnection oConexion = new FbConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ConexionBD"].ConnectionString);

           
                oConexion.Open();
                FbCommand oCommand = new FbCommand("select * FROM ESPECIALIDADES WHERE OID=" + OID_Especialidad, oConexion);
                FbDataReader oReader = oCommand.ExecuteReader();
                ESPECIALIDADES OEspecialidad = new ESPECIALIDADES();
            try
            {
                while (oReader.Read())
                {


                    OEspecialidad.OID = DataBase.GetIntFromReader(oReader, "OID");
                    OEspecialidad.DESCRIPCION = DataBase.GetStringFromReader(oReader, "DESCRIPCION");
                    OEspecialidad.BORRADO = DataBase.GetStringFromReader(oReader, "BORRADO");
                    OEspecialidad.CANAL = DataBase.GetStringFromReader(oReader, "CANAL");
                    OEspecialidad.CID = DataBase.GetIntFromReader(oReader, "CID");
                    OEspecialidad.MODIF = DataBase.GetDateTimeFromReader(oReader, "MODIF");
                    OEspecialidad.OWNER = DataBase.GetIntFromReader(oReader, "OWNER");
                    OEspecialidad.USERNAME = DataBase.GetStringFromReader(oReader, "USERNAME");

                }


                return OEspecialidad;
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

        public static List<ESPECIALIDADES> Lista()
        {
            FbConnection oConexion = new FbConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ConexionBD"].ConnectionString);

           
                oConexion.Open();
                FbCommand oCommand = new FbCommand("select * FROM ESPECIALIDADES ORDER BY DESCRIPCION", oConexion);
                FbDataReader oReader = oCommand.ExecuteReader();
                List<ESPECIALIDADES> oListaResult = new List<ESPECIALIDADES>();

                ESPECIALIDADES OEspecialidad = new ESPECIALIDADES();
                OEspecialidad.DESCRIPCION = "NO ASIGNADO";
                OEspecialidad.OID = -1;
                oListaResult.Add(OEspecialidad);
            try
            {
                while (oReader.Read())
                {
                    OEspecialidad = new ESPECIALIDADES();
                    OEspecialidad.OID = DataBase.GetIntFromReader(oReader, "OID");
                    OEspecialidad.DESCRIPCION = DataBase.GetStringFromReader(oReader, "DESCRIPCION");                    
                    OEspecialidad.BORRADO = DataBase.GetStringFromReader(oReader, "BORRADO");
                    OEspecialidad.CANAL = DataBase.GetStringFromReader(oReader, "CANAL");
                    OEspecialidad.CID = DataBase.GetIntFromReader(oReader, "CID");
                    OEspecialidad.MODIF = DataBase.GetDateTimeFromReader(oReader, "MODIF");                   
                    OEspecialidad.OWNER = DataBase.GetIntFromReader(oReader, "OWNER");       
                    OEspecialidad.USERNAME = DataBase.GetStringFromReader(oReader, "USERNAME");

                    oListaResult.Add(OEspecialidad);


                }


                return oListaResult;
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