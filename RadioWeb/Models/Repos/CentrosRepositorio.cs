using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using FirebirdSql.Data.FirebirdClient;
using RadioWeb.Models;
using RadioWeb.Utils;

namespace RadioWeb.Models.Repos
{
    public class CentrosRepositorio
    {

        public static List<CENTROS> ObtenerPorGrupoAparato(string grupoAparato)
        {
            FbConnection oConexion = new FbConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ConexionBD"].ConnectionString);
            oConexion.Open();
            string query="select distinct d.cid,c.NOMBRE,g.DES_GRUP,g.COD_GRUP from DAPARATOS d join CENTROS c on c.OID=d.CID join GAPARATOS g on g.OID=d.OWNER ";
            query += " where g.COD_GRUP='" + grupoAparato + "' and";
            query += " d.IOR_EMPRESA=4 and d.VERS=1 and g.OID<>10 and g.OID<>100 and d.CID<>2 and d.CID<>7  order by g.des_grup";
            List<CENTROS> oCentroResult = new List<CENTROS>();
            FbCommand oCommand = new FbCommand(query, oConexion);
            try
            {
               
                FbDataReader oReader = oCommand.ExecuteReader();
                

                while (oReader.Read())
                {
                    CENTROS oTempCentro = new CENTROS();
                    oTempCentro.NOMBRE = DataBase.GetStringFromReader(oReader, "NOMBRE").ToUpper().Trim().Replace(" ", "");
                    switch (oTempCentro.NOMBRE)
                    {
                        case "BALMES125":
                            oTempCentro.NOMBRE = "BALMES";
                            break;
                        case "SANTCUGAT":
                            oTempCentro.NOMBRE = "CUGAT";
                            break;
                        default:
                            break;
                    }
                    oCentroResult.Add(oTempCentro);
                }
            }
            catch (Exception)
            {


            }
            finally {
                if (oConexion.State == System.Data.ConnectionState.Open)
                {        

                    oConexion.Close();
                    if (oCommand != null)
                    {
                        oCommand.Dispose();
                    }
                
                }
            }
           

           
            return oCentroResult;
        }

        public static List<CENTROS> Obtener()
        {
            FbConnection oConexion = new FbConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ConexionBD"].ConnectionString);
            oConexion.Open();

            FbCommand oCommand = new FbCommand("select * from centros where oid>0 ", oConexion);
            FbDataReader oReader = oCommand.ExecuteReader();
            List<CENTROS> oCentroResult = new List<CENTROS>();
            try
            {
                while (oReader.Read())
                {
                    CENTROS oTempCentro = new CENTROS();
                    oTempCentro.NOMBRE = DataBase.GetStringFromReader(oReader, "NOMBRE");
                    oTempCentro.OID = DataBase.GetIntFromReader(oReader, "OID");

                    oCentroResult.Add(oTempCentro);
                }

            }
            catch (Exception)
            {

                throw;
            }
           
            finally
            {
                if (oConexion.State == System.Data.ConnectionState.Open) {
                    oConexion.Close();
                    if (oCommand != null)
                    {
                        oCommand.Dispose();
                    }
                }
                 
                
            }

           
           
            return oCentroResult;
        }

        public static List<CENTROS> Obtener(string CentroName)
        {
            FbConnection oConexion = new FbConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ConexionBD"].ConnectionString);
            oConexion.Open();

            FbCommand oCommand = new FbCommand("select * from centros where NOMBRE= " + DataBase.QuotedString( CentroName), oConexion);
            FbDataReader oReader = oCommand.ExecuteReader();
        List<CENTROS> oCentroResult = new  List<CENTROS> ();
            try
            {
                while (oReader.Read())
                {
                    CENTROS oTempCentro = new CENTROS();
                    oTempCentro.NOMBRE = DataBase.GetStringFromReader(oReader, "NOMBRE");
                    oTempCentro.OID = DataBase.GetIntFromReader(oReader, "OID");

                    oCentroResult.Add(oTempCentro);
                }

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
           
           
            return oCentroResult;
        }

        public static CENTROS Obtener(int oidCentro)
        {
            FbConnection oConexion = new FbConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ConexionBD"].ConnectionString);
            oConexion.Open();

            FbCommand oCommand = new FbCommand("select * from centros where oid= " + oidCentro, oConexion);
            FbDataReader oReader = oCommand.ExecuteReader();
            CENTROS oCentroResult = new CENTROS();
            try
            {
                while (oReader.Read())
                {

                    oCentroResult.NOMBRE = DataBase.GetStringFromReader(oReader, "NOMBRE");
                    oCentroResult.OID = DataBase.GetIntFromReader(oReader, "OID");
                    oCentroResult.HORARIO= DataBase.GetStringFromReader(oReader, "HORARIO");
                    oCentroResult.DESCRIPCIO = DataBase.GetStringFromReader(oReader, "DESCRIPCIO");
                    oCentroResult.DIRECCION = DataBase.GetStringFromReader(oReader, "DIRECCION");

                }
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
         

            return oCentroResult;
        }

   
        public static List< CENTROS> List()
        {
            FbConnection oConexion = new FbConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ConexionBD"].ConnectionString);
            oConexion.Open();

            FbCommand oCommand = new FbCommand("select * from centros ", oConexion);
            FbDataReader oReader = oCommand.ExecuteReader();

            List<CENTROS> lCentros = new List<CENTROS>();

            try
            {
                while (oReader.Read())
                {
                    CENTROS oCentro = new CENTROS();
                    oCentro.NOMBRE = DataBase.GetStringFromReader(oReader, "NOMBRE");
                    oCentro.DIRECCION = DataBase.GetStringFromReader(oReader, "DIRECCION");
                    oCentro.OID = DataBase.GetIntFromReader(oReader, "OID");
                    oCentro.CID = DataBase.GetIntFromReader(oReader, "CID");
                    lCentros.Add(oCentro);
                }
                if (HttpContext.Current.User!= null)
                {
                    string test = HttpContext.Current.User.Identity.Name;
                    var centroAsociado = UsuariosRepositorio.Obtener(HttpContext.Current.User.Identity.Name.ToString()).CENTROASOCIADO;
                    if (centroAsociado.HasValue && centroAsociado > 0)
                    {
                        lCentros = lCentros.Where(p => p.OID == centroAsociado.Value).ToList();
                    }
                }
                
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
    
            return lCentros;
        }

    }
}