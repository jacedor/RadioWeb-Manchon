using RadioWeb.Filters;
using RadioWeb.Models;
using RadioWeb.Models.Logica;
using RadioWeb.Models.Repos;
using RadioWeb.Repositories;
using RadioWeb.Utils;
using RadioWeb.ViewModels.Paciente;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace RadioWeb.Controllers
{
    public class PacienteController : Controller
    {
        //
        // GET: /Paciente/
        private RadioDBContext db = new RadioDBContext();
        private UsersDBContext usersDBContext = new UsersDBContext();

        private DireccionRepository _direccionRepository;
        private TelefonoRepository _telefonoRepository;

        public PacienteController()
        {
            _direccionRepository = new DireccionRepository(db);
            _telefonoRepository = new TelefonoRepository(db);
        }

        [ConfiguracionVisualAttribute]
        public ActionResult Index()
        {
            USUARIO oUsuarioLogeado = (USUARIO)Session["Usuario"];
            //Manresa o Sant Cugat
            if (oUsuarioLogeado.CENTROASOCIADO == 8 || oUsuarioLogeado.CENTROASOCIADO == 3)
            {
                return this.RedirectToAction("Index", "Users");
            }
            FiltrosBusquedaPaciente oFiltros = new FiltrosBusquedaPaciente();
            return View(Models.Repos.PacienteRepositorio.Lista(oFiltros));
        }

        [HttpPost]
        public ActionResult Index(FiltrosBusquedaPaciente oFiltros)
        {
            return PartialView("_ListaPacientesAlta", Models.Repos.PacienteRepositorio.Lista(oFiltros, 50));

        }

        [HttpPost]
        public JsonResult Buscar(string query, bool conDirecciones = false)
        {
            return Json(PacienteRepositorio.Lista(query, conDirecciones), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult Paso1AltaExploracionMultiple(FiltrosBusquedaPaciente oFiltros)
        {
            WebConfigRepositorio oConfig = new WebConfigRepositorio();

            ViewBag.CampoMutua = oConfig.ObtenerValor("ComboMutuas").ToUpper();
            return PartialView("PacienteFilaAltaExploMultiple", Models.Repos.PacienteRepositorio.Lista(oFiltros, 25));

        }

        [HttpPost]
        public ActionResult Paso1AltaExploracion(FiltrosBusquedaPaciente oFiltros)
        {
            WebConfigRepositorio oConfig = new WebConfigRepositorio();

            ViewBag.CampoMutua = oConfig.ObtenerValor("ComboMutuas").ToUpper();
            return PartialView("PacienteFilaAltaExploracion", Models.Repos.PacienteRepositorio.Lista(oFiltros, 25));
        }
        [HttpPost]
        public ActionResult BuscarPacienteFusionar(VMAddPaso1 oPacienteViewModel)
        {
            string paciente = oPacienteViewModel.PACIENTE1;


            WebConfigRepositorio oConfig = new WebConfigRepositorio();
            ViewBag.CampoMutua = oConfig.ObtenerValor("ComboMutuas").ToUpper();
            var item = new FiltrosBusquedaPaciente { Nombre = paciente };
            return PartialView("_ListaPacientesAlta", Models.Repos.PacienteRepositorio.Lista(item, 10));
        }

        [HttpPost]
        public ActionResult Fusionar(List<string> pacientes, int pacientePrincipal)
        {
            string query = "EXECUTE PROCEDURE FUSION_PACIENTES(" + pacientePrincipal;
            for (int i = 0; i < 5; i++)
            {
                try
                {
                    if (pacientes[i] != pacientePrincipal.ToString())
                    {
                        query = query + "," + pacientes[i].ToString();
                    }
                    else
                    {
                        query = query + ",-1";
                    }

                }
                catch (Exception)
                {

                    query = query + ",-1";
                }

            }
            query = query + ");";

            db.Database.ExecuteSqlCommand(query);


            return new HttpStatusCodeResult(HttpStatusCode.OK);
        }



        //
        // GET: /Paciente/Details/5
        [ConfiguracionVisualAttribute]
        public ActionResult Details(int IOR_PACIENTE, bool TraerInformesYExplos = false, string ReturnUrl = "")
        {

            if (Request.IsAjaxRequest())
            {
                return PartialView("Details", Models.Repos.PacienteRepositorio.Obtener(IOR_PACIENTE));
            }
            else
            {
                PACIENTE oPaciente = Models.Repos.PacienteRepositorio.Obtener(IOR_PACIENTE);

                if (TraerInformesYExplos || !String.IsNullOrEmpty(ReturnUrl))
                {
                    oPaciente.EXPLORACIONES = ListaDiaRepositorio.ObtenerPorPaciente(oPaciente.OID);
                    oPaciente.INFORMES = InformesRepositorio.ObtenerPorPaciente(oPaciente.OID);
                }

                if (oPaciente.NUEVA_LOPD == "T")
                {
                    if (db.Vid_Documentos.Where(d => d.IOR_PACIENTE.Value == IOR_PACIENTE && d.CID == 2).Count() > 0)
                    {
                        oPaciente.RUTA_LOPD = db.Vid_Documentos.Where(d => d.IOR_PACIENTE.Value == IOR_PACIENTE && d.CID == 2).First().NOMBRE;
                    }
                }


                //ViewBag.DatosPacienteContraer = usersDBContext.UCCADPERM
                //    .SingleOrDefault(p => p.MODULO == "RadioWeb" && p.OBJNAME == "Paciente.DatosPersonales");
                //ViewBag.DatosPacienteContraer = usersDBContext.UCCADPERM
                //  .SingleOrDefault(p => p.MODULO == "RadioWeb" && p.OBJNAME == "Paciente.DatosPersonales");

                //oPaciente.HAYCAMBIOSPACIENTE = "F";
                // get the previous url and store it with view model
                oPaciente.URLPREVIAPACIENTE = ReturnUrl;


                return View("Details", oPaciente);
            }


        }

        [HttpPost]
        public ActionResult Details(PACIENTE oPaciente)
        {

            int oidPaciente;

            try
            {

                if (!ModelState.IsValid)
                {
                    return View("Details", oPaciente);
                }
                //_direccionRepository.Update(oPaciente.DIRECCIONES, oPaciente.OID);
                //_telefonoRepository.Update(oPaciente.TELEFONOS, oPaciente.OID);

                oidPaciente = PacienteRepositorio.Update(oPaciente);
                if (oidPaciente > 0)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.OK, oPaciente.PACIENTE1);
                    return new HttpStatusCodeResult(200);
                }
                else
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

                }

            }
            catch (Exception)
            {

                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }


        }


        [HttpPost]
        public ActionResult EditarCampo(string name, int pk, string value)
        {

            PacienteRepositorio.UpdateCampo(name, value, pk);

            return new HttpStatusCodeResult(200);

        }
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        //POST de consulta permisos LOPD
        [HttpPost]
        public ActionResult getLOPDsettingsById(int idPaciente)
        {
            VMLOPDPaciente pacienteSettings = PacienteRepositorio.getLOPDsettings(idPaciente);
           
            return PartialView("_permisosLOPDbotones", pacienteSettings);

        }

}
}
