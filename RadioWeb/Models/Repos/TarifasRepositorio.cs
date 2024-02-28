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

    public class TarifasRepositorio
    {
        //Si el parametro obtener nomenclator esta a True, significa que queremos que se devuelvan
        //todos los tiposs de exploración que hace el centro
        public static List< APARATOS> ObtenerPorAparatoYMutua(int ? ior_aparato, int ?ior_mutua,bool obtenerNomenclator=false)
        {
            FbConnection oConexion = new FbConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ConexionBD"].ConnectionString);
            oConexion.Open();
            MUTUAS oMutua = MutuasRepositorio.Obtener(ior_mutua.Value);
            if (oMutua.IOR_CENTRAL != null && oMutua.IOR_CENTRAL > 0)
            {
                ior_mutua =oMutua.IOR_CENTRAL;
            }
            string query = "select a.OID, a.FIL,A.DES_FIL, p.CANTIDAD FROM PRECIOS p join aparatos a on a.OID = p.IOR_TIPOEXPLORACION WHERE (p.borrado='F' or p.borrado is null) and p.IOR_ENTIDADPAGADORA =" + ior_mutua + " AND p.IOR_GAPARATO= (select d.OWNER from daparatos d where d.OID = " + ior_aparato + ") ORDER BY a.FIL";
            if (obtenerNomenclator)
            {
                query = "select * from aparatos order by oid";
            }
            FbCommand oCommand = new FbCommand(query, oConexion);
            FbDataReader oReader = oCommand.ExecuteReader();
            List<APARATOS> oListAparato = new List<APARATOS>();
            if (!obtenerNomenclator)
            {
              
                APARATOS oNoAsignado = new APARATOS();
                oNoAsignado.COD_FIL = "N/A";
                oNoAsignado.FIL = "N/A";
                oNoAsignado.DES_FIL = "NO ASIGNADO";
                oNoAsignado.OID = -1;
                oListAparato.Add(oNoAsignado);
            }


            while (oReader.Read())
            {
                APARATOS oAparatoTemp = new APARATOS();
                oAparatoTemp.OID = (int)oReader["OID"];
               // oAparatoTemp.OWNER = (int)oReader["OWNER"];
               // CAMPO OBSOLETO DE DSM oAparatoTemp.COD_FIL = DataBase.GetStringFromReader(oReader, "COD_FIL");
                oAparatoTemp.FIL = DataBase.GetStringFromReader(oReader, "FIL");
                oAparatoTemp.DES_FIL = DataBase.GetStringFromReader(oReader, "DES_FIL");
                if (!obtenerNomenclator)
                {
                    oAparatoTemp.PRECIO = DataBase.GetDoubleFromReader(oReader, "CANTIDAD");

                }
                oListAparato.Add(oAparatoTemp);
            }
            oConexion.Close();
            oCommand.Dispose();
            return oListAparato;
        }

        public static string ObtenerPrecioExploracion(int IOR_TIPOEXPLORACION, int ior_mutua)
        {
            FbConnection oConexion = new FbConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ConexionBD"].ConnectionString);
            oConexion.Open();

            FbCommand oCommand = new FbCommand("select  p.CANTIDAD FROM PRECIOS p  WHERE p.IOR_ENTIDADPAGADORA =" + ior_mutua + " AND p.IOR_TIPOEXPLORACION=" + IOR_TIPOEXPLORACION , oConexion);
            string oPrecio;
            if (oCommand.ExecuteScalar() != null)
            {
                 oPrecio = oCommand.ExecuteScalar().ToString();
            }
            else {
                 oPrecio="";
            }


            oCommand.Dispose();
            oConexion.Close();
            return oPrecio;
        }

        public static List<APARATOS> ObtenerAparatosICS (int? ior_aparato, int? ior_mutua)
        {
            FbConnection oConexion = new FbConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ConexionBD"].ConnectionString);
            oConexion.Open();

            FbCommand oCommand = new FbCommand("select * from aparatos d where d.OWNER = " + ior_aparato + " ORDER BY DES_FIL", oConexion);
            FbDataReader oReader = oCommand.ExecuteReader();

            List<APARATOS> oListAparato = new List<APARATOS>();
            while (oReader.Read())
            {
                APARATOS oAparatoTemp = new APARATOS();
                oAparatoTemp.OID = (int)oReader["OID"];
                oAparatoTemp.COD_FIL = DataBase.GetStringFromReader(oReader, "COD_FIL");
               
                oAparatoTemp.DES_FIL = DataBase.GetStringFromReader(oReader, "DES_FIL");
               
                oListAparato.Add(oAparatoTemp);
            }
            oCommand.Dispose();
            oConexion.Close();
            return oListAparato;
        }

    }
}