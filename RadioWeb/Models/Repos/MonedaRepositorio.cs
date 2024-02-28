using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using FirebirdSql.Data.FirebirdClient;
using RadioWeb.Models;
using RadioWeb.Utils;

namespace RadioWeb.Models.Repos
{
    public class MonedaRepositorio
    {


        public static MONEDAS Obtener(int oid)
        {
            FbConnection oConexion = new FbConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ConexionBD"].ConnectionString);
            oConexion.Open();

            FbCommand oCommand = new FbCommand("select * from monedas where oid=" + oid, oConexion);
            FbDataReader oReader = oCommand.ExecuteReader();
            MONEDAS oMoneda = new MONEDAS();

            while (oReader.Read())
            {
                oMoneda.OID = DataBase.GetIntFromReader(oReader,"OID");
                oMoneda.BORRADO = oReader["BORRADO"].ToString();
                oMoneda.CANAL = oReader["CANAL"].ToString();
                oMoneda.CID = DataBase.GetIntFromReader(oReader, "CID");
                oMoneda.MONEDA = oReader["MONEDA"].ToString();
                oMoneda.SIMBOLO = oReader["SIMBOLO"].ToString();
                oMoneda.USERNAME = oReader["USERNAME"].ToString();
                oMoneda.OWNER = DataBase.GetIntFromReader(oReader, "OWNER");
                

            }
            oCommand.Dispose();
            oConexion.Close();
            return oMoneda;
        }
    }
}