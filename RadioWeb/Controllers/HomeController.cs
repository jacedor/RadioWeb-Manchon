using ADPM.Common;
using RadioWeb.Filters;
using RadioWeb.Models;
using RadioWeb.Models.Logica;
using RadioWeb.Models.Repos;
using RadioWeb.Repositories;
using RadioWeb.ViewModels;
using RadioWeb.ViewModels.Exploracion;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace RadioWeb.Controllers
{
    [Autorization]
    public class HomeController : Controller
    {

        private UsersDBContext usersDBContext = new UsersDBContext();
        private RadioDBContext db = new RadioDBContext();
        //private FiltrosRepository _Filtrosrepository;
        private ParametrosUsuarioRepository _ParametrosUsuarioRepo;


        public HomeController()
        {
            //_Filtrosrepository = new FiltrosRepository(usersDBContext);
            _ParametrosUsuarioRepo = new ParametrosUsuarioRepository(usersDBContext);
            //db = new RadioDBContext();
        }

        [HttpPost]
        public ActionResult CambiarEstado(int estadoActual, int estadoNuevo, int oidExploracion, string hhora, int oidAparato = -1)
        {
            WebConfigRepositorio oConfig = new WebConfigRepositorio();
            string RutaWL = oConfig.ObtenerValor("RutaWL");
            string RutaWLAffidea = oConfig.ObtenerValor("RutaWLAffidea");
            ViewBag.CampoMutua = oConfig.ObtenerValor("ComboMutuas").ToUpper();
            ViewBag.ModuloVidSigner = oConfig.ObtenerValor("ModuloVidSigner").ToUpper();
            //obtenemos la exploracion que estamos actualizando
            LISTADIA oExploActualizar = ExploracionRepositorio.CambiarEstado(estadoActual, estadoNuevo, oidExploracion, RutaWL, false, RutaWLAffidea);
            List<LISTADIA> oListaActualizadas = new List<LISTADIA>();
            oListaActualizadas.Add(oExploActualizar);
            EXPLORACION.ESTADOS valorEnumEstadoNuevo = (EXPLORACION.ESTADOS)estadoNuevo;
            if (System.Web.HttpContext.Current.Session["Usuario"] == null)
            {
                var usuarioAutenticated = UsuariosRepositorio.Obtener(User.Identity.Name);
                Session["Usuario"] = (USUARIO)usuarioAutenticated;
            }
            ViewData["oUsuario"] = System.Web.HttpContext.Current.Session["Usuario"];

            USUARIO oUSer = (USUARIO)ViewData["oUsuario"];

            _ParametrosUsuarioRepo.Update(oidExploracion, oUSer.IDUSER, "HOME", "OIDACTIVA");


            switch (valorEnumEstadoNuevo)
            {
                case EXPLORACION.ESTADOS.PRESENCIA:

                    //Si no hay filtro de aparato tenemos que actualizar todas las exploraciones
                    if (oidAparato < 0)
                    {
                        //Si el Valor del estado nuevo es presencia, debemos mirar si hay mas exploraciones el mismo dia
                        foreach (LISTADIA item in ListaDiaRepositorio.ObtenerPorPacienteYFecha(oidExploracion, oExploActualizar.IOR_PACIENTE, (DateTime)oExploActualizar.FECHA))
                        {
                            if (!(item.ESTADO == "3"))//sI UNA EXPLORACION YA ESTÁ CONFIRMADA NO SE PUEDE CAMBIAR SU ESTADO
                            {
                                if (item.ESTADO != EXPLORACION.ESTADOS.PRESENCIA.ToString())
                                {
                                    oListaActualizadas.Add(
                                        ExploracionRepositorio.CambiarEstado(estadoActual, estadoNuevo, item.OID, RutaWL, false, RutaWLAffidea));
                                    item.ESPERA = "00:00";
                                }

                            }
                        }
                    }
                    else
                    {
                        //Si el Valor del estado nuevo es presencia, debemos mirar si hay mas exploraciones el mismo dia
                        foreach (LISTADIA item in ListaDiaRepositorio.ObtenerPorPacienteYFecha(oidExploracion, oExploActualizar.IOR_PACIENTE, (DateTime)oExploActualizar.FECHA))
                        {
                            DAPARATOS oaparato = DaparatoRepositorio.Obtener(item.COD_FIL);
                            if (item.COD_FIL == oaparato.COD_FIL)
                            {
                                if (!(item.ESTADO == "3"))//sI UNA EXPLORACION YA ESTÁ CONFIRMADA NO SE PUEDE CAMBIAR SU ESTADO
                                {
                                    if (item.ESTADO != EXPLORACION.ESTADOS.PRESENCIA.ToString())
                                    {
                                        oListaActualizadas.Add(
                                       ExploracionRepositorio.CambiarEstado(estadoActual, estadoNuevo, item.OID, RutaWL, false, RutaWLAffidea));
                                        item.ESPERA = "00:00";
                                    }

                                }
                            }
                        }
                    }

                    break;



                default:
                    break;
            }

            List<LISTADIAAMBFORATS> oResult = new List<LISTADIAAMBFORATS>();
            foreach (var item in oListaActualizadas)
            {
                LISTADIAAMBFORATS oTemp = ListaDiaAmbForatsRepositorio.Obtener(item.OID);
                if (item.OID == oidExploracion)
                {
                    oTemp.ACTIVA = true;
                }
                oResult.Add(oTemp);
            }
            return PartialView("_ListaDiaFilas", oResult);
        }


        public void AnularHoraLibre(string fecha, string hhora, int aparato, string comentario)
        {
            HorasAnulasRepositorio.Insertar(fecha, aparato, hhora, comentario);
        }

        //public ActionResult AgendarPeticion(int oidPeticion, int ior_aparato)
        //{
        //    BOLSA_PRUEBAS oBolsa = db.BolsaPruebas.Single(p => p.OID == oidPeticion);
        //    DAPARATOS oAparato = DaparatoRepositorio.Obtener(ior_aparato);
        //    APARATOS oTipoExploracion = AparatoRepositorio.Obtener(oBolsa.IOR_TIPOEXPLORACION);
        //    //si el aprato y el grupo de exploracoin no son del mismo grupo, algo no va bien.
        //    if (oAparato.OWNER != oTipoExploracion.OWNER) 
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.InternalServerError);
        //    }else{
        //        return new HttpStatusCodeResult(HttpStatusCode.OK);

        //    }

        //}

        public ActionResult Trasladar(int oid, string fecha, string hhora, int aparato)
        {
            WebConfigRepositorio oConfig = new WebConfigRepositorio();
            string RutaWL = oConfig.ObtenerValor("RutaWL");
            string RutaWLAffidea = oConfig.ObtenerValor("RutaWLAffidea");
            ViewBag.CampoMutua = oConfig.ObtenerValor("ComboMutuas").ToUpper();
            ViewBag.ModuloVidSigner = oConfig.ObtenerValor("ModuloVidSigner").ToUpper();
            //1.- Trasladamos la exploración seleccionada estado de la exploración seleccionada
            ExploracionRepositorio.Trasladar(oid, fecha, hhora, aparato);

            //quitamos la exploración trasladada de las exploraciones almacenadas en la variable de session del usuario
            List<LISTADIA> oLista;
            List<LISTADIA> oListaResult = new List<LISTADIA>();
            if (Session["ExploracionesLista"] != null)
            {
                oLista = Session["ExploracionesLista"] as List<LISTADIA>;
                foreach (LISTADIA item in oLista)
                {
                    if (item.OID != oid)
                    {
                        oListaResult.Add(item);
                    }

                }
                Session["ExploracionesLista"] = oListaResult;
            }

            return new HttpStatusCodeResult(HttpStatusCode.OK);
            //FiltrosBusquedaExploracion oFiltros = new FiltrosBusquedaExploracion();
            //if (Session["FiltrosBusqueda"] != null)
            //{
            //    oFiltros = (FiltrosBusquedaExploracion)Session["FiltrosBusqueda"];
            //    oFiltros.oidExploracionSeleccionada = oid;

            //}

            //if (System.Web.HttpContext.Current.Session["Usuario"] == null)
            //{
            //    var usuarioAutenticated = UsuariosRepositorio.Obtener(User.Identity.Name);
            //    Session["Usuario"] = (USUARIO)usuarioAutenticated;
            //}
            //ViewData["oUsuario"] = System.Web.HttpContext.Current.Session["Usuario"];
            //List<LISTADIAAMBFORATS> oResult = new List<LISTADIAAMBFORATS>();
            //LISTADIAAMBFORATS oTemp = ListaDiaAmbForatsRepositorio.Obtener(oid);
            //oResult.Add(oTemp);
            //return PartialView("_ListaDiaFilas", oResult);
        }




        [HttpGet]
        public JsonResult Buscar(VWFiltros FILTROS, bool irACalendario = false)
        {
            ViewBag.Title = "Lista Día";
            ViewBag.estadoExploracion = "true";
            FiltrosBusquedaExploracion oFiltros = new FiltrosBusquedaExploracion
            {
                Fecha = FILTROS.FECHA,
                oidMutua = FILTROS.IOR_ENTIDADPAGADORA,
                oidAparato = FILTROS.IOR_APARATO,
                oidCentro = FILTROS.IOR_CENTRO,
                oidGrupoAparato = FILTROS.IOR_GRUPO,
                oidExploracion = FILTROS.IOR_TIPO,
                oidEstadoExploracion = FILTROS.ESTADO,
                IOR_COLEGIADO = FILTROS.IOR_COLEGIADO,
                oidMedicoInformante = FILTROS.IOR_MEDICO,
                oidExploracionSeleccionada = FILTROS.OIDACTIVA,
                informada = FILTROS.INFORMADA,
                pagado = FILTROS.PAGADO,
                Paciente = FILTROS.PACIENTE,
                facturado = FILTROS.FACTURADA,
                busquedaTotal = (FILTROS.BUSQUEDATOTAL ? "T" : "F"),
                busquedaTotalPorMedico = (FILTROS.BUSQUEDATOTALPORMEDICO ? "T" : "F"),
                iorPaciente = FILTROS.IOR_PACIENTE

            };
            if (oFiltros.Paciente == null)
            {
                oFiltros.Paciente = "";
            }
            oFiltros.Borrados = (oFiltros.oidEstadoExploracion == 1 ? "T" : "F");

            if (System.Web.HttpContext.Current.Session["Usuario"] == null)
            {
                var usuarioAutenticated = UsuariosRepositorio.Obtener(User.Identity.Name);
                Session["Usuario"] = (USUARIO)usuarioAutenticated;
            }
            ViewData["oUsuario"] = System.Web.HttpContext.Current.Session["Usuario"];
            int idUser = ((USUARIO)ViewData["oUsuario"]).IDUSER;
            _ParametrosUsuarioRepo.Update(FILTROS, idUser, "HOME");

            IEnumerable<LISTADIAAMBFORATS2> oResult;
            WebConfigRepositorio oConfig = new WebConfigRepositorio();
            ViewBag.CampoMutua = oConfig.ObtenerValor("ComboMutuas").ToUpper();
            
            ViewBag.ModuloVidSigner = oConfig.ObtenerValor("ModuloVidSigner").ToUpper();



            string camposQueryStandard = "select  p.color COLORHORARIO,";
            camposQueryStandard += "case p.ESTADO";
            camposQueryStandard += " when 0 then 'Blue'";
            camposQueryStandard += " when 1 then 'red'";
            camposQueryStandard += " when 2 then 'green'";
            camposQueryStandard += " when 3 then 'black'";
            camposQueryStandard += " when 4 then 'brown'";
            camposQueryStandard += " when 5 then 'purple'";
            camposQueryStandard += " else '' end COLORESTADO,";
            camposQueryStandard += " CASE";
            camposQueryStandard += " WHEN(p.OID IS NULL) THEN - 1";
            camposQueryStandard += " ELSE p.OID";
            camposQueryStandard += " END";
            camposQueryStandard += " OID, ";
            camposQueryStandard += "case p.CID";
            camposQueryStandard += " when 0 then 'white'";
            camposQueryStandard += " when 1 then 'teal'";
            camposQueryStandard += " when 2 then 'red'";
            camposQueryStandard += " when 3 then 'lime'";
            camposQueryStandard += " when 4 then 'yellow'";
            camposQueryStandard += " when 5 then 'blue'";
            camposQueryStandard += " when 6 then 'fuchsia'";
            camposQueryStandard += " when 7 then 'aqua'";
            camposQueryStandard += " when 8 then 'marron'";
            camposQueryStandard += " when 9 then 'green'";
            camposQueryStandard += " when 10 then 'olive'";
            camposQueryStandard += " when 11 then 'navy'";
            camposQueryStandard += " when 12 then 'purple'";
            camposQueryStandard += " when 13 then 'gray'";
            camposQueryStandard += " when 14 then 'black'";
            camposQueryStandard += " else '' end CID,";
         //  camposQueryStandard += "p.CITAEXTERNA,";
            camposQueryStandard += "p.simbolo,p.VIP,p.PACIENTE,p.cid,p.CENTRO,p.VERS,p.IOR_EMPRESA," +
                "p.IOR_GPR,p.IOR_PACIENTE,p.INTOCABLE,p.FECHA,p.hora,p.HORA_LL,p.HORA_EX,p.INFORMADA,p.COD_MUT," +
                "p.COD_FIL,p.FIL,p.TECNICO,p.CANTIDAD,p.SIMBOLO,p.PAGADO,p.FACTURADA,p.NUM_FAC,p.ESTADO,p.HAYCOMEN," +
                "p.NOFACTURAB,p.HAYCONSUMIBLE,p.medico,p.vip,LEFT (p.TEXTO, 15) SUBTEXTO,P.TEXTO,p.CANCELADO,p.FECHAMAXENTREGA," +
                "p.cod_med,p.IOR_MEDICO,p.FIRMA_CONSEN,P.APARATO DESAPARATO,p.LOPD,p.EXPLO DES_FIL,p.MUTUA DES_MUT,p.IOR_MASTER,p.QRCOMPARTIRCASO," +
                "p.BORRADO,p.colegiado,p.recorded,p.aplazado";


            string camposAgendaDiaria = "p.edad, p.ESPERA,a.OWNER GRUPOAPA,p.ORDER_HHORA,p.ORDER_HORA,p.HHORA, CASE WHEN(a.oid IS NULL) THEN " + oFiltros.oidAparato + "  ELSE a.oid END IOR_APARATO ";

            string query = camposQueryStandard.Replace("''", "p.COLOR") + "," + camposAgendaDiaria + "  from AGENDADIARIA (" + DateTime.Parse(oFiltros.Fecha).ToString("MM/dd/yyyy").QuotedString() + ", " + oFiltros.oidAparato +
             "," + oFiltros.oidCentro + "," + oFiltros.oidGrupoAparato + "," + oFiltros.oidMutua + "," + oFiltros.oidExploracion +
             "," + oFiltros.Borrados.QuotedString() + ")";

            if (oFiltros.oidAparato > 0)
            {
                query = query + " p left join DAPARATOS a on a.OID = " + oFiltros.oidAparato;
            }
            else
            {
                query = query + " p left join DAPARATOS a on p.COD_FIL=a.COD_FIL";
            }


            bool clausulaWhere = false;
            if (oFiltros.informada == "T" || oFiltros.informada == "F")
            {
                query = query + " where p.INFORMADA=" + oFiltros.informada.QuotedString();
                clausulaWhere = true;
            }
            if (!String.IsNullOrEmpty(oFiltros.facturado) && oFiltros.facturado != "null" & oFiltros.facturado != "A")
            {
                query = query + (clausulaWhere ? " and facturada=" + oFiltros.facturado.QuotedString() : " where facturada=" + oFiltros.facturado.QuotedString());
                clausulaWhere = true;
            }

            if (!String.IsNullOrEmpty(oFiltros.pagado) && oFiltros.pagado != "null" & oFiltros.pagado != "A")
            {
                query = query + (clausulaWhere ? " and pagado=" + oFiltros.pagado.QuotedString() : " where pagado=" + oFiltros.pagado.QuotedString());
                clausulaWhere = true;
            }
            if (oFiltros.oidMedicoInformante > 0)
            {
                if (oFiltros.busquedaTotalPorMedico == "T")
                {

                    query = camposQueryStandard.Replace("p.color","p.CID") + ",p.FECHA hhora from listadia p where p.IOR_MEDICO=" + oFiltros.oidMedicoInformante +
                        " and(not estado in ('1', '4', '5')) and p.INFORMADA <> 'T' and (p.fecha > '01/10/2017') order by fecha desc";
                }
                else
                {
                    if (clausulaWhere)
                    {
                        query = query + " and IOR_MEDICO=" + oFiltros.oidMedicoInformante;
                    }
                    else
                    {
                        query = query + " where IOR_MEDICO=" + oFiltros.oidMedicoInformante;
                        clausulaWhere = true;
                    }
                }

            }

            if (!String.IsNullOrEmpty(oFiltros.Paciente) && oFiltros.Paciente.Length > 2)
            {
                int oidExploracionBusqueda;
                if (int.TryParse(oFiltros.Paciente, out oidExploracionBusqueda))
                {
                    query = camposQueryStandard.Replace("p.color", "p.CID") + ",p.FECHA hhora from listadia p where oid=" + oFiltros.Paciente;
                }
                else if (oFiltros.busquedaTotal == "T")
                {
                    query = camposQueryStandard.Replace("p.color", "p.CID") + ",p.FECHA hhora from listadia p where p.IOR_PACIENTE=" + oFiltros.iorPaciente + " order by fecha desc";
                }
                else
                {
                    if (clausulaWhere)
                    {
                        query = query + " and PACIENTE like '%" + oFiltros.Paciente.ToUpper() + "%'";
                    }
                    else
                    {
                        query = query + " where PACIENTE like '%" + oFiltros.Paciente.ToUpper() + "%'";
                        clausulaWhere = true;
                    }

                }
            }

            if (oFiltros.IOR_COLEGIADO > 0)
            {

                if (clausulaWhere)
                {
                    query = query + " and P.ior_colegiado=" + oFiltros.IOR_COLEGIADO.ToString();
                }
                else
                {
                    query = query + " where P.ior_colegiado=" + oFiltros.IOR_COLEGIADO.ToString();
                    clausulaWhere = true;
                }


            }

            oResult = db.Database.SqlQuery<LISTADIAAMBFORATS2>(query).ToList<LISTADIAAMBFORATS2>();



            if (oFiltros.oidEstadoExploracion >= 0)
            {
                //solo huecos
                if (oFiltros.oidEstadoExploracion == 6)
                {
                    oResult = oResult.Where(e => e.OID < 0);

                }
                else
                {
                    oResult = oResult.Where(e => e.ESTADO == oFiltros.oidEstadoExploracion.ToString());

                }
            }

            if (oResult.Count() > 0)
            {
                string mensaje = AgendaGenRepositorio.Obtener(oFiltros.oidAparato, DateTime.Parse(oFiltros.Fecha));
                ViewBag.MensajeAgendaDiaria = mensaje;
                oResult.First().TEXTOAGENDA = mensaje;
            }
            return Json(oResult, JsonRequestBehavior.AllowGet);
            //return PartialView("_ListaDia", oResult);
        }



        [ConfiguracionVisualAttribute]
        public ActionResult Index(string fecha = "",
            int oidExploracion = -1)
        {
            VWFiltrosHome oViewModel = new VWFiltrosHome();
            oViewModel.FILTROS = new ViewModels.VWFiltros();
            if (String.IsNullOrEmpty(fecha))
            {
                oViewModel.FILTROS.FECHA = (ViewData["HOME.FECHA"] != null ? ViewData["HOME.FECHA"].ToString() : DateTime.Now.ToString("dd/MM/yyyy"));
            }
            else
            {
                oViewModel.FILTROS.FECHA = fecha;
                var usuarioAutenticated = UsuariosRepositorio.Obtener(User.Identity.Name);
                _ParametrosUsuarioRepo.Update(fecha, usuarioAutenticated.IDUSER, "HOME", "FECHA");



            }

            oViewModel.FILTROS.INFORMADA = (ViewData["HOME.INFORMADA"] != null ? ViewData["HOME.INFORMADA"].ToString() : "A");
            oViewModel.FILTROS.PAGADO = (ViewData["HOME.PAGADO"] != null ? ViewData["HOME.PAGADO"].ToString() : "A");
            oViewModel.FILTROS.FACTURADA = (ViewData["HOME.FACTURADA"] != null ? ViewData["HOME.FACTURADA"].ToString() : "A");
            oViewModel.FILTROS.IOR_GRUPO = (ViewData["HOME.IOR_GRUPO"] != null ? (int)ViewData["HOME.IOR_GRUPO"] : -1);
            oViewModel.FILTROS.IOR_APARATO = (ViewData["HOME.IOR_APARATO"] != null ? (int)ViewData["HOME.IOR_APARATO"] : -1);
            oViewModel.FILTROS.IOR_CENTRO = (ViewData["HOME.IOR_CENTRO"] != null ? (int)ViewData["HOME.IOR_CENTRO"] : -1);
            oViewModel.FILTROS.IOR_TIPO = (ViewData["HOME.IOR_TIPO"] != null ? (int)ViewData["HOME.IOR_TIPO"] : -1);
            oViewModel.FILTROS.IOR_MEDICO = (ViewData["HOME.IOR_MEDICO"] != null ? (int)ViewData["HOME.IOR_MEDICO"] : -1);
            oViewModel.FILTROS.IOR_ENTIDADPAGADORA = (ViewData["HOME.IOR_ENTIDADPAGADORA"] != null ? (int)ViewData["HOME.IOR_ENTIDADPAGADORA"] : -1);
            //oViewModel.FILTROS.PACIENTE = (ViewData["HOME.PACIENTE"] != null ? ViewData["HOME.PACIENTE"].ToString() : "");
            oViewModel.FILTROS.IOR_PACIENTE = (ViewData["HOME.IOR_PACIENTE"] != null ? (int)ViewData["HOME.IOR_PACIENTE"] : -1);
            oViewModel.FILTROS.BUSQUEDATOTAL = (ViewData["HOME.BUSQUEDATOTAL"] != null ? ViewData["HOME.BUSQUEDATOTAL"].ToString() == "T" : false);
            oViewModel.FILTROS.BUSQUEDATOTALPORMEDICO = (ViewData["HOME.BUSQUEDATOTALPORMEDICO"] != null ? ViewData["HOME.BUSQUEDATOTALPORMEDICO"].ToString() == "T" : false);
            oViewModel.FILTROS.ESTADO = (ViewData["HOME.ESTADO"] != null ? (int)ViewData["HOME.ESTADO"] : -1);
            oViewModel.FILTROS.OIDACTIVA = (ViewData["HOME.OIDACTIVA"] != null ? (int)ViewData["HOME.OIDACTIVA"] : -200);
            if (oidExploracion > 0)
            {
                EXPLORACION oexplo = ExploracionRepositorio.Obtener(oidExploracion);
                oViewModel.FILTROS.OIDACTIVA = oidExploracion;
                oViewModel.FILTROS.IOR_APARATO = oexplo.IOR_APARATO.Value;
            }
            //RellenarCombos();      
            WebConfigRepositorio oConfig = new WebConfigRepositorio();
            ViewBag.CampoMutua = oConfig.ObtenerValor("ComboMutuas").ToUpper();
            ViewBag.ModuloVidSigner = oConfig.ObtenerValor("ModuloVidSigner").ToUpper();
            ViewBag.ModuloAudio = oConfig.ObtenerValor("ModuloAudioInformes").ToUpper();
            ViewBag.PAGAANTESCONFIRMAR = oConfig.ObtenerValor("PagoAntesExploracion").ToUpper();
            ViewBag.MensajeAgendaDiaria = AgendaGenRepositorio.Obtener(oViewModel.FILTROS.IOR_APARATO, DateTime.Parse(oViewModel.FILTROS.FECHA));
            ViewBag.estadoExploracion = "true";
            if (System.Configuration.ConfigurationManager.AppSettings["NombreEmpresa"].ToString().Contains("CALERO"))
            {
                ViewBag.IdExploracion = "CITAEXTERNA".ToUpper();
            }
            else
            {
                ViewBag.IdExploracion = "OID".ToUpper();

            }
            return View(oViewModel);
        }



        public ActionResult Test()
        {

            return View();
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


        //
        // GET: /Default1/Delete/5

        public ActionResult TimeoutRedirect()
        {
            return View("Index");
        }

    }
}
