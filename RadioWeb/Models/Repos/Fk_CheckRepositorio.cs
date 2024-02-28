using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using FirebirdSql.Data.FirebirdClient;
using RadioWeb.Models;
using RadioWeb.Utils;

namespace RadioWeb.Models.Repos
{
    public class Fk_CheckRepositorio
    {

        public static List<FK_CHECK> BuscarDependencias(string tabla) {

            List<FK_CHECK> fkList = new List<FK_CHECK>();
            FbConnection oConexion = new FbConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ConexionBD"].ConnectionString);
            oConexion.Open();
            string query = "select * from fk_check where tabla = '"+tabla+"'";
            FbCommand oCommand = new FbCommand(query, oConexion);
            FbDataReader oReader = oCommand.ExecuteReader();
            try
            {
                while (oReader.Read())
                {
                    FK_CHECK fkcheck = new FK_CHECK();
                    fkcheck.TABLA = DataBase.GetStringFromReader(oReader, "TABLA");
                    fkcheck.DEPENDENCIA = DataBase.GetStringFromReader(oReader, "DEPENDENCIA");
                    fkcheck.FIELD_RELATED = DataBase.GetStringFromReader(oReader, "FIELD_RELATED");
                    fkcheck.DELETE_RULE = DataBase.GetIntFromReaderString(oReader, "DELETE_RULE");
                    fkList.Add(fkcheck);
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
            return fkList;
        }
    }
}