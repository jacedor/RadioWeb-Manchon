﻿using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Web.Mvc;

namespace ADPM.Common
{
    public static class ControllerExtensions
    {

        

        public static string RenderView(this Controller controller, string viewName, object model)
        {
            return RenderView(controller, viewName, new ViewDataDictionary(model));
        }

        public static string RenderView(this Controller controller, string viewName, ViewDataDictionary viewData)
        {
            var controllerContext = controller.ControllerContext;

            var viewResult = ViewEngines.Engines.FindView(controllerContext, viewName, null);

            StringWriter stringWriter;

            using (stringWriter = new StringWriter())
            {
                var viewContext = new ViewContext(
                    controllerContext,
                    viewResult.View,
                    viewData,
                    controllerContext.Controller.TempData,
                    stringWriter);

                viewResult.View.Render(viewContext, stringWriter);
                viewResult.ViewEngine.ReleaseView(controllerContext, viewResult.View);
            }

            return stringWriter.ToString();
        }
    }
}
