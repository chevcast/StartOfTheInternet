namespace Terminal.Domain.Entities
{
    using System;
    using System.Collections.Generic;

    public class Ignore
    {
        public long IgnoreID { get; set; }
        public string InitiatingUser { get; set; }
        public string IgnoredUser { get; set; }

        public virtual User User { get; set; }
        public virtual User Ignores { get; set; }
    }
}