using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using FirebirdSql.Data.FirebirdClient;

namespace RadioWeb.Utils
{
    public class LogLopd
    {

        public static void Insertar(string Msg, string Nivel)
        {
            FbConnection oConexion = new FbConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ConexionUsuarios"].ConnectionString);
            FbCommand oCommand=null;
            try
            {
                string InsertComand = "insert into UCLOG ( IdUser, MSG, Data, Nivel)  VALUES (";
                InsertComand += ((RadioWeb.Models.USUARIO)HttpContext.Current.Session["Usuario"]).IDUSER + ",'" + "RW-" + Msg + "','" + DateTime.Now.ToString("yyyyMMddHHmmssfff") + "','" + Nivel + "')";
                
                oConexion.Open();
                 oCommand = new FbCommand(InsertComand, oConexion);
                int result = (int)oCommand.ExecuteNonQuery();
               

            }
            catch (Exception ex)
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



        }
    }
}