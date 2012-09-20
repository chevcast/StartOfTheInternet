namespace Terminal.Domain.Entities
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

    public class Ban
    {
        public string Username { get; set; }
        public string Creator { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Reason { get; set; }

        public virtual User User { get; set; }
        public virtual User BanCreator { get; set; }
    }
}