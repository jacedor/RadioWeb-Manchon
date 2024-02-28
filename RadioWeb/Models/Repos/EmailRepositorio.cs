using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using FirebirdSql.Data.FirebirdClient;
using RadioWeb.Models;
using RadioWeb.Utils;


namespace RadioWeb.Models.Repos
{
    public class EmailRepositorio
    {


        public static EMAIL Obtener(int oid)
        {
            FbConnection oConexion = new FbConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ConexionBD"].ConnectionString);
            oConexion.Open();

            FbCommand oCommand = new FbCommand("select * from EMAIL where oid=" + oid, oConexion);
            FbDataReader oReader = oCommand.ExecuteReader();
            EMAIL oEmail = new EMAIL();
            try
            {
                while (oReader.Read())
                {
                    oEmail.OID = DataBase.GetIntFromReader(oReader, "OID");
                    oEmail.BORRADO = oReader["BORRADO"].ToString();
                    oEmail.DIRECCION = oReader["DIRECCION"].ToString();
                    oEmail.ASUNTO = DataBase.GetStringFromReader(oReader, "ASUNTO");
                    oEmail.TIPO = DataBase.GetStringFromReader(oReader, "TIPO");
                    oEmail.FECHA = DataBase.GetDateTimeFromReader(oReader, "FECHA");

                    oEmail.TEXTO = DataBase.convertRtf(DataBase.GetStringFromReader(oReader, "TEXTO"));

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


            return oEmail;
        }

        public static List<EMAIL> ListaPlantillas()
        {
            FbConnection oConexion = new FbConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ConexionBD"].ConnectionString);
            oConexion.Open();

            FbCommand oCommand = new FbCommand("select * from EMAIL where tipo= 'P' and texto is not null order by direccion", oConexion);
            FbDataReader oReader = oCommand.ExecuteReader();

            List<EMAIL> oEmailResult = new List<EMAIL>();
            try
            {
                while (oReader.Read())
                {
                    EMAIL oEmailTemp = new EMAIL();
                    oEmailTemp.OID = DataBase.GetIntFromReader(oReader, "OID");
                    oEmailTemp.BORRADO = oReader["BORRADO"].ToString();
                    oEmailTemp.DIRECCION = oReader["DIRECCION"].ToString();
                    oEmailTemp.ASUNTO = DataBase.GetStringFromReader(oReader, "ASUNTO");
                    oEmailTemp.TIPO = DataBase.GetStringFromReader(oReader, "TIPO");
                    oEmailTemp.FECHA = DataBase.GetDateTimeFromReader(oReader, "FECHA");
                    oEmailTemp.TEXTO = DataBase.convertRtf(DataBase.GetStringFromReader(oReader, "TEXTO"));
                    oEmailResult.Add(oEmailTemp);


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


            return oEmailResult;
        }


    }
}