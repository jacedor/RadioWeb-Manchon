using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using FirebirdSql.Data.FirebirdClient;
using RadioWeb.Models;
using RadioWeb.Utils;

namespace RadioWeb.Models.Repos
{
    public  class TextosRepositorio
    {
       
        /// <summary>
        /// Devuelve lo comentarios de la tabla textox
        /// </summary>   
        /// <param name="paciente">Oid del owner del texto</param>
        public static TEXTOS Obtener(int oidOwner)
        {
              FbConnection oConexion = new FbConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ConexionBD"].ConnectionString);
            FbCommand oCommand = null;
            try
            {
                oConexion.Open();
                 oCommand = new FbCommand("select t.TEXTO ,t.OWNER FROM TEXTOS t where t.OWNER=" + oidOwner, oConexion);
                FbDataReader oReader = oCommand.ExecuteReader();

                TEXTOS oTexto = new TEXTOS();
                while (oReader.Read())
                {
                    oTexto = new TEXTOS();
                    oTexto.OWNER = DataBase.GetIntFromReader(oReader, "OWNER");
                    oTexto.TEXTO = DataBase.GetStringFromReader(oReader,"TEXTO");

                    if (oTexto.TEXTO.StartsWith("{\\rtf"))
                    {
                        oTexto.TEXTO =DataBase.convertRtfToPlainText(oTexto.TEXTO);
                    }
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
                {
                    oConexion.Close();
                    if (oCommand != null)
                    {
                        oCommand.Dispose();
                    }
                }
                   
            }



        }

       
        public static TEXTOS InsertarOrUpdate(TEXTOS oTexto)
        {
            FbConnection oConexion = new FbConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ConexionBD"].ConnectionString);
            FbCommand oCommand = null;
            try
            {
                if (string.IsNullOrEmpty(oTexto.CANAL))
                {
                    oTexto.CANAL = "null";
                }
                oConexion.Open();   
               oCommand = new FbCommand("UPDATE OR INSERT INTO TEXTOS (CANAL,OWNER,TEXTO) VALUES ('" 
                  + oTexto.CANAL + "',"  + oTexto.OWNER + "," + DataBase.QuotedString(oTexto.TEXTO) + ")MATCHING  (OWNER)", oConexion);
                int result = oCommand.ExecuteNonQuery();


                return oTexto;
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



        }
    }
}