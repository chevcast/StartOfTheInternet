using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Terminal.Domain.ExtensionMethods
{
    public static class IntExtensions
    {
        public static int NumberOfPages(this int totalItems, int itemsPerPage)
        {
            return (int)(Math.Ceiling((Decimal)totalItems / (Decimal)itemsPerPage));
        }
    }
}
