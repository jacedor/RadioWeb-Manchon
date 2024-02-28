using RadioWeb.Models;
using RadioWeb.Models.Logica;
using RadioWeb.Models.Repos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace RadioWeb.Filters
{
    public class ConfiguracionVisualAttribute : FilterAttribute, IActionFilter
    {
        public string Key { get; set; }

        internal class Http403Result : ActionResult
        {
            public override void ExecuteResult(ControllerContext context)
            {
                // Set the response code to 403.
                context.HttpContext.Response.StatusCode = 403;
            }
        }


        public void OnActionExecuting(ActionExecutingContext filterContext)
        {
            //Check to see if we need to skip authentication	        
            if (filterContext.ActionDescriptor.GetCustomAttributes(typeof(AllowAnonymousAttribute), true).Any()
                || filterContext.ActionDescriptor.ControllerDescriptor.GetCustomAttributes(typeof(AllowAnonymousAttribute), true).Any())
                return;

            UsersDBContext oEntities = new UsersDBContext();
            string controladorId = filterContext.ActionDescriptor.ControllerDescriptor.ControllerName;           
            string accion = filterContext.ActionDescriptor.ActionName;
            if (controladorId == "Calendario" && (accion=="Index" || accion=="ResumenDiario"))
            {
                controladorId = "Home";
            }
            if (controladorId == "Exploracion" && accion == "ImprimirLista")
            {
                controladorId = "Home";
            }
            var usuario = oEntities.UCCADUSER.SingleOrDefault(u => u.LOGIN == filterContext.HttpContext.User.Identity.Name);
            if (usuario == null)
            {

            }
            else
            {
                var objName = (controladorId + "." + accion).ToString().ToUpper();
                var configuciones = oEntities.UCCADPERM
                    .Where(u => u.OBJNAME.ToUpper().StartsWith(controladorId.ToUpper() + ".")
                    && u.IDUSER == usuario.IDUSER
                    && (u.MODULO.ToUpper().StartsWith("RADIOWEB")) ||
                    (u.IDUSER == usuario.PERFIL && u.MODULO.ToUpper().StartsWith("RADIOWEBPERMISO")));

                foreach (var item in configuciones)
                {
                    if (item.ESTADO == -999)
                    {
                        filterContext.Controller.ViewData[item.OBJNAME.ToUpper()] = item.VALOR;
                    }
                    else {
                        filterContext.Controller.ViewData[item.OBJNAME.ToUpper()] = item.ESTADO;

                    }
                }


            }
        }

        public void OnActionExecuted(ActionExecutedContext filterContext)
        {

        }
    }
}