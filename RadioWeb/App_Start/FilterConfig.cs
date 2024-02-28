using RadioWeb.Filters;
using System.Web;
using System.Web.Mvc;

namespace RadioWeb
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            //filters.Add(new ExceptionLoggingFilter());
            filters.Add(new AutorizationAttribute());
            //   filters.Add(new SessionStateAttribute());
        }
    }
}