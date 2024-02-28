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
    public class Exp_ConsumRepositorio
    {

        public static EXP_CONSUM Obtener(int oid)
        {
            EXP_CONSUM oConsumible;
            try
            {
                using (var ctx = new RadioDBContext())
                {
                    oConsumible = ctx.Exp_Consum.Where(s => s.OID == oid).FirstOrDefault();
                }
            }
            catch (Exception ex)
            {

                throw;
            }






            return oConsumible;
        }


        public static List<EXP_CONSUM> GetConsumiblesPendientes(int oidExploracion)
        {


            FbConnection oConexion = new FbConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ConexionBD"].ConnectionString);
            string query = "select explo.informada,e.owner, e.CABINF_DOSIS,e.CABINF_DLP, e.MCI,E.DLP,E.DOSIS, e.OID,e.HORA, e.IOR_EXPLORACION, e.IOR_ENTIDADPAGADORA, " +
                "m.CODMUT as LKP_CODMUT, e.IOR_CONSUM, e.PRECIO, e.IOR_MONEDA, e.IOR_GPR, o.simbolo as LKP_SIMBOLO, e.pagado, e.aplazado, e.facturado, e.ior_factura, " +
                "e.pagar, E.BORRADO "+
                "from EXP_CONSUM e " +
                "join consumibles c on c.oid=e.ior_consum " +
                "join mutuas m on m.oid=e.ior_entidadpagadora " +                
                "join monedas o on o.oid=e.ior_moneda " +
                "join exploracion explo on e.ior_exploracion=explo.oid " +
                "where e.ior_exploracion=" + oidExploracion;

            FbCommand oCommand = new FbCommand(query, oConexion);
            oConexion.Open();
            FbDataReader oReader = oCommand.ExecuteReader();

            List<EXP_CONSUM> oResult = new List<EXP_CONSUM>();
            try
            {
                while (oReader.Read())
                {

                    EXP_CONSUM oExpConsumible = new EXP_CONSUM();
                    oExpConsumible.OID = (int)oReader["OID"];
                    oExpConsumible.CABINF_DLP = DataBase.GetStringFromReader(oReader, "CABINF_DLP");
                    oExpConsumible.CABINF_DOSIS = DataBase.GetStringFromReader(oReader, "CABINF_DOSIS");
                    oExpConsumible.IOR_EXPLORACION = DataBase.GetIntFromReader(oReader, "IOR_EXPLORACION");
                    oExpConsumible.IOR_ENTIDADPAGADORA = DataBase.GetIntFromReader(oReader, "IOR_ENTIDADPAGADORA");
                    oExpConsumible.CODMUT = DataBase.GetStringFromReader(oReader, "LKP_CODMUT");
                    oExpConsumible.SIMBOLO = DataBase.GetStringFromReader(oReader, "LKP_SIMBOLO");
                    oExpConsumible.IOR_CONSUM = DataBase.GetIntFromReader(oReader, "IOR_CONSUM");
                    oExpConsumible.PRECIO = DataBase.GetDoubleFromReader(oReader, "PRECIO");
                    oExpConsumible.IOR_MONEDA = DataBase.GetIntFromReader(oReader, "IOR_MONEDA");
                    oExpConsumible.IOR_GPR = DataBase.GetIntFromReader(oReader, "IOR_GPR");
                    oExpConsumible.CONSUMIBLE = ConsumibleRepositorio.Obtener((int)oExpConsumible.IOR_CONSUM);
                    oExpConsumible.PAGADO = DataBase.GetStringFromReader(oReader, "PAGADO");
                    oExpConsumible.APLAZADO = DataBase.GetStringFromReader(oReader, "APLAZADO");
                    oExpConsumible.FACTURADO = DataBase.GetStringFromReader(oReader, "FACTURADO");
                    oExpConsumible.IOR_MONEDA = DataBase.GetIntFromReader(oReader, "IOR_MONEDA");
                    oExpConsumible.IOR_FACTURA = DataBase.GetIntFromReader(oReader, "IOR_FACTURA");
                    oExpConsumible.PAGAR = DataBase.GetStringFromReader(oReader, "PAGAR");
                    oExpConsumible.BORRADO = DataBase.GetStringFromReader(oReader, "BORRADO");
                    oExpConsumible.DOSIS = DataBase.GetStringFromReader(oReader, "DOSIS");
                    oExpConsumible.MCI = DataBase.GetDoubleFromReader(oReader, "MCI");
                    oExpConsumible.DLP = DataBase.GetDoubleFromReader(oReader, "DLP");
                    oExpConsumible.HORA = DataBase.GetStringFromReader(oReader, "HORA");
                    oExpConsumible.PAGOS = PagosRepositorio.GetPagosExploracion(oExpConsumible.OID);
                    oExpConsumible.ENTIDADPAGADORA = MutuasRepositorio.Obtener(oExpConsumible.IOR_ENTIDADPAGADORA.Value);
                    oExpConsumible.EXPLORACIONINFORMADA = DataBase.GetStringFromReader(oReader, "informada");
                    int tecnicoConsumible = DataBase.GetIntFromReader(oReader, "OWNER"); ;
                    if (tecnicoConsumible > 0)
                    {
                        oExpConsumible.TECNICO = PersonalRepositorio.ObtenerTecnicos().Single(p=>p.OID==tecnicoConsumible).NOMBRE;
                    }
                    else
                    {
                        oExpConsumible.TECNICO = "...";

                    }
                    oResult.Add(oExpConsumible);
                }
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

            return oResult;
        }

        public static int UpdateCampo(string campo, string valor, int oid, string tipoCampo = "string")
        {


            FbConnection oConexion = new FbConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ConexionBD"].ConnectionString);
            FbCommand oCommand = null;
            try
            {
                oConexion.Open();
                string updateStament = "update EXP_CONSUM set " + campo + "=";
                if (tipoCampo != "string")
                {
                    updateStament += valor.ToUpper() + "";
                }
                else
                {
                    updateStament += "'" + valor.ToUpper() + "'";
                }

                updateStament += " where oid= " + oid;
                oCommand = new FbCommand(updateStament, oConexion);
                oCommand.ExecuteNonQuery();
            }
            catch (Exception ex)
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

            return oid;

        }
        public static int Insertar(EXP_CONSUM oConsumible)
        {
            WebConfigRepositorio oConfig = new WebConfigRepositorio();
            FbConnection oConexion = new FbConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ConexionBD"].ConnectionString);
            FbCommand oCommand = null;
            oConexion.Open();
            try
            {
                MUTUAS oMutua = MutuasRepositorio.Obtener(oConsumible.IOR_ENTIDADPAGADORA.Value);
                EXPLORACION oExplo = ExploracionRepositorio.Obtener(oConsumible.IOR_EXPLORACION.Value);
               string imprimirCabeceraDosis = (oConsumible.CABINF_DOSIS=="False"  ? "F" : "T");
               string imprimirCabeceraDLP = (oConsumible.CABINF_DLP == "False" ? "F" : "T");

            
                    string insertOrUpdateQuery = "INSERT INTO EXP_CONSUM  (OID,IOR_EMPRESA, IOR_CONSUM, IOR_EXPLORACION," +
              "PRECIO, PAGADO, APLAZADO, IOR_ENTIDADPAGADORA," +
              "IOR_GPR, IOR_MONEDA, FACTURADO, IOR_FACTURA, PAGAR, BORRADO,CABINF_DOSIS,CABINF_DLP)";
                    insertOrUpdateQuery += "VALUES (gen_id(GENUID,1),4," + oConsumible.IOR_CONSUM + "," + oConsumible.IOR_EXPLORACION.Value + ",";
                    insertOrUpdateQuery += oConsumible.PRECIO.ToString().Replace(",", ".") + "," + oConsumible.PAGADO.QuotedString() + ",";
                    insertOrUpdateQuery += oConsumible.APLAZADO.QuotedString() + "," + oConsumible.IOR_ENTIDADPAGADORA + ",";
                    insertOrUpdateQuery += oMutua.OWNER + "," + int.Parse(oConfig.ObtenerValor("IOR_MONEDA")) + "," + oConsumible.FACTURADO.QuotedString() + "," + oConsumible.IOR_FACTURA + ",";
                    insertOrUpdateQuery += oConsumible.PAGAR.QuotedString() + "," + oConsumible.BORRADO.QuotedString() + "," + imprimirCabeceraDosis.QuotedString() + "," + imprimirCabeceraDLP.QuotedString() + ")";
                    insertOrUpdateQuery += " returning OID";

                    oCommand = new FbCommand(insertOrUpdateQuery, oConexion);
                    int result = (int)oCommand.ExecuteScalar();
                    ExploracionRepositorio.UpdateCampo("HAYCONSUMIBLE", "T", oExplo.OID);
                bool pagoAntesConfirmacion = (oConfig.ObtenerValor("PagoAntesExploracion") == "T" ? true : false);

                //Se debe crear el pago del consumible solo cuando la exploración sea privada y
                //el estado sea igual a pendiente y el parametro global de pagoantesconfirmación sea true
                //o cuanto el estado sea igual a 3 y el pagoantes de exploracion sea igual a false
                if ( oMutua.OWNER == 1 && 
                    ((oExplo.ESTADO == "2" && pagoAntesConfirmacion) || (oExplo.ESTADO == "3" && !pagoAntesConfirmacion)) )
                    {
                        //Inserción del pago asociado al Consumible
                        PAGOS oPagoConsumible = new PAGOS
                        {
                            APLAZADO = "F",
                            BORRADO = "F",
                            CANTIDAD = 0,
                            CID = 1378,
                            TIPOPAGO = "V",
                            DEUDA_CANTIDAD = oConsumible.PRECIO,
                            FECHA = DateTime.Now,
                            DEUDA_FECHA = oExplo.FECHA,
                            IOR_EMPRESA = 4,
                            IOR_MONEDA = int.Parse(oConfig.ObtenerValor("IOR_MONEDA")),
                            OWNER = result
                        };
                        PagosRepositorio.Confirmar(oPagoConsumible);
                    }
                
              
                return 1;
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


        //TODO
        public static int delete(int oid)
        {
            FbConnection oConexion = new FbConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ConexionBD"].ConnectionString);

            oConexion.Open();
            FbCommand oCommand = new FbCommand("delete from  PAGOS where owner =" + oid, oConexion);
            int result = 0;
            try
            {
                result = oCommand.ExecuteNonQuery();
                oConexion.Close();
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
            return result;


        }



    }
}
