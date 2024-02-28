using System;
using System.Net;
using System.Net.Http;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Web.Http;
using System.Web.Http.Controllers;

public class BasicAuthenticationAttribute : AuthorizeAttribute
{
    public override void OnAuthorization(HttpActionContext actionContext)
    {
        if (actionContext.Request.Headers.Authorization == null)
        {
            actionContext.Response = actionContext.Request.CreateResponse(HttpStatusCode.Unauthorized);
        }
        else
        {
            string authenticationToken = actionContext.Request.Headers.Authorization.Parameter;
            string decodedToken = Encoding.UTF8.GetString(Convert.FromBase64String(authenticationToken));
            string[] usernamePasswordArray = decodedToken.Split(':');
            string username = usernamePasswordArray[0];
            string password = usernamePasswordArray[1];

            if (IsAuthorized(username, password))
            {
                Thread.CurrentPrincipal = new GenericPrincipal(new GenericIdentity(username), null);
            }
            else
            {
                actionContext.Response = actionContext.Request.CreateResponse(HttpStatusCode.Unauthorized);
            }
        }
    }

    private bool IsAuthorized(string username, string password)
    {
        // Aquí puedes implementar la lógica de autenticación, por ejemplo, verificar las credenciales en una base de datos
        return username.ToUpper() == "AffideaTuoTempo".ToUpper() && password == "Pj9aJPu[3jPj9aJPu[3jPj9aJPu[3j";
    }
}
