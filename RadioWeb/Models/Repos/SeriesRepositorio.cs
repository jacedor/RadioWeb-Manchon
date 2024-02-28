using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using FirebirdSql.Data.FirebirdClient;
using RadioWeb.Models;
using RadioWeb.Utils;

namespace RadioWeb.Models.Repos
{
    public class SeriesRepositorio
    {

        public static List<SERIES> Lista()
        {
            FbConnection oConexion = new FbConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ConexionBD"].ConnectionString);
            oConexion.Open();
            string query="select distinct(COD1),ior_gpr from FACTURAS";
          
            List<SERIES> oSerieResult = new List<SERIES>();
            FbCommand oCommand = new FbCommand(query, oConexion);
            try
            {
               
                FbDataReader oReader = oCommand.ExecuteReader();
              

                while (oReader.Read())
                {
                    SERIES oTempSerie = new SERIES();
                    oTempSerie.SERIE = DataBase.GetStringFromReader(oReader, "COD1");
                    oTempSerie.TIPO = (DataBase.GetIntFromReader(oReader, "IOR_GPR") == 1 ? "PRI" : "MUT");
                    oSerieResult.Add(oTempSerie);
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
           

           
            return oSerieResult;
        }



    }
}