using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Terminal.Core.ExtensionMethods
{
    /// <summary>
    /// Extentions to lists.
    /// </summary>
    public static class ListExtensions
    {
        public static string ToCommaDelimitedString(this List<string> args)
        {
            if (args != null)
            {
                var stringBuilder = new StringBuilder();
                foreach (var arg in args)
                    stringBuilder.Append(arg).Append(", ");
                return stringBuilder.ToString().TrimEnd(',', ' ');
            }
            else
                return null;
        }

        public static string ToSpaceDelimitedString(this List<string> args)
        {
            if (args != null)
            {
                var stringBuilder = new StringBuilder();
                foreach (var arg in args)
                    stringBuilder.Append(arg).Append(" ");
                return stringBuilder.ToString().Trim();
            }
            else
                return null;
        }
    }
}
