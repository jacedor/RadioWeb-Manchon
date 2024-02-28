using RadioWeb.Models;
using RadioWeb.Models.Repos;
using RadioWeb.ViewModels.Pagos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace RadioWeb.Controllers
{
    public class PagosController : Controller
    {
        //
        private RadioDBContext db;

        private VMPago getData(int oid)
        {
            double DeudaExplos = 0.0;
            double DeudaConsu = 0.0;
            double PagadoExplos = 0.0;
            double PagadoConsu = 0.0;
            WebConfigRepositorio oConfig = new WebConfigRepositorio();
            VMPago oResult = new VMPago();
            
            List<EXPLORACION> ExploracionesDia = new List<EXPLORACION>();

            EXPLORACION oExploracion = ExploracionRepositorio.Obtener(oid);
       
            oExploracion.GAPARATO = GAparatoRepositorio.Obtener(oExploracion.DAPARATO.OWNER);
            oResult.IOR_GAPARATO = oExploracion.GAPARATO.OID;
            oResult.GRUPO = oExploracion.GAPARATO.DES_GRUP;
            //esta propiedad la usamos para agregar un consumible como privado desde la ventana modal de agregar consumibles
            oResult.IOR_MUTUACONSUMIBLE = db.Mutuas.Where(p => p.OWNER == 1).First().OID;//  oExploracion.IOR_ENTIDADPAGADORA.Value;
            oResult.CONSUMIBLESASIGNABLES = Precios_ConsumRepositorio.GetConsumibles(oResult.IOR_MUTUACONSUMIBLE, oResult.IOR_GAPARATO);
            ExploracionesDia.Add(oExploracion);
            DeudaExplos = DeudaExplos + oExploracion.CANTIDAD.Value;
            PagadoExplos += PagosRepositorio.GetSumaPagado(oid);
            List<EXP_CONSUM> oLista = Exp_ConsumRepositorio.GetConsumiblesPendientes(oid);
            oExploracion.CONSUMIBLES = oLista;
            foreach (EXP_CONSUM item in oLista)
            {
                oResult.IOR_ADDCONSUMIBLE = item.IOR_CONSUM.Value;
                DeudaConsu += (double)item.PRECIO;
                PagadoConsu += PagosRepositorio.GetSumaPagado(item.OID);
            }

            bool pagoAntesConfirmacion = (oConfig.ObtenerValor("PagoAntesExploracion") == "T" ? true : false);
            //Despues vamos a agregar otras exploraciones hechas durante el mismo dia
            IEnumerable<LISTADIA> exploracionAPagar = new List<LISTADIA>();
            if (!pagoAntesConfirmacion)
            {
                exploracionAPagar = ListaDiaRepositorio
                    .ObtenerPorPacienteYFecha(oid, oExploracion.IOR_PACIENTE, oExploracion.FECHA.Value)
                    .Where(e => e.ESTADO == "3");
            }
            else
            {
                exploracionAPagar = ListaDiaRepositorio
                    .ObtenerPorPacienteYFecha(oid, oExploracion.IOR_PACIENTE, oExploracion.FECHA.Value)
                    .Where(e => e.ESTADO == "3" || e.ESTADO=="2");

            }
            foreach (LISTADIA item in exploracionAPagar)
            {
                //COMO NO TENEMOS EL CAMPO PAGAR A NIVEL DE LISTA DIA TENEMOS QUE IR A BUSCAR LA EXPLORACION
                EXPLORACION oExploracion2 = ExploracionRepositorio.Obtener(item.OID);

                List<EXP_CONSUM> oListaOtras = Exp_ConsumRepositorio.GetConsumiblesPendientes(item.OID);
                oExploracion2.CONSUMIBLES = oListaOtras;
                oExploracion2.GAPARATO = GAparatoRepositorio.Obtener(oExploracion.DAPARATO.OWNER);
                ExploracionesDia.Add(oExploracion2);

                foreach (EXP_CONSUM item2 in oListaOtras)
                {
                    DeudaConsu += (double)item2.PRECIO;
                    PagadoConsu += PagosRepositorio.GetSumaPagado(item2.OID);
                }
                DeudaExplos = DeudaExplos + item.CANTIDAD;
                PagadoExplos += PagosRepositorio.GetSumaPagado(item.OID);
            }

            oResult.EXPLORACIONES = ExploracionesDia;
            oResult.CANTIDADTOTALCONSUMIBLES = DeudaConsu;
            oResult.CANTIDADPAGADA = PagadoExplos + PagadoConsu;
            oResult.CANTIDAD = DeudaExplos;
            oResult.CANTIDADCONSUPENDIENTE = DeudaConsu;
            oResult.CANTIDADPENDIENTE = (DeudaExplos + DeudaConsu) - (PagadoExplos + PagadoConsu);
            oResult.TOTAL = (DeudaExplos + DeudaConsu);
            oResult.MONEDA = MonedaRepositorio.Obtener(oExploracion.IOR_MONEDA.Value);
            return oResult;
        }

        public PagosController()
        {
            db = new RadioDBContext();
        }

        //carga la lista de exploraciones de una fecha
        public ActionResult LoadExploraciones(int oid)
        {
            VMPago oPagosTotales = getData(oid);
            ViewBag.PendienteExplos = oPagosTotales.CANTIDAD;
            ViewBag.TotalPendiente = oPagosTotales.CANTIDADPENDIENTE;
            return PartialView("_ListaDiaPagos", oPagosTotales.EXPLORACIONES);
        }

        //carga la lista de consumibles asociados a una exploracion
        public ActionResult LoadConsumibles(int oid)
        {
            VMPago oPagosTotales = getData(oid);
            ViewBag.PendienteExplos = oPagosTotales.CANTIDADCONSUPENDIENTE;
            ViewBag.TotalPendiente = oPagosTotales.CANTIDADPENDIENTE;
            List<EXP_CONSUM> oPagosConsumibles = Exp_ConsumRepositorio.GetConsumiblesPendientes(oid);
            return PartialView("ListaConsumibles", oPagosConsumibles);
        }

        //se usa para cargar pagos de los consumibles y de las exploraciones
        public ActionResult LoadPagos(int oid)
        {
            List<PAGOS> oPagos = PagosRepositorio.GetPagosExploracion(oid);
            if (oPagos.Count > 0)
            {
                if (oPagos.First().CID == 1378)
                {
                    EXP_CONSUM oConsumible = Exp_ConsumRepositorio.Obtener(oid);
                    ViewBag.Pendiente = String.Concat((oConsumible.PRECIO - oPagos.Sum(p => p.CANTIDAD)), oConsumible.SIMBOLO);
                }
                else
                {
                    EXPLORACION oExplo = ExploracionRepositorio.Obtener(oid);
                    oPagos.First().EXPLORACION = oExplo;
                    ViewBag.Pendiente =String.Concat( (oExplo.CANTIDAD - oPagos.Sum(p => p.CANTIDAD)),"€") ;
                }
                return PartialView("ListaPagos", oPagos);
            }
            else
            {
                return new HttpStatusCodeResult(HttpStatusCode.NotFound, "");
            }

            
        }

        public ActionResult Index(int oid)
        {
            return View(getData(oid));
        }

        public ActionResult PagoRapido(int oid)
        {
            WebConfigRepositorio oConfig = new WebConfigRepositorio();
            VMPago oPagos = getData(oid);
            foreach (var explo in oPagos.EXPLORACIONES)
            {
                var pagoExploracionFechaHoy = db.Pagos.Where(p => p.FECHA == DateTime.Today && p.OWNER == explo.OID && p.CANTIDAD < p.DEUDA_CANTIDAD).ToList();
                //Si ya hay un pago generado con fecha de hoy actualizamos la cantidad de dicho pago al precio de la exploración
                if (pagoExploracionFechaHoy.Count > 0)
                {
                    var Pago = pagoExploracionFechaHoy.FirstOrDefault();
                    Pago.CANTIDAD = explo.CANTIDAD;
                    db.SaveChanges();
                }//sino hay ningun pago y además queda algo pendientecreamos un plago aplazado del tipo exploración con fecha de hoy
                else
                {
                    var PagoRealizadosExplo = db.Pagos.Where(p => p.OWNER == explo.OID).ToList();
                    if (PagoRealizadosExplo.Sum(p => p.CANTIDAD) < explo.CANTIDAD)
                    {
                        PAGOS oPago = new PAGOS
                        {
                            APLAZADO = "T",
                            BORRADO = "F",
                            CANTIDAD = explo.CANTIDAD - PagoRealizadosExplo.Sum(p => p.CANTIDAD),
                            CID = 1377,
                            DEUDA_CANTIDAD = explo.CANTIDAD,
                            FECHA = DateTime.Now,
                            DEUDA_FECHA = explo.FECHA,
                            IOR_EMPRESA = 4,
                            IOR_MONEDA = int.Parse(oConfig.ObtenerValor("IOR_MONEDA")),
                            MONEDA = MonedaRepositorio.Obtener(int.Parse(oConfig.ObtenerValor("IOR_MONEDA"))),
                            OWNER = explo.OID,
                            TIPOPAGO = "V"
                        };
                        PagosRepositorio.Insertar(oPago);
                    }

                }
                ExploracionRepositorio.UpdateCampo("PAGADO", "T", explo.OID);
                //RECORREMOS LOS CONSUMIBLES
                foreach (EXP_CONSUM consumible in explo.CONSUMIBLES)
                {
                    var pagoConsumibleFechaHoy = db.Pagos.Where(p => p.FECHA == DateTime.Today && p.OWNER == consumible.OID && p.CANTIDAD < p.DEUDA_CANTIDAD).ToList();
                    if (pagoConsumibleFechaHoy.Count > 0)
                    {
                        var Pago = pagoConsumibleFechaHoy.FirstOrDefault();
                        Pago.CANTIDAD = consumible.PRECIO;
                        db.SaveChanges();
                    }
                    else
                    {
                        var PagoRealizadosExplo = db.Pagos.Where(p => p.OWNER == consumible.OID).ToList();
                        if (PagoRealizadosExplo.Sum(p => p.CANTIDAD) < explo.CANTIDAD) { 
                            PAGOS oPago = new PAGOS
                        {
                            APLAZADO = "T",
                            BORRADO = "F",
                            CANTIDAD = consumible.PRECIO - PagoRealizadosExplo.Sum(p => p.CANTIDAD),                                
                            CID = 1378,
                            DEUDA_CANTIDAD = consumible.PRECIO,
                            FECHA = DateTime.Now,
                            DEUDA_FECHA = explo.FECHA,
                            IOR_EMPRESA = 4,
                            IOR_MONEDA = int.Parse(oConfig.ObtenerValor("IOR_MONEDA")),
                                MONEDA = MonedaRepositorio.Obtener(int.Parse(oConfig.ObtenerValor("IOR_MONEDA"))),

                                OWNER = consumible.OID,
                            TIPOPAGO = "V"
                        };
                        PagosRepositorio.Insertar(oPago);
                        }
                    }
                    var oConsumible = db.Exp_Consum.Where(p => p.OID == consumible.OID).FirstOrDefault();
                    oConsumible.PAGADO = "T";
                    db.SaveChanges();
                }
            }


            return RedirectToAction("Index", new { oid = oid });
        }

        //Agrega un pago a una exploracion
        [HttpPost]
        public ActionResult Add(int oid, string tipoPago)
        {
            WebConfigRepositorio oConfig = new WebConfigRepositorio();
            LISTADIA oTempExplo = ListaDiaRepositorio.Obtener(oid);
            //Inserción del pago asociado a la exploración

            if (tipoPago == "exploracion")
            {
                PAGOS oPago = new PAGOS
                {
                    APLAZADO = "F",
                    BORRADO = "F",
                    CANTIDAD = 0,
                    CID = (tipoPago == "exploracion" ? 1377 : 1378),
                    DEUDA_CANTIDAD = oTempExplo.CANTIDAD,
                    FECHA = DateTime.Now,
                    DEUDA_FECHA = oTempExplo.FECHA,
                    IOR_EMPRESA = 4,
                    IOR_MONEDA = int.Parse(oConfig.ObtenerValor("IOR_MONEDA")),
                    MONEDA=MonedaRepositorio.Obtener(int.Parse(oConfig.ObtenerValor("IOR_MONEDA"))),
                    OWNER = oTempExplo.OID,
                    TIPOPAGO = "V"
                };
                if (oPago.DEUDA_FECHA != DateTime.Today)
                {
                    oPago.APLAZADO = "T";
                }
                PagosRepositorio.Insertar(oPago);
            }
            else
            {
                EXP_CONSUM oConsumible = new EXP_CONSUM();
                oConsumible.PAGADO = "F";
                oConsumible.BORRADO = "F";
                oConsumible.APLAZADO = "F";
                oConsumible.PAGAR = "T";
                oConsumible.FACTURADO = "F";
                oConsumible.IOR_FACTURA = -1;
                oConsumible.IOR_EXPLORACION = oTempExplo.OID;

               

                Exp_ConsumRepositorio.Insertar(oConsumible);
            }



            return new HttpStatusCodeResult(200);

        }

        [HttpPost]
        public ActionResult UpdateFormaPago(string name, int pk, string value)
        {
            var Pago = db.Pagos.Where(p => p.OID == pk).FirstOrDefault();
            Pago.TIPOPAGO = value;
            db.SaveChanges();
            return new HttpStatusCodeResult(200);

        }

        [HttpPost]
        public ActionResult UpdateCantidad(string name, int pk, string value)
        {
            var Pago = db.Pagos.Where(p => p.OID == pk).FirstOrDefault();
            Pago.CANTIDAD = float.Parse(value);
            db.SaveChanges();


            //Significa que es un consumible
            if (Pago.CID == 1378)
            {
                var oConsumible = db.Exp_Consum.Where(p => p.OID == Pago.OWNER).FirstOrDefault();
                var PagoRealizadosConsum = db.Pagos.Where(p => p.OWNER == oConsumible.OID).ToList();
                oConsumible.PAGADO = (PagoRealizadosConsum.Sum(p => p.CANTIDAD) == oConsumible.PRECIO ? "T" : "F");
                db.SaveChanges();
            }
            else
            {
                EXPLORACION oExploracion = ExploracionRepositorio.Obtener(Pago.OWNER);
                var PagoRealizadosExplo = db.Pagos.Where(p => p.OWNER == oExploracion.OID).ToList();
                oExploracion.PAGADO = (PagoRealizadosExplo.Sum(p => p.CANTIDAD) == oExploracion.CANTIDAD ? "T" : "F");
                ExploracionRepositorio.Update(oExploracion);
            }

            return new HttpStatusCodeResult(200);

        }


        //[HttpPost]
        //public ActionResult PagarConsumible(int oidPago, int oidConsumible, string fechaExploracion, double cantidad, string formaPago, bool pagoRapido)
        //{


        //    EXP_CONSUM oConsumible = db.Exp_Consum.Where(c => c.OID == oidConsumible).First();

        //    //Obtenemos la linea de pago que nos estan abonando            
        //    PAGOS oPago = db.Pagos.Where(p => p.OID == oidPago).FirstOrDefault();

        //    oPago.CANTIDAD = cantidad;
        //    oPago.TIPOPAGO = formaPago;
        //    //si estan pagando mediante el pago rapido y la fecha de la deuda es anterior a hoy
        //    if (oPago.DEUDA_FECHA != DateTime.Today && pagoRapido)
        //    {
        //        oPago.APLAZADO = "T";
        //    }
        //    //Si la Cantidad Abonada es la misma que la deuda ponemos la exploración pagada
        //    if (oPago.DEUDA_CANTIDAD == oPago.CANTIDAD)
        //    {
        //        oConsumible.PAGADO = "T";
        //    }

        //    oPago.CANTIDAD = cantidad;
        //    oPago.TIPOPAGO = formaPago;

        //    db.SaveChanges();



        //    return RedirectToAction("PagoRapido", new { oid = oConsumible.IOR_EXPLORACION });


        //}

        //[HttpPost]
        //public ActionResult PagoRapido(int oidPago, int oidExploracion, string fechaExploracion, double cantidad, string formaPago, bool pagoRapido)
        //{

        //    EXPLORACION oExploracion = ExploracionRepositorio.Obtener(oidExploracion);
        //    List<LISTADIA> ExploracionesDeHoy = PagosRepositorio.GetPagosPaciente(oExploracion.IOR_PACIENTE, oExploracion.FECHA.Value);

        //    //double totalPagosExploracion =    PagosRepositorio.GetPagosExploracion(oidExploracion);
        //    double totalPagarHoy = ExploracionesDeHoy.Sum(p => p.CANTIDAD);
        //    var totalPagado = cantidad + ExploracionesDeHoy.Sum(e => e.PAGOS.Sum(p => p.CANTIDAD));

        //    //Obtenemos la linea de pago que nos estan abonando
        //    PAGOS oPago = PagosRepositorio.Obtener(oidPago);

        //    //si estan pagando mediante el pago rapido y la fecha de la deuda es anterior a hoy
        //    if (oPago.DEUDA_FECHA != DateTime.Today && pagoRapido)
        //    {
        //        oPago.APLAZADO = "T";
        //    }

        //    oPago.CANTIDAD = cantidad;
        //    oPago.TIPOPAGO = formaPago;

        //    PAGOS oPagado = PagosRepositorio.Update(oPago);

        //    //Si la Cantidad Abonada es la misma que la deuda ponemos la exploración pagada
        //    if (oPagado.DEUDA_CANTIDAD == oPagado.CANTIDAD)
        //    {
        //        oExploracion.PAGADO = "T";
        //        ExploracionRepositorio.Update(oExploracion);
        //    }

        //    return new HttpStatusCodeResult(200);
        //}

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
