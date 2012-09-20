namespace Terminal.Domain.Entities
{
    using System;
    using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

    public class Tag
    {
        public string Name { get; set; }

        public virtual ICollection<Link> Links { get; set; }
    }
}