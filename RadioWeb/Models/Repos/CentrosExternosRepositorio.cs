using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using FirebirdSql.Data.FirebirdClient;
using RadioWeb.Models;
using RadioWeb.Utils;

namespace RadioWeb.Models.Repos
{
    public class CentrosExternosRepositorio
    {

        public static List<CENTROSEXTERNOS> ObtenerPorMutuaPeticiones(int iorMutua)
        {
            FbConnection oConexion = new FbConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ConexionBD"].ConnectionString);
            oConexion.Open();

            FbCommand oCommand = new FbCommand("SELECT * FROM CENTROSEXTERNOS WHERE BORRADO = 'F' or BORRADO IS NULL AND IOR_MUTUA = " + iorMutua + " ORDER BY NOMBRE", oConexion);
            FbDataReader oReader = oCommand.ExecuteReader();
            List<CENTROSEXTERNOS> oCentroResult = new List<CENTROSEXTERNOS>();

            try
            {

                while (oReader.Read())
                {
                    CENTROSEXTERNOS oCentroTemp = new CENTROSEXTERNOS();
                    oCentroTemp.NOMBRE = DataBase.GetStringFromReader(oReader, "NOMBRE");
                    oCentroTemp.OID = DataBase.GetIntFromReader(oReader, "OID");
                    oCentroTemp.CODMUT = DataBase.GetStringFromReader(oReader, "CODMUT");
                    oCentroResult.Add(oCentroTemp);
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

        public static CENTROSEXTERNOS ObtenerParaInternet(int oidCentro)
        {
            FbConnection oConexion = new FbConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ConexionBD"].ConnectionString);
            oConexion.Open();

            FbCommand oCommand = new FbCommand("select * from CentrosExternos where  oid= " + oidCentro, oConexion);
            FbDataReader oReader = oCommand.ExecuteReader();
            CENTROSEXTERNOS oCentroResult = new CENTROSEXTERNOS();

            while (oReader.Read())
            {
                oCentroResult.CODMUT = DataBase.GetStringFromReader(oReader, "CODMUT");
                oCentroResult.NOMBRE = DataBase.GetStringFromReader(oReader, "NOMBRE");
                oCentroResult.OID = DataBase.GetIntFromReader(oReader, "OID");


            }
            oCommand.Dispose();
            oConexion.Close();
            return oCentroResult;
        }

        public static CENTROSEXTERNOS Obtener(int oidCentro)
        {
            FbConnection oConexion = new FbConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ConexionBD"].ConnectionString);
            oConexion.Open();

            FbCommand oCommand = new FbCommand("select * from CentrosExternos where oid= " + oidCentro, oConexion);
            FbDataReader oReader = oCommand.ExecuteReader();
            CENTROSEXTERNOS oCentroResult = new CENTROSEXTERNOS();

            while (oReader.Read())
            {

                oCentroResult.NOMBRE = DataBase.GetStringFromReader(oReader, "NOMBRE");
                oCentroResult.OID = DataBase.GetIntFromReader(oReader, "OID");
                oCentroResult.INFOMUTUA = InfoMutuasRepositorio.Obtener(oCentroResult.OID);
                
            }
            oCommand.Dispose();
            oConexion.Close();
            return oCentroResult;
        }

        public static List< CENTROSEXTERNOS> Obtener()
        {
            FbConnection oConexion = new FbConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ConexionBD"].ConnectionString);
            oConexion.Open();

            FbCommand oCommand = new FbCommand("select * from CentrosExternos where borrado='F' or borrado is null ORDER BY CODMUT", oConexion);
            FbDataReader oReader = oCommand.ExecuteReader();
           List< CENTROSEXTERNOS> oCentroResult = new List<CENTROSEXTERNOS>();

           CENTROSEXTERNOS oNoAsignado = new CENTROSEXTERNOS();
           oNoAsignado.NOMBRE = "NO ASIGNADO";
           oNoAsignado.OID = -1;
           oCentroResult.Add(oNoAsignado);
            try
            {
                while (oReader.Read())
                {
                    CENTROSEXTERNOS oCentroTemp = new CENTROSEXTERNOS();
                    oCentroTemp.NOMBRE = DataBase.GetStringFromReader(oReader, "NOMBRE");
                    oCentroTemp.OID = DataBase.GetIntFromReader(oReader, "OID");
                    oCentroTemp.CODMUT = DataBase.GetStringFromReader(oReader, "CODMUT");
                    oCentroResult.Add(oCentroTemp);
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

   
       

    }
}