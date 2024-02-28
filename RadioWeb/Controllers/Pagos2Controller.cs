using ADPM.Common;
using FirebirdSql.Data.FirebirdClient;
using RadioWeb.Models;
using RadioWeb.Models.Repos;
using RadioWeb.Repositories;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace RadioWeb.Controllers
{
    public class Pagos2Controller : Controller
    {

        private UsersDBContext usersDBContext = new UsersDBContext();
        private RadioDBContext db = new RadioDBContext();
        //private FiltrosRepository _Filtrosrepository;
        private ParametrosUsuarioRepository _ParametrosUsuarioRepo;


        public Pagos2Controller()
        {
            //_Filtrosrepository = new FiltrosRepository(usersDBContext);
            _ParametrosUsuarioRepo = new ParametrosUsuarioRepository(usersDBContext);
            //db = new RadioDBContext();
        }

        private double calculaDeuda(int oidPago)
        {
            var pago = db.Pagos.Single(p => p.OID == oidPago);
            var PagoRealizadosExplo = db.Pagos.Where(p => p.OWNER == pago.OWNER 
                                                        && p.BORRADO != "T").ToList();

            return pago.DEUDA_CANTIDAD.Value - PagoRealizadosExplo.Sum(p=>p.CANTIDAD).Value;
        }
        public ActionResult ActualizarPagado(int oid,string valor,string orden,int ior_consum=-1)
        {
            try
            {
                if (orden == "1")
                {
                    ExploracionRepositorio.UpdateCampo("PAGAR", valor, oid);

                }
                else
                {
                  //var pagos = PagosRepositorio.Obtener(oid);
                    
                 Exp_ConsumRepositorio.UpdateCampo("PAGAR", valor, db.Exp_Consum.Single(p => p.IOR_CONSUM == ior_consum && p.IOR_EXPLORACION==oid).OID);

                   

                }

                return new HttpStatusCodeResult(HttpStatusCode.OK);

            }
            catch
            {
                return View();
            }
        }
        // GET: Pagos2
        public ActionResult Index(string fecha, int ior_paciente)
        {
            string queryPago = "Select * from pago_paciente ("
                + DateTime.Parse(fecha).ToString("MM/dd/yyyy").QuotedString() + "," + ior_paciente +" )";
            IEnumerable<PAGO_PACIENTE> oResultFromProcedure = db.Database.SqlQuery<PAGO_PACIENTE>(queryPago).ToList<PAGO_PACIENTE>();
            oResultFromProcedure.First().PACIENTE = PacienteRepositorio.Obtener(ior_paciente);
          //  ViewData["TotalPendienteMutua"] = oResultFromProcedure.Where(p=>p.TIPO=="P").Sum(p => decimal.Parse(p.PRECIO));
            //ViewData["TotalAbonadoMutua"] = oResult.Where(p => p.TIPO == "M").Sum(p => decimal.Parse(p.PAGO));
           
            //ViewData["TotalAbonadoPrivado"] = oResult.Where(p => p.TIPO == "P").Sum(p => decimal.Parse(p.PAGO));
            List<PAGO_PACIENTE> oResult = new List<PAGO_PACIENTE>();
            foreach (var item in oResultFromProcedure)
            {
                //if (item.TIPO=="P")
                //{
                   //Nos cargamos los pagos de la explocarion
                    if (oResult.Where(p=>p.OID==item.OID && p.ORDEN=="1").Count()==0)
                    {
                        //item.PAGAR = "T";
                        if (item.FACTURADA=="T")
                        {
                            if (item.ORDEN=="1")
                            {
                                item.IOR_FACTURA =  ExploracionRepositorio.Obtener(item.OID).IOR_FACTURA.Value;
                            FACTURAS oFactura = db.Facturas.Single(p => p.OID == item.IOR_FACTURA);
                            if (oFactura!=null)
                            {
                                item.SERIEYNUMEROFACTURA = oFactura.NUM_FAC + "/" + oFactura.FECHA_FAC.Year.ToString() + "-" + oFactura.COD1;

                            }
                        }                       
                           
                        }
                        item.PagosExploracion= oResultFromProcedure.Where(p => p.OID == item.OID 
                                                                    && p.ORDEN == "1"
                                                                    && p.TIPO=="P"    ).OrderBy(p=>p.OIDPAGO).ToList();
                        item.ConsumiblesExploracion = new List<PAGO_PACIENTE>();
                        foreach (var consumible in oResultFromProcedure.Where(p => p.OID == item.OID && p.ORDEN == "2"))
                        {
                            if (item.ConsumiblesExploracion.Where(r=>r.OWNER==consumible.OWNER).Count()==0)
                            {
                            if (consumible.FACTURADA == "T")
                            {
                                PAGOS oPago = PagosRepositorio.Obtener(consumible.OIDPAGO);
                                consumible.IOR_FACTURA = db.Exp_Consum.Single(p => p.OID == oPago.OWNER).IOR_FACTURA.Value;
                                FACTURAS oFactura = db.Facturas.Single(p => p.OID == consumible.IOR_FACTURA);
                                consumible.SERIEYNUMEROFACTURA = oFactura.NUM_FAC + "/" + oFactura.FECHA_FAC.Year.ToString() + "-" + oFactura.COD1;
                            }
                            item.ConsumiblesExploracion.Add(consumible);
                            }
                        }
                        //item.ConsumiblesExploracion= oResultFromProcedure.Where(p => p.OID == item.OID && p.ORDEN == "2").OrderBy(p => p.OIDPAGO).ToList();
                        foreach (var consumible in item.ConsumiblesExploracion)
                        {
                            consumible.ConsumiblesPagos= oResultFromProcedure
                                .Where(p => p.OID == item.OID && p.ORDEN == "2" 
                                && p.EXPLORACION==consumible.EXPLORACION).ToList();
                        }
                        
                        oResult.Add(item);
                    }
                    
                //}
                //else
                //{
                //    oResult.Add(item);
                //}
            }
            oResult.First().IOR_PACIENTE = ior_paciente;
            decimal precioSumaExploraciones = oResult.Where(p => p.TIPO == "P")
                                                .Sum(p => decimal.Parse(p.PRECIO));

            decimal precioSumaConsumibles = 0;
            foreach (var explo in oResult)
            {
                precioSumaConsumibles = precioSumaConsumibles + explo.ConsumiblesExploracion.Sum(p=>decimal.Parse(p.PRECIO));
            }
            decimal TotalAbonado = oResultFromProcedure.Where(p => p.TIPO == "P")
                                                .Sum(p => decimal.Parse(p.PAGO));
            ViewData["TotalPendientePrivado"] = precioSumaExploraciones + precioSumaConsumibles - TotalAbonado;
            return View(oResult);
        }
             

        public ActionResult PagoSimple(int oid,int ior_paciente,bool crearNuevoPago)
        {
            PAGOS oPago = PagosRepositorio.Obtener(oid);
            WebConfigRepositorio oConfig = new WebConfigRepositorio();
            LISTADIA oTempExplo = ListaDiaRepositorio.Obtener(oPago.OWNER);
            var PagoRealizadosExplo = db.Pagos.Where(p => p.OWNER == oTempExplo.OID).ToList();
            //Esto significa que se quiere crear un nuevo pago
            if (crearNuevoPago )
            {
                //EXPLORACIONES
                if (oPago.CID == 1377)
                {                
                   
                    if (PagoRealizadosExplo.Sum(p => p.CANTIDAD) < oTempExplo.CANTIDAD)
                    {
                        PAGOS oPagoNuevo = new PAGOS
                        {
                            APLAZADO = "F",
                            BORRADO = "F",
                            CANTIDAD =0,
                            CID = oPago.CID,
                            DEUDA_CANTIDAD = oTempExplo.CANTIDAD,
                            FECHA = DateTime.Now,
                            DEUDA_FECHA = oTempExplo.FECHA,
                            IOR_EMPRESA = 4,
                            IOR_MONEDA = int.Parse(oConfig.ObtenerValor("IOR_MONEDA")),
                            MONEDA = MonedaRepositorio.Obtener(int.Parse(oConfig.ObtenerValor("IOR_MONEDA"))),
                            OWNER = oTempExplo.OID,
                            TIPOPAGO = "null"
                        };
                        if (oPagoNuevo.DEUDA_FECHA != DateTime.Today)
                        {
                            oPagoNuevo.APLAZADO = "T";
                        }
                      oid=  PagosRepositorio.Insertar(oPagoNuevo);
                    }


                }
                else
                {
                    var oConsumible = db.Exp_Consum.Where(p => p.OID == oPago.OWNER).FirstOrDefault();
                    PagoRealizadosExplo = db.Pagos.Where(p => p.OWNER == oConsumible.OID).ToList();
                    oTempExplo = ListaDiaRepositorio.Obtener(oConsumible.IOR_EXPLORACION.Value);
                    if (PagoRealizadosExplo.Sum(p => p.CANTIDAD) < oConsumible.PRECIO)
                    {
                        PAGOS oPagoNuevo = new PAGOS
                        {
                            APLAZADO = "F",
                            BORRADO = "F",
                            CANTIDAD = 0,
                            CID = oPago.CID,
                            DEUDA_CANTIDAD = oConsumible.PRECIO,
                            FECHA = DateTime.Now,
                            DEUDA_FECHA = oTempExplo.FECHA,
                            IOR_EMPRESA = 4,
                            IOR_MONEDA = int.Parse(oConfig.ObtenerValor("IOR_MONEDA")),
                            MONEDA = MonedaRepositorio.Obtener(int.Parse(oConfig.ObtenerValor("IOR_MONEDA"))),
                            OWNER = oConsumible.OID,
                            TIPOPAGO = "null"
                        };
                        if (oPagoNuevo.DEUDA_FECHA != DateTime.Today)
                        {
                            oPagoNuevo.APLAZADO = "T";
                        }
                        oid = PagosRepositorio.Insertar(oPagoNuevo);
                    }
                }
   
            }
            PAGO_PACIENTE oResult;
            var pago = db.Pagos.Single(p => p.OID == oid);
            string queryPago = "Select * from pago_paciente ("
                + pago.DEUDA_FECHA.Value.ToString("MM/dd/yyyy").QuotedString() + "," + ior_paciente + " ) where oidpago='" + oid +"'";
            oResult = db.Database.SqlQuery<PAGO_PACIENTE>(queryPago).SingleOrDefault();
            if (oResult.PAGO=="0")
            {
                oResult.PAGO = calculaDeuda(oResult.OIDPAGO).ToString();
            }
            
            oResult.IOR_PACIENTE = ior_paciente;
            return PartialView("_PagoSimple", oResult);
        }

        [HttpPost]
        public ActionResult PagoSimple(PAGO_PACIENTE oModel)
        {
            WebConfigRepositorio oConfig = new WebConfigRepositorio();
            var oPago = db.Pagos.Where(p => p.OID == oModel.OIDPAGO).FirstOrDefault();
            
            //si estan pagando mediante el pago rapido y la fecha de la deuda es anterior a hoy
            if (oPago.DEUDA_FECHA != DateTime.Today)
            {
                oPago.APLAZADO = "T";
            }
            oPago.TIPOPAGO = oModel.TIPOPAGO;

            if (oPago.CANTIDAD > oPago.DEUDA_CANTIDAD)
            {
                oPago.CANTIDAD = calculaDeuda(oModel.OIDPAGO);
            }
            else
            {
                oPago.CANTIDAD = float.Parse(oModel.PAGO);
            }
            //En el procedure realizado por massana se listan tanto las exploraciones como los consumibles en una sola venta
            //y para diferenciarlos y ordenarlos se creo el campo orden
            if (oModel.ORDEN=="1" )//1 - EXPLORACION 2- CONSUMIBLE
            {
                EXPLORACION oExploracion = ExploracionRepositorio.Obtener(oPago.OWNER);
                oPago.IOR_EMPRESA = oExploracion.IOR_EMPRESA;
                oPago.IOR_MONEDA = int.Parse(oConfig.ObtenerValor("IOR_MONEDA"));


                //Si el modelo viene con precio es porque un consumible con precio 0 se ha modificado 
                //su precio
                if (oModel.PRECIO != null && oExploracion.CANTIDAD != double.Parse(oModel.PRECIO))
                {
                    oExploracion.CANTIDAD = float.Parse(oModel.PRECIO);
                    oPago.DEUDA_CANTIDAD = float.Parse(oModel.PRECIO);
                }



                db.SaveChanges();

                oExploracion.PAGADO = (calculaDeuda(oModel.OIDPAGO)==0 ? "T" : "F");
                        oExploracion.APLAZADO = (oPago.DEUDA_FECHA < oPago.FECHA? "T" : "F");

                ExploracionRepositorio.Update(oExploracion);
            }
            else
            {
                var oConsumible = db.Exp_Consum.Where(p => p.OID == oPago.OWNER).FirstOrDefault();
             
                //Si el modelo viene con precio es porque un consumible con precio 0 se ha modificado 
                //su precio
                if (oModel.PRECIO!=null &&  oConsumible.PRECIO!=float.Parse(oModel.PRECIO))
                {
                    oConsumible.PRECIO = float.Parse(oModel.PRECIO);
                   oPago.DEUDA_CANTIDAD = float.Parse(oModel.PRECIO);                   
                }

                var PagoRealizadosExplo = db.Pagos.Where(p => p.OWNER == oConsumible.OID).ToList();
                oConsumible.PAGADO = (PagoRealizadosExplo.Sum(p => p.CANTIDAD) >= oConsumible.PRECIO  ? "T" : "F");
                oConsumible.APLAZADO = (oPago.DEUDA_FECHA < oPago.FECHA ? "T" : "F");

                db.SaveChanges();
            }
            
            return RedirectToAction("Index", "Pagos2", new
            {
                fecha = oModel.FECHA.Value.ToString("dd/MM/yyyy"),
                ior_paciente = oModel.IOR_PACIENTE
            });
        }


        [HttpGet]
        public ActionResult PagarTodo(string fecha, int ior_paciente)
        {
            string queryPago = "Select * from pago_paciente (" + DateTime.Parse(fecha).ToString("MM/dd/yyyy").QuotedString() + "," + ior_paciente + " )";

            IEnumerable<PAGO_PACIENTE> oResult = db.Database
                        .SqlQuery<PAGO_PACIENTE>(queryPago)
                        .ToList<PAGO_PACIENTE>();
            oResult.First().FECHA = DateTime.Now;
            return PartialView("_PagoTotal", oResult.First());
        }

        [HttpPost]
        public ActionResult PagarTodo(string FECHA,string FECHAPAGO, int IOR_PACIENTE,string TIPOPAGO)
        {
            if (!ModelState.IsValid)
            {
                string queryPagoError = "Select * from pago_paciente (" + DateTime.Parse(FECHA).ToString("MM/dd/yyyy").QuotedString() + "," + IOR_PACIENTE + " )";
                IEnumerable<PAGO_PACIENTE> oResultError = db.Database
                        .SqlQuery<PAGO_PACIENTE>(queryPagoError)
                        .ToList<PAGO_PACIENTE>();
                return PartialView("_PagoTotal", oResultError.First());
            }
                WebConfigRepositorio oConfig = new WebConfigRepositorio();
            string queryPago = "Select * from pago_paciente ("  + DateTime.Parse(FECHA).ToString("MM/dd/yyyy").QuotedString() + "," + IOR_PACIENTE + " )";

            IEnumerable<PAGO_PACIENTE> oResult = db.Database
                        .SqlQuery<PAGO_PACIENTE>(queryPago)
                        .ToList<PAGO_PACIENTE>();

            foreach (var item in oResult.Where(p=>p.PAGADO=="F" 
                            && p.TIPO=="P" && p.PAGAR=="T"))
            {
                double Deuda = calculaDeuda(item.OIDPAGO);
                bool espagoAplazado = false;
                if (Deuda > 0)
                {
                    var oPago = db.Pagos.Where(p => p.OID == item.OIDPAGO).FirstOrDefault();
                 
                    //si estan pagando mediante el pago rapido y la fecha de la deuda es anterior a hoy
                    if (oPago.DEUDA_FECHA < DateTime.Parse(FECHAPAGO))
                    {
                        PAGOS oPagoTemp = oPago;
                        oPago = new PAGOS
                        {
                            APLAZADO = "T",
                            FECHA = DateTime.Parse(FECHAPAGO),
                            OWNER = oPago.OWNER,
                            CANTIDAD = Deuda,
                            DEUDA_FECHA= oPagoTemp.DEUDA_FECHA,
                            BORRADO=oPagoTemp.BORRADO,
                            CID=oPagoTemp.CID,
                            DEUDA_CANTIDAD= float.Parse(item.PRECIO)

                         };
                        espagoAplazado = true;
                    }
                    else
                    {
                        oPago.CANTIDAD = float.Parse(item.PRECIO);
                    }
                    oPago.TIPOPAGO = TIPOPAGO;                  
                    EXPLORACION oExploracion = ExploracionRepositorio.Obtener(oPago.OWNER);
                    oPago.IOR_EMPRESA = oExploracion.IOR_EMPRESA;
                    oPago.IOR_MONEDA = int.Parse(oConfig.ObtenerValor("IOR_MONEDA"));
                    if (espagoAplazado)
                    {
                        db.Pagos.Add(oPago);
                    }
                    else
                    {
                        db.Entry(oPago).State = EntityState.Modified;
                    }
                    db.SaveChanges();
                    //En el procedure realizado por massana se listan tanto las exploraciones como los consumibles en una sola venta
                    //y para diferenciarlos y ordenarlos se creo el campo orden
                    if (item.ORDEN == "1")//1 - EXPLORACION 2- CONSUMIBLE
                    {                      
                        var PagoRealizadosExplo = db.Pagos.Where(p => p.OWNER == oExploracion.OID).ToList();
                        oExploracion.PAGADO = (PagoRealizadosExplo.Sum(p => p.CANTIDAD) >= oExploracion.CANTIDAD ? "T" : "F");
                        oExploracion.APLAZADO = (oPago.DEUDA_FECHA < oPago.FECHA? "T" : "F");
                        ExploracionRepositorio.Update(oExploracion);
                    }
                    else
                    {
                        var oConsumible = db.Exp_Consum.Where(p => p.OID == oPago.OWNER).FirstOrDefault();
                        var PagoRealizadosExplo = db.Pagos.Where(p => p.OWNER == oConsumible.OID).ToList();
                        oConsumible.PAGADO = (PagoRealizadosExplo.Sum(p => p.CANTIDAD) >= oConsumible.PRECIO ? "T" : "F");
                    }
                   
                }
               

            }
           
            
            db.SaveChanges();
            return RedirectToAction("Index", "Pagos2", new
            {
                fecha = FECHA,
                ior_paciente = IOR_PACIENTE
            });
        }


       
     

        // POST: Pagos2/Delete/5
        [HttpPost]
        public ActionResult Delete(int oid)
        {
            try
            {
                // TODO: Add delete logic here
                var oPago = db.Pagos.Single(p => p.OID == oid);
                if (oPago.CID == 1377)//si es una exploracion
                {
                    EXPLORACION oExploracionDelPago = ExploracionRepositorio.Obtener(oPago.OWNER);
                    var pagosAsociadosaEstaExploracion = db.Pagos.Where(p => p.OWNER == oExploracionDelPago.OID
                                                      && p.DEUDA_FECHA == oExploracionDelPago.FECHA);
                    if (pagosAsociadosaEstaExploracion.Count() == 1)
                    {
                        return new HttpStatusCodeResult(HttpStatusCode.InternalServerError, "IMPOSIBLE BORRAR. DEBE HABER AL MENOS UN PAGO ASOCIADO A LA EXPLORACIÓN");

                    }
                    else
                    {
                        ExploracionRepositorio.UpdateCampo("PAGADO", (pagosAsociadosaEstaExploracion
                                            .Where(p => p.OID != oid).Sum(p => p.CANTIDAD) >= oExploracionDelPago.CANTIDAD ? "T" : "F"), oExploracionDelPago.OID);

                    }

                }
                else
                {
                    EXP_CONSUM oExpConsum = Exp_ConsumRepositorio.Obtener(oPago.OWNER);
                    var pagosAsociadosaConsumible = db.Pagos.Where(p => p.OWNER == oExpConsum.OID);
                    if (pagosAsociadosaConsumible.Count() == 1)
                    {
                        return new HttpStatusCodeResult(HttpStatusCode.InternalServerError, "IMPOSIBLE BORRAR. DEBE HABER AL MENOS UN PAGO ASOCIADO AL CONSUMIBLE");

                    }
                    else
                    {
                        Exp_ConsumRepositorio.UpdateCampo("PAGADO", (pagosAsociadosaConsumible.Where(p=>p.OID!=oid)
                                        .Sum(p => p.CANTIDAD) >= oExpConsum.PRECIO ? "T" : "F"), oExpConsum.OID);

                    }
                }
             
              
                
                db.Pagos.Remove(oPago);
                db.SaveChanges();               
                return new HttpStatusCodeResult(HttpStatusCode.OK, "VINCULADO");

            }
            catch
            {
                return View();
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
                usersDBContext.Dispose();
            }
            base.Dispose(disposing);
        }


    }
}
