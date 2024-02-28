using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace RadioWeb.Helpers
{
    public static class UrlExtender
    {
        public static string ContentLastWrite(this UrlHelper helper, string contentPath)
        {
            try
            {
                DateTime lastWriteTime = (new FileInfo(helper.RequestContext.HttpContext.Server.MapPath(contentPath))).LastWriteTime;
                contentPath = string.Format("{0}?v={1:yyyyMMddHHmmss}", contentPath, lastWriteTime);

                return helper.Content(contentPath);
            }
            catch
            {
                return helper.Content(contentPath);
            }
        }
    }
}