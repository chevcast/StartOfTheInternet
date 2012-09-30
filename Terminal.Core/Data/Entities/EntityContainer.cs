namespace Terminal.Core.Data.Entities
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Data.Entity.Infrastructure;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Collections.Generic;
    
    public class EntityContainer : DbContext
    {
        public DbSet<Alias> Aliases { get; set; }
        public DbSet<Ban> Bans { get; set; }
        public DbSet<Board> Boards { get; set; }
        public DbSet<Ignore> Ignores { get; set; }
        public DbSet<InviteCode> InviteCodes { get; set; }
        public DbSet<LinkClick> LinkClicks { get; set; }
        public DbSet<LinkComment> LinkComments { get; set; }
        public DbSet<Link> Links { get; set; }
        public DbSet<LinkVote> LinkVotes { get; set; }
        public DbSet<LUEser> LUEsers { get; set; }
        public DbSet<MadlibTemplate> MadlibTemplates { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<Reply> Replies { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<Tag> Tags { get; set; }
        public DbSet<TopicFollow> TopicFollows { get; set; }
        public DbSet<TopicVisit> TopicVisits { get; set; }
        public DbSet<Topic> Topics { get; set; }
        public DbSet<UserActivityLogItem> UserActivityLog { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Variable> Variables { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Alias>().HasKey(x => new { x.Username, x.Shortcut });
            modelBuilder.Entity<Ban>().HasKey(x => x.Username);
            modelBuilder.Entity<InviteCode>().HasKey(x => x.Code);
            modelBuilder.Entity<LinkClick>().HasKey(x => new { x.LinkID, x.Username });
            modelBuilder.Entity<LinkComment>().HasKey(x => x.CommentID);
            modelBuilder.Entity<LinkVote>().HasKey(x => new { x.LinkID, x.Username });
            modelBuilder.Entity<LUEser>().HasKey(x => x.Username);
            modelBuilder.Entity<Role>().HasKey(x => x.Name);
            modelBuilder.Entity<Tag>().HasKey(x => x.Name);
            modelBuilder.Entity<TopicFollow>().HasKey(x => new { x.Username, x.TopicID });
            modelBuilder.Entity<TopicVisit>().HasKey(x => new { x.Username, x.TopicID });
            modelBuilder.Entity<User>().HasKey(x => x.Username);
            modelBuilder.Entity<Variable>().HasKey(x => x.Name);
            modelBuilder.Entity<Board>()
                .Property(x => x.BoardID)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);
        }
    }
}
