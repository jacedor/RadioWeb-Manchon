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
using System.Web;
using System.Web.Mvc;

namespace RadioWeb.Controllers
{
    [Autorization]
    public class CalendarioController : Controller
    {

        private UsersDBContext usersDBContext = new UsersDBContext();

       // private FiltrosRepository _Filtrosrepository;
        private ParametrosUsuarioRepository _ParametrosUsuarioRepo;

        public CalendarioController()
        {
        
            _ParametrosUsuarioRepo = new ParametrosUsuarioRepository(usersDBContext);
        }

        [ConfiguracionVisualAttribute]
        public ActionResult Index()
        {
            VWFiltrosHome oViewModel = new VWFiltrosHome();
            oViewModel.FILTROS = new ViewModels.VWFiltros();
            oViewModel.FILTROS.FECHA = (ViewData["HOME.FECHA"] != null ? ViewData["HOME.FECHA"].ToString() : DateTime.Now.ToString("dd/MM/yyyy"));
            oViewModel.FILTROS.INFORMADA = (ViewData["HOME.INFORMADA"] != null ? ViewData["HOME.INFORMADA"].ToString() : "");
            oViewModel.FILTROS.PAGADO = (ViewData["HOME.PAGADO"] != null ? ViewData["HOME.PAGADO"].ToString() : "");
            oViewModel.FILTROS.FACTURADA = (ViewData["HOME.FACTURADA"] != null ? ViewData["HOME.FACTURADA"].ToString() : "");            
            oViewModel.FILTROS.IOR_GRUPO = (ViewData["HOME.IOR_GRUPO"] != null ? (int)ViewData["HOME.IOR_GRUPO"] : -1);
            oViewModel.FILTROS.IOR_APARATO = (ViewData["HOME.IOR_APARATO"] != null ? (int)ViewData["HOME.IOR_APARATO"] : -1);
            oViewModel.FILTROS.IOR_CENTRO = (ViewData["HOME.IOR_CENTRO"] != null ? (int)ViewData["HOME.IOR_CENTRO"] : -1);
            oViewModel.FILTROS.IOR_TIPO = (ViewData["HOME.IOR_TIPO"] != null ? (int)ViewData["HOME.IOR_TIPO"] : -1);
            oViewModel.FILTROS.IOR_MEDICO = (ViewData["HOME.IOR_MEDICO"] != null ? (int)ViewData["HOME.IOR_MEDICO"] : -1);
            oViewModel.FILTROS.IOR_COLEGIADO = (ViewData["HOME.IOR_COLEGIADO"] != null ? (int)ViewData["HOME.IOR_COLEGIADO"] : -1);
            oViewModel.FILTROS.IOR_ENTIDADPAGADORA = (ViewData["HOME.IOR_ENTIDADPAGADORA"] != null ? (int)ViewData["HOME.IOR_ENTIDADPAGADORA"] : -1);
            oViewModel.FILTROS.ESTADO = (ViewData["HOME.ESTADO"] != null ? (int)ViewData["HOME.ESTADO"] : -1);
            oViewModel.FILTROS.OIDACTIVA = (ViewData["HOME.OIDACTIVA"] != null ? (int)ViewData["HOME.OIDACTIVA"] : -200);
          
            return View(oViewModel);
        }
        //
        [HttpPost]
        public ActionResult Index(VWFiltrosHome oFiltrosHome)
        {
            ViewBag.Title = "Calendario";
            if (User.Identity.Name == null)
            {
                return this.RedirectToAction("Index", "Users");
            }
            FiltrosBusquedaExploracion oFiltros = new FiltrosBusquedaExploracion
            {
                Fecha = oFiltrosHome.FILTROS.FECHA,
                oidMutua = oFiltrosHome.FILTROS.IOR_ENTIDADPAGADORA,
                oidAparato = oFiltrosHome.FILTROS.IOR_APARATO,
                oidCentro = oFiltrosHome.FILTROS.IOR_CENTRO,
                oidGrupoAparato = oFiltrosHome.FILTROS.IOR_GRUPO,
                oidExploracion = oFiltrosHome.FILTROS.IOR_TIPO,
                oidEstadoExploracion = oFiltrosHome.FILTROS.ESTADO,
                oidMedicoInformante = oFiltrosHome.FILTROS.IOR_MEDICO,
                IOR_COLEGIADO=oFiltrosHome.FILTROS.IOR_COLEGIADO,
                informada = oFiltrosHome.FILTROS.INFORMADA,
                pagado = oFiltrosHome.FILTROS.PAGADO,
                facturado = oFiltrosHome.FILTROS.FACTURADA
            };
            DateTime oMesToRender;
            if (oFiltros.Fecha == null)
            {
                oMesToRender = DateTime.Now;
            }
            else
            {
                oMesToRender = DateTime.Parse(oFiltros.Fecha);
            }
            USUARIO oUser = UsuariosRepositorio.Obtener(User.Identity.Name);
            Mes oMes = new Mes(oMesToRender, oFiltros, oUser);
            int idUser = oUser.IDUSER;

            _ParametrosUsuarioRepo.Update(oFiltrosHome.FILTROS, idUser, "HOME");
            return PartialView("_Calendario", oMes);
        }

        [ConfiguracionVisualAttribute]
        public ActionResult ResumenDiario(string fecha)
        {


            if (User.Identity.Name == null)
            {
                return this.RedirectToAction("Index", "Users");
            }
            VWFiltrosHome oViewModel = new VWFiltrosHome();
            oViewModel.FILTROS = new ViewModels.VWFiltros();
            oViewModel.FILTROS.FECHA =fecha;
          
            oViewModel.FILTROS.IOR_GRUPO = (ViewData["HOME.IOR_GRUPO"] != null ? (int)ViewData["HOME.IOR_GRUPO"] : -1);
            oViewModel.FILTROS.IOR_APARATO = (ViewData["HOME.IOR_APARATO"] != null ? (int)ViewData["HOME.IOR_APARATO"] : -1);
            oViewModel.FILTROS.IOR_CENTRO = (ViewData["HOME.IOR_CENTRO"] != null ? (int)ViewData["HOME.IOR_CENTRO"] : -1);
            oViewModel.FILTROS.IOR_TIPO = (ViewData["HOME.IOR_TIPO"] != null ? (int)ViewData["HOME.IOR_TIPO"] : -1);
            oViewModel.FILTROS.IOR_MEDICO = (ViewData["HOME.IOR_MEDICO"] != null ? (int)ViewData["HOME.IOR_MEDICO"] : -1);
            oViewModel.FILTROS.IOR_COLEGIADO = (ViewData["HOME.IOR_COLEGIADO"] != null ? (int)ViewData["HOME.IOR_COLEGIADO"] : -1);
            oViewModel.FILTROS.IOR_ENTIDADPAGADORA = (ViewData["HOME.IOR_ENTIDADPAGADORA"] != null ? (int)ViewData["HOME.IOR_ENTIDADPAGADORA"] : -1);
            oViewModel.FILTROS.ESTADO = (ViewData["HOME.ESTADO"] != null ? (int)ViewData["HOME.ESTADO"] : -1);
            oViewModel.FILTROS.INFORMADA = (ViewData["HOME.INFORMADA"] != null ? ViewData["HOME.INFORMADA"].ToString() : "A");
            oViewModel.FILTROS.PAGADO = (ViewData["HOME.PAGADO"] != null ? ViewData["HOME.PAGADO"].ToString() : "A");
            oViewModel.FILTROS.FACTURADA = (ViewData["HOME.FACTURADA"] != null ? ViewData["HOME.FACTURADA"].ToString() : "A");
            oViewModel.FILTROS.OIDACTIVA = (ViewData["HOME.OIDACTIVA"] != null ? (int)ViewData["HOME.OIDACTIVA"] : -200);

            USUARIO oUser = UsuariosRepositorio.Obtener(User.Identity.Name);
     
            _ParametrosUsuarioRepo.Update(fecha, oUser.IDUSER, "HOME","FECHA");
            
            Utils.ResumenDiario oResumenDiario = new Utils.ResumenDiario(
                DateTime.Parse( oViewModel.FILTROS.FECHA),
                oViewModel.FILTROS.IOR_ENTIDADPAGADORA,
                     oViewModel.FILTROS.IOR_GRUPO   ,
                          oViewModel.FILTROS.IOR_APARATO,
                               oViewModel.FILTROS.IOR_CENTRO,
                                    oViewModel.FILTROS.IOR_TIPO,
                                    oViewModel.FILTROS.ESTADO,
                                    oViewModel.FILTROS.IOR_MEDICO,
                                    oViewModel.FILTROS.INFORMADA,
                                    oViewModel.FILTROS.PAGADO,
                                    oViewModel.FILTROS.FACTURADA,
                                    oViewModel.FILTROS.IOR_COLEGIADO
);
            return PartialView("_ResumenDiario", oResumenDiario);




        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                usersDBContext.Dispose();
            }
            base.Dispose(disposing);
        }


    }
}
