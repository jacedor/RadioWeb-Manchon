using RadioWeb.Models;
using RadioWeb.Models.Logica;
using RadioWeb.Models.Repos;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace RadioWeb.Controllers
{
    public class SettingsController : Controller
    {
        private RadioDBContext db = new RadioDBContext();
        //Almacena los filtros de aparatos, mutua, fecha etc para luego volver al mismo estado
        public JsonResult Filtros(FiltrosBusquedaExploracion item)
        {
            try
            {
                if (!String.IsNullOrEmpty(item.DescAparato) && item.oidAparato <= 0)
                {
                    item.oidAparato = DaparatoRepositorio.Obtener(item.DescAparato).OID;
                }
            }
            catch (Exception)
            {


            }

            Session["FiltrosBusqueda"] = item;

            return Json(item);

        }

        [HttpPost]
        public void Visual(string controlador, string objeto, string collapsed)
        {
            using (var oEntities = new UsersDBContext())
            {
                var usuario = oEntities.UCCADUSER.Single(u => u.LOGIN == User.Identity.Name);
                UCCADPERM oPermiso = oEntities.UCCADPERM
                    .SingleOrDefault(u => u.IDUSER == usuario.IDUSER && u.MODULO.ToUpper() == "RADIOWEB"
                    && u.OBJNAME.ToUpper() == String.Concat(controlador.ToUpper(), ".", objeto.ToUpper()));
                if (oPermiso == null)
                {
                    oPermiso = new UCCADPERM();
                    oPermiso.IDUSER = usuario.IDUSER;
                    oPermiso.MODULO = "RadioWeb";
                    oPermiso.OBJNAME = String.Concat(controlador.ToUpper(), ".", objeto.ToUpper());
                    oPermiso.ESTADO = (collapsed == "true" ? 1 : 0);
                    oEntities.UCCADPERM.Add(oPermiso);
                }
                else
                {
                    oPermiso.ESTADO = (collapsed == "true" ? 1 : 0);
                    oEntities.Entry(oPermiso).State = EntityState.Modified;
                }
                oEntities.SaveChanges();

            }
        }

      

        //Este metodo quita una exploración en la lista de exploraciones que un usuario puede almacenar, por ejemplo en el carrito   
        [HttpPost]
        public void RemoveExploracion(int oid)
        {
            List<LISTADIA> oLista;
            List<LISTADIA> oListaResult = new List<LISTADIA>();

            oLista = Session["ExploracionesLista"] as List<LISTADIA>;

            foreach (LISTADIA item in oLista)
            {
                if (item.OID != oid && item.OID > 0)
                {
                    oListaResult.Add(item);
                }

            }
            Session["ExploracionesLista"] = oListaResult;

        }

        
        //Este metodo almacena una exploración en la lista de exploraciones que un usuario puede almacenar, por ejemplo en el carrito
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult SaveExploracion(int oid, bool todasDelDia = false)
        {
            List<LISTADIA> oLista;
            if (Session["ExploracionesLista"] == null)
            {
                oLista = new List<LISTADIA>();
            }
            else
            {
                oLista = (Session["ExploracionesLista"] as List<LISTADIA>).Where(s => s.OID > 0).ToList();
            }
            LISTADIA oExploracionAgregar;
            if (oid > 0)
            {
                oExploracionAgregar = ListaDiaRepositorio.Obtener(oid);
                if (todasDelDia)
                {
                    foreach (LISTADIA item in ListaDiaRepositorio.ObtenerPorPacienteYFecha(oExploracionAgregar.OID, oExploracionAgregar.IOR_PACIENTE, (DateTime)oExploracionAgregar.FECHA))
                    {
                        oLista.Add(item);
                    }
                }

                oLista.Add(oExploracionAgregar);

            }



            Session["ExploracionesLista"] = oLista;
            return PartialView("_ListaDiaMini", oLista);
        }
        //Carga las exploraciones del carrito
        [AllowAnonymous]
        public ActionResult LoadExploraciones()
        {
            List<LISTADIA> item = new List<LISTADIA>();

            if (Session["ExploracionesLista"] != null)
            {
                item = (Session["ExploracionesLista"] as List<LISTADIA>).Where(s => s.OID > 0).ToList();
            }
            WebConfigRepositorio oConfig = new WebConfigRepositorio();
            ViewBag.CampoMutua = oConfig.ObtenerValor("ComboMutuas").ToUpper();
            return PartialView("_ListaDiaMini", item);
        }


        //Carga las exploraciones del carrito
        [AllowAnonymous]
        public ActionResult LoadPeticiones()
        {

            // var peticionesHL7 = ListaDiaRepositorio.ListaPeticionesHL7();
            List<LISTADIA> item = new List<LISTADIA>();

            var listaPeticionesPendientesAgendar = db.BolsaPruebas.Where(p => p.IOR_EXPLORACION < 0);
            foreach (var itemBolsa in listaPeticionesPendientesAgendar)
            {
                string urlDocumento = "";
                if (itemBolsa.IOR_DOCUMENTO > 0)
                {
                    urlDocumento = "/Imagenes/Imprimir/" + itemBolsa.IOR_DOCUMENTO.ToString();

                }
                item.Add(new LISTADIA
                {
                    OID = -1,
                    IOR_BOLSAPETICIONES = itemBolsa.OID,
                    OIDDOCUMENTOBOLSA = itemBolsa.IOR_DOCUMENTO,
                    FECHA_IDEN = itemBolsa.FECHAENTRADA,
                    CENTRO = itemBolsa.IOR_CENTRO,
                    CENTRONAME = CentrosRepositorio.Obtener(itemBolsa.IOR_CENTRO).NOMBRE,
                    COMENT = itemBolsa.COMENTARIO,
                    IOR_PACIENTE = -1,                    
                    FACTURADA = false,
                    ESTADO = "0",
                    FECHA = itemBolsa.FECHAENTRADA,
                    HORA = itemBolsa.FECHAENTRADA.ToString("HH:mm"),
                    PACIENTE = itemBolsa.PACIENTE,
                    COLOR = (itemBolsa.PRIORIDAD == "URGENTE" ? "Crimson" : (itemBolsa.PRIORIDAD == "ALTA" ? "Orange" : "Green")),
                    PRIORIDAD = itemBolsa.PRIORIDAD,
                    MUTUA = MutuasRepositorio.Obtener(itemBolsa.IOR_ENTIDADPAGADORA ?? -1).NOMBRE,
                    COD_MUT = MutuasRepositorio.Obtener(itemBolsa.IOR_ENTIDADPAGADORA ?? -1).CODMUT,
                    EXPLO = itemBolsa.EXPLORACION,
                    GRUPOAPA = itemBolsa.IOR_GAPARATO,
                    TELEFONOPETICION = itemBolsa.TELEFONO1,
                    MEDICO = ColegiadoRepositorio.Obtener(itemBolsa.IOR_COLEGIADO).NOMBRE,
                    COD_FIL = itemBolsa.EXPLORACION,
                    CENTROEXTERNO = CentrosExternosRepositorio.Obtener(itemBolsa.IOR_CENTROEXTERNO ?? -1).NOMBRE,
                    TEXTO = itemBolsa.TEXTO,
                    URLDOCUMENT=urlDocumento
                }) ;

                
            }

            WebConfigRepositorio oConfig = new WebConfigRepositorio();
            ViewBag.CampoMutua = oConfig.ObtenerValor("ComboMutuas").ToUpper();
            
            return PartialView("_ListaDiaPeticionesMini", item);
        }

        //Este metodo almacena la url antes de redirigir a otra página para luego poder habilitar un botón de volver correcto
        [HttpPost]
        [ValidateInput(false)]
        public void SaveUrlRetono(string url)
        {
            Session["UrlRetorno"] = url;
        }

        public string LoadUrlRetorno()
        {

            string item = "/Home/Index";

            if (Session["UrlRetorno"] != null)
            {
                item = Session["UrlRetorno"] as string;
                Session["UrlRetorno"] = null;
            }

            return item;

        }


        //Almacena recorridos dentro de la aplicación tipo exploracion visitada , informe, paciente, etc
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult SaveActividad(ActividadUsuario item)
        {
            item.CUANDO = DateTime.Now;
            switch (item.TIPO)
            {
                case "EXPLORACION":
                    item.URL = "/Exploracion/Details/" + item.OID;
                    break;
                case "INFORME":
                    item.URL = "/Informe/Details/" + item.OID;
                    break;
                case "PACIENTE":
                    item.URL = "/Paciente/Details/" + item.OID;
                    break;
                default:
                    break;
            }
            Queue<ActividadUsuario> oCola;
            if (Session["ActividadesUsuario"] == null)
            {
                oCola = new Queue<ActividadUsuario>();
            }
            else
            {
                oCola = Session["ActividadesUsuario"] as Queue<ActividadUsuario>;
            }

            if (oCola.Count == 6)
            {
                oCola.Dequeue();
            }
            bool yaEstaEnCola = false;
            foreach (var elemento in oCola)
            {
                if (elemento.OID == item.OID)
                {
                    yaEstaEnCola = true;
                }
            }

            if (!yaEstaEnCola)
            {
                oCola.Enqueue(item);
            }

            Session["ActividadesUsuario"] = oCola;
            return PartialView("ListaActividad", oCola.Reverse());

        }



        //Almacena los filtros de aparatos, mutua, fecha etc para luego volver al mismo estado
        public JsonResult SaveFiltros(FiltrosBusquedaExploracion item)
        {
            try
            {
                if (!String.IsNullOrEmpty(item.DescAparato) && item.oidAparato <= 0)
                {
                    item.oidAparato = DaparatoRepositorio.Obtener(item.DescAparato).OID;
                }
            }
            catch (Exception)
            {


            }

            Session["FiltrosBusqueda"] = item;

            return Json(item);

        }

        public JsonResult LoadFiltros()
        {

            var item = new FiltrosBusquedaExploracion { Fecha = DateTime.Now.ToShortDateString() };

            if (Session["FiltrosBusqueda"] != null)
            {
                item = Session["FiltrosBusqueda"] as FiltrosBusquedaExploracion;
            }

            return Json(item);

        }


        public JsonResult SaveFiltrosPaciente(FiltrosBusquedaPaciente item)
        {

            Session["FiltrosBusquedaPaciente"] = item;

            return Json(item);

        }

        public JsonResult LoadFiltrosPaciente()
        {

            var item = new FiltrosBusquedaPaciente { Nombre = "" };

            if (Session["FiltrosBusquedaPaciente"] != null)

                item = Session["FiltrosBusquedaPaciente"] as FiltrosBusquedaPaciente;

            return Json(item);

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
