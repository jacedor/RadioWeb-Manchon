using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using FirebirdSql.Data.FirebirdClient;
using RadioWeb.Models;
using RadioWeb.Utils;
using System.Data;
using RadioWeb.ViewModels;
using RadioWeb.ViewModels.Informes;
using ADPM.Common;
using RadioWeb.ViewModels.Exploracion;
using System.IO;

namespace RadioWeb.Models.Repos
{

    public class ExploracionRepositorio
    {
        private static void generarLineaPago(LISTADIA oExploracion)
        {
            WebConfigRepositorio oConfig = new WebConfigRepositorio();

            //Inserción del pago asociado a la exploración
            PAGOS oPago = new PAGOS
            {
                APLAZADO = "F",
                BORRADO = "F",
                TIPOPAGO = "-",
                CANTIDAD = 0,
                CID = 1377,
                DEUDA_CANTIDAD = oExploracion.CANTIDAD,
                FECHA = DateTime.Now,
                DEUDA_FECHA = oExploracion.FECHA,
                IOR_EMPRESA = 4,
                IOR_MONEDA = int.Parse(oConfig.ObtenerValor("IOR_MONEDA")),
                OWNER = oExploracion.OID
            };
            PagosRepositorio.Confirmar(oPago);
            foreach (EXP_CONSUM oConsumible in Exp_ConsumRepositorio.GetConsumiblesPendientes(oExploracion.OID))
            {
                //Inserción del pago asociado al Consumible
                PAGOS oPagoConsumible = new PAGOS
                {
                    APLAZADO = "F",
                    BORRADO = "F",
                    CANTIDAD = 0,
                    CID = 1378,
                    DEUDA_CANTIDAD = oConsumible.PRECIO,
                    FECHA = DateTime.Now,
                    DEUDA_FECHA = oExploracion.FECHA,
                    IOR_EMPRESA = 4,
                    IOR_MONEDA = int.Parse(oConfig.ObtenerValor("IOR_MONEDA")),
                    OWNER = oConsumible.OID,
                    TIPOPAGO = "V"
                };
                PagosRepositorio.Confirmar(oPagoConsumible);
            }
        }

        public static LISTADIA CambiarEstado(int estadoAnterior, int estadoNuevo, int oid, string rutaWL, bool YaEstaEntrado = false, string rutaWLAffidea = "")
        {
            WebConfigRepositorio oConfig = new WebConfigRepositorio();
            FbConnection oConexion = new FbConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ConexionBD"].ConnectionString);
            EXPLORACION.ESTADOS valorEstadoNuevo = (EXPLORACION.ESTADOS)estadoNuevo;
            EXPLORACION.ESTADOS valorEstadoAnterior = (EXPLORACION.ESTADOS)estadoAnterior;
            LISTADIA oTempExplo = ListaDiaRepositorio.Obtener(oid);
            bool pagoAntesConfirmacion = (oConfig.ObtenerValor("PagoAntesExploracion") == "T" ? true : false);

            //EXPLORACION oTempExplo = ExploracionRepositorio.Obtener(oid);
            if (estadoAnterior == estadoNuevo)
            {
                return oTempExplo;
            }

            string updateStament = "update exploracion set estado=" + estadoNuevo;
            string textoLog = "";

            switch (valorEstadoNuevo)
            {
                // SE QUIERE PASAR EL ESTADO NUEVO A PRESENCITA... ESTO PUEDE VENIR DE PENDIENTE O DE CONFIRMADO

                case EXPLORACION.ESTADOS.PRESENCIA:
                    if (String.IsNullOrEmpty(oTempExplo.HORA_LL))
                    {
                        //ponemos la hora de llegada LA HORA DE LLEGADA
                        updateStament += ", hora_ll='" + DateTime.Now.ToString("HH:mm") + "'";
                    }

                    if (valorEstadoAnterior == EXPLORACION.ESTADOS.PENDIENTE)
                    {
                        //PONEMOS LA HORA DE LLEGADA A NOW                      
                        textoLog = "Exploración Presencia";
                        if ((oTempExplo.COD_FIL.Contains("RX") && !YaEstaEntrado) || !oTempExplo.COD_FIL.Contains("RX"))
                        {
                            WorkListRepositorio.CrearFicheroWL(true, oTempExplo.FECHA.ToString(), oTempExplo.OID, rutaWL, true);
                            WorkListRepositorio.CrearFicheroWL(true, oTempExplo.FECHA.ToString(), oTempExplo.OID, rutaWLAffidea, true);
                        }
                        //si el parametro general indica que la exploracion se paga antes
                        // de hacerse, ponemos la lina de pago en el momento de poner en verde
                        if (oTempExplo.IOR_GPR == 1 && pagoAntesConfirmacion)
                        {
                            generarLineaPago(oTempExplo);
                        }
                    }
                    else if (valorEstadoAnterior == EXPLORACION.ESTADOS.CONFIRMADO)
                    {
                        //BORRAMOS LA LINEA DE PAGO GENERADA PARA TEMAS DE CONTABILIDAD
                        if (oTempExplo.IOR_GPR == 1)
                        {
                            PagosRepositorio.delete(oid);
                            //borramos tambien las lineas de pago de las exploraciones relacionadas
                            foreach (EXP_CONSUM oConsumible in Exp_ConsumRepositorio.GetConsumiblesPendientes(oTempExplo.OID))
                            {
                                PagosRepositorio.delete(oConsumible.OID);
                            }
                        }
                        textoLog = "Exploración anula confirmación";
                    }


                    break;
                //quitar presencia       
                case EXPLORACION.ESTADOS.PENDIENTE:

                    if (valorEstadoAnterior == EXPLORACION.ESTADOS.PRESENCIA)
                    {
                        //quitamos la hora de llegada
                        updateStament += ", hora_ll=null";
                        textoLog = "Anulada Presencia";
                        WorkListRepositorio.BorrarFicheroWL(oTempExplo, rutaWL);
                        WorkListRepositorio.BorrarFicheroWL(oTempExplo, rutaWLAffidea);
                        if (oTempExplo.IOR_GPR == 1 && pagoAntesConfirmacion)
                        {
                            PagosRepositorio.delete(oid);
                            //borramos tambien las lineas de pago de las exploraciones relacionadas
                            foreach (EXP_CONSUM oConsumible in Exp_ConsumRepositorio.GetConsumiblesPendientes(oTempExplo.OID))
                            {
                                PagosRepositorio.delete(oConsumible.OID);
                            }
                        }
                    }
                    else if (valorEstadoAnterior == EXPLORACION.ESTADOS.BORRADO || valorEstadoAnterior == EXPLORACION.ESTADOS.NOPRESENTADO || valorEstadoAnterior == EXPLORACION.ESTADOS.LLAMAANULANDO)
                    {
                        textoLog = "Recuperada exploración borrada";
                    }


                    break;
                //exploracion borrada
                case EXPLORACION.ESTADOS.BORRADO:

                    if (valorEstadoAnterior == EXPLORACION.ESTADOS.PENDIENTE)
                    {
                        textoLog = "Exploración borrada";

                    }
                    else
                    {

                        return oTempExplo;
                    }
                    break;
                //exploracion realizada
                case EXPLORACION.ESTADOS.CONFIRMADO:
                    //AL confirmar una exploración insertamos una linea en la tabla de pagos, tanto de la exploración, 
                    //como de los posibles consumibles asociados
                    if (oTempExplo.IOR_GPR == 1 && !pagoAntesConfirmacion)
                    {
                        generarLineaPago(oTempExplo);


                    }
                    //si el estado enterior era presencia tenemos que hacer calculos de la espera
                    if (valorEstadoAnterior == EXPLORACION.ESTADOS.PRESENCIA)
                    {
                        //IF HORA_LL<HORA THEN HORA_LL=HORA                                                                 
                        //IF HORA_EX<HORA THEN HORA_EX=HORA
                        //ESPERA (campo calculado)  = HORA_EX-HORA_LL

                        EXPLORACION oTempExplo1 = ExploracionRepositorio.Obtener(oid);
                        TimeSpan HoraLlegada = TimeSpan.Parse(oTempExplo1.HORA_LL);
                        TimeSpan HoraExploracion = TimeSpan.Parse(oTempExplo1.HORA);
                        TimeSpan HoraActual = TimeSpan.Parse(DateTime.Now.Hour.ToString().PadLeft(2, '0') + ":" + DateTime.Now.Minute.ToString().PadLeft(2, '0'));


                        if (TimeSpan.Compare(HoraLlegada, HoraExploracion) < 0)
                        {
                            updateStament += ", HORA_LL=HORA";
                        }

                        if (TimeSpan.Compare(HoraActual, HoraExploracion) < 0)
                        {
                            updateStament += ", HORA_EX=HORA";
                        }
                        else
                        {
                            updateStament += ", hora_ex='" + HoraActual.Hours.ToString().PadLeft(2, '0') + ":" + HoraActual.Minutes.ToString().PadLeft(2, '0') + "'";
                        }

                        updateStament += ", fecha_fac=" + DataBase.QuotedString(oTempExplo1.FECHA.Value.ToString("yyyy/MM/dd"));

                        updateStament += ", pagar='T'";

                        WorkListRepositorio.BorrarFicheroWL(oTempExplo1, rutaWL);
                        WorkListRepositorio.BorrarFicheroWL(oTempExplo, rutaWLAffidea);
                        textoLog = "Exploracion confirmada";
                    }

                    textoLog = "Exploración realizada";
                    break;
                case EXPLORACION.ESTADOS.NOPRESENTADO:
                    textoLog = "No presentada";
                    break;
                case EXPLORACION.ESTADOS.LLAMAANULANDO:
                    textoLog = "Anulada por paciente";
                    break;
                default:
                    break;
            }

            updateStament += " where oid= " + oid;
            oConexion.Open();
            FbCommand oCommand = new FbCommand(updateStament, oConexion);
            oCommand.ExecuteNonQuery();
            oCommand.Dispose();
            oConexion.Close();
            string currentUser = "";

            currentUser = System.Web.HttpContext.Current.User.Identity.Name;

            LOGUSUARIOS oLog = new LOGUSUARIOS
            {
                OWNER = oid,
                FECHA = DateTime.Now.ToString("dd/MM/yyyy HH:mm"),
                TEXTO = textoLog,
                USUARIO = currentUser,
                DATA = DateTime.Now.ToString("dd/MM/yyyy"),
                COD_FIL = oTempExplo.FIL,
                MUTUA = oTempExplo.COD_MUT
            };

            LogUsuariosRepositorio.Insertar(oLog);

            return oTempExplo;
        }

        public static void Trasladar(int oid, string NuevaFecha, string hhora, int aparato)
        {

            FbConnection oConexion = new FbConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ConexionBD"].ConnectionString);

            EXPLORACION oTempExplo = ExploracionRepositorio.Obtener(oid);


            DAPARATOS oAparato = DaparatoRepositorio.Obtener(aparato);
            string updateStament = "update exploracion set fecha=" + DataBase.QuotedString(DateTime.Parse(NuevaFecha).ToString("MM-dd-yyyy"));
            updateStament += ",hora=" + DataBase.QuotedString(hhora) + ",ior_grupo=" + oAparato.OWNER.ToString() + ",OWNER=" + oAparato.CID.ToString();
            updateStament += ",ior_aparato=" + aparato + ", usermod=" + DataBase.QuotedString(HttpContext.Current.User.Identity.Name);
            updateStament += ",HORAMOD=" + DataBase.QuotedString(DateTime.Now.ToString("dd/MM/yyyy HH:mm"));

            if (oTempExplo.CID == 99)
            {
                updateStament += ",CID=-1 ";
            }

            updateStament += " where oid= " + oid;
            oConexion.Open();
            FbCommand oCommand = new FbCommand(updateStament, oConexion);
            oCommand.ExecuteNonQuery();
            oConexion.Close();

            string textoLog = "Exploración trasladada.";
            string currentUser = "";
            if (string.IsNullOrEmpty(System.Web.HttpContext.Current.User.Identity.Name))
            {
                currentUser = "API MANRESA";
            }
            else
            {

                currentUser = System.Web.HttpContext.Current.User.Identity.Name;
            }


            LOGUSUARIOS oLog = new LOGUSUARIOS
            {
                OWNER = oid,
                FECHA = DateTime.Now.ToString("dd/MM/yyyy HH:mm"),
                TEXTO = textoLog,
                USUARIO = currentUser,
                DATA = DateTime.Now.ToString("dd/MM/yyyy"),
                COD_FIL = oTempExplo.APARATO.FIL,
                MUTUA = oTempExplo.ENTIDAD_PAGADORA.CODMUT
            };

            LogUsuariosRepositorio.Insertar(oLog);

        }

        public static void CambiarBloqueoFicha(int oid)
        {
            EXPLORACION oExploracionActualBD = ExploracionRepositorio.Obtener(oid);

            FbConnection oConexion = new FbConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ConexionBD"].ConnectionString);
            oConexion.Open();


            string updateStament = "update exploracion set INTOCABLE='" + (oExploracionActualBD.INTOCABLE == "T" ? "F" : "T");
            updateStament += "' where oid= " + oid;

            FbCommand oCommand = new FbCommand(updateStament, oConexion);
            oCommand.ExecuteNonQuery();
            oConexion.Close();

            string textoLog = "";
            if (oExploracionActualBD.INTOCABLE == "T")
            {
                textoLog = "Exploración Desbloqueada";
            }
            else
            {
                textoLog = "Exploración Bloqueada";
            }

            LOGUSUARIOS oLog = new LOGUSUARIOS { OWNER = oid, FECHA = DateTime.Now.ToString("dd/MM/yyyy HH:mm"), TEXTO = textoLog, USUARIO = HttpContext.Current.User.Identity.Name.ToString(), DATA = DateTime.Now.ToString("dd/MM/yyyy"), COD_FIL = oExploracionActualBD.APARATO.FIL, MUTUA = oExploracionActualBD.ENTIDAD_PAGADORA.CODMUT };

            LogUsuariosRepositorio.Insertar(oLog);

            oConexion.Close();

        }

        public static void CambiarCID(int cid, int oidExploracion)
        {
            EXPLORACION oExploracionActualBD = ExploracionRepositorio.Obtener(oidExploracion);

            FbConnection oConexion = new FbConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ConexionBD"].ConnectionString);
            oConexion.Open();
            string updateStament = "update exploracion set CID=" + cid;


            updateStament += " where oid= " + oidExploracion;

            FbCommand oCommand = new FbCommand(updateStament, oConexion);
            oCommand.ExecuteNonQuery();
            oConexion.Close();



        }

        private static bool SeModificaAlgoExploracion(EXPLORACION oExploEnBd, EXPLORACION oExploEnMem)
        {
            bool result = true;
            if (oExploEnBd.FECHA != oExploEnMem.FECHA)
            {
                return result;
            }

            if (oExploEnBd.HORA != oExploEnMem.HORA)
            {
                return result;
            }
            if (oExploEnBd.GDS != oExploEnMem.GDS)
            {
                return result;
            }

            if (oExploEnBd.IOR_COLEGIADO != oExploEnMem.IOR_COLEGIADO)
            {
                return result;
            }

            if (oExploEnBd.IOR_CENTROEXTERNO != oExploEnMem.IOR_CENTROEXTERNO)
            {
                return result;
            }

            if (oExploEnBd.IOR_TECNICO != oExploEnMem.IOR_TECNICO)
            {
                return result;
            }
            if (oExploEnBd.IOR_ESTUDIANTE != oExploEnMem.IOR_ESTUDIANTE)
            {
                return result;
            }
            if (oExploEnBd.IOR_APARATO != oExploEnMem.IOR_APARATO)
            {
                return result;
            }
            //este campo muestra si una exploración ya ha sido pagada
            if (oExploEnBd.PAGADO != oExploEnMem.PAGADO)
            {
                return result;
            }
            //este campo marca si una exploracion está marcada para pagar
            if (oExploEnBd.PAGAR != oExploEnMem.PAGAR)
            {
                return result;
            }

            if (oExploEnBd.IOR_ENTIDADPAGADORA != oExploEnMem.IOR_ENTIDADPAGADORA)
            {
                return result;
            }
            if (oExploEnBd.CANTIDAD != oExploEnMem.CANTIDAD)
            {
                return result;
            }
            if (oExploEnBd.IOR_TIPOEXPLORACION != oExploEnMem.IOR_TIPOEXPLORACION)
            {
                return result;
            }

            if (oExploEnBd.RECOGIDO != oExploEnMem.RECOGIDO)
            {
                return result;
            }
            if (oExploEnBd.FECHADERIVACION != oExploEnMem.FECHADERIVACION)
            {
                return result;
            }
            if (oExploEnBd.FECHAMAXENTREGA != oExploEnMem.FECHAMAXENTREGA)
            {
                return result;
            }
            if (oExploEnBd.NHCAP != oExploEnMem.NHCAP)
            {
                return result;
            }

            if (oExploEnBd.REGISTRE != oExploEnMem.REGISTRE)
            {
                return result;
            }

            if (oExploEnBd.IOR_CONDICION != oExploEnMem.IOR_CONDICION)
            {
                return result;
            }

            if (oExploEnBd.PESO != oExploEnMem.PESO)
            {
                return result;
            }
            if (oExploEnBd.MOTIVO != oExploEnMem.MOTIVO)
            {
                return result;
            }

            if (oExploEnBd.IOR_MEDICO != oExploEnMem.IOR_MEDICO)
            {
                return result;
            }

            if (oExploEnBd.IOR_CIRUJANO != oExploEnMem.IOR_CIRUJANO)
            {
                return result;
            }

            if (oExploEnBd.TICKET_KIOSKO != oExploEnMem.TICKET_KIOSKO)
            {
                return result;
            }

            result = false;

            return result;

        }

        public static void ActualizarColegiado(int oidExploracion, int oidColegiado)
        {

            FbConnection oConexion = new FbConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ConexionBD"].ConnectionString);
            oConexion.Open();
            string updateStament = "update exploracion set IOR_COLEGIADO=" + oidColegiado;
            updateStament += " where oid= " + oidExploracion;
            FbCommand oCommand = new FbCommand(updateStament, oConexion);
            oCommand.ExecuteNonQuery();
            oConexion.Close();



        }

        public static int Update(EXPLORACION oexploracion)
        {
            EXPLORACION oExploracionActualBD = ExploracionRepositorio.Obtener(oexploracion.OID);

            oexploracion.DAPARATO = DaparatoRepositorio.Obtener(oexploracion.IOR_APARATO.Value);
            oexploracion.ENTIDAD_PAGADORA = MutuasRepositorio.Obtener(oexploracion.IOR_ENTIDADPAGADORA.Value);
            //Siempre que sea una exploracion de Manresa haremos el update para que funcionen los traslados
            bool seHaModificadoAlgo = SeModificaAlgoExploracion(oExploracionActualBD, oexploracion);
            string currentUser = "";
            if (oexploracion.DAPARATO != null && seHaModificadoAlgo) //&& oexploracion.DAPARATO.CID == 8
            {

                FbConnection oConexion = new FbConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ConexionBD"].ConnectionString);
                oConexion.Open();
                string updateStament = "update exploracion set IOR_COLEGIADO=" +
                    oexploracion.IOR_COLEGIADO + ",IOR_TECNICO=" + oexploracion.IOR_TECNICO + ",IOR_ESTUDIANTE=" +
                    oexploracion.IOR_ESTUDIANTE + ",IOR_APARATO=" + oexploracion.IOR_APARATO + ",HORA='" + oexploracion.HORA +
                    "',IOR_GRUPO=" + oexploracion.DAPARATO.OWNER + ",OWNER=" + oexploracion.DAPARATO.CID + ",IOR_ENTIDADPAGADORA=" +
                    oexploracion.IOR_ENTIDADPAGADORA + ",IOR_GPR=" + oexploracion.ENTIDAD_PAGADORA.OWNER;


                if (!string.IsNullOrEmpty(oexploracion.Q_ALFA))
                {
                    updateStament += ",Q_ALFA=" + oexploracion.Q_ALFA.QuotedString();
                }
                else
                {
                    updateStament += ",Q_ALFA=NULL";

                }

                if (!string.IsNullOrEmpty(oexploracion.IDCITAONLINE) && oexploracion.IDCITAONLINE != "VOLVER")
                {
                    updateStament += ",IDCITAONLINE=" + oexploracion.IDCITAONLINE.QuotedString();
                }
                if (!string.IsNullOrEmpty(oexploracion.GDS))
                {
                    updateStament += ",GDS=" + oexploracion.GDS.QuotedString();
                }

                if (!string.IsNullOrEmpty(oexploracion.NHCAP))
                {
                    updateStament += ",NHCAP=" + oexploracion.NHCAP.QuotedString();
                }
                if (!string.IsNullOrEmpty(oexploracion.CANTIDAD.ToString()))
                {
                    updateStament += ",CANTIDAD='" + oexploracion.CANTIDAD.ToString().Replace(",", ".") + "'";
                }

                if (!string.IsNullOrEmpty(oexploracion.PESO.ToString()) && oexploracion.PESO != -1)
                {
                    updateStament += ",PESO='" + oexploracion.PESO.ToString().Replace(",", ".") + "'";
                }



                if (!string.IsNullOrEmpty(oexploracion.PAGADO.ToString()))
                {
                    updateStament += ",PAGADO='" + oexploracion.PAGADO.ToString() + "'";
                }

                if (!string.IsNullOrEmpty(oexploracion.PERMISO.ToString()))
                {
                    updateStament += ",PERMISO='" + oexploracion.PERMISO.ToString() + "'";
                }

                if (!string.IsNullOrEmpty(oexploracion.IOR_CIRUJANO.ToString()) && oexploracion.IOR_CIRUJANO != -1)
                {
                    updateStament += ",IOR_CIRUJANO=" + oexploracion.IOR_CIRUJANO;
                }

                if (!string.IsNullOrEmpty(oexploracion.IOR_TIPOEXPLORACION.ToString()))// && oexploracion.IOR_TIPOEXPLORACION != -1)
                {
                    updateStament += ",IOR_TIPOEXPLORACION=" + oexploracion.IOR_TIPOEXPLORACION;
                }

                if (!string.IsNullOrEmpty(oexploracion.IOR_MEDICO.ToString()) && oexploracion.IOR_MEDICO != -1)
                {
                    updateStament += ",IOR_MEDICO=" + oexploracion.IOR_MEDICO;
                }

                if (!string.IsNullOrEmpty(oexploracion.IOR_CONDICION.ToString()) && oexploracion.IOR_CONDICION != -1)
                {
                    updateStament += ",IOR_CONDICION=" + oexploracion.IOR_CONDICION.ToString();
                }



                if (!string.IsNullOrEmpty(oexploracion.IOR_CENTROEXTERNO.ToString()) && oexploracion.IOR_CENTROEXTERNO != -1)
                {
                    updateStament += ",IOR_CENTROEXTERNO=" + oexploracion.IOR_CENTROEXTERNO;
                }

                if (oexploracion.REGISTRE != null && !string.IsNullOrEmpty(oexploracion.REGISTRE.ToString()))
                {
                    updateStament += ",REGISTRE=" + DataBase.QuotedString(oexploracion.REGISTRE);
                }

                if (oexploracion.RECOGIDO != "F")
                {
                    if (!oExploracionActualBD.FECHADERIVACION.HasValue)
                    {
                        updateStament += ",FECHADERIVACION=" + DataBase.QuotedString(DateTime.Now.ToString("MM/dd/yyyy"));

                    }
                    updateStament += ",RECOGIDO=" + oexploracion.RECOGIDO.QuotedString();
                }
                else
                {
                    if (oExploracionActualBD.FECHADERIVACION.HasValue)
                    {
                        updateStament += ",FECHADERIVACION=null";

                    }
                    updateStament += ",RECOGIDO=" + oexploracion.RECOGIDO.QuotedString();
                }



                if (oexploracion.FECHAMAXENTREGA.HasValue)
                {
                    updateStament += ",FECHAMAXENTREGA=" + DataBase.QuotedString(oexploracion.FECHAMAXENTREGA.Value.ToString("MM/dd/yyyy"));

                }

                if (!string.IsNullOrEmpty(oexploracion.APLAZADO))
                {
                    updateStament += ",APLAZADO=" + DataBase.QuotedString(oexploracion.APLAZADO);
                }

                if (!string.IsNullOrEmpty(oexploracion.MOTIVO))
                {
                    updateStament += ",MOTIVO=" + DataBase.QuotedString(oexploracion.MOTIVO.TrimStart());
                }

                if (!string.IsNullOrEmpty(oexploracion.TICKET_KIOSKO))
                {
                    updateStament += ",TICKET_KIOSKO=" + DataBase.QuotedString(oexploracion.TICKET_KIOSKO.TrimStart());
                }


                if (oexploracion.OWNER == 8 && oexploracion.USERNAME != "ADPMONLINE")
                {
                    currentUser = "API MANRESA";
                }
                else
                {
                    currentUser = HttpContext.Current.User.Identity.Name.ToString();
                }
                updateStament += ",USERMOD=" + DataBase.QuotedString(currentUser);
                updateStament += ",HORAMOD=" + DataBase.QuotedString(DateTime.Now.ToString("dd/MM/yyyy HH:mm"));


                string textoLog = "Modifica Ficha";
                //si se trata de dar entrada a un paciente
                updateStament += " where oid= " + oexploracion.OID;

                FbCommand oCommand = new FbCommand(updateStament, oConexion);

                oCommand.ExecuteNonQuery();
                oConexion.Close();
                oCommand.Dispose();

                //  EXPLORACION oTempExplo = ExploracionRepositorio.Obtener(oexploracion.OID);
                oexploracion.APARATO = AparatoRepositorio.Obtener((int)oexploracion.IOR_APARATO);
                oexploracion.ENTIDAD_PAGADORA = MutuasRepositorio.Obtener((int)oexploracion.IOR_ENTIDADPAGADORA);


                //guardamos en el Log que hemos cambiado la ficha de exploración
                LOGUSUARIOS oLog = new LOGUSUARIOS
                {
                    OWNER = oexploracion.OID,
                    FECHA = DateTime.Now.ToString("dd/MM/yyyy HH:mm"),
                    TEXTO = textoLog,
                    USUARIO = currentUser,
                    DATA = DateTime.Now.ToString("dd/MM/yyyy"),
                    COD_FIL = oexploracion.APARATO.FIL,
                    MUTUA = oexploracion.ENTIDAD_PAGADORA.CODMUT
                };

                LogUsuariosRepositorio.Insertar(oLog);

                if (oExploracionActualBD.IOR_TECNICO != oexploracion.IOR_TECNICO)
                {

                    LOGUSUARIOS oLogTecnico = new LOGUSUARIOS
                    {
                        OWNER = oexploracion.OID,
                        FECHA = DateTime.Now.ToString("dd/MM/yyyy HH:mm"),
                        TEXTO = "Cambia técnico",
                        USUARIO = currentUser,
                        DATA = DateTime.Now.ToString("dd/MM/yyyy"),
                        COD_FIL = oexploracion.APARATO.FIL,
                        MUTUA = oexploracion.ENTIDAD_PAGADORA.CODMUT
                    };

                    LogUsuariosRepositorio.Insertar(oLogTecnico);
                }

                if (oExploracionActualBD.IOR_COLEGIADO != oexploracion.IOR_COLEGIADO && oexploracion.IOR_COLEGIADO != 0)
                {

                    LOGUSUARIOS oLogTecnico = new LOGUSUARIOS
                    {
                        OWNER = oexploracion.OID,
                        FECHA = DateTime.Now.ToString("dd/MM/yyyy HH:mm"),
                        TEXTO = "Cambia colegiado",
                        USUARIO = currentUser,
                        DATA = DateTime.Now.ToString("dd/MM/yyyy"),
                        COD_FIL = oexploracion.APARATO.FIL,
                        MUTUA = oexploracion.ENTIDAD_PAGADORA.CODMUT
                    };

                    LogUsuariosRepositorio.Insertar(oLogTecnico);
                }
            }

            PACIENTE oPaciente = PacienteRepositorio.Obtener((int)oexploracion.IOR_PACIENTE);
            if (oPaciente.CID != oexploracion.IOR_ENTIDADPAGADORA)
            {
                // oPaciente.CID = oexploracion.IOR_ENTIDADPAGADORA;
                PacienteRepositorio.UpdateCampo("CID", oexploracion.IOR_ENTIDADPAGADORA.Value.ToString(), (int)oexploracion.IOR_PACIENTE, "INT");
                //  PacienteRepositorio.Update(oPaciente);
            }

            TEXTOS oTextoExplo = new TEXTOS();
            oTextoExplo.TEXTO = oexploracion.TEXTO;
            oTextoExplo.OWNER = oexploracion.OID;
            TEXTOS oTextoEnBD = TextosRepositorio.Obtener((int)oExploracionActualBD.OID);
            if (oExploracionActualBD.TEXTO != oexploracion.TEXTO)
            {
                LOGUSUARIOS oLogAviso = new LOGUSUARIOS
                {
                    OWNER = oexploracion.OID,
                    FECHA = DateTime.Now.ToString("dd/MM/yyyy HH:mm"),
                    TEXTO = "Modifica Aviso",
                    USUARIO = currentUser,
                    DATA = DateTime.Now.ToString("dd/MM/yyyy"),
                    COD_FIL = oexploracion.APARATO.FIL,
                    MUTUA = oexploracion.ENTIDAD_PAGADORA.CODMUT
                };

                LogUsuariosRepositorio.Insertar(oLogAviso);
                TextosRepositorio.InsertarOrUpdate(oTextoExplo);
            }
            return oexploracion.OID;

        }



        //EL ID DE CITAONLINE SE UTILIZA PARA GUARDAR EL CODIGO INTERNO DE MANRESA
        public static void DeleteDesdeApi(string IDCITAONLINE, string RutaWL, string RutaWLAffidea)
        {
            int idExploracion;
            if (int.TryParse(IDCITAONLINE.Replace("RS", ""), out idExploracion))
            {
                //primero miramos que no sea de manresa mediante el segundo parametro de obtener que busca sobre es campo si es true
                EXPLORACION oExploracion = ExploracionRepositorio.Obtener(idExploracion, true, IDCITAONLINE);
                //sino es de manresa la buscamos normal, por oid

                if (oExploracion.OID == -1)
                {
                    oExploracion = ExploracionRepositorio.Obtener(idExploracion);
                }
                if (oExploracion.ESTADO == "0")
                {
                    ExploracionRepositorio.CambiarEstado(int.Parse(oExploracion.ESTADO), 1, oExploracion.OID, RutaWL, true, RutaWLAffidea);
                }

            }

        }


        public static void InicializarExploracionDeApi(ref EXPLORACION oExploracion)
        {
            WebConfigRepositorio oConfig = new WebConfigRepositorio();
            //DATOS IGUALES PARA CUALQUIER EXPLORACION
            PACIENTE oPaciente = PacienteRepositorio.Obtener((int)oExploracion.IOR_PACIENTE);
            oExploracion.PACIENTE = oPaciente;
            if (oExploracion.CANTIDAD == null)
            {
                oExploracion.CANTIDAD =(float) 0.0;
            }
            else
            {
                oExploracion.CANTIDAD = oExploracion.CANTIDAD;

            }

            oExploracion.CANAL = "1";
            oExploracion.COLEGIADO = new COLEGIADOS();
            oExploracion.DIASEMANA = DateTimeFormatInfo.CurrentInfo.GetDayName(oExploracion.FECHA.Value.DayOfWeek);
            oExploracion.ESTADO = "0";
            oExploracion.FECHA_IDEN = DateTime.Now;
            oExploracion.FECHA_FAC = null;

            oExploracion.HORA_IDEN = DateTime.Now.ToString("HH:mm");
            oExploracion.IOR_EMPRESA = 4;
            oExploracion.BORRADO = "F";
            oExploracion.RECOGIDO = (String.IsNullOrEmpty(oExploracion.RECOGIDO) ? "F" : oExploracion.RECOGIDO);
            oExploracion.PAGADO = (String.IsNullOrEmpty(oExploracion.PAGADO) ? "F" : oExploracion.PAGADO);
            oExploracion.FACTURADA = "F";
            oExploracion.APLAZADO = "F";
            oExploracion.INFORMADA = "F";
            oExploracion.NOMODIFICA = "F";
            oExploracion.INTOCABLE = (String.IsNullOrEmpty(oExploracion.INTOCABLE) ? "F" : oExploracion.INTOCABLE);
            oExploracion.PAGAR = (String.IsNullOrEmpty(oExploracion.PAGAR) ? "T" : oExploracion.PAGAR);
            oExploracion.NOFACTURAB = "F";
            oExploracion.CABINF_EXPLO = "T";
            oExploracion.PERMISO = (String.IsNullOrEmpty(oExploracion.PERMISO) ? "T" : oExploracion.PERMISO);
            oExploracion.VERS = 1;
            oExploracion.IOR_MEDICO = (oExploracion.IOR_MEDICO > 0 ? oExploracion.IOR_MEDICO : -1);
            oExploracion.IOR_TECNICO = (oExploracion.IOR_TECNICO > 0 ? oExploracion.IOR_TECNICO : -1);
            oExploracion.IOR_CIRUJANO = (oExploracion.IOR_CIRUJANO > 0 ? oExploracion.IOR_CIRUJANO : -1);
            oExploracion.IOR_MASTER = (oExploracion.IOR_MASTER == null ? -1 : oExploracion.IOR_MASTER);
            oExploracion.IOR_CODIGORX = -1;
            oExploracion.HORAMOD = DateTime.Now.ToString("dd/MM/yyyy") + " " + DateTime.Now.ToString("HH:mm");
            oExploracion.IOR_CONDICION = -1;
            //string Empresa = oConfig.ObtenerValor("NOMBREEMPRESA");
            oExploracion.MONEDA = MonedaRepositorio.Obtener(int.Parse(oConfig.ObtenerValor("IOR_MONEDA")));
            oExploracion.IOR_MONEDA = oExploracion.MONEDA.OID;


            if (oExploracion.CENTROEXTERNO != null)
            {
                oExploracion.IOR_CENTROEXTERNO = oExploracion.CENTROEXTERNO.OID;
            }

            if (!(oExploracion.IOR_COLEGIADO > 0))
            {
                oExploracion.IOR_COLEGIADO = 0;
            }





            switch (oExploracion.OWNER)
            {
                case 8:
                    int Aparato = -1;
                    int Grupo = -1;
                    oExploracion.IOR_GPR = 1;
                    oExploracion.IOR_ENTIDADPAGADORA = 3820080;
                    oExploracion.USERNAME = "ApiManresa";
                    oExploracion.USERMOD = "ApiManresa";
                    Aparato = (int)Models.API.EXPLORACION.APARATOS.RESONANCIAMANRESA;
                    oExploracion.CID = 11;
                    if (!oExploracion.IDCITAONLINE.StartsWith("MAN_"))
                    {
                        oExploracion.IDCITAONLINE = "MAN_" + oExploracion.IDCITAONLINE;
                    }

                    Grupo = 16;
                    //si hay un guion de separacion
                    if (oExploracion.TEXTO.Split('-').Length > 0)
                    {
                        oExploracion.IOR_TIPOEXPLORACION = AparatoRepositorio.Obtener(oExploracion.TEXTO.Split('-')[1].ToString(), "16").OID;
                    }
                    else
                    {
                        oExploracion.IOR_TIPOEXPLORACION = -1;
                    }
                    oExploracion.IOR_APARATO = Aparato;
                    oExploracion.IOR_GRUPO = Grupo;
                    oExploracion.NHCAP = "null";

                    break;
                //OWNER DE LA EXPLORACION 99 QUIERE DECIR QUE ES CITAONLINE 
                case 99:
                    if (string.IsNullOrEmpty(oExploracion.IDCITAONLINE))
                    {
                        oExploracion.IDCITAONLINE = "null";
                    }

                    oExploracion.APARATO = Models.Repos.AparatoRepositorio.Obtener((int)oExploracion.IOR_TIPOEXPLORACION);
                    oExploracion.IOR_GPR = MutuasRepositorio.Obtener((int)oExploracion.IOR_ENTIDADPAGADORA).OWNER;
                    oExploracion.OWNER = DaparatoRepositorio.Obtener((int)oExploracion.IOR_APARATO).CID;
                    oExploracion.CID = 9;
                    //Aqui puede venir un Centro Externo si ha iniciado sesion con el OID
                    if (oExploracion.USERNAME == null)
                    {
                        oExploracion.USERNAME = "ADPMONLINE";
                        oExploracion.USERMOD = "ADPMONLINE";

                    }
                    if (oExploracion.USERNAME == "CAN MORA")
                    {
                        oExploracion.USERNAME = "MORAONLINE";
                        oExploracion.USERMOD = "MORAONLINE";

                    }
                    break;
                case 98:
                    if (string.IsNullOrEmpty(oExploracion.IDCITAONLINE))
                    {
                        oExploracion.IDCITAONLINE = "null";
                    }

                    oExploracion.APARATO = Models.Repos.AparatoRepositorio.Obtener((int)oExploracion.IOR_TIPOEXPLORACION);
                    oExploracion.IOR_GPR = MutuasRepositorio.Obtener((int)oExploracion.IOR_ENTIDADPAGADORA).OWNER;
                    oExploracion.OWNER = DaparatoRepositorio.Obtener((int)oExploracion.IOR_APARATO).CID;
                    oExploracion.CID = 11;
                    //Aqui puede venir un Centro Externo si ha iniciado sesion con el OID
                    if (oExploracion.USERNAME == null)
                    {
                        oExploracion.USERNAME = "ADPMONLINE";
                        oExploracion.USERMOD = "ADPMONLINE";

                    }
                    if (oExploracion.USERNAME == "CAN MORA")
                    {
                        oExploracion.USERNAME = "MORAONLINE";
                        oExploracion.USERMOD = "MORAONLINE";

                    }
                    break;
                default:

                    oExploracion.USERNAME = HttpContext.Current.User.Identity.Name.ToString();
                    oExploracion.USERMOD = HttpContext.Current.User.Identity.Name.ToString();
                    oExploracion.IOR_GPR = (int)oExploracion.ENTIDAD_PAGADORA.OWNER;
                    oExploracion.OWNER = DaparatoRepositorio.Obtener((int)oExploracion.IOR_APARATO).CID;
                    if (ListaDiaRepositorio.ObtenerPorPaciente(oExploracion.IOR_PACIENTE).Count > 0)
                    {
                        oExploracion.VERS = 0;
                    }
                    else
                    {
                        oExploracion.VERS = 1;
                    }

                    if (oExploracion.IOR_TIPOEXPLORACION == null)
                    {
                        oExploracion.IOR_TIPOEXPLORACION = -1;
                    }
                    oExploracion.NHCAP = (String.IsNullOrEmpty(oExploracion.NHCAP) ? "" : oExploracion.NHCAP);
                    switch (oExploracion.ENTIDAD_PAGADORA.OWNER)
                    {
                        case 1:
                            oExploracion.EXPLORACIONESCONTARIFA = TarifasRepositorio.ObtenerPorAparatoYMutua(oExploracion.IOR_APARATO, oExploracion.ENTIDAD_PAGADORA.OID);
                            break;
                        case 2:
                            //VAMOS A BUSCAR PARA UN APARATO, Y UNA MUTUA QUÉ PRECIOS HAY, CON ESTO FILTRAMOS EL TERCER COMBOBOX
                            if (oExploracion.ENTIDAD_PAGADORA.IOR_CENTRAL != null && oExploracion.ENTIDAD_PAGADORA.IOR_CENTRAL > 0)
                            {
                                //HAY ENTIDADES QUE SON DELEGACIONES DE OTRAS MAYORES, EN ESE CASO LLAMAMOS A LAS TARIFAS CON EL ID DE LA MUTUA PRINCIPAL (IOR_CENTRAL)
                                oExploracion.EXPLORACIONESCONTARIFA = TarifasRepositorio.ObtenerPorAparatoYMutua(oExploracion.IOR_APARATO, oExploracion.ENTIDAD_PAGADORA.IOR_CENTRAL);
                            }
                            else
                            {
                                oExploracion.EXPLORACIONESCONTARIFA = TarifasRepositorio.ObtenerPorAparatoYMutua(oExploracion.IOR_APARATO, oExploracion.ENTIDAD_PAGADORA.OID);
                            }
                            break;
                        default:
                            break;

                    }
                    break;
            }


        }


        public static int Insertar(EXPLORACION oExploracion)
        {
            FbConnection oConexion = new FbConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ConexionBD"].ConnectionString);
            FbCommand oCommand = null;
            bool esCitaDirectaPeticiones = (oExploracion.OWNER == 98);

            if (oExploracion.OWNER == 99 || oExploracion.OWNER == 98)//ADPM CITAONLINE
            {
                oConexion = new FbConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ConexionBD"].ConnectionString);
                
                InicializarExploracionDeApi(ref oExploracion);
                if (esCitaDirectaPeticiones)//CITACION DIRECTA DESDE API
                {
                     USUARIO oUser= UsuariosRepositorio.Obtener(oExploracion.USERNAME);
                    PACIENTE oPaciente = PacienteRepositorio.Obtener(oExploracion.IOR_PACIENTE);
                    oExploracion.IOR_ENTIDADPAGADORA = oPaciente.CID;
                    if (oUser.IOR_COLEGIADO.HasValue && oUser.IOR_COLEGIADO>0)
                    {
                        oExploracion.IOR_COLEGIADO = oUser.IOR_COLEGIADO.Value;
                    }
                    if (oUser.IOR_CENTROEXTERNO.HasValue && oUser.IOR_CENTROEXTERNO > 0)
                    {
                        oExploracion.IOR_CENTROEXTERNO = oUser.IOR_CENTROEXTERNO.Value;
                    }
                    if (oUser.IOR_ENTIDADPAGADORA.HasValue && oUser.IOR_CENTROEXTERNO > 0)
                    {
                        oExploracion.IOR_ENTIDADPAGADORA = oUser.IOR_ENTIDADPAGADORA.Value;
                    }
                }

            }

            if (oExploracion.OWNER == 8)//MANRESA
            {
                try
                {
                    EXPLORACION oExploTemporal = ExploracionRepositorio.Obtener(-1, true, oExploracion.IDCITAONLINE);
                    //SI LA EXPLORACION YA EXISTE EN LA BASE DE DATOS CON ESTE ID CITA ONLINE QUE EQUIVALE AL ID DE LA EXPLORACON 
                    //DE MANRESA NO HACEMOS NADA PORQUE YA EXISTE
                    if (oExploTemporal.OID > -1)
                    {
                        if (oExploTemporal.HORA != oExploracion.HORA || oExploTemporal.FECHA != oExploracion.FECHA)
                        {
                            ExploracionRepositorio.Trasladar(oExploTemporal.OID,
                                oExploracion.FECHA.Value.ToString("dd/MM/yyyy"),
                                oExploracion.HORA, oExploTemporal.DAPARATO.OID);
                        }

                        //Si la exploracion esta marcada como borrada la ponemos como pendiente
                        if (oExploTemporal.ESTADO == "1")
                        {
                            ExploracionRepositorio.CambiarEstado(1, 0, oExploTemporal.OID, "", false, "");
                        }

                        return 0;
                    }
                }
                catch (Exception ex)
                {

                }
                oConexion = new FbConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ConexionBD"].ConnectionString);

                InicializarExploracionDeApi(ref oExploracion);
            }

            string InsertComand;
            try
            {
                string USERNAME = oExploracion.USERNAME;
                if (USERNAME.Length > 14)
                {
                    if (USERNAME.StartsWith("CLINICUM"))
                    {
                        USERNAME = "CLIONLINE";
                    }
                    else
                    {
                        USERNAME = USERNAME.Substring(0, 14);
                    }
                }

                if (!String.IsNullOrEmpty(oExploracion.MOTIVO))
                {
                    oExploracion.MOTIVO = oExploracion.MOTIVO.Replace("'", "''");
                }
                //por peticion de la Roca en el second poit de pets ponemos el permiso igual a false
                //esto hace que la linea no salga en la factura
                EmpresaRepositorio oEmpresa = new EmpresaRepositorio();
                oExploracion.EMPRESA = oEmpresa.Obtener(4);
                if (oExploracion.EMPRESA.NOMBRE.Contains("DELFOS"))
                {
                    if (oExploracion.IOR_TIPOEXPLORACION == 20410128 ||
                        oExploracion.IOR_TIPOEXPLORACION == 20412897
                        || oExploracion.Q_ALFA == "U")
                    {
                        oExploracion.PERMISO = "F";
                    }
                }

                string fechaMaxEntrega = (oExploracion.FECHAMAXENTREGA.HasValue ? oExploracion.FECHAMAXENTREGA.Value.ToString("MM-dd-yyyy") : "");

                InsertComand = "INSERT INTO EXPLORACION ( OID,VERS,OWNER, USERNAME,IOR_EMPRESA, FECHA,FECHAMAXENTREGA, HORA,CANTIDAD, ESTADO, FACTURADA, PAGADO,";
                InsertComand += " APLAZADO, INFORMADA,";
                InsertComand += " FECHA_IDEN, HORA_IDEN,HORAMOD,RECOGIDO,";
                InsertComand += " IOR_GPR, IOR_ENTIDADPAGADORA, IOR_PACIENTE, IOR_APARATO, IOR_GRUPO , IOR_TIPOEXPLORACION,IOR_MONEDA, IOR_COLEGIADO, BORRADO,CANAL,PAGAR, ";
                InsertComand += " NOMODIFICA,Q_ALFA,PERMISO,INTOCABLE,NOFACTURAB,IOR_MEDICO,IOR_TECNICO,IOR_CODIGORX,IDCITAONLINE,CID,NHCAP,IOR_CENTROEXTERNO,MOTIVO,IOR_CIRUJANO,IOR_MASTER,REGISTRE,CABINF_EXPLO,IOR_CONDICION)";
                InsertComand += " VALUES (gen_id(GENUID,1), " + oExploracion.VERS + "," + oExploracion.OWNER + "," + DataBase.QuotedString(USERNAME) + ",";
                InsertComand += oExploracion.IOR_EMPRESA + ",'" + oExploracion.FECHA.Value.ToString("MM-dd-yyyy") + "','" + fechaMaxEntrega + "','" + oExploracion.HORA + "',";
                InsertComand += oExploracion.CANTIDAD.ToString().Replace(",", ".") + ",'" + oExploracion.ESTADO + "','" + oExploracion.FACTURADA + "','" + oExploracion.PAGADO;
                InsertComand += "','" + oExploracion.APLAZADO + "','" + oExploracion.INFORMADA + "','" + oExploracion.FECHA_IDEN.Value.ToString("MM-dd-yyyy") + "','";
                InsertComand += oExploracion.HORA_IDEN + "'," + DataBase.QuotedString(oExploracion.HORAMOD) + "," + DataBase.QuotedString(oExploracion.RECOGIDO);
                InsertComand += "," + oExploracion.IOR_GPR + "," + oExploracion.IOR_ENTIDADPAGADORA + "," + oExploracion.IOR_PACIENTE;
                InsertComand += "," + oExploracion.IOR_APARATO + "," + oExploracion.IOR_GRUPO + "," + oExploracion.IOR_TIPOEXPLORACION + "," + oExploracion.IOR_MONEDA + ",";
                InsertComand += oExploracion.IOR_COLEGIADO + ",'" + oExploracion.BORRADO + "','" + oExploracion.CANAL + "','" + oExploracion.PAGAR + "','";
                InsertComand += oExploracion.NOMODIFICA + "','" + oExploracion.Q_ALFA + "','" + oExploracion.PERMISO + "','" + oExploracion.INTOCABLE + "','" + oExploracion.NOFACTURAB + "','" + oExploracion.IOR_MEDICO;
                InsertComand += "'," + oExploracion.IOR_TECNICO + "," + oExploracion.IOR_CODIGORX + "," + DataBase.QuotedString(oExploracion.IDCITAONLINE) + "," + oExploracion.CID + ",'" + oExploracion.NHCAP + "'," + oExploracion.IOR_CENTROEXTERNO;
                InsertComand += ",'" + oExploracion.MOTIVO + "'," + oExploracion.IOR_CIRUJANO + "," + oExploracion.IOR_MASTER + "," + oExploracion.REGISTRE.QuotedString() + "," + oExploracion.CABINF_EXPLO.QuotedString() + "," + oExploracion.IOR_CONDICION.Value + ")";
                InsertComand += " returning OID";

                InsertComand = InsertComand.Replace(",)", ",null)");
                InsertComand = InsertComand.Replace("''", "null");
                InsertComand = InsertComand.Replace(",,", ",null,");

                oConexion.Open();
                oCommand = new FbCommand(InsertComand, oConexion);
                int result = (int)oCommand.ExecuteScalar();


                TEXTOS oTextoExplo = new TEXTOS();
                oTextoExplo.TEXTO = oExploracion.TEXTO;
                oTextoExplo.OWNER = result;
                TextosRepositorio.InsertarOrUpdate(oTextoExplo);
                string textoLog = "Alta exploración: ";

                string currentUser = "";

                if (oExploracion.OWNER == 8 && oExploracion.USERNAME != "ADPMONLINE")
                {
                    currentUser = "API MANRESA";
                }
                else
                {
                    if (oExploracion.USERNAME == "ADPMONLINE")
                    {
                        currentUser = "ADPMONLINE";

                    }
                    else
                    {
                        currentUser = HttpContext.Current.User.Identity.Name.ToString();
                    }

                }

                if (oExploracion.ENTIDAD_PAGADORA == null)
                {
                    oExploracion.ENTIDAD_PAGADORA = MutuasRepositorio.Obtener((int)oExploracion.IOR_ENTIDADPAGADORA);
                }
                if (oExploracion.DAPARATO == null)
                {
                    oExploracion.DAPARATO = DaparatoRepositorio.Obtener((int)oExploracion.IOR_APARATO);
                }

                LOGUSUARIOS oLog = new LOGUSUARIOS
                {
                    OWNER = result,
                    FECHA = DateTime.Now.ToString("dd/MM/yyyy HH:mm"),
                    TEXTO = textoLog,
                    USUARIO = currentUser,
                    DATA = DateTime.Now.ToString("dd/MM/yyyy"),
                    COD_FIL = oExploracion.DAPARATO.COD_FIL,
                    MUTUA = oExploracion.ENTIDAD_PAGADORA.CODMUT
                };
                LogUsuariosRepositorio.Insertar(oLog);
               // RadioWeb.Utils.LogLopd.Insertar("Alta exploración: " + oExploracion.PACIENTE.PACIENTE1 + " - " + oExploracion.FECHA.Value.ToString("dd/MM/yyyy") + " - " + oExploracion.HORA, "1");

                return result;

            }
            catch (Exception ex)
            {
                LogException.LogMessageToFile(ex.Message);
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


        public static int ContarListaEspera()
        {
            FbConnection oConexion = new FbConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ConexionBD"].ConnectionString);
            oConexion.Open();
            string querySelect = "select count(*) from EXPLORACION e WHERE e.ESTADO='0' and e.FECHA>'TODAY'";
            FbCommand oCommand = null;
            int oResult = -1;
            try
            {
                oCommand = new FbCommand(querySelect, oConexion);
                oResult = int.Parse(oCommand.ExecuteScalar().ToString());
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
                string updateStament = "update exploracion set " + campo + "=";
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
        //AÑADIMOS UN PARAMETRO BOOLEANO PARA REUTILIZAR ESTE METODO PARA MANRESA
        //EL CAMPO CLAVE DE LAS EXPLORACIONES DE MANRESA NO ES EL OID SINO EL ID CITAONLINE
        //CAMPO REAPROVECHADO PARA ALMACENAR EL IDENTIFICADOR INTERNO DEL SOFTWARE DE MANRESA
        public static EXPLORACION Obtener(int oid, bool ESDEMANRESA = false, string idManresa = "")
        {
            FbConnection oConexion = new FbConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ConexionBD"].ConnectionString);
            oConexion.Open();
            string querySelect = "select * from EXPLORACION where";

            if (ESDEMANRESA)
            {
                if (idManresa.Length > 1)
                {
                    if (!idManresa.StartsWith("MAN_"))
                    {
                        idManresa = "MAN_" + idManresa;
                    }
                    querySelect += " IDCITAONLINE like " + DataBase.QuotedString(idManresa);
                }
                else
                {
                    querySelect += " OID=99999999";
                }
            }
            else
            {
                querySelect += " OID=" + oid;
            }
            FbCommand oCommand = new FbCommand(querySelect, oConexion);
            FbDataReader oReader = oCommand.ExecuteReader();

            EXPLORACION oExploracion = new EXPLORACION();
            oExploracion.OID = -1;
            try
            {
                while (oReader.Read())
                {

                    oExploracion.OID = (int)oReader["OID"];
                    oExploracion.ARCHIVOBADALONA = DataBase.GetStringFromReader(oReader, "ARCHIVOBADALONA");
                    oExploracion.ALFA = DataBase.GetDoubleFromReader(oReader, "ALFA");
                    oExploracion.APARATO = AparatoRepositorio.Obtener((int)oReader["IOR_TIPOEXPLORACION"]);
                    oExploracion.DAPARATO = DaparatoRepositorio.Obtener((int)oReader["IOR_APARATO"]);
                    oExploracion.GAPARATO = GAparatoRepositorio.Obtener(oExploracion.DAPARATO.OWNER);
                    oExploracion.APLAZADO = oReader["APLAZADO"].ToString();

                    oExploracion.BORRADO = oReader["BORRADO"].ToString();
                    oExploracion.CANAL = oReader["CANAL"].ToString();
                    if (oReader["CABINF_EXPLO"] == System.DBNull.Value)
                    {
                        oExploracion.CABINF_EXPLO = "T";
                    }
                    else
                    {
                        oExploracion.CABINF_EXPLO = DataBase.GetStringFromReader(oReader, "CABINF_EXPLO");

                    }
                    oExploracion.CANTIDAD = DataBase.GetDoubleFromReader(oReader, "CANTIDAD");
                    oExploracion.CID = DataBase.GetIntFromReader(oReader, "CID");
                    oExploracion.CITAICS = oReader["CITAICS"].ToString();
                    oExploracion.COD_CAP = oReader["COD_CAP"].ToString();
                    oExploracion.COLEGIADO = ColegiadoRepositorio.Obtener(DataBase.GetIntFromReader(oReader, "IOR_COLEGIADO"));
                    //Esto es una propiedad de logica de aplicacion, no de la bas de datos
                    //lo usamos para relacionar el colegiado con la exploracion a la que estamos accediendo
                    oExploracion.COLEGIADO.IOR_EXPLORACION = oExploracion.OID;
                    oExploracion.CONTRASTE = oReader["CONTRASTE"].ToString();
                    oExploracion.DEPOSITO = DataBase.GetDoubleFromReader(oReader, "DEPOSITO");
                    oExploracion.MOTIVO = DataBase.GetStringFromReader(oReader, "MOTIVO");
                    oExploracion.GDS = DataBase.GetStringFromReader(oReader, "GDS");
                    EmpresaRepositorio oEmpresaRepo = new EmpresaRepositorio();
                    oExploracion.EMPRESA = oEmpresaRepo.Obtener((int)oReader["IOR_EMPRESA"]);
                    oExploracion.IOR_EMPRESA = (int)oReader["IOR_EMPRESA"];
                    oExploracion.ENTIDAD_PAGADORA = MutuasRepositorio.Obtener((int)oReader["IOR_ENTIDADPAGADORA"]);
                    oExploracion.ESTADO = oReader["ESTADO"].ToString();
                    oExploracion.REGISTRE = oReader["REGISTRE"].ToString();
                    oExploracion.IOR_CONDICION = DataBase.GetIntFromReader(oReader, "IOR_CONDICION");

                    switch (oExploracion.ESTADO)
                    {
                        case "0":
                            oExploracion.ESTADODESCRIPCION = "PENDIENTE";
                            break;

                        case "1":
                            oExploracion.ESTADODESCRIPCION = "BORRADO";
                            break;

                        case "2":
                            oExploracion.ESTADODESCRIPCION = "PRESENCIA";
                            break;
                        case "3":
                            oExploracion.ESTADODESCRIPCION = "CONFIRMADO";
                            break;

                    }
                    oExploracion.FACTURADA = DataBase.GetStringFromReader(oReader, "FACTURADA");
                    oExploracion.FECHA = DataBase.GetDateTimeFromReader(oReader, "FECHA");

                    var cultureInfo = new CultureInfo("es-ES");
                    var dateTimeInfo = cultureInfo.DateTimeFormat;
                    oExploracion.DIASEMANA = DateTimeFormatInfo.CurrentInfo.GetDayName(oExploracion.FECHA.Value.DayOfWeek);
                    oExploracion.FECHA_FAC = DataBase.GetDateTimeFromReader(oReader, "FECHA_FAC");
                    oExploracion.FECHA_IDEN = DataBase.GetDateTimeFromReader(oReader, "FECHA_IDEN");
                    oExploracion.FECHADERIVACION = DataBase.GetDateTimeFromReader(oReader, "FECHADERIVACION");
                    oExploracion.FECHAENVIO = DataBase.GetDateTimeFromReader(oReader, "FECHAENVIO");
                    oExploracion.FECHAMAXENTREGA = DataBase.GetDateTimeFromReader(oReader, "FECHAMAXENTREGA");
                    oExploracion.HAYCOMEN = oReader["HAYCOMEN"].ToString();
                    oExploracion.HAYCONSUMIBLE = oReader["HAYCONSUMIBLE"].ToString();
                    if (oExploracion.HAYCONSUMIBLE == "T")
                    {
                        List<EXP_CONSUM> oLista = Exp_ConsumRepositorio.GetConsumiblesPendientes(oid);
                        oExploracion.CONSUMIBLES = oLista;
                    }
                    oExploracion.HORA = oReader["HORA"].ToString();
                    oExploracion.HORA_EX = oReader["HORA_EX"].ToString();
                    oExploracion.HORA_IDEN = oReader["HORA_IDEN"].ToString();
                    oExploracion.HORA_LL = oReader["HORA_LL"].ToString();
                    oExploracion.HORAMOD = oReader["HORAMOD"].ToString();
                    oExploracion.IDCITAONLINE = DataBase.GetStringFromReader(oReader, "IDCITAONLINE");
                    oExploracion.IDENTIFICA = oReader["IDENTIFICA"].ToString();
                    oExploracion.INFORMADA = oReader["INFORMADA"].ToString();

                    oExploracion.INTOCABLE = oReader["INTOCABLE"].ToString();
                    oExploracion.IOR_MONEDA = DataBase.GetIntFromReader(oReader, "IOR_MONEDA");
                    oExploracion.IOR_APARATO = DataBase.GetIntFromReader(oReader, "IOR_APARATO");
                    oExploracion.IOR_GPR = oExploracion.ENTIDAD_PAGADORA.CID;
                    oExploracion.IOR_ENTIDADPAGADORA = oExploracion.ENTIDAD_PAGADORA.OID;
                    oExploracion.INFOMUTUA = InfoMutuasRepositorio.Obtener((int)oExploracion.IOR_ENTIDADPAGADORA);
                    oExploracion.VERS = DataBase.GetIntFromReader(oReader, "VERS");
                    oExploracion.IOR_CENTROEXTERNO = DataBase.GetIntFromReader(oReader, "IOR_CENTROEXTERNO");
                    oExploracion.CENTROEXTERNO = CentrosExternosRepositorio.Obtener((int)oExploracion.IOR_CENTROEXTERNO);
                    oExploracion.IOR_COLEGIADO = DataBase.GetIntFromReader(oReader, "IOR_COLEGIADO");

                    oExploracion.IOR_PACIENTE = DataBase.GetIntFromReader(oReader, "IOR_PACIENTE");
                    oExploracion.IOR_FACTURA = DataBase.GetIntFromReader(oReader, "IOR_FACTURA");
                    oExploracion.IOR_TIPOEXPLORACION = DataBase.GetIntFromReader(oReader, "IOR_TIPOEXPLORACION");
                    oExploracion.IOR_TECNICO = DataBase.GetIntFromReader(oReader, "IOR_TECNICO");
                    oExploracion.IOR_ESTUDIANTE = DataBase.GetIntFromReader(oReader, "IOR_ESTUDIANTE");
                    oExploracion.IOR_MASTER = DataBase.GetIntFromReader(oReader, "IOR_MASTER");
                    oExploracion.IOR_CODIGORX = DataBase.GetIntFromReader(oReader, "IOR_CODIGORX");
                    oExploracion.oMEDICO = MedicoRepositorio.Obtener(DataBase.GetIntFromReader(oReader, "IOR_MEDICO"));
                    oExploracion.IOR_MEDICO = DataBase.GetIntFromReader(oReader, "IOR_MEDICO");
                    oExploracion.IOR_CIRUJANO = DataBase.GetIntFromReader(oReader, "IOR_CIRUJANO");
                    oExploracion.MONEDA = MonedaRepositorio.Obtener(DataBase.GetIntFromReader(oReader, "IOR_MONEDA"));
                    oExploracion.NHCAP = DataBase.GetStringFromReader(oReader, "NHCAP");
                    oExploracion.PACIENTE = PacienteRepositorio.Obtener((int)oReader["IOR_PACIENTE"]);
                    oExploracion.PAGADO = DataBase.GetStringFromReader(oReader, "PAGADO");
                    oExploracion.PAGAR = DataBase.GetStringFromReader(oReader, "PAGAR");
                    oExploracion.Q_ALFA = oReader["Q_ALFA"].ToString();
                    oExploracion.FIRMA_CONSEN = DataBase.GetStringFromReader(oReader, "FIRMA_CONSEN");

                    oExploracion.PERMISO = DataBase.GetStringFromReader(oReader, "PERMISO");
                    oExploracion.PESO = DataBase.GetShortFromReader(oReader, "PESO");
                    oExploracion.PRIVADO = DataBase.GetShortFromReader(oReader, "PRIVADO");
                    oExploracion.TIPOPAGO = DataBase.GetStringFromReader(oReader, "TIPOPAGO");
                    oExploracion.RECOGIDO = DataBase.GetStringFromReader(oReader, "RECOGIDO");
                    oExploracion.USERNAME = DataBase.GetStringFromReader(oReader, "USERNAME");
                    oExploracion.USERMOD = DataBase.GetStringFromReader(oReader, "USERMOD");
                    oExploracion.TEXTO = ((TEXTOS)TextosRepositorio.Obtener((int)oReader["OID"])).TEXTO;

                    oExploracion.OWNER = DataBase.GetIntFromReader(oReader, "OWNER");//.ENTIDAD_PAGADORA.OWNER;
                    oExploracion.IMAGENES = ImagenesRepositorio.Obtener(oExploracion.OID, false);
                    oExploracion.QREPORT_ENVIADO = DataBase.GetStringFromReader(oReader, "QREPORT_ENVIADO");
                    oExploracion.TICKET_KIOSKO = DataBase.GetStringFromReader(oReader, "TICKET_KIOSKO");


                    switch (oExploracion.ENTIDAD_PAGADORA.OWNER)
                    {

                        case 1:
                            oExploracion.EXPLORACIONESCONTARIFA = TarifasRepositorio.ObtenerPorAparatoYMutua(oExploracion.IOR_APARATO, oExploracion.ENTIDAD_PAGADORA.OID);
                            break;
                        case 2:
                            //VAMOS A BUSCAR PARA UN APARATO, Y UNA MUTUA QUÉ PRECIOS HAY, CON ESTO FILTRAMOS EL TERCER COMBOBOX
                            if (oExploracion.ENTIDAD_PAGADORA.IOR_CENTRAL != null && oExploracion.ENTIDAD_PAGADORA.IOR_CENTRAL > 0)
                            {
                                //HAY ENTIDADES QUE SON DELEGACIONES DE OTRAS MAYORES, EN ESE CASO LLAMAMOS A LAS TARIFAS CON EL ID DE LA MUTUA PRINCIPAL (IOR_CENTRAL)
                                oExploracion.EXPLORACIONESCONTARIFA = TarifasRepositorio.ObtenerPorAparatoYMutua(oExploracion.IOR_APARATO, oExploracion.ENTIDAD_PAGADORA.IOR_CENTRAL);
                            }
                            else
                            {
                                oExploracion.EXPLORACIONESCONTARIFA = TarifasRepositorio.ObtenerPorAparatoYMutua(oExploracion.IOR_APARATO, oExploracion.ENTIDAD_PAGADORA.OID);
                            }
                            break;
                        case 3:
                            if (oExploracion.APARATO != null)
                            {
                                oExploracion.EXPLORACIONESCONTARIFA = TarifasRepositorio.ObtenerAparatosICS(oExploracion.APARATO.OWNER, oExploracion.ENTIDAD_PAGADORA.OID);
                            }
                            else
                            {
                                DAPARATOS oAparatoTemp = DaparatoRepositorio.Obtener((int)oExploracion.IOR_APARATO);
                                oExploracion.EXPLORACIONESCONTARIFA = TarifasRepositorio.ObtenerAparatosICS(oAparatoTemp.OWNER, oExploracion.ENTIDAD_PAGADORA.OID);
                            }
                            oExploracion.CODIGOSICS = RxRepositorio.ObtenerCodigoICS(oExploracion.IOR_CODIGORX);
                            break;


                        default:
                            break;

                    }

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


            return oExploracion;
        }

        public static List<EXPLORACION> ObtenerHijas(int oid)
        {
            FbConnection oConexion = new FbConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ConexionBD"].ConnectionString);
            oConexion.Open();
            string querySelect = "select oid from EXPLORACION where estado not in (1,4,5) and";
            querySelect += " IOR_MASTER=" + oid;

            FbCommand oCommand = new FbCommand(querySelect, oConexion);
            FbDataReader oReader = oCommand.ExecuteReader();

            List<EXPLORACION> oResult = new List<EXPLORACION>();

            try
            {
                while (oReader.Read())
                {
                    EXPLORACION oExploracion = new EXPLORACION();
                    oExploracion = ExploracionRepositorio.Obtener(DataBase.GetIntFromReader(oReader, "OID"));
                    oResult.Add(oExploracion);
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


        public static List<EXPLORACION> ObtenerPorFactura(int oid)
        {
            FbConnection oConexion = new FbConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ConexionBD"].ConnectionString);
            oConexion.Open();
            string querySelect = "select e.oid, e.facturada,e.fecha_fac,e.nhcap,e.permiso, e.fecha,p.paciente,m.codmut,g.cod_grup,a.fil,a.des_fil,e.cantidad,o.simbolo, e.oid,e.ior_empresa,e.ior_paciente,e.ior_factura from EXPLORACION e join paciente p on p.oid=e.ior_paciente join " +
                " gaparatos g on g.oid= e.ior_grupo join aparatos a on a.oid=e.ior_tipoexploracion join mutuas m on m.oid = e.ior_entidadpagadora " +
                " join monedas o on o.oid=e.ior_moneda where e.ior_factura=" + oid + " order by e.fecha";


            FbCommand oCommand = new FbCommand(querySelect, oConexion);
            FbDataReader oReader = oCommand.ExecuteReader();

            List<EXPLORACION> oResult = new List<EXPLORACION>();

            try
            {
                while (oReader.Read())
                {
                    EXPLORACION oExploracion = new EXPLORACION();
                    oExploracion.PACIENTE = new PACIENTE { PACIENTE1 = DataBase.GetStringFromReader(oReader, "PACIENTE") };
                    oExploracion.CANTIDAD = DataBase.GetDoubleFromReader(oReader, "CANTIDAD");
                    oExploracion.FECHA = DataBase.GetDateTimeFromReader(oReader, "FECHA");
                    oExploracion.FECHA_FAC = DataBase.GetDateTimeFromReader(oReader, "FECHA_FAC");
                    oExploracion.NHCAP = DataBase.GetStringFromReader(oReader, "NHCAP");
                    oExploracion.PERMISO = DataBase.GetStringFromReader(oReader, "PERMISO");
                    oExploracion.TIPOEXPLORACIONDESC = DataBase.GetStringFromReader(oReader, "DES_FIL");
                    oExploracion.OID = DataBase.GetIntFromReader(oReader, "OID");
                    oExploracion.FACTURADA = DataBase.GetStringFromReader(oReader, "FACTURADA");
                    oExploracion.MONEDA = new MONEDAS { SIMBOLO = DataBase.GetStringFromReader(oReader, "SIMBOLO") };
                    oExploracion.IOR_FACTURA = DataBase.GetIntFromReader(oReader, "IOR_FACTURA");

                    //oExploracion = ExploracionRepositorio.Obtener(DataBase.GetIntFromReader(oReader, "OID"));
                    oResult.Add(oExploracion);
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


        public static List<EXPLORACION> ObtenerParaFactura(string fechaInicial, string fechaFinal, int ior_entidadPagadora)
        {
            FbConnection oConexion = new FbConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ConexionBD"].ConnectionString);
            oConexion.Open();
            string querySelect = "select e.estado, e.oid, e.facturada,e.fecha_fac,e.nhcap,e.permiso, e.fecha,p.paciente,m.codmut,g.cod_grup,a.fil,a.des_fil,e.cantidad,o.simbolo, e.oid,e.ior_empresa,e.ior_paciente,e.ior_factura from EXPLORACION e join paciente p on p.oid=e.ior_paciente join " +
                " gaparatos g on g.oid= e.ior_grupo join aparatos a on a.oid=e.ior_tipoexploracion join mutuas m on m.oid = e.ior_entidadpagadora " +
                " join monedas o on o.oid=e.ior_moneda  ";
            querySelect += " where fecha_fac between " + DateTime.Parse(fechaInicial).ToString("MM/dd/yyyy").QuotedString()
       + " and " + DateTime.Parse(fechaFinal).ToString("MM/dd/yyyy").QuotedString() + " and e.ior_entidadpagadora=" + ior_entidadPagadora +" order by e.fecha";

            FbCommand oCommand = new FbCommand(querySelect, oConexion);
            FbDataReader oReader = oCommand.ExecuteReader();

            List<EXPLORACION> oResult = new List<EXPLORACION>();

            try
            {
                while (oReader.Read())
                {
                    EXPLORACION oExploracion = new EXPLORACION();
                    oExploracion.PACIENTE = new PACIENTE { PACIENTE1 = DataBase.GetStringFromReader(oReader, "PACIENTE") };
                    oExploracion.ESTADO = DataBase.GetStringFromReader(oReader, "ESTADO");
                    oExploracion.CANTIDAD = DataBase.GetDoubleFromReader(oReader, "CANTIDAD");
                    oExploracion.FECHA = DataBase.GetDateTimeFromReader(oReader, "FECHA");
                    oExploracion.FECHA_FAC = DataBase.GetDateTimeFromReader(oReader, "FECHA_FAC");
                    oExploracion.NHCAP = DataBase.GetStringFromReader(oReader, "NHCAP");
                    oExploracion.PERMISO = DataBase.GetStringFromReader(oReader, "PERMISO");
                    oExploracion.TIPOEXPLORACIONDESC = DataBase.GetStringFromReader(oReader, "DES_FIL");
                    oExploracion.OID = DataBase.GetIntFromReader(oReader, "OID");
                    oExploracion.FACTURADA = DataBase.GetStringFromReader(oReader, "FACTURADA");
                    oExploracion.MONEDA = new MONEDAS { SIMBOLO = DataBase.GetStringFromReader(oReader, "SIMBOLO") };
                    oExploracion.IOR_FACTURA= DataBase.GetIntFromReader(oReader, "IOR_FACTURA");
                    //oExploracion = ExploracionRepositorio.Obtener(DataBase.GetIntFromReader(oReader, "OID"));
                    oResult.Add(oExploracion);
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

        public static List<EXPLORACION> ObtenerIdCitaonline(string idCitaonline)
        {
            FbConnection oConexion = new FbConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ConexionBD"].ConnectionString);
            oConexion.Open();
            string querySelect = "select oid from EXPLORACION where ";
            querySelect += " IDCITAONLINE=" + idCitaonline.QuotedString();

            FbCommand oCommand = new FbCommand(querySelect, oConexion);
            FbDataReader oReader = oCommand.ExecuteReader();
            List<EXPLORACION> oResult = new List<EXPLORACION>();
            try
            {
                while (oReader.Read())
                {
                    EXPLORACION oExploracion = new EXPLORACION();
                    oExploracion = ExploracionRepositorio.Obtener(DataBase.GetIntFromReader(oReader, "OID"));
                    oResult.Add(oExploracion);
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

        public static List<VMExploNoInformadas> BusquedaAvanzada(VWBusquedaAvanzada filtros)
        {
            FbConnection oConexion = new FbConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ConexionBD"].ConnectionString);
            oConexion.Open();

            string queryInformes = "select e.NHCAP,cen.nombre as centro,  m.NOMBRE, a.DES_FIL, e.oid, e.ior_empresa, e.ior_master,e.ior_GPR, e.ior_paciente, e.intocable, e.fecha, e.hora, e.hora_ll, e.hora_ex, e.informada,";
            queryInformes += "rr.cod as medRev, p.paciente, e.privado, e.numeros, m.codmut,";
            queryInformes += "d.cod_fil,a.fil,c.cod_med,e.cantidad,o.simbolo,e.aplazado,i.cod as MEDINFO,";
            queryInformes += "e.pagado, e.facturada,  e.num_fac, e.estado, d.owner, e.haycomen, e.ior_colegiado, e.nofacturab,";
            queryInformes += "e.ior_aparato, e.ior_tipoexploracion, e.ior_entidadpagadora, m.ior_dap, e.ior_grupo, e.ior_codigorx, x.owner, e.hayconsumible, e.borrado, e.cid, e.fechamaxentrega,e.fecha_fac,e.fecha_iden, substring(t.texto from 1 for 200) as comentario ";
            queryInformes += "from exploracion e left join paciente p on p.oid = e.ior_paciente left join mutuas m on m.oid = e.ior_entidadpagadora left join daparatos d on d.oid = e.ior_aparato ";
            queryInformes += "left join aparatos a on a.oid = e.ior_tipoexploracion left join colegiados c on c.oid = e.ior_colegiado left join centros cen on cen.oid=d.cid ";
            queryInformes += "left join monedas o on o.oid = e.ior_moneda left join rx x on x.oid = e.ior_codigorx left join personal i on i.oid = e.ior_medico ";
            queryInformes += " left join textos t on t.owner = e.oid ";
            queryInformes += "left join personal rr on rr.oid = e.ior_medrevision ";

            queryInformes += " WHERE  (not e.estado in('1','4','5')) and (e.fecha >= " + DataBase.QuotedString(DateTime.Parse(filtros.FECHAINICIAL).ToString("MM-dd-yyyy"));

            queryInformes += " and e.fecha <=" + DataBase.QuotedString(DateTime.Parse(filtros.FECHAFINAL).ToString("MM-dd-yyyy")) + ") and  e.IOR_EMPRESA = 4  ";
            //exploraciones informadas
            if (filtros.INFORMADA == "P")
            {
                string queryLimpiezaRelacional = "update exploracion e set e.IOR_MEDICO=(select first 1 i.IOR_MEDINFORME from informes i where i.OWNER=e.OID ";
                queryLimpiezaRelacional += "and(select first 1 i.OWNER from informes i where i.OWNER = e.OID and i.validacion = 'F') = e.OID)";
                queryLimpiezaRelacional += " where e.INFORMADA = 'F' and e.fecha >" + DataBase.QuotedString(DateTime.Now.AddDays(-15).ToString("MM-dd-yyyy"));

                FbCommand oCommandLimpieza = new FbCommand(queryLimpiezaRelacional, oConexion);
                oCommandLimpieza.ExecuteNonQuery();


                queryInformes += " and E.INFORMADA ='F' AND (SELECT FIRST 1 II.VALIDACION FROM INFORMES II WHERE II.OWNER = E.OID AND II.VALIDACION = 'F') = 'F'";
            }
            else
            {
                if (filtros.INFORMADA != "A" && filtros.INFORMADA != "P")
                {
                    if (filtros.INFORMADA.ToUpper() == "FALSE")
                    {
                        queryInformes += " and (select count(f.oid) from informes f where f.owner = e.oid and f.validacion in ('T','F'))= 0";
                    }
                    else
                    {
                        queryInformes += " and e.informada = " + (filtros.INFORMADA.ToUpper() == "TRUE" ? "'T'" : "'F'");
                    }

                }
                //condiciones de la búsqueda avanzada
                if (filtros.IOR_MEDICOINFORMANTE > 0)
                {
                    queryInformes += " and e.IOR_MEDICO = " + filtros.IOR_MEDICOINFORMANTE;
                }
                if (filtros.IOR_MEDICOREVISION > 0)
                {
                    queryInformes += " and e.IOR_MEDREVISION = " + filtros.IOR_MEDICOREVISION;
                }
                if (filtros.IOR_CENTRO > 0)
                {
                    queryInformes += " and d.CID= " + filtros.IOR_CENTRO;
                }
                if (filtros.IOR_GAPARATO > 0)
                {
                    queryInformes += " and e.ior_grupo = " + filtros.IOR_GAPARATO;
                }
                if (filtros.IOR_DAPARATO > 0)
                {
                    queryInformes += " and e.IOR_APARATO = " + filtros.IOR_DAPARATO;
                }
                //tipo de exploracion
                if (filtros.IOR_APARATO > 0)
                {
                    queryInformes += " and e.ior_tipoexploracion = " + filtros.IOR_APARATO;
                }
                //pri , mut
                if (filtros.IOR_TIPO > 0)
                {
                    queryInformes += " and m.Owner = " + filtros.IOR_TIPO;
                }

                if (filtros.IOR_ENTIDADPAGADORA > 0)
                {
                    queryInformes += " and e.ior_entidadpagadora = " + filtros.IOR_ENTIDADPAGADORA;
                }


            }




            queryInformes += " and ( e.fechamaxentrega > '01/01/2017' or e.FECHAMAXENTREGA is null) order by e.FECHAMAXENTREGA, e.fecha ASC";

            FbCommand oCommand = new FbCommand(queryInformes, oConexion);
            FbDataReader oReader = oCommand.ExecuteReader();

            List<VMExploNoInformadas> oResult = new List<VMExploNoInformadas>();

            try
            {
                while (oReader.Read())
                {

                    VMExploNoInformadas oExploracion =
                        new VMExploNoInformadas
                        {
                            OID = DataBase.GetIntFromReader(oReader, "OID"),
                            COD_FIL = DataBase.GetStringFromReader(oReader, "COD_FIL"),
                            FIL = DataBase.GetStringFromReader(oReader, "DES_FIL"),
                            COD_MED = DataBase.GetStringFromReader(oReader, "COD_MED"),
                            COD_MUT = DataBase.GetStringFromReader(oReader, "CODMUT"),
                            DES_MUT = DataBase.GetStringFromReader(oReader, "NOMBRE"),
                            FECHA = DataBase.GetDateTimeFromReader(oReader, "FECHA"),
                            HAYCONSUMIBLE = DataBase.GetBoolFromReader(oReader, "HAYCONSUMIBLE"),
                            HORA = DataBase.GetStringFromReader(oReader, "HORA"),
                            MEDINFO = DataBase.GetStringFromReader(oReader, "MEDINFO"),
                            MEDREV = DataBase.GetStringFromReader(oReader, "medRev"),
                            PACIENTE = DataBase.GetStringFromReader(oReader, "PACIENTE"),
                            TEXTO = DataBase.GetStringFromReader(oReader, "COMENTARIO"),
                            BORRADO = DataBase.GetBoolFromReader(oReader, "BORRADO"),
                            INFORMADO = DataBase.GetBoolFromReader(oReader, "INFORMADA"),
                            FECHAMAXIMA = DataBase.GetDateTimeFromReader(oReader, "fechamaxentrega"),
                            IOR_PACIENTE = DataBase.GetIntFromReader(oReader, "IOR_PACIENTE"),
                            IOR_MASTER = DataBase.GetIntFromReader(oReader, "IOR_MASTER"),
                            AUTORIZACION = DataBase.GetStringFromReader(oReader, "NHCAP"),
                            CENTRO = DataBase.GetStringFromReader(oReader, "CENTRO")

                        };
                    if (oExploracion.FECHAMAXIMA.HasValue)
                    {
                        oExploracion.DIAS_ENTREGA = (oExploracion.FECHAMAXIMA.Value - DateTime.Now).Days + 1;
                    }

                    oResult.Add(oExploracion);
                }



                foreach (var item in oResult)
                {



                    if (filtros.IOR_ENTIDADPAGADORA == 139)
                    {
                        EXPLORACION oLista = ExploracionRepositorio.Obtener(item.OID);
                        if (!String.IsNullOrEmpty(oLista.IDCITAONLINE))
                        {
                            try
                            {
                                item.IOR_INFORME = InformesRepositorio.ObtenerDeExploracion(item.OID).First().OID;
                                InformesRepositorio.GenerarFicherosFiatc(item.IOR_INFORME);
                            }
                            catch (Exception)
                            {

                            }
                        }
                        else
                        {
                            File.AppendAllText(@"C:\Users\jaced\Documents\RadioWeb2.0\RadioWeb\XML\fileErrorBadalona.txt", oLista.PACIENTE.PACIENTE1 + "-" + oLista.FECHA + "-" + oLista.HORA + Environment.NewLine);

                        }


                    }

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

        public static List<VMExploNoInformadas> ObtenerMedicoInformante(int oidMedicoInformante, bool oidNoPersonalNoInformadas = false)
        {
            FbConnection oConexion = new FbConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ConexionBD"].ConnectionString);
            oConexion.Open();

            string queryInformes = "select  rr.cod as medRev,  m.NOMBRE, a.DES_FIL,e.ior_master, e.oid, e.ior_empresa, e.ior_GPR, e.ior_paciente, e.intocable, e.fecha, e.hora, e.hora_ll, e.hora_ex, e.informada,";
            queryInformes += "i.cod as lkp_medinfo, l.cod as lkp_medrevision,i.nombre as nommedico, p.paciente, e.privado, e.numeros, m.codmut,";
            queryInformes += "d.cod_fil,a.fil,c.cod_med,e.cantidad,o.simbolo,e.aplazado,";
            queryInformes += "e.pagado, e.facturada,  e.num_fac, e.estado, d.owner, e.haycomen, e.ior_colegiado, e.nofacturab, e.ior_medico,";
            queryInformes += "e.ior_aparato, e.ior_tipoexploracion, e.ior_entidadpagadora, m.ior_dap, e.ior_grupo, e.ior_codigorx, x.owner, e.hayconsumible, e.borrado, e.cid, e.fechamaxentrega,e.fecha_fac,e.fecha_iden, substring(t.texto from 1 for 200) as comentario, e.fecha_recogida ";
            queryInformes += "from exploracion e left join paciente p on p.oid = e.ior_paciente left join mutuas m on m.oid = e.ior_entidadpagadora left join daparatos d on d.oid = e.ior_aparato ";
            queryInformes += "left join aparatos a on a.oid = e.ior_tipoexploracion left join colegiados c on c.oid = e.ior_colegiado left join monedas o on o.oid = e.ior_moneda left join rx x on x.oid = e.ior_codigorx left join personal i on i.oid = e.ior_medico ";
            queryInformes += "left join personal l on l.oid = e.cid left join textos t on t.owner = e.oid ";
            queryInformes += "left join personal rr on rr.oid = e.ior_medrevision ";
            queryInformes += " WHERE e.IOR_EMPRESA = 4 and ((e.FECHA<CURRENT_TIMESTAMP AND e.ESTADO='3') OR (e.FECHA>=CURRENT_TIMESTAMP)) AND (e.FECHA>CURRENT_TIMESTAMP-90) and (e.INFORMADA <> 'T' or e.INFORMADA IS NULL) AND e.ESTADO='3' AND (e.IOR_MOTDESPROG<1 or e.IOR_MOTDESPROG is null) ";

            if (oidNoPersonalNoInformadas)
            {
                queryInformes += " and (e.ior_medico < 1 or e.ior_medico is null) and (not e.ior_aparato in (226176,226182,226181)) ";
            }
            else
            {
                if (oidMedicoInformante > 0)
                {
                    queryInformes += " and e.IOR_MEDICO = " + oidMedicoInformante;
                }
                else
                {
                    //si no nos han pasado ningun médico para firmar debemos filtar por aquellas 
                    //explocariones que les han asignado algun medico pero no han sido informadas
                    queryInformes += " and (not (e.ior_aparato in (226176,226182,226181,226180) and (((e.ior_medico<1) or (e.ior_medico is null)) or (e.ior_medico=252516)))) ";
                }
            }

            queryInformes += " order by e.FECHAMAXENTREGA, e.fecha ASC";

            FbCommand oCommand = new FbCommand(queryInformes, oConexion);
            FbDataReader oReader = oCommand.ExecuteReader();

            List<VMExploNoInformadas> oResult = new List<VMExploNoInformadas>();

            try
            {
                while (oReader.Read())
                {
                    VMExploNoInformadas oExploracion =
                        new VMExploNoInformadas
                        {
                            OID = DataBase.GetIntFromReader(oReader, "OID"),
                            COD_FIL = DataBase.GetStringFromReader(oReader, "COD_FIL"),
                            FIL = DataBase.GetStringFromReader(oReader, "DES_FIL"),
                            COD_MED = DataBase.GetStringFromReader(oReader, "COD_MED"),
                            NOM_MED = DataBase.GetStringFromReader(oReader, "nommedico"),
                            COD_MUT = DataBase.GetStringFromReader(oReader, "CODMUT"),
                            DES_MUT = DataBase.GetStringFromReader(oReader, "NOMBRE"),
                            FECHA = DataBase.GetDateTimeFromReader(oReader, "FECHA"),
                            HAYCONSUMIBLE = DataBase.GetBoolFromReader(oReader, "HAYCONSUMIBLE"),
                            HORA = DataBase.GetStringFromReader(oReader, "HORA"),
                            MEDINFO = DataBase.GetStringFromReader(oReader, "LKP_MEDINFO"),
                            MEDREV = DataBase.GetStringFromReader(oReader, "medRev"),
                            IOR_MASTER = DataBase.GetIntFromReader(oReader, "IOR_MASTER"),
                            PACIENTE = DataBase.GetStringFromReader(oReader, "PACIENTE"),
                            TEXTO = DataBase.GetStringFromReader(oReader, "COMENTARIO"),
                            BORRADO = DataBase.GetBoolFromReader(oReader, "BORRADO"),
                            INFORMADO = DataBase.GetBoolFromReader(oReader, "INFORMADA"),
                            FECHAMAXIMA = DataBase.GetDateTimeFromReader(oReader, "fechamaxentrega"),
                            IOR_PACIENTE = DataBase.GetIntFromReader(oReader, "IOR_PACIENTE"),
                            IOR_MEDICO = DataBase.GetIntFromReader(oReader, "IOR_MEDICO"),
                            FECHAMAXIMAADMIN = DataBase.GetDateTimeFromReader(oReader, "fechamaxentrega"),
                            FECHA_RECOGIDA = DataBase.GetDateTimeFromReader(oReader, "fecha_recogida"),
                        };
                    if (oExploracion.FECHAMAXIMA.HasValue)
                    {
                        oExploracion.DIAS_ENTREGA = (oExploracion.FECHAMAXIMA.Value - DateTime.Now).Days + 1;

                        //var fechaMaximaAdmin = oExploracion.FECHAMAXIMA.Value.ToString("dd/MM/yyyy");
                        //oExploracion.FECHAMAXIMAADMIN = fechaMaximaAdmin;
                    }


                    oResult.Add(oExploracion);


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

        public static List<VMExploNoInformadas> ObtenerMedicoInformante(int oidMedicoInformante)
        {
            FbConnection oConexion = new FbConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ConexionBD"].ConnectionString);
            oConexion.Open();

            string queryInformes = "select  rr.cod as medRev,  m.NOMBRE, a.DES_FIL,e.ior_master, e.oid, e.ior_empresa, e.ior_GPR, e.ior_paciente, e.intocable, e.fecha, e.hora, e.hora_ll, e.hora_ex, e.informada,";
            queryInformes += "i.cod as lkp_medinfo, l.cod as lkp_medrevision,i.nombre as nommedico, p.paciente, e.privado, e.numeros, m.codmut,";
            queryInformes += "d.cod_fil,a.fil,c.cod_med,e.cantidad,o.simbolo,e.aplazado,";
            queryInformes += "e.pagado, e.facturada,  e.num_fac, e.estado, d.owner, e.haycomen, e.ior_colegiado, e.nofacturab,";
            queryInformes += "e.ior_aparato, e.ior_tipoexploracion, e.ior_entidadpagadora, m.ior_dap, e.ior_grupo, e.ior_codigorx, x.owner, e.hayconsumible, e.borrado, e.cid, e.fechamaxentrega,e.fecha_fac,e.fecha_iden, substring(t.texto from 1 for 200) as comentario ";
            queryInformes += "from exploracion e left join paciente p on p.oid = e.ior_paciente left join mutuas m on m.oid = e.ior_entidadpagadora left join daparatos d on d.oid = e.ior_aparato ";
            queryInformes += "left join aparatos a on a.oid = e.ior_tipoexploracion left join colegiados c on c.oid = e.ior_colegiado left join monedas o on o.oid = e.ior_moneda left join rx x on x.oid = e.ior_codigorx left join personal i on i.oid = e.ior_medico ";
            queryInformes += "left join personal l on l.oid = e.cid left join textos t on t.owner = e.oid ";
            queryInformes += "left join personal rr on rr.oid = e.ior_medrevision ";
            queryInformes += " WHERE e.IOR_EMPRESA = 4 and (not e.estado in('1','4','5')) and e.INFORMADA <> 'T'  ";

            if (oidMedicoInformante > 0)
            {
                queryInformes += " and e.IOR_MEDICO = " + oidMedicoInformante;
            }
            else
            {
                //si no nos han pasado ningun médico para firmar debemos filtar por aquellas 
                //explocariones que les han asignado algun medico pero no han sido informadas
                queryInformes += " and e.ior_medico>0   and (e.fecha > '01/10/2017')";
            }

            queryInformes += " and ( e.fechamaxentrega > '01/01/2017' or e.FECHAMAXENTREGA is null) order by e.FECHAMAXENTREGA, e.fecha ASC";

            FbCommand oCommand = new FbCommand(queryInformes, oConexion);
            FbDataReader oReader = oCommand.ExecuteReader();

            List<VMExploNoInformadas> oResult = new List<VMExploNoInformadas>();

            try
            {
                while (oReader.Read())
                {

                    VMExploNoInformadas oExploracion =
                        new VMExploNoInformadas
                        {
                            OID = DataBase.GetIntFromReader(oReader, "OID"),
                            COD_FIL = DataBase.GetStringFromReader(oReader, "COD_FIL"),
                            FIL = DataBase.GetStringFromReader(oReader, "DES_FIL"),
                            COD_MED = DataBase.GetStringFromReader(oReader, "COD_MED"),
                            NOM_MED = DataBase.GetStringFromReader(oReader, "nommedico"),
                            COD_MUT = DataBase.GetStringFromReader(oReader, "CODMUT"),
                            DES_MUT = DataBase.GetStringFromReader(oReader, "NOMBRE"),
                            FECHA = DataBase.GetDateTimeFromReader(oReader, "FECHA"),
                            HAYCONSUMIBLE = DataBase.GetBoolFromReader(oReader, "HAYCONSUMIBLE"),
                            HORA = DataBase.GetStringFromReader(oReader, "HORA"),
                            MEDINFO = DataBase.GetStringFromReader(oReader, "LKP_MEDINFO"),
                            MEDREV = DataBase.GetStringFromReader(oReader, "medRev"),
                            IOR_MASTER = DataBase.GetIntFromReader(oReader, "IOR_MASTER"),
                            PACIENTE = DataBase.GetStringFromReader(oReader, "PACIENTE"),
                            TEXTO = DataBase.GetStringFromReader(oReader, "COMENTARIO"),
                            BORRADO = DataBase.GetBoolFromReader(oReader, "BORRADO"),
                            INFORMADO = DataBase.GetBoolFromReader(oReader, "INFORMADA"),
                            FECHAMAXIMA = DataBase.GetDateTimeFromReader(oReader, "fechamaxentrega"),
                            IOR_PACIENTE = DataBase.GetIntFromReader(oReader, "IOR_PACIENTE")
                        };
                    if (oExploracion.FECHAMAXIMA.HasValue)
                    {
                        oExploracion.DIAS_ENTREGA = (oExploracion.FECHAMAXIMA.Value - DateTime.Now).Days + 1;
                    }

                    oResult.Add(oExploracion);


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


        public static List<VWImprimirFicha> ImprimirFichaPri(int oidExploracion, bool soloExplosDia = true)
        {

            List<VWImprimirFicha> oResult = new List<VWImprimirFicha>();
            FbConnection oConexion = new FbConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ConexionBD"].ConnectionString);
            oConexion.Open();

            try
            {
                string query = "SELECT Historias.oid oidhistoria, con.DESCRIPCION CONDICION,  e.IOR_MASTER, e.CID, e.IOR_CENTROEXTERNO, textopaciente.texto TEXTOPACIENTE, e.ior_paciente,e.oid,P.TRAC, P.PACIENTE,P.EMAIL,P.POLIZA, P.COD_PAC";
                query += ", P.FECHAN,p.QRCOMPARTIRCASO, P.SEXO, P.PROFESION, P.DNI, d.cod_fil as Aparato, a.FIL as Explo,a.DES_FIL,e.hora_ll, e.fecha, e.hora,e.fechamaxentrega,t.cod_grup,text.TEXTO";
                query += ",m.codmut,m.nombre,m.owner,co.trata,co.nombre NOMBRECOLEGIADO,co.espec, co.cod_med,es.descripcion as especialidad,e.oid,cen.NOMBRE NOMBRECENTRO,CEN.DIRECCION DIRECCIONCENTRO,CEN.TELEFONO TELEFONOCENTRO,CEN.HORARIO2 HORARIORECOGIDA2,CEN.HORARIO HORARIORECOGIDA,CEN.CIUTAT CIUTATCENTRO,CEN.CP CPCENTRO";
                query += ",IIF(EXTRACT(MONTH FROM CURRENT_TIMESTAMP)<EXTRACT(MONTH FROM FECHAN),EXTRACT(year from CURRENT_TIMESTAMP) - EXTRACT(year from fechan) - 1,EXTRACT(year from CURRENT_TIMESTAMP) - EXTRACT(year from fechan)) as edad ,";
                query += "D.OWNER OWNERAPARATO";
                query += ",dir.PROVINCIA, dir.DIRECCION,dir.CP, dir.POBLACION, TEL.NUMERO,telMovil.NUMERO NUMEROMOVIL,centroex.oid oidcentroex,centroex.nombre nombrecentroex";
                query += " from exploracion e left join paciente p on p.oid=e.ior_paciente left join mutuas m on m.oid=e.ior_entidadpagadora ";
                query += " left join daparatos d on d.oid=e.ior_aparato";
                query += " left join gaparatos t on t.oid=e.ior_grupo ";
                query += " LEFT join TEXTOS textopaciente on textopaciente.OWNER= e.ior_paciente ";
                query += " LEFT join TEXTOS text on text.OWNER= e.oid ";
                query += " LEFT join Historias on Historias.ior_paciente= e.ior_paciente ";
                query += " left join aparatos a on a.oid=e.ior_tipoexploracion left join colegiados co on co.oid=e.ior_colegiado ";
                query += " left join especialidades es on es.oid=co.ior_especialidad";
                query += " left join CENTROSEXTERNOS centroex on centroex.oid=e.ior_centroexterno";
                query += " left join centros cen on cen.oid = d.CID";
                query += " left join condicion con on con.oid = e.ior_condicion";
                query += " left join DIRECCION dir on dir.OID=(select first 1 dd.OID from direccion dd where dd.OWNER=p.OID)";
                query += " left join TELEFONO telMovil on telMovil.OID = (select first 1 tt.OID from telefono tt where tt.NUMERO like '6%' and tt.OWNER = p.OID)";
                query += " left join TELEFONO tel on tel.OID = (select first 1 tt.OID from telefono tt where tt.OWNER = p.OID)";
                query += " where  e.OID = " + oidExploracion;
                VWImprimirFicha oTemp = new VWImprimirFicha();
                FbCommand dc = new FbCommand(query, oConexion);
                FbDataReader oReader = dc.ExecuteReader();



                while (oReader.Read())
                {
                    oTemp.OID = DataBase.GetIntFromReader(oReader, "OID");
                    oTemp.CID = DataBase.GetIntFromReader(oReader, "CID");
                    oTemp.COD_PAC = DataBase.GetStringFromReader(oReader, "COD_PAC");
                    oTemp.DNI = DataBase.GetStringFromReader(oReader, "DNI");
                    oTemp.PACIENTE = DataBase.GetStringFromReader(oReader, "PACIENTE");
                    oTemp.EMAIL = DataBase.GetStringFromReader(oReader, "EMAIL");
                    oTemp.PROFESION = DataBase.GetStringFromReader(oReader, "PROFESION");
                    oTemp.QRCOMPARTIRCASO = DataBase.GetStringFromReader(oReader, "QRCOMPARTIRCASO");
                    oTemp.FECHA = DataBase.GetDateTimeFromReader(oReader, "FECHA").Value;
                    oTemp.HORARIOREGOGIDA = DataBase.GetStringFromReader(oReader, "HORARIORECOGIDA");
                    oTemp.HORARIOREGOGIDA2 = DataBase.GetStringFromReader(oReader, "HORARIORECOGIDA2");
                    oTemp.CONDICION = DataBase.GetStringFromReader(oReader, "CONDICION");

                    oTemp.FECHAMAXIMA = (DataBase.GetDateTimeFromReader(oReader, "fechamaxentrega").HasValue
                        ? DataBase.GetDateTimeFromReader(oReader, "fechamaxentrega").Value.ToShortDateString()
                        : "");

                    if (DataBase.GetDateTimeFromReader(oReader, "FECHAN").HasValue)
                    {
                        oTemp.FECHAN = DataBase.GetDateTimeFromReader(oReader, "FECHAN").Value.ToString("dd-MM-yyyy");
                        oTemp.EDAD = (long)oReader["EDAD"];
                    }
                    else
                    {
                        oTemp.FECHAN = DateTime.MinValue.ToString("dd-MM-yyyy");
                        oTemp.EDAD = 0;
                    }

                    oTemp.HORA = DataBase.GetStringFromReader(oReader, "HORA");
                    oTemp.HORA_LL = DataBase.GetStringFromReader(oReader, "HORA_LL");
                    oTemp.IOR_PACIENTE = DataBase.GetIntFromReader(oReader, "IOR_PACIENTE");
                    oTemp.TRAC = DataBase.GetShortFromReader(oReader, "TRAC");
                    oTemp.APARATO = DataBase.GetStringFromReader(oReader, "APARATO");
                    oTemp.COD_GRUP = DataBase.GetStringFromReader(oReader, "COD_GRUP");
                    oTemp.EXPLO = DataBase.GetStringFromReader(oReader, "EXPLO");
                    oTemp.DES_FIL = DataBase.GetStringFromReader(oReader, "DES_FIL");
                    oTemp.DIRECCION = DataBase.GetStringFromReader(oReader, "DIRECCION");
                    oTemp.CP = DataBase.GetStringFromReader(oReader, "CP");
                    oTemp.POBLACION = DataBase.GetStringFromReader(oReader, "POBLACION");
                    oTemp.PROVINCIA = DataBase.GetStringFromReader(oReader, "PROVINCIA");
                    oTemp.NUMERO = DataBase.GetStringFromReader(oReader, "NUMERO");
                    oTemp.NUMEROMOVIL = DataBase.GetStringFromReader(oReader, "NUMEROMOVIL");
                    oTemp.NOMBRECOLEGIADO = DataBase.GetStringFromReader(oReader, "NOMBRECOLEGIADO");
                    oTemp.ESPECIALIDAD = DataBase.GetStringFromReader(oReader, "ESPECIALIDAD");
                    oTemp.ESPEC = DataBase.GetStringFromReader(oReader, "ESPEC");
                    oTemp.TRATA = DataBase.GetStringFromReader(oReader, "TRATA");
                    oTemp.COD_MED = DataBase.GetStringFromReader(oReader, "COD_MED");
                    oTemp.OWNER = DataBase.GetIntFromReader(oReader, "OWNER");
                    oTemp.CODMUT = DataBase.GetStringFromReader(oReader, "CODMUT");
                    oTemp.NOMBRE = DataBase.GetStringFromReader(oReader, "NOMBRE");
                    oTemp.POLIZA = DataBase.GetStringFromReader(oReader, "POLIZA");
                    oTemp.NOMBRECENTRO = DataBase.GetStringFromReader(oReader, "NOMBRECENTRO");
                    oTemp.DIRECCIONCENTRO = DataBase.GetStringFromReader(oReader, "DIRECCIONCENTRO");
                    oTemp.CPCENTRO = DataBase.GetStringFromReader(oReader, "CPCENTRO");
                    oTemp.CIUTATCENTRO = DataBase.GetStringFromReader(oReader, "CIUTATCENTRO");
                    oTemp.TELEFONOCENTRO = DataBase.GetStringFromReader(oReader, "TELEFONOCENTRO");
                    oTemp.CENTROEXTERNOOID = DataBase.GetIntFromReader(oReader, "oidcentroex");
                    oTemp.CENTROEXTERNO = DataBase.GetStringFromReader(oReader, "nombrecentroex");
                    oTemp.IOR_MASTER = DataBase.GetIntFromReader(oReader, "IOR_MASTER");
                    try
                    {
                        oTemp.HISTORIACLINICA = TextosRepositorio.Obtener(DataBase.GetIntFromReader(oReader, "oidhistoria")).TEXTO;

                        if (!String.IsNullOrEmpty( oTemp.HISTORIACLINICA) &&  oTemp.HISTORIACLINICA.StartsWith("{\\rtf1"))
                        {
                            oTemp.HISTORIACLINICA = DataBase.convertRtfToPlainText(TextosRepositorio.Obtener(DataBase.GetIntFromReader(oReader, "oidhistoria")).TEXTO);
                            oTemp.HISTORIACLINICA = oTemp.HISTORIACLINICA.Replace("<p style=\"margin:0pt 0pt 0pt 0pt;line-height:normal;\"><span class=\"st1\">&nbsp;</span></p>", "");
                            oTemp.HISTORIACLINICA = oTemp.HISTORIACLINICA.Replace("The trial version of RTF-to-HTML DLL .Net can convert up to 10000 symbols.", "");
                            oTemp.HISTORIACLINICA = oTemp.HISTORIACLINICA.Replace("Get the full featured version!", "");
                        }
                    }
                    catch (Exception)
                    {


                    }
                    //sI UN TEXTO EMPIEZA POR INTRO NO SE IMPRIME
                    if (oReader["TEXTOPACIENTE"].ToString().Contains("\r"))
                    {
                        if (oReader["TEXTOPACIENTE"].ToString().StartsWith("\r"))
                        {
                            oTemp.TEXTOPACIENTE = "";
                        }
                        else
                        {
                            oTemp.TEXTOPACIENTE = oReader["TEXTOPACIENTE"].ToString().Substring(0, (oReader["TEXTOPACIENTE"].ToString().IndexOf("\r")));
                        }
                    }
                    else
                    {
                        if (oReader["TEXTOPACIENTE"].ToString().Length > 70)
                        {
                            oTemp.TEXTOPACIENTE = oReader["TEXTOPACIENTE"].ToString().Substring(0, 70);
                        }
                    }
                    if (oReader["TEXTO"].ToString().Contains("\r"))
                    {
                        if (oReader["TEXTO"].ToString().StartsWith("\r"))
                        {
                            oTemp.TEXTO = "";
                        }
                        else
                        {
                            oTemp.TEXTO = oReader["TEXTO"].ToString().Substring(0, (oReader["TEXTO"].ToString().IndexOf("\r")));
                        }
                    }
                    else
                    {
                        if (oReader["TEXTO"].ToString().Length > 50)
                        {
                            oTemp.TEXTO = oReader["TEXTO"].ToString().Substring(0, 50);
                        }
                        else
                        {
                            oTemp.TEXTO = oReader["TEXTO"].ToString();

                        }
                    }
                }


                oReader.Close();
                dc.Dispose();



                query = "SELECT e.IOR_MASTER, E.FECHA, E.HORA, M.COD_MUT, D.COD_FIL,E.OID,A.FIL, A.DES_FIL";
                query += " FROM EXPLORACION E JOIN APARATOS A ON A.OID=E.IOR_TIPOEXPLORACION JOIN DAPARATOS D ON D.OID=E.IOR_APARATO JOIN MUTUAS M ON M.OID=E.IOR_ENTIDADPAGADORA ";
                query += " WHERE  e.estado not in (1,4,5) and E.IOR_PACIENTE=" + oTemp.IOR_PACIENTE;

                if (soloExplosDia)
                {
                    query += " AND E.FECHA = '" + oTemp.FECHA.ToString("MM-dd-yyyy") + "'";
                }

                query += " AND E.OID<>" + oTemp.OID.ToString() + " order by hora DESC";

                FbCommand oCommandExplosAsociadas = new FbCommand(query, oConexion);

                FbDataReader oExplosAsociadas = oCommandExplosAsociadas.ExecuteReader();


                while (oExplosAsociadas.Read())
                {
                    try
                    {
                        OTRASEXPLORACIONES oTempAsociada = new OTRASEXPLORACIONES
                        {
                            HORA = DataBase.GetStringFromReader(oExplosAsociadas, "HORA"),
                            COD_FIL = DataBase.GetStringFromReader(oExplosAsociadas, "COD_FIL"),
                            FECHA = DataBase.GetDateTimeFromReader(oExplosAsociadas, "FECHA").Value,
                            COD_MUT = DataBase.GetStringFromReader(oExplosAsociadas, "COD_MUT"),
                            DES_FIL = DataBase.GetStringFromReader(oExplosAsociadas, "DES_FIL"),
                            FIL = DataBase.GetStringFromReader(oExplosAsociadas, "FIL"),
                            OID = DataBase.GetIntFromReader(oExplosAsociadas, "OID"),
                            IOR_MASTER = DataBase.GetIntFromReader(oExplosAsociadas, "IOR_MASTER")
                        };

                        oTemp.EXPLOSASOCIADAS.Add(oTempAsociada);
                    }
                    catch (Exception ex)
                    {


                    }


                }

                oCommandExplosAsociadas.Dispose();

                
                oResult.Add(oTemp);


                return oResult;
            }
            catch (Exception EX)
            {

                throw;
            }
            finally
            {

                oConexion.Close();

            }


        }



        public static Boolean checkExistingAppointments(EXPLORACION oExploracion)
        {
            FbConnection oConexion = new FbConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ConexionBD"].ConnectionString);
            oConexion.Open();

            String hora = oExploracion.HORA;
            DateTime? fecha = oExploracion.FECHA;
            int pac_id = oExploracion.IOR_PACIENTE;



            String sql = "SELECT * FROM EXPLORACION WHERE IOR_PACIENTE = "+pac_id+" AND FECHA = '"+fecha?.ToString("yyyy-MM-dd HH:mm:ss") +"' AND HORA = '"+hora+"'";

            FbCommand queryRes = new FbCommand(sql, oConexion);

            FbDataReader rs = queryRes.ExecuteReader();
            //Si hay lineas, devolvemos un true conforme que hemos encontrado líneas, y prevenimos al usuario de pedir más citas para esa hora en concreto.
            if(rs.Read())
            {
                rs.Close();
                queryRes.Dispose();
                return true;
            }
            else
            {
                rs.Close();
                queryRes.Dispose();
                return false;
            }

        }

        public static Boolean updateQreportEnviado(Int32 OID, String estado)
        {
            FbConnection oConexion = new FbConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ConexionBD"].ConnectionString);
            FbCommand oCommand = null;
            try
            {
                oConexion.Open();
                string updateStament = "update EXPLORACION set QREPORT_ENVIADO=@estado where OID=@OID ";
                oCommand = new FbCommand(updateStament, oConexion);
                oCommand.Parameters.Add("@estado", estado);
                oCommand.Parameters.Add("@OID", OID);
                oCommand.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                return false;
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

            return true;
        }
    }
}