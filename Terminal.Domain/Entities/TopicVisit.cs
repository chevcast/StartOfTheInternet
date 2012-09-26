namespace Terminal.Domain.Entities
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.ComponentModel.DataAnnotations;

    public class TopicVisit
    {
        [Key, Column(Order = 1)]
        public string Username { get; set; }
        [Key, Column(Order = 2)]
        public long TopicID { get; set; }
        public int Count { get; set; }

        public virtual Topic Topic { get; set; }
        public virtual User User { get; set; }
    }
}
