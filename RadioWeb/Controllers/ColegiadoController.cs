using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using RadioWeb.Models.Repos;
using RadioWeb.Models;
using FirebirdSql.Data.FirebirdClient;
using RadioWeb.ViewModels;

namespace RadioWeb.Controllers
{
    public class ColegiadoController : Controller
    {


        /// <summary>
        /// The method the ajax select2 query hits to get the attendees to display in the dropdownlist
        /// </summary>
        [HttpGet]
        public ActionResult GetColegiados(string q)
        {
            List<Select2Result> oResult = new List<Select2Result>();
            try
            {
                int colegiadonum;
                bool busquedaPorNumero = false;
                List<COLEGIADOS> oResultado;
                if (int.TryParse(q, out colegiadonum))
                {
                    oResultado = ColegiadoRepositorio.Buscar(colegiadonum);
                    busquedaPorNumero = true;
                }
                else
                {
                    oResultado = ColegiadoRepositorio.Buscar(q);
                }
                if (oResultado.Count==0)
                {
                    oResultado = ColegiadoRepositorio.BuscarSinEspec(q);
                }
                oResult = ColegiadosToSelect2Format(oResultado, busquedaPorNumero);
            }
            catch (Exception ex)
            {
                return Json("", JsonRequestBehavior.AllowGet);
            }
            //Return the data as a jsonp result
            return Json(new { items = oResult }, JsonRequestBehavior.AllowGet);

        }

        private List<Select2Result> ColegiadosToSelect2Format(List<COLEGIADOS> colegiados, bool numeroColegiado = false)
        {
            List<Select2Result> jsonColegiados = new List<Select2Result>();


            //Loop through our attendees and translate it into a text value and an id for the select list
            foreach (COLEGIADOS a in colegiados)
            {
                Select2Result oTemp = new Select2Result
                {
                    id = a.OID.ToString(),
                    text = a.NOMBRE
                };
                if (a.ESPECIALIDAD != null)
                {
                    oTemp.text = string.Concat(oTemp.text, "-", a.ESPECIALIDAD.DESCRIPCION);
                }
                if (numeroColegiado)
                {
                    oTemp.text = string.Concat(oTemp.text, "-", a.COD_MED);
                }
                jsonColegiados.Add(oTemp);

            }

            return jsonColegiados;
        }


        public class Select2Result
        {
            public string id { get; set; }
            public string text { get; set; }

        }


        [HttpPost]
        public ActionResult GetParaFichaExploracion(COLEGIADOS colegiado)
        {
            //1.- Cambiamos el colegiado de la exploracion
            ExploracionRepositorio.ActualizarColegiado(colegiado.IOR_EXPLORACION ?? -1, colegiado.OID);
            COLEGIADOS oColegiado = ColegiadoRepositorio.Obtener(colegiado.OID);
            oColegiado.IOR_EXPLORACION = colegiado.IOR_EXPLORACION;

            return PartialView("ColegiadoFichaExploracion", oColegiado);
        }

        [HttpPost]
        public ActionResult List(COLEGIADOS colegiado)
        {
            int colegiadonum;

            if (int.TryParse(colegiado.NOMBRE, out colegiadonum))
            {

                return PartialView("ColegiadoFila", ColegiadoRepositorio.Buscar(colegiadonum));
            }

            return PartialView("ColegiadoFila", ColegiadoRepositorio.Buscar(colegiado.NOMBRE));
        }

        public JsonResult Tratamientos()
        {
            return Json(new[] {
        new { id = "1", name = "Sr" },
        new { id = "2", name = "Sr. D." },
        new { id = "3", name = "Dr." },
        new { id = "4", name = "Srta." },
        new { id = "5", name = "Sra." },
        new { id = "6", name = "Sra. Dña." },
        new { id = "7", name = "Dra." },
        new { id = "8", name = "Niño" },
        new { id = "9", name = "Niña" }
    }, JsonRequestBehavior.AllowGet);


        }

        public ActionResult Create(int oid, int oidExploracion)
        {
            VWColegiado viewModel = null;

            //Crear Colegiado Nuevo
            if (oid == 0)
            {
                viewModel = new VWColegiado
                {
                    ESPECIALIDADES = EspecialidadRepositorio.Lista(),
                    CENTROSEXTERNOS = CentrosExternosRepositorio.Obtener(),
                    HEADING = "Crear Colegiado",
                    OIDEXPLORACION = oidExploracion
                };
            }
            else
            {
                //Obtenemos el colegiado de la base de datos
                COLEGIADOS oColegiado = ColegiadoRepositorio.Obtener(oid);
                viewModel = new VWColegiado
                {
                    ESPECIALIDADES = EspecialidadRepositorio.Lista(),
                    CENTROSEXTERNOS = CentrosExternosRepositorio.Obtener(),
                    HEADING = "Editar Colegiado",
                    OID = oColegiado.OID,
                    TRATA = oColegiado.TRATA,
                    COD_MED = oColegiado.COD_MED,
                    ESPEC = oColegiado.ESPEC,
                    IOR_CENTRO = oColegiado.IOR_CENTRO,
                    IOR_ESPECIALIDAD = oColegiado.IOR_ESPECIALIDAD ?? -1,
                    NIF = oColegiado.NIF,
                    NOMBRE = oColegiado.NOMBRE.Substring(oColegiado.NOMBRE.IndexOf(",") + 1),
                    APELLIDOS = oColegiado.NOMBRE.Substring(0, oColegiado.NOMBRE.IndexOf(",")),
                    TEXTO = oColegiado.TEXTO,
                    EMAIL = oColegiado.EMAIL,
                    DIRECCIONES = oColegiado.DIRECCIONES,
                    TELEFONOS = oColegiado.TELEFONOS,
                    OIDEXPLORACION = oidExploracion
                };
            }

            return View("ColegiadoForm", viewModel);

        }

        [HttpPost] // this action takes the viewModel from the modal
        [ValidateAntiForgeryToken]
        public ActionResult Create(VWColegiado viewModel)
        {
            if (!ModelState.IsValid)
            {
                viewModel.ESPECIALIDADES = EspecialidadRepositorio.Lista();
                viewModel.CENTROSEXTERNOS = CentrosExternosRepositorio.Obtener();
                viewModel.HEADING = "Crear Colegiado";
                return View("ColegiadoForm", viewModel);
            }

            var colegiado = new COLEGIADOS
            {
                TRATA = viewModel.TRATA,
                COD_MED = viewModel.COD_MED,
                ESPEC = viewModel.ESPEC,
                IOR_CENTRO = viewModel.IOR_CENTRO,
                IOR_ESPECIALIDAD = viewModel.IOR_ESPECIALIDAD,
                NIF = viewModel.NIF,
                NOMBRE = viewModel.APELLIDOS.ToUpper() + ", " + viewModel.NOMBRE.ToUpper(),
                TEXTO = viewModel.TEXTO,
                EMAIL = viewModel.EMAIL,
                DIRECCIONES = viewModel.DIRECCIONES,
                TELEFONOS = viewModel.TELEFONOS
            };

            colegiado.OID = ColegiadoRepositorio.Insertar(colegiado);
            //Si la creación de este colegiado esta vinculada a un oid de exploracion en su modelo,
            //asignamos el colegiado recién creado a la exploración y volvemos a los detalles de la misma
            if (viewModel.OIDEXPLORACION > 0)
            {
                //1.- Cambiamos el colegiado de la exploracion
                ExploracionRepositorio.ActualizarColegiado(viewModel.OIDEXPLORACION, colegiado.OID);
                Response.SetCookie(new HttpCookie("ColegiadoSaveSuccess", ""));
                //2.- Volvemos a la exploracion con el colegiado asignado
                return RedirectToAction("Details", "Exploracion", new { oid = viewModel.OIDEXPLORACION });
            }
            else
            {
                return View("ColegiadoForm", viewModel);
            }
        }



        [HttpPost] // this action takes the viewModel from the modal
        [ValidateAntiForgeryToken]
        public ActionResult Update(VWColegiado viewModel)
        {
            if (!ModelState.IsValid)
            {

                return View("ColegiadoForm", viewModel);
            }

            viewModel.ESPECIALIDADES = EspecialidadRepositorio.Lista();
            viewModel.CENTROSEXTERNOS = CentrosExternosRepositorio.Obtener();
            viewModel.HEADING = "Editar Colegiado";

            var colegiado = new COLEGIADOS
            {
                OID = viewModel.OID,
                TRATA = viewModel.TRATA,
                COD_MED = viewModel.COD_MED,
                ESPEC = viewModel.ESPEC,
                IOR_CENTRO = viewModel.IOR_CENTRO,
                IOR_ESPECIALIDAD = viewModel.IOR_ESPECIALIDAD,
                NIF = viewModel.NIF,
                NOMBRE = viewModel.APELLIDOS.ToUpper() + "," + viewModel.NOMBRE.ToUpper(),
                TEXTO = viewModel.TEXTO,
                EMAIL = viewModel.EMAIL,
                DIRECCIONES = viewModel.DIRECCIONES,
                TELEFONOS = viewModel.TELEFONOS

            };
            ColegiadoRepositorio.Update(colegiado);
            //Si la creación de este colegiado esta vinculada a un oid de exploracion en su modelo,
            //asignamos el colegiado recién creado a la exploración y volvemos a los detalles de la misma
            if (viewModel.OIDEXPLORACION > 0)
            {
                //1.- Cambiamos el colegiado de la exploracion
                ExploracionRepositorio.ActualizarColegiado(viewModel.OIDEXPLORACION, colegiado.OID);

                Response.SetCookie(new HttpCookie("ColegiadoSaveSuccess", ""));
                //2.- Volvemos a la exploracion con el colegiado asignado
                return RedirectToAction("Details", "Exploracion", new { oid = viewModel.OIDEXPLORACION });
            }
            else
            {
                return View("ColegiadoForm", viewModel);
            }

        }






    }
}
