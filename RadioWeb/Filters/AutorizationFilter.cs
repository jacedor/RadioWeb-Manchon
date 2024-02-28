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
    public class AutorizationAttribute : FilterAttribute,IActionFilter
    {
        internal class Http403Result : ActionResult
        {
            public override void ExecuteResult(ControllerContext context)
            {
                // Set the response code to 403.
                context.HttpContext.Response.StatusCode = 403;
            }
        }

        internal class Http401Result : ActionResult
        {
            public override void ExecuteResult(ControllerContext context)
            {
                // Set the response code to 401.
                context.HttpContext.Response.StatusCode = 401;
            }
        }

        public void OnActionExecuting(ActionExecutingContext filterContext)
        {
            
            //Check to see if we need to skip authentication	        
            if (filterContext.ActionDescriptor.GetCustomAttributes( typeof( AllowAnonymousAttribute ), true ).Any()	      
                || filterContext.ActionDescriptor.ControllerDescriptor.GetCustomAttributes( typeof( AllowAnonymousAttribute ), true ).Any() )
                return;

            string controladorId = filterContext.ActionDescriptor.ControllerDescriptor.ControllerName;
            if (!filterContext.HttpContext.User.Identity.IsAuthenticated)
            {
                filterContext.Result = new Http401Result();
            }                
            else
            {
                string accion = filterContext.ActionDescriptor.ActionName;
                using (var oEntities = new UsersDBContext())
                {
                    var usuario = oEntities.UCCADUSER.Single(u => u.LOGIN == filterContext.HttpContext.User.Identity.Name);
                    if (usuario == null)
                    {
                        filterContext.Result = new Http403Result();
                    }
                    else if( (usuario.PERFIL== 11 && (controladorId.ToUpper() != "PETICIONES")) && (usuario.PERFIL == 11 && ((controladorId.ToUpper() != "INFORME") && accion.ToUpper() != "IMPRIMIR")) && controladorId.ToUpper()!="USERS")
                    {
                        filterContext.Result = new Http403Result();
                    }
                 
                    else
                    {
                        var objName = (controladorId + "." + accion).ToString().ToUpper();
                        var permiso = oEntities.UCCADPERM
                            .SingleOrDefault(u => u.OBJNAME.ToUpper() == objName
                                                && u.IDUSER == usuario.PERFIL);
                        //EN LA TABLA UCADPERM POR DEFECTO TODO ESTÁ PERMITIDO AUNQUE NO EXISTA EL REGISTRO CÓMO TAL
                        //SI EXISTE EL PERMISO ES O BIEN POR QUE ESTÁ DENEGADO (igual a 0) o PORQUE ESTÁ COMO SOLO LECTURA
                        //ESTADO=2
                        if (permiso != null && permiso.ESTADO == 0)
                        {
                            filterContext.Result = new Http403Result();
                        }
                        if (permiso != null && permiso.ESTADO == 2)
                        {
                            filterContext.Controller.ViewData["READONLY"] = true;
                        }

                    }
                }
            }
         
            
            
           
        }

        public void OnActionExecuted(ActionExecutedContext filterContext)
        {
            
        }
    }
}