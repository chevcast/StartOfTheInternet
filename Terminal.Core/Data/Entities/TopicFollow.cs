namespace Terminal.Core.Data.Entities
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

    public class TopicFollow
    {
        public string Username { get; set; }
        public long TopicID { get; set; }
        public bool Saved { get; set; }

        public virtual Topic Topic { get; set; }
        public virtual User User { get; set; }
    }
}