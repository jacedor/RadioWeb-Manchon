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
    public class ExploracionActivaFilterAttribute : FilterAttribute,IActionFilter
    {

        public void OnActionExecuted(ActionExecutedContext filterContext)
        {
             try
            {
                //  get the view bag

                var viewBag = filterContext.Controller.ViewBag;               
                var oidExploracionActiva = viewBag.ExploracionActiva;

                FiltrosBusquedaExploracion oFiltros = null;
                if (filterContext.HttpContext.Session["FiltrosBusqueda"] != null)
                {
                    oFiltros = (FiltrosBusquedaExploracion)filterContext.HttpContext.Session["FiltrosBusqueda"];
                    oFiltros.oidExploracionSeleccionada = (int)oidExploracionActiva;
                    filterContext.HttpContext.Session["FiltrosBusqueda"] = oFiltros;
                }
            }
            catch (Exception ex)
            {
                

            }
            
        }

        public void OnActionExecuting(ActionExecutingContext filterContext)
        {
           
        }
    }
}