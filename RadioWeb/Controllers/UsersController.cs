using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using FirebirdSql.Data.FirebirdClient;
using System.Web.Security;
using RadioWeb.Utils;
using RadioWeb.Models;
using RadioWeb.Models.Repos;
using System.Threading;
using RadioWeb.Filters;
using RadioWeb.Models.Logica;
using RadioWeb.Repositories;

namespace RadioWeb.Controllers
{
    [Autorization]
    public class UsersController : Controller
    {
        private readonly UsersDBContext _ctx;
        private ParametrosUsuarioRepository _ParametrosUsuarioRepo;

        public UsersController()
        {
            _ctx = new UsersDBContext();
            _ParametrosUsuarioRepo = new ParametrosUsuarioRepository(_ctx);
        }

        [AllowAnonymous]
        [HttpPost]
        public ActionResult Login(USUARIO user)
        {
            ViewBag.NombreEmpresa = System.Configuration.ConfigurationManager.AppSettings["NombreEmpresa"];

            if (ModelState.IsValid)
            {
                USUARIO usuarioAutenticated = UsuariosRepositorio.IsValid(user.LOGIN, user.PASSWORDWEB);
                if (usuarioAutenticated!=null && usuarioAutenticated.LOGIN != null)
                {
                    if (String.IsNullOrEmpty(usuarioAutenticated.ENCRIPTADO) || usuarioAutenticated.ENCRIPTADO == "F")
                    {
                        USUARIO oEncriptarClave = _ctx.UCCADUSER.Single(u => u.IDUSER == usuarioAutenticated.IDUSER);
                        oEncriptarClave.PASSWORDWEB = SHA1.Encode(user.PASSWORDWEB);
                        oEncriptarClave.ENCRIPTADO = "T";
                        oEncriptarClave.PASSWORD = "";
                        _ctx.SaveChanges();
                    }
                    _ParametrosUsuarioRepo.Update(DateTime.Now.ToString("dd/MM/yyyy"), usuarioAutenticated.IDUSER, "HOME", "FECHA");
                    Session["Usuario"] = usuarioAutenticated;

                    //Si se está accediendo a la app desde el exterior
                    if (!Request.UserHostAddress.StartsWith("::1"))
                    {
                        //Si es usuario privilegiado o es un usuario del perfil del CENTRO DE MANRESA se le da acceso y se apunta en el log                       
                        RadioWeb.Utils.LogLopd.Insertar("Entrada exterior: " + usuarioAutenticated.IDUSER + " - " + Request.UserHostAddress + " nombre: " + usuarioAutenticated.NOME, "0");
                     
                         FormsAuthentication.SetAuthCookie(usuarioAutenticated.LOGIN,false);
                        // Modificar la cookie para que sea Secure y HttpOnly
                        HttpCookie authCookie = Response.Cookies[FormsAuthentication.FormsCookieName];
                        if (authCookie != null)
                        {
                            authCookie.Secure = true;
                            authCookie.HttpOnly = true;
                        }
                    }//                    SI ACCEDE DESDE LA LAN
                    else
                    {
                        RadioWeb.Utils.LogLopd.Insertar("Entrada en el sistema usuario: " + usuarioAutenticated.IDUSER + " nombre: " + usuarioAutenticated.NOME, "0");
                     
                        FormsAuthentication.SetAuthCookie(usuarioAutenticated.LOGIN,false);
                        // Modificar la cookie para que sea Secure y HttpOnly
                        HttpCookie authCookie = Response.Cookies[FormsAuthentication.FormsCookieName];
                        if (authCookie != null)
                        {
                            authCookie.Secure = true;
                            authCookie.HttpOnly = true;
                        }
                    }


                    //Comprobamos si la contraseña del usuario está caducada.
                    if (IsPasswordExpired(usuarioAutenticated))
                    {
                        return RedirectToAction("ForzarCambioPassword");
                    }



                    if (Thread.CurrentPrincipal.IsInRole("RW-MedicoInformante"))
                    {
                        return this.RedirectToAction("ExploracionesPendientes", "Informe");
                    }
                    else if (Thread.CurrentPrincipal.IsInRole("RW-MedicoSolicitante") ||
                        (usuarioAutenticated.IOR_COLEGIADO.HasValue && usuarioAutenticated.IOR_COLEGIADO > 0) ||
                        (usuarioAutenticated.IOR_ENTIDADPAGADORA.HasValue && usuarioAutenticated.IOR_ENTIDADPAGADORA > 0) ||
                        (usuarioAutenticated.IOR_CENTROEXTERNO.HasValue && usuarioAutenticated.IOR_CENTROEXTERNO > 0))
                    {
                        if (usuarioAutenticated.CREARPETICIONES == "F")
                        {
                            return this.RedirectToAction("ListExploraciones", "Peticiones");
                        }
                        else
                        {
                            return this.RedirectToAction("Create", "Peticiones");
                        }
                    }
                    {
                        return this.RedirectToAction("Index", "Calendario");
                    }


                }
                else
                {
                    RadioWeb.Utils.LogLopd.Insertar("Error de login de usuario: " + user.LOGIN + " password: " + user.PASSWORDWEB, "0");

                    //Si se ha encontrado el usuario
                    USUARIO oUsuario = _ctx.UCCADUSER.SingleOrDefault(u => u.LOGIN.ToUpper() == user.LOGIN.ToUpper());
                    if (oUsuario != null)
                    {

                        WebConfigRepositorio oConfig = new WebConfigRepositorio();
                        string sIntentosBloqueo = oConfig.ObtenerValor("INTENTOS_BLOQUEO");
                        int intentosBloqueo = int.Parse(sIntentosBloqueo);


                        int? intentos = oUsuario.LOGIN_ATTEMPTS;

                        if (intentos == null)
                        {
                            intentos = 0;
                        }

                        intentos++;
                        if (intentos >= intentosBloqueo)
                        {
                            oUsuario.BLOQUEADO = "T";
                            ViewBag.UsuarioBloqueado = "Usuario bloqueado. Contacte con el administrador.";
                        }
                        else
                        {
                            ViewBag.UsuarioBloqueado = "Intento de inicio de sesion no válido.";
                        }

                        oUsuario.LOGIN_ATTEMPTS = intentos;
                        // Desactivar validaciones del modelo
                        var validateOnSaveEnabled = _ctx.Configuration.ValidateOnSaveEnabled;
                        _ctx.Configuration.ValidateOnSaveEnabled = false;

                        try
                        {
                            _ctx.SaveChanges();
                        }
                        finally
                        {
                            // Reactivar validaciones del modelo
                            _ctx.Configuration.ValidateOnSaveEnabled = validateOnSaveEnabled;
                        }

                    }

                    return View("Index");
                }
            }

            return View("Index");


        }

        [AllowAnonymous]
        public ActionResult Index()
        {
            ViewBag.NombreEmpresa = System.Configuration.ConfigurationManager.AppSettings["NombreEmpresa"];
            if (!Request.UserHostAddress.StartsWith("::1"))
            {
                return View();
            }
            else
            {
                return View();
            }
        }

        [HttpGet]
        public ActionResult Edit(int oid)
        {
            var user = _ctx.UCCADUSER.Single(h => h.IDUSER == oid);
            var personal = PersonalRepositorio.Obtener(user.LOGIN);
            if (personal.OID > 0)
            {
                user.IOR_PERSONAL = personal.OID;
            }
            return View("UserForm", user);
        }

        [HttpPost]
        public ActionResult Edit(USUARIO user)
        {
            if (!ModelState.IsValid)
            {
                return View("UserForm", user);
            }
            var usuario = _ctx.UCCADUSER.Single(h => h.IDUSER == user.IDUSER);

            usuario.LOGIN = user.LOGIN;
            usuario.NOME = user.NOME;
            if (usuario.PASSWORDWEB != user.PASSWORDWEB)
            {
                usuario.PASSWORDWEB = SHA1.Encode(user.PASSWORDWEB);
                usuario.ENCRIPTADO = "T";
            }
            usuario.PERFIL = user.PERFIL;
            usuario.EMAIL = user.EMAIL;
            usuario.LOGIN_ATTEMPTS = 0;
            usuario.BLOQUEADO = user.BLOQUEADO;
            usuario.IOR_CENTROEXTERNO = user.IOR_CENTROEXTERNO;
            usuario.IOR_ENTIDADPAGADORA = user.IOR_ENTIDADPAGADORA;
            usuario.IOR_COLEGIADO = user.IOR_COLEGIADO;
            usuario.CITACIONONLINE = user.CITACIONONLINE;
            usuario.CREARPETICIONES = user.CREARPETICIONES;
            usuario.LOGIN_ATTEMPTS = 0;
            usuario.BLOQUEADO = user.BLOQUEADO;
            if (user.IOR_PERSONAL > 0)
            {
                var personalRelacionado = PersonalRepositorio.UpdateCampo("LOGIN", user.LOGIN, user.IOR_PERSONAL);
            }
            _ctx.SaveChanges();

            return Redirect(String.Format("{0}?perfil={1}&user={2}#{3}", 
                Url.RouteUrl(new { controller = "Permisos", action = "Index" }),usuario.PERFIL,usuario.IDUSER,"tab-usuarios"));
             

        }


        public ActionResult List()
        {
            List<USUARIO> oModel = new List<USUARIO>();
            oModel = _ctx.UCCADUSER
                .Where(u => u.TIPO == "U")
                .OrderBy(u => u.LOGIN).ToList();
            return PartialView("_List", oModel);
        }

        [HttpPost]
        public ActionResult List(int IOR_ROLE3,string TipoUsuario, string SearchTerm)
        {
            IEnumerable<USUARIO> oModel = new List<USUARIO>();
            oModel = _ctx.UCCADUSER
                .Where(u => u.TIPO == "U" );
            //si no hay filtro por Perfil mostramos todos los usuarios
            if (IOR_ROLE3 > 0)
            {
                oModel = oModel.Where(u => u.PERFIL == IOR_ROLE3);

            };

            // Filtro por texto de búsqueda (Login o Nombre en Personal)
            if (!string.IsNullOrEmpty(SearchTerm))
            {
                string lowerSearchTerm = SearchTerm.ToLower(); // Convertir el término de búsqueda a minúsculas
                oModel = oModel.Where(u =>
                    u.LOGIN.ToLower().Contains(lowerSearchTerm) || // Comparar login sin sensibilidad a mayúsculas
                    (u.PERSONAL != null && u.PERSONAL.NOMBRE.ToLower().Contains(lowerSearchTerm)) // Comparar nombre en Personal sin sensibilidad a mayúsculas
                );
            }
            //como la propiedad descripcion del perfil es una relacion con la misma tabla 
            //en este bucle lo rellenamos con un nombre amigable
            foreach (var item in oModel)
            {
                var perfil= _ctx.UCCADUSER.SingleOrDefault(u => u.IDUSER == item.PERFIL);
                if (perfil!=null)
                {
                    item.DESCPERFILWEB = perfil.NOME;

                }
                var personal = PersonalRepositorio.Obtener(item.LOGIN);
                if (personal.OID > 0)
                {
                    item.IOR_PERSONAL = personal.OID;
                    item.PERSONAL = personal;
                }
            }

            if (TipoUsuario=="1")
            {
                oModel = oModel.Where(p => !p.IOR_CENTROEXTERNO.HasValue && !p.IOR_COLEGIADO.HasValue && !p.IOR_ENTIDADPAGADORA.HasValue);
            }

            if (TipoUsuario == "2")
            {
                oModel = oModel.Where(p =>
                    (p.IOR_CENTROEXTERNO.HasValue && p.IOR_CENTROEXTERNO.Value > 0) ||
                    (p.IOR_COLEGIADO.HasValue && p.IOR_COLEGIADO.Value > 0) ||
                    (p.IOR_ENTIDADPAGADORA.HasValue && p.IOR_ENTIDADPAGADORA.Value > 0)
                    );

            }

            return PartialView("_List", oModel.OrderBy(u => u.LOGIN).ToList());
        }



        [HttpGet]
        public ActionResult CambioPassword()
        {
            USUARIO usuarioAutenticated = (USUARIO)Session["Usuario"];
            //USUARIO user = _ctx.UCCADUSER.Single(u => u.IDUSER == usuarioAutenticated.IDUSER);

            CambioPassword cp = new CambioPassword();
            cp.LOGIN = usuarioAutenticated.LOGIN;
            cp.IDUSER = usuarioAutenticated.IDUSER;
            return View("CambioPassword", cp);
        }

        [HttpPost]
        public ActionResult CambioPassword(CambioPassword CambioPassword)
        {

            ViewBag.NombreEmpresa = System.Configuration.ConfigurationManager.AppSettings["NombreEmpresa"];

            USUARIO usuarioAutenticated = (USUARIO)Session["Usuario"];
            CambioPassword cp = new CambioPassword();
            cp.LOGIN = usuarioAutenticated.LOGIN;
            cp.IDUSER = usuarioAutenticated.IDUSER;


            if (!ModelState.IsValid)
            {
                return View("CambioPassword", cp);
            }


            String PasswordOldSha1 = SHA1.Encode(CambioPassword.PasswordOld);
            //La contraseña antigua no es válida.
            if (!PasswordOldSha1.Equals(usuarioAutenticated.PASSWORDWEB))
            {
                ViewBag.Error = "La contraseña actual no es correcta.";
                return View("CambioPassword", cp);
            }

            //Si las contraseñas nuevas no coinciden.
            if (!CambioPassword.Password1.Equals(CambioPassword.Password2))
            {
                ViewBag.Error = "Las contraseñas no coinciden.";
                return View("CambioPassword", cp);
            }

            //Si la contraseña actual es la misma que la anterior
            if (SHA1.Encode(CambioPassword.Password1).Equals(usuarioAutenticated.PASSWORDWEB))
            {
                ViewBag.Error = "La nueva contraseña no puede ser la misma que la anterior.";
                return View("CambioPassword", cp);
            }

            //Cambiamos la contraseña del usuario y actualizamos la fecha de cambio.
            USUARIO oCambioPassword = _ctx.UCCADUSER.Single(u => u.IDUSER == usuarioAutenticated.IDUSER);
            oCambioPassword.PASSWORDWEB = SHA1.Encode(CambioPassword.Password1);
            oCambioPassword.FECHA_PASSWORD = DateTime.Now;
            _ctx.SaveChanges();


            //Desconectamos la sessión
            return this.RedirectToAction("LogOut", "Users");
        }



        public ActionResult ForzarCambioPassword()
        {
            ViewBag.NombreEmpresa = System.Configuration.ConfigurationManager.AppSettings["NombreEmpresa"];


            USUARIO usuarioAutenticated = (USUARIO)Session["Usuario"];
            //USUARIO user = _ctx.UCCADUSER.Single(u => u.IDUSER == usuarioAutenticated.IDUSER);

            CambioPassword cp = new CambioPassword();
            cp.LOGIN = usuarioAutenticated.LOGIN;
            cp.IDUSER = usuarioAutenticated.IDUSER;

            return View("ForzarCambioPassword", cp);
        }


        [HttpPost]
        public ActionResult ForzarCambioPassword(CambioPassword forzarCambioPassword)
        {

            ViewBag.NombreEmpresa = System.Configuration.ConfigurationManager.AppSettings["NombreEmpresa"];
            if (!ModelState.IsValid)
            {
                return View();
            }

            USUARIO usuarioAutenticated = (USUARIO)Session["Usuario"];
            String PasswordOldSha1 = SHA1.Encode(forzarCambioPassword.PasswordOld);

            //La contraseña antigua no es válida.
            if (!PasswordOldSha1.Equals(usuarioAutenticated.PASSWORDWEB))
            {
                ViewBag.Error = "La contraseña actual no es correcta.";
                return View();
            }

            //Si las contraseñas nuevas no coinciden.
            if (!forzarCambioPassword.Password1.Equals(forzarCambioPassword.Password2))
            {
                ViewBag.Error = "Las contraseñas no coinciden.";
                return View();
            }

            //Si la contraseña actual es la misma que la anterior
            if (SHA1.Encode(forzarCambioPassword.Password1).Equals(usuarioAutenticated.PASSWORDWEB))
            {
                ViewBag.Error = "La nueva contraseña no puede ser la misma que la anterior.";
                return View();
            }

            //Cambiamos la contraseña del usuario y actualizamos la fecha de cambio.
            USUARIO oCambioPassword = _ctx.UCCADUSER.Single(u => u.IDUSER == usuarioAutenticated.IDUSER);
            oCambioPassword.PASSWORDWEB = SHA1.Encode(forzarCambioPassword.Password1);
            oCambioPassword.FECHA_PASSWORD = DateTime.Now;
            _ctx.SaveChanges();

            //Desconectamos la sessión
            return this.RedirectToAction("LogOut", "Users");
        }

        private Boolean IsPasswordExpired(USUARIO usuarioAutenticated)
        {
            
            WebConfigRepositorio oConfig = new WebConfigRepositorio();
            String sDiasExpiracion = oConfig.ObtenerValor("EXPIRACION_PASSWORD");

            //Comprobamos si el valor no esta informado en la webconfig o es 0.
            //Si no está configurado o está a 0 no validamos la contraseña.
            if (String.IsNullOrEmpty(sDiasExpiracion) || Double.Parse(sDiasExpiracion) == 0)
            {
                return false;
            }


            //Comprobamos si el usuario ya ha realizado algun cambio de contraseña.
            //En caso contrario, forzamos el cambio.
            if (!usuarioAutenticated.FECHA_PASSWORD.HasValue)
            {
                return true;
            }            

            Double diasExpiracion = Double.Parse(sDiasExpiracion);
            DateTime fechaRenovacion = usuarioAutenticated.FECHA_PASSWORD.Value;
            DateTime fechaExpiracion = fechaRenovacion.AddDays(diasExpiracion);
            DateTime fechaActual = DateTime.Now;

            //Comprobamos si la contraseña está caducada.
            if (fechaActual.CompareTo(fechaExpiracion) > 0)
            {
                return true;
            }            

            return false;
        }
       

        public ActionResult IPQuery()
        {
            //System.Net.IPAddress oIp = RadioWeb.Helpers.HttpRequestExtensions.GetIp(this.Request);
            ViewBag.Ip = Request.UserHostAddress;
            return View();
        }


        public ActionResult LogOut()
        {
            RadioWeb.Utils.LogLopd.Insertar("Salida de la aplicación desde LogOff: ", "0");
            FormsAuthentication.SignOut();
            Session["Usuario"] = null; //it's my session variable
            Session.Clear();
            Session.Abandon();
            
            Session["Usuario"] = null;
            return this.RedirectToAction("Index", "Users");

        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _ctx.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
