using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Terminal.Core.ExtensionMethods;

namespace Terminal.Core.Utilities
{
    public static class PagingUtility
    {
        public static string[] Shortcuts
        {
            get
            {
                return new string[]
                {
                    "FIRST",
                    "PREV",
                    "NEXT",
                    "LAST"
                };
            }
        }

        public static int TranslateShortcut(string shortcut, int currentPage)
        {
            if (shortcut.Is("FIRST"))
                currentPage = 1;
            else if (shortcut.Is("PREV"))
                currentPage--;
            else if (shortcut.Is("NEXT"))
                currentPage++;
            else if (shortcut.Is("LAST"))
                currentPage = int.MaxValue;

            return currentPage;
        }
    }
}
