
using System.Web.Mvc;
using RadioWeb.Models.Repos;
using System.Collections.Generic;
using RadioWeb.Models;
using System.Linq;
using System.Net;
using System;
using System.Data.Entity;

namespace RadioWeb.Controllers
{
    public class ConsumibleController : Controller
    {
        private RadioDBContext db = new RadioDBContext();


        public class Select2Result
        {
            public string value { get; set; }
            public string text { get; set; }

        }
        private List<Select2Result> TecnicosToSelect2Format(List<PERSONAL> tecnicos)
        {
            List<Select2Result> jsonTecnicos = new List<Select2Result>();


            //Loop through our attendees and translate it into a text value and an id for the select list
            foreach (PERSONAL a in tecnicos)
            {
                Select2Result oTemp = new Select2Result
                {
                    value = a.OID.ToString(),
                    text = a.NOMBRE
                };
               
                jsonTecnicos.Add(oTemp);

            }

            return jsonTecnicos;
        }


        [HttpGet]
        public ActionResult GetTecnicos()
        {
            List<Select2Result> oResult = new List<Select2Result>();
            try
            {
             
                   
                
                oResult = TecnicosToSelect2Format(PersonalRepositorio.ObtenerTecnicos());
            }
            catch (Exception ex)
            {
                return Json("", JsonRequestBehavior.AllowGet);
            }
            //Return the data as a jsonp result
            return Json(oResult , JsonRequestBehavior.AllowGet);

        }

        [HttpPost]
        public ActionResult EditarCampo(string name, int pk, string value)
        {
        
            var consumibleAsociado = db.Exp_Consum
                                       .Single(p => p.OID == pk);
                                    
            EXPLORACION oExplo = ExploracionRepositorio.Obtener(consumibleAsociado.IOR_EXPLORACION.Value);
            if (oExplo.INFORMADA == "T" && name!= "CABINF_DOSIS")
            {
                return new HttpStatusCodeResult(HttpStatusCode.InternalServerError, "Imposible modificar, No se puede alterar los consumibles de una exploración informada");
            }

            if (name == "CABINF_DOSIS" && string.IsNullOrEmpty(value))
            {
                value = "F";
            }

            Exp_ConsumRepositorio.UpdateCampo(name, value, pk);
            return new HttpStatusCodeResult(200);
        }



        [HttpGet]
        public ActionResult ListaEntrada(int codMut, int oidGrupo,int oidExploracion)
        {
            EXPLORACION oExplo = ExploracionRepositorio.Obtener(oidExploracion);

            //Si la exploración ya tiene consumibles asociados los tenemos que identificar para moverlos al lado derecho
            //del DualListBox
            List<EXP_CONSUM> oListaConsumibles = Exp_ConsumRepositorio.GetConsumiblesPendientes(oidExploracion);
            List<int> oConsumibles = new List<int>();
            if (oListaConsumibles.Count > 0)
            {
                foreach (var item in oListaConsumibles)
                {
                    oConsumibles.Add(item.IOR_CONSUM.Value);
                }
            }
            else {
                CONSUMIBLES oConsumibleDefecto = db.Consumibles
                    .Where(c => c.OID == oExplo.APARATO.CID)
                    .SingleOrDefault();
                if (oConsumibleDefecto != null)
                {
                    oConsumibles.Add(oConsumibleDefecto.OID);
                }
                else {
                    oConsumibles.Add( -1 );
                }
                
            }
            
            ViewBag.ConsumibleDefecto = oConsumibles;
            var listConsumibles = Precios_ConsumRepositorio.GetConsumibles(codMut, oidGrupo);
            if (listConsumibles.Count==0)
            {
                listConsumibles = Precios_ConsumRepositorio.GetConsumibles(101, oidGrupo);
            }

            return PartialView("ListaConsumibles", listConsumibles);
        }

        [HttpPost]
        public ActionResult GuardaEntrada(List<string> consumibles, int oidExploracion,bool TarifaPrivada=false)
        {
            EXPLORACION oExplo = ExploracionRepositorio.Obtener(oidExploracion);
            if (oExplo.INFORMADA=="T" )
            {
                return new HttpStatusCodeResult(HttpStatusCode.InternalServerError,"Imposible borrar, Exploración informada");
            }

            if (oExplo.PAGADO =="T" || oExplo.FACTURADA=="T")
            {
                return new HttpStatusCodeResult(HttpStatusCode.InternalServerError, "Imposible borrar, Exploración pagada o facturada");

            }

            //Primero borramos todos los consumibles asociados a la exploración si lo hay
            if (oExplo.CONSUMIBLES != null)
            {
                foreach (var consuEliminar in oExplo.CONSUMIBLES)
                {
                    EXP_CONSUM oConsumibleaBorrar = db.Exp_Consum.Single(p => p.IOR_EXPLORACION == oExplo.OID && p.IOR_CONSUM == consuEliminar.IOR_CONSUM);
                    
                        PAGOS oPagoConsumible = db.Pagos.SingleOrDefault(c => c.OWNER == oConsumibleaBorrar.OID);

                    db.Exp_Consum.Remove(oConsumibleaBorrar);

                    if (oPagoConsumible!=null)
                    {
                        db.Pagos.Remove(oPagoConsumible);
                        //ojo controlar si la lista viene vacia

                    }
                    db.SaveChanges();

                }
                ExploracionRepositorio.UpdateCampo("HAYCONSUMIBLE", "F", oExplo.OID);

            }

            if (!String.IsNullOrEmpty(consumibles.First()))
            {
                foreach (string item in consumibles)
                {
                    int iorConsum = int.Parse(item);
                    if (db.Exp_Consum.Where(p => p.IOR_EXPLORACION == oExplo.OID && p.IOR_CONSUM == iorConsum).Count() == 0)
                    {
                        EXP_CONSUM oConsumible = new EXP_CONSUM();
                        oConsumible.PAGADO = "F";
                        oConsumible.BORRADO = "F";
                        oConsumible.APLAZADO = "F";
                        oConsumible.PAGAR = "T";
                        oConsumible.FACTURADO = "F";
                        oConsumible.IOR_FACTURA = -1;
                        oConsumible.IOR_EXPLORACION = oExplo.OID;
                        oConsumible.CABINF_DLP = "T";
                        oConsumible.CABINF_DOSIS = "T";
                        oConsumible.IOR_CONSUM = int.Parse(item);
                        if (TarifaPrivada)
                        {
                            oConsumible.IOR_ENTIDADPAGADORA = db.Mutuas.Single(p=>p.OWNER==1).OID;
                        }
                        else {
                            oConsumible.IOR_ENTIDADPAGADORA = oExplo.IOR_ENTIDADPAGADORA;
                        }
                       
                        PRECIOS_CONSUM oPrecio = db.PreciosConsum
                            .Where(p => p.IOR_CONSUM == oConsumible.IOR_CONSUM
                             && p.IOR_ENTIDADPAGADORA == oExplo.IOR_ENTIDADPAGADORA).SingleOrDefault();

                        //todo repasar tras arranque
                        if (oPrecio == null)
                        {
                            oConsumible.PRECIO = 0;
                        }
                        else
                        {
                            if (oPrecio.PRECIO == null)
                            {
                                oConsumible.PRECIO = 0;
                            }
                            else
                            {
                                oConsumible.PRECIO = oPrecio.PRECIO;
                            }

                        }

                        Exp_ConsumRepositorio.Insertar(oConsumible);
                    }

                }
            }
     

            return new HttpStatusCodeResult(200);
        }


        [HttpGet]
        public JsonResult List(int iorMutua, int oidGrupo)
        {

            return Json(Precios_ConsumRepositorio.GetConsumibles(iorMutua,  oidGrupo), JsonRequestBehavior.AllowGet);

        }

        [HttpPost]
        public ActionResult List(string codMut, int oidGrupo)
        {           
           
            IEnumerable<RadioWeb.Models.MUTUAS> oListTempMutuas = MutuasRepositorio.Lista(false);
            MUTUAS oMutua = null;
            foreach (var item in oListTempMutuas)
            {
                if (item.CODMUT==codMut)
                {
                    oMutua = item;
                }
            }
            return PartialView("ListaConsumibles", Precios_ConsumRepositorio.GetConsumibles(oMutua.OID, oidGrupo));
        }

        [HttpPost]
        public ActionResult ListAsignados( int oid)
        {
           
            return PartialView("ListaInyectables", Exp_ConsumRepositorio.GetConsumiblesPendientes(oid));
        }

        
        [HttpPost]
        public ActionResult Add(int IOR_MUTUACONSUMIBLE, int IOR_ADDCONSUMIBLE, int PRECIO,int OIDEXPLORACION)
        {
            
            EXP_CONSUM oConsumible = new EXP_CONSUM
            {
                PAGADO = "F",
                BORRADO = "F",
                APLAZADO = "F",
                FACTURADO = "F",
                IOR_FACTURA = -1,
                CABINF_DLP = "T",
                CABINF_DOSIS = "T",
                IOR_ENTIDADPAGADORA=IOR_MUTUACONSUMIBLE,
                IOR_CONSUM=IOR_ADDCONSUMIBLE,
                PRECIO=PRECIO,
                IOR_EXPLORACION= OIDEXPLORACION
            };
         

            
            Exp_ConsumRepositorio.Insertar(oConsumible);
            return new HttpStatusCodeResult(200);
        }


        [HttpPost]
        public ActionResult Edit(EXP_CONSUM oConsumible)
        {

            oConsumible.PAGADO = "F";
            oConsumible.BORRADO = "F";
            oConsumible.APLAZADO = "F";
            oConsumible.PAGAR = "T";
            oConsumible.FACTURADO = "F";
            oConsumible.IOR_FACTURA = -1;

            Exp_ConsumRepositorio.Insertar(oConsumible);
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

        //CREATE
        public ActionResult CreateConsumible(int groupOid)
        {
            CONSUMIBLES consumible = new CONSUMIBLES();
            consumible.BORRADO = "F";
            consumible.IOR_EMPRESA = 4;
            consumible.OWNER = groupOid;

            //Recuperamos los Grupos disponibles
            var gaparatos = GAparatoRepositorio.Lista();

            int[] selectedItems = new int[99];
            selectedItems[0] = groupOid;
            ViewBag.Grupos = new MultiSelectList(gaparatos, "OID", "DES_GRUP", selectedItems);

            return View("Consumibles", consumible);
        }

        [HttpPost]
        public ActionResult CreateConsumible(CONSUMIBLES consumible)
        {
            if (ModelState.IsValid)
            {
                db.Consumibles.Add(consumible);
                db.SaveChanges();
                int groupOid = consumible.OWNER.GetValueOrDefault();

                foreach (var selected_group in consumible.SELECTED_GROUPS)
                {
                    CONS_GRUPO cons_grupo = new CONS_GRUPO();
                    cons_grupo.IOR_CONSUMIBLE = consumible.OID;
                    cons_grupo.IOR_GAPARATO = selected_group;
                    db.Cons_Grupo.Add(cons_grupo);
                    db.SaveChanges();
                }

                return RedirectToAction("Index","Grupos", new { id = groupOid, tab = "tabConsumibles" });
            }



            //Recuperamos los Grupos disponibles
            var gaparatos = GAparatoRepositorio.Lista();

            int[] selectedItems = consumible.SELECTED_GROUPS;
            ViewBag.Grupos = new MultiSelectList(gaparatos, "OID", "DES_GRUP", selectedItems);

            return View("Consumibles", consumible);
        }


        //EDIT
        public ActionResult EditConsumible(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }


            CONSUMIBLES consumible = db.Consumibles.Find(id);

            //Recuperamos los Grupos disponibles Y los guardados
            var gaparatos = GAparatoRepositorio.Lista();
            int[] selectedGroups = Cons_GrupoRepositorio.FindByConsumibleOid(id);
            ViewBag.Grupos = new MultiSelectList(gaparatos, "OID", "DES_GRUP", selectedGroups);


            if (consumible == null)
            {
                return HttpNotFound();
            }
            return View("Consumibles", consumible);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditConsumible(CONSUMIBLES consumible)
        {
            if (ModelState.IsValid)
            {
                db.Entry(consumible).State = EntityState.Modified;
                db.SaveChanges();

                //Borramos la tabla CONS_GRUPO
                int consumibleOid = consumible.OID;
                Cons_GrupoRepositorio.DeleteByConsumibleOid(consumibleOid);

                //Generamos la tabla CONS_GRUPO con los valores del multiselect
                foreach (var selected_group in consumible.SELECTED_GROUPS)
                {
                    CONS_GRUPO cons_grupo = new CONS_GRUPO();
                    cons_grupo.IOR_CONSUMIBLE = consumible.OID;
                    cons_grupo.IOR_GAPARATO = selected_group;
                    db.Cons_Grupo.Add(cons_grupo);
                    db.SaveChanges();
                }


                int groupOid = consumible.OWNER.GetValueOrDefault();
                return RedirectToAction("Index", "Grupos", new { id = consumible.OWNER, tab = "tabConsumibles" });
            }
            return View("Consumibles", consumible);
        }

        //DELETE
        public ActionResult DeleteConsumible(int id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            ForeignkeyCheckController fkcc = new ForeignkeyCheckController();
            ViewBag.fkBorrable = fkcc.isBorrable("CONSUMIBLES", id);

            CONSUMIBLES consumible = db.Consumibles.Find(id);
            if (consumible == null)
            {
                return HttpNotFound();
            }
            return View(consumible);
        }

        [HttpPost, ActionName("DeleteConsumible")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConsumibleConfirmed(int id)
        {
            CONSUMIBLES consumible = db.Consumibles.Find(id);

            ForeignkeyCheckController fkcc = new ForeignkeyCheckController();
            if (fkcc.isBorrable("CONSUMIBLES", id))
            {
                consumible.BORRADO = "T";
                db.SaveChanges();
            }

            return RedirectToAction("Index", "Grupos", new { id = consumible.OWNER, tab = "tabConsumibles" });
        }


        [HttpPost]
        public ActionResult ListaConsumibles(int oidGrupo)
        {
            List<CONSUMIBLES> oListaConsumibles = ConsumibleRepositorio.ListaPorGrupoAparatos(oidGrupo);
            ViewBag.groupOid = oidGrupo;
            return PartialView("_ListaConsumibles", oListaConsumibles);
        }
    }

    
}
