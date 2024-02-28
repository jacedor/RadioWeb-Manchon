using RadioWeb.Models;
using RadioWeb.Models.Logica;
using RadioWeb.Models.Repos;
using RadioWeb.Utils;
using RadioWeb.ViewModels;
using RadioWeb.ViewModels.Paciente;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace RadioWeb.Controllers
{
    public class AgendaMultiController : Controller
    {

        private bool EsFestivo(string fecha, int oidAparato)
        {

            return !(FestivosRepositorio.Obtener(oidAparato, DateTime.Parse(fecha).ToString("yyyy/MM/dd")).Count == 0 && DateTime.Parse(fecha).DayOfWeek != DayOfWeek.Sunday);
        }


        [OutputCache(NoStore = true, Duration = 0, VaryByParam = "None")]
        public ActionResult Index()
        {
            int numeroBuscadores = 6;
            FiltrosBusquedaExploracion oFiltros = (FiltrosBusquedaExploracion)Session["FiltrosBusqueda"];
            List<VWAgendaMultiple> oResultado = new List<VWAgendaMultiple>();
            if (oFiltros == null)
            {
                oFiltros = new FiltrosBusquedaExploracion();
                oFiltros.DescAparato = "-1";
                oFiltros.Fecha = DateTime.Now.ToString("dd-MM-yyyy");
            }


            List<VWAgendaMultiple> oCondicionesBusqueda = (List<VWAgendaMultiple>)Session["CondicionesBusquedaAgendaMultiple"];
            if (oCondicionesBusqueda != null)
            {
                oFiltros.Fecha = oCondicionesBusqueda.First().Fecha.ToString("dd-MM-yyyy");
                oFiltros.oidAparato = oCondicionesBusqueda.First().oidAparato;
            }
            else
            {
                while (EsFestivo(oFiltros.Fecha, oFiltros.oidAparato))
                {
                    oFiltros.Fecha = DateTime.Parse(oFiltros.Fecha).AddDays(1).ToString("dd/MM/yyyy");
                }
            }

            for (int i = 0; i < numeroBuscadores; i++)
            {
                if (oCondicionesBusqueda != null)
                {
                    oFiltros.Fecha = oCondicionesBusqueda[i].Fecha.ToString("dd-MM-yyyy");
                    oFiltros.oidAparato = oCondicionesBusqueda[i].oidAparato;
                    oFiltros.DescAparato = DaparatoRepositorio.Obtener(oFiltros.oidAparato).COD_FIL;
                }
                if (oFiltros.Borrados == "True")
                {
                    oFiltros.Borrados = "T";
                }
                else
                {
                    oFiltros.Borrados = "F";
                }

                VWAgendaMultiple oResult = new VWAgendaMultiple();
                IEnumerable<LISTADIAAMBFORATS> oListaDia;
                oListaDia = ListaDiaAmbForatsRepositorio.Get(oFiltros);
                oResult.Fecha = DateTime.Parse(oFiltros.Fecha);
                oResult.ListaDia = oListaDia.ToList();
                oResult.oidAparato = oFiltros.oidAparato;
                oResult.DiaSemana = DateTimeFormatInfo.CurrentInfo.GetDayName(oResult.Fecha.DayOfWeek);
                oResult.TextoAgenda = AgendaGenRepositorio.Obtener(oFiltros.oidAparato, oResult.Fecha);
                oResult.HuecosLibres = oListaDia.Where(x => x.OID <= 0 && !x.ANULADA).Count().ToString();

                if (oResult.ListaDia.Count == 0 && DateTime.Parse(oFiltros.Fecha).DayOfWeek == DayOfWeek.Saturday)
                {
                    numeroBuscadores = numeroBuscadores + 1;
                }
                else
                {
                    oResultado.Add(oResult);
                }

                if (oCondicionesBusqueda == null)
                {
                    oFiltros.Fecha = DateTime.Parse(oFiltros.Fecha).AddDays(1).ToString("dd/MM/yyyy");
                    while (EsFestivo(oFiltros.Fecha, oFiltros.oidAparato))
                    {
                        oFiltros.Fecha = DateTime.Parse(oFiltros.Fecha).AddDays(1).ToString("dd/MM/yyyy");
                    }

                }
            }


            return View("Index", oResultado);
        }

        [HttpPost]
        public ActionResult Index(FiltrosBusquedaExploracion oFiltros, string Direccion = "ASC", int numeroBuscadores = 4, bool saltaFestivos = false)
        {
            List<VWAgendaMultiple> oResultado = new List<VWAgendaMultiple>();
            string fecha = oFiltros.Fecha;

            for (int i = 0; i < numeroBuscadores; i++)
            {
                if (oFiltros.Borrados == "True")
                {
                    oFiltros.Borrados = "T";
                }
                else
                {
                    oFiltros.Borrados = "F";
                }


                if (saltaFestivos)
                {
                    //PRIMERO HEMOS INTENTADO OBTENER UN DIA HABIL CON LA FECHA PASADA POR DEFECTO AL METODO, PERO SI ES FESTIVO O
                    //SI NO TIENES 

                    while (EsFestivo(fecha, oFiltros.oidAparato) || (DateTime.Parse(fecha).DayOfWeek == DayOfWeek.Saturday))
                    {
                        if (Direccion == "DESC")
                        {
                            fecha = DateTime.Parse(fecha).AddDays(-1).ToString("dd/MM/yyyy");
                        }
                        else
                        {
                            fecha = DateTime.Parse(fecha).AddDays(1).ToString("dd/MM/yyyy");
                        }
                        oFiltros.Fecha = fecha;
                    }

                }

                VWAgendaMultiple oResult = new VWAgendaMultiple();
                IEnumerable<LISTADIAAMBFORATS> oListaDia;
                oFiltros.Fecha = fecha;
                oListaDia = ListaDiaAmbForatsRepositorio.Get(oFiltros);
                oResult.Fecha = DateTime.Parse(oFiltros.Fecha);
                oResult.ListaDia = oListaDia.ToList();
                oResult.oidAparato = oFiltros.oidAparato;
                oResult.DiaSemana = DateTimeFormatInfo.CurrentInfo.GetDayName(oResult.Fecha.DayOfWeek);
                oResult.TextoAgenda = AgendaGenRepositorio.Obtener(oFiltros.oidAparato, oResult.Fecha);
                oResult.HuecosLibres = oListaDia.Where(x => x.OID <= 0 && !x.ANULADA).Count().ToString();
                oResultado.Add(oResult);

                fecha = DateTime.Parse(fecha).AddDays(1).ToString("dd/MM/yyyy");
                while (EsFestivo(fecha, oFiltros.oidAparato))
                {
                    fecha = DateTime.Parse(fecha).AddDays(1).ToString("dd/MM/yyyy");
                    oFiltros.Fecha = fecha;
                }

            }
            //si solo hemos solicitado un buscador se trata de reemplazar solo uno de ellos, sino de rellenar todos.
            if (numeroBuscadores == 1)
            {
                return PartialView("_ListaDia", oResultado.First());
            }
            else
            {
                return PartialView("_ListaDiaMultiple", oResultado);
            }

        }



        public string GetPrecioExploracion(int IOR_TIPOEXPLORACION, int IOR_MUTUA)
        {
            string Result = "0";
            MUTUAS oMutua = MutuasRepositorio.Obtener(IOR_MUTUA);
            if (oMutua.IOR_CENTRAL != null && oMutua.IOR_CENTRAL > 0)
            {
                Result = TarifasRepositorio.ObtenerPrecioExploracion(IOR_TIPOEXPLORACION, (int)oMutua.IOR_CENTRAL);
            }
            else
            {
                Result = TarifasRepositorio.ObtenerPrecioExploracion(IOR_TIPOEXPLORACION, oMutua.OID);
            }
            return Result;
        }



        [HttpPost]
        public void GuardarHuecosSeleccionados(List<EXPLORACION> oLista)
        {
            List<EXPLORACION> oListaHuecosSeleccionados = new List<EXPLORACION>();
            for (int i = 0; i < oLista.Count; i++)
            {
                EXPLORACION temp = oLista[i];
                temp.DAPARATO = DaparatoRepositorio.Obtener((int)temp.IOR_APARATO);
                oListaHuecosSeleccionados.Add(temp);
            }

            Session["HuecosSeleccionadosAgendaMultiple"] = oListaHuecosSeleccionados;

        }

        [HttpPost]
        public void GuardarCondicionesBusqueda(List<VWAgendaMultiple> oLista)
        {

            Session["CondicionesBusquedaAgendaMultiple"] = oLista;

        }

        public ActionResult AddPaso1()
        {
            return View("AddPaso1", (List<EXPLORACION>)Session["HuecosSeleccionadosAgendaMultiple"]);
        }




        public ActionResult AddPaso2(int oid)
        {

            PACIENTE oPaciente;
            if (oid == 0)
            {
                oPaciente = new PACIENTE();
                oPaciente.TELEFONOS.Add(new TELEFONO());
                //oPaciente.HAYCAMBIOSPACIENTE = "T";
                if (Session["FiltrosBusquedaPaciente"] != null)
                {
                    FiltrosBusquedaPaciente oFiltroPac = (FiltrosBusquedaPaciente)Session["FiltrosBusquedaPaciente"];
                    oPaciente.PACIENTE1 = oFiltroPac.Nombre;
                }
            }
            else
            {
                oPaciente = PacienteRepositorio.Obtener(oid);
            }
            VMAddPaso2 oviewModel = new VMAddPaso2();
            oviewModel.PACIENTEALTA = oPaciente;

            return View("AddPaso2", oviewModel);

        }

        public ActionResult AddPaso3(PACIENTE oPaciente)
        {


            if (oPaciente.OID == 0)
            {
                oPaciente.OID = PacienteRepositorio.Insertar(oPaciente);
            }
            else
            {
                PacienteRepositorio.Update(oPaciente);
            }

            // ViewBag.Mutuas = System.Web.HttpContext.Current.Application["Mutuas"];
            List<EXPLORACION> oLista = (List<EXPLORACION>)Session["HuecosSeleccionadosAgendaMultiple"];
            List<EXPLORACION> oListaInicializada = new List<EXPLORACION>();

            for (int i = 0; i < oLista.Count; i++)
            {
                EXPLORACION temp = oLista[i];
                temp.IOR_PACIENTE = oPaciente.OID;
                temp.IOR_GRUPO = temp.DAPARATO.OWNER;
                temp.IOR_PACIENTE = oPaciente.OID;
                temp.PACIENTE = PacienteRepositorio.Obtener(oPaciente.OID);
                temp.ENTIDAD_PAGADORA = MutuasRepositorio.Obtener((int)temp.PACIENTE.CID);
                temp.IOR_ENTIDADPAGADORA = temp.ENTIDAD_PAGADORA.OID;
                temp.INFOMUTUA = InfoMutuasRepositorio.Obtener((int)temp.PACIENTE.CID);
                List<DAPARATOS> oListaAparatosComplejo = DaparatoRepositorio.EsAparatoComplejo(temp.DAPARATO.COD_FIL);
                if (oListaAparatosComplejo.Count > 0)
                {
                    temp.DAPARATOS = oListaAparatosComplejo;
                }
                ExploracionRepositorio.InicializarExploracionDeApi(ref temp);
                //SI HEMOS LLEGADO A ESTA PANTALLA CON UN APARATO COMPLEJO, LOS TIPOS DE EXPLORACION NO ESTARAN RELLANADOS, 
                //POR TANTO, TENDREMOS QUE SELECCIONAR EL PRIMER APARATO (FIRST) Y RELLENAR LAS EXPLORACOINES
                if (oListaAparatosComplejo.Count > 0)
                {
                    temp.EXPLORACIONESCONTARIFA = TarifasRepositorio.ObtenerPorAparatoYMutua(temp.DAPARATOS.First().OID, temp.IOR_ENTIDADPAGADORA);
                }
                oListaInicializada.Add(temp);
            }

            Session["HuecosSeleccionadosAgendaMultiple"] = oListaInicializada;

            // ViewBag.Tratamientos = DataBase.Tratamientos();
            return View("AddPaso3", oListaInicializada);
        }


        [HttpPost]
        public ActionResult GuardarPaso3(List<EXPLORACION> EXPLORACION)
        {

            foreach (var item in EXPLORACION)
            {
                EXPLORACION oExploracion = new Models.EXPLORACION();
                oExploracion.IOR_APARATO = item.IOR_APARATO;
                oExploracion.HORA = item.HORA;
                oExploracion.DAPARATO = DaparatoRepositorio.Obtener((int)item.IOR_APARATO);
                oExploracion.IOR_GRUPO = oExploracion.DAPARATO.OWNER;
                oExploracion.OWNER = oExploracion.DAPARATO.CID;
                oExploracion.FECHA = item.FECHA;
                oExploracion.IOR_PACIENTE = item.IOR_PACIENTE;
                PACIENTE oPaciente = PacienteRepositorio.Obtener((int)item.IOR_PACIENTE);
                oExploracion.ENTIDAD_PAGADORA = MutuasRepositorio.Obtener((int)item.IOR_ENTIDADPAGADORA);
                oExploracion.IOR_ENTIDADPAGADORA = item.IOR_ENTIDADPAGADORA;
                if (oPaciente.CID != item.IOR_ENTIDADPAGADORA)
                {
                    PacienteRepositorio.UpdateCampo("CID", item.IOR_ENTIDADPAGADORA.Value.ToString(), (int)oExploracion.IOR_PACIENTE, "INT");


                }


                ExploracionRepositorio.InicializarExploracionDeApi(ref oExploracion);
                ExploracionRepositorio.Obtener(ExploracionRepositorio.Insertar(oExploracion));
            }





            return RedirectToAction("Index", "AgendaMulti", new { oidPaciente = EXPLORACION.First().IOR_PACIENTE });
        }



    }
}
