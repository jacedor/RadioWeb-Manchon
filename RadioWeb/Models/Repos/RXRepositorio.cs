using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using FirebirdSql.Data.FirebirdClient;
using RadioWeb.Models;
using RadioWeb.Utils;

namespace RadioWeb.Models.Repos
{

    public class RxRepositorio
    {


        public static List<RX> ObtenerCodigoICS(int? ior_codigoRX)
        {
            FbConnection oConexion = new FbConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ConexionBD"].ConnectionString);
            oConexion.Open();

            FbCommand oCommand = new FbCommand("select * from rx order by codi desc", oConexion);
            FbDataReader oReader = oCommand.ExecuteReader();
            List<RX> oListRX = new List<RX>();
           
            while (oReader.Read())
            {
                RX oRXTemp = new RX();
                oRXTemp.OID = (int)oReader["OID"];
                oRXTemp.CODI = DataBase.GetStringFromReader(oReader, "CODI");
                oRXTemp.DENOM = DataBase.GetStringFromReader(oReader, "DENOM");
                oRXTemp.CANTIDAD = DataBase.GetDoubleFromReader(oReader, "CANTIDAD");
                oListRX.Add(oRXTemp);
           
            }
            oCommand.Dispose();
            oConexion.Close();
            return oListRX;
        }

    }
}