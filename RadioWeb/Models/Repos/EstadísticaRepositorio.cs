using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using FirebirdSql.Data.FirebirdClient;
using RadioWeb.Models;
using RadioWeb.Utils;
using RadioWeb.Models.Estadistica;
using System.Globalization;
using System.Data;
using ADPM.Common;
using RadioWeb.Models.Logica;
using RadioWeb.Models.Clases;

namespace RadioWeb.Models.Repos
{
    public class EstadisticaRepositorio
    {

        public enum Operacion
        {
            Cuenta = 1,
            Suma = 2,
            Porcentage = 3
        }
        private bool EsFestivo(string fecha, int oidAparato)
        {

            return !(FestivosRepositorio.Obtener(oidAparato, DateTime.Parse(fecha).ToString("yyyy/MM/dd")).Count == 0 && DateTime.Parse(fecha).DayOfWeek != DayOfWeek.Sunday);
        }



        public static ItemEstadisticoOcupacion ActiviadDia(string fecha, int oidAparato)
        {
            FbConnection oConexion = new FbConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ConexionBD"].ConnectionString);

            DateTime startDate = DateTime.Parse(fecha);




            ItemEstadisticoOcupacion temp = null;


            FbCommand cmd = new FbCommand();
            try
            {
                cmd.Connection = oConexion;
                cmd.CommandText = "execute PROCEDURE CALCULARHUECOS(@aparato,@fecha)";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("aparato", oidAparato);
                cmd.Parameters.AddWithValue("fecha", fecha);
                var returnParameter = cmd.Parameters.Add("(HUECOSOCUPADOS", SqlDbType.Int);
                var returnParameter2 = cmd.Parameters.Add("HUECOSTOTALES", SqlDbType.Int);
                returnParameter.Direction = ParameterDirection.ReturnValue;
                returnParameter2.Direction = ParameterDirection.ReturnValue;

                oConexion.Open();
                cmd.ExecuteNonQuery();
                oConexion.Close();

                temp = new ItemEstadisticoOcupacion
                {
                    Anyo = startDate.Year.ToString(),
                    Aparato = oidAparato.ToString(),
                    Dia = startDate.Day,
                    HuecosHorario = (int)returnParameter2.Value,
                    HuecosOcupados = (int)returnParameter.Value,
                    Mes = startDate.Month.ToString()
                };

                temp.Fecha = startDate.ToShortDateString();

                if (temp.HuecosHorario > 0)
                {
                    temp.PorcentageOcupados = temp.HuecosOcupados * 100 / temp.HuecosHorario;
                    temp.PorcentageLibre = 100 - temp.PorcentageOcupados;
                }








                return temp;
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
                    if (cmd != null)
                    {
                        cmd.Dispose();
                    }

                }

            }
        }

        public static List<ItemEstadisticoOcupacion> ActividadPorMes(string fecha_inicio, string fecha_fin, int grupoAparato)
        {
            FbConnection oConexion = new FbConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ConexionBD"].ConnectionString);
            FbCommand cmd = null;
            try
            {

                List<ItemEstadisticoOcupacion> oListaResultado = new List<ItemEstadisticoOcupacion>();
                foreach (DAPARATOS aparato in DaparatoRepositorio.ListaPorGrupoAparatos(grupoAparato))
                {
                    oConexion.Open();
                    ItemEstadisticoOcupacion oResultado = new ItemEstadisticoOcupacion();
                    //DateTime fechaParametro = DateTime.Parse(fecha);
                    //DateTime startDate = new DateTime(fechaParametro.Year, fechaParametro.Month, 1);
                    //DateTime stopDate = new DateTime(startDate.Year, startDate.Month, DateTime.DaysInMonth(startDate.Year, startDate.Month));
                    oResultado.CodFil = aparato.COD_FIL;
                    oResultado.Fecha = fecha_inicio + " - " + fecha_fin;// CultureInfo.CurrentCulture.TextInfo.ToTitleCase(System.Globalization.DateTimeFormatInfo.CurrentInfo.GetMonthName(startDate.Month));
                    oResultado.IOR_APARATO = aparato.OID;
                    cmd = new FbCommand();
                    cmd.Connection = oConexion;
                    cmd.CommandText = "execute PROCEDURE CALCULARHUECOS_VARIOSDIAS(@aparato,@start, @end)";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("aparato", aparato.OID);
                    cmd.Parameters.AddWithValue("start", DateTime.Parse(fecha_inicio).ToString("dd/MM/yyyy"));
                    cmd.Parameters.AddWithValue("end", DateTime.Parse(fecha_fin).ToString("dd/MM/yyyy"));
                    var returnParameter = cmd.Parameters.Add("HUECOSOCUPADOS", SqlDbType.Int);
                    var returnParameter2 = cmd.Parameters.Add("HUECOSTOTALES", SqlDbType.Int);
                    returnParameter.Direction = ParameterDirection.ReturnValue;
                    returnParameter2.Direction = ParameterDirection.ReturnValue;

                    cmd.ExecuteNonQuery();
                    int huecosOcupados = (int)returnParameter.Value;
                    int huecosTotales = (int)returnParameter2.Value;
                    oResultado.HuecosOcupados = huecosOcupados;
                    oResultado.HuecosHorario = huecosTotales;
                    if (huecosTotales > 0)
                    {
                        oResultado.PorcentageOcupados = huecosOcupados * 100 / huecosTotales;

                    }
                    if (oConexion.State == System.Data.ConnectionState.Open)
                    {
                        oConexion.Close();
                        if (cmd != null)
                        {
                            cmd.Dispose();
                        }


                    }
                    oListaResultado.Add(oResultado);


                }

                return oListaResultado;
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
                    if (cmd != null)
                    {
                        cmd.Dispose();
                    }


                }

            }

        }

        public static List<ESTADISTICAHUECOSOCUPACION> CalculoOcupacion(string fecha_inicio, string fecha_fin, int ior_aparato)
        {
            FbConnection oConexion = new FbConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ConexionBD"].ConnectionString);
            FbCommand cmd = null;
            try
            {

                List<ESTADISTICAHUECOSOCUPACION> oListaResultado = new List<ESTADISTICAHUECOSOCUPACION>();

                oConexion.Open();
                ItemEstadisticoOcupacion oResultado = new ItemEstadisticoOcupacion();


                cmd = new FbCommand();
                cmd.Connection = oConexion;
                cmd.CommandText = "select *  from ESTADIS_HUECOS_PORC(@APARATO,@FECHAINICIAL, @FECHAFINAL)";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("APARATO", ior_aparato);
                cmd.Parameters.AddWithValue("FECHAINICIAL", DateTime.Parse(fecha_inicio).ToString("dd/MM/yyyy"));
                cmd.Parameters.AddWithValue("FECHAFINAL", DateTime.Parse(fecha_fin).ToString("dd/MM/yyyy"));
                //var returnParameter = cmd.Parameters.Add("HORALIBRE", SqlDbType.VarChar);
                //var returnParameter2 = cmd.Parameters.Add("NUMERO_HORALIBRE", SqlDbType.Int);
                //var returnParameter3 = cmd.Parameters.Add("RECUENTO_TOTALHORAS", SqlDbType.Int);
                //var returnParameter4 = cmd.Parameters.Add("PORCENTAJE", SqlDbType.Float);
                //returnParameter.Direction = ParameterDirection.ReturnValue;
                //returnParameter2.Direction = ParameterDirection.ReturnValue;
                //returnParameter3.Direction = ParameterDirection.ReturnValue;
                //returnParameter4.Direction = ParameterDirection.ReturnValue;

              FbDataReader oReader=   cmd.ExecuteReader();
                while (oReader.Read())
                {
                    oListaResultado.Add(new ESTADISTICAHUECOSOCUPACION
                    {
                        HORA = DataBase.GetStringFromReader(oReader, "HORALIBRE"),
                        NUMERO_HORALIBRE = DataBase.GetIntFromReader(oReader, "NUMERO_HORALIBRE"),
                        RECUENTO_TOTALHORAS = DataBase.GetIntFromReader(oReader, "RECUENTO_TOTALHORAS"),
                        PORCENTAJE = Math.Round(DataBase.GetDecimalFromReader(oReader, "PORCENTAJE"), 2)
                    });
                }
               
                if (oConexion.State == System.Data.ConnectionState.Open)
                {
                    oConexion.Close();
                    if (cmd != null)
                    {
                        cmd.Dispose();
                    }


                }




                return oListaResultado;
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
                    if (cmd != null)
                    {
                        cmd.Dispose();
                    }


                }

            }

        }



        public static List<ItemEstadisticoExploraciones> ListaEsperaPorGrupo()
        {
            FbConnection oConexion = new FbConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ConexionBD"].ConnectionString);

            FbDataReader oReader = null;
            List<ItemEstadisticoExploraciones> result = new List<ItemEstadisticoExploraciones>();
            string sql = "select g.COD_GRUP,COUNT(e.OID) from exploracion e join GAPARATOS g on g.OID=e.IOR_GRUPO WHERE e.ESTADO='0' and e.FECHA>'TODAY' group by g.COD_GRUP order by  COUNT(*) DESC, g.COD_GRUP";
            FbCommand oCommand = new FbCommand(sql, oConexion);
            try
            {
                oConexion.Open();
                oReader = oCommand.ExecuteReader();
                while (oReader.Read())
                {

                    result.Add(new ItemEstadisticoExploraciones { Grupo = DataBase.GetStringFromReader(oReader, "COD_GRUP"), Valor = DataBase.GetIntFromReader(oReader, "COUNT") });


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
                {
                    oConexion.Close();
                    if (oCommand != null)
                    {
                        oCommand.Dispose();
                    }

                }

            }



        }

        public static List<ItemEstadisticoExploraciones> ListaEsperaPorGruposYCentro(string Centro = "")
        {
            FbConnection oConexion = new FbConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ConexionBD"].ConnectionString);
            FbCommand oCommand = null;
            try
            {
                FbDataReader oReader = null;

                List<ItemEstadisticoExploraciones> result = new List<ItemEstadisticoExploraciones>();
                string query = "select g.COD_GRUP,c.nombre,COUNT(e.OID) from exploracion e join CENTROS c on c.OID=e.OWNER join GAPARATOS g on g.OID=e.IOR_GRUPO  WHERE e.ESTADO='0' and e.FECHA>'TODAY' ";
                if (!string.IsNullOrEmpty(Centro))
                {
                    query += " and c.NOMBRE='" + Centro + "'";
                }
                query += " group by  g.COD_GRUP,c.nombre order by  COUNT(*) DESC, c.NOMBRE";
                oConexion.Open();
                oCommand = new FbCommand(query, oConexion);
                oReader = oCommand.ExecuteReader();
                while (oReader.Read())
                {
                    string grupo = DataBase.GetStringFromReader(oReader, "COD_GRUP");
                    string color = System.Configuration.ConfigurationManager.AppSettings[grupo].ToString();
                    result.Add(new ItemEstadisticoExploraciones { Grupo = grupo, Color = color, Centro = DataBase.GetStringFromReader(oReader, "nombre"), Valor = DataBase.GetIntFromReader(oReader, "COUNT") });

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
                {
                    if (oCommand != null)
                    {
                        oCommand.Dispose();
                    }
                    oConexion.Close();

                }

            }




        }

        public static List<ItemEstadisticoExploraciones> ListaEsperaPorCentro()
        {
            FbConnection oConexion = new FbConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ConexionBD"].ConnectionString);
            FbCommand oCommand = null;
            try
            {
                FbDataReader oReader = null;
                List<ItemEstadisticoExploraciones> result = new List<ItemEstadisticoExploraciones>();
                oConexion.Open();
                oCommand = new FbCommand("select c.nombre,COUNT(e.OID) from exploracion e join CENTROS c on c.OID=e.OWNER WHERE e.ESTADO='0' and e.FECHA>'TODAY'group by c.nombre order by COUNT(e.OID) DESC,c.NOMBRE", oConexion);
                oReader = oCommand.ExecuteReader();
                while (oReader.Read())
                {

                    result.Add(new ItemEstadisticoExploraciones { Centro = DataBase.GetStringFromReader(oReader, "nombre"), Valor = DataBase.GetIntFromReader(oReader, "COUNT") });


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
                {
                    oConexion.Close();
                    if (oCommand != null)
                    {
                        oCommand.Dispose();
                    }


                }

            }



        }

        public static List<ItemEstadisticoExploraciones> ListaEsperaAparatos(string Centro, string Grupo)
        {
            FbConnection oConexion = new FbConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ConexionBD"].ConnectionString);
            FbCommand oCommand = null;
            try
            {
                FbDataReader oReader = null;
                List<ItemEstadisticoExploraciones> result = new List<ItemEstadisticoExploraciones>();
                string query = "select c.nombre, g.COD_GRUP,d.COD_FIL,COUNT(e.OID) from exploracion e join CENTROS c on c.OID=e.OWNER join GAPARATOS g on g.OID=e.IOR_GRUPO join DAPARATOS d on d.OID=e.IOR_APARATO WHERE e.ESTADO='0' and e.FECHA>'TODAY' ";
                if (!string.IsNullOrEmpty(Centro))
                {
                    query += " and c.NOMBRE='" + Centro + "'";
                }
                if (!string.IsNullOrEmpty(Grupo))
                {
                    query += " and g.COD_GRUP='" + Grupo + "'";
                }

                query += " group by c.nombre, g.COD_GRUP,d.COD_FIL order by COUNT(e.OID) DESC,c.NOMBRE,g.COD_GRUP,d.COD_FIL";
                oConexion.Open();
                oCommand = new FbCommand(query, oConexion);

                oReader = oCommand.ExecuteReader();
                while (oReader.Read())
                {
                    string grupo = DataBase.GetStringFromReader(oReader, "COD_GRUP");
                    string color = System.Configuration.ConfigurationManager.AppSettings[grupo].ToString();
                    result.Add(new ItemEstadisticoExploraciones { Color = color, Centro = DataBase.GetStringFromReader(oReader, "nombre"), Aparato = DataBase.GetStringFromReader(oReader, "COD_fil"), Grupo = DataBase.GetStringFromReader(oReader, "COD_GRUP"), Valor = DataBase.GetIntFromReader(oReader, "COUNT") });

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
                {
                    oConexion.Close();
                    if (oCommand != null)
                    {
                        oCommand.Dispose();
                    }
                }

            }

        }

        public static List<ItemEstadisticoExploraciones> ListaEsperaTipoExploracion(string Aparato)
        {
            FbConnection oConexion = new FbConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ConexionBD"].ConnectionString);
            FbCommand oCommand = null;
            try
            {
                FbDataReader oReader = null;
                List<ItemEstadisticoExploraciones> result = new List<ItemEstadisticoExploraciones>();
                string query = "select G.COD_GRUP, d.cod_fil,a.FIL,a.DES_FIL, count(e.oid) from exploracion e JOIN aparatos a on a.OID=e.ior_tipoexploracion join daparatos d on d.OID=e.ior_aparato join GAPARATOS G on d.OWNER=G.OID WHERE e.ESTADO='0' and e.FECHA>'TODAY' AND d.COD_FIL= '" + Aparato + "' GROUP by d.COD_FIL,a.FIL,a.DES_FIL,G.COD_GRUP order by count(e.oid) desc";




                oCommand = new FbCommand(query, oConexion);
                oConexion.Open();
                oReader = oCommand.ExecuteReader();
                while (oReader.Read())
                {
                    string grupo = DataBase.GetStringFromReader(oReader, "COD_GRUP");
                    string color = System.Configuration.ConfigurationManager.AppSettings[grupo].ToString();
                    result.Add(new ItemEstadisticoExploraciones { Color = color, TipoExploracion = DataBase.GetStringFromReader(oReader, "fil") + " - " + DataBase.GetStringFromReader(oReader, "des_fil"), Aparato = DataBase.GetStringFromReader(oReader, "FIL"), Valor = DataBase.GetIntFromReader(oReader, "COUNT") });


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
                {
                    oConexion.Close();
                    if (oCommand != null)
                    {
                        oCommand.Dispose();
                    }
                }

            }


        }

        public static ItemEstadisticoVentas GetPendientesFacturacionAnual(string year = "")
        {
            if (string.IsNullOrEmpty(year))
            {
                year = DateTime.Now.Year.ToString();
            }

            FbConnection oConexion = new FbConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ConexionBD"].ConnectionString);
            FbCommand oCommand = null;
            try
            {
                oConexion.Open();
                FbDataReader oReader = null;
                ItemEstadisticoVentas result = null;

                oCommand = new FbCommand("select  COUNT(e.oid),SUM(e.cantidad) from EXPLORACION e where e.ESTADO='3' and e.CANTIDAD >0  AND e.PAGADO='F' and EXTRACT(YEAR FROM E.FECHA)=" + year, oConexion);

                oReader = oCommand.ExecuteReader();
                while (oReader.Read())
                {
                    result = (new ItemEstadisticoVentas
                    {
                        Cuenta = DataBase.GetIntFromReader(oReader, "COUNT"),
                        Ventas = DataBase.GetDoubleFromReader(oReader, "SUM")
                    });
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
                {
                    oConexion.Close();
                    if (oCommand != null)
                    {
                        oCommand.Dispose();
                    }
                }

            }
        }

        public static ItemEstadisticoVentas GetFacturacionDia(string dia)
        {
            if (string.IsNullOrEmpty(dia))
            {
                dia = DateTime.Now.ToString("dd/MM/YYYY");
            }

            FbConnection oConexion = new FbConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ConexionBD"].ConnectionString);
            FbCommand oCommand = null;
            try
            {
                FbDataReader oReader = null;
                ItemEstadisticoVentas result = null;
                oConexion.Open();

                oCommand = new FbCommand("select  COUNT(e.oid),SUM(e.cantidad) from EXPLORACION e where e.ESTADO='3'  and e.fecha=" + dia.QuotedString(), oConexion);
                WebConfigRepositorio oConfig = new WebConfigRepositorio();
                oReader = oCommand.ExecuteReader();
                while (oReader.Read())
                {
                    result = (new ItemEstadisticoVentas
                    {
                        Cuenta = DataBase.GetIntFromReader(oReader, "COUNT"),
                        Ventas = DataBase.GetDoubleFromReader(oReader, "SUM"),
                        Simbolo=MonedaRepositorio.Obtener( int.Parse(oConfig.ObtenerValor("IOR_MONEDA"))).SIMBOLO
                    });
                }

                result.Ventas = Math.Round(result.Ventas, 2);
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

        public static ItemEstadisticoVentas GetFacturacionAnual(string year = "")
        {

            FbCommand oCommand = null;
            if (string.IsNullOrEmpty(year))
            {
                year = DateTime.Now.Year.ToString();
            }

            FbConnection oConexion = new FbConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ConexionBD"].ConnectionString);
            try
            {
                FbDataReader oReader = null;
                ItemEstadisticoVentas result = null;
                oConexion.Open();
                oCommand = new FbCommand("select  COUNT(e.oid),SUM(e.cantidad) from EXPLORACION e where e.ESTADO='3' and e.cantidad>0 and (not e.ior_grupo in (10,18))  and EXTRACT(YEAR FROM E.FECHA)=" + year, oConexion);

                oReader = oCommand.ExecuteReader();

                while (oReader.Read())
                {
                    result = (new ItemEstadisticoVentas
                    {
                        Cuenta = DataBase.GetIntFromReader(oReader, "COUNT"),
                        Ventas = DataBase.GetDoubleFromReader(oReader, "SUM")
                    });
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
                {
                    oConexion.Close();
                    if (oCommand != null)
                    {
                        oCommand.Dispose();
                    }
                }

            }
        }

        public static List<ItemEstadisticoVentas> GetFacturacionAnualPorMeses(string year = "")
        {
            WebConfigRepositorio oConfig = new WebConfigRepositorio();
            if (string.IsNullOrEmpty(year))
            {
                year = DateTime.Now.Year.ToString();
            }
            FbCommand oCommand = null;
            FbConnection oConexion = new FbConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ConexionBD"].ConnectionString);
            try
            {
                FbDataReader oReader = null;
                List<ItemEstadisticoVentas> result = new List<ItemEstadisticoVentas>();


                oCommand = new FbCommand("select EXTRACT(MONTH FROM E.FECHA) mes, COUNT(e.oid), SUM(e.cantidad)" +
                  "  from EXPLORACION e where e.ESTADO='3' and (not e.ior_grupo in (10,18))  and EXTRACT(YEAR FROM E.FECHA)=" + year + " group by EXTRACT(MONTH FROM E.FECHA)", oConexion);
                oConexion.Open();
                oReader = oCommand.ExecuteReader();
                while (oReader.Read())
                {
                    string oVentas = DataBase.GetDoubleFromReader(oReader, "SUM").ToString("f2");

                    result.Add((new ItemEstadisticoVentas
                    {
                        Cuenta = DataBase.GetIntFromReader(oReader, "COUNT"),
                        Ventas = Double.Parse(oVentas),
                        Mes = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(System.Globalization.DateTimeFormatInfo.CurrentInfo.GetMonthName(int.Parse(oReader["mes"].ToString()))),
                        Anyo = year,
                        Simbolo = MonedaRepositorio.Obtener(int.Parse(oConfig.ObtenerValor("IOR_MONEDA"))).SIMBOLO

                    }));
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
                {
                    oConexion.Close();
                    if (oCommand != null)
                    {
                        oCommand.Dispose();
                    }
                }

            }
        }

        //Devuelve la facturacion por grupo del año actual y año anterior
        //public static List<ItemEstadisticoVentas> GetResumenFacturacionPorGrupo()
        //{
        //    FbConnection oConexion = new FbConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ConexionBD"].ConnectionString);
        //    FbCommand oCommand = null;
        //    try
        //    {
        //        FbDataReader oReader = null;
        //        string anyoActual = DateTime.Now.ToString("MM-dd-yyyy");
        //        string Actual = DateTime.Now.ToString("yyyy");
        //        string anyoAnterior = DateTime.Now.AddYears(-1).ToString("yyyy");
        //        List<ItemEstadisticoVentas> result = new List<ItemEstadisticoVentas>();
        //        string query = "select G.COD_GRUP, EXTRACT(MONTH FROM E.FECHA),SUM(IIF(EXTRACT(YEAR FROM E.FECHA) = " + anyoAnterior + ", e.cantidad, 0)) as sAnterior,SUM(IIF(EXTRACT(YEAR FROM E.FECHA) = " + Actual + ", e.cantidad, 0)) as sActual," +
        //                      " count(IIF(EXTRACT(YEAR FROM E.FECHA) = " + anyoAnterior + ", e.cantidad, null)) as cAnterior,count(IIF(EXTRACT(YEAR FROM E.FECHA) = " + Actual + ", e.cantidad, null)) as cActual" +
        //                      " from EXPLORACION e JOIN GAPARATOS G ON G.OID = E.IOR_GRUPO " +
        //                      " where e.ESTADO = '3'  and e.IOR_GRUPO <> 10 and e.CANTIDAD > 0 and " +
        //                      "(E.FECHA >= '01-01-" + anyoAnterior + "' and  E.FECHA < '" + anyoActual + "') group by G.COD_GRUP,  EXTRACT(MONTH FROM E.FECHA)" +
        //                      "order bY  EXTRACT(MONTH FROM E.FECHA), G.COD_GRUP";


        //        oCommand = new FbCommand(query, oConexion);
        //        oConexion.Open();
        //        oReader = oCommand.ExecuteReader();



        //        while (oReader.Read())
        //        {
        //            string oVentas = DataBase.GetDoubleFromReader(oReader, "sActual").ToString();

        //            result.Add(new ItemEstadisticoVentas
        //            {
        //                Mes = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(System.Globalization.DateTimeFormatInfo.CurrentInfo.GetMonthName(int.Parse(oReader["EXTRACT"].ToString()))),
        //                Grupo = oReader["COD_GRUP"].ToString(),
        //                Ventas = Double.Parse(oVentas),
        //                Venta2 = Double.Parse(DataBase.GetDoubleFromReader(oReader, "sAnterior").ToString()),
        //                Cuenta2 = int.Parse(oReader["cAnterior"].ToString()),
        //                Cuenta = int.Parse(oReader["cActual"].ToString())
        //            });
        //        }
        //        return result;
        //    }
        //    catch (Exception ex)
        //    {

        //        throw;
        //    }
        //    finally
        //    {
        //        if (oConexion.State == System.Data.ConnectionState.Open)
        //        {
        //            oConexion.Close();
        //            if (oCommand != null)
        //            {
        //                oCommand.Dispose();
        //            }
        //        }

        //    }
        //}

        //Devuelve la facturacion por grupo del año actual y año anterior
        //el ultimo parametro indica si queremos agrupado por mes o por dia
        public static List<ItemResumen> GetResumenFacturacionPorGrupo(string start, string end, FiltrosBusquedaExploracion oFiltros, bool diario = false)
        {
            FbConnection oConexion = new FbConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ConexionBD"].ConnectionString);
            FbCommand oCommand = null;
            try
            {
                FbDataReader oReader = null;
                string fechaInicialAnyoActual = DateTime.Parse(start).ToString("MM-dd-yyyy");
                string fechaFinallAnyoActual = DateTime.Parse(end).ToString("MM-dd-yyyy");


                int anyo = oFiltros.anyo;

                string ior_mutua = "'(";
                int centro = 0;
                int ior_gpr = 0;
                int ior_medico = 0;
                int ior_colegiado = 0;
                string pagado = "'A'";
                string facturado = "'A'";

                foreach (int item in oFiltros.MutuaList)
                {
                    ior_mutua += item + ",";
                }
                ior_mutua += ")'";
                if (oFiltros.oidMedicoInformante > 0)
                {
                    ior_medico = oFiltros.oidMedicoInformante;
                }
                if (oFiltros.IOR_COLEGIADO > 0)
                {
                    ior_colegiado = oFiltros.IOR_COLEGIADO;
                }
                if (oFiltros.oidExploracion > 0)
                {
                    ior_gpr = oFiltros.oidExploracion;

                }
                if (oFiltros.oidCentro > 0)
                {
                    centro = oFiltros.oidCentro;
                }
                if (!String.IsNullOrEmpty(oFiltros.pagado) && oFiltros.pagado != "-1")
                {
                    pagado = oFiltros.pagado.QuotedString();
                }
                if (!String.IsNullOrEmpty(oFiltros.facturado) && oFiltros.pagado != "-1")
                {
                    facturado = oFiltros.facturado.QuotedString();
                }


                List<ItemResumen> result = new List<ItemResumen>();
                string query = "SELECT *" +
                     " FROM FACTURACION_MENSUAL  ('" + fechaInicialAnyoActual + "', '" + fechaFinallAnyoActual + "', " + anyo + "," +
                    ior_mutua + "," + centro + "," + pagado + "," + ior_gpr + "," + ior_colegiado + "," + ior_medico + "," + facturado + " ) p";

                if (oFiltros.oidGrupoAparato > 0)
                {
                    GAPARATOS oGrupo = GAparatoRepositorio.Obtener(oFiltros.oidGrupoAparato);

                    query = query + " where p.COD_GRUP=" + oGrupo.COD_GRUP.QuotedString();
                }



                oConexion.Open();
                if (diario)
                {
                    query = query.Replace("MENSUAL", "DIARIA");
                    query = query.Replace(",'A' ) ", ")");
                    query = query.Replace("mes", "dia");
                    query = query + " order by p.mes,p.dia,p.ORDEN,p.COD_GRUP";
                }
                else
                {
                    query = query + " order by p.mes,p.orden,p.cod_grup";
                }

                oCommand = new FbCommand(query, oConexion);
                oReader = oCommand.ExecuteReader();

                while (oReader.Read())
                {
                    ItemResumen oItem = new ItemResumen();
                    if (diario)
                    {
                        oItem.Dia = DataBase.GetIntFromReader(oReader, "DIA");
                    }
                    oItem.MesNumero = DataBase.GetIntFromReader(oReader, "MES");
                    oItem.Mes = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(
                        System.Globalization.DateTimeFormatInfo.CurrentInfo.GetMonthName(
                            int.Parse(oReader["MES"].ToString())));
                    if (diario)
                    {
                        //  CultureInfo.CurrentCulture.TextInfo.ToTitleCase(System.Globalization.DateTimeFormatInfo.CurrentInfo.GetDayName(DateTime.Parse( fechaInicialAnyoActual)));
                        oItem.DiaFormateado = oItem.Dia.ToString().PadLeft(2, '0') + "-" + oItem.MesNumero.ToString().PadLeft(2, '0') + "-" + DateTime.Now.Year.ToString();
                        DateTime oFechaTemp = DateTime.Parse(oItem.DiaFormateado);
                        oItem.DiaFormateado = oFechaTemp.ToString("dddd") + " " + oItem.DiaFormateado;
                    }

                    oItem.Grupo = oReader["COD_GRUP"].ToString();
                    oItem.Ventas = DataBase.GetDecimalFromReader(oReader, "sActual");
                   // oItem.PromedioVentas = DataBase.GetDecimalFromReader(oReader, "pActual");
                    oItem.VentaConsum = DataBase.GetDecimalFromReader(oReader, "sActualConsum");
                    oItem.VentaTotal = DataBase.GetDecimalFromReader(oReader, "sActualTOTAL");
                    oItem.Venta2 = DataBase.GetDecimalFromReader(oReader, "sAnterior");
                   // oItem.PromedioVentas2 = DataBase.GetDecimalFromReader(oReader, "pAnterior");
                    oItem.Venta2Consum = DataBase.GetDecimalFromReader(oReader, "SANTERIORCONSUM");
                    oItem.Venta2Total = DataBase.GetDecimalFromReader(oReader, "SANTERIORTOTAL");
                    oItem.DiferenciaVentas = DataBase.GetDecimalFromReader(oReader, "difCantidad");//Double.Parse(DataBase.GetDoubleFromReader(oReader, "difCantidad").ToString());
                    oItem.DiferenciaVentasPorc = DataBase.GetDecimalFromReader(oReader, "difCantidadPorc"); //(decimal)oReader["difCantidadPorc"];// Double.Parse(DataBase.GetDoubleFromReader(oReader, "difCantidadPorc").ToString());
                    oItem.Cuenta2 = int.Parse(oReader["cAnterior"].ToString());
                    oItem.Cuenta = int.Parse(oReader["cActual"].ToString());
                    oItem.DiferenciaCuenta = int.Parse(oReader["difCuenta"].ToString());
                    oItem.DiferenciaCuentaPorc = DataBase.GetDecimalFromReader(oReader, "difCuentaPorc"); //(decimal)oReader[""];//((oItem.Cuenta - oItem.Cuenta2) / oItem.Cuenta2) * 100;
                    result.Add(oItem);
                }
                return result;
            }
            catch (Exception ex)
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

        public static List<ItemResumen> GetResumenFacturacionPorGrupoAcumulado(string start, string end, FiltrosBusquedaExploracion oFiltros)
        {
            FbConnection oConexion = new FbConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ConexionBD"].ConnectionString);
            FbCommand oCommand = null;
            try
            {
                FbDataReader oReader = null;
                string fechaInicialAnyoActual = DateTime.Parse(start).ToString("MM-dd-yyyy");
                string fechaFinallAnyoActual = DateTime.Parse(end).ToString("MM-dd-yyyy");


                List<ItemResumen> result = new List<ItemResumen>();


                int anyo = oFiltros.anyo;
                //'(x,x,x,)'
                string ior_mutua = "'(";
                int centro = 0;
                int ior_gpr = 0;
                int ior_medico = 0;
                int ior_colegiado = 0;
                string pagado = "'A'";
                string facturado = "'A'";
                foreach (int item in oFiltros.MutuaList)
                {
                    ior_mutua += item + ",";
                }
                ior_mutua += ")'";
                if (oFiltros.oidMedicoInformante > 0)
                {
                    ior_medico = oFiltros.oidMedicoInformante;
                }
                if (oFiltros.IOR_COLEGIADO > 0)
                {
                    ior_colegiado = oFiltros.IOR_COLEGIADO;
                }
                if (oFiltros.oidExploracion > 0)
                {
                    ior_gpr = oFiltros.oidExploracion;

                }
                if (oFiltros.oidCentro > 0)
                {
                    centro = oFiltros.oidCentro;
                }
                if (!String.IsNullOrEmpty(oFiltros.pagado) && oFiltros.pagado != "-1")
                {
                    pagado = oFiltros.pagado.QuotedString();
                }
                if (!String.IsNullOrEmpty(oFiltros.facturado) && oFiltros.facturado != "-1")
                {
                    pagado = oFiltros.facturado.QuotedString();
                }


                string query = "SELECT * " +
                        " FROM FACTURACION  ('" + fechaInicialAnyoActual + "', '" + fechaFinallAnyoActual + "', " + anyo + "," +
                        ior_mutua + "," + centro + "," + pagado + "," + ior_gpr + "," + ior_colegiado + "," + ior_medico + "," + facturado + " ) p ";

                if (oFiltros.oidGrupoAparato > 0)
                {
                    GAPARATOS oGrupo = GAparatoRepositorio.Obtener(oFiltros.oidGrupoAparato);

                    query = query + " where p.COD_GRUP=" + oGrupo.COD_GRUP.QuotedString();
                }

                oConexion.Open();
                oCommand = new FbCommand(query, oConexion);

                oReader = oCommand.ExecuteReader();
                while (oReader.Read())
                {
                    ItemResumen oItem = new ItemResumen();
                    oItem.Grupo = oReader.IsDBNull(oReader.GetOrdinal("COD_GRUP")) ? string.Empty : oReader["COD_GRUP"].ToString();
                    oItem.Ventas = oReader.IsDBNull(oReader.GetOrdinal("sActual")) ? 0 : DataBase.GetDecimalFromReader(oReader, "sActual");
                    oItem.PromedioVentas = oReader.IsDBNull(oReader.GetOrdinal("pAnterior")) ? 0 : DataBase.GetDecimalFromReader(oReader, "pAnterior");
                    oItem.VentaTotal = oReader.IsDBNull(oReader.GetOrdinal("sActualTOTAL")) ? 0 : DataBase.GetDecimalFromReader(oReader, "sActualTOTAL");
                    oItem.VentaConsum = oReader.IsDBNull(oReader.GetOrdinal("SACTUALCONSUM")) ? 0 : DataBase.GetDecimalFromReader(oReader, "SACTUALCONSUM");
                    oItem.Venta2 = oReader.IsDBNull(oReader.GetOrdinal("sAnterior")) ? 0 : DataBase.GetDecimalFromReader(oReader, "sAnterior");
                    oItem.PromedioVentas2 = oReader.IsDBNull(oReader.GetOrdinal("pActual")) ? 0 : DataBase.GetDecimalFromReader(oReader, "pActual");

                    oItem.Venta2Consum = oReader.IsDBNull(oReader.GetOrdinal("SANTERIORCONSUM")) ? 0 : DataBase.GetDecimalFromReader(oReader, "SANTERIORCONSUM");
                    oItem.Venta2Total = oReader.IsDBNull(oReader.GetOrdinal("SANTERIORTOTAL")) ? 0 : DataBase.GetDecimalFromReader(oReader, "SANTERIORTOTAL");
                    oItem.DiferenciaVentas = oReader.IsDBNull(oReader.GetOrdinal("difCantidad")) ? 0 : DataBase.GetDecimalFromReader(oReader, "difCantidad");
                    oItem.DiferenciaVentasPorc = oReader.IsDBNull(oReader.GetOrdinal("difCantidadPorc")) ? 0 : DataBase.GetDecimalFromReader(oReader, "difCantidadPorc");
                    oItem.Cuenta2 = oReader.IsDBNull(oReader.GetOrdinal("cAnterior")) ? 0 : int.Parse(oReader["cAnterior"].ToString());
                    oItem.Cuenta = oReader.IsDBNull(oReader.GetOrdinal("cActual")) ? 0 : int.Parse(oReader["cActual"].ToString());
                    oItem.DiferenciaCuenta = oReader.IsDBNull(oReader.GetOrdinal("difCuenta")) ? 0 : int.Parse(oReader["difCuenta"].ToString());
                    oItem.DiferenciaCuentaPorc = oReader.IsDBNull(oReader.GetOrdinal("difCuentaPorc")) ? 0 : DataBase.GetDecimalFromReader(oReader, "difCuentaPorc");

                    result.Add(oItem);
                }

                //while (oReader.Read())
                //{

                //    ItemResumen oItem = new ItemResumen();
                //    oItem.Grupo = oReader["COD_GRUP"].ToString();
                //    oItem.Ventas = DataBase.GetDecimalFromReader(oReader, "sActual"); //(decimal)oReader["sActual"];
                //    oItem.PromedioVentas = DataBase.GetDecimalFromReader(oReader, "pAnterior"); //(decimal)oReader["sActual"];
                //    oItem.VentaTotal = DataBase.GetDecimalFromReader(oReader, "sActualTOTAL");
                //    oItem.VentaConsum = DataBase.GetDecimalFromReader(oReader, "SACTUALCONSUM"); //(decimal)oReader["SACTUALCONSUM"];
                //    oItem.Venta2 = DataBase.GetDecimalFromReader(oReader, "sAnterior"); //(decimal)oReader["sAnterior"];
                //    oItem.PromedioVentas2 = DataBase.GetDecimalFromReader(oReader, "pActual"); //(decimal)oReader["sActual"];

                //    oItem.Venta2Consum = DataBase.GetDecimalFromReader(oReader, "SANTERIORCONSUM");// (decimal)oReader["SANTERIORCONSUM"];
                //    oItem.Venta2Total = DataBase.GetDecimalFromReader(oReader, "SANTERIORTOTAL");
                //    oItem.DiferenciaVentas = DataBase.GetDecimalFromReader(oReader, "difCantidad"); //(decimal)oReader["difCantidad"];//Double.Parse(DataBase.GetDoubleFromReader(oReader, "difCantidad").ToString());
                //    oItem.DiferenciaVentasPorc = DataBase.GetDecimalFromReader(oReader, "difCantidadPorc"); //(decimal)oReader["difCantidadPorc"];// Double.Parse(DataBase.GetDoubleFromReader(oReader, "difCantidadPorc").ToString());
                //    oItem.Cuenta2 = int.Parse(oReader["cAnterior"].ToString());
                //    oItem.Cuenta = int.Parse(oReader["cActual"].ToString());
                //    oItem.DiferenciaCuenta = int.Parse(oReader["difCuenta"].ToString());
                //    oItem.DiferenciaCuentaPorc = DataBase.GetDecimalFromReader(oReader, "difCuentaPorc");// (decimal)oReader["difCuentaPorc"];//((oItem.Cuenta - oItem.Cuenta2) / oItem.Cuenta2) * 100;
                //    result.Add(oItem);

                //}


                return result;
            }
            catch (Exception ex)
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

        public static List<ItemResumen> GetResumenFacturacionEvolutivaAcumulado(string start, string end, FiltrosBusquedaExploracion oFiltros)
        {
            FbConnection oConexion = new FbConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ConexionBD"].ConnectionString);
            FbCommand oCommand = null;
            try
            {
                FbDataReader oReader = null;
                string fechaInicialAnyoActual = DateTime.Parse(start).AddYears(-5).ToString("MM-dd-yyyy");
                string fechaFinallAnyoActual = DateTime.Parse(end).ToString("MM-dd-yyyy");
                List<ItemResumen> result = new List<ItemResumen>();
                int anyo = oFiltros.anyo;
                string ior_mutua = "'(";
                int centro = 0;
                int ior_gpr = 0;
                int ior_medico = 0;
                int ior_colegiado = 0;
                string pagado = "'A'";
                string facturado = "'A'";

                foreach (int item in oFiltros.MutuaList)
                {
                    ior_mutua += item + ",";
                }
                ior_mutua += ")'";
                if (oFiltros.oidMedicoInformante > 0)
                {
                    ior_medico = oFiltros.oidMedicoInformante;
                }
                if (oFiltros.IOR_COLEGIADO > 0)
                {
                    ior_colegiado = oFiltros.IOR_COLEGIADO;
                }
                if (oFiltros.oidExploracion > 0)
                {
                    ior_gpr = oFiltros.oidExploracion;

                }
                if (oFiltros.oidCentro > 0)
                {
                    centro = oFiltros.oidCentro;
                }
                if (!String.IsNullOrEmpty(oFiltros.pagado) && oFiltros.pagado != "-1")
                {
                    pagado = oFiltros.pagado.QuotedString();
                }
                if (!String.IsNullOrEmpty(oFiltros.facturado) && oFiltros.pagado != "-1")
                {
                    facturado = oFiltros.facturado.QuotedString();
                }

                string query = "SELECT * " +
                        " FROM FACTURACION_EVOLUTIVA  ('" + fechaInicialAnyoActual + "', '" + fechaFinallAnyoActual + "',"
                        + ior_mutua + "," + centro + "," + pagado + "," + ior_gpr + "," + ior_colegiado + "," + ior_medico + "," + facturado + " ) p";

                if (oFiltros.oidGrupoAparato > 0)
                {
                    GAPARATOS oGrupo = GAparatoRepositorio.Obtener(oFiltros.oidGrupoAparato);

                    query = query + " where p.COD_GRUP=" + oGrupo.COD_GRUP.QuotedString();
                }

                oConexion.Open();
                oCommand = new FbCommand(query, oConexion);

                oReader = oCommand.ExecuteReader();
                while (oReader.Read())
                {

                    ItemResumen oItem = new ItemResumen();
                    oItem.Grupo = oReader["COD_GRUP"].ToString();
                    oItem.Anyo = oReader["ANUAL"].ToString();
                    oItem.Ventas = DataBase.GetDecimalFromReader(oReader, "SEXPLO"); //(decimal)oReader["sActual"];
                    oItem.PromedioVentas = DataBase.GetDecimalFromReader(oReader, "PEXPLO"); //(decimal)oReader["sActual"];
                    oItem.VentaTotal = DataBase.GetDecimalFromReader(oReader, "STOTAL");
                    oItem.VentaConsum = DataBase.GetDecimalFromReader(oReader, "SCONSUM"); //(decimal)oReader["SACTUALCONSUM"];

                    oItem.Cuenta = int.Parse(oReader["CEXPLO"].ToString());

                    result.Add(oItem);

                }


                return result;
            }
            catch (Exception ex)
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

        public static List<ItemFacturasMes> GetFacturasMeses(string start, string end, FiltrosBusquedaExploracion oFiltros)
        {
            FbConnection oConexion = new FbConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ConexionBD"].ConnectionString);
            FbCommand oCommand = null;
            try
            {
                FbDataReader oReader = null;
                string fechaInicialAnyoActual = DateTime.Parse(start).ToString("MM-dd-yyyy");
                string fechaFinallAnyoActual = DateTime.Parse(end).ToString("MM-dd-yyyy");
                List<ItemFacturasMes> result = new List<ItemFacturasMes>();
                int anyo = oFiltros.anyo;
                string ior_mutua = "'(";

                int ior_gpr = 0;
                int centro = -1;

                if (oFiltros.MutuaList == null)
                {
                    ior_mutua += "0,";
                }
                else
                {
                    foreach (int item in oFiltros.MutuaList)
                    {
                        ior_mutua += item + ",";
                    }
                }

                ior_mutua += ")'";

                if (oFiltros.oidExploracion > 0)
                {
                    ior_gpr = oFiltros.oidExploracion;
                }
                if (oFiltros.oidCentro > 0)
                {
                    centro = oFiltros.oidCentro;

                }
                string query = "";

                query = "SELECT m.NOMBRE,p.*   " +
                    " FROM FACTURACION_FACTURAS_MES  ('" + fechaInicialAnyoActual + "', '" + fechaFinallAnyoActual + "',"
                    + ior_mutua + "," + ior_gpr + "," + oFiltros.oidCentro + " )  p left join mutuas m on p.CODMUT=m.CODMUT";


                if (!String.IsNullOrEmpty(oFiltros.DescMutua))
                {

                    query = query + " where p.CODMUT=" + oFiltros.DescMutua.QuotedString();
                }

                oConexion.Open();
                oCommand = new FbCommand(query, oConexion);

                oReader = oCommand.ExecuteReader();
                while (oReader.Read())
                {

                    ItemFacturasMes oItem = new ItemFacturasMes();
                    oItem.Mutua = oReader["CODMUT"].ToString();
                    oItem.MutuaNombre = oReader["NOMBRE"].ToString();
                    oItem.Enero = DataBase.GetDecimalFromReader(oReader, "Enero"); //(decimal)oReader["sActual"];
                    oItem.Febrero = DataBase.GetDecimalFromReader(oReader, "Febrero"); //(decimal)oReader["sActual"];
                    oItem.Marzo = DataBase.GetDecimalFromReader(oReader, "Marzo"); //(decimal)oReader["sActual"];
                    oItem.Abril = DataBase.GetDecimalFromReader(oReader, "Abril"); //(decimal)oReader["sActual"];
                    oItem.Mayo = DataBase.GetDecimalFromReader(oReader, "Mayo"); //(decimal)oReader["sActual"];
                    oItem.Junio = DataBase.GetDecimalFromReader(oReader, "Junio"); //(decimal)oReader["sActual"];
                    oItem.Julio = DataBase.GetDecimalFromReader(oReader, "Julio"); //(decimal)oReader["sActual"];
                    oItem.Agosto = DataBase.GetDecimalFromReader(oReader, "Agosto"); //(decimal)oReader["sActual"];
                    oItem.Septiembre = DataBase.GetDecimalFromReader(oReader, "Septiembre"); //(decimal)oReader["sActual"];
                    oItem.Octubre = DataBase.GetDecimalFromReader(oReader, "Octubre");
                    oItem.Noviembre = DataBase.GetDecimalFromReader(oReader, "Noviembre"); //(decimal)oReader["SACTUALCONSUM"];
                    oItem.Diciembre = DataBase.GetDecimalFromReader(oReader, "Diciembre"); //(decimal)oReader["SACTUALCONSUM"];
                    oItem.Total = DataBase.GetDecimalFromReader(oReader, "Total"); //(decimal)oReader["SACTUALCONSUM"];


                    result.Add(oItem);

                }


                return result;
            }
            catch (Exception ex)
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

        public static List<ItemEstadisticoVentas> GetFacturacionDeUnMes(string start, string end)
        {


            FbConnection oConexion = new FbConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ConexionBD"].ConnectionString);
            FbCommand oCommand = null;
            try
            {
                FbDataReader oReader = null;
                List<ItemEstadisticoVentas> result = new List<ItemEstadisticoVentas>();
                //string query = "select R.OID, EXTRACT(DAY FROM E.FECHA) DIA, COUNT(e.oid),SUM(e.cantidad) from EXPLORACION" +
                // "   e JOIN GPR R ON R.OID=E.IOR_GPR where e.ESTADO='3' and (E.FECHA>=" + DateTime.Parse(start).ToString("MM-dd-yyyy").QuotedString() +
                // " and  E.FECHA<=" + DataBase.QuotedString(DateTime.Parse(end).ToString("MM-dd-yyyy")) + ") " +
                // " group by EXTRACT(MONTH FROM E.FECHA), EXTRACT(DAY FROM E.FECHA),R.OID order by EXTRACT(MONTH FROM E.FECHA), EXTRACT(DAY FROM E.FECHA),R.OID";

                string query = "select  E.FECHA DIA, 'mutua' AS TIPO,count(e.OID) as recuento ,CAST(SUM(e.cantidad) AS NUMERIC(9,2)) as suma " +
"from EXPLORACION e where e.ESTADO = '3' and e.ior_gpr = 2 and(E.FECHA  BETWEEN " + DateTime.Parse(start).ToString("MM-dd-yyyy").QuotedString() + " AND " + DataBase.QuotedString(DateTime.Parse(end).ToString("MM-dd-yyyy")) + ") group by E.FECHA,'mutua'" +
" UNION select E.FECHA DIA,'privados' as tipo, count(e.oid) as recuento,CAST(SUM(e.cantidad) AS NUMERIC(9, 2)) as suma " +
"from EXPLORACION e where e.ESTADO = '3' and e.ior_gpr = 1 and(E.FECHA  BETWEEN " + DateTime.Parse(start).ToString("MM-dd-yyyy").QuotedString() + " AND " + DataBase.QuotedString(DateTime.Parse(end).ToString("MM-dd-yyyy")) + ") group by E.FECHA,'privados'" +

" UNION SELECT DISTINCT E.FECHA DIA,'privados' AS TIPO, 0 AS recuento, CAST(0 AS NUMERIC(9, 2)) as suma " +
" from EXPLORACION E where (E.FECHA  BETWEEN " + DateTime.Parse(start).ToString("MM-dd-yyyy").QuotedString() + " AND " + DataBase.QuotedString(DateTime.Parse(end).ToString("MM-dd-yyyy")) + ") AND (SELECT COUNT(ee.oid) from EXPLORACION ee WHERE ee.ESTADO = '3' and ee.ior_gpr = 1 and(EE.FECHA = E.FECHA)) < 1 " +
" UNION SELECT DISTINCT E.FECHA DIA, 'mutua' AS TIPO, 0 AS recuento, CAST(0 AS NUMERIC(9, 2)) as suma " +
" from EXPLORACION E where (E.FECHA  BETWEEN " + DateTime.Parse(start).ToString("MM-dd-yyyy").QuotedString() + " AND " + DataBase.QuotedString(DateTime.Parse(end).ToString("MM-dd-yyyy")) + ") AND (SELECT COUNT(ee.oid) from EXPLORACION ee WHERE ee.ESTADO = '3' and ee.ior_gpr = 2 and(EE.FECHA = E.FECHA)) < 1 " +
" ORDER BY 1,2 ";
                oConexion.Open();
                oCommand = new FbCommand(query, oConexion);

                oReader = oCommand.ExecuteReader();

                while (oReader.Read())
                {
                    string oVentas = DataBase.GetDecimalFromReader(oReader, "SUMA").ToString("f2");
                    result.Add((new ItemEstadisticoVentas
                    {
                        Grupo = DataBase.GetStringFromReader(oReader, "TIPO").ToString().Trim(),
                        Cuenta = DataBase.GetIntFromReader(oReader, "recuento"),
                        Ventas = Double.Parse(oVentas),
                        Mes =DataBase.GetDateTimeFromReader( oReader,"DIA").Value.Day.ToString()
                    }));
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
                {
                    oConexion.Close();
                    if (oCommand != null)
                    {
                        oCommand.Dispose();
                    }
                }
            }
        }


        public static List<ItemEstadisticoVentas> GetFacturacionAnualPorPRIMUTICS(string year = "")
        {
            if (string.IsNullOrEmpty(year))
            {
                year = DateTime.Now.Year.ToString();
            }

            FbConnection oConexion = new FbConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ConexionBD"].ConnectionString);
            oConexion.Open();
            FbCommand oCommand = null;
            try
            {
                string query = "select R.GRUPO,COUNT(e.oid),SUM(e.cantidad) from EXPLORACION e JOIN GPR R ON R.OID=E.IOR_GPR where e.ESTADO='3' AND  EXTRACT(YEAR FROM E.FECHA)=" + year + " GROUP BY R.GRUPO";
                oCommand = new FbCommand(query, oConexion);
                FbDataReader oReader = oCommand.ExecuteReader();
                List<ItemEstadisticoVentas> result = new List<ItemEstadisticoVentas>();

                while (oReader.Read())
                {
                    string oVentas = DataBase.GetDoubleFromReader(oReader, "SUM").ToString("f0");

                    result.Add((
                        new ItemEstadisticoVentas
                        {
                            Grupo = DataBase.GetStringFromReader(oReader, "GRUPO"),
                            Cuenta = DataBase.GetIntFromReader(oReader, "COUNT"),
                            Ventas = Double.Parse(oVentas),
                            Anyo = year
                        }));
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
                {
                    oConexion.Close();
                    if (oCommand != null)
                    {
                        oCommand.Dispose();
                    }
                }
            }
        }


        public static List<ItemEstadisticoVentas> GetFacturacionMensualPorGrupo(string fechaInicial, string fechaFinal)
        {

            FbConnection oConexion = new FbConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ConexionBD"].ConnectionString);
            FbCommand oCommand = null;
            try
            {
                FbDataReader oReader = null;
                List<ItemEstadisticoVentas> result = new List<ItemEstadisticoVentas>();
                string query = "select  G.COD_GRUP, EXTRACT(MONTH FROM E.FECHA) mes,EXTRACT(YEAR FROM E.FECHA) ANYO,round(SUM(e.cantidad) , 0)  , count(e.oid)" +
                 " from EXPLORACION e JOIN GAPARATOS G ON G.OID = E.IOR_GRUPO where e.ESTADO = '3'  and e.IOR_GRUPO <> 10 and e.CANTIDAD > 0 and  E.FECHA >= '" +
                     DateTime.Parse(fechaInicial).ToString("yyyy-MM-dd") + "' and E.FECHA <='" + DateTime.Parse(fechaFinal).ToString("yyyy-MM-dd") +
                     "'  group by G.COD_GRUP, EXTRACT(MONTH FROM E.FECHA), EXTRACT(YEAR FROM E.FECHA) HAVING SUM(e.cantidad) > 0 order by  EXTRACT(MONTH FROM E.FECHA),G.COD_GRUP, EXTRACT(YEAR FROM E.FECHA) desc";

                oConexion.Open();
                oCommand = new FbCommand(query, oConexion);

                oReader = oCommand.ExecuteReader();


                while (oReader.Read())
                {
                    string oVentas = DataBase.GetDoubleFromReader(oReader, "SUM").ToString("f2");

                    result.Add((new ItemEstadisticoVentas
                    {
                        Mes = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(System.Globalization.DateTimeFormatInfo.CurrentInfo.GetMonthName(int.Parse(oReader["mes"].ToString()))),
                        Grupo = DataBase.GetStringFromReader(oReader, "COD_GRUP"),
                        Cuenta = DataBase.GetIntFromReader(oReader, "COUNT"),
                        Ventas = Double.Parse(oVentas),
                        Anyo = DateTime.Parse(fechaInicial).Year.ToString()
                    }));
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
                {
                    oConexion.Close();
                    if (oCommand != null)
                    {
                        oCommand.Dispose();
                    }
                }
            }

        }


        public static List<ItemEstadisticoVentas> GetFacturacionAnualPorGrupo(string year = "")
        {
            if (string.IsNullOrEmpty(year))
            {
                year = DateTime.Now.Year.ToString();
            }
            FbCommand oCommand = null;
            FbConnection oConexion = new FbConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ConexionBD"].ConnectionString);

            try
            {
                FbDataReader oReader = null;
                List<ItemEstadisticoVentas> result = new List<ItemEstadisticoVentas>();
                string query = "select G.COD_GRUP,COUNT(e.oid),SUM(e.cantidad) from EXPLORACION e JOIN GAPARATOS G ON G.OID=E.IOR_GRUPO where e.ESTADO='3' and EXTRACT(YEAR FROM E.FECHA)=" + year + "  GROUP BY G.COD_GRUP order by SUM(e.cantidad) desc";
                oConexion.Open();
                oCommand = new FbCommand(query, oConexion);

                oReader = oCommand.ExecuteReader();
                while (oReader.Read())
                {
                    string oVentas = DataBase.GetDoubleFromReader(oReader, "SUM").ToString("f2");

                    result.Add((new ItemEstadisticoVentas
                    {
                        Grupo = DataBase.GetStringFromReader(oReader, "COD_GRUP"),
                        Cuenta = DataBase.GetIntFromReader(oReader, "COUNT"),
                        Ventas = Double.Parse(oVentas),
                        Anyo = year
                    }));
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
                {
                    oConexion.Close();
                    if (oCommand != null)
                    {
                        oCommand.Dispose();
                    }
                }
            }
        }




    }
}