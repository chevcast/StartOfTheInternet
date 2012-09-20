namespace Terminal.Domain.Entities
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

    public class Alias
    {
        public string Username { get; set; }
        public string Shortcut { get; set; }
        public string Command { get; set; }

        public virtual User User { get; set; }
    }
}