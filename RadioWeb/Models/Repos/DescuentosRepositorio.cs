using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using FirebirdSql.Data.FirebirdClient;
using RadioWeb.Models;
using RadioWeb.Utils;

namespace RadioWeb.Models.Repos
{
    public class DescuentosRepositorio
    {

        public static List<DESCUENTOS> Obtener()
        {
            FbConnection oConexion = new FbConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ConexionBD"].ConnectionString);
            oConexion.Open();
            string query = "select * from descuentos";


            List<DESCUENTOS> oDescuentosResult = new List<DESCUENTOS>();
            FbCommand oCommand = new FbCommand(query, oConexion);
            try
            {

                FbDataReader oReader = oCommand.ExecuteReader();


                while (oReader.Read())
                {
                    DESCUENTOS oTempDesc = new DESCUENTOS();
                    oTempDesc.OID = DataBase.GetIntFromReader(oReader, "OID");
                    oTempDesc.CODIGO = DataBase.GetStringFromReader(oReader, "CODIGO");
                    oTempDesc.DESCUENTO = DataBase.GetIntFromReader(oReader, "DESCUENTO");
                    oTempDesc.DESCRIPCION = DataBase.GetStringFromReader(oReader, "DESCRIPCION");
                    oDescuentosResult.Add(oTempDesc);
                }
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



            return oDescuentosResult;
        }

    }
}