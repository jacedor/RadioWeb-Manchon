using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using ADPM.Common;
using RadioWeb.Models;
using RadioWeb.Models.Repos;

namespace RadioWeb.Controllers
{

    public class resultadoExploraciones
    {
        public int total { get; set; }
        public IEnumerable<LISTADIA> rows { get; set; }
    }
    public class resultadoPeticiones
    {
        public int total { get; set; }
        public IEnumerable<BOLSA_PRUEBAS> rows { get; set; }
    }
    public class PeticionesController : Controller
    {
        private RadioDBContext db = new RadioDBContext();


        private BOLSA_PRUEBAS inicializarBolsa()
        {
            BOLSA_PRUEBAS oModel = new BOLSA_PRUEBAS();
            USUARIO oUsuario = UsuariosRepositorio.Obtener(User.Identity.Name);
            oModel.USERNAME = oUsuario.LOGIN;
           
            if (oUsuario.IOR_ENTIDADPAGADORA.HasValue && oUsuario.IOR_ENTIDADPAGADORA.Value > 0)
            {
                oModel.PETICIONESANOMBREDEMUTUA = oUsuario.MUTUA.NOMBRE;
                oModel.IOR_ENTIDADPAGADORA = oUsuario.MUTUA.OID;

                List<CENTROSEXTERNOS> centroExterno = CentrosExternosRepositorio.ObtenerPorMutuaPeticiones(oModel.IOR_ENTIDADPAGADORA ?? -1);
                if (centroExterno.Count() > 0)
                {
                    oModel.IOR_CENTROEXTERNO = centroExterno.First().OID;
                }
            }
            if (oUsuario.IOR_COLEGIADO.HasValue && oUsuario.IOR_COLEGIADO.Value > 0)
            {
                oModel.PETICIONESANOMBREDECOLEGIADO = oUsuario.PERSONAL.NOMBRE;
                oModel.IOR_COLEGIADO = oUsuario.IOR_COLEGIADO.Value;
            }

            if (oUsuario.IOR_CENTROEXTERNO.HasValue && oUsuario.IOR_CENTROEXTERNO.Value > 0)
            {
                oModel.PETICIONESANOMBREDECENTROEXTERNO = oUsuario.CENTROEXTERNO.NOMBRE;
                oModel.IOR_CENTROEXTERNO = oUsuario.IOR_CENTROEXTERNO.Value;

                CENTROSEXTERNOS centroExterno = CentrosExternosRepositorio.Obtener(oModel.IOR_CENTROEXTERNO ?? -1);
                if(centroExterno.IOR_MUTUA > 0)
                {
                    oModel.IOR_ENTIDADPAGADORA = MutuasRepositorio.Obtener(centroExterno.IOR_MUTUA ?? -1).OID;
                }
            }
            return oModel;
        }
        // GET: Peticiones
        public ActionResult Index(string search = "", string sort="OID", string order = "desc", int offset = 0, int limit = 10)
        {
            USUARIO oUsuario = UsuariosRepositorio.Obtener(User.Identity.Name);
            int ior_colegiado = -1;
            int ior_entidadpagadora = -1;
            int ior_centroexterno = -1;
            var serializer = new System.Web.Script.Serialization.JavaScriptSerializer();
            string whereParaCount = "where ";
            resultadoPeticiones oResultado = new resultadoPeticiones();
            IQueryable<BOLSA_PRUEBAS> queryable= db.BolsaPruebas;
            if (oUsuario.IOR_COLEGIADO.HasValue && oUsuario.IOR_COLEGIADO.Value > 0)
            {
                ior_colegiado = oUsuario.IOR_COLEGIADO.Value;
                queryable= queryable.Where(p => p.IOR_COLEGIADO == ior_colegiado);
                whereParaCount = whereParaCount + " ior_colegiado=" + ior_colegiado;
            }
            if (oUsuario.IOR_ENTIDADPAGADORA.HasValue && oUsuario.IOR_ENTIDADPAGADORA.Value > 0)
            {
                ior_entidadpagadora = oUsuario.IOR_ENTIDADPAGADORA.Value;
                queryable = queryable.Where(p => p.IOR_ENTIDADPAGADORA == ior_entidadpagadora);
                whereParaCount = whereParaCount + " IOR_ENTIDADPAGADORA=" + ior_entidadpagadora;

            }
            if (oUsuario.IOR_CENTROEXTERNO.HasValue && oUsuario.IOR_CENTROEXTERNO.Value > 0)
            {
                ior_centroexterno = oUsuario.IOR_CENTROEXTERNO.Value;
                queryable = queryable.Where(p => p.IOR_CENTROEXTERNO == ior_centroexterno);
                whereParaCount = whereParaCount + " IOR_CENTROEXTERNO=" + ior_centroexterno;

            }



            switch (sort)
            {
                case "PACIENTE":
                    if (order=="asc")
                    {
                        oResultado.rows =queryable.OrderBy(p => p.PACIENTE);
                    }
                    else
                    {
                        oResultado.rows = queryable.OrderByDescending(p => p.PACIENTE);
                    }
                    break;
                case "FECHAENTRADA":
                    if (order == "asc")
                    {
                        oResultado.rows = queryable.OrderBy(p => p.FECHAENTRADA);
                    }
                    else
                    {
                        oResultado.rows = queryable.OrderByDescending(p => p.FECHAENTRADA);

                    }
                    //students = students.OrderByDescending(s => s.LastName);
                    break;
                case "EXPLORACION":
                    if (order == "asc")
                    {
                        oResultado.rows = queryable.OrderBy(p => p.EXPLORACION);
                    }
                    else
                    {
                        oResultado.rows = queryable.OrderByDescending(p => p.EXPLORACION);

                    }
                    //students = students.OrderByDescending(s => s.LastName);
                    break;
                case "PRIORIDAD":
                    if (order == "asc")
                    {
                        oResultado.rows = queryable.OrderBy(p => p.PRIORIDAD);
                    }
                    else
                    {
                        oResultado.rows = queryable.OrderByDescending(p => p.PRIORIDAD);

                    }
                    //students = students.OrderByDescending(s => s.LastName);
                    break;
                case "EXPLORACIONAGENDADA.FECHA":
                    if (order == "asc")
                    {
                        oResultado.rows = queryable.OrderBy(p => p.EXPLORACIONAGENDADA.FECHA);
                    }
                    else
                    {
                        oResultado.rows = queryable.OrderByDescending(p => p.EXPLORACIONAGENDADA.FECHA);

                    }
                    //students = students.OrderByDescending(s => s.LastName);
                    break;
                
                default:
                    if (order == "asc")
                    {
                        oResultado.rows = queryable.OrderBy(p => p.OID);
                    }
                    else
                    {
                        oResultado.rows = queryable.OrderByDescending(p => p.OID);

                    }

                    break;
            }
         


            if (!String.IsNullOrEmpty(search))
            {
                oResultado.rows = oResultado.rows.Where(p => p.PACIENTE.Contains( search.ToUpper()));
            }
            //if (oUsuario.IOR_COLEGIADO.HasValue)
            //{
            //   var exploraciones= ListaDiaRepositorio.Lista(oUsuario.IOR_COLEGIADO.Value);
            //    foreach (var item in exploraciones)
            //    {
            //        oResultado.rows.ToList().Add(new BOLSA_PRUEBAS { })
            //    }
            //}

            oResultado.total = oResultado.rows.Count();
            oResultado.rows = oResultado.rows.Skip(offset).Take(limit);

            foreach (var item in oResultado.rows.ToList())
            {
                if (item.IOR_EXPLORACION > 0)
                {
                    item.EXPLORACIONAGENDADA = ExploracionRepositorio.Obtener(item.IOR_EXPLORACION.Value);
                }
                if (item.IOR_ENTIDADPAGADORA.HasValue)
                {
                    item.MUTUA = MutuasRepositorio.Obtener(item.IOR_ENTIDADPAGADORA.Value).NOMBRE;
                }
                if (item.IOR_CENTROEXTERNO.HasValue)
                {
                    item.CENTROEXTERNO = CentrosExternosRepositorio.Obtener(item.IOR_CENTROEXTERNO.Value).NOMBRE;
                }
              
            }

            var json = Json(oResultado, JsonRequestBehavior.AllowGet);
            return json;

        }

        // GET: Peticiones/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            BOLSA_PRUEBAS bOLSA_PRUEBAS = db.BolsaPruebas.Find(id);
            if (bOLSA_PRUEBAS == null)
            {
                return HttpNotFound();
            }
            return View(bOLSA_PRUEBAS);
        }

        // GET: Peticiones/Create

        public ActionResult Create()
        {
        ViewData["tiposDeDocumento"]= from s in db.Refractometros
                                  where s.CID == 6 // if you want to filer the results
                                  select s;
            return View(inicializarBolsa());
        }
  
        public ActionResult List()
        {             
            
            return View(inicializarBolsa());
        }


        public ActionResult ListExplos(string search = "",string FECHAINICIAL="", string FECHAFINAL = "",string ESTADO="-1", string sort = "FECHA", string order = "desc", int offset = 0, int limit = 10)
        {
            USUARIO oUsuario = UsuariosRepositorio.Obtener(User.Identity.Name);
            int ior_colegiado = -1;
            int ior_entidadpagadora = -1;
            int ior_centroexterno = -1;
            bool WhereIniciado = false;
            bool WhereInicado2 = false;
            var fechaInicial = DateTime.Parse(FECHAINICIAL);
            var fechaFinal = DateTime.Parse(FECHAFINAL);
            var serializer = new System.Web.Script.Serialization.JavaScriptSerializer();
            resultadoExploraciones oResultado = new resultadoExploraciones();

            string whereParaCount = "select count(oid) from LISTADIA e where ";
           
            IQueryable<LISTADIA> queryable = db.ListaDia.Where(p=>p.FECHA >= fechaInicial).Where(p => p.FECHA <= fechaFinal);

            if (oUsuario.IOR_COLEGIADO.HasValue && oUsuario.IOR_COLEGIADO.Value > 0)
            {
                ior_colegiado = oUsuario.IOR_COLEGIADO.Value;
                queryable = queryable.Where(p => p.IOR_COLEGIADO == ior_colegiado);
                whereParaCount = whereParaCount + " e.ior_colegiado=" + ior_colegiado;
                WhereIniciado = true;
            }

            if (oUsuario.IOR_ENTIDADPAGADORA.HasValue && oUsuario.IOR_ENTIDADPAGADORA.Value > 0)
            {
                ior_entidadpagadora = oUsuario.IOR_ENTIDADPAGADORA.Value;
                MUTUAS oMutua = MutuasRepositorio.Obtener(ior_entidadpagadora);
                queryable = queryable.Where(p => p.IOR_ENTIDADPAGADORA == oMutua.OID);
                if (WhereIniciado)
                {
                    whereParaCount = whereParaCount + " and e.IOR_ENTIDADPAGADORA=" + oMutua.OID;

                }
                else
                {
                    whereParaCount = whereParaCount + " e.IOR_ENTIDADPAGADORA=" + oMutua.OID;

                }
                WhereIniciado = true;
                WhereInicado2 = true;
            }
            if (oUsuario.IOR_CENTROEXTERNO.HasValue && oUsuario.IOR_CENTROEXTERNO.Value > 0)
            {
                ior_centroexterno = oUsuario.IOR_CENTROEXTERNO.Value;
                CENTROSEXTERNOS oCentroExterno = CentrosExternosRepositorio.Obtener(ior_centroexterno);
                queryable = queryable.Where(p => p.IOR_CENTROEXTERNO == ior_centroexterno);
                if (WhereIniciado)
                {
                    whereParaCount = whereParaCount + " AND e.IOR_CENTROEXTERNO=" + oCentroExterno.OID;
                }
                else
                {
                    whereParaCount = whereParaCount + "  e.IOR_CENTROEXTERNO=" + oCentroExterno.OID;

                }
                WhereIniciado = true;

            }

            if (!String.IsNullOrEmpty(search))
            {
                queryable = queryable.Where(p => p.PACIENTE.StartsWith(search.ToUpper()));
                // change
                if (WhereIniciado || WhereInicado2)
                {
                    whereParaCount = whereParaCount + "  AND e.PACIENTE like '%" + search.ToUpper() + "%'";
                }
                else
                {
                    whereParaCount = whereParaCount + " e.PACIENTE like '%" + search.ToUpper() + "%'";
                }

                whereParaCount = whereParaCount.ToUpper();
               // whereParaCount = whereParaCount.Replace("EXPLORACION", "LISTADIA");
            }
            int estado = int.Parse(ESTADO);
            if (estado>=0)
            {
                queryable = queryable.Where(p => p.ESTADO==ESTADO);
                // change
                if (WhereIniciado || WhereInicado2)
                {
                    whereParaCount = whereParaCount + "  AND e.ESTADO='" + ESTADO +"'";
                }
                else
                {
                    whereParaCount = whereParaCount + " e.ESTADO='" + ESTADO + "'"; 
                }

                whereParaCount = whereParaCount.ToUpper();
                // whereParaCount = whereParaCount.Replace("EXPLORACION", "LISTADIA");
            }
            switch (sort)
            {
                case "PACIENTE":
                    if (order == "asc")
                    {
                        oResultado.rows = queryable.OrderBy(p => p.PACIENTE);
                    }
                    else
                    {
                        oResultado.rows = queryable.OrderByDescending(p => p.PACIENTE);
                    }
                    break;
           
          
                case "FECHA":
                    if (order == "asc")
                    {
                        oResultado.rows = queryable.OrderBy(p => p.FECHA);
                    }
                    else
                    {
                        oResultado.rows = queryable.OrderByDescending(p => p.FECHA);

                    }
                    //students = students.OrderByDescending(s => s.LastName);
                    break;

                default:
                    if (order == "asc")
                    {
                        oResultado.rows = queryable.OrderBy(p => p.OID);
                    }
                    else
                    {
                        oResultado.rows = queryable.OrderByDescending(p => p.OID);

                    }

                    break;
            }

            if (WhereIniciado || WhereInicado2)
            {
                whereParaCount = whereParaCount + " and ";
            }

            whereParaCount = whereParaCount + "  e.FECHA between " + DateTime.Parse(FECHAINICIAL).ToString("MM/dd/yyyy").QuotedString()
           + " and " + DateTime.Parse(FECHAFINAL).ToString("MM/dd/yyyy").QuotedString(); //+ " AND e.FECHA IS NOT NULL";

            oResultado.total = db.Database.SqlQuery<int>(whereParaCount).First(); //queryable.Count();// oResultado.rows.Count();
            oResultado.rows = oResultado.rows.Skip(offset).Take(limit).ToList();


            if (oUsuario.IOR_CENTROEXTERNO.HasValue && oUsuario.IOR_CENTROEXTERNO.Value > 0)
            {
                foreach (var item in oResultado.rows)
                {
                    item.CENTROEXTERNO = CentrosExternosRepositorio.Obtener(item.IOR_CENTROEXTERNO ?? -1).NOMBRE;
                }
            }
          //  List<COLEGIADOS> oTempColegiados = ColegiadoRepositorio.List();
            foreach (var item in oResultado.rows)
            {
                INFORMES informe = InformesRepositorio.ObtenerDeExploracionValidado(item.OID);

                if(informe.OID > 0)
                {
                    item.IOR_INFORME = informe.OID;
                }
                try
                {
                    item.COD_MED = ColegiadoRepositorio.Obtener( item.IOR_COLEGIADO).NOMBRE;//.Obtener()

                }
                catch (Exception)
                {

                   // throw;
                }
            }

            var json = Json(oResultado, JsonRequestBehavior.AllowGet);
            return json;

        }


        public ActionResult ListExploraciones()
        {

            return View(inicializarBolsa());
        }
        public ActionResult CitaOnline()
        {
            var modelo = inicializarBolsa();
            modelo.FECHAPRUEBA = DateTime.Now;
            modelo.HORAINICIOBUSQUEDA = "07:00";
            return View(modelo);
        }


        

       [HttpPost]
        public ActionResult BuscarHuecos([Bind(Include = "FECHAPRUEBA,HORAINICIOBUSQUEDA,IOR_GAPARATO,IOR_TIPOEXPLORACION,IOR_ENTIDADPAGADORA,IOR_CENTRO")] BOLSA_PRUEBAS oViewModel)
        {
            string query = "";
            List<HUECO> oListaResult = new List<HUECO>();
            MUTUAS oMutua = MutuasRepositorio.Obtener(oViewModel.IOR_ENTIDADPAGADORA.Value);
            int oidMutua = oMutua.OID;

            string codigoGrupo = "";//PREGUNTAR A MASSANA GAparatoRepositorio.Obtener(oViewModel.IOR_GAPARATO).COD_GRUP;
            string codigoActo = AparatoRepositorio.Obtener(oViewModel.IOR_TIPOEXPLORACION).COD_FIL;
            string Centro = CentrosRepositorio.Obtener(oViewModel.IOR_CENTRO).DESCRIPCIO;

            if ((codigoGrupo + codigoActo.Trim() == "RX ORT" && Centro == "MERIDIANA")
                  || (codigoGrupo + codigoActo.Trim() == "RX ORT" && Centro == "BALMES")
                  || string.IsNullOrWhiteSpace(codigoActo))
            {
                oListaResult.Add(new HUECO { CENTRO = Centro });
               // return oListaResult;
            }

            if ((codigoGrupo + codigoActo.Trim() == "RX CC" && Centro != "TIBIDABO")
                || (codigoGrupo + codigoActo.Trim() == "RX CCP" && Centro != "TIBIDABO")
                || (codigoGrupo + codigoActo.Trim() == "RX CC1" && Centro != "TIBIDABO")
                || (codigoGrupo + codigoActo.Trim() == "RX TEF" && Centro != "TIBIDABO"))
            {
                oListaResult.Add(new HUECO { CENTRO = Centro });
               // return oListaResult;
            }

            string ipAddress = System.Web.HttpContext.Current.Request.UserHostAddress;
           
            LOGCITAONLINE oLog = new LOGCITAONLINE
            {
                FECHA = DateTime.Now.ToString("MM/dd/yyyy"),
                HORA = DateTime.Now.ToString("HH:MM"),
                TEXTO = ipAddress,
                GRUPO = codigoGrupo.Trim().PadRight(3, ' '),
                EXPLORACION = codigoActo.Trim(),
                OWNER = 1,
                CID = 0,
                MODIF = DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss"),
                APARATO = "null",
                USERNAME = "CITACIONDIRECTAPET"

            };
            Dictionary<string, List<HUECO>> fechasConHuecos = new Dictionary<string, List<HUECO>>();

           
            foreach (var centro in CentrosRepositorio.List().Where(c=>c.CID ==1))
            {
                //Coleccion con la fecha del hueco separado con dos ## y el nombre del centro
                var fechaDeLosHuecos = new List<string>();
                switch (centro.NOMBRE.ToUpper())
                {
                    case "TIBIDABO":
                        centro.NOMBRE = "CDPI";
                        break;
                    case "SANT CUGAT":
                        centro.NOMBRE = "CUGAT";
                        break;
                    case "BALMES125":
                        centro.NOMBRE = "BALMES";
                        break;
                    case "MERIDIANA":
                        centro.NOMBRE = "MERIDIANA";
                        break;
                    case "HOSPITALET":
                        centro.NOMBRE = "HOSPITALET";
                        break;
                    default:
                        break;
                }
                oListaResult = HorasLibreRepositorio.ListaDesdePeticiones(centro.NOMBRE.ToUpper(), codigoActo.Trim(), oViewModel.FECHAPRUEBA.ToShortDateString(),
                oViewModel.HORAINICIOBUSQUEDA,
                false,
                false,
                oViewModel.IOR_ENTIDADPAGADORA.Value,
                ref query);


                // MIRAMOS CUANTAS FECHAS DEL RANGO BUSCADO TIENEN HUECOS
                var fechaDeLosHuecosTemp = oListaResult.Select(h => h.FECHA)
                                                     .Distinct().Take(7);



                if (fechaDeLosHuecosTemp.Count() >0)
                {
                    //PARA CADA FECHA CON HUECOS LE AGREGAMOS COMO CLAVE DEL DICCIONARIO LA FECHA # EL CENTRO
                    //POR EJEMPLO 12/12/2022#CDPI O 12/12/2022#MERIDIANA
                    foreach (var fechaDeUnCentro in fechaDeLosHuecosTemp)
                    {
                        string fechaHuecoConCentro = fechaDeUnCentro + "#" + centro.NOMBRE;
                        fechaDeLosHuecos.Add(fechaHuecoConCentro);
                    }

                    foreach (var item in fechaDeLosHuecos)
                    {
                        if (fechasConHuecos.ContainsKey(item))
                        {
                            var fechaHueco = item.Split('#')[0];
                            fechasConHuecos[item].AddRange(oListaResult.Where(h => h.FECHA == fechaHueco && h.CENTRO== centro.NOMBRE));
                        }

                        else
                        {
                            var fechaHueco = item.Split('#')[0];
                            fechasConHuecos.Add(item, oListaResult.Where(h => h.FECHA == fechaHueco && h.CENTRO == centro.NOMBRE).ToList());

                        }
                    }
                }
               

            }


            return PartialView("_ListaHuecos", fechasConHuecos);
        }

        [HttpPost]
        public ActionResult EditarTexto(string name, int pk, string value)
        {


            BOLSA_PRUEBAS bOLSA_PRUEBAS = db.BolsaPruebas.Find(pk);
            if (bOLSA_PRUEBAS == null)
            {
                return HttpNotFound();
            }
            ModelState.Clear();
            bOLSA_PRUEBAS.COMENTARIO = value;   
            db.Entry(bOLSA_PRUEBAS).State = EntityState.Modified;
            db.SaveChanges();

            return new HttpStatusCodeResult(200);

        }
        // POST: Peticiones/Create
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        //public ActionResult Create([Bind(Include = "USERNAME,NOMBRE,APELLIDOS,MAIL,TELEFONO1,TELEFONO2,DIRECCION,POBLACION,PROVINCIA,FECHAPRUEBA,HORAPRUEBA,PRUEBA,COMENTARIO,IOR_TIPOEXPLORACION,IOR_ENTIDADPAGADORA,PRIORIDAD,IOR_GAPARATO,IOR_COLEGIADO,IOR_CENTROEXTERNO,DOCUMENTO,SEXO,FECHANACIMIENTO")] BOLSA_PRUEBAS oViewModel)
        public ActionResult Create([Bind(Include = "OID,USERNAME,NOMBRE,APELLIDOS,MAIL,TELEFONO1,TELEFONO2,DIRECCION,POBLACION,PROVINCIA,FECHAPRUEBA,HORAPRUEBA,PRUEBA,COMENTARIO,IOR_TIPOEXPLORACION,IOR_ENTIDADPAGADORA,FECHARESULTADO,PRIORIDAD,IOR_GAPARATO,IOR_COLEGIADO,IOR_CENTROEXTERNO,DOCUMENTO,SEXO,FECHANACIMIENTO,IOR_CENTRO,IOR_TIPODOC")] BOLSA_PRUEBAS oViewModel)

        {
            if (ModelState.IsValid)
            {
                USUARIO oUsuario = UsuariosRepositorio.Obtener(User.Identity.Name);
                oViewModel.SEXO = oViewModel.SEXO;
                oViewModel.FECHANACIMIENTO = oViewModel.FECHANACIMIENTO;
                oViewModel.PACIENTE = oViewModel.APELLIDOS.ToUpper() + ", " + oViewModel.NOMBRE.ToUpper();
                oViewModel.FECHAENTRADA = DateTime.Now;
                //oViewModel.IOR_COLEGIADO = UsuariosRepositorio.Obtener(User.Identity.Name).PERSONAL.OID;
                oViewModel.EXPLORACION = AparatoRepositorio.Obtener(oViewModel.IOR_TIPOEXPLORACION).DES_FIL;// UsuariosRepositorio.Obtener(User.Identity.Name).PERSONAL.OID;
                oViewModel.IOR_GAPARATO = AparatoRepositorio.Obtener(oViewModel.IOR_TIPOEXPLORACION).OWNER.Value;
                oViewModel.IOR_CENTRO = oViewModel.IOR_CENTRO;
                oViewModel.IOR_EXPLORACION = -1;

                oViewModel.USERNAME = User.Identity.Name;
                
                db.BolsaPruebas.Add(oViewModel);
                db.SaveChanges();
                db.Entry(oViewModel).GetDatabaseValues();
                //return RedirectToAction("Create", "Peticiones");

                if (oViewModel.DOCUMENTO != null)
                {
                    adjuntarFichero(oViewModel);
                }

                return RedirectToAction("List", new
                {
                    OID = oViewModel.OID

                });
                // return View(new BOLSA_PRUEBAS());

            }

            return new HttpStatusCodeResult(HttpStatusCode.InternalServerError);
            //  return View(oViewModel);
        }

        // GET: Peticiones/Edit/5
        public ActionResult Edit(int oid)
        {
            if (oid == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            BOLSA_PRUEBAS bOLSA_PRUEBAS = db.BolsaPruebas.Find(oid);
            if (bOLSA_PRUEBAS == null)
            {
                return HttpNotFound();
            }
            bOLSA_PRUEBAS.NOMBRE = bOLSA_PRUEBAS.PACIENTE.Split(',')[1];
            bOLSA_PRUEBAS.APELLIDOS = bOLSA_PRUEBAS.PACIENTE.Split(',')[0];
            // bOLSA_PRUEBAS.FECHARESULTADO = bOLSA_PRUEBAS.FECHARESULTADO.ToShortDateString();

            return View("Create", bOLSA_PRUEBAS);
        }

        // POST: Peticiones/Edit/5
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        public ActionResult Edit([Bind(Include = "OID,NOMBRE,APELLIDOS,MAIL,TELEFONO1,TELEFONO2,DIRECCION,POBLACION,PROVINCIA,FECHAPRUEBA,HORAPRUEBA,PRUEBA,COMENTARIO,IOR_TIPOEXPLORACION,IOR_ENTIDADPAGADORA,FECHARESULTADO,PRIORIDAD,IOR_GAPARATO,IOR_CENTROEXTERNO,IOR_COLEGIADO, DOCUMENTO,SEXO,FECHANACIMIENTO,IOR_CENTRO,IOR_DOCUMENTO,IOR_TIPODOC")] BOLSA_PRUEBAS bOLSA_PRUEBAS)
        {
            if (ModelState.IsValid)
            {
                bOLSA_PRUEBAS.PACIENTE = bOLSA_PRUEBAS.APELLIDOS.ToUpper() + ", " + bOLSA_PRUEBAS.NOMBRE.ToUpper().Trim();
                bOLSA_PRUEBAS.FECHAENTRADA = DateTime.Now;
                bOLSA_PRUEBAS.IOR_COLEGIADO = UsuariosRepositorio.Obtener(User.Identity.Name).PERSONAL.OID;
                bOLSA_PRUEBAS.EXPLORACION = AparatoRepositorio.Obtener(bOLSA_PRUEBAS.IOR_TIPOEXPLORACION).DES_FIL;
                bOLSA_PRUEBAS.IOR_GAPARATO = AparatoRepositorio.Obtener(bOLSA_PRUEBAS.IOR_TIPOEXPLORACION).OWNER.Value;
                bOLSA_PRUEBAS.USERNAME = User.Identity.Name;
                bOLSA_PRUEBAS.IOR_EXPLORACION = -1;
                db.Entry(bOLSA_PRUEBAS).State = EntityState.Modified;
                db.SaveChanges();

                if (bOLSA_PRUEBAS.DOCUMENTO != null)
                {
                    adjuntarFichero(bOLSA_PRUEBAS);
                }

                return RedirectToAction("List", new
                {
                    OID = bOLSA_PRUEBAS.OID

                });
                //return RedirectToAction("Create", new
                //{
                //    OID = bOLSA_PRUEBAS.OID

                //});
            }
            return View("List");
        }
        //Ficar aquest metode a l'hora de citació directa
        private void adjuntarFichero(BOLSA_PRUEBAS oViewModel)
        {
            System.IO.FileInfo fInfo = new System.IO.FileInfo(oViewModel.DOCUMENTO.FileName);

            IMAGENES oImagen = new IMAGENES
            {
                NOMBRE = Path.GetFileNameWithoutExtension(fInfo.Name),
                IOR_EXPLORACION = oViewModel.OID,
                IOR_PACIENTE = -1,
                EXT = fInfo.Extension,
                PATH = Utils.Varios.ObtenerCarpetaImagen()
            };

            ImagenesRepositorio.Insertar(oImagen, true);
            oImagen.OID = db.Imagenes.Where(p => p.IOR_EXPLORACION == oViewModel.OID).OrderByDescending(p => p.OID).FirstOrDefault().OID;
            BOLSA_PRUEBAS oBolsa = db.BolsaPruebas.Single(p => p.OID == oViewModel.OID);
            oBolsa.IOR_DOCUMENTO = oImagen.OID;
            db.Entry(oBolsa).State = EntityState.Modified;
            db.SaveChanges();

            oViewModel.DOCUMENTO.SaveAs(Utils.Varios.ObtenerCarpetaImagen() + oImagen.OID + fInfo.Extension.ToString().ToLower());
        }

       

        // POST: Peticiones/Delete/5
        [HttpDelete, ActionName("Delete")]       
        public ActionResult DeleteConfirmed(int id)
        {
            BOLSA_PRUEBAS bOLSA_PRUEBAS = db.BolsaPruebas.Find(id);
            db.BolsaPruebas.Remove(bOLSA_PRUEBAS);
            db.SaveChanges();
            return new HttpStatusCodeResult(HttpStatusCode.OK);
        }

        public JsonResult GetAparatosPorGrupo(int oidGrupo, int mutua)
        {
            MUTUAS oMutua = MutuasRepositorio.Obtener(mutua);                    
                

            //VAMOS A BUSCAR PARA UN APARATO, Y UNA MUTUA QUÉ PRECIOS HAY, CON ESTO FILTRAMOS EL TERCER COMBOBOX
            if (oMutua.IOR_CENTRAL != null && oMutua.IOR_CENTRAL > 0)
            {
                mutua = oMutua.IOR_CENTRAL.Value ;
            }     


            List<APARATOS> aparatos = RadioWeb.Models.Repos.AparatoRepositorio.ListExploracionesCubiertasPorMutuaYGrupo(oidGrupo,mutua);
            

            return Json(aparatos, JsonRequestBehavior.AllowGet);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        public ActionResult AnularPeticion(int oid, int motivo)
        {
            try
            {
                BolsaPruebasRepositorio.Anular(oid, motivo);
            }
            catch
            {
                return Json(new { success = false, message = "Se ha producido un error al anular la petición." }, JsonRequestBehavior.AllowGet);
            }

            return Json(new { success = true, message = "La petición se ha anulado con éxito." }, JsonRequestBehavior.AllowGet);
        }
    }
}
