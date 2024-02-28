using System;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ADPM.Common.Web.DataAnnotations
{

    /// <summary>
    /// 
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class TableSelectAttribute : Attribute, IMetadataAware
    {

        #region .Declarations 

        // Defaults
        private const string _IDProperty = "ID";
        private const string _DisplayProperty = "Name";
        // Error messages
        private const string _InvalidProperty = "The class definition '{0}' does not contain a public property named '{1}'";

        #endregion

        #region .Constructors 

        /// <summary>
        /// 
        /// </summary>
        public TableSelectAttribute()
        {
            IDProperty = _IDProperty;
            DisplayProperty = _DisplayProperty;
        }

        #endregion

        #region .Metadata keys 

        /// <summary>
        /// Gets the key for the metadata
        /// </summary>
        public static string TableSelectKey
        {
            get { return "TableSelect"; }
        }

        /// <summary>
        /// Gets the key for the metadata <see cref="IDProperty"/> property.
        /// </summary>
        public static string IDPropertyKey
        {
            get { return "TableSelectIDProperty"; }
        }

        /// <summary>
        /// Gets the key for the metadata <see cref="DisplayProperty"/> property.
        /// </summary>
        public static string DisplayPropertyKey
        {
            get { return "TableSelectDisplayProperty"; }
        }

        #endregion

        #region .Properties 

        /// <summary>
        /// Gets or sets the name of the property that uniquely identifies the 
        /// model in a collection. 
        /// The default is 'ID'.
        /// </summary>
        public string IDProperty { get; set; }

        /// <summary>
        /// Gets or sets the name of the property that provides the value to display 
        /// in the select.
        /// The default is 'Name'.
        /// </summary>
        public string DisplayProperty { get; set; }

        #endregion

        #region .Methods 

        /// <summary>
        /// Adds additional metadata values used to render the select.
        /// </summary>
        public void OnMetadataCreated(ModelMetadata metaData)
        {
            // If applied to a property, the property must be a complex type
            if (metaData.ContainerType != null && !metaData.IsComplexType)
            {
                return;
            }
            // Check the ID and Display properties exist
            ModelMetadata idMetaData = metaData.Properties
                .FirstOrDefault(m => m.PropertyName == IDProperty);
            if (idMetaData == null)
            {
                throw new ArgumentException(string
                    .Format(_InvalidProperty, metaData.ModelType.Name, IDProperty));
            }
            ModelMetadata textMetaData = metaData.Properties
                .FirstOrDefault(m => m.PropertyName == DisplayProperty);
            if (textMetaData == null)
            {
                throw new ArgumentException(string
                    .Format(_InvalidProperty, metaData.ModelType.Name, DisplayProperty));
            }
            // Add metadata
            metaData.AdditionalValues[TableSelectKey] = true;
            metaData.AdditionalValues[IDPropertyKey] = IDProperty;
            metaData.AdditionalValues[DisplayPropertyKey] = DisplayProperty;

            //if (metaData.Model != null)
            //{
            //    metaData.AdditionalValues[IDPropertyKey] = IDProperty;
            //    metaData.AdditionalValues[DisplayPropertyKey] = DisplayProperty;
            //}
        }

        #endregion

    }

}
