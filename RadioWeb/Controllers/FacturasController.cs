using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using RadioWeb.Models;
using RadioWeb.Models.Repos;
using RadioWeb.ViewModels;
using System.IO;
using ADPM.Common;
using RadioWeb.Repositories;
using RadioWeb.Filters;

namespace RadioWeb.Controllers
{
    [Authorize]
    public class FacturasController : Controller
    {
        private RadioDBContext db = new RadioDBContext();



        private UsersDBContext usersDBContext = new UsersDBContext();


        // GET: Facturas
        public double NumeroFactura(string serie)
        {
            var numeroFactura = db.Facturas
                     .Where(f => f.FECHA_FAC.Year == DateTime.Now.Year
                           && f.COD1 == serie.ToUpper()
                           && f.IOR_EMPRESA == 4)
                     .Max(f => f.NUM_FAC) + 1;

            if (numeroFactura == null)
                return 1;


            return numeroFactura.Value;
        }

        [HttpPost]
        public ActionResult Borrar(int oid)
        {
            
            FACTURAS oFactura = db.Facturas.Single(f => f.OID ==oid);          
            db.Facturas.Remove(oFactura);
            db.SaveChanges();
            return new HttpStatusCodeResult(HttpStatusCode.OK);
        }

        [HttpPost]
        public ActionResult BorrarLineaFactura(int oid)
        {
            LINEAS_FACTURAS lineaFactura = db.Lineas_Facturas.Find(oid);
            FACTURAS oFactura = db.Facturas.Single(f => f.OID == lineaFactura.OWNER);
            oFactura.SUMA = oFactura.SUMA - (float)lineaFactura.PRECIO;
            ExploracionRepositorio.UpdateCampo("IOR_FACTURA", "-1", lineaFactura.CID.Value);
            ExploracionRepositorio.UpdateCampo("FACTURADA", "F", lineaFactura.CID.Value);
            Exp_ConsumRepositorio.UpdateCampo("IOR_FACTURA", "-1", lineaFactura.CID.Value);
            Exp_ConsumRepositorio.UpdateCampo("FACTURADO", "F", lineaFactura.CID.Value);
            db.Entry(oFactura).State = EntityState.Modified;
            db.Lineas_Facturas.Remove(lineaFactura);
            db.SaveChanges();
            return new HttpStatusCodeResult(HttpStatusCode.OK);
        }


        // GET: Facturas
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Buscar(int year)
        {
            try
            {
                var listaFacturas = db.Facturas.Where(f => f.FECHA_FAC.Year == year)
           .OrderBy(f => f.COD1).ThenByDescending(f => f.NUM_FAC).ToList();
                var serializer = new System.Web.Script.Serialization.JavaScriptSerializer();
                serializer.MaxJsonLength = 500000000;

                var json = Json(listaFacturas, JsonRequestBehavior.AllowGet);
                json.MaxJsonLength = 500000000;


                return json;
            }
            catch (Exception)
            {

                throw;
            }



        }

        public ActionResult Arqueo(string fecha)
        {
            try
            {
                WebConfigRepositorio oConfig = new WebConfigRepositorio();
                string ior_moneda = oConfig.ObtenerValor("IOR_MONEDA").ToUpper();

                string query = "select p.fecha,r.descripcion as descripcionconcepto,p.aplazado,count(p.cid) as CuentaConcepto,e.owner, sum(p.cantidad) as CantidadSumaConcepto from pagos p join ref_class r on r.oid = p.cid join monedas_valor v " +
                    " on (v.owner = p.ior_moneda and v.ior_moneda = " + ior_moneda + ")" +
                    " left join EXPLORACION e on e.OID = p.OWNER left join EXP_CONSUM x on x.OID = p.OWNER" +
                    " left join EXPLORACION c on c.OID = x.IOR_EXPLORACION" +
                    " where p.fecha = '" + DateTime.Parse(fecha).ToString("MM-dd-yyyy") + "' and p.cantidad > 0" +
                    " and(p.borrado <> 'T' or p.borrado is null)" +
                    " group by p.fecha,r.descripcion,p.aplazado,e.owner ";

                IEnumerable<VWArqueo> oResult;

                oResult = db.Database.SqlQuery<VWArqueo>(query).ToList<VWArqueo>();

                return Json(oResult, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {

                throw;
            }
        }

        private List<LINEAS_FACTURAS> GetLineasFacturaPrivada(int ior_exploracion)
        {
            WebConfigRepositorio oConfig = new WebConfigRepositorio();

            List<LINEAS_FACTURAS> listaLineas = new List<LINEAS_FACTURAS>();
            EXPLORACION oExploracion = ExploracionRepositorio.Obtener(ior_exploracion);
            bool pagoAntesConfirmacion = (oConfig.ObtenerValor("PagoAntesExploracion") == "T" ? true : false);


            if (oExploracion.FACTURADA!="T" 
                & oExploracion.PAGADO=="T" && oExploracion.PAGAR == "T" && oExploracion.IOR_GPR== 1)
            {
                listaLineas.Add(new LINEAS_FACTURAS
                {
                    TIPOLINEA = "EXPLORACION",
                    TEXTO = oExploracion.APARATO.DES_FIL,
                    PRECIO = oExploracion.CANTIDAD,
                    IOR_EXPLORACION = oExploracion.OID,
                    PERMISO="T"
                });
            }


            foreach (var expConsumible in Exp_ConsumRepositorio.GetConsumiblesPendientes(oExploracion.OID))
            {
                if (expConsumible.FACTURADO != "T" && expConsumible.PAGAR == "T" && expConsumible.PAGADO=="T" && expConsumible.ENTIDADPAGADORA.OWNER == 1)
                {
                    CONSUMIBLES oConsum = ConsumibleRepositorio.Obtener(expConsumible.IOR_CONSUM.Value);
                    listaLineas.Add(
                      new LINEAS_FACTURAS
                      {
                          TIPOLINEA = "CONSUMIBLE",
                          TEXTO = oConsum.DES_CONSUM,
                          PRECIO = expConsumible.PRECIO,
                          IOR_EXPLORACION = expConsumible.IOR_EXPLORACION.Value,
                          IOR_EXPCONSUMIBLE = expConsumible.OID,
                          PERMISO = "T"
                      });
                }
            }

            //Despues vamos a agregar otras exploraciones hechas durante el mismo dia
            IEnumerable<LISTADIA> otraExploracionesMismoDia = new List<LISTADIA>();
            if (!pagoAntesConfirmacion)
            {
                otraExploracionesMismoDia = ListaDiaRepositorio.ObtenerPorPacienteYFecha(oExploracion.OID,
                                       oExploracion.PACIENTE.OID,
                                       (DateTime)oExploracion.FECHA).Where(e => e.IOR_GPR == 1 &&
                                       e.ESTADO == "3" && e.FACTURADA == false);
            }
            else
            {
                otraExploracionesMismoDia = ListaDiaRepositorio.ObtenerPorPacienteYFecha(oExploracion.OID,
                                       oExploracion.PACIENTE.OID,
                                       (DateTime)oExploracion.FECHA).Where(e => e.IOR_GPR == 1 &&
                                       e.ESTADO == "3" && e.FACTURADA == false || e.ESTADO == "2");

            }


            foreach (LISTADIA item in otraExploracionesMismoDia)
            {
                EXPLORACION oTemp = ExploracionRepositorio.Obtener(item.OID);

                if (oTemp.PAGAR == "T" && oTemp.FACTURADA=="F" && oTemp.PAGADO=="T" && oExploracion.ENTIDAD_PAGADORA.OWNER == 1)
                {
                    listaLineas.Add(
                       new LINEAS_FACTURAS
                       {
                           TIPOLINEA = "EXPLORACION",
                           TEXTO = oTemp.APARATO.DES_FIL,
                           PRECIO = item.CANTIDAD,
                           IOR_EXPLORACION = oTemp.OID,
                           PERMISO = "T"
                       });
                }
                foreach (var expConsumible in Exp_ConsumRepositorio.GetConsumiblesPendientes(item.OID))
                {
                    if (expConsumible.FACTURADO!="T" && expConsumible.PAGADO == "T"  && expConsumible.PAGAR == "T" && expConsumible.ENTIDADPAGADORA.OWNER == 1)
                    {
                        CONSUMIBLES oConsum = ConsumibleRepositorio.Obtener(expConsumible.IOR_CONSUM.Value);
                        listaLineas.Add(
                          new LINEAS_FACTURAS
                          {
                              TIPOLINEA = "CONSUMIBLE",
                              TEXTO = oConsum.DES_CONSUM,
                              PRECIO = expConsumible.PRECIO,
                              IOR_EXPLORACION = expConsumible.IOR_EXPLORACION.Value,
                              IOR_EXPCONSUMIBLE = expConsumible.OID,
                              PERMISO = "T"
                          });
                    }
                }
            }

            return listaLineas;
        }

        [HttpPost]
        public ActionResult Desbloquear(int oid)
        {
            FACTURAS oFactura = db.Facturas.Single(f => f.OID == oid);
            oFactura.SUMA = 0;
            oFactura.EURO_SUMA = 0;
            oFactura.COD3 = "F";
            foreach (var item in ExploracionRepositorio.ObtenerPorFactura(oFactura.OID))
            {
                ExploracionRepositorio.UpdateCampo("IOR_FACTURA", "-1", item.OID, "int");
                ExploracionRepositorio.UpdateCampo("FACTURADA", "F", item.OID);
            }
            db.Entry(oFactura).State = EntityState.Modified;
            db.SaveChanges();
            return new HttpStatusCodeResult(HttpStatusCode.OK);

        }

        public ActionResult Lineas(int oid, int ior_exploracion, string fecha_inicial = "", string fecha_final = "", int ior_entidadPagadora = -1)
        {
            List<LINEAS_FACTURAS> listaLineas = new List<LINEAS_FACTURAS>();
            try
            {
                FACTURAS oFactura = db.Facturas.SingleOrDefault(f => f.OID == oid);

                //primero miramos si es una factura de mutua ya existente
                if (oFactura != null && oFactura.IOR_GPR == 2)//Factura de mutua
                {
                    //si la factura está bloqueada tenemos que traernos las lineas de factura con facturado igual a True y con
                    //las exploraciones que tienen el campo ior_factura igual al número de factura
                    if (oFactura.COD3 == "T")
                    {
                        listaLineas = ExploracionRepositorio.ObtenerPorFactura(oFactura.OID)
                            .Where(p => p.FACTURADA == "T")
                            .Select(explo => new LINEAS_FACTURAS
                            {
                                PACIENTE = explo.PACIENTE.PACIENTE1,
                                FECHA = explo.FECHA.Value.ToShortDateString(),
                                TEXTO = explo.TIPOEXPLORACIONDESC,
                                SIMBOLO = explo.MONEDA.SIMBOLO,
                                PERMISO = explo.PERMISO,
                                FACTURADA = explo.FACTURADA,
                                AUTORIZACION = explo.NHCAP,
                                FECHA_FAC = explo.FECHA_FAC.Value.ToShortDateString(),
                                IOR_EXPLORACION = explo.OID,
                                PRECIO = explo.CANTIDAD,
                                OWNER = explo.IOR_FACTURA

                            }).ToList();
                    }
                    else
                    {
                        listaLineas = ExploracionRepositorio.ObtenerParaFactura(fecha_inicial, fecha_final, oFactura.IOR_ENTIDADPAGADORA.Value)
                            .Where(p => p.FACTURADA != "T")
                            .Select(explo => new LINEAS_FACTURAS
                            {
                                OID = explo.OID,
                                PACIENTE = explo.PACIENTE.PACIENTE1,
                                FECHA = explo.FECHA.Value.ToShortDateString(),
                                TEXTO = explo.TIPOEXPLORACIONDESC,
                                SIMBOLO = explo.MONEDA.SIMBOLO,
                                FACTURADA = explo.FACTURADA,
                                PERMISO = explo.PERMISO,
                                AUTORIZACION = explo.NHCAP,
                                FECHA_FAC = explo.FECHA_FAC.Value.ToShortDateString(),
                                IOR_EXPLORACION = explo.OID,
                                PRECIO = explo.CANTIDAD,
                                OWNER=explo.IOR_FACTURA,
                                ESTADO=explo.ESTADO

                            }).ToList();

                    }


                }
                else if (ior_entidadPagadora > 0)
                {
                    listaLineas = ExploracionRepositorio.ObtenerParaFactura(fecha_inicial, fecha_final, ior_entidadPagadora)
                        .Where(p => p.FACTURADA != "T")
                        .Select(explo => new LINEAS_FACTURAS
                        {
                            OID = explo.OID,
                            PACIENTE = explo.PACIENTE.PACIENTE1,
                            FECHA = explo.FECHA.Value.ToShortDateString(),
                            TEXTO = explo.TIPOEXPLORACIONDESC,
                            SIMBOLO = explo.MONEDA.SIMBOLO,
                            PERMISO = explo.PERMISO,
                            AUTORIZACION = explo.NHCAP,
                            FECHA_FAC = explo.FECHA_FAC.Value.ToShortDateString(),
                            IOR_EXPLORACION = explo.OID,
                            PRECIO = explo.CANTIDAD,
                            OWNER = explo.IOR_FACTURA,
                             ESTADO = explo.ESTADO
                        }).ToList();
                }
                else
                {
                    //PRIMERO MIRAMOS SI LLEGAMOS AQUI CONSULTANDO UNA FACTURA YA EXISTENTE
                    listaLineas = db.Lineas_Facturas
                       .Where(f => f.OWNER == oid)
                       .OrderBy(f => f.OID).ToList();
                    //si nos han pasado un oid de factura menor a 0 y no nos pasan oid de exploracion
                    //creamos una linea de factura en blanco
                    if (oid <= 0 && ior_exploracion <= 0)
                    {
                        //si no existe ninguna linea de factura NUEVA es porque estamos llegando a una factura en blanco
                        listaLineas.Add(new LINEAS_FACTURAS
                        {
                            TEXTO = "",
                            PRECIO = 0,
                            OID = -1,
                            PERMISO="T"
                        });
                    }
                    //Si nos pasan un OID DE EXPLORACION pero no hemos encontrado lineas de Facturas
                    //significa que es una factura nueva. 
                    if (ior_exploracion > 0 && oid <= 0 && listaLineas.Count == 0)
                    {
                        listaLineas = GetLineasFacturaPrivada(ior_exploracion);

                    }


                    for (int i = 0; i < listaLineas.Count; i++)
                    {
                        listaLineas[i].INDEX = i;
                    }

                }




            }
            catch (Exception)
            {

                throw;
            }

            return Json(listaLineas, JsonRequestBehavior.AllowGet);

        }


        [ConfiguracionVisualAttribute]
        public ActionResult CreateOrEdit(int ior_factura, int ior_exploracion = -1, int ior_paciente = -1, int ior_gpr = 1,string url_previa="")
        {
            WebConfigRepositorio oConfig = new WebConfigRepositorio();
            string ior_moneda = oConfig.ObtenerValor("IOR_MONEDA").ToUpper();
            VWFactura oViewModel = new VWFactura();
            oViewModel.HEADING = "Nueva Factura";
            oViewModel.OID = ior_factura;
            oViewModel.IOR_EXPLORACION = ior_exploracion;
            oViewModel.IOR_PAC = ior_paciente;
            oViewModel.FECHA_FAC = DateTime.Now;
            oViewModel.IOR_MONEDA = int.Parse(ior_moneda);
            if (String.IsNullOrEmpty( url_previa))
            {
                oViewModel.URLPREVIA = Request.UrlReferrer.PathAndQuery;

            }
            else
            {
                oViewModel.URLPREVIA =url_previa;

            }
            oViewModel.SIMBOLO = MonedaRepositorio.Obtener(int.Parse(ior_moneda)).SIMBOLO;
            oViewModel.NOMBREEMPRESA = oConfig.ObtenerValor("NOMBREEMPRESA");

                  if (oViewModel.NOMBREEMPRESA.ToUpper().Contains("DELFOS"))
            {
                // oViewModel.OWNER = DaparatoRepositorio.Obtener(oExplo.DAPARATO.OID).CID.Value;

                if (oViewModel.OWNER == 1)
                {
                    oViewModel.CIFEMPRESA = "B61579520";
                    oViewModel.EMPRESA = "Gabinete Nuclear Delfos";
                }
                else
                {
                    oViewModel.CIFEMPRESA = "A61150991";
                    oViewModel.EMPRESA = "Servei Imatge Molecular i Metabòlic";
                }

            }
            //Si el ior_gpr es 2 significa que quieren hacer una factura nueva de mutua
            if (ior_gpr == 2)
            {
                oViewModel.IOR_PAC = -1;
                oViewModel.IOR_GPR = 2;
                oViewModel.FECHA_INICIAL = DateTime.Now.AddDays(-30);
                oViewModel.FECHA_FINAL = DateTime.Now;
            }
            //si nos pasan un oid de factura significa que quieren editar una factura existente
            if (ior_factura > 0)
            {
                oViewModel.HEADING = "Editar Fáctura";
                FACTURAS oFactura = db.Facturas.Single(f => f.OID == ior_factura);
                oViewModel.FECHA_FAC = oFactura.FECHA_FAC;

                oViewModel.NOMBRE = oFactura.NOMBRE;
                if (oFactura.IOR_PAC.HasValue && oFactura.IOR_PAC > 0)
                {
                    oViewModel.PACIENTE = PacienteRepositorio.Obtener(oFactura.IOR_PAC.Value).PACIENTE1;
                }

                if (oFactura.IOR_ENTIDADPAGADORA.HasValue && oFactura.IOR_ENTIDADPAGADORA.Value > 0)
                {
                    oViewModel.PACIENTE = MutuasRepositorio.Obtener(oFactura.IOR_ENTIDADPAGADORA.Value).NOMBRE;
                }
                //eSTO ES EL CENTRO ASOCIADO A LA FACTURA, EN SIMM SE UTILZA
                oViewModel.OWNER = oFactura.OWNER;
                oViewModel.IOR_GPR = oFactura.IOR_GPR;
                oViewModel.NUM_FAC = oFactura.NUM_FAC;
                oViewModel.DNI = oFactura.DNI;
                oViewModel.DIRECCION = oFactura.DIRECCION;
                oViewModel.CP = oFactura.CP;
                oViewModel.CIUDAD = oFactura.CIUDAD;
                oViewModel.PROVINCIA = oFactura.PROVINCIA;
                oViewModel.COD1 = oFactura.COD1;
                oViewModel.COD2 = oFactura.COD2;
                //SABER SI UNA FACTURA YA ESTÁ CALCULADA
                oViewModel.COD3 = oFactura.COD3;
                oViewModel.IVA = (oFactura.IVA1.HasValue ? oFactura.IVA1.Value : 0);
                //si la factura es privada
                if (oFactura.IOR_GPR == 1)
                {
                    var totalSinIVA =(float?) db.Lineas_Facturas
                              .Where(f => f.OWNER == ior_factura).Sum(p => p.PRECIO);
                    oViewModel.TOTALSINIVA = (totalSinIVA.HasValue ? totalSinIVA.Value : 0).ToString();
                    oViewModel.TOTALCONIVA = oFactura.SUMA.Value.ToString();
                    oViewModel.IVAIMPUTADO = (float.Parse( oViewModel.TOTALCONIVA) - float.Parse(oViewModel.TOTALSINIVA)).ToString();
                    oViewModel.IOR_PAC = oFactura.IOR_PAC;
                    List<EXPLORACION> oExploracionFactura = ExploracionRepositorio.ObtenerPorFactura(oFactura.OID);
                    if (oExploracionFactura.Count > 0)
                    {
                        oViewModel.IOR_EXPLORACION = oExploracionFactura.First().OID;
                    }
                    if (oFactura.FECHA.HasValue)
                    {
                        oViewModel.FECHAEXPLORACION = oFactura.FECHA.Value;
                    }
                }
                else
                {
                    oViewModel.TOTALCONIVA = oFactura.SUMA.Value.ToString();
                    oViewModel.FECHA_INICIAL = oFactura.FECHA_INICIAL.Value;
                    oViewModel.FECHA_FINAL = oFactura.FECHA_FINAL.Value;
                }

                oViewModel.COMENTARIO = TextosRepositorio.Obtener(oFactura.OID).TEXTO;
            }
            //si nos pasan un oid de exploracion quiere decir que es un insert de una factura de exploración que no existe.           
            if (ior_exploracion > 0)
            {
                EXPLORACION oExplo = ExploracionRepositorio.Obtener(ior_exploracion);
                //if (oExplo.FACTURADA == "T" && oExplo.IOR_FACTURA.HasValue)
                //{
                //    return RedirectToAction("CreateOrEdit", new
                //    {
                //        ior_factura = oExplo.IOR_FACTURA
                //    });
                //}
                ViewBag.UltimaSerieUsuario = ViewData["FACTURAS.COD1"];
                oViewModel.FECHAEXPLORACION = oExplo.FECHA.Value;
                oViewModel.PACIENTE = oExplo.PACIENTE.PACIENTE1;
                oViewModel.NOMBRE = oExplo.PACIENTE.PACIENTE1;
                oViewModel.IOR_PAC = oExplo.IOR_PACIENTE;
                oViewModel.DNI = oExplo.PACIENTE.DNI;
                if (oExplo.PACIENTE.DIRECCIONES.Count> 0)
                {
                oViewModel.DIRECCION = oExplo.PACIENTE.DIRECCIONES.FirstOrDefault().DIRECCION1;
                oViewModel.CP = oExplo.PACIENTE.DIRECCIONES.FirstOrDefault().CP;
                oViewModel.CIUDAD = oExplo.PACIENTE.DIRECCIONES.FirstOrDefault().POBLACION;
                oViewModel.PROVINCIA = oExplo.PACIENTE.DIRECCIONES.FirstOrDefault().PROVINCIA;
                }
                oViewModel.IOR_MONEDA = oExplo.IOR_MONEDA;
                oViewModel.IOR_GPR = oExplo.IOR_GPR;
                oViewModel.NOMBREEMPRESA = oConfig.ObtenerValor("NOMBREEMPRESA");
                if (oViewModel.NOMBREEMPRESA.ToUpper().Contains("DELFOS"))
                {
                    oViewModel.OWNER = oExplo.OWNER;// DaparatoRepositorio.Obtener(oExplo.DAPARATO.OID).CID.Value;
                    if (oViewModel.OWNER == 1)
                    {
                        oViewModel.CIFEMPRESA = "B61579520";
                        oViewModel.EMPRESA = "Gabinete Nuclear Delfos";
                    }
                    else
                    {
                        oViewModel.CIFEMPRESA = "A61150991";
                        oViewModel.EMPRESA = "Servei Imatge Molecular i Metabòlic";
                    }

                }
                //PRIMERO MIRAMOS SI LLEGAMOS AQUI CONSULTANDO UNA FACTURA YA EXISTENTE
                List<LINEAS_FACTURAS> listaLineas = GetLineasFacturaPrivada(ior_exploracion);
                oViewModel.LINEAS = listaLineas;
                oViewModel.IVA = 0;
                if (listaLineas.Count > 0)
                {
                 oViewModel.TOTALSINIVA = listaLineas.Sum(p => p.PRECIO).Value.ToString();
                 oViewModel.TOTALCONIVA = (float.Parse( oViewModel.TOTALSINIVA) * (1 + (oViewModel.IVA / 100))).ToString();
                    oViewModel.IVAIMPUTADO = (float.Parse( oViewModel.TOTALCONIVA) - float.Parse(oViewModel.TOTALSINIVA)).ToString();
                }
                else
                {

                    oViewModel.TOTALSINIVA = "0";
                    oViewModel.TOTALCONIVA ="0";
                    oViewModel.IVAIMPUTADO ="0";
                }

  

            }
            return View("Create", oViewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateOrEdit(VWFactura viewModelFac)
        {
            if (!ModelState.IsValid)
            {
                EmpresaRepositorio oRepoEmpresa = new EmpresaRepositorio();
                viewModelFac.EMPRESA = oRepoEmpresa.Obtener(viewModelFac.IOR_EMPRESA.Value).NOMBRE;
                //viewModelFac.TOTALSINIVA = viewModelFac.LINEAS.Sum(p => p.PRECIO).Value.ToString();
                //viewModelFac.TOTALCONIVA = viewModelFac.TOTALSINIVA * (1 + (viewModelFac.IVA / 100));
                //viewModelFac.IVAIMPUTADO = viewModelFac.TOTALCONIVA - viewModelFac.TOTALSINIVA;
                if (viewModelFac.IOR_PAC.HasValue && viewModelFac.IOR_PAC > 0)
                {
                    viewModelFac.PACIENTE = PacienteRepositorio.Obtener(viewModelFac.IOR_PAC.Value).PACIENTE1;
                }

                if (viewModelFac.IOR_ENTIDADPAGADORA.HasValue && viewModelFac.IOR_ENTIDADPAGADORA.Value > 0)
                {
                    viewModelFac.PACIENTE = MutuasRepositorio.Obtener(viewModelFac.IOR_ENTIDADPAGADORA.Value).NOMBRE;
                }
                foreach (ModelState modelState in ViewData.ModelState.Values)
                {
                    foreach (ModelError error in modelState.Errors)
                    {
                        if (1==1)
                        {

                        }
                    }
                }
                return View("Create", viewModelFac);
            }
            FACTURAS factura;
            
            //Si es una creacion de factura privada entramos  calculamos el proximo número de factura libre
            if (viewModelFac.ACTION == "Create")
            {
                var numeroFactura = NumeroFactura(viewModelFac.COD1);
                //Esta query solo puede fallar a principio de año
                //en ese caso entramos por else
                if (numeroFactura != null)
                {
                    viewModelFac.NUM_FAC = numeroFactura;
                }
                else
                {
                    viewModelFac.NUM_FAC = 1;
                }
                //Creamos la factura en la tabla facturas
                factura = new FACTURAS
                {
                    BORRADO = "F",
                    FECHA_FAC = viewModelFac.FECHA_FAC,
                    CIUDAD = viewModelFac.CIUDAD,
                    COD1 = viewModelFac.COD1.ToUpper(),
                    FAC = "T",
                    IVA1 = viewModelFac.IVA,
                    IOR_EMPRESA = viewModelFac.IOR_EMPRESA,
                    NOMBRE = viewModelFac.NOMBRE,
                    DIRECCION = viewModelFac.DIRECCION,
                    CP = viewModelFac.CP,
                    PROVINCIA = viewModelFac.PROVINCIA,
                    NUM_FAC = viewModelFac.NUM_FAC,
                    DNI = viewModelFac.DNI,
                    //IOR_PAC = viewModelFac.IOR_PAC,
                    IOR_GPR = viewModelFac.IOR_GPR,
                    OWNER = viewModelFac.OWNER,
                    IOR_MONEDA = viewModelFac.IOR_MONEDA,
                    SUMA = (float)viewModelFac.LINEAS.Where(l => l.PERMISO == "T").Sum(l => l.PRECIO),
                    USERNAME = User.Identity.Name
                };
                if (viewModelFac.IOR_GPR == 1)
                {
                    factura.IOR_PAC = viewModelFac.IOR_PAC;
                }
                if (viewModelFac.IOR_GPR == 2)
                {
                    factura.IOR_ENTIDADPAGADORA = viewModelFac.IOR_ENTIDADPAGADORA;
                    factura.FECHA_INICIAL = viewModelFac.FECHA_INICIAL;
                    factura.FECHA_FINAL = viewModelFac.FECHA_FINAL;
                    factura.COD2 = "T";
                    factura.COD3 = "T";
                    factura.TIPO = "1";

                }
                if (viewModelFac.FECHAEXPLORACION.HasValue)
                {
                    factura.FECHA = viewModelFac.FECHAEXPLORACION.Value;
                }


            }

            else
            {
                factura = db.Facturas.Find(viewModelFac.OID);
                factura.FECHA_FAC = viewModelFac.FECHA_FAC;
                factura.CIUDAD = viewModelFac.CIUDAD;
                factura.COD1 = viewModelFac.COD1;
                factura.COD3 = (viewModelFac.COD3 == "F" ? "T" : "F");
                factura.FAC = "T";
                factura.IVA1 = viewModelFac.IVA;
                factura.IOR_EMPRESA = viewModelFac.IOR_EMPRESA;
                factura.NOMBRE = viewModelFac.NOMBRE;
                factura.DIRECCION = viewModelFac.DIRECCION;
                factura.CP = viewModelFac.CP;
                factura.PROVINCIA = viewModelFac.PROVINCIA;
                factura.NUM_FAC = viewModelFac.NUM_FAC;
                factura.DNI = viewModelFac.DNI;
                factura.IOR_PAC = viewModelFac.IOR_PAC;
                factura.IOR_GPR = viewModelFac.IOR_GPR;
                factura.IOR_MONEDA = viewModelFac.IOR_MONEDA;
                factura.OWNER = viewModelFac.OWNER;
                if (factura.IOR_GPR==1)
                {
                    if (viewModelFac.LINEAS.Count > 0)
                    {
                        viewModelFac.TOTALSINIVA = viewModelFac.LINEAS.Sum(p => p.PRECIO).Value.ToString();
                        viewModelFac.TOTALCONIVA = (float.Parse(viewModelFac.TOTALSINIVA) * (1 + (viewModelFac.IVA / 100))).ToString();
                        factura.SUMA = float.Parse(viewModelFac.TOTALCONIVA);
                    }
                    else
                    {
                        viewModelFac.TOTALSINIVA = viewModelFac.LINEAS.Sum(p => p.PRECIO).Value.ToString();
                        viewModelFac.TOTALCONIVA = (float.Parse(viewModelFac.TOTALSINIVA) * (1 + (viewModelFac.IVA / 100))).ToString();
                        factura.SUMA = float.Parse(viewModelFac.TOTALCONIVA);
                    }
                }
                else
                {
                    if (viewModelFac.LINEAS.Count > 0)
                    {
                        factura.SUMA = (float)viewModelFac.LINEAS.Where(l => l.PERMISO == "T" )
                        .Sum(l => l.PRECIO);
                    }
                    else
                    {
                        factura.SUMA = 0;
                    }
                    

                }
            }

            if (viewModelFac.ACTION == "Create")
            {
                db.Facturas.Add(factura);
            }
            else
            {
                db.Entry(factura).State = EntityState.Modified;
            }


            db.SaveChanges();
            db.Entry(factura).GetDatabaseValues();
            int idFactura = factura.OID;
            //creamos las lineas asociadas a dicha Factura
            foreach (var lineaFactura in viewModelFac.LINEAS.Where(p => p.PERMISO == "T"))
            {

                if (viewModelFac.IOR_GPR == 1)
                {
                    var linea = new LINEAS_FACTURAS
                    {
                        BORRADO = "F",
                        IOR_EXPLORACION = (viewModelFac.IOR_EXPLORACION.HasValue ? viewModelFac.IOR_EXPLORACION.Value : -1),
                        CID = (viewModelFac.IOR_EXPLORACION.HasValue ? viewModelFac.IOR_EXPLORACION.Value : -1),
                        OWNER = idFactura,
                        TEXTO = lineaFactura.TEXTO.ToUpper(),
                        PRECIO = lineaFactura.PRECIO
                    };
                    if (lineaFactura.IOR_EXPCONSUMIBLE > 0)
                    {
                        linea.CID = lineaFactura.IOR_EXPCONSUMIBLE;
                    }
                        if (viewModelFac.ACTION == "Create" || viewModelFac.COD3 != "T" && (lineaFactura.TIPOLINEA!=null))
                    {
                        
                        db.Lineas_Facturas.Add(linea);
                        if (lineaFactura.IOR_EXPCONSUMIBLE > 0)
                        {
                            Exp_ConsumRepositorio.UpdateCampo("FACTURADO", "T", lineaFactura.IOR_EXPCONSUMIBLE);
                            Exp_ConsumRepositorio.UpdateCampo("IOR_FACTURA", factura.OID.ToString(), lineaFactura.IOR_EXPCONSUMIBLE, "int");
                        }
                        else
                        {
                            ExploracionRepositorio.UpdateCampo("FACTURADA", "T", lineaFactura.IOR_EXPLORACION);
                            ExploracionRepositorio.UpdateCampo("IOR_FACTURA", factura.OID.ToString(), lineaFactura.IOR_EXPLORACION, "int");
                        }
                    }
                    else
                    {
                        linea.OID = lineaFactura.OID;
                        db.Entry(linea).State = (linea.OID > 0 ? EntityState.Modified : EntityState.Added);
                    }

                    db.SaveChanges();
                }
                else
                {
                    if (lineaFactura.IOR_EXPCONSUMIBLE > 0)
                    {
                        Exp_ConsumRepositorio.UpdateCampo("FACTURADO", "T", lineaFactura.IOR_EXPCONSUMIBLE);
                        Exp_ConsumRepositorio.UpdateCampo("IOR_FACTURA", factura.OID.ToString(), lineaFactura.IOR_EXPCONSUMIBLE, "int");
                    }
                    else
                    {
                        ExploracionRepositorio.UpdateCampo("FACTURADA", "T", lineaFactura.OID);
                        ExploracionRepositorio.UpdateCampo("IOR_FACTURA", factura.OID.ToString(), lineaFactura.OID, "int");
                    }
                }




            }

            TEXTOS oTextoFactura = new TEXTOS();
            oTextoFactura.TEXTO = viewModelFac.COMENTARIO;
            oTextoFactura.OWNER = factura.OID;
            TextosRepositorio.InsertarOrUpdate(oTextoFactura);
            FiltrosRepository _Filtrosrepository;
            _Filtrosrepository = new FiltrosRepository(usersDBContext);
            var usuarioAutenticated = UsuariosRepositorio.Obtener(User.Identity.Name);
            _Filtrosrepository.Guardar(usuarioAutenticated.IDUSER, "Facturas", "COD1", factura.COD1, "string");

            return RedirectToAction("CreateOrEdit", new
            {
                ior_factura = idFactura,
                url_previa= viewModelFac.URLPREVIA

            });
        }





        public FileContentResult Imprimir(int oid)
        {
            //FACTURAS fACTURAS = db.Facturas.Find(oid);
            //inicializamos el DTO para generar la factura
            VWFactura oViewModel = new VWFactura();
            FACTURAS oFactura = db.Facturas.Single(f => f.OID == oid);
            oViewModel.NOMBRE = oFactura.NOMBRE;
            oViewModel.FECHA_FAC = oFactura.FECHA_FAC;
            oViewModel.PACIENTE = oFactura.NOMBRE;
            oViewModel.NUM_FAC = oFactura.NUM_FAC;
            oViewModel.DNI = oFactura.DNI;
            oViewModel.DIRECCION = oFactura.DIRECCION;
            oViewModel.CP = oFactura.CP;
            oViewModel.CIUDAD = oFactura.CIUDAD;
            oViewModel.PROVINCIA = oFactura.PROVINCIA;
            oViewModel.COD1 = oFactura.COD1;
            oViewModel.TOTALCONIVA = oFactura.SUMA.Value.ToString();
            oViewModel.IOR_PAC = oFactura.IOR_PAC;
            oViewModel.COMENTARIO = TextosRepositorio.Obtener(oFactura.OID).TEXTO;
            oViewModel.LINEAS = db.Lineas_Facturas.Where(p => p.OWNER == oFactura.OID).ToList();
            if (oFactura.FECHA.HasValue)
            {
                oViewModel.FECHAEXPLORACION = oFactura.FECHA.Value;

            }
            if (oViewModel.NOMBREEMPRESA.Contains("DELFOS"))
            {
                oViewModel.OWNER = oFactura.OWNER.Value;
                if (oFactura.OWNER == 1)
                {
                    oViewModel.CIFEMPRESA = "B61579520";
                    oViewModel.EMPRESA = "Gabinete Nuclear Delfos";
                }
                else
                {
                    oViewModel.CIFEMPRESA = "A61150991";
                    oViewModel.EMPRESA = "Servei Imatge Molecular i Metabòlic";
                }
            }

            CENTROS oCentro;
            //A nivel de centro definimos el logo a utilizar en las cabeceras
            try
            {
                oCentro = db.Centros.Single(c => c.OID == oFactura.OWNER);
            }
            catch (Exception)
            {

                oCentro = db.Centros.Single(c => c.OID == 1);
            }


            WebConfigRepositorio oConfig = new WebConfigRepositorio();
            string RutaDOCS = oConfig.ObtenerValor("RUTADOCS");

            //Generamos el HTML de ejemplo de la factura para poder combinarlo con el modelo de datos
            string rutaFacturaTemplate = RutaDOCS + @"\RWINVOICE_" + DateTime.Now.ToString("ss") + oViewModel.OID + ".html";

            string rutaFactura = rutaFacturaTemplate.Substring(0, rutaFacturaTemplate.Length - 4) + ".pdf"; //RutaDOCS + @"\RWINVOICE_" + oViewModel.OID + DateTime.Now.ToString("ss") + ".pdf";
            StreamWriter Swr = new StreamWriter(rutaFacturaTemplate);
            Swr.Write(this.RenderView("Imprimir", oViewModel));
            Swr.Close();
            Swr.Dispose();
            byte[] aPDF = Models.HTMLToPDF.Converter.InformeFromHtml(rutaFacturaTemplate,
                oCentro.LOGO_URL,
                oCentro.LOGO_HEIGHT,
                oCentro.LOGO_WIDTH);


            return File(aPDF, "application/pdf");
        }



        // GET: Facturas/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            FACTURAS fACTURAS = db.Facturas.Find(id);
            if (fACTURAS == null)
            {
                return HttpNotFound();
            }
            return View(fACTURAS);
        }

        // POST: Facturas/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            FACTURAS fACTURAS = db.Facturas.Find(id);
            db.Facturas.Remove(fACTURAS);
            db.SaveChanges();
            return RedirectToAction("Index");
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
