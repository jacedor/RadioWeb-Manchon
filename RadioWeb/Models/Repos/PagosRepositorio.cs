using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using FirebirdSql.Data.FirebirdClient;
using RadioWeb.Models;
using RadioWeb.Utils;


namespace RadioWeb.Models.Repos
{
    public class PagosRepositorio
    {
        public static double GetSumaPagado(int oidExploracion)
        {

            FbConnection oConexion = new FbConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ConexionBD"].ConnectionString);
            oConexion.Open();
            string query = "select sum(cantidad) as Pagado from pagos where owner=" + oidExploracion + " and (borrado<>'T' or borrado is null)";

            FbCommand oCommand = new FbCommand(query, oConexion);
            FbDataReader oReader = oCommand.ExecuteReader();
            double result = 0.0;
            while (oReader.Read())
            {

                result = DataBase.GetDoubleFromReader(oReader, "PAGADO");
                if (result <0)
                {
                    result = 0;
                }
            }
            oConexion.Close();
            return result;
        }


        public static PAGOS Obtener(int oid)
        {

            FbConnection oConexion = new FbConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ConexionBD"].ConnectionString);
            string query = "select * from PAGOS where oid = " + oid;

            FbCommand oCommand = new FbCommand(query, oConexion);
            oConexion.Open();
            FbDataReader oReader = oCommand.ExecuteReader();

            PAGOS oPago = new PAGOS();
            while (oReader.Read())
            {


                oPago.OID = (int)oReader["OID"];
                oPago.APLAZADO = DataBase.GetStringFromReader(oReader, "APLAZADO");
                oPago.BORRADO = oReader["BORRADO"].ToString();
                oPago.CANTIDAD = DataBase.GetDoubleFromReader(oReader, "CANTIDAD");
                oPago.CID = DataBase.GetIntFromReader(oReader, "CID");
                oPago.OWNER = DataBase.GetIntFromReader(oReader, "OWNER");
                oPago.DEUDA_CANTIDAD = DataBase.GetDoubleFromReader(oReader, "DEUDA_CANTIDAD");
                oPago.DEUDA_FECHA = DataBase.GetDateTimeFromReader(oReader, "DEUDA_FECHA");
                oPago.FECHA = DataBase.GetDateTimeFromReader(oReader, "FECHA");
                oPago.IOR_EMPRESA = DataBase.GetIntFromReader(oReader, "IOR_EMPRESA");
                oPago.IOR_MONEDA = DataBase.GetIntFromReader(oReader, "IOR_MONEDA");
                oPago.TIPOPAGO = DataBase.GetStringFromReader(oReader, "TIPOPAGO");





            }
            oConexion.Close();
            oCommand.Dispose();
            return oPago;
        }

        public static List<PAGOS> GetPagosExploracion(int oidExploracion)
        {

            FbConnection oConexion = new FbConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ConexionBD"].ConnectionString);
            string query = "select p.OID,p.APLAZADO, p.BORRADO, p.CANTIDAD,p.CID, p.DEUDA_CANTIDAD, p.DEUDA_FECHA, p.FECHA, p.IOR_EMPRESA, p.IOR_MONEDA, p.OWNER,p.TIPOPAGO,o.SIMBOLO as LKP_SIMBOLO, r.DESCRIPCION as LKP_DESCRIPCION";
            query = query + " from PAGOS p left join monedas o on o.oid=p.ior_moneda join ref_class r on r.oid=p.cid where p.owner= " + oidExploracion;
            query = query + " order by p.fecha  ";
            FbCommand oCommand = new FbCommand(query, oConexion);
            oConexion.Open();
            FbDataReader oReader = oCommand.ExecuteReader();

            List<PAGOS> oResult = new List<PAGOS>();
            while (oReader.Read())
            {

                PAGOS oPago = new PAGOS();
                oPago.OID = (int)oReader["OID"];
                oPago.APLAZADO = DataBase.GetStringFromReader(oReader, "APLAZADO");
                oPago.BORRADO = oReader["BORRADO"].ToString();
                oPago.CANTIDAD = DataBase.GetDoubleFromReader(oReader, "CANTIDAD");
                oPago.CID = DataBase.GetIntFromReader(oReader, "CID");
                oPago.OWNER = DataBase.GetIntFromReader(oReader, "OWNER");
                oPago.DEUDA_CANTIDAD = DataBase.GetDoubleFromReader(oReader, "DEUDA_CANTIDAD");
                oPago.DEUDA_FECHA = DataBase.GetDateTimeFromReader(oReader, "DEUDA_FECHA");
                oPago.FECHA = DataBase.GetDateTimeFromReader(oReader, "FECHA");
                oPago.IOR_EMPRESA = DataBase.GetIntFromReader(oReader, "IOR_EMPRESA");
                oPago.IOR_MONEDA = DataBase.GetIntFromReader(oReader, "IOR_MONEDA");
                oPago.TIPOPAGO = DataBase.GetStringFromReader(oReader, "TIPOPAGO");
                oPago.SIMBOLO = DataBase.GetStringFromReader(oReader, "LKP_SIMBOLO");
                oPago.DESCRIPCION = DataBase.GetStringFromReader(oReader, "LKP_DESCRIPCION");
               


                oResult.Add(oPago);
            }
            oConexion.Close();
            oCommand.Dispose();
            return oResult;
        }

        public static List<LISTADIA> GetPagosPaciente(int ior_paciente, DateTime fecha)
        {

            FbConnection oConexion = new FbConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ConexionBD"].ConnectionString);
            string query = "select e.hora,e.estado, e.oid,e.ior_gpr,e.ior_empresa,e.ior_paciente,e.fecha, p.paciente as KP_PACIENTE, d.cod_fil as LKP_COD_FIL, " +
                        "a.fil as LKP_FIL, e.cantidad, o.simbolo as LKP_SIMBOLO, " +
                        "m.codmut as LKP_CODMUT, e.ior_moneda, e.pagado,e.aplazado,e.facturada,e.ior_factura,e.pagar,e.borrado,e.nofacturab " +
                        "from exploracion e join paciente p on p.oid = e.ior_paciente join mutuas m on m.oid = e.ior_entidadpagadora " +
                        "join daparatos d on d.oid = e.ior_aparato join aparatos a on a.oid = e.ior_tipoexploracion join monedas o on o.oid = e.ior_moneda " +
                        "where e.ior_empresa = 4 and e.estado = '3' and e.ior_paciente = " + ior_paciente + " and e.fecha = " + DataBase.QuotedString(fecha.ToString("yyyy-MM-dd")) +
                        " order by e.fecha desc";

            FbCommand oCommand = new FbCommand(query, oConexion);
            oConexion.Open();
            FbDataReader oReader = oCommand.ExecuteReader();
            List<LISTADIA> oListaExploracionesAPagar = new List<LISTADIA>();
            while (oReader.Read())
            {
                oListaExploracionesAPagar.Add(new LISTADIA
                {
                    OID = DataBase.GetIntFromReader(oReader, "OID"),
                    FECHA = DataBase.GetDateTimeFromReader(oReader, "FECHA").Value,
                    HORA = DataBase.GetStringFromReader(oReader, "HORA"),
                    IOR_PACIENTE = DataBase.GetIntFromReader(oReader, "IOR_PACIENTE"),
                    PACIENTE = DataBase.GetStringFromReader(oReader, "KP_PACIENTE"),
                    COD_FIL = DataBase.GetStringFromReader(oReader, "LKP_COD_FIL"),
                    FIL = DataBase.GetStringFromReader(oReader, "LKP_FIL"),
                    CANTIDAD = DataBase.GetDoubleFromReader(oReader, "CANTIDAD"),
                    SIMBOLO = DataBase.GetStringFromReader(oReader, "LKP_SIMBOLO"),
                    COD_MUT = DataBase.GetStringFromReader(oReader, "LKP_CODMUT"),
                    PAGADO = DataBase.GetBoolFromReader(oReader, "PAGADO"),
                    APLAZADO = DataBase.GetBoolFromReader(oReader, "APLAZADO"),
                    FACTURADA = DataBase.GetBoolFromReader(oReader, "FACTURADA"),
                    PAGAR = DataBase.GetBoolFromReader(oReader, "PAGAR"),
                    NOFACTURAB = DataBase.GetBoolFromReader(oReader, "NOFACTURAB"),
                    PAGOS = PagosRepositorio.GetPagosExploracion(DataBase.GetIntFromReader(oReader, "OID")),
                    CONSUMIBLES = Exp_ConsumRepositorio.GetConsumiblesPendientes(DataBase.GetIntFromReader(oReader, "OID"))


                }
                );
            }

            oConexion.Close();
            oCommand.Dispose();
            return oListaExploracionesAPagar;
        }

        //metodo que se llama en el momento de confirmar una exploración para luego ser pagada
        public static int Insertar(PAGOS oPago)
        {

            FbConnection oConexion = null;
            FbCommand oCommand = null;
            try
            {
                oConexion = new FbConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ConexionBD"].ConnectionString);
                oConexion.Open();
                string insertOrUpdateQuery = "INSERT INTO PAGOS  (APLAZADO,BORRADO, CID, FECHA,OWNER, DEUDA_CANTIDAD, IOR_MONEDA, IOR_EMPRESA, DEUDA_FECHA,CANTIDAD,TIPOPAGO) ";
                insertOrUpdateQuery += "VALUES (" + DataBase.QuotedString(oPago.APLAZADO) + "," + DataBase.QuotedString(oPago.BORRADO) + ","
                    + oPago.CID + ",'" + oPago.FECHA.Value.ToString("MM-dd-yyyy") + "'," + oPago.OWNER + "," + oPago.DEUDA_CANTIDAD.ToString().Replace(",", ".")
                    + "," + oPago.IOR_MONEDA + "," + oPago.IOR_EMPRESA + ",'" + oPago.DEUDA_FECHA.Value.ToString("MM-dd-yyyy") + "'," + oPago.CANTIDAD.ToString().Replace(",", ".") + ",'" + oPago.TIPOPAGO + "')";
                insertOrUpdateQuery += " returning OID";
                oCommand = new FbCommand(insertOrUpdateQuery.Replace("'null'", "null"), oConexion);
                int result = (int)oCommand.ExecuteScalar();
                return result;
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

        public static PAGOS Update(PAGOS oPago)
        {
            FbConnection oConexion = null;
            FbCommand oCommand = null;
            try
            {
                oConexion = new FbConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ConexionBD"].ConnectionString);
                oConexion.Open();
                string UpdateQuery = "UPDATE PAGOS SET APLAZADO=" + DataBase.QuotedString(oPago.APLAZADO) + ",TIPOPAGO=" + DataBase.QuotedString(oPago.TIPOPAGO) + ",BORRADO=" + DataBase.QuotedString(oPago.BORRADO) +
             ", CID=" + oPago.CID + ",FECHA='" + oPago.FECHA.Value.ToString("MM-dd-yyyy") + "',OWNER=" + oPago.OWNER + ",DEUDA_CANTIDAD=" + oPago.DEUDA_CANTIDAD.ToString().Replace(",", ".") +
             ",IOR_MONEDA=" + oPago.IOR_MONEDA + ",IOR_EMPRESA=" + oPago.IOR_EMPRESA + ",DEUDA_FECHA='" + oPago.DEUDA_FECHA.Value.ToString("MM-dd-yyyy") + "',CANTIDAD=" + oPago.CANTIDAD.ToString().Replace(",", ".") +
             " where oid= " + oPago.OID;

                oCommand = new FbCommand(UpdateQuery.Replace("'null'", "null"), oConexion);
                int result = oCommand.ExecuteNonQuery();
                return oPago;
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


        //metodo que se llama en el momento de confirmar una exploración para luego ser pagada
        public static int Confirmar(PAGOS oPago)
        {
            FbConnection oConexion = null;
            FbCommand oCommand = null;
            try
            {
                oConexion = new FbConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ConexionBD"].ConnectionString);
                oConexion.Open();
                string insertOrUpdateQuery = "UPDATE OR INSERT INTO PAGOS  (APLAZADO,BORRADO, CID, FECHA,OWNER, DEUDA_CANTIDAD, IOR_MONEDA, IOR_EMPRESA, DEUDA_FECHA,CANTIDAD) ";
                insertOrUpdateQuery += "VALUES (" + DataBase.QuotedString(oPago.APLAZADO) + "," + DataBase.QuotedString(oPago.BORRADO) + ","
                    + oPago.CID + ",'" + oPago.FECHA.Value.ToString("MM-dd-yyyy") + "'," + oPago.OWNER + "," + oPago.DEUDA_CANTIDAD.ToString().Replace(",", ".")
                    + "," + oPago.IOR_MONEDA + "," + oPago.IOR_EMPRESA + ",'" + oPago.DEUDA_FECHA.Value.ToString("MM-dd-yyyy") + "'," + oPago.CANTIDAD.ToString().Replace(",", ".") +") MATCHING  (OWNER)";

                oCommand = new FbCommand(insertOrUpdateQuery, oConexion);
                int result = oCommand.ExecuteNonQuery();
                return result;
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

        public static int delete(int oid)
        {
            FbConnection oConexion = new FbConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ConexionBD"].ConnectionString);
            oConexion.Open();
            FbCommand oCommand = new FbCommand("delete from  PAGOS where owner =" + oid, oConexion);
            int result = oCommand.ExecuteNonQuery();
            oCommand.Dispose();
            oConexion.Close();
            return result;
        }



    }
}