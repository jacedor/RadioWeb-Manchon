using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using FirebirdSql.Data.FirebirdClient;
using RadioWeb.Models;
using RadioWeb.Utils;

namespace RadioWeb.Models.Repos
{

    public class ListaEsperaRepositorio
    {
                       

        public static Utils.RecuentoPorGrupo ObtenerPorGrupoYMes(string anyo, string grupoAparato)
        {
            FbConnection oConexion = new FbConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ConexionBD"].ConnectionString);
            try
            {
                FbDataReader oReader = null;
                Utils.RecuentoPorGrupo result = new Utils.RecuentoPorGrupo();

                oReader = DataBase.EjecutarQuery(oConexion, "select g.COD_GRUP, COUNT(*) ,EXTRACT(MONTH FROM E.FECHA) MES from exploracion e join GAPARATOS g on g.OID=e.IOR_GRUPO WHERE e.ESTADO=2  g.COD_GRUP='" + grupoAparato +"' AND  e.fecha is not null and EXTRACT(YEAR FROM E.FECHA)='2015' group by  g.COD_GRUp, EXTRACT(MONTH FROM E.FECHA)  ORDER BY G.COD_GRUP,EXTRACT(MONTH FROM E.FECHA)");
                result.Mes = DateTime.Now.Month.ToString();

                result.RecuentoPorMes = new Dictionary<string, int>();
                while (oReader.Read())
                {

                    result.RecuentoPorMes.Add(oReader["MES"].ToString(), DataBase.GetIntFromReader(oReader, "COUNT"));


                }

                return result;
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

        public static Utils.RecuentoPorGrupo ObtenerPorGrupo(string anyo, int mes = 99)
        {
            FbConnection oConexion = new FbConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ConexionBD"].ConnectionString);
            try
            {
                FbDataReader oReader = null;
                Utils.RecuentoPorGrupo result = new Utils.RecuentoPorGrupo();
                //sino nos pasan ningun paramentro para el mes solo quieren ver las futuras
                if (mes > 12)
                {
                    oReader = DataBase.EjecutarQuery(oConexion, "select g.COD_GRUP,COUNT(e.OID) from exploracion e join GAPARATOS g on g.OID=e.IOR_GRUPO WHERE e.ESTADO='0' and e.FECHA>'TODAY' group by g.COD_GRUP order by g.COD_GRUP");
                    result.Mes = DateTime.Now.Month.ToString();
                }
                else
                {
                    oReader = DataBase.EjecutarQuery(oConexion, "select g.COD_GRUP, COUNT(*) from exploracion e join GAPARATOS g on g.OID=e.IOR_GRUPO  WHERE   e.fecha is not null  and EXTRACT(YEAR FROM E.FECHA)='" + anyo + "' and EXTRACT(MONTH FROM E.FECHA) ='" + mes + "' group by  g.COD_GRUp ORDER BY G.COD_GRUP");
                    result.Mes = mes.ToString();
                }
                result.Anyo = anyo;

                result.RecuentoPorMes = new Dictionary<string, int>();
                while (oReader.Read())
                {

                    result.RecuentoPorMes.Add(DataBase.GetStringFromReader(oReader, "COD_GRUP"), DataBase.GetIntFromReader(oReader, "COUNT"));


                }

                return result;
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