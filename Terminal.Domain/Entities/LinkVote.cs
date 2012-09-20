namespace Terminal.Domain.Entities
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

    public class LinkVote
    {
        public long LinkID { get; set; }
        public string Username { get; set; }
        public short Rating { get; set; }

        public virtual Link Link { get; set; }
        public virtual User User { get; set; }
    }
}