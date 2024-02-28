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

    public class TipoDocRepositorio
    {


        public static List<TIPODOC> Lista()
        {
            FbConnection oConexion = new FbConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ConexionBD"].ConnectionString);
            //Creamos una lista de festivos de este mes marcados en la base de datos
            List<TIPODOC> lResult = new List<TIPODOC>();
           
            string queryreftel = "Select * from TIPODOC where ior_empresa=4  ";
           
            oConexion.Open();
            FbCommand oCommandFestivos = new FbCommand(queryreftel, oConexion);
            FbDataReader oReaderFestivos = oCommandFestivos.ExecuteReader();
            try
            {
                while (oReaderFestivos.Read())
                {
                    lResult.Add(new TIPODOC
                    {
                        OID = DataBase.GetIntFromReader(oReaderFestivos, "OID"),
                        BORRADO = DataBase.GetStringFromReader(oReaderFestivos, "BORRADO"),
                        COD_TIPO = DataBase.GetStringFromReader(oReaderFestivos, "COD_TIPO"),
                        DESCRIPCION = DataBase.GetStringFromReader(oReaderFestivos, "DESCRIPCION")
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
                    if (oCommandFestivos != null)
                    {
                        oCommandFestivos.Dispose();
                    }
                }

            }         
            return lResult;
        }    

    }
}