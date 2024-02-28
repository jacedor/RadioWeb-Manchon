using System;
using System.Linq.Expressions;
using System.Text;
using System.Web.Mvc;
using ADPM.Common.Extensions;
using RadioWeb.Extensions;

namespace RadioWeb.Helpers.Web.Html
{

    /// <summary>
    /// 
    /// </summary>
    public static class HiddenInputHelper
    {

        /// <summary>
        /// Returns the html for a hidden input(s) of a property.
        /// </summary>
        /// <typeparam name="TModel"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="helper"></param>
        /// <param name="expression"></param>
        /// <param name="includeID">
        /// A value indicating the the 'id' attribute should be rendered for the input.
        /// </param>
        /// <remarks>
        /// If the property is a complex type, the methods is called recursively for each property
        /// of the type. Collections and complex types with null value (except those with the 
        /// Required attribute) are ignored.
        /// </remarks>
        public static MvcHtmlString HiddenInputFor<TModel, TValue>
            (this HtmlHelper<TModel> helper, Expression<Func<TModel, TValue>> expression, bool includeID = false)
        {
            // Get the model metadata
            ModelMetadata metaData = ModelMetadata.FromLambdaExpression(expression, helper.ViewData);
            string name = ExpressionHelper.GetExpressionText(expression);
            return MvcHtmlString.Create(HiddenInput(metaData, name, includeID, true));
        }

        /// <summary>
        /// Returns the html for a hidden input(s) of a property defined by its metadata.
        /// The string is not html-encoded.
        /// </summary>
        /// <param name="metaData">
        /// The metadata of the property.
        /// </param>
        /// <param name="name">
        /// The name of the property (rendered as the 'name' attribute).
        /// </param>
        /// <param name="includeID">
        /// A value indicating the the 'id' attribute should be rendered for the input.
        /// </param>
        /// <param name="includeDefault">
        /// </param>
        /// <remarks>
        /// If the property is a complex type, the methods is called recursively for each property
        /// of the type. Collections and complex types with null value (except those with the 
        /// Required attribute) are ignored.
        /// </remarks>
        public static string HiddenInputForMetadata(ModelMetadata metaData, string name, bool includeID, bool includeDefault)
        {
            return HiddenInput(metaData, name, includeID, includeDefault);
        }

        #region .Helper methods 

        /// <summary>
        /// Returns the html for a hidden input(s) of a property.
        /// </summary>
        /// <param name="metaData">
        /// The property metadata.
        /// </param>
        /// <param name="name">
        /// The name of the property (rendered as the 'name' attribute).
        /// </param>
        /// <param name="includeID">
        /// A value indicating the the 'id' attribute should be rendered for the input.
        /// </param>
        /// <param name="includeDefault">
        /// 
        /// </param>
        private static string HiddenInput(ModelMetadata metaData, string name, bool includeID, bool includeDefault)
        {
            StringBuilder html = new StringBuilder();
            if (metaData.ModelType.IsArray && metaData.Model != null)
            {
                // Primarily for database time stamps, this need to called before checking IsComplexType
                // otherwise an endless loop is created
                html.Append(HiddenInput(name, Convert.ToBase64String(metaData.Model as byte[]), includeID));
            }
            else if (metaData.IsComplexType)
            {
                foreach (ModelMetadata property in metaData.Properties)
                {
                    if (property.IsCollection() && !property.ModelType.IsArray)
                    {
                        // This would just render the Count and Capacity property of List<T>
                        continue;
                    }
                    if (property.Model == null && property.ModelType != typeof(string) && !property.IsRequired)
                    {
                        // Ignore complex types that are null and do not have the RequiredAttribute
                        continue;
                    }
                    // Recursive call to render a hidden input for the property
                    string prefix = string.Format("{0}.{1}", name, property.PropertyName);
                    html.Append(HiddenInput(property, prefix, includeID, false));
                }
            }
            else
            {
                object value = metaData.Model;
                if (includeDefault && value == null)
                {
                    value = metaData.DefaultValue();
                }
                html.Append(HiddenInput(name, value, includeID));
            }
            return html.ToString();
        }

        /// <summary>
        /// Returns the html for a hidden input.
        /// </summary>
        /// <param name="name">
        /// The name of the property.
        /// </param>
        /// <param name="value">
        /// The value of the property.
        /// </param>
        /// <param name="includeID">
        /// A value indicating the the 'id' attribute should be rendered for the input.
        /// </param>
        /// <returns></returns>
        private static string HiddenInput(string name, object value, bool includeID)
        {
            TagBuilder input = new TagBuilder("input");
            input.MergeAttribute("type", "hidden");
            if (includeID)
            {
                input.MergeAttribute("id", HtmlHelper.GenerateIdFromName(name));
            }
            input.MergeAttribute("name", name);
            input.MergeAttribute("value", string.Format("{0}", value));
            return input.ToString(TagRenderMode.SelfClosing);
        }

        #endregion

    }

}