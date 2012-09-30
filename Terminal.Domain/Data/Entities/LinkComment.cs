namespace Terminal.Domain.Data.Entities
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

    public class LinkComment
    {
        public int CommentID { get; set; }
        public long LinkID { get; set; }
        public string Username { get; set; }
        public DateTime Date { get; set; }
        public string Body { get; set; }

        public virtual Link Link { get; set; }
        public virtual User User { get; set; }
    }
}