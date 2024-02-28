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
    public class ExceptionLoggingFilter : FilterAttribute, IExceptionFilter
    {
        public void OnException(ExceptionContext filterContext)
        {
            //if (filterContext.HttpContext.Request.Headers["X-Request-With"] == "XMLHttpRequest")
            //{
            //if (filterContext.HttpContext.Request.IsAjaxRequest())
            //{
            //    filterContext.Result = new JsonResult
            //    {
            //        JsonRequestBehavior = JsonRequestBehavior.AllowGet,
            //        Data = new
            //        {
            //            Message = "Se ha producido un error",
            //        }
            //    };
            //}
            if (filterContext.HttpContext.Session["Usuario"] == null || !filterContext.HttpContext.Request.IsAuthenticated)
            {
                if (filterContext.HttpContext.Request.IsAjaxRequest())
                {
                    filterContext.Result = new JsonResult
                    {
                        JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                        Data = new
                        {
                            Message = "_data_",
                        }
                    };
                }

            }
            filterContext.HttpContext.Response.StatusCode = 500;
            filterContext.ExceptionHandled = true;
            using (var oEntities = new UsersDBContext())
            {
                var usuario = oEntities.UCCADUSER.Single(u => u.LOGIN == filterContext.HttpContext.User.Identity.Name);
                string excepcion=filterContext.Controller.GetType().Name + "-" + filterContext.RouteData.Values["action"];
                excepcion = excepcion + " " + filterContext.Exception.Message;
                if (excepcion.Length > 100)
                {
                    excepcion = excepcion.Substring(0, 99);
                }
                var error = new LOGERRORS
                {
                    FECHA = DateTime.Now.ToString("yyyyMMdd"),
                    USERNAME = usuario.LOGIN,
                    CID=1,
                    CANAL = DateTime.Now.ToString("HH:mm"),
                    ERROR = excepcion
                };
                oEntities.LOGERRORS.Add(error);
                oEntities.SaveChanges();
            }
        }
    }
}