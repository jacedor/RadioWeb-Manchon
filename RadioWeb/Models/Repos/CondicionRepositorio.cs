using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using FirebirdSql.Data.FirebirdClient;
using RadioWeb.Models;
using RadioWeb.Utils;

namespace RadioWeb.Models.Repos
{
     /// <summary>
     /// GRUPOS DE APARATOS
     /// </summary>
    public static class CondicionRepositorio
    {
    
        
        
        public static List<CONDICION> Lista(int cid)
        {
            FbConnection oConexion = new FbConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ConexionBD"].ConnectionString); 
            oConexion.Open();

            FbCommand oCommand = new FbCommand("select * from CONDICION where ior_empresa=4 and cid=" + cid + " order by DESCRIPCION", oConexion);
            FbDataReader oReader = oCommand.ExecuteReader();

            List<CONDICION> lCondicion = new List<CONDICION>();


            while (oReader.Read())
            {
                CONDICION oCondicion = new CONDICION();
                oCondicion.OID = DataBase.GetIntFromReader(oReader, "OID");
                oCondicion.BORRADO = oReader["BORRADO"].ToString();
                oCondicion.CANAL = oReader["CANAL"].ToString();
                oCondicion.CID = DataBase.GetIntFromReader(oReader, "CID");
                oCondicion.DESCRIPCION = oReader["DESCRIPCION"].ToString();
                oCondicion.COD_COND = oReader["COD_COND"].ToString();
            
            
                lCondicion.Add(oCondicion);
            }


            oConexion.Close();
            if (oCommand != null)
            {
                oCommand.Dispose();
            }
            return lCondicion;
        }
    }
}