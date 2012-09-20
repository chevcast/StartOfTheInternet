namespace Terminal.Domain.Entities
{
    using System;
    using System.Collections.Generic;

    public class Link
    {
        public long LinkID { get; set; }
        public string Username { get; set; }
        public DateTime Date { get; set; }
        public string URL { get; set; }
        public string Description { get; set; }
        public string Title { get; set; }

        public virtual ICollection<LinkClick> LinkClicks { get; set; }
        public virtual ICollection<LinkComment> LinkComments { get; set; }
        public virtual ICollection<LinkVote> LinkVotes { get; set; }
        public virtual User User { get; set; }
        public virtual ICollection<Tag> Tags { get; set; }
    }
}