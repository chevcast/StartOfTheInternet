using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Terminal.Core.Objects
{
    /// <summary>
    /// Holds relevant information related to a page of a collection of information.
    /// </summary>
    public class CollectionPage<T>
    {
        /// <summary>
        /// A page of items.
        /// </summary>
        public IList<T> Items { get; set; }

        /// <summary>
        /// Total number of items, regardless of page.
        /// </summary>
        public int TotalItems { get; set; }

        /// <summary>
        /// The total number of pages.
        /// </summary>
        public int TotalPages { get; set; }
    }
}
