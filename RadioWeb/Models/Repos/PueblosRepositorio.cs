using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using FirebirdSql.Data.FirebirdClient;
using RadioWeb.Models;
using RadioWeb.Utils;

namespace RadioWeb.Models.Repos
{

    public class PueblosRepositorio
    {

        FbConnection oConexion = new FbConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ConexionUsuarios"].ConnectionString);
        public List<PUEBLOS> Obtener(string term)
        {
            oConexion.Open();
            FbCommand oCommand = new FbCommand("Select FIRST(5) PU.OID, PU.PUEBLO,PU.CODIGO,PRO.PROVINCIA FROM PUEBLOS PU join PROVINCIAS PRO  on PU.REFPROVINCIA=PRO.OID where  PU.PUEBLO like '" + term.ToUpper() + "%' GROUP BY PU.PUEBLO,PU.CODIGO,PU.OID,PRO.PROVINCIA ",oConexion);     
            
            
            FbDataReader oReader = oCommand.ExecuteReader();
            List<PUEBLOS> oPueblosList = new List<PUEBLOS>();
            while (oReader.Read())
            {
                PUEBLOS oPueblo = new PUEBLOS();
                oPueblo.OID = DataBase.GetIntFromReader(oReader, "OID");
                oPueblo.PUEBLO = DataBase.GetStringFromReader(oReader, "PUEBLO");
                oPueblo.CODIGO = DataBase.GetStringFromReader(oReader, "CODIGO");
                oPueblo.PROVINCIA = DataBase.GetStringFromReader(oReader, "PROVINCIA");
                oPueblosList.Add(oPueblo);
            }
            oCommand.Dispose();
            oConexion.Close();
            return oPueblosList;
        }

        public PUEBLOS ObtenerPoblacionPorCodigo(string term)
        {
            oConexion.Open();
            FbCommand oCommand = new FbCommand("Select PU.OID, PU.PUEBLO,PU.CODIGO,PRO.PROVINCIA FROM PUEBLOS PU join PROVINCIAS PRO  on PU.REFPROVINCIA=PRO.OID where  PU.CODIGO='" + term + "'", oConexion);
            FbDataReader oReader = oCommand.ExecuteReader();

         
                PUEBLOS oPueblo = new PUEBLOS();
                while (oReader.Read())
                {
                    oPueblo.OID = DataBase.GetIntFromReader(oReader, "OID");
                    oPueblo.PUEBLO = DataBase.GetStringFromReader(oReader, "PUEBLO");
                    oPueblo.CODIGO = DataBase.GetStringFromReader(oReader, "CODIGO");
                    oPueblo.PROVINCIA = DataBase.GetStringFromReader(oReader, "PROVINCIA");
                }
            oCommand.Dispose();
            oConexion.Close();
            return oPueblo;
        }
        
        
        
        
    }
}