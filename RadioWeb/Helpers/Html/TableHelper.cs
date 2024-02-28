using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using System.Web.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using ADPM.Common.Extensions;
using ADPM.Common.Web.DataAnnotations;
using RadioWeb.Extensions;

namespace RadioWeb.Helpers.Web.Html
{

    /// <summary>
    /// 
    /// </summary>
    public static class TableHelper
    {

        #region .Declarations 

        // Error messages
        private const string _InvalidCollection = "The property {0} cannot be cast to IEnumerable";
        private const string _InvalidSelect = "View data does not contain a collection property named {0}List";

        #endregion

        #region .Methods 

        /// <summary>
        /// Returns the html to render a readonly display table for a collection.
        /// </summary>
        /// <typeparam name="TModel"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="helper">
        /// The HtmlHelper instance that this method extends.
        /// </param>
        /// <param name="expression">
        /// An expression that identifies the property to display.
        /// </param>
        /// <exception cref="InvalidCastException">
        /// If the property does not implement <see cref="T:System.Collections.IEnumerable"/>.
        /// </exception>
        /// <remarks>
        /// <para>
        /// The html outputs a column for each property of the model, including properties of complex 
        /// objects except properties that implement <see cref="System.Collections.IEnumerable"/>, 
        /// properties with <see cref="T:System.Web.Mvc.HiddenInputAttribute"/> and 
        /// properties with <see cref="P:Sandtrap.Web.DataAnnotations.TableColumnAttribute.Exclude"/> 
        /// </para>
        /// <para>
        /// The order of the columns can be controlled by setting the
        /// <see cref="P:System.ComponentModel.DataAnnotations.DisplayAttribute.Order"/> property.
        /// </para>
        /// <para>
        /// If <see cref="P:Sandtrap.Web.DataAnnotations.TableColumnAttribute.IncludeTotal"/> is true,
        /// a total value is rendered in the table footer.
        /// </para>
        /// </remarks>
        public static MvcHtmlString TableDisplayFor<TModel, TValue>(this HtmlHelper<TModel> helper,
            Expression<Func<TModel, TValue>> expression)
        {
            // Get the model metadata
            ModelMetadata metaData = ModelMetadata.FromLambdaExpression(expression, helper.ViewData);
            // Get the fully qualified name of the property
            string fieldName = ExpressionHelper.GetExpressionText(expression);
            // Get the collection to render in the table
            IEnumerable collection = metaData.Model as IEnumerable;
            if (collection == null)
            {
                throw new InvalidCastException(string.Format(_InvalidCollection, fieldName));
            }
            // Get the type in the collection
            Type type = GetCollectionType(collection);
            if (type == null)
            {
                // TODO: Wahts the right exception?
                throw new InvalidCastException("The type in the collection could not be resolved");
            }
            // Get the metadata of the type (in case there are no items in the table)
            ModelMetadata typeMetadata = ModelMetadataProviders.Current.GetMetadataForType(null, type);
            // Initalise column properties
            List<TableColumn> columns = new List<TableColumn>();
            // Get table properties
            bool includeRowNumbers = typeMetadata.TableIncludeRowNumbers();
            bool includeViewLink = typeMetadata.TableIncludeViewLink();
            bool includeEditLink = typeMetadata.TableIncludeEditLink();
            // Add table components
            StringBuilder html = new StringBuilder();
            // Table header
            html.Append(TableHeader(helper, typeMetadata, columns, includeRowNumbers,
                includeViewLink, includeEditLink, false, false));
            // Table body
            html.Append(ReadonlyTableBody(collection, type, columns, includeRowNumbers,
                includeViewLink, includeEditLink));
            // Table footer (only if there is something to show)
            if (columns.Any(c => c.IncludeTotals))
            {
                html.Append(TableFooter(columns, includeRowNumbers, includeViewLink,
                    includeEditLink, false, false, false));
            }
            // Create the table
            TagBuilder table = new TagBuilder("table");
            table.MergeAttribute("id", HtmlHelper.GenerateIdFromName(fieldName));
            // table.AddCssClass("readonly-table");
            table.AddCssClass("table table-bordered table-stripped table-hover");
            table.InnerHtml = html.ToString();
            // Return the html
            return MvcHtmlString.Create(table.ToString());
        }
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TModel"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="helper"></param>
        /// <param name="expression"></param>
        /// <returns></returns>
        public static MvcHtmlString TableEditorFor<TModel, TValue>(this HtmlHelper<TModel> helper,
            Expression<Func<TModel, TValue>> expression)
        {
            // Get the model metadata
            ModelMetadata metaData = ModelMetadata.FromLambdaExpression(expression, helper.ViewData);
            // Get the fully qualified name of the property
            string fieldName = ExpressionHelper.GetExpressionText(expression);
            // Get the collection to render in the table
            IEnumerable collection = metaData.Model as IEnumerable;
            if (collection == null)
            {
                throw new InvalidCastException(string.Format(_InvalidCollection, fieldName));
            }
            // Get the type in the collection
            Type type = GetCollectionType(collection);
            // Get the metadata of the type (in case there are no items in the table)
            ModelMetadata typeMetadata = ModelMetadataProviders.Current.GetMetadataForType(null, type);
            // Initalise column properties
            List<TableColumn> columns = new List<TableColumn>();
            // Determine if the rows can added and deleted
            bool allowAdditions = typeMetadata.TableCanAddRows();
            bool allowDeletions = typeMetadata.TableCanDeleteRows();
            bool hasButtons = allowAdditions || allowDeletions;
            // Add table components
            StringBuilder html = new StringBuilder();
            // Table header
            html.Append(TableHeader(helper, typeMetadata, columns, false, false,
                false, hasButtons, true));
            // Table body
            html.Append(EditableTableBody(helper, collection, type, fieldName,
                columns, hasButtons, allowDeletions));
            // Add new row
            object instance = Activator.CreateInstance(typeMetadata.ModelType);
            ModelMetadata itemMetadata = ModelMetadataProviders.Current
                .GetMetadataForType(() => instance, type);
            html.Append(EditableTableNewRow(helper, itemMetadata, fieldName,
                columns, hasButtons, allowAdditions));


            //html.Append(EditableTableNewRow(helper, typeMetadata, fieldName, 
            //    columns, hasButtons, allowAdditions));
            // Table footer
            html.Append(TableFooter(columns, false, false, false, hasButtons,
                allowDeletions, true));
            // Create the table
            TagBuilder table = new TagBuilder("table");
            table.AddCssClass("edit-table");
            table.MergeAttribute("id", HtmlHelper.GenerateIdFromName(fieldName));
            if (typeMetadata.TableIsActiveProperty() != null)
            {
                table.MergeAttribute("data-isactiveproperty", typeMetadata.TableIsActiveProperty());
            }
            if (typeMetadata.TableIsDirtyProperty() != null)
            {
                table.MergeAttribute("data-isdirtyproperty", typeMetadata.TableIsDirtyProperty());

            }
            table.InnerHtml = html.ToString();
            // Return the html
            return MvcHtmlString.Create(table.ToString());
        }

        #endregion

        #region .Helper methods 

        /// <summary>
        /// Returns the html for the table header.
        /// </summary>
        /// <param name="helper">
        /// The HtmlHelper instance that this method extends.
        /// </param>
        /// <param name="metaData">
        /// The metadata of the <see cref="System.Type"/> to display in the table.
        /// </param>
        /// <param name="columns">
        /// A collection of <see cref="Sandtrap.Web.Html.TableColumn"/> used 
        /// to initialise table properties and calculate footer totals.
        /// </param>
        /// <param name="includeRowNumbers">
        /// A value indicating if the table should include a column for row numbers.
        /// </param>
        /// <param name="includeViewLink">
        /// A value indicating if the table should include a column for a view details link.
        /// </param>
        /// <param name="includeEditLink">
        /// A value indicating if the table should include a column for a edit link.
        /// </param>
        /// <param name="includeButtons">
        /// A value indicating if the table should include a column for edit buttons.
        /// </param>
        /// <param name="isEditMode">
        /// A value indicating if the table is an editable table.
        /// </param>
        private static string TableHeader(HtmlHelper helper, ModelMetadata metaData,
            List<TableColumn> columns, bool includeRowNumbers, bool includeViewLink,
            bool includeEditLink, bool includeButtons, bool isEditMode)
        {
            // Build row
            TagBuilder row = new TagBuilder("tr");
            row.InnerHtml = TableHeaderRow(helper, metaData, columns, includeRowNumbers,
                includeViewLink, includeEditLink, includeButtons, isEditMode);
            // Build header
            TagBuilder head = new TagBuilder("thead");
            head.InnerHtml = row.ToString();
            // Return html
            return head.ToString();
        }

        /// <summary>
        /// Returns the html for each cell of the table header.
        /// </summary>
        /// <param name="helper">
        /// The HtmlHelper instance that this method extends.
        /// </param>
        /// <param name="metaData">
        /// The metadata of the <see cref="System.Type"/> to display in the table.
        /// </param>
        /// <param name="columns">
        /// A collection of <see cref="Sandtrap.Web.Html.TableColumn"/> used 
        /// to initialise table properties and calculate footer totals.
        /// </param>
        /// <param name="includeRowNumbers">
        /// A value indicating if the table should include a column for row numbers.
        /// </param>
        /// <param name="includeViewLink">
        /// A value indicating if the table should include a column for a view details link.
        /// </param>
        /// <param name="includeEditLink">
        /// A value indicating if the table should include a column for a edit link.
        /// </param>
        /// <param name="includeButtons">
        /// A value indicating if the table should include a column for edit buttons.
        /// </param>
        /// <param name="isEditMode">
        /// A value indicating if the table is an editable table.
        /// </param>
        /// <remarks>
        /// The method is called recursively for complex properties.
        /// </remarks>
        private static string TableHeaderRow(HtmlHelper helper, ModelMetadata metaData,
            List<TableColumn> columns, bool includeRowNumbers, bool includeViewLink,
            bool includeEditLink, bool includeButtons, bool isEditMode)
        {
            // Build the html for each cell
            StringBuilder html = new StringBuilder();
            if (includeRowNumbers)
            {
                html.Append(TableHeaderCell("No."));
            }
            string linkProperty = null;
            if (metaData.TableIsLink())
            {
                linkProperty = (string)metaData
                    .AdditionalValues[TableLinkAttribute.DisplayPropertyKey];
            }
            string iconProperty = null;

            int index = 0;
            foreach (ModelMetadata property in metaData.Properties)
            {
                TableColumn column = new TableColumn();
                // Do we need to include the property
                if (property.IsHidden() || property.ColumnIsExcluded())
                {
                    column.PropertyName = property.PropertyName;
                    column.IsExcluded = true;
                    columns.Add(column);
                    continue;
                }
                if (property.PropertyName == linkProperty)
                {
                    column.IsLink = true;
                }
                if (property.ColumnIsIcon())
                {
                    column.IsIconColumn = true;
                }
                if (property.DataTypeName == "EmailAddress")
                {
                    column.IsEmailAddress = true;
                }
                // Is the value omitted if its the same as the previous row
                if (property.ColumnNoRepeat())
                {
                    // This only makes sense if its the first column or the
                    // previous column is NoRepeat
                    if (index == 0 || columns[index - 1].NoRepeat)
                    {
                        column.NoRepeat = true;
                    }
                }
                // Are column totals required
                if (property.ColumnIncludeTotals())
                {
                    column.IncludeTotals = true;
                    column.FormatString = property.DisplayFormatString ?? "{0:0.00}";
                }
                if (property.IsComplexType)
                {
                    if (typeof(IEnumerable).IsAssignableFrom(property.ModelType))
                    {
                        // Its a collection within the collection so ignore.
                        continue;
                    }
                    // Is the property displayed as a hyperlink (display table)
                    if (property.ColumnIsLink())
                    {
                        column.IsLink = true;
                    }
                    // Is the property readonly (edit table)
                    if (property.ColumnIsReadOnly())
                    {
                        column.IsReadonly = true;
                    }
                    // Does the property have a display property
                    if (property.ColumnHasDisplayProperty())
                    {
                        column.HasDisplayProperty = true;
                        column.DisplayProperty = (string)property
                            .AdditionalValues[TableColumnAttribute.DisplayPropertyKey];
                        ModelMetadata displayProperty = property.Properties
                            .First(m => m.PropertyName == column.DisplayProperty);
                        if (displayProperty.ColumnIncludeTotals())
                        {
                            column.IncludeTotals = true;
                            column.FormatString = displayProperty.DisplayFormatString ?? column.FormatString;
                        }
                    }
                    if (column.IsLink || column.IsReadonly || column.HasDisplayProperty)
                    {
                        // Only one column is required for the type
                        if (isEditMode && property.ColumnIsSelect())
                        {
                            // Check if there is a matching collection in view data
                            string key = string.Format("{0}List", property.PropertyName);
                            if (helper.ViewData.ContainsKey(key))
                            {
                                column.IsSelect = true;
                                column.SelectList = helper.ViewData[key] as IEnumerable;
                            }
                            else
                            {
                                throw new Exception(string.Format(_InvalidSelect, property.PropertyName));
                            }
                        }
                        columns.Add(column);
                        html.Append(TableHeaderCell(property.GetDisplayName()));
                    }
                    else
                    {
                        // Add columns for each property of the type (recursive call)
                        html.Append(TableHeaderRow(helper, property, columns, false,
                            false, false, false, false));
                    }
                }
                else
                {
                    column.PropertyName = property.PropertyName;
                    columns.Add(column);
                    html.Append(TableHeaderCell(property.GetDisplayName()));
                }
                index++;
            }
            if (includeViewLink)
            {
                html.Append(TableHeaderCell(null));
            }
            if (includeEditLink)
            {
                html.Append(TableHeaderCell(null));
            }
            if (includeButtons)
            {
                html.Append(TableHeaderCell(null));
            }
            else if (isEditMode)
            {
                // An extra column is required for the hidden inputs 
                TagBuilder cell = new TagBuilder("th");
                cell.MergeAttribute("style", "width:0;");
                html.Append(cell.ToString());
            }
            return html.ToString();
        }

        /// <summary>
        /// Returns the html for the table footer.
        /// </summary>
        /// <param name="columns">
        /// A collection of <see cref="Sandtrap.Web.Html.TableColumn"/> used 
        /// to initialise table properties and calculate footer totals.
        /// </param>
        /// <param name="includeRowNumbers">
        /// A value indicating if the table should include a column for row numbers.
        /// </param>
        /// <param name="includeViewLink">
        /// A value indicating if the table should include a column for a view details link.
        /// </param>
        /// <param name="includeEditLink">
        /// A value indicating if the table should include a column for a edit link.
        /// </param>
        /// <param name="includeButtons">
        /// A value indicating if the table should include a column for edit buttons.
        /// </param>
        /// <param name="hasButton">
        /// A value indicating if edit buttons should be rendered.
        /// </param>
        /// <param name="isEditMode">
        /// A value indicating if the table is an editable table.
        /// </param>
        private static string TableFooter(List<TableColumn> columns, bool includeRowNumbers,
            bool includeViewLink, bool includeEditLink, bool includeButtons, bool hasButton,
            bool isEditMode)
        {
            TagBuilder row = new TagBuilder("tr");
            row.InnerHtml = TableFooterRow(columns, includeRowNumbers, includeViewLink,
                includeEditLink, includeButtons, hasButton, isEditMode);
            TagBuilder footer = new TagBuilder("tfoot");
            footer.InnerHtml = row.ToString();
            return footer.ToString();
        }

        /// <summary>
        /// Returns the html for each cell of the table footer.
        /// </summary>
        /// <param name="columns">
        /// A collection of <see cref="Sandtrap.Web.Html.TableColumn"/> used 
        /// to initialise table properties and calculate footer totals.
        /// </param>
        /// <param name="includeRowNumbers">
        /// A value indicating if the table should include a column for row numbers.
        /// </param>
        /// <param name="includeViewLink">
        /// A value indicating if the table should include a column for a view details link.
        /// </param>
        /// <param name="includeEditLink">
        /// A value indicating if the table should include a column for a edit link.
        /// </param>
        /// <param name="includeButtons">
        /// A value indicating if the table should include a column for edit buttons.
        /// </param>
        /// <param name="hasButton">
        /// A value indicating if edit buttons should be rendered.
        /// </param>
        /// <param name="isEditMode">
        /// A value indicating if the table is an editable table.
        /// </param>
        private static string TableFooterRow(List<TableColumn> columns, bool includeRowNumbers,
            bool includeViewLink, bool includeEditLink, bool includeButtons, bool hasButton,
            bool isEditMode)
        {
            // Build the html for each cell
            StringBuilder html = new StringBuilder();
            if (includeRowNumbers)
            {
                html.Append(TableCellText(null));
            }
            foreach (TableColumn column in columns)
            {
                if (column.IsExcluded)
                {
                    continue;
                }
                string text = null;
                if (column.IncludeTotals)
                {
                    text = string.Format(column.FormatString, column.ColumnTotal);
                }
                html.Append(TableCellText(text));
            }
            if (includeViewLink)
            {
                html.Append(TableCellText(null));
            }
            if (includeEditLink)
            {
                html.Append(TableCellText(null));
            }
            if (includeButtons)
            {
                html.Append(TableCellAddButton(hasButton));
            }
            else if (isEditMode)
            {
                TagBuilder cell = new TagBuilder("td");
                cell.MergeAttribute("style", "width:0;");
                html.Append(cell.ToString());
            }
            //  Return the html
            return html.ToString();
        }

        /// <summary>
        /// Returns the html for a readonly only table body.
        /// </summary>
        /// <param name="collection">
        /// The collection of items to display in the table.
        /// </param>
        /// <param name="type">
        /// The <see cref="System.Type"/> of the iitems in the collection.
        /// </param>
        /// <param name="columns">
        /// A collection of <see cref="Sandtrap.Web.Html.TableColumn"/> used 
        /// to initialise table properties and calculate footer totals.
        /// </param>
        /// <param name="includeRowNumbers">
        /// A value indicating if the table should include a column for row numbers.
        /// </param>
        /// <param name="includeViewLink">
        /// A value indicating if the table should include a column for a view details link.
        /// </param>
        /// <param name="includeEditLink">
        /// A value indicating if the table should include a column for a edit link.
        /// </param>
        private static string ReadonlyTableBody(IEnumerable collection, Type type,
            List<TableColumn> columns, bool includeRowNumbers, bool includeViewLink,
            bool includeEditLink)
        {
            StringBuilder html = new StringBuilder();
            int? rowNumber = null;
            if (includeRowNumbers)
            {
                rowNumber = 0;
            }
            foreach (var item in collection)
            {
                rowNumber++;
                int columnNumber = 0;
                ModelMetadata itemMetadata = ModelMetadataProviders.Current
                    .GetMetadataForType(() => item, type);
                TagBuilder row = new TagBuilder("tr");
                row.InnerHtml = ReadonlyTableBodyRow(itemMetadata, columns, rowNumber,
                    includeViewLink, includeEditLink, ref columnNumber);
                html.Append(row.ToString());
            }
            // Build table boddy
            TagBuilder body = new TagBuilder("tbody");
            body.InnerHtml = html.ToString();
            // Return the html
            return body.ToString();
        }

        /// <summary>
        /// Returns the html for each cell of a table row.
        /// </summary>
        /// <param name="metaData">
        /// A collection of <see cref="Sandtrap.Web.Html.TableColumn"/> used 
        /// to initialise table properties and calculate footer totals.
        /// </param>
        /// <param name="columns">
        /// A collection of <see cref="Sandtrap.Web.Html.TableColumn"/> used 
        /// to initialise table properties and calculate footer totals.
        /// </param>
        /// <param name="rowNumber">
        /// The row number to display, or null of row numbers are not included.
        /// </param>
        /// <param name="includeViewLink">
        /// A value indicating if the table should include a column for a view details link.
        /// </param>
        /// <param name="includeEditLink">
        /// A value indicating if the table should include a column for a edit link.
        /// </param>
        /// <param name="columnIndex">
        /// The index of the column in the row.
        /// </param>
        private static string ReadonlyTableBodyRow(ModelMetadata metaData,
            List<TableColumn> columns, int? rowNumber, bool includeViewLink,
            bool includeEditLink, ref int columnIndex)
        {
            // Build the html for each cell
            StringBuilder html = new StringBuilder();
            if (rowNumber.HasValue)
            {
                html.Append(TableCellText(rowNumber.Value.ToString()));
            }
            foreach (ModelMetadata property in metaData.Properties)
            {
                TableColumn column = columns[columnIndex];
                if (!property.IsComplexType && column.IsExcluded)
                {
                    // Nothing to display
                    columnIndex++;
                    continue;
                }
                if (column.IncludeTotals)
                {
                    // Update running total
                    if (column.HasDisplayProperty)
                    {
                        string displayProperty = (string)property
                            .AdditionalValues[TableColumnAttribute.DisplayPropertyKey];
                        column.ColumnTotal += Convert.ToDecimal(property.Properties
                            .FirstOrDefault(m => m.PropertyName == displayProperty).Model);
                    }
                    else
                    {
                        column.ColumnTotal += Convert.ToDecimal(property.Model);
                    }
                }
                if (property.IsComplexType && !column.IsLink && !column.HasDisplayProperty)
                {
                    if (typeof(IEnumerable).IsAssignableFrom(property.ModelType))
                    {
                        // Its a collection within the collection so ignore.
                        continue;
                    }
                    else
                    {
                        // Add cells for each property of the type (recursive call)
                        html.Append(ReadonlyTableBodyRow(property, columns, null, false,
                            false, ref columnIndex));
                    }
                }
                else
                {
                    // Get the display text
                    string text = null;
                    if (column.IsEmailAddress && property.Model != null)
                    {
                        // Build link
                        TagBuilder link = new TagBuilder("a");
                        link.MergeAttribute("href", string.Format("mailto:{0}", property.Model));
                        link.InnerHtml = string.Format("{0}", property.Model);
                        text = link.ToString();
                    }//Codigo Jose Acedo
                    else if (column.IsIconColumn && property.Model != null)
                    {
                        // Build link
                        TagBuilder icon = new TagBuilder("i");
                        string iconClass = null;
                        string colorClass = null;

                        if (property.ModelType == typeof(bool))
                        {
                            if ((bool)property.Model)
                            {
                                iconClass = (string)property.AdditionalValues[TableIconAttribute.FaIconTrueKey];
                                colorClass = (string)property.AdditionalValues[TableIconAttribute.ColorTrueKey];

                            }
                            else
                            {
                                if (!String.IsNullOrEmpty(property.AdditionalValues[TableIconAttribute.FaIconFalseKey].ToString()))
                                {
                                    iconClass = (string)property.AdditionalValues[TableIconAttribute.FaIconFalseKey];
                                    colorClass = (string)property.AdditionalValues[TableIconAttribute.ColorFalseKey];
                                }
                             
                            }
                        }

                        if (property.ModelType == typeof(string))
                        {
                            if (property.Model == "T")
                            {
                                iconClass = (string)property.AdditionalValues[TableIconAttribute.FaIconTrueKey];
                                colorClass = (string)property.AdditionalValues[TableIconAttribute.ColorTrueKey];

                            }
                            else
                            {
                                if (!String.IsNullOrEmpty(property.AdditionalValues[TableIconAttribute.FaIconFalseKey].ToString()))
                                {
                                    iconClass = (string)property.AdditionalValues[TableIconAttribute.FaIconFalseKey];
                                    colorClass = (string)property.AdditionalValues[TableIconAttribute.ColorFalseKey];
                                }
                               
                            }
                        }

                        icon.MergeAttribute("class", iconClass);
                        icon.Attributes.Add("style", string.Format("color:{0}", colorClass));
                        text = icon.ToString();
                    }
                    else if (column.IsLink && property.Model != null)
                    {
                        // Build link
                        TagBuilder link = new TagBuilder("a");
                        // The link could be applied either at class level or property level
                        string url = null;
                        if (metaData.AdditionalValues.ContainsKey(TableLinkAttribute.UrlKey))
                        {
                            url = (string)metaData.AdditionalValues[TableLinkAttribute.UrlKey];
                        }
                        else
                        {
                            url = (string)property.AdditionalValues[TableLinkAttribute.UrlKey];
                        }
                        link.MergeAttribute("href", url);
                        // Get the display property
                        string displayProperty = null;
                        if (metaData.AdditionalValues.ContainsKey(TableLinkAttribute.DisplayPropertyKey))
                        {
                            displayProperty = (string)metaData
                                .AdditionalValues[TableLinkAttribute.DisplayPropertyKey];
                        }
                        else
                        {
                            displayProperty = (string)property
                                .AdditionalValues[TableLinkAttribute.DisplayPropertyKey];
                        }
                        if (property.PropertyName == displayProperty)
                        {
                            string formatString = property.DisplayFormatString ?? "{0}";
                            link.InnerHtml = string.Format(formatString, property.Model);
                        }
                        else
                        {
                            link.InnerHtml = string.Format("{0}", property.Properties
                                .FirstOrDefault(m => m.PropertyName == displayProperty).Model);
                        }
                        text = link.ToString();

                    }
                    else if (column.HasDisplayProperty && property.Model != null)
                    {
                        string formatString = column.FormatString ?? "{0}";
                        string displayProperty = (string)property
                            .AdditionalValues[TableColumnAttribute.DisplayPropertyKey];
                        text = string.Format(formatString, property.Properties
                            .FirstOrDefault(m => m.PropertyName == displayProperty).Model);
                    }
                    else
                    {
                        text = GetFormattedValue(property);
                    }
                    if (column.NoRepeat)
                    {
                        if (text == column.PreviousValue)
                        {
                            text = string.Empty;
                        }
                        else
                        {
                            column.PreviousValue = text;
                        }
                    }
                    html.Append(TableCellText(text));
                    columnIndex++;
                }
            }
            if (includeViewLink)
            {
                html.Append(TableCellViewLink(metaData));
            }
            if (includeEditLink)
            {
                html.Append(TableCellEditLink(metaData));
            }
            // Return the html
            return html.ToString();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="helper">
        /// 
        /// </param>
        /// <param name="collection">
        /// 
        /// </param>
        /// <param name="type">
        /// 
        /// </param>
        /// <param name="prefix">
        /// 
        /// </param>
        /// <param name="columns">
        /// 
        /// </param>
        /// <param name="includeButton">
        /// 
        /// </param>
        /// <param name="hasButton">
        /// 
        /// </param>
        private static string EditableTableBody(HtmlHelper helper, IEnumerable collection, Type type,
            string prefix, List<TableColumn> columns, bool includeButton, bool hasButton)
        {
            StringBuilder html = new StringBuilder();
            int rowNumber = 0;
            foreach (var item in collection)
            {
                int columnNumber = 0;
                string fieldName = string.Format("{0}[{1}]", prefix, rowNumber.ToString());
                ModelMetadata itemMetadata = ModelMetadataProviders.Current
                    .GetMetadataForType(() => item, type);
                TagBuilder row = new TagBuilder("tr");
                // Check for an archived property
                var isActiveMetadata = itemMetadata.Properties
                    .FirstOrDefault(m => m.PropertyName == itemMetadata.TableIsActiveProperty());
                if (isActiveMetadata != null && (bool)(isActiveMetadata.Model) == false)
                {
                    row.AddCssClass("archived");
                }
                row.InnerHtml = EditableTableBodyRow(helper, itemMetadata, fieldName, rowNumber, columns,
                    includeButton, hasButton, ref columnNumber);
                html.Append(row.ToString());
                rowNumber++;
            }
            // Build table boddy
            TagBuilder body = new TagBuilder("tbody");
            body.InnerHtml = html.ToString();
            // Return the html
            return body.ToString();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="helper">
        /// 
        /// </param>
        /// <param name="metaData">
        /// 
        /// </param>
        /// <param name="prefix">
        /// 
        /// </param>
        /// <param name="rowNumber">
        /// 
        /// </param>
        /// <param name="columns">
        /// 
        /// </param>
        /// <param name="includeButton">
        /// 
        /// </param>
        /// <param name="hasButton">
        /// 
        /// </param>
        /// <param name="columnIndex">
        /// 
        /// </param>
        /// <param name="innerHtml">
        /// 
        /// </param>
        private static string EditableTableBodyRow(HtmlHelper helper, ModelMetadata metaData,
            string prefix, int? rowNumber, List<TableColumn> columns, bool includeButton,
            bool hasButton, ref int columnIndex, StringBuilder innerHtml = null)
        {
            // Build the html for each cell
            StringBuilder html = new StringBuilder();
            if (innerHtml == null)
            {
                innerHtml = new StringBuilder();
            }
            foreach (ModelMetadata property in metaData.Properties)
            {
                string fieldName = string.Format("{0}.{1}", prefix, property.PropertyName);
                TableColumn column = columns[columnIndex];
                if (column.IsExcluded)
                {
                    // No column but hidden inputs required for posback
                    innerHtml.Append(HiddenInputHelper.HiddenInputForMetadata(property, fieldName, false, false));
                    columnIndex++;
                    continue;
                }
                if (column.IncludeTotals)
                {
                    // Update running total
                    column.ColumnTotal += Convert.ToDecimal(property.Model);
                }
                if (column.IsSelect)
                {
                    html.Append(TableCellSelect(property, fieldName, column.SelectList));
                    columnIndex++;
                }
                else if (column.IsReadonly)
                {
                    // Get the display text
                    string text = GetReadonlyDisplayText(property, column.HasDisplayProperty);
                    if (column.HasDisplayProperty && column.NoRepeat)
                    {
                        if (text == column.PreviousValue)
                        {
                            text = string.Empty;
                        }
                        else
                        {
                            column.PreviousValue = text;
                        }
                    }
                    html.Append(TableCellReadOnly(property, text, fieldName));
                    columnIndex++;
                }
                else if (property.IsComplexType)
                {
                    if (typeof(IEnumerable).IsAssignableFrom(property.ModelType))
                    {
                        // Its a collection within the collection so ignore.
                        continue;
                    }
                    else
                    {
                        // Add cells for each property of the type (recursive call)
                        html.Append(EditableTableBodyRow(helper, property, fieldName, rowNumber,
                            columns, false, false, ref columnIndex));
                    }
                }
                else
                {
                    // Get validation attributes
                    IDictionary<string, object> attributes = helper
                        .GetUnobtrusiveValidationAttributes(fieldName, property);
                    // Input types
                    if (property.ModelType == typeof(bool))
                    {
                        // Checkbox
                        html.Append(TableCellCheckBox(property, fieldName));
                    }
                    else if (property.ModelType.IsEnum)
                    {
                        //SelectHelper.
                    }
                    else if (property.ModelType.IsNumeric())
                    {
                        // Numeric input
                        html.Append(TableCellText(NumericInputHelper
                            .NumericInputForMetadata(property, fieldName, attributes)));
                    }
                    else
                    {
                        // Text input
                        html.Append(TableCellTextInput(property, fieldName, attributes));
                    }

                    // Enum (select)
                    // Select (also need to include in complex type)
                    // Date

                    columnIndex++;
                }
            }
            // Add indexer
            innerHtml.Append(TableCellIndexer(prefix, rowNumber));

            // Add button column
            if (includeButton)
            {
                // TODO: Add indexer and hidden inputs
                html.Append(TableCellDeleteButton(hasButton, innerHtml));
            }
            else
            {
                TagBuilder cell = new TagBuilder("td");
                cell.InnerHtml = innerHtml.ToString();
                html.Append(cell.ToString());
            }
            // Return the html
            return html.ToString();
        }

        private static string EditableTableNewRow(HtmlHelper helper, ModelMetadata metaData,
            string prefix, List<TableColumn> columns, bool includeButton, bool hasButton)
        {
            int index = 0;
            string fieldName = string.Format("{0}[#]", prefix);
            //string row = EditableTableBodyRow(helper, metaData, fieldName, null, columns, includeButton, hasButton, ref index);

            TagBuilder row = new TagBuilder("tr");
            row.InnerHtml = EditableTableBodyRow(helper, metaData, fieldName, null,
                columns, includeButton, hasButton, ref index); ;
            TagBuilder table = new TagBuilder("tbody");
            table.MergeAttribute("style", "display:none");
            table.InnerHtml = row.ToString();
            return table.ToString();
        }

        /// <summary>
        /// Returns the html for a table header cell.
        /// </summary>
        /// <param name="text">
        /// The text to display in the cell.
        /// </param>
        private static string TableHeaderCell(string text)
        {
            TagBuilder cell = new TagBuilder("th");
            if (text != null)
            {
                cell.InnerHtml = text;
            }
            return cell.ToString();
        }

        /// <summary>
        /// Returns the html for a readonly table cell.
        /// </summary>
        /// <param name="text">
        /// The text to display in the cell.
        /// </param>
        private static string TableCellText(string text)
        {
            TagBuilder cell = new TagBuilder("td");
            if (text != null)
            {
                cell.InnerHtml = text;
            }
            return cell.ToString();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="metaData">
        /// 
        /// </param>
        private static string TableCellEditLink(ModelMetadata metaData)
        {
            // Link
            TagBuilder link = new TagBuilder("a");
            link.MergeAttribute("href", (string)metaData
                .AdditionalValues[TableDisplayAttribute.UrlEditKey]);
            link.InnerHtml = "edit";
            // Table cell
            TagBuilder cell = new TagBuilder("td");
            cell.InnerHtml = link.ToString();
            return cell.ToString();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="metaData">
        /// 
        /// </param>
        private static string TableCellViewLink(ModelMetadata metaData)
        {
            //< div class="btn-group">
            //                <button data-toggle="dropdown" class="btn btn-default btn-xs dropdown-toggle">Action<span class="caret"></span></button>
            //                <ul class="dropdown-menu">
            //                    <li><a href = "#" > Action </ a ></ li >
            //                    < li >< a href="#">Another action</a></li>
            //                    <li><a href = "#" > Something else here</a></li>
            //                    <li class="divider"></li>
            //                    <li><a href = "#" > Separated link</a></li>
            //                </ul>
            //            </div>
            // Link
            TagBuilder div = new TagBuilder("div");
            div.Attributes.Add("class", "btn-group");
                TagBuilder button = new TagBuilder("button");
                button.Attributes.Add("data-toggle", "dropdown");
                button.Attributes.Add("class", "btn btn-primary btn-xs dropdown-toogle");          
                     TagBuilder span = new TagBuilder("span");
                     span.Attributes.Add("class", "caret");
                button.InnerHtml = "Acciones" + span.ToString();
                TagBuilder ul = new TagBuilder("ul");
                ul.Attributes.Add("class", "dropdown-menu");
                TagBuilder li = new TagBuilder("li");
                  TagBuilder link = new TagBuilder("a");
                  link.MergeAttribute("href", (string)metaData
                    .AdditionalValues[TableDisplayAttribute.UrlViewKey]);
                  link.InnerHtml = "Ver";
                li.InnerHtml = link.ToString();
                ul.InnerHtml = li.ToString();
            div.InnerHtml = button.ToString()  + ul.ToString();
            
            TagBuilder cell = new TagBuilder("td");
            cell.InnerHtml = div.ToString();
            return cell.ToString();
        }

        /// <summary>
        /// Returns the html for a editable table cell where the property is readonly.
        /// </summary>
        /// <param name="metaData">
        /// The metadata of the property.
        /// </param>
        /// <param name="text">
        /// The display text.
        /// </param>
        /// <param name="fieldName">
        /// The fully qualified property name.
        /// </param>
        /// <returns></returns>
        private static string TableCellReadOnly(ModelMetadata metaData, string text, string fieldName)
        {
            StringBuilder html = new StringBuilder();
            // Add display text
            TagBuilder div = new TagBuilder("div");
            div.InnerHtml = text;
            div.AddCssClass("table-text");
            html.Append(div.ToString());
            // Add hidden inputs
            html.Append(HiddenInputHelper.HiddenInputForMetadata(metaData, fieldName, false, false));
            TagBuilder cell = new TagBuilder("td");
            cell.InnerHtml = html.ToString();
            return cell.ToString();
        }



        private static string TableCellSelect(ModelMetadata metaData, string fieldName, IEnumerable items)
        {
            // Get the ID and Display properties
            string idProperty = (string)metaData.AdditionalValues[TableSelectAttribute.IDPropertyKey];
            string displayProperty = (string)metaData.AdditionalValues[TableSelectAttribute.DisplayPropertyKey];
            TagBuilder cell = new TagBuilder("td");
            cell.InnerHtml = SelectHelper.SelectForMetadata(metaData, fieldName, items, idProperty, displayProperty);
            return cell.ToString();
        }



        /// <summary>
        /// Returns the html for a table cell with a checkbox input.
        /// </summary>
        /// <param name="metaData">
        /// The metadata of the property.
        /// </param>
        /// <param name="fieldName">
        /// The fully qualified name of the propery.
        /// </param>
        private static string TableCellCheckBox(ModelMetadata metaData, string fieldName)
        {
            StringBuilder html = new StringBuilder();
            // Checkbox 
            TagBuilder checkbox = new TagBuilder("input");
            checkbox.MergeAttribute("type", "checkbox");
            checkbox.MergeAttribute("name", fieldName);
            checkbox.MergeAttribute("value", "true");
            if (metaData.Model != null && (bool)metaData.Model)
            {
                checkbox.MergeAttribute("checked", "checked");
            }
            html.Append(checkbox.ToString(TagRenderMode.SelfClosing));
            // Build additional hidden input to address scenario where 
            // unchecked checkboxes are not sent in the request.
            TagBuilder hidden = new TagBuilder("input");
            hidden.MergeAttribute("type", "hidden");
            hidden.MergeAttribute("name", fieldName);
            hidden.MergeAttribute("value", "false");
            html.Append(hidden.ToString(TagRenderMode.SelfClosing));
            // Table cell
            TagBuilder cell = new TagBuilder("td");
            cell.InnerHtml = html.ToString();
            return cell.ToString();
        }

        /// <summary>
        /// Returns the html for a text input.
        /// </summary>
        /// <param name="metaData">
        /// The metadata of the property.
        /// </param>
        /// <param name="fieldName">
        /// The fullly qualified name of the property
        /// </param>
        /// <param name="attributes">
        /// The properties validation attributes.
        /// </param>
        private static string TableCellTextInput(ModelMetadata metaData, string fieldName,
            IDictionary<string, object> attributes)
        {
            // Build the input
            TagBuilder input = new TagBuilder("input");
            input.MergeAttribute("type", "text");
            input.MergeAttribute("name", fieldName);
            input.MergeAttribute("value", string.Format("{0}", metaData.Model ?? GetDefaultValue(metaData)));
            // Add validation attributes
            input.MergeAttributes(attributes);
            TagBuilder cell = new TagBuilder("td");
            cell.InnerHtml = input.ToString(TagRenderMode.SelfClosing);
            // Return the html
            return cell.ToString();
        }

        /// <summary>
        /// Returns the html for a table button.
        /// </summary>
        private static string TableCellDeleteButton(bool hasButton, StringBuilder innerHtml)
        {
            StringBuilder html = new StringBuilder();
            if (hasButton)
            {
                TagBuilder button = new TagBuilder("button");
                button.MergeAttribute("type", "button");
                button.AddCssClass("table-button");
                button.AddCssClass("delete-button");
                TagBuilder container = new TagBuilder("div");
                container.InnerHtml = button.ToString();
                html.Append(container.ToString());
                html.Append(innerHtml.ToString());
            }
            TagBuilder cell = new TagBuilder("td");
            cell.InnerHtml = html.ToString();
            // Return the html
            return cell.ToString();
        }


        private static string TableCellAddButton(bool hasButton)
        {
            StringBuilder html = new StringBuilder();
            if (hasButton)
            {
                TagBuilder button = new TagBuilder("button");
                button.MergeAttribute("type", "button");
                button.AddCssClass("table-button");
                button.AddCssClass("add-button");
                TagBuilder container = new TagBuilder("div");
                container.InnerHtml = button.ToString();
                html.Append(container.ToString());
            }
            TagBuilder cell = new TagBuilder("td");
            cell.InnerHtml = html.ToString();
            // Return the html
            return cell.ToString();
        }


        /// <summary>
        /// Returns the html for a table cell containing a row indexer
        /// </summary>
        /// <param name="fieldName">
        /// The name of the property.
        /// </param>
        /// <param name="index">
        /// The index of the row.
        /// </param>
        private static string TableCellIndexer(string fieldName, int? index)
        {
            TagBuilder indexer = new TagBuilder("input");
            indexer.MergeAttribute("type", "hidden");
            indexer.MergeAttribute("name", string.Format("{0}.Index", fieldName));
            indexer.MergeAttribute("value", index.HasValue ? index.Value.ToString() : "%");
            // Return the html
            return indexer.ToString(TagRenderMode.SelfClosing);
        }

        /// <summary>
        /// Returns the <see cref="System.Type"/> of the items in the collection.
        /// </summary>
        /// <param name="collection">
        /// The collection of items to display in the table.
        /// </param>
        private static Type GetCollectionType(IEnumerable collection)
        {
            // Check types that wont make any sense to render in a table
            if (collection.IsGrouped())
            {
                return null;
            }
            Type type = collection.GetType();
            if (type.IsGenericType)
            {
                return type.GetInterfaces().Where(t => t.IsGenericType)
                    .Where(t => t.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                    .Single().GetGenericArguments()[0];
            }
            if (collection.GetType().IsArray)
            {
                return type.GetElementType();
            }
            // Who knows?
            return null;
        }

        /// <summary>
        /// Returns the default value for a property.
        /// </summary>
        /// <param name="metaData">
        /// The property metadata.
        /// </param>
        private static object GetDefaultValue(ModelMetadata metaData)
        {
            return metaData.ModelType.IsValueType ? Activator.CreateInstance(metaData.ModelType) : null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="metaData">
        /// 
        /// </param>
        private static string GetFormattedValue(ModelMetadata metaData)
        {
            if (metaData.Model == null)
            {
                return metaData.NullDisplayText;
            }
            if (metaData.ModelType == typeof(Nullable<bool>) || metaData.ModelType == typeof(bool))
            {
                // If we got this far it has a value
                return (bool)metaData.Model ? "Sí" : "No";
            }
            // Return the formatted value
            string formatString = metaData.DisplayFormatString ?? "{0}";
            return string.Format(formatString, metaData.Model);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="metaData">
        /// 
        /// </param>
        /// <param name="hasDisplayProperty">
        /// 
        /// </param>
        private static string GetReadonlyDisplayText(ModelMetadata metaData, bool hasDisplayProperty)
        {
            if (hasDisplayProperty)
            {
                string displayProperty = (string)metaData
                    .AdditionalValues[TableColumnAttribute.DisplayPropertyKey];
                return string.Format("{0}", metaData.Properties
                    .FirstOrDefault(m => m.PropertyName == displayProperty).Model);
            }
            else
            {
                return string.Format("{0}", metaData.Model);
            }
        }

        #endregion


        // UNDONE: The metdata of a property now created based on the type in the collection (as opposed to
        // the model type) so not an issue? More testing required!
        //if (property.IsComplexType)
        //{
        //    // Check if the model matches the model type (the table header outputs the properties of the model type, 
        //    // but the model may be a derived class that contains additional properties - if these properties were 
        //    // output in the table it would screw up the columns so we need to include only inherited properties
        //    if (metaData.ModelType != metaData.Model.GetType())
        //    {
        //        // Get the names of properties that need to be excluded
        //        BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly;
        //        IEnumerable<string> excluded = metaData.Model.GetType().GetProperties(flags).Select(p => p.Name);
        //        if (excluded.Contains(propertyMetadata.PropertyName))
        //        {
        //            // Ignore the property
        //            continue;
        //        }
        //    }
        //}


    }

}
