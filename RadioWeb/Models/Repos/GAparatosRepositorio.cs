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
     /// 
    public static class GAparatoRepositorio
    {
        public static GAPARATOS Obtener(int oid)
        {
            FbConnection oConexion = new FbConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ConexionBD"].ConnectionString);
            oConexion.Open();

            FbCommand oCommand = new FbCommand("select * from  gaparatos where ior_empresa=4 and OID='" + oid + "'", oConexion);
            FbDataReader oReader = oCommand.ExecuteReader();


            GAPARATOS oAparato = new GAPARATOS();
            while (oReader.Read())
            {


                oAparato.OID = DataBase.GetIntFromReader(oReader, "OID");
                oAparato.BORRADO = oReader["BORRADO"].ToString();
                oAparato.CANAL = oReader["CANAL"].ToString();
                oAparato.CID = DataBase.GetIntFromReader(oReader, "CID");
                oAparato.COD_GRUP = oReader["COD_GRUP"].ToString();
                oAparato.DES_GRUP = oReader["DES_GRUP"].ToString();

                oAparato.MODIF = DataBase.GetDateTimeFromReader(oReader, "MODIF");
                oAparato.OWNER = DataBase.GetIntFromReader(oReader, "OWNER");
            }

            oConexion.Close();
            if (oCommand != null)
            {
                oCommand.Dispose();
            }
          
            return oAparato;
        } 
      
        
        public static GAPARATOS Obtener(string DES_GRUP)
        {
            FbConnection oConexion = new FbConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ConexionBD"].ConnectionString);
            oConexion.Open();

            FbCommand oCommand = new FbCommand("select * from  gaparatos where ior_empresa=4 and cod_GRUP='" + DES_GRUP + "'", oConexion);
            FbDataReader oReader = oCommand.ExecuteReader();


            GAPARATOS oAparato = new GAPARATOS();
            while (oReader.Read())
            {
                oAparato.OID = DataBase.GetIntFromReader(oReader, "OID");
                oAparato.BORRADO = oReader["BORRADO"].ToString();
                oAparato.CANAL = oReader["CANAL"].ToString();
                oAparato.CID = DataBase.GetIntFromReader(oReader, "CID");
                oAparato.COD_GRUP = oReader["COD_GRUP"].ToString();
                oAparato.DES_GRUP = oReader["DES_GRUP"].ToString();

                oAparato.MODIF = DataBase.GetDateTimeFromReader(oReader, "MODIF");
                oAparato.OWNER = DataBase.GetIntFromReader(oReader, "OWNER");
            }



            oConexion.Close();
            if (oCommand != null)
            {
                oCommand.Dispose();
            }
            return oAparato;
        } 
        public static List<GAPARATOS> Lista()
        {
            FbConnection oConexion = new FbConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ConexionBD"].ConnectionString); 
            oConexion.Open();

            FbCommand oCommand = new FbCommand("select * from gaparatos where ior_empresa=4 order by cod_grup", oConexion);
            FbDataReader oReader = oCommand.ExecuteReader();

            List<GAPARATOS> lGaparatos = new List<GAPARATOS>();


            while (oReader.Read())
            {
                GAPARATOS oGaparatos = new GAPARATOS();
                oGaparatos.OID = DataBase.GetIntFromReader(oReader, "OID");
                oGaparatos.BORRADO = oReader["BORRADO"].ToString();
                oGaparatos.CANAL = oReader["CANAL"].ToString();
                oGaparatos.CID = DataBase.GetIntFromReader(oReader, "CID");
                oGaparatos.COD_GRUP = oReader["COD_GRUP"].ToString();
                oGaparatos.DES_GRUP = oReader["DES_GRUP"].ToString();
            
                oGaparatos.MODIF = DataBase.GetDateTimeFromReader(oReader, "MODIF");
                oGaparatos.OWNER = DataBase.GetIntFromReader(oReader, "OWNER");
                lGaparatos.Add(oGaparatos);
            }


            oConexion.Close();
            if (oCommand != null)
            {
                oCommand.Dispose();
            }
            return lGaparatos;
        }
    }
}