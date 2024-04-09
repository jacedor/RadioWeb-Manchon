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

    public class ImagenesRepositorio
    {



        public static string Insertar(IMAGENES oImagen, bool peticiones = false)
        {
            FbConnection oConexion = new FbConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ConexionBD"].ConnectionString);

            FbCommand oCommand = null;


            string InsertComand;
            try
            {
                //El owner de la imagen es el tipo de imagen. Por defecto ponemos que es una imagen obtenida de la Webcam.
                //Pero si es un documento asociado a la exploracion tenemos que cambiar este valor.
                int ownerImagen = 12;

                if (oImagen.OWNER.HasValue)
                {
                    ownerImagen = oImagen.OWNER.Value;
                }

                InsertComand = "insert into imagenes (OID,GRUPO,IOR_PACIENTE,FECHA,OWNER, PATH, EXT, IOR_EXPLORACION,USERNAME)";

                InsertComand += " VALUES (gen_id(GENUID,1),'F', " + oImagen.IOR_PACIENTE + ",'" + DateTime.Now.ToString("MM-dd-yyyy HH:mm") + "'," + ownerImagen.ToString() + ","
                    + DataBase.QuotedString(oImagen.PATH) + ",'" + oImagen.EXT.Replace(".", "") + "'," + oImagen.IOR_EXPLORACION + ",";
                InsertComand += HttpContext.Current.User.Identity.Name.QuotedString() + " ) returning OID";
                InsertComand = InsertComand.Replace("''", "null");
                InsertComand = InsertComand.Replace(",,", ",null,");

                oConexion.Open();
                oCommand = new FbCommand(InsertComand, oConexion);
                string result = ((int)oCommand.ExecuteScalar()).ToString();
                if (peticiones)
                {
                    oImagen.NOMBRE = result;
                }
                else
                {
                    //Si el objeto imagene nombre tiene algun valor ponemos dicho valor en la base de datos, sino nombramos la 
                    //Imagen con el OID recién insertado
                    if (String.IsNullOrEmpty(oImagen.NOMBRE))
                    {
                        oImagen.NOMBRE = result + "01";
                    }
                    else
                    {
                        oImagen.NOMBRE = oImagen.NOMBRE + "_" + result;
                    }
                }

                string UpdateCommand = "update imagenes set nombre='" + oImagen.NOMBRE + "' where oid = " + result;
                FbCommand oUpdateComand = new FbCommand(UpdateCommand, oConexion);
                oUpdateComand.ExecuteNonQuery();

                return oImagen.NOMBRE;

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

        public static int UpdateCampo(string campo, string valor, int oid)
        {


            FbConnection oConexion = new FbConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ConexionBD"].ConnectionString);
            FbCommand oCommand = null;
            try
            {
                oConexion.Open();
                string updateStament = "update imagenes set " + campo + "='" + valor + "'";
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

            return oid;

        }

        public static List<REFRACTOMETROS> ObtenerTipos(int tipo = 6)
        {
            FbConnection oConexion = new FbConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ConexionBD"].ConnectionString);
            oConexion.Open();
            List<REFRACTOMETROS> tiposDocumento = new List<REFRACTOMETROS>();

            FbCommand oCommand = null;

            try
            {
                string query = "select * from refractometros where cid= " + tipo;

                oCommand = new FbCommand(query, oConexion);

                FbDataReader oReader = oCommand.ExecuteReader();


                while (oReader.Read())
                {
                    REFRACTOMETROS oTipoDocumento = new REFRACTOMETROS();
                    oTipoDocumento.OID = DataBase.GetIntFromReader(oReader, "OID");
                    oTipoDocumento.NOMBRE = DataBase.GetStringFromReader(oReader, "NOMBRE");


                    tiposDocumento.Add(oTipoDocumento);
                }
                return tiposDocumento;
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

        public static List<IMAGENES> ObtenerPorPaciente(int oidPaciente)
        {

            FbConnection oConexion = new FbConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ConexionBD"].ConnectionString);
            oConexion.Open();
            List<IMAGENES> oImagenes = new List<IMAGENES>();

            FbCommand oCommand = null;

            try
            {
                string query = "select r.oid,r.descrip,r.username,r.cid,r.owner,r.ext,r.fecha,r.ior_paciente,r.path," +
                    "r.nombre,r.ior_exploracion,a.nombre tipo, a.CANAL VIDSIGNER " +
                    "from imagenes r left join refractometros a on  a.oid=r.owner where r.ior_paciente=" + oidPaciente;            


                query = query + " and ( not (r.BORRADO='T') or r.BORRADO is null) order by r.fecha desc";
                oCommand = new FbCommand(query, oConexion);

                FbDataReader oReader = oCommand.ExecuteReader();


                while (oReader.Read())
                {
                    IMAGENES oImagen = new IMAGENES();
                    oImagen.OID = DataBase.GetIntFromReader(oReader, "OID");
                    oImagen.CID = DataBase.GetIntFromReader(oReader, "CID");
                    oImagen.OWNER = DataBase.GetIntFromReader(oReader, "OWNER");
                    oImagen.EXT = DataBase.GetStringFromReader(oReader, "EXT");
                    oImagen.USERNAME = DataBase.GetStringFromReader(oReader, "USERNAME");
                    oImagen.FECHA = DataBase.GetDateTimeFromReader(oReader, "FECHA");
                    oImagen.TIPO = DataBase.GetStringFromReader(oReader, "TIPO");
                    oImagen.IOR_PACIENTE = DataBase.GetIntFromReader(oReader, "IOR_PACIENTE");
                    oImagen.PATH = DataBase.GetStringFromReader(oReader, "PATH");
                    oImagen.NOMBRE = DataBase.GetStringFromReader(oReader, "NOMBRE");
                    oImagen.FIRMABLEENTABLET = (DataBase.GetStringFromReader(oReader, "VIDSIGNER") == "1" ? true : false);
                    oImagen.IOR_EXPLORACION = DataBase.GetIntFromReader(oReader, "IOR_EXPLORACION");
                    oImagen.RUTACOMPLETA = oImagen.PATH + oImagen.NOMBRE + "." + oImagen.EXT;
                    oImagenes.Add(oImagen);
                }
                return oImagenes;
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

        public static List<IMAGENES> Obtener(int oidExploracion, int tipoDocumento )
        {

            FbConnection oConexion = new FbConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ConexionBD"].ConnectionString);
            oConexion.Open();
            List<IMAGENES> oImagenes = new List<IMAGENES>();

            FbCommand oCommand = null;

            try
            {
                string query = "select r.oid,r.descrip,r.username,r.cid,r.owner,r.ext,r.fecha,r.ior_paciente,r.path," +
                    "r.nombre,r.ior_exploracion,a.nombre tipo, a.CANAL VIDSIGNER " +
                    "from imagenes r left join refractometros a on  a.oid=r.owner where ior_exploracion=" + oidExploracion;
               
                    query = query + " and r.owner = " + tipoDocumento;
                

                query = query + " and ( not (r.BORRADO='T') or r.BORRADO is null) order by r.fecha desc";
                oCommand = new FbCommand(query, oConexion);

                FbDataReader oReader = oCommand.ExecuteReader();


                while (oReader.Read())
                {
                    IMAGENES oImagen = new IMAGENES();
                    oImagen.OID = DataBase.GetIntFromReader(oReader, "OID");
                    oImagen.CID = DataBase.GetIntFromReader(oReader, "CID");
                    oImagen.OWNER = DataBase.GetIntFromReader(oReader, "OWNER");
                    oImagen.EXT = DataBase.GetStringFromReader(oReader, "EXT");
                    oImagen.USERNAME = DataBase.GetStringFromReader(oReader, "USERNAME");
                    oImagen.FECHA = DataBase.GetDateTimeFromReader(oReader, "FECHA");
                    oImagen.TIPO = DataBase.GetStringFromReader(oReader, "TIPO");
                    oImagen.IOR_PACIENTE = DataBase.GetIntFromReader(oReader, "IOR_PACIENTE");
                    oImagen.PATH = DataBase.GetStringFromReader(oReader, "PATH");
                    oImagen.NOMBRE = DataBase.GetStringFromReader(oReader, "NOMBRE");
                    oImagen.FIRMABLEENTABLET = (DataBase.GetStringFromReader(oReader, "VIDSIGNER") == "1" ? true : false);
                    oImagen.IOR_EXPLORACION = DataBase.GetIntFromReader(oReader, "IOR_EXPLORACION");
                    oImagen.RUTACOMPLETA = oImagen.PATH + oImagen.NOMBRE + "." + oImagen.EXT;
                    oImagenes.Add(oImagen);
                }
                return oImagenes;
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


        public static List<IMAGENES> Obtener(int oidExploracion, bool documentos = false)
        {

            FbConnection oConexion = new FbConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ConexionBD"].ConnectionString);
            oConexion.Open();
            List<IMAGENES> oImagenes = new List<IMAGENES>();

            FbCommand oCommand = null;

            try
            {
                string query = "select r.oid,r.descrip,r.cid,r.owner,r.ext,r.fecha,r.ior_paciente,r.path," +
                    "r.nombre,r.ior_exploracion,a.nombre tipo, a.CANAL VIDSIGNER " +
                    "from imagenes r left join refractometros a on  a.oid=r.owner where ior_exploracion=" + oidExploracion ;
                if (documentos)
                {
                    query = query + " and r.owner is not null";
                }
                else
                {
                    query = query + " and r.owner = 12";
                }

                query = query + " and ( not (r.BORRADO='T') or r.BORRADO is null) order by r.fecha desc";
                oCommand = new FbCommand(query, oConexion);

                FbDataReader oReader = oCommand.ExecuteReader();


                while (oReader.Read())
                {
                    IMAGENES oImagen = new IMAGENES();
                    oImagen.OID = DataBase.GetIntFromReader(oReader, "OID");
                    oImagen.CID = DataBase.GetIntFromReader(oReader, "CID");
                    oImagen.OWNER = DataBase.GetIntFromReader(oReader, "OWNER");
                    oImagen.EXT = DataBase.GetStringFromReader(oReader, "EXT");
                    oImagen.FECHA = DataBase.GetDateTimeFromReader(oReader, "FECHA");
                    oImagen.TIPO = DataBase.GetStringFromReader(oReader, "TIPO");
                    oImagen.IOR_PACIENTE = DataBase.GetIntFromReader(oReader, "IOR_PACIENTE");
                    oImagen.PATH = DataBase.GetStringFromReader(oReader, "PATH");
                    oImagen.NOMBRE = DataBase.GetStringFromReader(oReader, "NOMBRE");
                    oImagen.FIRMABLEENTABLET = (DataBase.GetStringFromReader(oReader, "VIDSIGNER") == "1" ? true : false);
                    oImagen.IOR_EXPLORACION = DataBase.GetIntFromReader(oReader, "IOR_EXPLORACION");
                    oImagen.RUTACOMPLETA = oImagen.PATH + oImagen.NOMBRE + "." + oImagen.EXT;
                    oImagenes.Add(oImagen);
                }
                return oImagenes;
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


        public static IMAGENES Obtener(int oid)
        {

            FbConnection oConexion = new FbConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ConexionBD"].ConnectionString);
            oConexion.Open();
            IMAGENES oResult = new IMAGENES();

            FbCommand oCommand = null;

            try
            {
                string query = "select r.oid,r.owner,r.ext,r.fecha,r.ior_paciente,r.path," +
                    "r.nombre,r.ior_exploracion,a.nombre tipo, a.CANAL VIDSIGNER " +
                    "from imagenes r left join refractometros a on  a.oid=r.owner where r.oid=" + oid;

                oCommand = new FbCommand(query, oConexion);
                oResult.OID = -1;
                FbDataReader oReader = oCommand.ExecuteReader();


                while (oReader.Read())
                {

                    oResult.OID = DataBase.GetIntFromReader(oReader, "OID");
                    oResult.OWNER = DataBase.GetIntFromReader(oReader, "OWNER");
                    oResult.EXT = DataBase.GetStringFromReader(oReader, "EXT");
                    oResult.FECHA = DataBase.GetDateTimeFromReader(oReader, "FECHA");
                    oResult.TIPO = DataBase.GetStringFromReader(oReader, "TIPO");
                    oResult.IOR_PACIENTE = DataBase.GetIntFromReader(oReader, "IOR_PACIENTE");
                    oResult.PATH = DataBase.GetStringFromReader(oReader, "PATH");
                    oResult.NOMBRE = DataBase.GetStringFromReader(oReader, "NOMBRE");
                    oResult.FIRMABLEENTABLET = (DataBase.GetStringFromReader(oReader, "VIDSIGNER") == "1" ? true : false);
                    oResult.IOR_EXPLORACION = DataBase.GetIntFromReader(oReader, "IOR_EXPLORACION");
                    oResult.RUTACOMPLETA = oResult.PATH + oResult.NOMBRE + "." + oResult.EXT;


                }
                return oResult;
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



        public static IMAGENES Obtener(string guid)
        {

            FbConnection oConexion = new FbConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ConexionBD"].ConnectionString);
            oConexion.Open();
            IMAGENES oResult = new IMAGENES();

            FbCommand oCommand = null;

            try
            {
                string query = "select r.oid,r.owner,r.ext,r.fecha,r.ior_paciente,r.path," +
                    "r.nombre,r.ior_exploracion,a.nombre tipo, a.CANAL VIDSIGNER " +
                    "from imagenes r left join refractometros a on  a.oid=r.owner where r.descrip=" + guid.QuotedString();

                oCommand = new FbCommand(query, oConexion);
                oResult.OID = -1;
                FbDataReader oReader = oCommand.ExecuteReader();


                while (oReader.Read())
                {

                    oResult.OID = DataBase.GetIntFromReader(oReader, "OID");
                    oResult.OWNER = DataBase.GetIntFromReader(oReader, "OWNER");
                    oResult.EXT = DataBase.GetStringFromReader(oReader, "EXT");
                    oResult.FECHA = DataBase.GetDateTimeFromReader(oReader, "FECHA");
                    oResult.TIPO = DataBase.GetStringFromReader(oReader, "TIPO");
                    oResult.IOR_PACIENTE = DataBase.GetIntFromReader(oReader, "IOR_PACIENTE");
                    oResult.PATH = DataBase.GetStringFromReader(oReader, "PATH");
                    oResult.NOMBRE = DataBase.GetStringFromReader(oReader, "NOMBRE");
                    oResult.FIRMABLEENTABLET = (DataBase.GetStringFromReader(oReader, "VIDSIGNER") == "1" ? true : false);
                    oResult.IOR_EXPLORACION = DataBase.GetIntFromReader(oReader, "IOR_EXPLORACION");
                    oResult.RUTACOMPLETA = oResult.PATH + oResult.NOMBRE + "." + oResult.EXT;


                }
                return oResult;
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