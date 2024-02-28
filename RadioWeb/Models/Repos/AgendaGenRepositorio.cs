using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using FirebirdSql.Data.FirebirdClient;
using RadioWeb.Models;
using RadioWeb.Utils;
using ADPM.Common;
namespace RadioWeb.Models.Repos

{
    
    public class AgendaGenRepositorio
    {
        public static string Obtener(int oidAparato, DateTime Fecha)
        {
            string result = "";
            FbConnection oConexion = new FbConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ConexionBD"].ConnectionString);
            oConexion.Open();
            string sql = "select first 1 AGENDA,TEXTO from agendagen where ior_empresa=4 " +
                "and agenda   =" + Fecha.ToString("MM/dd/yyyy").QuotedString() +
                " and ior_daparato=" + oidAparato;
            FbCommand oCommand = new FbCommand(sql, oConexion);
             FbDataReader oReaderTextos = oCommand.ExecuteReader();   
            try
            {
                while (oReaderTextos.Read())
                {
                    result = DataBase.convertRtf((string)oReaderTextos["TEXTO"]);
                    if (!String.IsNullOrEmpty(result))
                    {
                        result = "#" + result.Substring(1);
                    }
             
                }
                oReaderTextos.Close();
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

           

            oConexion.Close();

            return result;
        }
        
    }
}