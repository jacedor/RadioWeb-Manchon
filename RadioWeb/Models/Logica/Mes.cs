using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using FirebirdSql.Data.FirebirdClient;
using RadioWeb.Utils;
using RadioWeb.Models.Repos;
using ADPM.Common;

namespace RadioWeb.Models.Logica
{


    public class Mes
    {

        FbConnection oConexion = new FbConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ConexionBD"].ConnectionString);

        public int ID { get; set; }
        public string Descripcion { get; set; }
        public string FechaInicial { get; set; }
        public string FechaFinal { get; set; }
        public string Año { get; set; }
        public int DiasVaciosPintarDelante { get; set; }
        public List<Dia> Dias { get; set; }



        public Mes(DateTime fecha, FiltrosBusquedaExploracion oFiltros, USUARIO UserLogeado)
        {

            Descripcion = fecha.ToString("y").ToUpper();
            Dias = new List<Dia>();

            FechaInicial = fecha.Year.ToString() + "/" + fecha.Month.ToString().PadLeft(2, '0') + "/01";
            FechaFinal = fecha.Year.ToString() + "/" + fecha.Month.ToString().PadLeft(2, '0') + "/" + DateTime.DaysInMonth(fecha.Year, fecha.Month).ToString();

            DiasVaciosPintarDelante = ((int)new DateTime(fecha.Year, fecha.Month, 1).DayOfWeek) - 1;

            if (DiasVaciosPintarDelante == -1)
                DiasVaciosPintarDelante = 6;

            string queryCuentaTotales = "SELECT A.FECHA, COUNT(OID) TOTAL FROM EXPLORACION a where (A.FECHA between '" + FechaInicial + "' AND '" + FechaFinal + "') ";

            Dictionary<DateTime, string> oDiccionarioTextosAparato = new Dictionary<DateTime, string>();
            //Si hay filtro de Aparato
            if (oFiltros.oidAparato > 0)
            {
                try
                {
                    queryCuentaTotales = queryCuentaTotales + " and a.IOR_APARATO =" + oFiltros.oidAparato;

                    FbDataReader oReaderTextos = RadioWeb.Utils.DataBase.EjecutarQuery(oConexion, "select first 1 AGENDA,TEXTO from agendagen where ior_empresa=4 and agenda between '" + FechaInicial + "' AND '" + FechaFinal + "' and ior_daparato=" + oFiltros.oidAparato);

                    while (oReaderTextos.Read())
                    {
                        oDiccionarioTextosAparato.Add((DateTime)oReaderTextos["AGENDA"], (string)oReaderTextos["TEXTO"]);
                    }
                    oReaderTextos.Close();

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
            else
            {

                //textos generales, que no son concretos de un aparato
                FbDataReader oReaderTextos = RadioWeb.Utils.DataBase.EjecutarQuery(oConexion, "select first 1 AGENDA,TEXTO from agendagen where ior_empresa=4 and agenda between '" + FechaInicial + "' AND '" + FechaFinal + "' and ior_daparato=0");

                try
                {
                    while (oReaderTextos.Read())
                    {
                        oDiccionarioTextosAparato.Add((DateTime)oReaderTextos["AGENDA"], (string)oReaderTextos["TEXTO"]);
                    }
                    oReaderTextos.Close();
                    oConexion.Close();
                }
                catch (Exception)
                {


                }
                finally
                {
                    if (oConexion.State == System.Data.ConnectionState.Open)
                        oConexion.Close();
                }


            }

            if (oFiltros.oidGrupoAparato > 0)
                queryCuentaTotales = queryCuentaTotales + " and a.IOR_GRUPO =" + oFiltros.oidGrupoAparato;

            if (oFiltros.oidMutua > 0)
                queryCuentaTotales = queryCuentaTotales + " and a.IOR_ENTIDADPAGADORA =" + oFiltros.oidMutua;

            if (oFiltros.oidCentro > 0)
                queryCuentaTotales = queryCuentaTotales + " and a.OWNER =" + oFiltros.oidCentro;

            if (oFiltros.oidMedicoInformante > 0)
                queryCuentaTotales = queryCuentaTotales + " and a.IOR_MEDICO =" + oFiltros.oidMedicoInformante;

            if (oFiltros.IOR_COLEGIADO > 0)
                queryCuentaTotales = queryCuentaTotales + " and a.IOR_COLEGIADO =" + oFiltros.IOR_COLEGIADO;

            if (oFiltros.oidEstadoExploracion >= 0 )
            {
                queryCuentaTotales = queryCuentaTotales + " and a.ESTADO =" + oFiltros.oidEstadoExploracion;
            }
            else
            {
                queryCuentaTotales = queryCuentaTotales + " AND(A.ESTADO = 0 or A.ESTADO = 2 or A.ESTADO = 3)";
            }

            if (!String.IsNullOrEmpty(oFiltros.informada) && oFiltros.informada!="A")
            {
                queryCuentaTotales = queryCuentaTotales + " and a.INFORMADA=" + oFiltros.informada.QuotedString();
            }

            if (!String.IsNullOrEmpty(oFiltros.pagado) && oFiltros.pagado != "A")
            {
                queryCuentaTotales = queryCuentaTotales + " and a.PAGADO =" + oFiltros.pagado.QuotedString();
            }

            if (!String.IsNullOrEmpty(oFiltros.facturado) && oFiltros.facturado != "A")
            {
                queryCuentaTotales = queryCuentaTotales + " and a.FACTURADA =" + oFiltros.facturado.QuotedString();
            }

            //PRI MUT O ICS
            if (oFiltros.oidExploracion > 0)
                queryCuentaTotales = queryCuentaTotales + " and a.IOR_GPR =" + oFiltros.oidExploracion;


            queryCuentaTotales = queryCuentaTotales + " GROUP BY A.FECHA";




            //Creamos un array de tipo Key/Value donde la key es la fecha y el value el número de
            //Exploraciones para esa fecha.
            Dictionary<DateTime, int> oDiccionarioCuentaExploraciones = new Dictionary<DateTime, int>();
            bool hayAlgunFiltro = false;
            hayAlgunFiltro = (oFiltros.oidAparato > 0 || oFiltros.oidMedicoInformante > 0 || oFiltros.oidMutua > 0 || !string.IsNullOrEmpty(oFiltros.pagado) || oFiltros.oidEstadoExploracion > 0 || oFiltros.oidGrupoAparato > 0 || oFiltros.oidEstadoExploracion >0|| !string.IsNullOrEmpty(oFiltros.facturado));
            if (UserLogeado.PRIVILEGIADO == -1 || hayAlgunFiltro)
            {
                try
                {
                    oConexion.Open();
                    FbCommand oCommand = new FbCommand(queryCuentaTotales, oConexion);
                    FbDataReader oReader = oCommand.ExecuteReader();

                    while (oReader.Read())
                    {
                        oDiccionarioCuentaExploraciones.Add((DateTime)oReader["FECHA"], (int)oReader["TOTAL"]);
                    }
                    oReader.Close();

                }
                catch (Exception)
                {


                }
                finally
                {
                    if (oConexion.State == System.Data.ConnectionState.Open)
                        oConexion.Close();
                }

            }

            //Creamos una lista de festivos de este mes marcados en la base de datos
            List<FESTIVOS> lFestivos = RadioWeb.Models.Repos.FestivosRepositorio.Obtener(oFiltros.oidAparato, FechaInicial, FechaFinal);


            //Iteramos por los dias del mes que estamos render y buscamos ese dia en los resultados
            //que provienen de la Base de Datos que están el ResultadosBD (Diccionario) en memoria
            for (int i = 1; i <= DateTime.DaysInMonth(fecha.Year, fecha.Month); i++)
            {
                int cuenta = 0;

                if (oDiccionarioCuentaExploraciones.ContainsKey(new DateTime(fecha.Year, fecha.Month, i)))
                {
                    //si el dia en el que estamos en el bucle tiene exploraciones la asignamos para luego llamar al contructor de dia
                    cuenta = oDiccionarioCuentaExploraciones[new DateTime(fecha.Year, fecha.Month, i)];
                }
                string textoDia = "";
                if (oDiccionarioTextosAparato.ContainsKey(new DateTime(fecha.Year, fecha.Month, i)))
                {
                    //si el dia en el que estamos en el bucle tiene exploraciones la asignamos para luego llamar al contructor de dia
                    textoDia = oDiccionarioTextosAparato[new DateTime(fecha.Year, fecha.Month, i)];
                }
                bool EsFestivo = false;
                DateTime FechaCurrentDia = new DateTime(fecha.Year, fecha.Month, i);
                int PorcentageOcupacion = 0;

                FESTIVOS oTemporal = lFestivos.Find(delegate (FESTIVOS festivo)
                {
                    return festivo.FESTIVO == FechaCurrentDia;
                });

                if (oTemporal != null)
                {
                    EsFestivo = true;
                }

                Dias.Add(new Dia(i, FechaCurrentDia, cuenta, i.ToString(), textoDia, EsFestivo, PorcentageOcupacion));
            }



        }
    }
}