namespace Terminal.Core.Data.Entities
{
    using System;
    using System.Linq;
    using System.Collections.Generic;

    public class Topic
    {
        public long TopicID { get; set; }
        public short BoardID { get; set; }
        public string Username { get; set; }
        public DateTime PostedDate { get; set; }
        public string Title { get; set; }
        public string Body { get; set; }
        public bool ModsOnly { get; set; }
        public bool Locked { get; set; }
        public bool Stickied { get; set; }
        public bool GlobalSticky { get; set; }
        public DateTime LastEdit { get; set; }
        public string EditedBy { get; set; }

        public virtual Board Board { get; set; }
        public virtual ICollection<Reply> Replies { get; set; }
        public virtual ICollection<TopicFollow> Followers { get; set; }
        public virtual ICollection<TopicVisit> Visits { get; set; }
        public virtual User User { get; set; }

        /// <summary>
        /// Checks if the topic or board is moderator-only.
        /// </summary>
        /// <returns>True if the topic or its board is moderator-only.</returns>
        public bool IsModsOnly()
        {
            return (ModsOnly || Board.ModsOnly);
        }

        public IEnumerable<Reply> GetReplies(bool isModerator)
        {
            if (isModerator)
                return Replies
                    .OrderBy(x => x.PostedDate);
            else
                return Replies
                    .Where(x => !x.ModsOnly)
                    .OrderBy(x => x.PostedDate);
        }
    }
}