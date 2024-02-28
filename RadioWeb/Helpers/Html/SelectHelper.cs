using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Web;
using System.Web.Mvc;
using ADPM.Common.Extensions;
using RadioWeb.Extensions;

namespace RadioWeb.Helpers.Web.Html
{

    /// <summary>
    /// Renders the html for a custom select control.
    /// </summary>
    public static class SelectHelper
    {

        #region .Enums 

        // TODO: Consider grouped and hierarchial list for value type 
        private enum SelectListType
        {
            // Displays an empty list
            EmptyList,
            // Displays enumerated values for an enum
            EnumList,
            // Displays values for the primitive property
            ValueList,
            // Displays values for a complex property
            FlatList,
            // Displays grouped values for a complex property
            GroupedList,
            // Displays hierarchial values for a complex property
            HierarchialList
        }

        #endregion

        #region .Declarations 

        // Error messages
        private const string _NullProperty = "The {0} parameter cannot be null if the model is a complex type.";
        private const string _InvalidDictionaryKey = "The dictionary key is not of type System.String";
        private const string _InvalidDictionaryValue = "The dictionary value is not of type System.Collections.IEnumerable";
        private const string _InvalidItemType = "The property type is not assignable from the items type";
        private const string _InvalidProperty = "{0} does not contain a property named {1}";

        #endregion

        #region .Methods 

        /// <summary>
        /// Returns the html for a custom control that allows selection of a single item 
        /// from a list of items.
        /// </summary>
        /// <typeparam name="TModel"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="helper">
        /// The HtmlHelper instance that this method extends.
        /// </param>
        /// <param name="expression">
        /// An expression that identifies the property to display.
        /// </param>
        /// <remarks>
        /// This overload renders a select list for an enum.
        /// </remarks>
        public static MvcHtmlString SelectFor<TModel, TValue>(this HtmlHelper<TModel> helper,
            Expression<Func<TModel, TValue>> expression)
        {
            // Get the model metadata
            ModelMetadata metaData = ModelMetadata.FromLambdaExpression(expression, helper.ViewData);
            // Get the fully qualified name of the property
            string fieldName = ExpressionHelper.GetExpressionText(expression);
            // Return the html
            return MvcHtmlString.Create(SelectForHelper(helper, metaData, fieldName, null, null, null, null, null));
        }

        /// <summary>
        /// Returns the html for a custom control that allows selection of a single item 
        /// from a list of items.
        /// </summary>
        /// <typeparam name="TModel">
        /// The type of the model.
        /// </typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="helper">
        /// The HtmlHelper instance that this method extends.
        /// </param>
        /// <param name="expression">
        /// An expression that identifies the property to display.
        /// </param>
        /// <param name="items">
        /// The collection of items to display.
        /// </param>
        /// <exception cref="System.ArgumentException">
        /// If the property is a complex type.
        /// </exception>
        /// <remarks>
        /// This overload renders a select list for a value type.
        /// The <paramref name="items"/> can be a collection of value type or complex type. If the 
        /// collection is a complex type, the items are displayed using their .ToString() method.
        /// </remarks>
        public static MvcHtmlString SelectFor<TModel, TValue>(this HtmlHelper<TModel> helper,
            Expression<Func<TModel, TValue>> expression, IEnumerable items)
        {
            // Get the model metadata
            ModelMetadata metaData = ModelMetadata.FromLambdaExpression(expression, helper.ViewData);
            // Get the fully qualified name of the property
            string fieldName = ExpressionHelper.GetExpressionText(expression);
            // Return the html
            return MvcHtmlString.Create(SelectForHelper(helper, metaData, fieldName, items, null, null, null, null));
        }

        /// <summary>
        /// Returns the html for a custom control that allows selection of a single item 
        /// from a list of items.
        /// </summary>
        /// <typeparam name="TModel">
        /// The type of the model.
        /// </typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="helper">
        /// The HtmlHelper instance that this method extends.
        /// </param>
        /// <param name="expression">
        /// An expression that identifies the property to display.
        /// </param>
        /// <param name="items">
        /// The collection of items to display.
        /// </param>
        /// <param name="idProperty">
        /// The name of property that uniquely identifies the item in the list.
        /// </param>
        /// <param name="displayProperty">
        /// The name of the property that identifies the text to display
        /// </param>
        /// <exception cref="System.ArgumentNullException">
        /// If the <paramref name="idProperty"/> or the <paramref name="displayProperty"/> 
        /// is null.
        /// </exception>
        /// <exception cref="System.ArgumentException">
        /// If the <paramref name="items"/> is <see cref="System.Collections.IEnumerable"/>  and the
        /// property type if not assignable from the items type, or if the <paramref name="items"/> is
        /// <see cref="System.Collections.IDictionary"/> and the key is not <see cref="System.String"/>,  
        /// or if the <paramref name="items"/> is <see cref="System.Collections.IDictionary"/> and the 
        /// value is not <see cref="System.Collections.IEnumerable"/>.
        /// </exception>
        /// <remarks>
        /// This overload renders a select list for a complex type. All properties of the complex type
        /// are posted back (except if the property is a collection).
        /// If any property of the complex  type is a collection of its own type, a hierarchial list is 
        /// displayed.
        /// If the <paramref name="items"/> is <see cref="System.Collections.IDictionary"/> and the key
        /// is <see cref="System.String"/> and the value is <see cref="System.Collections.IEnumerable"/>,
        /// a grouped list is displayed.
        /// </remarks>
        public static MvcHtmlString SelectFor<TModel, TValue>(this HtmlHelper<TModel> helper,
            Expression<Func<TModel, TValue>> expression, IEnumerable items, string idProperty,
            string displayProperty)
        {
            // Get the model metadata
            ModelMetadata metaData = ModelMetadata.FromLambdaExpression(expression, helper.ViewData);
            // Get the fully qualified name of the property
            string fieldName = ExpressionHelper.GetExpressionText(expression);
            // Return the html
            return MvcHtmlString.Create(SelectForHelper(helper, metaData, fieldName, items, idProperty, 
                displayProperty, null, null));
        }

        /// <summary>
        /// Returns the html for a custom control that allows selection of a single item 
        /// from a list of items.
        /// and <paramref name="urlParameter"/> parameters.
        /// </summary>
        /// <typeparam name="TModel"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="helper">
        /// The HtmlHelper instance that this method extends.
        /// </param>
        /// <param name="expression">
        /// An expression that identifies the property to display.
        /// </param>
        /// <param name="idProperty">
        /// The name of property that uniquely identifies the item in the list.
        /// </param>
        /// <param name="displayProperty">
        /// The name of the property that identifies the text to display.
        /// </param>
        /// <param name="controller">
        /// The name of the controller used to fetch items.
        /// </param>
        /// <param name="action">
        /// The name of the action method used to fetch items.
        /// </param>
        /// <param name="urlParameter">
        /// The name of the parameter in the controller action method used to filter the items.
        /// </param>
        /// <exception cref="System.ArgumentNullException">
        /// If the <paramref name="idProperty"/> or the <paramref name="displayProperty"/> 
        /// is null.
        /// </exception>
        /// <remarks>
        /// This overload renders a select list that is dynamically populated from the action 
        /// method defined by the <paramref name="controller"/>, <paramref name="action"/> and 
        /// <paramref name="urlParameter"/> parameters.
        /// </remarks>
        public static MvcHtmlString SelectFor<TModel, TValue>(this HtmlHelper<TModel> helper,
            Expression<Func<TModel, TValue>> expression, string idProperty, string displayProperty, 
            string controller, string action, string urlParameter = "startsWith")
        {
            // Get the model metadata
            ModelMetadata metaData = ModelMetadata.FromLambdaExpression(expression, helper.ViewData);
            // Get the fully qualified name of the property
            string fieldName = ExpressionHelper.GetExpressionText(expression);
            // Build the url
            string root = HttpRuntime.AppDomainAppVirtualPath;
            if (root == "/")
            {
                root = string.Empty;
            }
            string url = string.Format("{0}/{1}/{2}", root, controller, action);
            // TODO: Is there a way to easily validate the url (check the action method exists on he controller)?

            //List<TValue> x = new List<TValue>() { (TValue)metaData.Model };

            return MvcHtmlString.Create(SelectForHelper(helper, metaData, fieldName, null, idProperty, 
                displayProperty, url, urlParameter));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="metaData"></param>
        /// <param name="fieldName"></param>
        /// <param name="items"></param>
        /// <param name="idProperty"></param>
        /// <param name="displayProperty"></param>
        public static string SelectForMetadata(ModelMetadata metaData, string fieldName,
            IEnumerable items, string idProperty, string displayProperty)
        {
            return SelectForHelper(null, metaData, fieldName, items, idProperty,
                displayProperty, null, null, false);
        }

        #endregion

        #region .Helper methods 

        /// <summary>
        /// Returns the html for the select control.
        /// </summary>
        /// <param name="helper">
        /// The HtmlHelper instance that this method extends.
        /// </param>
        /// <param name="metaData">
        /// The metadata of the property to be displayed.
        /// </param>
        /// <param name="fieldName">
        /// The fully qualified name of the property to be displayed.
        /// </param>
        /// <param name="items">
        /// The collection of items to be displayed for selection.
        /// </param>
        /// <param name="idProperty">
        /// The name of property that uniquely identifies the item in the list.
        /// </param>
        /// <param name="displayProperty">
        /// The name of the property that identifies the text to display.
        /// </param>
        /// <param name="url">
        /// The url of the action method to be called if the items are populated via AJAX.
        /// </param>
        /// <param name="urlParameter">
        /// The name of the url parameter.
        /// </param>
        /// <param name="includeID">
        /// 
        /// </param>
        private static string SelectForHelper(HtmlHelper helper,
            ModelMetadata metaData, string fieldName, IEnumerable items, string idProperty,
            string displayProperty, string url, string urlParameter, bool includeID = true)
        {
            // Get the select list display type
            SelectListType listType = GetListType(metaData, items);
            // Validate the ID and Display properties
            if (metaData.IsComplexType && idProperty == null)
            {
                throw new ArgumentNullException(string.Format(_NullProperty, "idProperty"));
            }
            if (metaData.IsComplexType && displayProperty == null)
            {
                throw new ArgumentNullException(string.Format(_NullProperty, "idProperty"));
            }
            if (idProperty != null && !metaData.Properties.Any(m => m.PropertyName == idProperty))
            {
                throw new ArgumentException(string.Format(_InvalidProperty, metaData.ModelType, idProperty));
            }
            if (displayProperty != null && !metaData.Properties.Any(m => m.PropertyName == displayProperty))
            {
                throw new ArgumentException(string.Format(_InvalidProperty, metaData.ModelType, displayProperty));
            }
            // Build the control
            StringBuilder html = new StringBuilder();
            // Add select
            html.Append(SelectInput(helper, metaData, fieldName, displayProperty, includeID));
            // Add validation
            html.Append(Validation(helper, metaData, fieldName));
            // Add the items
            switch (listType)
            {
                // TODO: Add value list
                case SelectListType.EmptyList:
                    html.Append(EmptyList());
                    break;
                case SelectListType.EnumList:
                    html.Append(EnumList(metaData));
                    break;
                case SelectListType.ValueList:
                    html.Append(ValueList(items, metaData));
                    break;
                case SelectListType.FlatList:
                    html.Append(FlatList(items, metaData, idProperty, displayProperty));
                    break;
                case SelectListType.HierarchialList:
                    html.Append(HierarchialList(items, metaData, idProperty, displayProperty));
                    break;
                case SelectListType.GroupedList:
                    html.Append(GroupList(items as IDictionary, metaData, idProperty, displayProperty));
                    break;
            }
            // Build container
            TagBuilder container = new TagBuilder("div");
            container.AddCssClass("select");
            container.InnerHtml = html.ToString();
            // Add attributes for use by client script
            container.MergeAttribute("data-propertyName", fieldName.ToLower());
            if (metaData.IsRequired)
            {
                container.MergeAttribute("data-isrequired", "true");
            }
            if (url != null)
            {
                container.MergeAttribute("data-url", url);
                if (urlParameter != null)
                {
                    container.MergeAttribute("data-urlparameter", urlParameter);
                }
            }
            if (idProperty != null)
            {
                container.MergeAttribute("data-idproperty", idProperty);
            }
            if (displayProperty != null)
            {
                container.MergeAttribute("data-displayproperty", displayProperty);
            }
            // Return the html
            return container.ToString();
            //return MvcHtmlString.Create(container.ToString());
        }

        /// <summary>
        /// Returns the html for the select input.
        /// </summary>
        /// <param name="helper">
        /// The HtmlHelper instance that this method extends.
        /// </param>
        /// <param name="metaData">
        /// The property metadata.
        /// </param>
        /// <param name="fieldName">
        /// The fully qualified name of the property.
        /// </param>
        /// <param name="displayTextProperty">
        /// The name of the property used to identify the display text
        /// </param>
        /// <param name="includeID">
        /// 
        /// </param>
        private static string SelectInput(HtmlHelper helper, ModelMetadata metaData,
            string fieldName, string displayTextProperty, bool includeID)
        {
            StringBuilder html = new StringBuilder();
            // Add search box
            html.Append(SearchBox(fieldName, includeID));
            // Add display text
            html.Append(DisplayText(metaData, displayTextProperty));
            // Add drop button
            html.Append(ButtonHelper.DropButton());
            // Add hidden inputs
            html.Append(HiddenInputHelper.HiddenInputForMetadata(metaData, fieldName, false, false));
            // Build container
            TagBuilder container = new TagBuilder("div");
            container.InnerHtml = html.ToString();
            // Add essential style properties
            container.AddCssClass("select-input");
            container.MergeAttribute("style", "position:relative;");
            // Return the html
            return container.ToString();
        }

        /// <summary>
        /// Returns the html for the search box.
        /// </summary>
        /// <param name="fieldName">
        /// The property name.
        /// </param>
        /// <param name="includeID">
        /// A value indicating of ID's should be rendered for hidden inputs.
        /// </param>
        private static string SearchBox(string fieldName, bool includeID)
        {
            TagBuilder search = new TagBuilder("input");
            search.MergeAttribute("type", "text");
            search.MergeAttribute("value", "");
            search.MergeAttribute("autocomplete", "off");
            if (includeID)
            {
                // Give it an ID for use by the associated label
                search.MergeAttribute("id", HtmlHelper.GenerateIdFromName(fieldName));
            }
            // Add essential style properties
            search.MergeAttribute("style", "color:transparent;");
            // Return the html
            return search.ToString();
        }

        /// <summary>
        /// Returns the html for the select display text.
        /// </summary>
        /// <param name="metaData">
        /// The metadata of the property to display.
        /// </param>
        /// <param name="displayTextProperty">
        /// The name of the property used to display the property value.
        /// </param>
        /// <returns></returns>
        private static string DisplayText(ModelMetadata metaData, string displayTextProperty)
        {
            // Get the display text
            // Note if its a group or hierarchial list, the client script will reformat it
            string text = null;
            if (!metaData.IsComplexType ||metaData.ModelType.IsEnum)
            {
                text = string.Format("{0}", metaData.Model);
            }
            else
            {
                // Get the display text property
                ModelMetadata property = metaData.Properties
                    .First(m => m.PropertyName == displayTextProperty);
                text = string.Format("{0}", property.Model);
            }
            // Build span element
            TagBuilder display = new TagBuilder("div");
            display.InnerHtml = text;
            // Add essential style properties
            display.MergeAttribute("style", "position:absolute;overflow:initial;text-overflow:initial;");
            // Return the html
            return display.ToString();
        }

        /// <summary>
        /// Returns the html for validation messages.
        /// </summary>
        /// <param name="helper">
        /// The HtmlHelper instance that this method extends.
        /// </param>
        /// <param name="metaData">
        /// The metadata of the property to display.
        /// </param>
        /// <param name="fieldName">
        /// The fully qualified name of the property.
        /// </param>
        private static string Validation(HtmlHelper helper, ModelMetadata metaData, string fieldName)
        {
            // HACK: Client validation does not work (but server validation does)
            // The hidden input is not doing anything (remove if cant find solution to client validation)

            StringBuilder html = new StringBuilder();
            // Add hidden input for client validation


            //TagBuilder input = new TagBuilder("input");
            //input.MergeAttribute("name", fieldName);
            //input.MergeAttribute("type", "hidden");
            //input.MergeAttribute("value", string.Format("{0}", metaData.Model ?? string.Empty));
            //input.MergeAttributes(helper.GetUnobtrusiveValidationAttributes(fieldName));
            //html.Append(input.ToString());

            // Add validation message
            TagBuilder message = new TagBuilder("span");
            message.AddCssClass(HtmlHelper.ValidationMessageCssClassName);

            // UNDONE: Add the following to use it for server validation (decided to 
            // use the standard @Html.ValidationMessageFor() method)
            //message.MergeAttribute("data-valmsg-for", fieldName);
            //message.MergeAttribute("data-valmsg-replace", "true");
            //ModelState modelState = helper.ViewData.ModelState[fieldName];
            //if (modelState != null)
            //{
            //    ModelErrorCollection modelErrors = modelState.Errors;
            //    if (modelErrors != null && modelErrors.Count > 0)
            //    {
            //        ModelError modelError = modelErrors
            //            .FirstOrDefault(m => !String.IsNullOrEmpty(m.ErrorMessage)) ?? modelErrors[0];
            //        if (modelError != null)
            //        {
            //            message.InnerHtml = modelError.ErrorMessage;
            //        }
            //    }
            //}

            html.Append(message.ToString());
            // Build container
            TagBuilder container = new TagBuilder("div");
            container.AddCssClass("select-validation");
            container.InnerHtml = html.ToString();
            // Return the html
            return container.ToString();
        }

        /// <summary>
        /// Returns the html for an empty list that will be populated using client script.
        /// </summary>
        private static string EmptyList()
        {
            // Return the html
            return SelectList(null);
        }

        /// <summary>
        /// Returns the html for the enum list.
        /// </summary>
        /// <param name="metaData">
        /// The metadata of the property.
        /// </param>
        private static string EnumList(ModelMetadata metaData)
        {
            StringBuilder html = new StringBuilder();
            foreach (Enum item in Enum.GetValues(metaData.ModelType))
            {
                // Build text
                TagBuilder text = new TagBuilder("div");
                // Use the ToDescription() extension method as the display text
                text.InnerHtml = item.ToDescription();
                if (item.Equals(metaData.Model))
                {
                    text.AddCssClass("selected");
                }
                // Build list item
                TagBuilder listItem = new TagBuilder("li");
                // Add data attribute for use by client script
                string attributeName = string.Format("data-{0}", metaData.PropertyName);
                string attributeValue = string.Format("{0}", item);
                listItem.MergeAttribute(attributeName, attributeValue);
                listItem.InnerHtml = text.ToString();
                // Append the enum
                html.Append(listItem.ToString());
            }
            // Return the html
            return SelectList(html.ToString());
        }

        /// <summary>
        /// Returns the html for the value list.
        /// </summary>
        /// <param name="items">
        /// The collection of items to display in the list.
        /// </param>
        /// <param name="selectedItem">
        /// The metadata of the property.
        /// </param>
        private static string ValueList(IEnumerable items, ModelMetadata selectedItem)
        {
            // Build the html for each item
            StringBuilder html = new StringBuilder();
            foreach (var item in items)
            {
                // Build text
                TagBuilder text = new TagBuilder("div");
                // Use the ToDescription() extension method as the display text
                text.InnerHtml = string.Format("{0}", item);
                if (item.Equals(selectedItem.Model))
                {
                    text.AddCssClass("selected");
                }
                // Build list item
                TagBuilder listItem = new TagBuilder("li");
                // Add data attribute for use by client script
                string attributeName = string.Format("data-{0}", selectedItem.PropertyName);
                string attributeValue = string.Format("{0}", item);
                listItem.MergeAttribute(attributeName, attributeValue);
                listItem.InnerHtml = text.ToString();
                // Append the item
                html.Append(listItem.ToString());
            }
            // Return the html
            return SelectList(html.ToString());

        }

        /// <summary>
        /// Returns the html for a flat list.
        /// </summary>
        /// <param name="items">
        /// The collection of items to display in the list.
        /// </param>
        /// <param name="selectedItem">
        /// The metadata of the selected item.
        /// </param>
        /// <param name="idProperty">
        /// The name of property that uniquely identifies the item in the list.
        /// </param>
        /// <param name="displayProperty">
        /// The name of the property that identifies the text to display
        /// </param>
        private static string FlatList(IEnumerable items, ModelMetadata selectedItem,
            string idProperty, string displayProperty)
        {
            // Flag selected item has been found
            bool foundSelected = false;
            // Build the html for each item
            StringBuilder html = new StringBuilder();
            foreach (var item in items)
            {
                // Get the metadata of the item
                ModelMetadata metaData = ModelMetadataProviders.Current
                    .GetMetadataForType(() => item, selectedItem.ModelType);
                //  Check if its the selected item
                bool isSelected = false;
                if (selectedItem != null && !foundSelected)
                {
                    isSelected = IsSelected(metaData, selectedItem, idProperty);
                    foundSelected = isSelected;
                }
                // Append the list item
                html.Append(ListItem(metaData, displayProperty, isSelected));
            }
            // Return the html
            return SelectList(html.ToString());
        }

        /// <summary>
        /// Returns the html for a grouped list.
        /// </summary>
        /// <param name="items">
        /// The dictionary of items to display in the list.
        /// </param>
        /// <param name="selectedItem">
        /// The metadata of the selected item.
        /// </param>
        /// <param name="idProperty">
        /// The name of property that uniquely identifies the item in the list.
        /// </param>
        /// <param name="displayProperty">
        /// The name of the property that identifies the text to display
        /// </param>
        private static string GroupList(IDictionary items, ModelMetadata selectedItem,
            string idProperty, string displayProperty)
        {
            // Flag selected item has been found
            bool foundSelected = false;
            // Build the html for each item
            StringBuilder html = new StringBuilder();
            foreach (DictionaryEntry entry in items)
            {
                StringBuilder groupHtml = new StringBuilder();
                // Build group text
                TagBuilder groupText = new TagBuilder("div");
                groupText.AddCssClass("group");
                groupText.InnerHtml = string.Format("{0}", entry.Key);
                groupHtml.Append(groupText);
                // Build group items
                StringBuilder innerHtml = new StringBuilder();
                foreach (var item in entry.Value as IEnumerable)
                {
                    // Get the metadata of the item
                    ModelMetadata metaData = ModelMetadataProviders.Current
                        .GetMetadataForType(() => item, selectedItem.ModelType);
                    //  Check if its the selected item
                    bool isSelected = false;
                    if (selectedItem != null && !foundSelected)
                    {
                        isSelected = IsSelected(metaData, selectedItem, idProperty);
                        foundSelected = isSelected;
                    }
                    // Append the list item
                    innerHtml.Append(ListItem(metaData, displayProperty, isSelected));
                }
                // Build group list
                TagBuilder list = new TagBuilder("ul");
                list.InnerHtml = innerHtml.ToString();
                groupHtml.Append(list);
                //Build group item
                TagBuilder groupItem = new TagBuilder("li");
                groupItem.InnerHtml = groupHtml.ToString();
                // Append the group item
                html.Append(groupItem.ToString());
            }
            // Return the html
            return SelectList(html.ToString());
        }

        /// <summary>
        /// Returns the html for a hierarchial list.
        /// </summary>
        /// <param name="items">
        /// The collection of items to display in the list.
        /// </param>
        /// <param name="selectedItem">
        /// The metadata of the selected item.
        /// </param>
        /// <param name="idProperty">
        /// The name of property that uniquely identifies the item in the list.
        /// </param>
        /// <param name="displayProperty">
        /// The name of the property that identifies the text to display
        /// </param>
        private static string HierarchialList(IEnumerable items, ModelMetadata selectedItem,
            string idProperty, string displayProperty)
        {
            // Flag selected item has been found
            bool selectionFound = false;
            // Get the child property
            string hierarchialProperty = selectedItem.Properties
                .First(m => m.ModelType.IsGenericType && m.ModelType
                    .GetGenericArguments()[0] == selectedItem.ModelType).PropertyName;
            // Build the html for each item
            StringBuilder html = new StringBuilder();
            foreach (var item in items)
            {
                // Get the metadata of the item
                ModelMetadata metaData = ModelMetadataProviders.Current
                    .GetMetadataForType(() => item, selectedItem.ModelType);
                // Append the list item
                html.Append(HierarchialItem(metaData, selectedItem, idProperty,
                    displayProperty, hierarchialProperty, ref selectionFound));
            }
            // Return the html
            return SelectList(html.ToString());
        }

        /// <summary>
        /// Return the html for the select list
        /// </summary>
        /// <param name="innerHtml">
        /// A string containing the html for the list items.
        /// </param>
        private static string SelectList(string innerHtml)
        {
            // Build the list
            TagBuilder list = new TagBuilder("ul");
            // In case its an empty list
            if (innerHtml != null)
            {
                list.InnerHtml = innerHtml;
            }
            // Add essential style properties
            list.MergeAttribute("style", "position:absolute;display:none;z-index:1000;");
            // Build container
            TagBuilder container = new TagBuilder("div");
            container.InnerHtml = list.ToString();
            // Add essential style properties
            container.AddCssClass("select-list");
            container.MergeAttribute("style", "position:relative;");
            // Return the html
            return container.ToString();
        }

        /// <summary>
        /// Return the html for a list item in a select list.
        /// </summary>
        /// <param name="item">
        /// The metadata of the item to display.
        /// </param>
        /// <param name="displayProperty">
        /// The name of the property that identifies the text to display.
        /// </param>
        /// <param name="isSelected">
        /// A value indicating if the item is the selected item.
        /// </param>
        private static string ListItem(ModelMetadata item, string displayProperty,
            bool isSelected)
        {
            // Build display text
            TagBuilder text = new TagBuilder("div");
            if (isSelected)
            {
                text.AddCssClass("selected");
            }
            text.InnerHtml = GetDisplayText(item, displayProperty);
            // Build list item
            TagBuilder listItem = new TagBuilder("li");
            listItem.InnerHtml = text.ToString();
            // Add attributes for each property
            foreach (ModelMetadata property in item.Properties)
            {
                string attributeName = string.Format("data-{0}", property.PropertyName);
                string attributeValue = string.Format("{0}", property.Model); ;
                listItem.MergeAttribute(attributeName, attributeValue);
            }
            // Add essential style properties
            listItem.MergeAttribute("style", "margin:0;padding:0;");
            // Return the html
            return listItem.ToString();
        }

        /// <summary>
        /// Return the html for a list item in a rial select list.
        /// </summary>
        /// <param name="item">
        /// The metadata of the item to display.
        /// </param>
        /// <param name="selectedItem">
        /// The metadata of the selected item.
        /// </param>
        /// <param name="idProperty">
        /// The name of property that uniquely identifies the item in the list.
        /// </param>
        /// <param name="displayProperty">
        /// The name of the property that identifies the text to display.
        /// </param>
        /// <param name="hierarchialProperty">
        /// The name of the property that contains the child items in the hierarchy.
        /// </param>
        /// <param name="selectionFound">
        /// A value indicating if the selected item has been found.
        /// </param>
        private static string HierarchialItem(ModelMetadata item,
            ModelMetadata selectedItem, string idProperty, string displayProperty,
            string hierarchialProperty, ref bool selectionFound)
        {
            StringBuilder html = new StringBuilder();
            //Build the display text
            TagBuilder text = new TagBuilder("div");
            if (!selectionFound)
            {
                if (IsSelected(item, selectedItem, idProperty))
                {
                    selectionFound = true;
                    text.AddCssClass("selected");
                }
            }
            text.InnerHtml = GetDisplayText(item, displayProperty);
            html.Append(text.ToString());
            // Build list item
            TagBuilder listItem = new TagBuilder("li");
            foreach (ModelMetadata property in item.Properties)
            {
                if (property.PropertyName == hierarchialProperty)
                {
                    StringBuilder innerHtml = new StringBuilder();
                    // Flag to indicate if there are child items to display
                    bool hasChildren = false;
                    IEnumerable children = property.Model as IEnumerable;
                    foreach (var child in children)
                    {
                        // Signal there are child items to display
                        hasChildren = true;
                        // Get the metadata
                        ModelMetadata childItem = ModelMetadataProviders.Current
                            .GetMetadataForType(() => child, item.ModelType);
                        // Recursive call to build the child items
                        innerHtml.Append(HierarchialItem(childItem, selectedItem, idProperty,
                            displayProperty, hierarchialProperty, ref selectionFound));
                    }
                    if (hasChildren)
                    {
                        TagBuilder list = new TagBuilder("ul");
                        list.InnerHtml = innerHtml.ToString();
                        html.Append(list.ToString());
                    }
                    else
                    {
                        html.Append(innerHtml.ToString());
                    }
                }
                else
                {
                    // Add attributes
                    string attributeName = string.Format("data-{0}", property.PropertyName);
                    string attributeValue = string.Format("{0}", property.Model);
                    listItem.MergeAttribute(attributeName, attributeValue);
                }
            }
            listItem.InnerHtml = html.ToString();
            // Add essential style properties
            listItem.MergeAttribute("style", "margin:0;padding:0;");
            // Return the html
            return listItem.ToString();
        }

        /// <summary>
        /// Returns a value indicating if an item is the selected item.
        /// </summary>
        /// <param name="item">
        /// The metadata of the item.
        /// </param>
        /// <param name="selectedItem">
        /// The metadata of the selected item.
        /// </param>
        /// <param name="idProperty">
        /// The name of property that uniquely identifies the item in the list.
        /// </param>
        private static bool IsSelected(ModelMetadata item, ModelMetadata selectedItem, string idProperty)
        {
            if (item.Model == null || selectedItem.Model == null)
            {
                return false;
            }
            if (idProperty == null)
            {
                return Object.ReferenceEquals(item.Model, selectedItem.Model);
            }
            // Note we need to use the .Equals() method because ModelMetadata.Model is typeof(object)
            if (item.IsComplexType)
            {
                return item.Properties.First(m => m.PropertyName == idProperty)
                    .Model.Equals(selectedItem.Properties.First(m => m.PropertyName == idProperty).Model);
            }
            return item.Model.Equals(selectedItem.Model);
        }

        /// <summary>
        /// Returns the display text for an item.
        /// </summary>
        /// <param name="item">
        /// The metadata of the item to display.
        /// </param>
        /// <param name="displayProperty">
        /// The name of the property that identifies the text to display.
        /// </param>
        public static string GetDisplayText(ModelMetadata item, string displayProperty)
        {
            if (displayProperty == null)
            {
                // Use the .ToString() method of the model
                return string.Format("{0}", item.Model);
            }
            else
            {
                return string.Format("{0}", item.Properties
                    .FirstOrDefault(m => m.PropertyName == displayProperty).Model);
            }
        }

        /// <summary>
        /// // Gets the type of list  to display in the select list
        /// </summary>
        /// <param name="metaData">
        /// The metadata of the property to display.
        /// </param>
        /// <param name="items">
        /// The items to display in the select list.
        /// </param>
        /// <remarks>
        /// The method validates the item type(s) against the property type.
        /// </remarks>
        private static SelectListType GetListType(ModelMetadata metaData, IEnumerable items)
        {
            if (metaData.ModelType.IsEnum)
            {
                return SelectListType.EnumList;
            }
            else if (items == null)
            {
                return SelectListType.EmptyList;
            }
            else if (!metaData.IsComplexType)
            {
                return SelectListType.ValueList;
            }
            if (items is IDictionary)
            {
                // Vaidate the key and value types
                IDictionary dictionary = items as IDictionary;
                Type[] arguments = dictionary.GetType().GetGenericArguments();
                // The key must be a string (the group name) and the value must 
                // be a collection (the groups items)

                // TODO: Consider allowing a complex type as the key and (1) checking that it
                // contains a string property (but what if it contains more than one string
                // property) or (2) use its ToString() method but only if its been overridden

                if (arguments[0] != typeof(string))
                {
                    throw new ArgumentException(_InvalidDictionaryKey);
                }
                if (!typeof(IEnumerable).IsAssignableFrom(arguments[1]))
                {
                    throw new ArgumentException(_InvalidDictionaryValue);
                }
                // Get the items type
                Type type = null;
                if (arguments[1].IsGenericType)
                {
                    type = arguments[1].GetGenericArguments()[0];
                }
                else
                {
                    type = arguments[1].GetProperty("Item").PropertyType;
                }
                // The model type must be the same as (or be assignable from an instance of) the items type 
                if (!metaData.ModelType.IsAssignableFrom(type))
                {
                    throw new ArgumentException(_InvalidItemType);
                }
                return SelectListType.GroupedList;
            }
            else
            {
                // Get the items type
                Type type = null;
                // Assign the items type
                if (items.GetType().IsGenericType)
                {
                    type = items.GetType().GetGenericArguments()[0];
                }
                else
                {
                    foreach(var x in items)
                    {
                        // Only need the first one
                        type = x.GetType();
                        break;
                    }
                }
                // The model type must be the same as (or be assignable from an instance of) the items type 
                if (!metaData.ModelType.IsAssignableFrom(type))
                {
                    throw new ArgumentException(_InvalidItemType);
                }
                // Determine if its a hierarchial list
                metaData = metaData.Properties.FirstOrDefault(m => m.ModelType
                    .IsGenericType && m.ModelType.GetGenericArguments()[0] == metaData.ModelType);
                if (metaData != null)
                {
                    return SelectListType.HierarchialList;
                }
                else
                {
                    return SelectListType.FlatList;
                }
            }
        }

        #endregion

    }

}
