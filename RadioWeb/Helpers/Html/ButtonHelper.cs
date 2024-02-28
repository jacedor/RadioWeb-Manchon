using System.Web.Mvc;

 namespace RadioWeb.Helpers.Web.Html

{

    /// <summary>
    /// Renders html for buttons.
    /// </summary>
    public static class ButtonHelper
    {

        #region .Methods 

        /// <summary>
        /// Returns the html for a drop down button associated with an input control.
        /// </summary>
        public static string DropButton()
        {
            TagBuilder button = new TagBuilder("button");
            button.MergeAttribute("type", "button");
            // Remove from tabbing so main element does not lose focus
            button.MergeAttribute("tabindex", "-1");
            button.AddCssClass("drop-button");
            // Add essential style properties
            button.MergeAttribute("style", "position:absolute;");
            // Return the html
            return button.ToString();
        }

        #endregion

    }


}
