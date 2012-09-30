namespace Terminal.Domain.Data.Entities
{
    using System;
    using System.Collections.Generic;

    public class Reply
    {
        public long ReplyID { get; set; }
        public long TopicID { get; set; }
        public string Username { get; set; }
        public DateTime PostedDate { get; set; }
        public string Body { get; set; }
        public bool ModsOnly { get; set; }
        public DateTime LastEdit { get; set; }
        public string EditedBy { get; set; }

        public virtual Topic Topic { get; set; }
        public virtual User User { get; set; }

        /// <summary>
        /// Checks if the reply, the topic, or the board is moderator only.
        /// </summary>
        /// <returns></returns>
        public bool IsModsOnly()
        {
            return (ModsOnly || Topic.ModsOnly || Topic.Board.ModsOnly);
        }
    }
}