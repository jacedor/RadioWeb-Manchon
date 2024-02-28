using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace RadioWeb.Helpers
{
    public class ImageHelper
    {
        /// <summary>Converts a photo to a base64 string.</summary>
        /// <param name="html">The extended HtmlHelper.</param>
        /// <param name="fileNameandPath">File path and name.</param>
        /// <returns>Returns a base64 string.</returns>
        public static MvcHtmlString PhotoBase64ImgSrc(string fileNameandPath)
        {
            try
            {
                var byteArray = File.ReadAllBytes(fileNameandPath);
                var base64 = Convert.ToBase64String(byteArray);

                return MvcHtmlString.Create(String.Format("data:image/gif;base64,{0}", base64));
            }
            catch (Exception)
            {
                return MvcHtmlString.Create("");
            }
            
        }
    }
}