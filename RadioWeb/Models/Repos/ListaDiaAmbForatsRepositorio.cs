using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using FirebirdSql.Data.FirebirdClient;
using RadioWeb.Models;
using RadioWeb.Utils;
using RadioWeb.Models.Logica;
using System.Drawing;
using System.Globalization;
using ADPM.Common;

namespace RadioWeb.Models.Repos
{

    public class ListaDiaAmbForatsRepositorio
    {

        public static LISTADIAAMBFORATS Obtener(int oid)
        {
            LISTADIAAMBFORATS oResult = new LISTADIAAMBFORATS();


            LISTADIA item = ListaDiaRepositorio.Obtener(oid);

            CastListDiaToListDiaAmbForats(item, ref oResult, 0, 0);

            return oResult;

        }
        public static string ColorDelphiToHml(string code)
        {
            string result = "";
            result = '#' + code.Substring(6, 2) + code.Substring(4, 2) + code.Substring(2, 2);
            if (code == "FFFFFFFF")
            {
                return "Black";
            }
            return result;

        }

        public static List<LISTADIAAMBFORATS> Get(FiltrosBusquedaExploracion oFiltros)
        {
            List<LISTADIAAMBFORATS> oResult = new List<LISTADIAAMBFORATS>();

            FbConnection oConexion = new FbConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ConexionBD"].ConnectionString);

            string queryHorario = "";
            FbCommand oCommand = null;

            try
            {
                List<DAPARATOS> ListaAparatos = DaparatoRepositorio.Lista();
                oFiltros.Borrados = (oFiltros.oidEstadoExploracion == 1 || oFiltros.oidEstadoExploracion > 3 ? "T" : "F");
                //bool whereiniciado = false;
                queryHorario = "SELECT * FROM AGENDADIARIA(" +
                    DateTime.Parse(oFiltros.Fecha).ToString("MM/dd/yyyy").QuotedString() + "," + oFiltros.oidAparato +
                    "," + oFiltros.oidCentro + "," + oFiltros.oidGrupoAparato + "," + oFiltros.oidMutua + "," + oFiltros.oidExploracion +
                    "," + DataBase.QuotedString(oFiltros.Borrados) + ")";
                bool clausulaWhere = false;
                if (!String.IsNullOrEmpty(oFiltros.informada) && oFiltros.informada != "null" & oFiltros.informada != "A")
                {
                    queryHorario = queryHorario + " where INFORMADA=" + oFiltros.informada.QuotedString();
                    clausulaWhere = true;
                }

                //si no hay filtro de aparato vamos a por el listadia a piñon fijo
                //if (oFiltros.oidAparato<0)
                //{
                //    string codfil = ListaAparatos.Single(a => a.OID == oFiltros.oidAparato).COD_FIL;
                //    queryHorario = "select * from listadia where (not estado in ('1', '4', '5')) and fecha=" + DateTime.Parse(oFiltros.Fecha).ToString("MM/dd/yyyy").QuotedString();

                //}

                if (!String.IsNullOrEmpty(oFiltros.facturado) && oFiltros.facturado != "null" & oFiltros.facturado != "A")
                {
                    if (clausulaWhere)
                    {
                        queryHorario = queryHorario + " and facturada=" + oFiltros.facturado.QuotedString();
                    }
                    else
                    {
                        queryHorario = queryHorario + " where facturada=" + oFiltros.facturado.QuotedString();
                        clausulaWhere = true;
                    }
                }

                if (!String.IsNullOrEmpty(oFiltros.pagado) && oFiltros.pagado != "null" & oFiltros.pagado != "A")
                {
                    if (clausulaWhere)
                    {
                        queryHorario = queryHorario + " and PAGADO=" + oFiltros.pagado.QuotedString();
                    }
                    else
                    {
                        queryHorario = queryHorario + " where PAGADO=" + oFiltros.pagado.QuotedString();
                        clausulaWhere = true;
                    }
                }

                queryHorario = queryHorario.Replace("-1", "0");
                queryHorario = queryHorario.Replace("-2", "0");

                if (oFiltros.oidMedicoInformante > 0)
                {
                    if (oFiltros.busquedaTotalPorMedico == "T")
                    {
                        
                        queryHorario = "select * from listadia where IOR_MEDICO=" + oFiltros.oidMedicoInformante +
                            " and(not estado in ('1', '4', '5')) and INFORMADA <> 'T' and (fecha > '01/10/2017') order by fecha desc";
                    }
                    else
                    {
                        if (clausulaWhere)
                        {
                            queryHorario = queryHorario + " and IOR_MEDICO=" + oFiltros.oidMedicoInformante;
                        }
                        else
                        {
                            queryHorario = queryHorario + " where IOR_MEDICO=" + oFiltros.oidMedicoInformante;
                            clausulaWhere = true;
                        }
                    }

                }

                if (!String.IsNullOrEmpty(oFiltros.Paciente) && oFiltros.Paciente.Length > 2 && oFiltros.Paciente != "UNDEFINED")
                {
                    int oidExploracionBusqueda;
                    if (int.TryParse(oFiltros.Paciente, out oidExploracionBusqueda))
                    {
                        queryHorario = "select * from listadia where oid=" + oFiltros.Paciente;
                    }
                    else if (oFiltros.busquedaTotal == "T")
                    {
                        queryHorario = "select * from listadia where IOR_PACIENTE=" + oFiltros.iorPaciente + " order by fecha desc";
                    }

                    else
                    {
                        if (clausulaWhere)
                        {
                            queryHorario = queryHorario + " and PACIENTE like '%" + oFiltros.Paciente.ToUpper() + "%'";
                        }
                        else
                        {
                            queryHorario = queryHorario + " where PACIENTE like '%" + oFiltros.Paciente.ToUpper() + "%'";
                            clausulaWhere = true;
                        }

                    }
                }

                oConexion.Open();

                oCommand = new FbCommand(queryHorario, oConexion);

                FbDataReader oReader = oCommand.ExecuteReader();
                bool hayHorario = oFiltros.oidAparato > 0;// oReader.GetSchemaTable().Columns.Contains("ORDER_HHORA");
                //   RadioDBContext db = new RadioDBContext();
                while (oReader.Read())
                {
                    LISTADIAAMBFORATS oTemp = new LISTADIAAMBFORATS();
                    oTemp.OID = DataBase.GetIntFromReader(oReader, "OID");
                    oTemp.ACTIVA = (oTemp.OID == oFiltros.oidExploracionSeleccionada ? true : false);
                    if (oTemp.OID > -1)
                    {
                        string esperaTime = "";
                        int pEstado = DataBase.GetIntFromReaderString(oReader, "ESTADO");
                        DateTime pFecha = (DateTime)DataBase.GetDateTimeFromReader(oReader, "FECHA");
                        string pHoraLlegada = DataBase.GetStringFromReader(oReader, "HORA_LL");
                        string pHoraExploracion = DataBase.GetStringFromReader(oReader, "HORA_EX");
                        string pTexto = DataBase.GetStringFromReader(oReader, "TEXTO");
                        //Si el estado es PRESENCIA Y ESTAMOS EN EL DIA DE HOY
                        if (pEstado == 2 && pFecha.CompareTo(DateTime.Now) == 0)
                        {
                            esperaTime = "00:00";

                        }
                        if ((!string.IsNullOrEmpty(pHoraLlegada)))
                        {
                            if ((!string.IsNullOrEmpty(pHoraExploracion)))
                            {
                                esperaTime = CalcularEspera(pHoraLlegada, pHoraExploracion);
                            }
                            else
                            {
                                // si estamos de hoy
                                if (pFecha.CompareTo(DateTime.Now) == 0)
                                {
                                    esperaTime = CalcularEspera(pHoraLlegada, DateTime.Now.ToString("HH:mm"));
                                }
                            }
                        }

                        string subtexto = "";
                        if ((!string.IsNullOrEmpty(pTexto)) && (pTexto.Length > 30))
                        {
                            subtexto = pTexto.Substring(0, 15) + "...";
                        }
                        else
                        {
                            subtexto = pTexto;
                        }

                        try
                        {
                          
                            if (hayHorario)
                            {
                                oTemp.ORDER_HHORA = DataBase.GetIntFromReaderString(oReader, "ORDER_HHORA");
                                oTemp.ORDER_HORA = DataBase.GetIntFromReaderString(oReader, "ORDER_HORA");

                            }
                            else
                            {
                                oTemp.ORDER_HHORA = 0;
                                oTemp.ORDER_HORA = 0;
                            }

                        }
                        catch (Exception)
                        {
                            oTemp.ORDER_HHORA = 0;
                        }

                        

                        oTemp.APLAZADO = DataBase.GetBoolFromReader(oReader, "APLAZADO");
                        oTemp.ANULACONSENTIMIENTO = (DataBase.GetStringFromReader(oReader, "BORRADO") == "A" ? true : false);


                        try
                        {
                            oTemp.CANTIDAD = DataBase.GetDoubleFromReader(oReader, "CANTIDAD");

                        }
                        catch (Exception ex)
                        {
                            float number = DataBase.GetFloatFromReader(oReader, "CANTIDAD");
                            oTemp.CANTIDAD = double.Parse( number.ToString());// DataBase.GetDoubleFromReader(oReader, "CANTIDAD");
                        }

                        oTemp.CIDCOLOR = GetColorByCID(DataBase.GetIntFromReader(oReader, "CID"));
                        oTemp.CID = DataBase.GetIntFromReader(oReader, "CID");
                        oTemp.COD_MED = DataBase.GetStringFromReader(oReader, "COD_MED");
                        oTemp.COD_MUT = DataBase.GetStringFromReader(oReader, "COD_MUT");
                        oTemp.COD_FIL = DataBase.GetStringFromReader(oReader, "COD_FIL");
                        try
                        {
                            oTemp.DES_MUT = DataBase.GetStringFromReader(oReader, "MUTUA");
                            oTemp.DES_FIL = DataBase.GetStringFromReader(oReader, "EXPLO");
                            oTemp.QRCOMPARTIRCASO = DataBase.GetBoolFromReader(oReader, "QRCOMPARTIRCASO");
                        }
                        catch (Exception)
                        {


                        }

                        oTemp.ESPERA = esperaTime;
                        oTemp.ESTADO = DataBase.GetStringFromReader(oReader, "ESTADO");
                        oTemp.FACTURADA = DataBase.GetBoolFromReader(oReader, "FACTURADA");
                        oTemp.EDAD= DataBase.GetIntFromReader(oReader, "EDAD");
                        oTemp.FIL = DataBase.GetStringFromReader(oReader, "FIL");
                        oTemp.GRUPOAPA = DataBase.GetIntFromReader(oReader, "GRUPOAPA");
                        oTemp.HAYCONSUMIBLE = DataBase.GetBoolFromReader(oReader, "HAYCONSUMIBLE");
                        oTemp.HAYCOMEN = DataBase.GetBoolFromReader(oReader, "HAYCOMEN");
                        oTemp.HORA = DataBase.GetStringFromReader(oReader, "HORA");
                        oTemp.HORA_LL = DataBase.GetStringFromReader(oReader, "HORA_LL");
                        oTemp.HORA_EX = DataBase.GetStringFromReader(oReader, "HORA_EX");
                        oTemp.INFORMADA = DataBase.GetBoolFromReader(oReader, "INFORMADA");
                        try
                        {
                            oTemp.IOR_MASTER = DataBase.GetIntFromReader(oReader, "IOR_MASTER");
                        }
                        catch (Exception)
                        {

                            oTemp.IOR_MASTER = -1;
                        }

                        oTemp.FIRMA_CONSEN = DataBase.GetBoolFromReader(oReader, "FIRMA_CONSEN");
                        oTemp.INTOCABLE = DataBase.GetBoolFromReader(oReader, "INTOCABLE");
                        oTemp.IOR_PACIENTE = DataBase.GetIntFromReader(oReader, "IOR_PACIENTE");
                        oTemp.FECHAMAXENTREGA = DataBase.GetDateTimeFromReader(oReader, "FECHAMAXENTREGA");
                        oTemp.MEDICO = DataBase.GetStringFromReader(oReader, "MEDICO");
                        oTemp.NOFACTURAB = DataBase.GetBoolFromReader(oReader, "NOFACTURAB");
                        oTemp.PAGADO = DataBase.GetBoolFromReader(oReader, "PAGADO");
                        oTemp.PRIVADO = DataBase.GetIntFromReaderString(oReader, "PRIVADO");
                        oTemp.SIMBOLO = DataBase.GetStringFromReader(oReader, "SIMBOLO");
                        oTemp.TECNICO = DataBase.GetStringFromReader(oReader, "TECNICO");
                        oTemp.TEXTO = DataBase.GetStringFromReader(oReader, "TEXTO");
                        oTemp.SUBTEXTO = subtexto;
                        oTemp.VIP = DataBase.GetBoolFromReader(oReader, "VIP");
                        oTemp.VERS = DataBase.GetIntFromReaderString(oReader, "VERS");
                        oTemp.LOPD = DataBase.GetBoolFromReader(oReader, "LOPD");
                        oTemp.COLOR = GetColorByEstadoExploracion(DataBase.GetStringFromReader(oReader, "ESTADO"));
                        oTemp.IOR_APARATO = ListaAparatos.Single(a => a.COD_FIL == oTemp.COD_FIL).OID;
                        if (oTemp.ANULACONSENTIMIENTO)
                        {
                            oTemp.COLOR = "White";
                        }
                    }
                    else
                    {
                        try
                        {
                            if (oReader["ORDER_HHORA"] != System.DBNull.Value)
                            {
                                oTemp.ORDER_HHORA = int.Parse(oReader["ORDER_HHORA"].ToString());
                            }
                            else
                            {
                                oTemp.ORDER_HHORA = 0;
                            }
                        }
                        catch (Exception)
                        {
                            oTemp.ORDER_HHORA = 0;
                        }
                        try
                        {
                            oTemp.ORDER_HORA = DataBase.GetIntFromReaderString(oReader, "ORDER_HORA");
                        }
                        catch (Exception)
                        {

                            oTemp.ORDER_HORA = 0;
                        }
                        string colorcode = DataBase.GetStringFromReader(oReader, "COLOR");
                        if (colorcode.Contains("cl"))
                        {
                            oTemp.COLOR = colorcode.Replace("cl", "");
                        }
                        else
                        {
                            //int argb = Int32.Parse(colorcode.Replace("$", ""), NumberStyles.HexNumber);
                            //Color clr = Color.FromArgb(argb);
                            //oTemp.COLOR = System.Drawing.ColorTranslator.ToHtml(clr);
                            oTemp.COLOR = colorcode;
                        }
                        oTemp.IOR_APARATO = oFiltros.oidAparato;
                    }



                    oTemp.PACIENTE = DataBase.GetStringFromReader(oReader, "PACIENTE");


                    if (oTemp.PACIENTE.StartsWith(">"))
                    {
                        oTemp.ANULADA = true;
                        oTemp.COLOR = "Red";
                    }
                    else
                    {
                        if (oTemp.PACIENTE.StartsWith(">CAMBIO:") || oTemp.PACIENTE.Contains("RX/MAM"))
                        {
                            oTemp.ANULADA = false;
                            oTemp.COLOR = "Green";
                        }
                        else
                        {
                            if (oTemp.OID <= 0)
                            {
                                oTemp.ANULADA = false;
                                oTemp.COLOR = DataBase.GetStringFromReader(oReader, "COLOR");
                                if (oTemp.COLOR.StartsWith("cl"))
                                {
                                    oTemp.COLOR = oTemp.COLOR.Replace("cl", "");
                                }
                                else
                                {
                                    try
                                    {
                                        string colorcode = DataBase.GetStringFromReader(oReader, "COLOR");
                                        if (colorcode.Contains("cl"))
                                        {
                                            oTemp.COLOR = colorcode.Replace("cl", "");
                                        }
                                        else
                                        {
                                            //int argb = Int32.Parse(colorcode.Replace("$", ""), NumberStyles.HexNumber);
                                            //Color clr = Color.FromArgb(argb);
                                            //oTemp.COLOR = System.Drawing.ColorTranslator.ToHtml(clr);
                                            oTemp.COLOR = colorcode;
                                        }

                                    }
                                    catch (Exception)
                                    {


                                    }
                                }
                            }


                        }
                    }



                    oTemp.FECHA = (DateTime)DataBase.GetDateTimeFromReader(oReader, "FECHA");

                    try
                    {
                        if (oFiltros.busquedaTotalPorMedico=="T" ||
                            ( oFiltros.busquedaTotal == "T" && !String.IsNullOrEmpty(oFiltros.Paciente) && oFiltros.Paciente.Length > 0))
                        {
                            oTemp.HHORA = oTemp.FECHA.ToShortDateString();
                        }
                        else
                        {
                            if (hayHorario)
                            {
                                oTemp.HHORA = DataBase.GetStringFromReader(oReader, "HHORA");
                                string colorcode = DataBase.GetStringFromReader(oReader, "COLOR");
                               
                                    oTemp.COLOR_HORARIO =(colorcode.Contains("cl")? colorcode.Replace("cl", ""):colorcode);
                                
                            }
                            else
                            {
                                oTemp.HHORA = "-";
                            }
                           

                        }
                    }
                    catch (Exception)
                    {
                        oTemp.HHORA = "-";

                    }
                   


                    oResult.Add(oTemp);
                }
                // db.Dispose();
            }
            catch (Exception ex)
            {

                throw ex;
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



        public static List<LISTADIAAMBFORATS> RellenarListaAmbForats(List<HORAS_HORARIO> oHorarios, List<LISTADIA> oListaDia)
        {
            List<LISTADIAAMBFORATS> oResult = new List<LISTADIAAMBFORATS>();
            //primero llenamos con el horario
            int vOrder = 0;
            int vHOrder = 0;
            //rellenamos las horas del horario
            if (oHorarios != null)
            {
                foreach (HORAS_HORARIO item in oHorarios)
                {
                    LISTADIAAMBFORATS oTemporalAmbForats = new LISTADIAAMBFORATS();
                    oTemporalAmbForats.ANULADA = item.ANULADA;
                    oTemporalAmbForats.HHORA = item.HORA;
                    oTemporalAmbForats.FECHA = item.FECHA;
                    vOrder += 1;
                    oTemporalAmbForats.ORDER_HHORA = vOrder;
                    oTemporalAmbForats.COLOR_HORARIO = item.COLOR.Replace("$", "#").Replace("FFFFFFFF", "0000000");
                    oTemporalAmbForats.COLOR = item.COLOR.Replace("$", "#");
                    oTemporalAmbForats.PACIENTE = item.TEXTODEFECTO;
                    oTemporalAmbForats.INTOCABLE = false;
                    oResult.Add(oTemporalAmbForats);
                }
            }
            //ahora iteramos sobre el listadia

            foreach (LISTADIA item in oListaDia)
            {
                vHOrder = 0;
                string HoraHorario = AveriguaHorario(oHorarios, item.HORA);
                string ColorHorario = AveriguaColor(oHorarios, item.HORA);
                //esto devuelve un objeto temporal si en la lista amb forats hay una hora igual que la de averiguahorario
                LISTADIAAMBFORATS oTemporal = oResult.Find(delegate (LISTADIAAMBFORATS hora)
                    {
                        return hora.HHORA == HoraHorario;
                    });

                //si no encuentro en el array de listahuecoambforats un objeto con el mismo hhora que este listadia añado uno
                if (oTemporal == null)
                {
                    LISTADIAAMBFORATS oListaDiaHuecoOcupado = new LISTADIAAMBFORATS();
                    oListaDiaHuecoOcupado.COLOR_HORARIO = ColorHorario;
                    oResult.Add(CastListDiaToListDiaAmbForats(item, ref oListaDiaHuecoOcupado, vHOrder, vOrder));
                    vOrder = +1;
                }
                //si si que lo he encontrado pueden pasar dos cosas
                else
                {
                    vHOrder = oTemporal.ORDER_HHORA;
                    //que el oid sea igual a 0, en ese caso lo añado
                    if (oTemporal.OID == 0)
                    {

                        oTemporal = CastListDiaToListDiaAmbForats(item, ref oTemporal, vHOrder, vOrder);
                        oTemporal.COLOR_HORARIO = ColorHorario;
                        vOrder = +1;
                    }
                    else
                    {
                        LISTADIAAMBFORATS oListaDiaDoblado = new LISTADIAAMBFORATS();
                        oListaDiaDoblado = CastListDiaToListDiaAmbForats(item, ref oListaDiaDoblado, vHOrder, vOrder);
                        oListaDiaDoblado.COLOR_HORARIO = ColorHorario;
                        oResult.Add(oListaDiaDoblado);
                        vOrder = +1;
                    }
                }





            }
            return oResult;

        }

        private static string CalcularEspera(string hora1, string hora2)
        {
            string startTime = hora1;
            string endTime = hora2;
            TimeSpan duration = DateTime.Parse(endTime).Subtract(DateTime.Parse(startTime));
            return (duration.Hours.ToString("00") + ":" + duration.Minutes.ToString("00"));
        }


        public static LISTADIAAMBFORATS CastListDiaToListDiaAmbForats(LISTADIA oListaDia, ref LISTADIAAMBFORATS oListaDiaAmbForats, int vHOrder, int vOrder)
        {



            string esperaTime = "";
            //Si el estado es PRESENCIA Y ESTAMOS EN EL DIA DE HOY
            if (oListaDia.ESTADO == "2" && oListaDia.FECHA.CompareTo(DateTime.Now) == 0)
            {
                esperaTime = "00:00";

            }

            if ((!string.IsNullOrEmpty(oListaDia.HORA_LL)))
            {
                if ((!string.IsNullOrEmpty(oListaDia.HORA_EX)))
                {
                    esperaTime = CalcularEspera(oListaDia.HORA_LL, oListaDia.HORA_EX);
                }
                else
                {
                    // si estamos de hoy
                    if (oListaDia.FECHA.CompareTo(DateTime.Now) == 0)
                    {
                        esperaTime = CalcularEspera(oListaDia.HORA_LL, DateTime.Now.ToString("HH:mm"));
                    }

                }


            }


            string subtexto = "";

            if ((!string.IsNullOrEmpty(oListaDia.TEXTO)) && (oListaDia.TEXTO.Length > 30))
            {
                subtexto = oListaDia.TEXTO.Substring(0, 15) + "...";
            }
            else
            {
                subtexto = oListaDia.TEXTO;
            }


            oListaDiaAmbForats.APLAZADO = oListaDia.APLAZADO;
            oListaDiaAmbForats.FIRMA_CONSEN = oListaDia.FIRMA_CONSEN;
            oListaDiaAmbForats.LOPD = oListaDia.LOPD;
            oListaDiaAmbForats.IOR_MASTER = oListaDia.IOR_MASTER;
            oListaDiaAmbForats.QRCOMPARTIRCASO = oListaDia.QRCOMPARTIRCASO;
            oListaDiaAmbForats.CANTIDAD = oListaDia.CANTIDAD.Value;
            oListaDiaAmbForats.CID = oListaDia.CID;
            oListaDiaAmbForats.CIDCOLOR = GetColorByCID(oListaDia.CID);
            oListaDiaAmbForats.COD_MED = oListaDia.COD_MED;
            oListaDiaAmbForats.COD_MUT = oListaDia.COD_MUT;
            oListaDiaAmbForats.DES_MUT = oListaDia.MUTUA;

            oListaDiaAmbForats.COD_FIL = oListaDia.COD_FIL;
            oListaDiaAmbForats.DES_FIL = oListaDia.EXPLO;

            oListaDiaAmbForats.COLOR = GetColorByEstadoExploracion(oListaDia.ESTADO);
            oListaDiaAmbForats.ESPERA = esperaTime;
            oListaDiaAmbForats.ESTADO = oListaDia.ESTADO;
            oListaDiaAmbForats.FECHA = oListaDia.FECHA;
            oListaDiaAmbForats.FACTURADA = oListaDia.FACTURADA;
            oListaDiaAmbForats.FIL = oListaDia.FIL;
            oListaDiaAmbForats.GRUPOAPA = oListaDia.GRUPOAPA;
            oListaDiaAmbForats.HAYCONSUMIBLE = oListaDia.HAYCONSUMIBLE;
            oListaDiaAmbForats.HAYCOMEN = oListaDia.HAYCOMEN;
            oListaDiaAmbForats.HORA = oListaDia.HORA;
            oListaDiaAmbForats.HORA_LL = oListaDia.HORA_LL;
            oListaDiaAmbForats.HORA_EX = oListaDia.HORA_EX;
          //  oListaDiaAmbForats.INFORMADA = oListaDia.INFORMADA;
            oListaDiaAmbForats.INFORMADA = (oListaDia.INFORMADA == "T" ? true : false);

            oListaDiaAmbForats.INTOCABLE = oListaDia.INTOCABLE;
            oListaDiaAmbForats.IOR_PACIENTE = oListaDia.IOR_PACIENTE;
            oListaDiaAmbForats.FECHAMAXENTREGA = oListaDia.FECHAMAXENTREGA;
            oListaDiaAmbForats.MEDICO = oListaDia.MEDICO;
            oListaDiaAmbForats.NOFACTURAB = oListaDia.NOFACTURAB;
            oListaDiaAmbForats.OID = oListaDia.OID;
            oListaDiaAmbForats.ORDER_HHORA = vHOrder;
            oListaDiaAmbForats.ORDER_HORA = vOrder;
            oListaDiaAmbForats.PAGADO = oListaDia.PAGADO;
            oListaDiaAmbForats.PACIENTE = oListaDia.PACIENTE;
            oListaDiaAmbForats.PRIVADO = oListaDia.PRIVADO;
            oListaDiaAmbForats.SIMBOLO = oListaDia.SIMBOLO;
            oListaDiaAmbForats.TECNICO = oListaDia.TECNICO;
            oListaDiaAmbForats.TEXTO = oListaDia.TEXTO;
            oListaDiaAmbForats.SUBTEXTO = subtexto;
            oListaDiaAmbForats.VIP = oListaDia.VIP;
            oListaDiaAmbForats.VERS = oListaDia.VERS;
            oListaDiaAmbForats.IOR_APARATO = DaparatoRepositorio.Obtener(oListaDia.COD_FIL).OID;


            return oListaDiaAmbForats;
        }

        private static string GetColorByCID(int? colorCID)
        {
            string cssEstado = "";
            switch (colorCID)
            {

                case 0:
                    cssEstado = "white";
                    break;
                case 1:
                    cssEstado = "teal";

                    break;

                case 2:
                    cssEstado = "red";
                    break;
                //Exploración realizado
                case 3:
                    cssEstado = "lime";
                    break;

                case 4:
                    cssEstado = "yellow";

                    break;
                //Borrada, el paciente llamó anulando
                case 5:
                    cssEstado = "blue";
                    break;
                case 6:
                    cssEstado = "fuchsia";
                    break;
                case 7:
                    cssEstado = "aqua";
                    break;
                case 8:
                    cssEstado = "maroon";
                    break;
                case 9:
                    cssEstado = "green";
                    break;
                case 10:
                    cssEstado = "olive";
                    break;
                case 11:
                    cssEstado = "navy";
                    break;
                case 12:
                    cssEstado = "purple";
                    break;
                case 13:
                    cssEstado = "gray";
                    break;
                case 14:
                    cssEstado = "black";
                    break;


                default:
                    cssEstado = "";
                    break;
            }
            return cssEstado;
        }

        private static string GetEstadoDescExploracion(string EstadoExploracion)
        {
            string EstadoDesc = "";
            switch (EstadoExploracion)
            {
                //visita no ha llegado
                case "0":
                    EstadoDesc = "No llegado";
                    break;
                //Exploración anulada o borrada
                case "1":
                    EstadoDesc = "Borrada";

                    break;
                //Indica presencia, el paciente ha venido
                case "2":
                    EstadoDesc = "Presente";
                    break;
                //Exploración realizado
                case "3":
                    EstadoDesc = "Realizado";
                    break;
                //Borrada, el paciente no se ha presentado                                          
                case "4":
                    EstadoDesc = "No presen.";

                    break;
                //Borrada, el paciente llamó anulando
                case "5":
                    EstadoDesc = "Anula";
                    break;
                default:
                    EstadoDesc = "";
                    break;
            }
            return EstadoDesc;
        }

        public static string GetColorByEstadoExploracion(string EstadoExploracion)
        {
            string cssEstado = "";
            switch (EstadoExploracion)
            {
                //visita no ha llegado
                case "0":
                    cssEstado = "blue";
                    break;
                //Exploración anulada o borrada
                case "1":
                    cssEstado = "red";

                    break;
                //Indica presencia, el paciente ha venido
                case "2":
                    cssEstado = "green";
                    break;
                //Exploración realizado
                case "3":
                    cssEstado = "black";
                    break;
                //Borrada, el paciente no se ha presentado                                          
                case "4":
                    cssEstado = "brown";
                    break;
                //Borrada, el paciente llamó anulando
                case "5":
                    cssEstado = "purple";
                    break;
                default:
                    cssEstado = "";
                    break;
            }
            return cssEstado;
        }



        private static string AveriguaColor(List<HORAS_HORARIO> oHorarios, string vHora)
        {

            bool rValid = false;
            string Result = "";

            foreach (HORAS_HORARIO item in oHorarios)
            {
                if (rValid)
                {
                    if (string.Compare(item.HORA, vHora) > 0) { break; }
                    else { rValid = false; }
                }
                if (!rValid)
                {
                    if (string.Compare(item.HORA, vHora) <= 0)
                    {
                        Result = item.COLOR;
                        rValid = true;
                    }

                }

            }

            return Result;
        }

        //La función AveriguaHorario devuelve la hora real a la que corresponde una hora dada
        //(por ejemplo, he dado una hora a las 9:15 pero el horario solo tiene 9:00 y 9:300, la función devolverá 9:00)
        private static string AveriguaHorario(List<HORAS_HORARIO> oHorarios, string vHora)
        {

            bool rValid = false;
            string Result = "";

            foreach (HORAS_HORARIO item in oHorarios)
            {
                if (rValid)
                {
                    if (string.Compare(item.HORA, vHora) > 0) { break; }
                    else { rValid = false; }
                }
                if (!rValid)
                {
                    if (string.Compare(item.HORA, vHora) <= 0)
                    {
                        Result = item.HORA;
                        rValid = true;
                    }

                }

            }

            return Result;
        }


    }
}