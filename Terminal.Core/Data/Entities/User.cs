namespace Terminal.Core.Data.Entities
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Terminal.Core.ExtensionMethods;

    public class User
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string IPAddress { get; set; }
        public DateTime JoinDate { get; set; }
        public DateTime LastLogin { get; set; }
        public string Email { get; set; }
        public bool NotifyReplies { get; set; }
        public bool NotifyMessages { get; set; }
        public bool AutoFollow { get; set; }
        public string Gender { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Location { get; set; }
        public string Bio { get; set; }
        public long Credits { get; set; }
        public string TimeZone { get; set; }
        public bool Sound { get; set; }
        public bool ChatOpen { get; set; }
        public bool ShowTimestamps { get; set; }

        public virtual ICollection<Alias> Aliases { get; set; }
        public virtual Ban BanInfo { get; set; }
        public virtual ICollection<Ban> BannedUsers { get; set; }
        public virtual ICollection<Ignore> IgnoredBy { get; set; }
        public virtual ICollection<Ignore> Ignores { get; set; }
        public virtual ICollection<LinkClick> LinkClicks { get; set; }
        public virtual ICollection<LinkComment> LinkComments { get; set; }
        public virtual ICollection<Link> Links { get; set; }
        public virtual ICollection<LinkVote> LinkVotes { get; set; }
        public virtual ICollection<Message> ReceivedMessages { get; set; }
        public virtual ICollection<Message> SentMessages { get; set; }
        public virtual ICollection<Reply> Replies { get; set; }
        public virtual ICollection<TopicFollow> FollowedTopics { get; set; }
        public virtual ICollection<TopicVisit> VisitedTopics { get; set; }
        public virtual ICollection<Topic> Topics { get; set; }
        public virtual ICollection<UserActivityLogItem> UserActivityLog { get; set; }
        public virtual ICollection<Role> Roles { get; set; }
        public virtual ICollection<ChannelStatus> ChannelStatuses { get; set; }


        public bool IsModeratorOrAdministrator()
        {
            return IsModerator || IsAdministrator;
        }
        
        public bool IsModerator
        {
            get { return Roles.Any(x => x.Name.Is("Moderator")); }
        }

        public bool IsAdministrator
        {
            get { return Roles.Any(x => x.Name.Is("Administrator")); }
        }
    }
}