namespace Terminal.Domain.Data.Entities
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

    public class Role
    {
        public string Name { get; set; }

        public virtual ICollection<User> Users { get; set; }
    }
}