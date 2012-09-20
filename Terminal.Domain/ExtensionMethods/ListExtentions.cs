using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Terminal.Domain.ExtensionMethods
{
    /// <summary>
    /// Extentions to lists.
    /// </summary>
    public static class ListExtensions
    {
        public static string ToCommaDelimitedString(this List<string> arg)
        {
            if (arg != null)
            {
                var stringBuilder = new StringBuilder();
                foreach (var listItem in arg)
                {
                    stringBuilder.Append(listItem);
                    stringBuilder.Append(", ");
                }
                return stringBuilder.ToString().TrimEnd(',', ' ');
            }
            else
                return null;
        }
    }
}
