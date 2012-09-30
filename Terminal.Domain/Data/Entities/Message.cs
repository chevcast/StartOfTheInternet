namespace Terminal.Domain.Data.Entities
{
    using System;
    using System.Collections.Generic;

    public class Message
    {
        public long MessageID { get; set; }
        public string Sender { get; set; }
        public string Recipient { get; set; }
        public bool MessageRead { get; set; }
        public bool SenderDeleted { get; set; }
        public bool RecipientDeleted { get; set; }
        public DateTime SentDate { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
        public bool RecipientLocked { get; set; }
        public bool SenderLocked { get; set; }

        public virtual User FromUser { get; set; }
        public virtual User ToUser { get; set; }
    }
}