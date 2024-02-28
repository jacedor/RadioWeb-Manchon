using FirebirdSql.Data.FirebirdClient;
using RadioWeb.Models;
using RadioWeb.Models.Logica;
using RadioWeb.Models.Repos;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.IO;
using FastReport.Web;
using System.Web.UI.WebControls;
using RadioWeb.Utils;
using FastReport;
using System.Diagnostics;
using System.Net;
using FastReport.Data;
using RadioWeb.ViewModels;
using ADPM.Common;
using RadioWeb.Models.VidSigner;
using RadioWeb.Filters;
using RadioWeb.ViewModels.Exploracion;
using RadioWeb.ViewModels.Informes;
using System.Drawing;
using System.Drawing.Imaging;
using iTextSharp.text.pdf;
using System.Data.Entity;
using RadioWeb.ViewModels.Paciente;
using RadioWeb.Repositories;
using System.Globalization;
using CrystalDecisions.CrystalReports.Engine;
using CrystalDecisions.Shared;
using System.Reflection;

namespace RadioWeb.Controllers
{
    [SessionExpireFilterAttribute]
    public class ExploracionController : Controller
    {

        private RadioDBContext db = new RadioDBContext();
        private UsersDBContext usersDBContext = new UsersDBContext();
        private DireccionRepository _direccionRepository;
        private TelefonoRepository _telefonoRepository;
        private FiltrosRepository _Filtrosrepository;
        public static DataTable ToDataTable<T>(List<T> items)
        {
            DataTable dataTable = new DataTable(typeof(T).Name);

            //Get all the properties
            PropertyInfo[] Props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (PropertyInfo prop in Props)
            {
                
                System.TypeCode typeCode = Type.GetTypeCode(prop.PropertyType);
                //Setting column names as Property names
                switch (typeCode)
                {
                    case TypeCode.DateTime:
                        dataTable.Columns.Add(prop.Name, typeof(DateTime));

                        break;
                    case TypeCode.String:
                        dataTable.Columns.Add(prop.Name, typeof(string));

                        break;
                    case TypeCode.Double:
                        dataTable.Columns.Add(prop.Name, typeof(Double));

                        break;
                    default:
                        dataTable.Columns.Add(prop.Name, typeof(string));
                        break;
                }
               // dataTable.Columns.Add(prop.Name);
            }
            foreach (T item in items)
            {
                var values = new object[Props.Length];
                for (int i = 0; i < Props.Length; i++)
                {
                    //inserting property values to datatable rows
                    values[i] = Props[i].GetValue(item, null);
                }
                dataTable.Rows.Add(values);
            }
            //put a breakpoint here and check datatable
            return dataTable;
        }
        public ExploracionController()
        {
            _direccionRepository = new DireccionRepository(db);
            _telefonoRepository = new TelefonoRepository(db);
            _Filtrosrepository = new FiltrosRepository(usersDBContext);
        }

        public ActionResult ArqueoPrint(string fechaArqueo)
        {

            WebConfigRepositorio oConfig = new WebConfigRepositorio();
        

            ReportDocument rd = new ReportDocument();
                        rd.Load(Path.Combine(Server.MapPath("~/Content/Reports"), oConfig.ObtenerValor("ArqueoDiario").ToUpper()));
            string camposQueryStandard = "select * from arqueodiario where fecha=" + DateTime.Parse(fechaArqueo).ToString("MM/dd/yyyy").QuotedString();
            var oResult = db.Database.SqlQuery<ARQUEODIARIO>(camposQueryStandard).ToList<ARQUEODIARIO>();
            DataTable dt = ToDataTable<ARQUEODIARIO>(oResult);
         

            rd.SetDataSource(dt);
            //rd.SetParameterValue("FECHAPARAM", fechaArqueo);
            Response.Buffer = false;
            Response.ClearContent();
            Response.ClearHeaders();
            Stream stream = rd.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
            return File(stream, "application/pdf");
        }

   

        public void AnularHoraLibre(FiltrosBusquedaExploracion oFiltros)
        {
            HorasAnulasRepositorio.Insertar(oFiltros.Fecha, oFiltros.oidAparato, oFiltros.Hora, oFiltros.Comentario);
        }

        public FileResult DownloadLopd(int oid)
        {
            RadioDBContext db = new RadioDBContext();
            VID_DOCUMENTOS oDocum = db.Vid_Documentos.Where(d => d.IOR_PACIENTE.Value == oid && d.CID == 2).OrderByDescending(d => d.FECHA).FirstOrDefault();
            byte[] fileBytes = System.IO.File.ReadAllBytes(oDocum.NOMBRE);
            string fileName = "LOPD_" + oid + ".pdf";
            db.Dispose();
            return File(fileBytes, System.Net.Mime.MediaTypeNames.Application.Octet, fileName);
        }

        public FileResult Download(string Ruta)
        {
            byte[] fileBytes = System.IO.File.ReadAllBytes(Ruta);
            string fileName = "Consentimiento.pdf";
            return File(fileBytes, System.Net.Mime.MediaTypeNames.Application.Octet, fileName);
        }
        //Método de si es posible vincula una exploración mal citada porque proviene de una
        //cita que deberia haber entrado por petición HL7

        [ExploracionActivaFilter]
        public ActionResult vincularHL7(int oidOrigen, int oidDestino)
        {
            ViewBag.ExploracionActiva = oidDestino;
            EXPLORACION exploracionOrigen = null;
            try
            {
                exploracionOrigen = ExploracionRepositorio.Obtener(oidOrigen);
                if (exploracionOrigen == null || exploracionOrigen.IDCITAONLINE.Length == 0)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.NotFound);
                }
                if (oidOrigen == oidDestino )
                {
                    return new HttpStatusCodeResult(HttpStatusCode.NotFound);
                }
                switch (exploracionOrigen.IOR_ENTIDADPAGADORA)
                {
                    case 139:
                    case 11042 ://badalona de serveis asistencials
                               //La exploracion que tiene tiene un  nombre de archivo de badalona significa que
                               //se ha citado por hl7
                        if (exploracionOrigen == null)
                        {
                            return new HttpStatusCodeResult(HttpStatusCode.InternalServerError,
                               "Error.");
                        }
                        if (String.IsNullOrEmpty(exploracionOrigen.ARCHIVOBADALONA))
                        {
                            return new HttpStatusCodeResult(HttpStatusCode.InternalServerError,
                                "La exploración Origen no es válida");
                        }


                        //exploracion no tiene nombre de fichero de badalona porque se 
                        //ha citado manualmente
                        EXPLORACION exploracionDestino = ExploracionRepositorio.Obtener(oidDestino);

                        //a la exploracion destino le ponemos el archivo badalona de la otra
                        ExploracionRepositorio
                            .UpdateCampo("ARCHIVOBADALONA",
                                          exploracionOrigen.ARCHIVOBADALONA,
                                          exploracionDestino.OID);
                        //ExploracionRepositorio
                        //    .UpdateCampo("CID",
                        //                  exploracionOrigen.CID.Value.ToString(),
                        //                  exploracionDestino.OID, "int");
                        ExploracionRepositorio
                          .UpdateCampo("IDCITAONLINE",
                                        exploracionOrigen.IDCITAONLINE.ToString(),
                                        exploracionDestino.OID);
                        ExploracionRepositorio
                         .UpdateCampo("IDCITAONLINE",
                                       "Vinculada a " + exploracionDestino.OID,
                                       exploracionOrigen.OID);
                        //al paciente le ponemos el campo mailing4
                        PacienteRepositorio.UpdateCampo
                            ("MAILING4",
                                          exploracionOrigen.PACIENTE.MAILING4,
                                          exploracionDestino.IOR_PACIENTE);
                        ExploracionRepositorio
                            .UpdateCampo("NHCAP",
                                          exploracionOrigen.NHCAP,
                                          exploracionDestino.OID);
                        var historiaClinica = db.Historias.SingleOrDefault(h => h.IOR_PACIENTE == exploracionOrigen.PACIENTE.OID);
                        if (historiaClinica != null)
                        {
                            historiaClinica.IOR_PACIENTE = exploracionDestino.PACIENTE.OID;
                            db.Entry(historiaClinica).State = EntityState.Modified;
                            db.SaveChanges();
                        }
                        var texto = TextosRepositorio.Obtener(exploracionOrigen.OID);
                        if (texto != null)
                        {
                            texto.OWNER = exploracionDestino.OID;
                            texto.TEXTO = "Vinculada de " + exploracionOrigen.OID + " " + texto.TEXTO;
                            TextosRepositorio.InsertarOrUpdate(texto);

                        }
                        //SI EL PACIENTE ORIGEN SOLO TIENE UNA EXPLORACION, LO BORRAMOS
                        if (exploracionOrigen.PACIENTE.OID != exploracionDestino.PACIENTE.OID)
                        {
                            exploracionOrigen.PACIENTE.EXPLORACIONES = ListaDiaRepositorio.ObtenerPorPaciente(exploracionOrigen.PACIENTE.OID);
                            if (exploracionOrigen.PACIENTE.EXPLORACIONES.Count() == 1)
                            {
                                PacienteRepositorio.UpdateCampo("BORRADO", "T", exploracionOrigen.PACIENTE.OID);
                                PacienteRepositorio.UpdateCampo("COMENTARIO", exploracionOrigen.PACIENTE.COMENTARIO + " BORRADO POR VINCULO HL7", exploracionOrigen.PACIENTE.OID);
                            }
                        }

                        if (exploracionOrigen.ESTADO == "0")
                        {
                            //si la exploracion esta pendiente la borramos
                            ExploracionRepositorio
                           .UpdateCampo("ESTADO",
                                        "1",
                                         exploracionOrigen.OID);
                        }
                        if (exploracionDestino.INFORMADA == "T")
                        {
                            INFORMES oInforme = db.Informes.Single(p => p.OWNER == exploracionDestino.OID && p.VALIDACION == "T");
                            if (oInforme != null)
                            {
                                if (exploracionOrigen.IOR_ENTIDADPAGADORA== 11042)
                                {
                                    InformesRepositorio.GenerarFicherosBadalona(oInforme.OID);
                                }
                                if (exploracionOrigen.IOR_ENTIDADPAGADORA == 139)
                                {
                                    InformesRepositorio.GenerarFicherosFiatc(oInforme.OID);

                                }


                            }

                        }
                        break;

                    default:
                        break;
                }

            }
            catch (Exception ex)
            {

                return new HttpStatusCodeResult(HttpStatusCode.InternalServerError, ex.Message);
            }
            //Si la exploracion es de badalon


            return new HttpStatusCodeResult(HttpStatusCode.OK, "VINCULADO");
        }


        [HttpPost]
        public void ActualizarColegiado(int oidExploracion, int oidColegiado)
        {
            //1.- Cambiamos el colegiado de la exploracion
            ExploracionRepositorio.ActualizarColegiado(oidExploracion, oidColegiado);
        }

        [AutorizationAttribute]
        public ActionResult BusquedaAvanzada()
        {
            VWBusquedaAvanzada oFiltros = new VWBusquedaAvanzada();
            if (Session["FiltrosBusquedaAvanzada"] != null)
            {
                oFiltros = (VWBusquedaAvanzada)Session["FiltrosBusquedaAvanzada"];
                Session["FiltrosBusquedaAvanzada"] = oFiltros;
            }

            return View(oFiltros);
        }


        [HttpPost]
        [AutorizationAttribute]
        public ActionResult BusquedaAvanzada(VWBusquedaAvanzada oViewModel)
        {
            if (!ModelState.IsValid)
            {
                return new HttpStatusCodeResult(404, "Revise las condiciones de búsqueda");
            }
            if (oViewModel.INFORMADA != "A")
            {
                oViewModel.INFORMADA = (oViewModel.INFORMADA == "T" ? "true" : "false");
            }

            WebConfigRepositorio oConfig = new WebConfigRepositorio();
            ViewBag.CampoMutua = oConfig.ObtenerValor("ComboMutuas").ToUpper();
            oViewModel.Resultados = ExploracionRepositorio.BusquedaAvanzada(oViewModel);
            Session["FiltrosBusquedaAvanzada"] = oViewModel;


            return PartialView("_ListaBusquedaAvanzada", oViewModel.Resultados);
        }


        public ActionResult ListaExploracionesPaciente(int oid)
        {
            VMListaExploracionesPaciente oViewModel = new VMListaExploracionesPaciente();
            oViewModel.EXPLORACIONES = ListaDiaRepositorio.ObtenerPorPaciente(oid);
            var usuario = UsuariosRepositorio.Obtener(User.Identity.Name);
            oViewModel.ESPERFILMANRESA = (usuario.PERFIL == 202);
            oViewModel.VECANTIDADES = (usuario.VECANTIDADES == 1);

            return PartialView("_ExploracionesPaciente", oViewModel.EXPLORACIONES);
        }




        [HttpPost]
        public void CambiarColor(int cid, int oid)
        {
            FiltrosBusquedaExploracion oFiltros = null;
            if (Session["FiltrosBusqueda"] != null)
            {
                oFiltros = (FiltrosBusquedaExploracion)Session["FiltrosBusqueda"];
                oFiltros.oidExploracionSeleccionada = oid;
                Session["FiltrosBusqueda"] = oFiltros;
            }
            ExploracionRepositorio.CambiarCID(cid, oid);
        }
        //EL PARAMETRO CAMBIARREGIMEECONIMICO CTUAL ES PARA DIFERENCIAR CUANDO LLAMAMOS A ESTE METODO PARA UNA EXPLORACION A LA
        //QUE VAMOS A PASAR DE PRI A MUT O CUANOD LO LLAMOS DESDE LA FICHA DE EXPLORACION PARA NAVEGAR POR LAS EXPLORACIONES. 
        public ActionResult GetPanelExploracion(EXPLORACION oExploracion, bool cambiaRegimenEconomicoActual = true)
        {
            EXPLORACION oExploracionActual = ExploracionRepositorio.Obtener(oExploracion.OID);
            oExploracionActual.URLPREVIAEXPLORACION = oExploracion.URLPREVIAEXPLORACION;
            if (cambiaRegimenEconomicoActual)
            {

                //Si es una exploracion de mutua que pasa a privada
                if (oExploracion.ENTIDAD_PAGADORA.OWNER == 1)
                {
                    if (oExploracionActual.IOR_EMPRESA == null)
                    {
                        oExploracionActual.IOR_EMPRESA = 4;
                    }
                    var mutuaPrivada = db.Mutuas.Single(p => p.OWNER == 1 && p.IOR_EMPRESA == oExploracionActual.IOR_EMPRESA);
                    oExploracion.ENTIDAD_PAGADORA.OID = mutuaPrivada.OID;
                    oExploracion.IOR_ENTIDADPAGADORA = mutuaPrivada.OID;
                    oExploracion.IOR_APARATO = oExploracion.IOR_APARATO;
                    oExploracion.DAPARATO = DaparatoRepositorio.Obtener(oExploracion.IOR_APARATO.Value);
                    oExploracion.IOR_GRUPO = oExploracion.DAPARATO.CID;
                    oExploracion.FECHA = oExploracion.FECHA;
                    oExploracion.IOR_PACIENTE = oExploracion.IOR_PACIENTE;
                    oExploracion.PACIENTE = PacienteRepositorio.Obtener(oExploracion.IOR_PACIENTE);
                    oExploracion.HORA = oExploracion.HORA;

                    oExploracion.INFOMUTUA = InfoMutuasRepositorio.Obtener(mutuaPrivada.OID);
                    ExploracionRepositorio.InicializarExploracionDeApi(ref oExploracion);

                    oExploracion.ESTADODESCRIPCION = "PENDIENTE";
                    oExploracion.ESTADO = "0";
                }
                else
                {
                    oExploracion.IOR_ENTIDADPAGADORA = -1;
                }

                try
                {
                    oExploracion.CANTIDAD = float.Parse(GetPrecioExploracionConDescuento(oExploracion.IOR_TIPOEXPLORACION.Value, oExploracion.IOR_ENTIDADPAGADORA.Value, oExploracion.Q_ALFA));

                }
                catch (Exception)
                {

                    oExploracion.CANTIDAD = 0;
                }
                if (oExploracion.OID == 0)
                {
                    oExploracion.ESTADO = "2";
                    oExploracion.ESTADODESCRIPCION = "PENDIENTE";

                }
                else
                {
                    oExploracion.ESTADO = oExploracionActual.ESTADO;
                    oExploracion.ESTADODESCRIPCION = oExploracionActual.ESTADODESCRIPCION;
                    oExploracion.DAPARATO = DaparatoRepositorio.Obtener((int)oExploracion.IOR_APARATO);
                    oExploracion.IOR_ENTIDADPAGADORA = -1;
                    oExploracion.DIASEMANA = oExploracionActual.DIASEMANA;
                    oExploracion.MONEDA = oExploracionActual.MONEDA;
                }


            }
            else
            {
                oExploracion = oExploracionActual;
                oExploracion.LOGUSUARIOS = LogUsuariosRepositorio.Obtener(oExploracion.OID);
                oExploracion.LOGVIDSIGNER = LogVidSignerRepositorio.Obtener(oExploracion.OID);
                oExploracion.EXPLORAMISMODIA = ListaDiaRepositorio.ObtenerPorPacienteYFecha(oExploracion.OID, oExploracion.PACIENTE.OID, (DateTime)oExploracion.FECHA);
            }


            return PartialView("PartialMutuasNew", oExploracion);

        }

        public string GetPrecioExploracionConDescuento(int IOR_TIPOEXPLORACION, int IOR_MUTUA, string Q_ALFA)
        {
            string Result = "0";
            int descuento = 0;
            var qalfa = db.DESCUENTOS.Where(p => p.CODIGO == Q_ALFA).FirstOrDefault();
            if (qalfa != null)
            {
                descuento = qalfa.DESCUENTO;
            }
            if (IOR_TIPOEXPLORACION < 0)
            {
                return "NO TIPO EXPLORACION";
            }
            MUTUAS oMutua = MutuasRepositorio.Obtener(IOR_MUTUA);
            if (oMutua.IOR_CENTRAL != null && oMutua.IOR_CENTRAL > 0)
            {
                Result = TarifasRepositorio.ObtenerPrecioExploracion(IOR_TIPOEXPLORACION, (int)oMutua.IOR_CENTRAL);
            }
            else
            {
                Result = TarifasRepositorio.ObtenerPrecioExploracion(IOR_TIPOEXPLORACION, oMutua.OID);
            }

            double CANTIDAD = double.Parse(
                 GetPrecioExploracion(IOR_TIPOEXPLORACION, IOR_MUTUA));

            CANTIDAD = CANTIDAD - ((CANTIDAD * descuento) / 100);
            return CANTIDAD.ToString();
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
            if (String.IsNullOrEmpty(Result))
            {
                Result = "0";
            }
            return Result;
        }

        public JsonResult GetTipoExploraciones(int IOR_APARATO, int IOR_MUTUA)
        {
            List<APARATOS> Result = new List<APARATOS>();
            MUTUAS oMutua = MutuasRepositorio.Obtener(IOR_MUTUA);
            if (oMutua.IOR_CENTRAL != null && oMutua.IOR_CENTRAL > 0)
            {
                Result = TarifasRepositorio.ObtenerPorAparatoYMutua((int)IOR_APARATO, (int)oMutua.IOR_CENTRAL);
            }
            else
            {
                Result = TarifasRepositorio.ObtenerPorAparatoYMutua((int)IOR_APARATO, oMutua.OID);
            }
            return Json(Result, JsonRequestBehavior.AllowGet);
        }

        [OutputCache(NoStore = true, Duration = 0, VaryByParam = "None")]
        public string GetTextoExploraciones(int oidAparato)
        {
            string TextoAparato = AparatoRepositorio.Obtener(oidAparato).TEXTO;
            string html = "";
            if (!String.IsNullOrEmpty(TextoAparato))
            {
                html = TextoAparato;
            }
            return html;
        }

        [HttpPost]
        public ActionResult EditarTexto(string name, int pk, string value)
        {
            EXPLORACION oexploracion = ExploracionRepositorio.Obtener(pk);
            TEXTOS oTexto = TextosRepositorio.Obtener(pk);

            oTexto.TEXTO = value;
            if (oTexto.OWNER == null)
            {
                oTexto.OWNER = pk;
            }
            TextosRepositorio.InsertarOrUpdate(oTexto);
            USUARIO usuarioAutenticated = (USUARIO)System.Web.HttpContext.Current.Session["Usuario"];

            LOGUSUARIOS oLogTecnico = new LOGUSUARIOS
            {
                OWNER = oexploracion.OID,
                FECHA = DateTime.Now.ToString("dd/MM/yyyy HH:mm"),
                TEXTO = "Modifica Aviso",
                USUARIO = usuarioAutenticated.LOGIN,
                DATA = DateTime.Now.ToString("dd/MM/yyyy"),
                COD_FIL = oexploracion.APARATO.FIL,
                MUTUA = oexploracion.ENTIDAD_PAGADORA.CODMUT
            };

            LogUsuariosRepositorio.Insertar(oLogTecnico);
            ExploracionRepositorio.UpdateCampo("USERMOD", usuarioAutenticated.NOME, oexploracion.OID);
            ExploracionRepositorio.UpdateCampo("HORAMOD", DateTime.Now.ToString("dd/MM/yyyy HH:mm").QuotedString(), oexploracion.OID);
            return new HttpStatusCodeResult(200);

        }

        [HttpPost]
        public ActionResult Desvincular(int oid)
        {
            ExploracionRepositorio.UpdateCampo("IOR_MASTER", "-1", oid, "integer");
            ResultadoRequest oResult;
            oResult = new ResultadoRequest()
            {
                Mensaje = "Se ha desvinculado la Exploración correctamente",
                Resultado = ResultadoRequest.RESULTADO.SUCCESS
            };
            return PartialView("_ResultRequest", oResult);
        }

        public ActionResult DesvincularHija(int oid)
        {
            EXPLORACION oExploracionHija = ExploracionRepositorio.Obtener(oid);
            ViewBag.oidHija = oExploracionHija.OID;
            return PartialView("_DesvincularExploracion", ExploracionRepositorio.Obtener(oExploracionHija.IOR_MASTER.Value));
        }

        public ActionResult Hijas(int oid)
        {
            List<EXPLORACION> oListaExploraciones = ExploracionRepositorio.ObtenerHijas(oid);
            oListaExploraciones.Add(ExploracionRepositorio.Obtener(oid));

            return PartialView("_ModalHijas", oListaExploraciones.OrderBy(p => p.IOR_MASTER));
        }

        public ActionResult ListaBadalonaPendienteCitar(int oid)
        {
            EXPLORACION oexplo = ExploracionRepositorio.Obtener(oid);
            List<LISTADIA> oListaExploraciones = ListaDiaRepositorio.Lista(oexplo.IOR_ENTIDADPAGADORA.Value, "0",oexplo.USERNAME);
            ViewBag.oidDestino = oid;
           
            ViewBag.header = oexplo.PACIENTE.PACIENTE1 + " " + oexplo.FECHA.Value.ToShortDateString() + " " + oexplo.HORA;
            return PartialView("_ListaBadalona", oListaExploraciones);
        }


        public ActionResult ListaMini()
        {

            List<LISTADIA> oListaExploraciones = (List<LISTADIA>)Session["ListaExploraciones"];
            WebConfigRepositorio oConfig = new WebConfigRepositorio();
            ViewBag.CampoMutua = oConfig.ObtenerValor("ComboMutuas").ToUpper();
            return PartialView("_ListaDiaMini", oListaExploraciones);
        }



        [ConfiguracionVisualAttribute]
        public FileResult ImprimirLista()
        {

            ViewData["oUsuario"] = System.Web.HttpContext.Current.Session["Usuario"];

            IEnumerable<LISTADIAAMBFORATS> oResult;

            WebConfigRepositorio oConfig = new WebConfigRepositorio();
            FiltrosBusquedaExploracion oFiltros = new FiltrosBusquedaExploracion();


            string RutaDOCS = oConfig.ObtenerValor("RUTADOCS");

            oFiltros.oidCentro = (ViewData["HOME.IOR_CENTRO"] != null ? (int)ViewData["HOME.IOR_CENTRO"] : -1);
            oFiltros.Fecha = (ViewData["HOME.FECHA"] != null ? ViewData["HOME.FECHA"].ToString() : DateTime.Now.ToString("dd/MM/yyyy"));
            oFiltros.Borrados = (ViewData["HOME.ESTADO"] != null ? "T" : "F");
            oFiltros.oidAparato = (ViewData["HOME.IOR_APARATO"] != null ? (int)ViewData["HOME.IOR_APARATO"] : -1);
            //A nivel de centro definimos el logo a utilizar en las cabeceras
            CENTROS oCentro = db.Centros.Single(c => c.OID == oFiltros.oidCentro);
            //Generamos el HTML de ejemplo 
            string rutaListaTemplate = RutaDOCS + @"\RWLISTADIA_" + DateTime.Now.ToString("hhmmss") + ".html";
            string rutaLista = RutaDOCS + @"\RWLISTADIA_" + DateTime.Now.ToString("hhmmss") + ".pdf";

            StreamWriter Swr = new StreamWriter(rutaListaTemplate);
            oResult = ListaDiaAmbForatsRepositorio.Get(oFiltros);
            foreach (var item in oResult)
            {
                item.PACIENTEOBJECT = PacienteRepositorio.Obtener(item.IOR_PACIENTE);
            }

            Swr.Write(this.RenderView("_ListaDiaPrint", oResult));
            Swr.Close();
            Swr.Dispose();
            byte[] aPDF = Models.HTMLToPDF.Converter.InformeFromHtml(rutaListaTemplate, oCentro.LOGO_URL, 100, 2000, true);

            return File(aPDF, "application/pdf");


        }

        ////método utilizado por x-editable para editar campos sueltos
        [HttpPost]
        public ActionResult EditarCampo(string name, int pk, string value)
        {
            if (name == "CABINF_EXPLO" && string.IsNullOrEmpty(value))
            {
                value = "F";
            }
            ExploracionRepositorio.UpdateCampo(name, value, pk);
            return new HttpStatusCodeResult(200);
        }

        public ActionResult Intocable(int oid)
        {
            //1.- Cambiamos el colegiado de la exploracion
            ExploracionRepositorio.CambiarBloqueoFicha(oid);
            return RedirectToAction("Details", "Exploracion", new
            {
                oid = oid
            });
        }

        [ConfiguracionVisualAttribute]
        public ActionResult Details(int oid, string ReturnUrl = "")
        {

            EXPLORACION oExplo = ExploracionRepositorio.Obtener(oid);
            oExplo.LOGUSUARIOS = LogUsuariosRepositorio.Obtener(oExplo.OID);
            oExplo.LOGVIDSIGNER = LogVidSignerRepositorio.Obtener(oExplo.OID);
            oExplo.EXPLORAMISMODIA = ListaDiaRepositorio.ObtenerPorPacienteYFecha(oExplo.OID, oExplo.PACIENTE.OID, (DateTime)oExplo.FECHA);
            USUARIO oUser = UsuariosRepositorio.Obtener(User.Identity.Name);
            _Filtrosrepository.Guardar(oUser.IDUSER, "HOME", "OIDACTIVA", oid.ToString());

            if (db.Refractometros.Count(r => r.VERS == 1) > 0)
            {
                oExplo.DOCUMENTOSIMPRIMIBLES = db.Refractometros.Where(r => r.VERS == 1).ToList();
            }


            bool saveSuccess = false;
            if (Request.Cookies["ColegiadoSaveSuccess"] != null)
            {
                Response.SetCookie(new HttpCookie("ColegiadoSaveSuccess", "") { Expires = DateTime.Now.AddDays(-1) });
                saveSuccess = true;
            }
            ViewBag.SaveColegiado = saveSuccess;
            oExplo.URLPREVIAEXPLORACION = ReturnUrl;

            ViewBag.EstadoPanelPaciente = (ViewData["EXPLORACION.DATOSPERSONALES"] != null ? (int)ViewData["EXPLORACION.DATOSPERSONALES"] : 1);
            return View(oExplo);
        }

        [HttpPost]
        public ActionResult Details(EXPLORACION oExploracion)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.SaveColegiado = false;
                oExploracion.ENTIDAD_PAGADORA = MutuasRepositorio.Obtener(oExploracion.IOR_ENTIDADPAGADORA.Value);
                oExploracion.LOGUSUARIOS = LogUsuariosRepositorio.Obtener(oExploracion.OID);
                oExploracion.LOGVIDSIGNER = LogVidSignerRepositorio.Obtener(oExploracion.OID);
                oExploracion.EXPLORAMISMODIA = ListaDiaRepositorio.ObtenerPorPacienteYFecha(oExploracion.OID, oExploracion.IOR_PACIENTE, (DateTime)oExploracion.FECHA);
                oExploracion.PACIENTE = PacienteRepositorio.Obtener(oExploracion.IOR_PACIENTE);
                oExploracion.COLEGIADO = ColegiadoRepositorio.Obtener(oExploracion.IOR_COLEGIADO.HasValue ? oExploracion.IOR_COLEGIADO.Value : -1);
                oExploracion.CENTROEXTERNO = CentrosExternosRepositorio.Obtener(oExploracion.IOR_CENTROEXTERNO.HasValue ? oExploracion.IOR_CENTROEXTERNO.Value : -1);

                if (db.Refractometros.Count(r => r.VERS == 1) > 0)
                {
                    oExploracion.DOCUMENTOSIMPRIMIBLES = db.Refractometros.Where(r => r.VERS == 1).ToList();
                }
                return View("Details", oExploracion);
            }


            try
            {


                int oid = ExploracionRepositorio.Update(oExploracion);

                if (oExploracion.OTRASEXPLORACIONESRECOGIDAS != null)
                {
                    EXPLORACION oExploEnBd = ExploracionRepositorio.Obtener(oid);
                    foreach (string oidExploracion in oExploracion.OTRASEXPLORACIONESRECOGIDAS.Split('-'))
                    {
                        if (oidExploracion.Length > 0)
                        {
                            ExploracionRepositorio.UpdateCampo("RECOGIDO",oExploracion.RECOGIDO, int.Parse(oidExploracion));
                            if (oExploracion.RECOGIDO != "F")
                            {
                                if (!oExploEnBd.FECHADERIVACION.HasValue)
                                {
                                    ExploracionRepositorio.UpdateCampo("FECHADERIVACION", DateTime.Now.ToString("MM/dd/yyyy"), int.Parse(oidExploracion));

                                }
                              
                            }
                            else
                            {
                                if (oExploEnBd.FECHADERIVACION.HasValue)
                                {
                                    ExploracionRepositorio.UpdateCampo("FECHADERIVACION","null", int.Parse(oidExploracion));

                                }
                              
                            }
                            if (!oExploracion.FECHADERIVACION.HasValue)
                            {
                                ExploracionRepositorio.UpdateCampo("FECHADERIVACION", DateTime.Now.ToString("MM/dd/yyyy"), int.Parse(oidExploracion));                              

                            }
                       

                        }
                    }
                }

                return new HttpStatusCodeResult(HttpStatusCode.OK, oExploracion.CANTIDAD.ToString());

            }
            catch (Exception ex)
            {
                return new HttpStatusCodeResult(HttpStatusCode.InternalServerError, ex.Message);
            }

        }

        public ActionResult Details2(int oid)
        {
            //Si hay filtros de busqueda aplicados en la session agregamos la propiedad oidExploracionSeleccionada
            //Esto sirve para cuando volvamos al listadia poner la actual en verde
            if (Session["FiltrosBusqueda"] != null)
            {
                FiltrosBusquedaExploracion oFiltros = (FiltrosBusquedaExploracion)Session["FiltrosBusqueda"];
                oFiltros.oidExploracionSeleccionada = oid;
                Session["FiltrosBusqueda"] = oFiltros;
            }
            EXPLORACION oExplo = ExploracionRepositorio.Obtener(oid);
            ViewBag.Idiomas = DataBase.Idiomas();
            VWExploracion oModel = new VWExploracion();
            oModel.PACIENTE = oExplo.PACIENTE;
            //oModel.ESTADO = oExplo.ESTADO;
            oModel.EXPLORACION = oExplo;

            return View("Details2", oModel);
        }


        public ActionResult AddPaso1(VMAddPaso1 oPacienteViewModel)
        {
            var item = new FiltrosBusquedaPaciente { Nombre = "A" };
            if (Session["FiltrosBusquedaPaciente"] != null)
            {
                item = Session["FiltrosBusquedaPaciente"] as FiltrosBusquedaPaciente;
            }
            for (int i = 0; i < oPacienteViewModel.HUECOS.Count; i++)
            {
                oPacienteViewModel.HUECOS[i].DAPARATO = DaparatoRepositorio.Obtener(oPacienteViewModel.HUECOS[i].IOR_APARATO);
            }
            oPacienteViewModel.IOR_PACIENTE = 0;
            if (oPacienteViewModel.HUECOS.First().IOR_BOLSA > 0)
            {
                int ior_bolsa = oPacienteViewModel.HUECOS.First().IOR_BOLSA;
                BOLSA_PRUEBAS otemp = db.BolsaPruebas.Single(p => p.OID == ior_bolsa);
                oPacienteViewModel.PACIENTE1 = otemp.PACIENTE.Replace(", ", "%").Trim();
                oPacienteViewModel.IOR_BOLSA = ior_bolsa;
                oPacienteViewModel.TELEFONO1 = otemp.TELEFONO1;
            }
            else
            {
                oPacienteViewModel.IOR_BOLSA = -1;
            }
            return View("AddPaso1", oPacienteViewModel);
        }


        [HttpPost]
        public ActionResult AddPaso1(FormCollection form)
        {
            string paciente = Request.Form["pacienteFilter"];
            string dni = Request.Form["dniFilter"];
            string telefono = Request.Form["telefonoFilter"];
            WebConfigRepositorio oConfig = new WebConfigRepositorio();
            ViewBag.CampoMutua = oConfig.ObtenerValor("ComboMutuas").ToUpper();
            var item = new FiltrosBusquedaPaciente { Nombre = paciente, Dni = dni, Telefono = telefono };
            return PartialView("_ListaPacientesAlta", Models.Repos.PacienteRepositorio.Lista(item, 25));
        }

        public ActionResult AddPaso2(VMAddPaso1 oPacienteViewModel)
        {
            PACIENTE oPaciente;
            if (oPacienteViewModel.IOR_PACIENTE == 0)
            {
                oPaciente = new PACIENTE();

                oPaciente.TELEFONOS.Add(new TELEFONO());
                if (oPacienteViewModel.IOR_BOLSA > 0)
                {
                    BOLSA_PRUEBAS otemp = db.BolsaPruebas.Single(p => p.OID == oPacienteViewModel.IOR_BOLSA);
                    oPaciente.TELEFONOS.First().NUMERO = otemp.TELEFONO1;
                    oPaciente.FECHAN = otemp.FECHANACIMIENTO;
                    oPaciente.EMAIL = otemp.MAIL;
                    oPaciente.PACIENTE1 = otemp.PACIENTE;
                    oPaciente.SEXO = otemp.SEXO;
                    oPaciente.DNI = otemp.DNI;

                }
                oPaciente.OTROS4 = Utils.DataBase.DefaultIdioma();
                oPaciente.TRAC = 1;
                if (!String.IsNullOrEmpty(oPacienteViewModel.PACIENTE1))
                {

                    oPaciente.PACIENTE1 = oPacienteViewModel.PACIENTE1.ToUpper();
                }
            }
            else
            {
                oPaciente = PacienteRepositorio.Obtener(oPacienteViewModel.IOR_PACIENTE);
            }
            VMAddPaso2 oviewModel = new VMAddPaso2();
            oviewModel.HUECOS = oPacienteViewModel.HUECOS;
            oviewModel.IOR_BOLSA = oPacienteViewModel.IOR_BOLSA;
            if (oviewModel.IOR_BOLSA > 0 && oPacienteViewModel.IOR_PACIENTE==0)
            {
                var peticion = db.BolsaPruebas.Single(p => p.OID == oPacienteViewModel.IOR_BOLSA);
                if (peticion!=null)
                {
                    oPaciente.CID = peticion.IOR_ENTIDADPAGADORA;

                }
                else
                {
                    oPaciente.CID =-1;

                }

            }
            oviewModel.PACIENTEALTA = oPaciente;
            return View("AddPaso2", oviewModel);
        }


        public ActionResult AddPaso3(VMAddPaso2 oPacienteViewModel)
        {
            PACIENTE oPaciente = oPacienteViewModel.PACIENTEALTA;
            
            if (!ModelState.IsValid)
            {
                PACIENTE pacienteLleno = PacienteRepositorio.Obtener(oPacienteViewModel.PACIENTEALTA.OID);

                oPacienteViewModel.PACIENTEALTA = pacienteLleno;
                return View("AddPaso2", oPacienteViewModel);

                //return RedirectToAction("AddPaso2", oviewModel);
            }


            if (oPaciente.OID == 0)
            {
                oPaciente.OID = PacienteRepositorio.Insertar(oPaciente);
            }
            else
            {
                PacienteRepositorio.Update(oPaciente);
            }

            List<EXPLORACION> listaAcrear = new List<EXPLORACION>();

            //Si hay filtros de busqueda aplicados en la session agregamos la propiedad oidExploracionSeleccionada
            //Esto sirve para cuando volvamos al listadia poner la actual en verde
            foreach (var item in oPacienteViewModel.HUECOS)
            {
                EXPLORACION oExploracion = new EXPLORACION();
                oExploracion.IOR_APARATO = item.IOR_APARATO;
                oExploracion.DAPARATO = DaparatoRepositorio.Obtener(item.IOR_APARATO);
                oExploracion.IOR_GRUPO = oExploracion.DAPARATO.CID;
                oExploracion.FECHA = DateTime.Parse(item.FECHA);
                oExploracion.IOR_PACIENTE = oPaciente.OID;
                oExploracion.PACIENTE = PacienteRepositorio.Obtener(oPaciente.OID);
                oExploracion.HORA = item.HORA;
                oExploracion.ENTIDAD_PAGADORA = MutuasRepositorio.Obtener((int)oExploracion.PACIENTE.CID);
                oExploracion.INFOMUTUA = InfoMutuasRepositorio.Obtener((int)oExploracion.PACIENTE.CID);
                ExploracionRepositorio.InicializarExploracionDeApi(ref oExploracion);
                oExploracion.IOR_ENTIDADPAGADORA = oPaciente.CID;
                oExploracion.ESTADODESCRIPCION = "PENDIENTE";
                oExploracion.ESTADO = "0";
                if (oPacienteViewModel.IOR_BOLSA > 0)
                {
                    BOLSA_PRUEBAS otemp = db.BolsaPruebas.Single(p => p.OID == oPacienteViewModel.IOR_BOLSA);
                    oExploracion.FECHAMAXENTREGA = otemp.FECHARESULTADO;
                    oExploracion.IOR_PETICIONPRUEBAORIGEN = oPacienteViewModel.IOR_BOLSA;
                    oExploracion.TEXTO = otemp.COMENTARIO + "\n" + otemp.TEXTO;
                    oExploracion.IOR_COLEGIADO = otemp.IOR_COLEGIADO;
                    oExploracion.COLEGIADO = ColegiadoRepositorio.Obtener(oExploracion.IOR_COLEGIADO.Value);
                    if (otemp.FECHARESULTADO.HasValue)
                    {
                        oExploracion.FECHAMAXENTREGA = otemp.FECHARESULTADO;

                    }
                    //APARATOS oAparato
                    oExploracion.IOR_ENTIDADPAGADORA = otemp.IOR_ENTIDADPAGADORA;
                    oExploracion.ENTIDAD_PAGADORA = MutuasRepositorio.Obtener(otemp.IOR_ENTIDADPAGADORA ?? -1);
                    oExploracion.INFOMUTUA = InfoMutuasRepositorio.Obtener(otemp.IOR_ENTIDADPAGADORA ?? -1);
                    oExploracion.IOR_CENTROEXTERNO = otemp.IOR_CENTROEXTERNO;
                    oExploracion.IOR_TIPOEXPLORACION = otemp.IOR_TIPOEXPLORACION;
                    oExploracion.CANTIDAD = double.Parse(GetPrecioExploracion(oExploracion.IOR_TIPOEXPLORACION.Value, oExploracion.IOR_ENTIDADPAGADORA.Value));
                }
                listaAcrear.Add(oExploracion);
            }

            return View("AddPaso3", listaAcrear);
        }


        [HttpPost]
        public ActionResult GuardarPaso3(EXPLORACION oExploracion)
        {
            if (!ModelState.IsValid)
            {
                oExploracion.DAPARATO = DaparatoRepositorio.Obtener(oExploracion.IOR_APARATO.Value);
                oExploracion.IOR_GRUPO = oExploracion.DAPARATO.CID;
                oExploracion.PACIENTE = PacienteRepositorio.Obtener(oExploracion.IOR_PACIENTE);
                oExploracion.ENTIDAD_PAGADORA = MutuasRepositorio.Obtener(oExploracion.IOR_ENTIDADPAGADORA.Value);
                oExploracion.INFOMUTUA = InfoMutuasRepositorio.Obtener((int)oExploracion.PACIENTE.CID);
                ExploracionRepositorio.InicializarExploracionDeApi(ref oExploracion);
                return View("AddPaso3", oExploracion);
            }
            oExploracion.DAPARATO = DaparatoRepositorio.Obtener((int)oExploracion.IOR_APARATO);
            oExploracion.IOR_GRUPO = oExploracion.DAPARATO.OWNER;
            oExploracion.OWNER = oExploracion.DAPARATO.CID;
            // oExploracion.FECHA = DateTime.Parse(oExploracion.Fecha);
            PACIENTE oPaciente = PacienteRepositorio.Obtener((int)oExploracion.IOR_PACIENTE);
            if (oPaciente.CID != oExploracion.IOR_ENTIDADPAGADORA)
            {
                oPaciente.CID = oExploracion.IOR_ENTIDADPAGADORA;
                PacienteRepositorio.Update(oPaciente);
            }
            oExploracion.ENTIDAD_PAGADORA = MutuasRepositorio.Obtener((int)oExploracion.IOR_ENTIDADPAGADORA);

            ExploracionRepositorio.InicializarExploracionDeApi(ref oExploracion);


            //EXPLORACION oResult = ExploracionRepositorio.Obtener();
            int oid = ExploracionRepositorio.Insertar(oExploracion);
            if (oExploracion.IOR_PETICIONPRUEBAORIGEN.HasValue)
            {
                BOLSA_PRUEBAS otemp = db.BolsaPruebas.Single(p => p.OID == oExploracion.IOR_PETICIONPRUEBAORIGEN);
                otemp.IOR_EXPLORACION = oid;
                otemp.NOMBRE = otemp.PACIENTE.Split(',')[1];
                otemp.APELLIDOS = otemp.PACIENTE.Split(',')[0];
                db.Entry(otemp).State = EntityState.Modified;
                db.SaveChanges();
                if (otemp.IOR_DOCUMENTO>0)
                {
                    IMAGENES oImagen = db.Imagenes.SingleOrDefault(p => p.OID == otemp.IOR_DOCUMENTO);
                    oImagen.IOR_EXPLORACION = oid;
                    oImagen.IOR_PACIENTE = oExploracion.IOR_PACIENTE;
                    db.Entry(oImagen).State = EntityState.Modified;
                    db.SaveChanges();

                }


            }
            USUARIO oUser = UsuariosRepositorio.Obtener(User.Identity.Name);
            _Filtrosrepository.Guardar(oUser.IDUSER, "HOME", "OIDACTIVA", oid.ToString());
            return RedirectToAction("Index", "Home");
        }

        public ActionResult Duplicar(int oid, string hora, int ioraparato, int relacionada = -1, string fecha = "01/01/1990")
        {

            EXPLORACION oExploracionOrigen = ExploracionRepositorio.Obtener(oid);
            EXPLORACION oExploracionDuplicada = oExploracionOrigen;
            oExploracionDuplicada.HORA = hora;
            oExploracionDuplicada.IOR_TIPOEXPLORACION = -1;
            oExploracionDuplicada.CANTIDAD = 0;
            oExploracionDuplicada.IOR_APARATO = ioraparato;
            oExploracionDuplicada.APARATO = null;
            oExploracionDuplicada.PAGADO = "F";
            oExploracionDuplicada.FACTURADA = "F";
            oExploracionDuplicada.IOR_FACTURA = -1;


            ExploracionRepositorio.InicializarExploracionDeApi(ref oExploracionDuplicada);

            if (relacionada > 0)
            {
                oExploracionDuplicada.IOR_MASTER = relacionada;
                oExploracionDuplicada.FECHA = DateTime.Parse(fecha);
                oExploracionDuplicada.RECOGIDO = "F";
                oExploracionDuplicada.FECHADERIVACION = null;
                oExploracionDuplicada.FECHAMAXENTREGA = null;
                oExploracionDuplicada.ESTADO = "0";
                oExploracionDuplicada.IDCITAONLINE = "";
                ViewBag.EXPLORACIONRELACIONADA = ExploracionRepositorio.Obtener(oid);
            }
            oExploracionDuplicada.ESTADODESCRIPCION = "PENDIENTE";
            oExploracionDuplicada.INTOCABLE = "F";
            //oExploracionDuplicada.OID=   ExploracionRepositorio.Insertar(oExploracionDuplicada);
            //Si hay filtros de busqueda aplicados en la session agregamos la propiedad oidExploracionSeleccionada
            //Esto sirve para cuando volvamos al listadia poner la actual en verde

            oExploracionDuplicada.DAPARATO = DaparatoRepositorio.Obtener(ioraparato);
            oExploracionDuplicada.INFOMUTUA = InfoMutuasRepositorio.Obtener((int)oExploracionDuplicada.PACIENTE.CID);
            if (fecha != "01/01/1990")
            {
                oExploracionDuplicada.FECHA = DateTime.Parse(fecha);
            }
            List<EXPLORACION> listaAcrear = new List<EXPLORACION>();
            listaAcrear.Add(oExploracionDuplicada);
            return View("AddPaso3", listaAcrear);
        }

        public FileResult ImprimirRecordatorioCita(int oid)
        {

            WebConfigRepositorio oConfig = new WebConfigRepositorio();


            ReportDocument rd = new ReportDocument();
            //rd.Load(Path.Combine(Server.MapPath("~/Content/Reports"), oConfig.ObtenerValor("RecordatorioCita").ToUpper()));
            rd.Load(Path.Combine(Server.MapPath("~/Content/Reports/Crystal/"), "RecordatorioCita.rpt"));
            string camposQueryStandard = "select * from listadia where oid=" + oid;
            var oResult = db.Database.SqlQuery<LISTADIA2>(camposQueryStandard).ToList<LISTADIA2>();
            DataTable dt = ToDataTable<LISTADIA2>(oResult);


            rd.SetDataSource(dt);
            //rd.SetParameterValue("FECHAPARAM", fechaArqueo);
            Response.Buffer = false;
            Response.ClearContent();
            Response.ClearHeaders();
            Stream stream = rd.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
            return File(stream, "application/pdf");
        }

        private void ImprimirFastReport(EXPLORACION oExplo,bool recordatoriocita=false)
        {
            List<VWImprimirFicha> oFichaImprimir = new List<VWImprimirFicha>();
            oFichaImprimir = ExploracionRepositorio.ImprimirFichaPri(oExplo.OID);
            var oDocumento = db.Vid_Documentos.Where(d => d.OWNER == oExplo.OID && !d.NOMBRE.Contains("LOPD")).FirstOrDefault();
            if (oDocumento != null)
            {

                oFichaImprimir.First().RESPUESTAS = db.Vid_Respuestas.Where(r => r.OWNER == oDocumento.OID).ToList();
            }
            else
            {
                oFichaImprimir.First().RESPUESTAS = new List<VID_RESPUESTAS>();
            }
            WebReport webReport = new WebReport();
            string reportPath = "";
            if (!recordatoriocita)
            {
                reportPath=Path.Combine(Server.MapPath("~/Reports"), "Exploracion.frx");

            }
            else
            {
                reportPath= Path.Combine(Server.MapPath("~/Reports"), "ExploracionRecordatorio.frx");

            }
            // create report instance
            Report report = new Report();

            // load the existing report
            webReport.Report.Load(reportPath);

            if (!recordatoriocita)
            {
                //exploraciones del tipo Mamografia
                if (oExplo.APARATO.OWNER == 14 || (oExplo.APARATO.OWNER == 22) || (oExplo.IOR_TIPOEXPLORACION == 144)
                    || (oExplo.IOR_TIPOEXPLORACION == 702) || (oExplo.IOR_TIPOEXPLORACION == 319) || (oExplo.IOR_TIPOEXPLORACION == 877)
                    || (oExplo.IOR_TIPOEXPLORACION == 149) || (oExplo.IOR_TIPOEXPLORACION == 707) || (oExplo.IOR_TIPOEXPLORACION == 445)
                    || (oExplo.IOR_TIPOEXPLORACION == 465))
                {
                    webReport.Report.FindObject("txtHideMamo").Delete();
                    FastReport.TextObject textoRecogida = (FastReport.TextObject)webReport.Report.FindObject("txtRecogida");
                    textoRecogida.Text = "A partir del dia.................................desde las 18 h.";
                }
                else
                {
                    FastReport.TextObject textoRecogida = (FastReport.TextObject)webReport.Report.FindObject("txtHideMamo");
                    CENTROS oCentro = CentrosRepositorio.Obtener(oExplo.OWNER.Value);
                    textoRecogida.Text = textoRecogida.Text.Replace("@horario", oCentro.HORARIO);
                    webReport.Report.FindObject("Picture2").Delete();
                }
                string centro = "";

                FastReport.TextObject textoOid = (FastReport.TextObject)webReport.Report.FindObject("Text15");
                textoOid.Text = centro + textoOid.Text;
            }
            else
            {
                FastReport.TextObject textoIndicaciones = (FastReport.TextObject)webReport.Report.FindObject("txtIndicaciones");
                textoIndicaciones.Text = AparatoRepositorio.ObtenerTextoAparato(oExplo.IOR_TIPOEXPLORACION.Value);
                FastReport.TextObject txtCentro = (FastReport.TextObject)webReport.Report.FindObject("txtCentro");
                txtCentro.Text = "Centro:" + CentrosRepositorio.Obtener(oExplo.OWNER.Value).NOMBRE + "(" + CentrosRepositorio.Obtener(oExplo.OWNER.Value).DIRECCION + ")";

            }
           
            // register the business object
            webReport.RegisterData(oFichaImprimir, "EXPLORACION");
            ViewBag.WebReport = webReport;
        }

        public ActionResult Imprimir(int oid, int oidTipoDocumento = -1,bool recordatorioCita=false)
        {
            EXPLORACION oExplo = ExploracionRepositorio.Obtener(oid);
            if (oExplo.EMPRESA.NOMBRE.Contains("FORASTÉ"))
            {
                ImprimirFastReport(oExplo, recordatorioCita);
                return View("Report");

            }
            else
            {

                return RedirectToAction("ImprimirDelfos", "Exploracion", new
                {
                    oid = oid,
                    oidtipoDocumento = oidTipoDocumento
                });
            }

        }


        //imprime la hoja de trabajo del grupo Irla o la hoja de recogida
        //public FileContentResult ImprimirDelfos(int oid, bool esRecogida = false)
        //el parementro oidtipodocumento esta en la tabla REFRACTOMETROS

        //imprime la hoja de trabajo del grupo Irla o la hoja de recogida
        //public FileContentResult ImprimirDelfos(int oid, bool esRecogida = false)
        //el parementro oidtipodocumento esta en la tabla REFRACTOMETROS
        public FileContentResult ImprimirDelfos(int oid, int oidtipoDocumento)
        {
            List<VWImprimirFicha> oFichaImprimir = new List<VWImprimirFicha>();

            EXPLORACION oExplo = ExploracionRepositorio.Obtener(oid);
            oFichaImprimir = ExploracionRepositorio.ImprimirFichaPri(oid, false);
            REFRACTOMETROS tipoDocumento = db.Refractometros.Single(p => p.OID == oidtipoDocumento);

            WebConfigRepositorio oConfig = new WebConfigRepositorio();
            string RutaDOCS = oConfig.ObtenerValor("RUTADOCS");
            //Generamos el HTML de ejemplo de la factura para poder combinarlo con el modelo de datos
            string rutaFichaTemplate = RutaDOCS + @"\RWEXPLORACION_" + oFichaImprimir.First().IOR_PACIENTE + ".html";
            string rutaFicha = RutaDOCS + @"\RWEXPLORACION_" + oFichaImprimir.First().IOR_PACIENTE + ".pdf";
            StreamWriter Swr = new StreamWriter(rutaFichaTemplate);
            if (oExplo.GAPARATO.COD_GRUP == "PET" && oidtipoDocumento == 19)
            {
                tipoDocumento.BAUDIOS = "ImprimirDelfosPET";
            }
            Swr.Write(this.RenderView(tipoDocumento.BAUDIOS, oFichaImprimir.First()));

            Swr.Close();
            Swr.Dispose();
            CENTROS oCentro = db.Centros.Single(c => c.OID == oExplo.OWNER);
            byte[] aPDF = Models.HTMLToPDF.Converter.InformeFromHtml(
                rutaFichaTemplate,
                oCentro.LOGO_URL,
                500,
                1000);
            string documentoTemp = Utils.Varios.ObtenerCarpetaDocumentosEscaneados() + DateTime.Now.ToString("ddMMyyyyhhmmss") + ".pdf";
            //almacenamos el documento en la tabla imágenes
            if (tipoDocumento.GUARDAR == "T")
            {

                //FileInfo oFileInfo = new FileInfo(documentoTemp);
                IMAGENES oImagen = new IMAGENES
                {
                    IOR_PACIENTE = oExplo.IOR_PACIENTE,
                    IOR_EXPLORACION = oExplo.OID,
                    EXT = "pdf",
                    PATH = Utils.Varios.ObtenerCarpetaDocumentosEscaneados(),
                    OWNER = oidtipoDocumento
                };
                oImagen.OID = int.Parse(ImagenesRepositorio.Insertar(oImagen));
                string nombreDocumentoAlmacenado = Utils.Varios.ObtenerCarpetaDocumentosEscaneados() + oImagen.OID + ".pdf";
                if (oidtipoDocumento == 16)
                {
                    System.IO.File.WriteAllBytes(documentoTemp, aPDF);
                    Models.HTMLToPDF.Converter.InsertBarCodeToPdf(documentoTemp, nombreDocumentoAlmacenado, oExplo.OID.ToString());
                    aPDF = System.IO.File.ReadAllBytes(nombreDocumentoAlmacenado);
                }
                else
                {
                    System.IO.File.WriteAllBytes(nombreDocumentoAlmacenado, aPDF);

                }
            }

            if (oidtipoDocumento == 19)
            {
                string documentoTempFicha = RutaDOCS + @"\RWEXPLORACION_" + oFichaImprimir.First().IOR_PACIENTE + DateTime.Now.ToString("ddMMyyyyhhmmss") + ".pdf";

                System.IO.File.WriteAllBytes(documentoTempFicha, aPDF);
                Models.HTMLToPDF.Converter.InsertBarCodeToPdf(documentoTempFicha, rutaFichaTemplate, oExplo.OID.ToString());
                aPDF = System.IO.File.ReadAllBytes(rutaFichaTemplate);
                try
                {
                    System.IO.File.Delete(rutaFichaTemplate);
                }
                catch (Exception)
                {


                }
            }
            return File(aPDF, "application/pdf");
        }
        public ActionResult ImprimirJustificante(int oid, string horaLlegada, string horaRealizada, string textoJustificante)
        {

            EXPLORACION oExplo = ExploracionRepositorio.Obtener(oid);
            if (oExplo.EMPRESA.NOMBRE.Contains("DELFOS"))
            {
                return RedirectToAction("ImprimirJustificanteDelfos", "Exploracion", new
                {
                    oid = oid,
                    observaciones = textoJustificante
                });
            }
            WebReport webReport = new WebReport();
            string reportPath = Path.Combine(Server.MapPath("~/Reports"), "Justificante.frx");

            // create report instance
            Report report = new Report();

            // load the existing report
            webReport.Report.Load(reportPath);

            List<VWImprimirJustificante> oFichaImprimir = new List<VWImprimirJustificante>();
            VWImprimirJustificante oItem = new VWImprimirJustificante
            {
                FECHA = oExplo.FECHA.Value,
                HORA = oExplo.HORA_EX,
                PACIENTE = oExplo.PACIENTE.PACIENTE1,
                HORA_LL = oExplo.HORA_LL,
                TRAC = (int)oExplo.PACIENTE.TRAC,

            };
            oFichaImprimir.Add(oItem);
            FastReport.TextObject txtLlegada = (FastReport.TextObject)webReport.Report.FindObject("txtHoraSortida");
            txtLlegada.Text = "Hora sortida " + horaRealizada;

            FastReport.TextObject txtSortida = (FastReport.TextObject)webReport.Report.FindObject("txtHoraRealizada");
            txtSortida.Text = "Hora realitzada " + horaLlegada;

            FastReport.TextObject textoLibre = (FastReport.TextObject)webReport.Report.FindObject("txtTexto");
            textoLibre.Text = textoJustificante;

            // register the business object
            webReport.RegisterData(oFichaImprimir, "EXPLORACION");
            ViewBag.WebReport = webReport;
            return View("Report");

        }

        public ActionResult ImprimirJustificanteDelfos(int oid, string observaciones)
        {

            EXPLORACION oExplo = ExploracionRepositorio.Obtener(oid);
            CENTROS oCentro = db.Centros.Single(c => c.OID == oExplo.OWNER);
            P_INFORMES oPlantilla = db.P_Informes.Where(p => p.TITULO.Contains("JUSTIFICANTE")
                                                        && p.CANAL == oExplo.PACIENTE.OTROS4).ToList().First();

            VidSignerClient oClienteVid = new VidSignerClient();
            string ofichero = oClienteVid.GenerarPDFiText(true, oPlantilla.OID, oid, observaciones);
            //string nombreFichero=Server.MapPath("~/pdf/JUST_" + oid + DateTime.Now.ToString("HHmmss") + ".pdf");
            string nombreFichero = ("JUST_" + oid + DateTime.Now.ToString("HHmmss") + ".pdf");
            System.IO.File.Move(ofichero, Server.MapPath("~/pdf/" + nombreFichero));
            return new HttpStatusCodeResult(HttpStatusCode.OK, nombreFichero);



        }



        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();

            }
            base.Dispose(disposing);
        }
    }
}
