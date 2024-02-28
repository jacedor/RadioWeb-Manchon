using System;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ADPM.Common.Web.DataAnnotations
{

    /// <summary>
    /// Definimos un atributo para crear un icono dentro de una celda de la 
    /// by the <see cref="Sandtrap.Web.Html.TableHelper.DisplayTableFor"/> method.
    /// </summary>
    /// <exception cref="ArgumentNullException">
    /// If the <see cref="Controller"/> is null.
    /// </exception>
    /// <exception cref="ArgumentException">
    ///  If the property the attribute is applied to does not contain the properties defined
    ///  by <see cref="IDProperty"/> or <see cref="DisplayProperty"/>.
    /// </exception>
    /// <remarks>
    /// If the attribute is applied to a property and the property is not a complex type, then
    /// the attribute is ignored.
    /// If the attribute is applied to a complex property, only the hyperlink is rendered and all 
    /// other properties of the complex type are ignored.
    /// </remarks>
    /// <example>
    /// Given the following class definitions:
    /// <code>
    /// public class Organisation
    /// {
    ///     public int ID { get; set; }
    ///     public string Name { get; set; }
    ///     public string Alias { get; set; }
    /// }
    /// [TableLink(Controller = "Project", Action = "Edit")]
    /// public class Project
    /// {
    ///     public int ID { get; set; }
    ///     public string Name { get;  set; }
    ///     [TableLink(Controller = "Organisation", TextProperty = "Alias")]
    ///     public Organisation Client { get; set; }
    /// }
    /// </code>
    /// where the properties of Project are
    /// <para>ID = 17</para>
    /// <para>Name = "Windsor Hospital"</para>
    /// <para>Client.ID = 104</para>
    /// <para>Client.Name = "Acme Developments Pty. Ltd."</para>
    /// <para>Client.Alias = "Acme"</para>
    /// then the @Html.TableDisplayFor() method applied to a collection of projects will 
    /// render <code>&lt;a href="/Project/Edit/17"&gt;Windsor Hospital&lt;/a&gt;</code>
    /// in the tables Name column, and
    /// <code>&lt;a href="/Organisation/Details/104"&gt;Acme&lt;/a&gt;</code>
    /// in the tables Client column.
    /// </example>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Property)]
    public class TableIconAttribute : Attribute, IMetadataAware
    {

        #region .Declarations 

        // Defaults
        private const string _Action = "Details";
        private const string _IDProperty = "ID";
        private const string _DisplayProperty = "Name";
        private const string _faIconTrue = "fa-clipboard";
        private const string _ColorTrue = "green";

        private const string _faIconFalse = "fa-clipboard";
        private const string _ColorFalse = "red";

        // Error messages
        private const string _InvalidController = "The controller cannot be null.";
        private const string _InvalidProperty = "The class definition '{0}' does not contain a public property named '{1}'";

        #endregion

        #region .Constructors 

        /// <summary>
        /// Initialises a new instance of a TableLinkAttribute class  with default 
        /// properties.
        /// </summary>
        public TableIconAttribute()
        {
            // Set defaults
            Action = _Action;
            IDProperty = _IDProperty;
            DisplayProperty = _DisplayProperty;
            FaIconTrue = _faIconTrue;
            FaIconFalse = _faIconFalse;
            ColorTrue = _ColorTrue;
            ColorFalse = _ColorFalse;
        }

        #endregion

        #region .Metadata keys 

        /// <summary>
        /// Gets the key for the metadata ActionLink property.
        /// </summary>
        public static string TableIconKey
        {
            get { return "TableIcon"; }
        }

        /// <summary>
        /// Gets the key for the metadata TextProperty property.
        /// </summary>
        public static string IDPropertyKey
        {
            get { return "TableIconIDProperty"; }
        }

        /// <summary>
        /// Gets the key for the metadata TextProperty property.
        /// </summary>
        public static string DisplayPropertyKey
        {
            get { return "TableIconDisplayProperty"; }
        }


        /// <summary>
        /// Get the key for the metadata Url property.
        /// </summary>
        public static string UrlKey
        {
            get { return "TableIconUrl"; }
        }

        /// <summary>
        /// Get the key for the metadata Url property.
        /// </summary>
        public static string FaIconTrueKey
        {
            get { return "TableIconFaIconTrue"; }
        }
        public static string FaIconFalseKey
        {
            get { return "TableIconFaIconFalse"; }
        }
        public static string ColorTrueKey
        {
            get { return "TableIconColorTrue"; }
        }
        public static string ColorFalseKey
        {
            get { return "TableIconColorFalse"; }
        }

        #endregion

        #region .Properties 

        /// <summary>
        /// Gets or sets the Fa-IconClass for True Propertyes 
        /// </summary>
        public string FaIconTrue { get; set; }
        /// <summary>
        /// Gets or sets the Fa-IconClass for True Propertyes 
        /// </summary>
        public string FaIconFalse { get; set; }
        /// <summary>
        /// Gets or sets the Color for True Propertyes 
        /// </summary>
        public string ColorTrue { get; set; }
        /// <summary>
        /// Gets or sets the Color for True Propertyes 
        /// </summary>
        public string ColorFalse { get; set; }

        /// <summary>
        /// Gets or sets the name of the route controller.
        /// </summary>
        public string Controller { get; set; }

        /// <summary>
        /// Gets or sets the name of the route action.
        /// The default is 'Details'.
        /// </summary>
        public string Action { get; set; }

        /// <summary>
        /// Gets or sets the name of the property that provides the value to the  
        /// route parameter. 
        /// The default is 'ID'.
        /// </summary>
        public string IDProperty { get; set; }

        /// <summary>
        /// Gets or sets the name of the property that provides the value to display 
        /// in the hyperlink.
        /// The default is 'Name'.
        /// </summary>
        public string DisplayProperty { get; set; }

        #endregion

        #region .Methods 

        /// <summary>
        /// Adds additional metadata values used to render the hyperlink.
        /// </summary>
        public void OnMetadataCreated(ModelMetadata metaData)
        {
            //// If applied to a property, the property must be a complex type
            //if (metaData.ContainerType != null && !metaData.IsComplexType)
            //{       
            //    return;
            //}
            // Check the controller has been provided
            if (Controller == null)
            {
                throw new ArgumentNullException(_InvalidController);
            }
            //// Check the ID and Display properties exist
            //ModelMetadata idMetaData = metaData.Properties
            //    .FirstOrDefault(m => m.PropertyName == IDProperty);
            //if (idMetaData == null)
            //{
            //    throw new ArgumentException(string
            //        .Format(_InvalidProperty, metaData.ModelType.Name, IDProperty));
            //}
            //ModelMetadata textMetaData = metaData.Properties
            //    .FirstOrDefault(m => m.PropertyName == DisplayProperty);
            //if (textMetaData == null)
            //{
            //    throw new ArgumentException(string
            //        .Format(_InvalidProperty, metaData.ModelType.Name, DisplayProperty));
            //}
            // Add metadata
            metaData.AdditionalValues[TableIconKey] = true;
            metaData.AdditionalValues[IDPropertyKey] = IDProperty;
            metaData.AdditionalValues[DisplayPropertyKey] = DisplayProperty;
            metaData.AdditionalValues[FaIconTrueKey] = FaIconTrue;
            metaData.AdditionalValues[FaIconFalseKey] = FaIconFalse;
            metaData.AdditionalValues[ColorFalseKey] = ColorFalse;
            metaData.AdditionalValues[ColorTrueKey] = ColorTrue;

            if (metaData.Model != null)
            {
                string root = HttpRuntime.AppDomainAppVirtualPath;
                if (root == "/")
                {
                    root = string.Empty;
                }
                string url = "/TEST.HTML";     // string.Format("{0}/{1}/{2}/{3}", root, Controller,
                  //  Action ?? _Action, idMetaData.Model); ;
                metaData.AdditionalValues[UrlKey] = url;
            }
        }

        #endregion

    }

}
