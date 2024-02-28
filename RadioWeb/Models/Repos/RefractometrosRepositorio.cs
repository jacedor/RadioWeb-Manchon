using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using FirebirdSql.Data.FirebirdClient;
using RadioWeb.Models;
using RadioWeb.Utils;

namespace RadioWeb.Models.Repos
{

    public class RefractometrosRepositorio
    {


        public static List<REFRACTOMETROS> Lista()
        {
            FbConnection oConexion = new FbConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ConexionBD"].ConnectionString);
            //Creamos una lista de festivos de este mes marcados en la base de datos
            List<REFRACTOMETROS> lResult = new List<REFRACTOMETROS>();
           
            string queryreftel = "Select * from refractometros";
           
            oConexion.Open();
            FbCommand oCommandRefractometros = new FbCommand(queryreftel, oConexion);
            FbDataReader oReaderRefractometros = oCommandRefractometros.ExecuteReader();
            try
            {
                while (oReaderRefractometros.Read())
                {
                    lResult.Add(new REFRACTOMETROS
                    {
                        OID = DataBase.GetIntFromReader(oReaderRefractometros, "OID"),
                        CID = DataBase.GetIntFromReader(oReaderRefractometros, "CID"),
                        BORRADO = DataBase.GetStringFromReader(oReaderRefractometros, "BORRADO"),
                        NOMBRE = DataBase.GetStringFromReader(oReaderRefractometros, "NOMBRE")
                    });
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
                    if (oCommandRefractometros != null)
                    {
                        oCommandRefractometros.Dispose();
                    }
                }

            }         
            return lResult;
        }    

    }
}