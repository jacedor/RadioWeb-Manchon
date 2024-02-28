using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using FirebirdSql.Data.FirebirdClient;
using RadioWeb.Models;
using RadioWeb.Utils;
using ADPM.Common;

namespace RadioWeb.Models.Repos
{
    public class Precios_ConsumRepositorio
    {

        public static List< PRECIOS_CONSUM> GetConsumibles (int oidMutua, int oidGrupo) {

            
            FbConnection oConexion = new FbConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ConexionBD"].ConnectionString);
            string query = "select p.OID,p.IOR_CONSUM, p.IOR_ENTIDADPAGADORA, p.PRECIO, p.IOR_EMPRESA,m.codmut as LKP_CODMUT,m.nombre as LKP_NOMBRE,c. cod_consum as LKP_COD_CONSUM,g.cod_grup as LKP_COD_GRUP," +
            " c. des_consum as LKP_DES_CONSUM,p.ior_moneda,o.simbolo as LKP_SIMBOLO,c.owner as LKP_OWNER, m.owner as LKP_IOR_GPR,p.BORRADO" +
            " from precios_consum p left join mutuas m on m.oid = p.ior_entidadpagadora " +
            "left join consumibles c on c.oid = p.ior_consum " +
            "left join monedas o on o.oid = p.ior_moneda " +
            "left join gaparatos g on g.oid = c.owner " +
            "join CONS_gRUPO CG ON  CG.IOR_CONSUMIBLE=P.IOR_CONSUM " +
            " where p.ior_empresa = 4  and p.ior_entidadpagadora =" + oidMutua + " and CG.IOR_gAPARATO=" + oidGrupo +
            " and  (p.BORRADO!='T' or p.borrado is null) order by m.cod_mut,c.cod_consum    ";
            
            FbCommand oCommand = new FbCommand(query, oConexion);
            oConexion.Open();
            FbDataReader oReader = oCommand.ExecuteReader();

            List<PRECIOS_CONSUM> oResult = new List<PRECIOS_CONSUM>();
            while (oReader.Read())
            {

                PRECIOS_CONSUM oPrecioConsumible = new PRECIOS_CONSUM();
                oPrecioConsumible.CONSUMIBLE = new CONSUMIBLES();
                oPrecioConsumible.OID = (int)oReader["OID"];
                oPrecioConsumible.IOR_CONSUM = DataBase.GetIntFromReader(oReader, "IOR_CONSUM");
                oPrecioConsumible.IOR_ENTIDADPAGADORA = DataBase.GetIntFromReader(oReader, "IOR_ENTIDADPAGADORA");
                oPrecioConsumible.PRECIO = DataBase.GetDoubleFromReader(oReader, "PRECIO");
                oPrecioConsumible.IOR_EMPRESA = DataBase.GetIntFromReader(oReader, "IOR_EMPRESA");
                oPrecioConsumible.CONSUMIBLE.COD_CONSUM = DataBase.GetStringFromReader(oReader, "LKP_COD_CONSUM");
                oPrecioConsumible.CONSUMIBLE.DES_CONSUM = DataBase.GetStringFromReader(oReader, "LKP_DES_CONSUM");
                oPrecioConsumible.IOR_MONEDA = DataBase.GetIntFromReader(oReader, "IOR_MONEDA");
                oPrecioConsumible.OWNER = DataBase.GetIntFromReader(oReader, "LKP_OWNER");
                oResult.Add(oPrecioConsumible);
            }
            
            oConexion.Close();
            if (oCommand!= null)
            {
                oCommand.Dispose();

            }
         
            return oResult;
        }

       
        //TODO
        public static int delete(int  oid)
        {
            FbConnection oConexion = new FbConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ConexionBD"].ConnectionString);
            oConexion.Open();
            FbCommand oCommand = new FbCommand("delete from  PAGOS where owner =" +  oid, oConexion);
            int result = oCommand.ExecuteNonQuery();
            if (oCommand != null)
            {
                oCommand.Dispose();

            }
            oConexion.Close();
            return result;
        }


      
    }
}
