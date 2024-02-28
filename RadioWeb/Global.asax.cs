using RadioWeb.Models.Repos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using FirebirdSql.Data.FirebirdClient;
using System.Web.Security;
using RadioWeb.Models;

namespace RadioWeb
{
    // Nota: para obtener instrucciones sobre cómo habilitar el modo clásico de IIS6 o IIS7, 
    // visite http://go.microsoft.com/?LinkId=9394801

    public class MvcApplication : System.Web.HttpApplication
    {
   
        public static  List<USUARIO> UsuariosConectados = new List<USUARIO>();
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            Application["Mutuas"] = MutuasRepositorio.Lista(false);
            Application["DAparatos"] = DaparatoRepositorio.Lista();
            Application["Centros"] = CentrosRepositorio.List();
            Application["GrupoAparatos"] = GAparatoRepositorio.Lista();
            Application["CentrosExternos"] = CentrosExternosRepositorio.Obtener();
            Application["PlantillasCorreoSMS"] = EmailRepositorio.ListaPlantillas();
            Application["Medicos"] = PersonalRepositorio.ObtenerMedicos();
            Application["Estudiantes"] = PersonalRepositorio.ObtenerEstudiantes();
            Application["Tecnicos"] = PersonalRepositorio.ObtenerTecnicos();

            WebApiConfig.Register(GlobalConfiguration.Configuration);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }

        protected void Application_BeginRequest(object sender, EventArgs e)
        {
            var context = new HttpContextWrapper(Context);
            // set flag only if forms auth enabled and request comes from ajax
            if (FormsAuthentication.IsEnabled && context.Request.IsAjaxRequest())
            {
                context.Response.SuppressFormsAuthenticationRedirect = true;
            }
        }

        protected void Session_End(Object sender, EventArgs e)
        {
            FirebirdSql.Data.FirebirdClient.FbConnection.ClearAllPools();
            
        }

        protected void Session_Start()
        {
            //Session["Mutuas"] = MutuasRepositorio.Lista(true);
            //Session["DAparatos"] = DaparatoRepositorio.Lista();
            //Session["Centros"] = CentrosRepositorio.List();

            //Session["GrupoAparatos"] = GAparatoRepositorio.Lista();
           
            //Session["CentrosExternos"] = CentrosExternosRepositorio.Obtener();
           
          //  Session["Tecnicos"] = PersonalRepositorio.ObtenerTecnicos();
            //Session["Estudiantes"] = PersonalRepositorio.ObtenerEstudiantes();
            

        }
    }
}