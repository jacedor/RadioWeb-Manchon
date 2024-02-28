using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ADPM.Common;
using FirebirdSql.Data.FirebirdClient;
using RadioWeb.Models;
using RadioWeb.Utils;

namespace RadioWeb.Models.Repos
{
    public class VidDocumentosRepositorio
    {

        public static VID_DOCUMENTOS Obtener(string docguid)
        {
            FbConnection oConexion = new FbConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ConexionBD"].ConnectionString);
            oConexion.Open();
            string query = "select * from VID_DOCUMENTOS where DOCGUI=" + docguid.QuotedString();


            VID_DOCUMENTOS oDocumento = null;
            FbCommand oCommand = new FbCommand(query, oConexion);
            try
            {

                FbDataReader oReader = oCommand.ExecuteReader();


                while (oReader.Read())
                {
                    oDocumento = new VID_DOCUMENTOS();
                    oDocumento.OID = DataBase.GetIntFromReader(oReader, "OID");
                    oDocumento.VERS = DataBase.GetIntFromReader(oReader, "VERS");
                    oDocumento.CID = DataBase.GetIntFromReader(oReader, "CID");
                    oDocumento.OWNER = DataBase.GetIntFromReader(oReader, "OWNER");
                    oDocumento.IOR_PACIENTE = DataBase.GetIntFromReader(oReader, "IOR_PACIENTE");
                    oDocumento.NOMBRE = DataBase.GetStringFromReader(oReader, "NOMBRE");
                    oDocumento.DOCGUI = DataBase.GetStringFromReader(oReader, "DOCGUI");
                    oDocumento.FECHA = DataBase.GetDateTimeFromReader(oReader, "FECHA").Value.ToShortDateString();
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



            return oDocumento;
        }

    }
}