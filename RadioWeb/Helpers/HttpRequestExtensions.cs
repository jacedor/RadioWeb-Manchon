using System;
using System.Web;
using System.Net;
using System.Linq;


namespace RadioWeb.Helpers
{

    /// <summary>
    /// Extends HttpRequest class with additional methods.
    /// </summary>
    public static class HttpRequestExtensions
    {
        /// <summary>
        /// Gets the IP address of the request.
        /// This method is more useful than built in because in 
        /// some cases it may show real user IP address even under proxy.
        /// The <see cref="System.Net.IPAddress.None" /> value 
        /// will be returned if getting is failed.
        /// </summary>
        /// <param name="request">The HTTP request object.</param>
        /// <returns>IPAddress object</returns>
        public static IPAddress GetIp(this HttpRequestBase request)
        {
            string ipString;
            if (string.IsNullOrEmpty(request.ServerVariables["HTTP_X_FORWARDED_FOR"]))
            {
                ipString = request.ServerVariables["REMOTE_ADDR"];
            }
            else
            {
                ipString = request.ServerVariables["HTTP_X_FORWARDED_FOR"]
                   .Split(",".ToCharArray(),
                   StringSplitOptions.RemoveEmptyEntries).FirstOrDefault();
            }

            IPAddress result;
            if (!IPAddress.TryParse(ipString, out result))
            {
                result = IPAddress.None;
            }

            return result;
        }
    }
}
