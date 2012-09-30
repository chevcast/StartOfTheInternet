namespace Terminal.Core.Data.Entities
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

    public class InviteCode
    {
        public string Code { get; set; }
        public string Username { get; set; }

        public virtual User User { get; set; }
    }
}