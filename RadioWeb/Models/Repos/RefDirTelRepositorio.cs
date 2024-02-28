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

    public class RefDirTelRepositorio
    {


        public static List<REF_DIRTEL> Obtener(int tipo)
        {
            FbConnection oConexion = new FbConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ConexionBD"].ConnectionString);
            //Creamos una lista de festivos de este mes marcados en la base de datos
            List<REF_DIRTEL> lResult = new List<REF_DIRTEL>();

           
            string queryreftel = "Select * from ref_dirtel where ior_empresa=4 and cid= " + tipo;

           
            oConexion.Open();
            FbCommand oCommandFestivos = new FbCommand(queryreftel, oConexion);
            FbDataReader oReaderFestivos = oCommandFestivos.ExecuteReader();
            try
            {
                while (oReaderFestivos.Read())
                {
                    lResult.Add(new REF_DIRTEL
                    {
                        OID = DataBase.GetIntFromReader(oReaderFestivos, "OID"),
                        BORRADO = DataBase.GetStringFromReader(oReaderFestivos, "BORRADO"),
                        COD_TIPO = DataBase.GetStringFromReader(oReaderFestivos, "COD_TIPO"),
                        DES_TIPO = DataBase.GetStringFromReader(oReaderFestivos, "DES_TIPO")
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