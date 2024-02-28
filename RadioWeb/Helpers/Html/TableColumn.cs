
using System.Collections;
namespace RadioWeb.Helpers.Web.Html
{

    /// <summary>
    /// Utility class used by the <see cref="Sandtrap.Web.Html.TableHelper.TableDisplayFor"/> and 
    /// <see cref="Sandtrap.Web.Html.TableHelper.TableEditorFor"/> methods to set column properties 
    /// when creating the table header, update totals when creating rows and get totals
    /// when creating footers.
    /// </summary>
    /// <remarks>
    /// Setting the values when creating the header avoids repeated calls to the more 
    /// complex extension methods.
    /// </remarks>
    internal class TableColumn
    {

        internal string PropertyName { get; set; }

        internal bool IsExcluded { get; set; }

        internal bool NoRepeat { get; set; }

        internal string PreviousValue { get; set; }

        internal bool IncludeTotals { get; set; }

        internal decimal ColumnTotal { get; set; }

        internal string FormatString { get; set; }

        internal bool IsEmailAddress { get; set; }

        internal bool IsIconColumn { get; set; }

        internal bool IsLink { get; set; }

        internal bool IsReadonly { get; set; }

        internal bool HasDisplayProperty { get; set;}

        internal string DisplayProperty { get; set; }

        internal bool IsSelect { get; set; }

        internal IEnumerable SelectList { get; set; }

    }

}
