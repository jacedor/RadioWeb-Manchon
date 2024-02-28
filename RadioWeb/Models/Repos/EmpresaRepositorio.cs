using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using FirebirdSql.Data.FirebirdClient;
using RadioWeb.Models;
using RadioWeb.Utils;

namespace RadioWeb.Models.Repos
{
    public class EmpresaRepositorio
    {


        public EMPRESAS Obtener(int oid)
        {
            FbConnection oConexion = new FbConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ConexionBD"].ConnectionString);
            oConexion.Open();

            FbCommand oCommand = new FbCommand("select * from empresas where oid=" + oid, oConexion);
            FbDataReader oReader = oCommand.ExecuteReader();
            EMPRESAS oEmpresa = new EMPRESAS();
            try
            {
                while (oReader.Read())
                {

                    oEmpresa.BORRADO = oReader["BORRADO"].ToString();
                    oEmpresa.CID = DataBase.GetIntFromReader(oReader, "CID");
                    oEmpresa.CIUDAD = oReader["CIUDAD"].ToString();
                    oEmpresa.CP = oReader["CP"].ToString();
                    oEmpresa.DIRECCIO = oReader["DIRECCIO"].ToString();
                    oEmpresa.NOMBRE = oReader["NOMBRE"].ToString();
                    oEmpresa.OID = DataBase.GetIntFromReader(oReader, "OID");
                    oEmpresa.ORDEN = DataBase.GetShortFromReader(oReader, "ORDEN");
                    oEmpresa.OWNER = DataBase.GetIntFromReader(oReader, "OWNER");
                    oEmpresa.CIF = DataBase.GetStringFromReader(oReader, "CIF");
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


            return oEmpresa;
        }
    }
}