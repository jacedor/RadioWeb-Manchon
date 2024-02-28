using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using FirebirdSql.Data.FirebirdClient;
using RadioWeb.Models;
using RadioWeb.Utils;

namespace RadioWeb.Models.Repos
{
    public  class InfoMutuasRepositorio
    {
       
        /// <summary>
        /// Devuelve lo comentarios de la tabla textox
        /// </summary>   
        /// <param name="paciente">Oid del owner del texto</param>
        public static INFOMUTUAS Obtener(int oidOwner)
        {
              FbConnection oConexion = new FbConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ConexionBD"].ConnectionString);

            try
            {
                oConexion.Open();
                FbCommand oCommand = new FbCommand("select t.TEXTO,t.TEXTO2 FROM INFOMUTUAS t where t.OWNER=" + oidOwner, oConexion);
                FbDataReader oReader = oCommand.ExecuteReader();

                INFOMUTUAS oTexto = new INFOMUTUAS();
                while (oReader.Read())
                {
                    oTexto = new INFOMUTUAS();
                    oTexto.TEXTO =  DataBase.convertRtfToHtml( DataBase.GetStringFromReader(oReader,"TEXTO"));
                    oTexto.TEXTO2 = DataBase.convertRtfToHtml(DataBase.GetStringFromReader(oReader, "TEXTO2"));

                }


                return oTexto;
            }
            catch (Exception)
            {

                throw;
            }
            finally
            {
                if (oConexion.State == System.Data.ConnectionState.Open)
                    oConexion.Close();
            }



        }

      

    }
}