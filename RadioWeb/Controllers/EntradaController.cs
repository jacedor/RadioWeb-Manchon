using RadioWeb.Models;
using RadioWeb.Models.Repos;
using RadioWeb.Utils;
using RadioWeb.ViewModels.Exploracion;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace RadioWeb.Controllers
{
    public class EntradaController : Controller
    {


        private RadioDBContext db = new RadioDBContext();

        // GET: Entrada
        public ActionResult Index(int oid)
        {
            EXPLORACION oExploracion = ExploracionRepositorio.Obtener(oid);

            VWEntrada oviewModel = new VWEntrada
            {
                OIDEXPLORACION = oid,
                PACIENTE = oExploracion.PACIENTE.PACIENTE1,
                SEXO= oExploracion.PACIENTE.SEXO,
                EMPRESA = oExploracion.EMPRESA,
                CIP = oExploracion.PACIENTE.CIP,
                TARJETA = oExploracion.PACIENTE.TARJETA,
                HORA = oExploracion.HORA,
                HORALL = DateTime.Now.ToString("HH:mm"),
                DNI = oExploracion.PACIENTE.DNI,
                TRAC= oExploracion.PACIENTE.TRAC,
                DNIRESPONSABLE = oExploracion.PACIENTE.DNIRESPONSABLE,
                RESPONSABLE = oExploracion.PACIENTE.RESPONSABLE,
                FECHANACIMIENTO = (oExploracion.PACIENTE.FECHAN.HasValue ? oExploracion.PACIENTE.FECHAN.Value.ToShortDateString() : ""),
                FECHAMAXENTREGA = oExploracion.FECHAMAXENTREGA,
                IOR_PACIENTE = oExploracion.PACIENTE.OID,
                FACTURADA=oExploracion.FACTURADA,
                PAGADA = oExploracion.PAGADO,
                INTOCABLE=oExploracion.INTOCABLE,
                NHCAP=oExploracion.NHCAP,
                REGISTRE=oExploracion.REGISTRE,
                SIMBOLO=oExploracion.MONEDA.SIMBOLO,
                IOR_TIPOEXPLORACION = oExploracion.IOR_TIPOEXPLORACION.Value,
                IOR_CARDIOLOGO = (oExploracion.IOR_CIRUJANO.HasValue ? oExploracion.IOR_CIRUJANO.Value : -1),
                IOR_MEDICOINFORMANTE = (oExploracion.IOR_MEDICO.HasValue ? oExploracion.IOR_MEDICO.Value : -1),
                IOR_MEDICOREFERIDOR = (oExploracion.IOR_COLEGIADO.HasValue ? oExploracion.IOR_COLEGIADO.Value : -1),
                IOR_CONDICION = (oExploracion.IOR_CONDICION.HasValue ? oExploracion.IOR_CONDICION.Value : -1),
                INFORMADA = (oExploracion.INFORMADA == "T" ? true : false),
                TIPOSEXPLORACION = oExploracion.EXPLORACIONESCONTARIFA,
                NOMBREMEDICOINFORMANTE = (oExploracion.IOR_MEDICO.HasValue && oExploracion.IOR_MEDICO > 0 ?
                                                  db.Personal
                                                      .Where(p => p.OID == (oExploracion.IOR_MEDICO.HasValue ?
                                                      oExploracion.IOR_MEDICO.Value : 0))
                                                     .FirstOrDefault().NOMBRE :
                                                 "No Asignado"),
                NOMBREMEDICOREFERIDOR = (oExploracion.IOR_COLEGIADO.HasValue && oExploracion.IOR_COLEGIADO > 0 ?
                                                 ColegiadoRepositorio.Obtener(oExploracion.IOR_COLEGIADO.Value).NOMBRE :
                                                 "No Asignado"),
                MUTUADESCRIPCION = oExploracion.ENTIDAD_PAGADORA.NOMBRE,
                MUTUAS = MutuasRepositorio.Lista(false),
                IOR_ENTIDADPAGADORA = oExploracion.IOR_ENTIDADPAGADORA.Value,
                IOR_APARATO = oExploracion.IOR_APARATO.Value,
                IOR_CENTROEXTERNO= oExploracion.IOR_CENTROEXTERNO.Value,
                NOMBRECENTROEXTERNO=oExploracion.CENTROEXTERNO.NOMBRE,
                EMAIL = oExploracion.PACIENTE.EMAIL,
                MEDICOS = PersonalRepositorio.ObtenerMedicos(),
                CARDIOLOGOS = PersonalRepositorio.ObtenerCardiologos(),
                MEDICOSREFERIDORES = new List<COLEGIADOS>(),
                IDIOMAS = DataBase.Idiomas(),
                IDIOMA = oExploracion.PACIENTE.OTROS4,
                IMPORTE = oExploracion.CANTIDAD.Value,
                QRCOMPARTIRCASO = (oExploracion.PACIENTE.COMPARTIR == "T" ? true : false),
                DIRECCIONES = DireccionRepositorio.Obtener(oExploracion.IOR_PACIENTE),
                TELEFONOS = TelefonoRepositorio.Obtener(oExploracion.IOR_PACIENTE),
                CENTROEXTERNOS= CentrosExternosRepositorio.Obtener(),
                TICKET_KIOSKO = oExploracion.TICKET_KIOSKO

        };

            WebConfigRepositorio oConfig = new WebConfigRepositorio();
      
            ViewBag.ModuloVidSigner = oConfig.ObtenerValor("ModuloVidSigner").ToUpper();
         
            if (db.Refractometros.Count(r => r.VERS == 1) > 0)
            {
                ViewBag.DocumentosImprimibles = db.Refractometros.Where(r => r.VERS == 1).ToList();
            }

            return PartialView(oviewModel);
        }

        [HttpPost]
        public ActionResult Exploracion(VWEntrada oViewModel)
        {

            EXPLORACION oExploBd = ExploracionRepositorio.Obtener(oViewModel.OIDEXPLORACION);
            oExploBd.IOR_ENTIDADPAGADORA = oViewModel.IOR_ENTIDADPAGADORA;
            oExploBd.IOR_TIPOEXPLORACION = oViewModel.IOR_TIPOEXPLORACION;
            oExploBd.IOR_COLEGIADO = oViewModel.IOR_MEDICOREFERIDOR;
            oExploBd.IOR_MEDICO = oViewModel.IOR_MEDICOINFORMANTE;
            oExploBd.IOR_COLEGIADO = oViewModel.IOR_MEDICOREFERIDOR;
            oExploBd.IOR_CIRUJANO = oViewModel.IOR_CARDIOLOGO;
            oExploBd.IOR_CENTROEXTERNO = oViewModel.IOR_CENTROEXTERNO;
            oExploBd.IOR_CONDICION = oViewModel.IOR_CONDICION;
            oExploBd.TICKET_KIOSKO = oViewModel.TICKET_KIOSKO;
            
            oExploBd.FECHAMAXENTREGA = oViewModel.FECHAMAXENTREGA;
            
            if (!String.IsNullOrEmpty(oViewModel.REGISTRE))
            {
                oExploBd.REGISTRE =oViewModel.REGISTRE;
            }
            if (!String.IsNullOrEmpty(oViewModel.NHCAP))
            {
                oExploBd.NHCAP = oViewModel.NHCAP;
            }

            
            //Si es una exploración privada y se modifica el precio hay que cambiar la deuda_cantidad de 
            //todos los pagos de dicha exploración
            if (oExploBd.CANTIDAD!=oViewModel.IMPORTE && oExploBd.IOR_GPR==1)
            {
                var PagoRealizadosExplo = db.Pagos.Where(p => p.OWNER == oExploBd.OID).ToList();
                foreach (var pago in PagoRealizadosExplo)
                {
                    pago.DEUDA_CANTIDAD = oViewModel.IMPORTE;
                }
            }
            oExploBd.CANTIDAD = oViewModel.IMPORTE;
            ExploracionRepositorio.Update(oExploBd);
            return new HttpStatusCodeResult(200);
        }

        [HttpPost]
        public ActionResult Paciente(VWEntrada oViewModel)
        {
            if (!ModelState.IsValid)
            {
                return new HttpStatusCodeResult(400);
            }
            PACIENTE oPaciente = PacienteRepositorio.Obtener(oViewModel.IOR_PACIENTE);

            oPaciente.PACIENTE1 = oViewModel.PACIENTE;
            oPaciente.FECHAN = DateTime.Parse(oViewModel.FECHANACIMIENTO);
            oPaciente.EMAIL = oViewModel.EMAIL;
            //oPaciente.CID = oViewModel.IOR_ENTIDADPAGADORA;
            oPaciente.OTROS4 = oViewModel.IDIOMA;

            oPaciente.CIP = oViewModel.CIP;
            oPaciente.TARJETA = oViewModel.TARJETA;
            oPaciente.SEXO = oViewModel.SEXO;
            oPaciente.COMPARTIR = (oViewModel.QRCOMPARTIRCASO ? "T" : "F");
            

            PacienteRepositorio.Update(oPaciente);

            return new HttpStatusCodeResult(200);
        }

        [HttpPost]
        public ActionResult Contacto(VWEntrada oViewModel)
        {
            if (!ModelState.IsValid)
            {
                return new HttpStatusCodeResult(400);
            }


            //este indizador es para cuando agreguemos multiples direcciones
            for (int i = 0; i < oViewModel.DIRECCIONES.Count; i++)
            {

                //SI LA DIRECCIÓN ES UNA NUEVA INSERCION.
                if (oViewModel.DIRECCIONES.ElementAt(i).OID == -1)
                {
                    oViewModel.DIRECCIONES.ElementAt(i).OWNER = oViewModel.IOR_PACIENTE;
                    int oidDireccion = DireccionRepositorio.Insertar(oViewModel.DIRECCIONES.ElementAt(i));
                }
                else
                {
                    DireccionRepositorio.Editar(oViewModel.DIRECCIONES.ElementAt(i));
                }

            }

            List<TELEFONO> oListTelPacBD = TelefonoRepositorio.Obtener(oViewModel.IOR_PACIENTE);


            for (int j = 0; j < oListTelPacBD.Count; j++)
            {

                try
                {
                    //Buscamos en el objeto enviado desde la Página si coincide el oid, sino lo eliminamos
                    if (oListTelPacBD.Where(t => t.OID == oViewModel.TELEFONOS.ElementAt(j).OID).Count() == 0)
                    {
                        TelefonoRepositorio.Delete(oListTelPacBD[j].OID.ToString());
                    }

                }
                catch (Exception)
                {


                }

            }

            //este indizador es para cuando agreguemos multiples telefonos
            for (int i = 0; i < oViewModel.TELEFONOS.Count; i++)
            {

                //SI EL TELEFONO ES UNA NUEVA INSERCION.
                if (oViewModel.TELEFONOS.ElementAt(i).OID == -1)
                {
                    oViewModel.TELEFONOS.ElementAt(i).OWNER = oViewModel.IOR_PACIENTE;
                    int oidTelefono = TelefonoRepositorio.Insertar(oViewModel.TELEFONOS.ElementAt(i));
                }
                else
                {
                    TelefonoRepositorio.Editar(oViewModel.TELEFONOS.ElementAt(i));
                }

            }
            return new HttpStatusCodeResult(200);
        }

        [HttpPost]
        public ActionResult CopiarColegiado(int IOR_COLEGIADO, int OIDEXPLORACION)
        {
      
            try
            {
                EXPLORACION oExploactualizar = ExploracionRepositorio.Obtener(OIDEXPLORACION);

                foreach (LISTADIA item in ListaDiaRepositorio.ObtenerPorPacienteYFecha(OIDEXPLORACION, oExploactualizar.IOR_PACIENTE, (DateTime)oExploactualizar.FECHA))
                {
                    ExploracionRepositorio.ActualizarColegiado(item.OID, IOR_COLEGIADO);

                }
                return new HttpStatusCodeResult(200);
            }
            catch (Exception)
            {
                return new HttpStatusCodeResult(400);

            }


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