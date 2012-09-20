namespace Terminal.Domain.Entities
{
    using System;
    using System.Collections.Generic;

    public class UserActivityLogItem
    {
        public long ID { get; set; }
        public string Username { get; set; }
        public DateTime Date { get; set; }
        public string Information { get; set; }
        public string Type { get; set; }

        public virtual User User { get; set; }
    }
}