using System;
using System.ComponentModel;
using System.Reflection;

namespace ADPM.Common.Extensions
{
    
    public static class EnumExtensions
    {

        /// <summary>
        /// Returns the value of an enums <see cref="System.Component.DataAnnotations
        /// .DescriptionAttribute"/> if it exists, otherwise the enums value.
        /// </summary>
        /// <param name="value">
        /// The enum instance that this method extends.
        /// </param>
        public static string ToDescription(this Enum value)
        {
            FieldInfo field = value.GetType().GetField(value.ToString());
            DescriptionAttribute[] attributes = (DescriptionAttribute[])field
                .GetCustomAttributes(typeof(DescriptionAttribute), false);
            if (attributes.Length > 0)
            {
                return attributes[0].Description;
            }
            return value.ToString();
        }

    }
}
