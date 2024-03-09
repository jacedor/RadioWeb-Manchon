using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using RadioWeb.Models.Repos;
using RadioWeb.Models.Estadistica;
using RadioWeb.Models;
using RadioWeb.Models.Logica;
using RadioWeb.ViewModels.Estadistica;
using RadioWeb.Repositories;
using RadioWeb.Filters;
using RadioWeb.ViewModels;
using RadioWeb.Models.Clases;
using ADPM.Common;
using System.Globalization;
using System.Net;
using FirebirdSql.Data.FirebirdClient;

namespace RadioWeb.Controllers
{
    [Authorize]
    public class EstadisticasController : Controller
    {

        private RadioDBContext db = new RadioDBContext();
        private UsersDBContext usersDBContext = new UsersDBContext();

        private FiltrosRepository _Filtrosrepository;


        public EstadisticasController()
        {
            _Filtrosrepository = new FiltrosRepository(usersDBContext);
        }
        //
        // GET: /Estadisticas/
        private void RellenarCombos()
        {
            //Al iniciar una session desde cualquier navegador, nos guardamos todas las mutuas
            //en una variable de application, así no tenemos que atacar a la base de datos cada vez que 
            //queramos consultar la lista de mutuas. 
            if (System.Web.HttpContext.Current.Application["Mutuas"] != null)
            {
                IEnumerable<MUTUAS> oListTemp = ((List<MUTUAS>)System.Web.HttpContext.Current.Application["Mutuas"]).Where(s => s.VERS == 1);
                ViewBag.Mutuas = Utils.DropDownList<MUTUAS>.LoadItems(oListTemp.ToList(), "OID", "CODMUT");
            }


            if (System.Web.HttpContext.Current.Application["GrupoAparatos"] != null)
            {
                List<GAPARATOS> oListTemp = (List<GAPARATOS>)System.Web.HttpContext.Current.Application["GrupoAparatos"];
                ViewBag.GrupoAparatos = Utils.DropDownList<GAPARATOS>.LoadItems(oListTemp, "OID", "COD_GRUP");
            }
        }

        public ActionResult ListaEspera()
        {
            ViewData["Centros"] = CentrosRepositorio.Obtener();
            ViewBag.TotalListaEspera = ExploracionRepositorio.ContarListaEspera();

            return View("ListaEspera");
        }


        public ActionResult ListPivotTable()
        {


            USUARIO oUser = UsuariosRepositorio.Obtener(User.Identity.Name);
            List<PIVOTTABLE> oModel = db.PivotTable.Where(p => p.OWNER == oUser.IDUSER || p.OWNER==-1).OrderBy(p=>p.NOMBRE).ToList();
            return View( oModel);
        }

        public ActionResult PivotTable(int oid)
        {
            PIVOTTABLE oTable = db.PivotTable.Single(p => p.OID == oid);
            VWFiltros oFiltros = new VWFiltros
            {
                FECHA_INICIO = oTable.FECHAINICIO.Value.ToString("dd/MM/yyyy"),
                FECHA_FIN = oTable.FECHAFIN.Value.ToString("dd/MM/yyyy"),
                OIDACTIVA =oid
            };
          
            ViewBag.PivotData = oTable.DATOS;
            ViewBag.PivotTitulo = oTable.NOMBRE;
            return View("PivotTable", oFiltros);
        }

        [HttpPost]
        public ActionResult EditarCampoPivot(string name, int pk, string value)
        {

            FbConnection oConexion = new FbConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ConexionBD"].ConnectionString);
            FbCommand oCommand = null;
            try
            {
                oConexion.Open();
                string updateStament = "update PIVOTTABLE set " + name + "='" + value + "'";
                updateStament += " where oid= " + pk;
                oCommand = new FbCommand(updateStament, oConexion);
                oCommand.ExecuteNonQuery();
            }
            catch (Exception)
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

            return new HttpStatusCodeResult(200);
        }
        public ActionResult PivotTableDuplicar(int oid)
        {

        
            PIVOTTABLE oTable = db.PivotTable.Single(p => p.OID == oid);
            PIVOTTABLE oNewTable = new PIVOTTABLE();
            oNewTable.NOMBRE = "Copia de " + oTable.NOMBRE;
            oNewTable.DESCRIPCION = "-";
            oNewTable.BORRADO = "F";
            oNewTable.IOR_EMPRESA = "4";
            oNewTable.FECHAINICIO = oTable.FECHAINICIO;
            oNewTable.FECHAFIN = oTable.FECHAFIN;
            oNewTable.OWNER = oTable.OWNER;
            oNewTable.DATOS = oTable.DATOS;
            db.PivotTable.Add(oNewTable);
            db.SaveChanges();

            

            return RedirectToAction("ListPivotTable", "Estadisticas");
        }

        string GetEstadoText(string estado)
        {
            switch (estado)
            {
                case "0": return "PENDIENTE";
                case "1": return "BORRADO";
                case "2": return "PRESENCIA";
                case "3": return "REALIZADO";
                case "4": return "NO PRESENTADO";
                case "5": return "LLAMA ANULANDO";
                default: return "DESCONOCIDO"; // En caso de un valor no esperado
            }
        }

        public ActionResult PivotTableData(string FECHA_INICIO, string FECHA_FIN)
        {
            //  RellenarCombos();

            IEnumerable<LISTADIA2> oResult;
            
            string query = "select E.*, l.colegiado,  l.mutua,l.explo,l.cantidad,l.paciente,l.fecha,l.medico,c.NOMBRE as CENTROD,g.COD_GRUP GRUPO,l.cirujano,l.aparato,";
            query += "  case l.ESTADO";
            query += " when 0 then 'PENDIENTE'";
            query += " when 1 then 'BORRADO'";
            query += " when 2 then 'PRESENTE'";
            query += " when 3 then 'CONFIRMADO'";
            query += " when 4 then 'LLAMA ANULANDO'";
            query += " when 5 then 'NO PRESENTADO'";
            query += " else '' end ESTADO,l.PAGADO, l.FACTURADA, CEN.NOMBRE AS CENTROEXTERNO, l.INFORMADA,l.TECNICO,tec.login as LOGINTECNICO ";
            query += "from listadia l left join CENTROS c on l.centro=c.OID ";
            query += "LEFT JOIN GAPARATOS G ON G.OID=L.grupoapa ";
            query += "LEFT JOIN CENTROSEXTERNOS CEN ON CEN.OID=l.IOR_CENTROEXTERNO ";
            query += "left join personal tec on tec.COD=l.TECNICO ";
            query += "JOIN EXPLORACION E ON E.OID=L.OID";
            
            query += " where l.fecha between " + DateTime.Parse(FECHA_INICIO).ToString("MM/dd/yyyy").QuotedString()
           + " and " + DateTime.Parse(FECHA_FIN).ToString("MM/dd/yyyy").QuotedString();

            oResult = db.Database.SqlQuery<LISTADIA2>(query).ToList<LISTADIA2>();
            var cultureInfo = new CultureInfo("es-ES");

            var json = from explo in oResult
                       select new
                       {
                           FECHA=explo.FECHA.ToString("dd/MM/yyyy"),
                           FECHA_CREACION = explo.FECHA_IDEN.ToString("dd/MM/yyyy"),
                           USERNAME=explo.USERNAME,
                           CENTROEXTERNO = explo.CENTROEXTERNO,
                           MUTUA = explo.MUTUA,
                           LOGINTECNICO=explo.LOGINTECNICO,
                           EXPLO = explo.EXPLO,
                           explo.CANTIDAD,
                           PACIENTE = explo.PACIENTE,
                           MES = explo.FECHA.Month + " - " + CultureInfo.CurrentCulture.TextInfo.ToTitleCase(System.Globalization.DateTimeFormatInfo.CurrentInfo.GetMonthName(explo.FECHA.Month)),
                           CENTROD = explo.CENTROD,
                           MEDINF = explo.MEDICO,                          
                           ESTADO= GetEstadoText(explo.ESTADO),
                           FACT = explo.FACTURADA,
                           PAGADO=explo.PAGADO,
                           GRUPO = explo.GRUPO,
                           REFERIDOR=explo.COLEGIADO,
                           APARATO = explo.APARATO,
                           INFORMADA = explo.INFORMADA,                        
                           TECNICO = explo.TECNICO

                       };

            var jsonParsed = Json(json, JsonRequestBehavior.AllowGet);
            jsonParsed.MaxJsonLength = 500000000;


            return jsonParsed;

        }

        [HttpPost]
        public ActionResult PivotTableData(int oid, string fechaInicio, string fechafin,string data)
        {

          
            PIVOTTABLE oTable = db.PivotTable.Single(p => p.OID == oid);
            oTable.DATOS = data;
            oTable.FECHAINICIO =DateTime.Parse( fechaInicio);
            oTable.FECHAFIN = DateTime.Parse(fechafin);
            db.Entry(oTable).State = System.Data.Entity.EntityState.Modified;
            db.SaveChanges();
            return new HttpStatusCodeResult(HttpStatusCode.OK);
        }

        public ActionResult Facturacion2()
        {
            VWEstadistica oFiltros = new VWEstadistica();
            int anyo = DateTime.Now.Year;

            for (int i = 0; i < 10; i++)
            {
                oFiltros.ANYOSLIST.Add(anyo - i);
            }
            return View(oFiltros);
        }

        [HttpPost]
        public ActionResult Facturacion2(VWEstadistica oViewModel)
        {
            itemEstadisticaC3 oResult = new itemEstadisticaC3();
            oResult.axis.Add("Ene");
            oResult.axis.Add("Feb");
            oResult.axis.Add("Mar");
            oResult.axis.Add("Abr");
            oResult.axis.Add("May");
            oResult.axis.Add("Jun");
            oResult.axis.Add("Jul");
            oResult.axis.Add("Ago");
            oResult.axis.Add("Sep");
            oResult.axis.Add("Oct");
            oResult.axis.Add("Nov");
            oResult.axis.Add("Dic");


            foreach (var item in oViewModel.ANYOSSELECTED)
            {
                var itemestadistico = EstadisticaRepositorio.GetFacturacionAnualPorMeses(item.ToString());
                List<object> oAnyo = new List<object>();
                oAnyo.Add(item.ToString());
                foreach (var actualItem in itemestadistico)
                {
                    if (oViewModel.SUMCOUNT == "SUM")
                    {
                        oAnyo.Add(actualItem.Ventas);

                    }
                    else
                    {
                        oAnyo.Add(actualItem.Cuenta);

                    }
                }
                oResult.data.Add(oAnyo);

            }
            oResult.tipografico = oViewModel.TIPOGRAFICO;

            return Json(oResult, JsonRequestBehavior.AllowGet);

        }


        public ActionResult Facturacion()
        {
            WebConfigRepositorio oConfig = new WebConfigRepositorio();
            ItemEstadisticoVentas oAnual = EstadisticaRepositorio.GetFacturacionAnual();
            ViewBag.ExploracionesPendientesAnualesCobrar = EstadisticaRepositorio.GetPendientesFacturacionAnual().Cuenta;
            string Simbolo = MonedaRepositorio.Obtener(int.Parse(oConfig.ObtenerValor("IOR_MONEDA"))).SIMBOLO;
            if (Simbolo == "€")
            {
                ViewBag.FacturacionAnual = oAnual.Ventas.ToString("c");
            }
            else
            {
                var culture = new System.Globalization.CultureInfo(oConfig.ObtenerValor("CULTURE"));
                ViewBag.FacturacionAnual = oAnual.Ventas.ToString("C",culture);

            }

            ViewBag.ExploracionesAnualesCobradas = oAnual.Cuenta;

            return View();
        }


        public ActionResult Ocupacion()
        {

            //List<GAPARATOS> oListTemp = (List<GAPARATOS>)System.Web.HttpContext.Current.Application["GrupoAparatos"];
            //ViewBag.GrupoAparatos = Utils.DropDownList<GAPARATOS>.LoadItems(oListTemp, "OID", "COD_GRUP");
            ItemEstadisticoOcupacion oModel = new ItemEstadisticoOcupacion();
            return View(oModel);
        }


        [HttpPost]
        public ActionResult Ocupacion(string Fecha_Inicio, string Fecha_Fin, int oidGrupo)
        {
            //List<ItemEstadisticoOcupacion> oOcupacion = EstadisticaRepositorio.ActiviadPorDias(DateTime.Now.AddDays(-7).ToString("dd/MM/yyyy"),DateTime.Now.ToString("dd/MM/yyyy"),42);

            List<ItemEstadisticoOcupacion> oOcupacion = EstadisticaRepositorio.ActividadPorMes(Fecha_Inicio, Fecha_Fin, oidGrupo);
            return PartialView("_OcupacionLista", oOcupacion);


        }


        public ActionResult OcupacionPorHoras(string Fecha_Inicio, string Fecha_Fin, int ior_aparato)
        {
            //List<ItemEstadisticoOcupacion> oOcupacion = EstadisticaRepositorio.ActiviadPorDias(DateTime.Now.AddDays(-7).ToString("dd/MM/yyyy"),DateTime.Now.ToString("dd/MM/yyyy"),42);

            List<ESTADISTICAHUECOSOCUPACION> oOcupacion = EstadisticaRepositorio.CalculoOcupacion(Fecha_Inicio, Fecha_Fin, ior_aparato);
            var serializer = new System.Web.Script.Serialization.JavaScriptSerializer();
            serializer.MaxJsonLength = 500000000;

            var json = Json(oOcupacion, JsonRequestBehavior.AllowGet);
            json.MaxJsonLength = 500000000;


            return json;


        }


        public JsonResult GetListaEsperaPorGrupos()
        {

            List<ItemEstadisticoExploraciones> oRecuento = EstadisticaRepositorio.ListaEsperaPorGrupo();
            return Json(oRecuento, JsonRequestBehavior.AllowGet);


        }

        public JsonResult GetListaEsperaPorCentros()
        {

            List<ItemEstadisticoExploraciones> oRecuento = EstadisticaRepositorio.ListaEsperaPorCentro();

            return Json(oRecuento, JsonRequestBehavior.AllowGet);


        }
        //uno lo usamos para el pintado del donut en formato json
        public JsonResult GetListaDeEsperaDeUnCentro(string centro)
        {
            List<ItemEstadisticoExploraciones> oRecuento = EstadisticaRepositorio.ListaEsperaPorGruposYCentro(centro);
            return Json(oRecuento, JsonRequestBehavior.AllowGet);

        }
        //uno lo usamos para el pintado de la tabla
        public ActionResult GetListaDeEsperaDeUnCentroList(string centro)
        {

            List<ItemEstadisticoExploraciones> oRecuento = EstadisticaRepositorio.ListaEsperaPorGruposYCentro(centro);
            return PartialView("ListaCuentaGrupos", oRecuento);


        }
        public ActionResult GetListaDeEsperaDeUnCentroYGrupo(string centro, string grupo)
        {

            List<ItemEstadisticoExploraciones> oLista = EstadisticaRepositorio.ListaEsperaAparatos(centro, grupo);
            return PartialView("ListaCuentaAparato", oLista);


        }
        public ActionResult GetListaDeEsperaDeAparato(string Aparato)
        {
            List<ItemEstadisticoExploraciones> oLista = EstadisticaRepositorio.ListaEsperaTipoExploracion(Aparato);
            return PartialView("ListaCuentaTipoExploracion", oLista);


        }
        public JsonResult GetListaEsperaPorGrupoYCentros()
        {

            List<ItemEstadisticoExploraciones> oRecuento = EstadisticaRepositorio.ListaEsperaPorGruposYCentro();
            return Json(oRecuento, JsonRequestBehavior.AllowGet);

        }


        public JsonResult GetFacturacionDeUnDia(string dia)
        {

            ItemEstadisticoVentas oRecuentoDia = EstadisticaRepositorio.GetFacturacionDia(dia);
            return Json(oRecuentoDia, JsonRequestBehavior.AllowGet);

        }

        //public JsonResult GetFacturacionSemanas()
        //{

        //    List<ItemEstadisticoVentas> oRecuento = EstadisticaRepositorio.GetFacturacionSemanas();

        //    return Json(oRecuento, JsonRequestBehavior.AllowGet);

        //}


        public JsonResult GetFacturacionAnualPorMeses(string anyo)
        {

            List<ItemEstadisticoVentas> oRecuento = EstadisticaRepositorio.GetFacturacionAnualPorMeses();
            //Recuento Año Anterior
            List<ItemEstadisticoVentas> oRecuentoAñoAnterior = EstadisticaRepositorio.GetFacturacionAnualPorMeses((int.Parse(anyo) - 1).ToString());

            foreach (ItemEstadisticoVentas item in oRecuentoAñoAnterior)
            {
                oRecuento.Add(item);
            }

            return Json(oRecuento, JsonRequestBehavior.AllowGet);

        }

        public JsonResult GetFacturacionDiariaDeUnMes(string start, string end)
        {

            List<ItemEstadisticoVentas> oRecuento = EstadisticaRepositorio.GetFacturacionDeUnMes(start, end);

         
            return Json(oRecuento, JsonRequestBehavior.AllowGet);


        }

        public JsonResult GetFacturacionAnualPorGrupo(string anyo)
        {

            List<ItemEstadisticoVentas> oRecuento = EstadisticaRepositorio.GetFacturacionAnualPorGrupo();
            return Json(oRecuento, JsonRequestBehavior.AllowGet);


        }


        public JsonResult GetFacturacionAnualPorICSPRIMUT(string anyo)
        {

            List<ItemEstadisticoVentas> oRecuento = EstadisticaRepositorio.GetFacturacionAnualPorPRIMUTICS(anyo);
            return Json(oRecuento, JsonRequestBehavior.AllowGet);


        }

        [ConfiguracionVisualAttribute]
        public ActionResult Resumen(string fecha = "")
        {
            //  RellenarCombos();
            VWFiltrosResumenFacturacion oViewModel = new VWFiltrosResumenFacturacion();
            oViewModel.FILTROS = new ViewModels.VWFiltros();
          
            
            if (String.IsNullOrEmpty(fecha))
            {
                oViewModel.FILTROS.FECHA = (ViewData["ESTADISTICAS.FECHA"] != null ? ViewData["ESTADISTICAS.FECHA"].ToString() : new DateTime(DateTime.Now.Year, 1, 1).ToString("dd/MM/yyyy") + " - " + DateTime.Now.ToString("dd/MM/yyyy"));
            }
            else
            {
                oViewModel.FILTROS.FECHA = fecha;
                var usuarioAutenticated = UsuariosRepositorio.Obtener(User.Identity.Name);
                _Filtrosrepository.Guardar(usuarioAutenticated.IDUSER, "ESTADISTICAS", "FECHA", fecha, "string");

            }
            ViewBag.EsconderPromedio = false; 
            return View("Resumen", oViewModel);
        }

        [HttpPost]
        public ActionResult ResumenMensual(string fechaInicial, string fechaFinal, int tipoPago,
            int centro, List<int> mutua, string pagado, string facturado,
            int grupo, int oidMedicoInformante, int ior_colegiado, int anyo)
        {
            FiltrosBusquedaExploracion oFiltros = new FiltrosBusquedaExploracion
            {
                oidGrupoAparato = grupo,
                oidExploracion = tipoPago,
                oidCentro = centro,
                MutuaList = mutua,
                oidMedicoInformante = oidMedicoInformante,
                IOR_COLEGIADO = ior_colegiado,
                pagado = pagado,
                facturado = facturado,
                anyo = anyo
            };
            List<ItemResumen> oRecuento = EstadisticaRepositorio.GetResumenFacturacionPorGrupo(fechaInicial, fechaFinal, oFiltros);
            ViewBag.EsconderPromedio = true; 
            ViewBag.Actual = DateTime.Parse(fechaInicial).Year.ToString();
            ViewBag.Comparado = (DateTime.Parse(fechaInicial).Year - anyo).ToString();
            return PartialView("ResumenTable", oRecuento);
        }

        [HttpPost]
        public ActionResult ResumenDiario(string fechaInicial, string fechaFinal, int tipoPago, int centro, List<int> mutua, string pagado, int grupo, int oidMedicoInformante, int ior_colegiado, int anyo)
        {
            FiltrosBusquedaExploracion oFiltros = new FiltrosBusquedaExploracion
            {
                oidGrupoAparato = grupo,
                oidExploracion = tipoPago,
                oidCentro = centro,
                MutuaList = mutua,
                oidMedicoInformante = oidMedicoInformante,
                IOR_COLEGIADO = ior_colegiado,
                pagado = pagado,
                anyo = anyo
            };


            List<ItemResumen> oRecuento = EstadisticaRepositorio.GetResumenFacturacionPorGrupo(fechaInicial, fechaFinal, oFiltros, true);

            ViewBag.Actual = DateTime.Parse(fechaInicial).Year.ToString();
            ViewBag.Comparado = (DateTime.Parse(fechaInicial).Year - anyo).ToString();

            return PartialView("ResumenTableDiario", oRecuento);


        }
        [HttpPost]
        public ActionResult ResumenAcumuladoEvolutivo(string fechaInicial, string fechaFinal, int tipoPago, int centro,
                     List<int> mutua, string pagado, string facturado, int grupo, int oidMedicoInformante, int ior_colegiado, int anyo)
        {
            FiltrosBusquedaExploracion oFiltros = new FiltrosBusquedaExploracion
            {
                oidGrupoAparato = grupo,
                oidExploracion = tipoPago,
                oidCentro = centro,
                MutuaList = mutua,
                oidMedicoInformante = oidMedicoInformante,
                IOR_COLEGIADO = ior_colegiado,
                pagado = pagado,
                facturado = facturado,
                anyo = anyo
            };

            ViewBag.Actual = DateTime.Parse(fechaInicial).Year.ToString();
            ViewBag.Comparado = (DateTime.Parse(fechaInicial).Year - anyo).ToString();
            List<ItemResumen> oRecuento = EstadisticaRepositorio.GetResumenFacturacionEvolutivaAcumulado(fechaInicial, fechaFinal, oFiltros);

            return PartialView("ResumenTableAcumuladoEvolutivo", oRecuento);


        }

        [HttpPost]
        public ActionResult ResumenAcumulado(string fechaInicial, string fechaFinal, int tipoPago, int centro,
                        List<int> mutua, string pagado, string facturado, int grupo, int oidMedicoInformante, int ior_colegiado, int anyo)
        {
            FiltrosBusquedaExploracion oFiltros = new FiltrosBusquedaExploracion
            {
                oidGrupoAparato = grupo,
                oidExploracion = tipoPago,
                oidCentro = centro,
                MutuaList = mutua,
                oidMedicoInformante = oidMedicoInformante,
                IOR_COLEGIADO = ior_colegiado,
                pagado = pagado,
                anyo = anyo,
                facturado = facturado
            };

            ViewBag.Actual = DateTime.Parse(fechaInicial).Year.ToString();
            ViewBag.Comparado = (DateTime.Parse(fechaInicial).Year - anyo).ToString();
            List<ItemResumen> oRecuento = EstadisticaRepositorio.GetResumenFacturacionPorGrupoAcumulado(fechaInicial, fechaFinal, oFiltros);

            return PartialView("ResumenTableAcumulado", oRecuento);


        }

        public ActionResult FacturasPorMeses(List<int> FILTROS_IOR_ENTIDADPAGADORA, string FILTROS_FECHA = "")
        {
            //  RellenarCombos();

            string fechaInicial = "";
            string fechaFinal = "";
            List<int> listaMutuas = new List<int>();
            //if (String.IsNullOrEmpty(FILTROS_FECHA))
            //{
            //    fechaInicial = DateTime.Now.AddYears(-1).ToString("dd/MM/yyyy");
            //}
            //else
            //{
            //    fechaInicial = FILTROS_FECHA.Split('-')[0];
            //}
            if (String.IsNullOrEmpty(FILTROS_FECHA))
            {
                fechaFinal = DateTime.Now.ToString("dd/MM/yyyy");
            }
            //else
            //{
            //    fechaFinal = FILTROS_FECHA.Split('-')[1];
            //}

            if (FILTROS_IOR_ENTIDADPAGADORA == null)
            {
                FILTROS_IOR_ENTIDADPAGADORA = new List<int>();
                FILTROS_IOR_ENTIDADPAGADORA.Add(0);
            }

            FiltrosBusquedaExploracion oFiltros = new FiltrosBusquedaExploracion
            {

                MutuaList = FILTROS_IOR_ENTIDADPAGADORA.ToList(),

            };
            VWFiltros oViewModel = new VWFiltros();
            oViewModel.FECHA_INICIO = "01/01/" + DateTime.Now.Year.ToString();
            oViewModel.FECHA_FIN = DateTime.Now.ToString("dd/MM/yyyy");

            //oViewModel.FECHA = fechaInicial + " - " + fechaFinal;


            return View("FacturasPorMeses", oViewModel);
        }

        [HttpPost]
        public ActionResult FacturasPorMeses(VWFiltros oVModel, IEnumerable<int> MutuaList)
        {
            //  RellenarCombos();

            //string fechaInicial = "";
            //string fechaFinal = "";
            List<int> listaMutuas = new List<int>();
            //if (String.IsNullOrEmpty(oVModel.FECHA))
            //{
            //    fechaInicial = DateTime.Now.AddYears(-1).ToString("dd/MM/yyyy");
            //}
            //else
            //{
            //    fechaInicial = oVModel.FECHA.Split('-')[0];
            //}
            //if (String.IsNullOrEmpty(oVModel.FECHA))
            //{
            //    fechaFinal = DateTime.Now.ToString("dd/MM/yyyy");
            //}
            //else
            //{
            //    fechaFinal = oVModel.FECHA.Split('-')[1];
            //}



            FiltrosBusquedaExploracion oFiltros = new FiltrosBusquedaExploracion();
            oFiltros.oidCentro = oVModel.IOR_CENTRO;
            oFiltros.MutuaList = new List<int>();
            if (MutuaList == null)
            {
                oFiltros.MutuaList.Add(0);
            }
            else
            {
                foreach (var item in MutuaList)
                {
                    oFiltros.MutuaList.Add(item);
                }
            }

            VWFiltros oViewModel = new VWFiltros();
            List<ItemFacturasMes> oModelTable = EstadisticaRepositorio
                .GetFacturasMeses(oVModel.FECHA_INICIO, oVModel.FECHA_FIN, oFiltros);


            return PartialView("_FacturasMesesPartial", oModelTable);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                usersDBContext.Dispose();
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
