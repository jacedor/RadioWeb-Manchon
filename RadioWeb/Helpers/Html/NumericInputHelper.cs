using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.Mvc;
using ADPM.Common.Extensions;
using RadioWeb.Extensions;

namespace RadioWeb.Helpers.Web.Html
{

    /// <summary>
    /// Renders the html for a custom numeric control.
    /// </summary>
    public static class NumericInputHelper
    {

        #region .Declarations 

        // Error messages
        private const string _NotNumeric = "The property {0} is type {1} which is not numeric";

        #endregion

        #region .Methods 

        /// <summary>
        /// Returns the html for a numeric input that formats the display of values.
        /// </summary>
        /// <typeparam name="TModel"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="helper">
        /// The HtmlHelper instance that this method extends.
        /// </param>
        /// <param name="expression">
        /// An expression that identifies the property to display.
        /// </param>
        /// <exception cref="System.ArgumentException">
        /// If the property is type not a numeric type.
        /// </exception>
        public static MvcHtmlString NumericInputFor<TModel, TValue> (this HtmlHelper<TModel> helper, 
            Expression<Func<TModel, TValue>> expression)
        {
            // Get the model metadata
            ModelMetadata metaData = ModelMetadata.FromLambdaExpression(expression, helper.ViewData);
            // Get the fully qualified name of the property
            string fieldName = ExpressionHelper.GetExpressionText(expression);
            // Get the validation attributes
            IDictionary<string, object> attributes = helper.GetUnobtrusiveValidationAttributes(fieldName, metaData);
            // Return the html
            return MvcHtmlString.Create(NumericInput(metaData, fieldName, attributes, true));
        }

        /// <summary>
        /// Returns the html for a numeric input that formats the display of values.
        /// </summary>
        /// <param name="metaData">
        /// The metadata of the property.
        /// </param>
        /// <param name="fieldName">
        /// The fully qualified name of the property.
        /// </param>
        /// <param name="attributes">
        /// The validation attributes to rendered in the input.
        /// </param>
        /// <param name="includeID">
        /// A value indicating the the 'id' attribute should be rendered for the input.
        /// </param>
        /// <returns></returns>
        public static string NumericInputForMetadata(ModelMetadata metaData,
            string fieldName, IDictionary<string, object> attributes, bool includeID = false)
        {
            // Check its numeric
            if (!metaData.ModelType.IsNumeric())
            {
                throw new ArgumentException(string
                    .Format(_NotNumeric, fieldName, metaData.ModelType.Name));
            }
            return NumericInput(metaData, fieldName, attributes, includeID);
        }

        #endregion

        #region .Helper methods 

        /// <summary>
        /// Returns the html for a numeric input that formats the display of values.
        /// </summary>
        /// <param name="metaData">
        /// The metadata of the property.
        /// </param>
        /// <param name="fieldName">
        /// The fully qualified name of the property.
        /// </param>
        /// <param name="attributes">
        /// The validation attributes to rendered in the input.
        /// </param>
        /// <param name="includeID">
        /// A value indicating the the 'id' attribute should be rendered for the input.
        /// </param>
        private static string NumericInput(ModelMetadata metaData, string fieldName,
            IDictionary<string, object> attributes, bool includeID)
        {
            string defaultFormat = null;
            StringBuilder html = new StringBuilder();



            // Build formatted display text
            TagBuilder display = new TagBuilder("div");
            // Add essential styles
            display.MergeAttribute("style", "position:absolute;");
            // Add class names
            display.AddCssClass("numeric-text");
            if (Convert.ToDouble(metaData.Model) < 0)
            {
                display.AddCssClass("negative");
            }
            if (metaData.IsCurrency())
            {
                defaultFormat = "{0:C}";
                display.AddCssClass("currency");
            }
            else if (metaData.IsPercent())
            {
                defaultFormat = "{0:P}";
                display.AddCssClass("percent");
            }
            else
            {
                defaultFormat = "{0:0.00}";
                display.AddCssClass("number");
            }
            // Format text
            defaultFormat = metaData.DisplayFormatString ?? defaultFormat;


            string text = string.Format(defaultFormat, metaData.Model ?? metaData.DefaultValue());

            // TODO: What about nullable types
            //metaData.NullDisplayText;



            //if (metaData.Model == null)
            //{
            //    //text = metaData.DefaultValue();
            //    //text = "0";
            //}
            //else
            //{
            //    text = string.Format(metaData.DisplayFormatString ?? defaultFormat,
            //        metaData.Model ?? metaData.DefaultValue());
            //}

            display.InnerHtml = text ?? metaData.NullDisplayText;
            html.Append(display.ToString());
            // Build input
            TagBuilder input = new TagBuilder("input");
            input.AddCssClass("numeric-input");
            input.MergeAttribute("autocomplete", "off");
            input.MergeAttribute("type", "text");
            if (includeID)
            {
                input.MergeAttribute("id", HtmlHelper.GenerateIdFromName(fieldName));
            }
            input.MergeAttribute("name", fieldName);
            input.MergeAttribute("value", string.Format("{0}", metaData.Model ?? metaData.DefaultValue()));
            // Remove the data-val-number attribute (the client script ensures its a number and jquery 
            // validation generates an error message if the first character is a decimal point which
            // disappears as soon as the next digit is entered - PITA when entering percentage values)
            // TODO: Still happens if a Range attribute is added!
            if (attributes != null && attributes.ContainsKey("data-val-number"))
            {
                attributes.Remove("data-val-number");
            }
            input.MergeAttributes(attributes);
            html.Append(input.ToString());
            // Build container
            TagBuilder container = new TagBuilder("div");
            container.AddCssClass("numeric-container");
            // Add data attributes for use by client script



            if (text == null)
            {
                text = string.Format(defaultFormat, 0);
            }


            Regex regex = new Regex(@"\.(\d+)");
            int decimals = regex.Match(text).Groups[1].Length;
            container.MergeAttribute("data-decimalPlaces", decimals.ToString());
            // Add essential styles
            container.MergeAttribute("style", "position:relative;margin:0;padding:0;");
            container.InnerHtml = html.ToString();
            // Return the html
            return container.ToString();
        }

        #endregion

    }

}
