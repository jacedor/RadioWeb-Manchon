using RadioWeb.Models;
using RadioWeb.Models.Logica;
using RadioWeb.Models.Repos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Web;

namespace RadioWeb.Filters
{
    public class VidSignerBasicAuthenticationAttribute : System.Web.Http.Filters.ActionFilterAttribute
    {
        public override void OnActionExecuting(System.Web.Http.Controllers.HttpActionContext actionContext)
        {

            if (actionContext.Request.Headers.Authorization == null)
            {
                actionContext.Response = new System.Net.Http.HttpResponseMessage(System.Net.HttpStatusCode.Unauthorized);
            }
            else
            {
                string authToken = actionContext.Request.Headers.Authorization.Parameter;
                string decodedToken = Encoding.UTF8.GetString(Convert.FromBase64String(authToken));
                string username = decodedToken.Substring(0, decodedToken.IndexOf(":"));
                string password = decodedToken.Substring(decodedToken.IndexOf(":") + 1);

                WebConfigRepositorio oConfig = new WebConfigRepositorio();
                bool autenticado = false;
                try
                {
                    string centrosVid = oConfig.ObtenerValor("CentrosVidSigner");
                    string[] centrosArray = centrosVid.Split(',');

                    foreach(string i in centrosArray)
                    {
                        try
                        {
                            string userVid = oConfig.ObtenerValor("UserNameVidSigner" + i.ToString());
                            string passVid = oConfig.ObtenerValor("PasswordVidSigner" + i.ToString());
                            if (username.ToUpper() == userVid.ToUpper()
                                && password == passVid)
                            {
                                USUARIO oUSer = new USUARIO { LOGIN = userVid };
                                HttpContext.Current.User = new GenericPrincipal(new ApiIdentity(oUSer), new string[] { });
                                autenticado = true;
                                base.OnActionExecuting(actionContext);
                            }
                        }
                        catch (Exception)
                        {

                            
                        }
                        
                    }

                }
                catch (Exception ex)
                {
                    if (!autenticado)
                    {
                        actionContext.Response = new System.Net.Http.HttpResponseMessage(System.Net.HttpStatusCode.Unauthorized);
                    }
                }

                if (!autenticado)
                {
                    actionContext.Response = new System.Net.Http.HttpResponseMessage(System.Net.HttpStatusCode.Unauthorized);

                }

            }
        }
    }
}