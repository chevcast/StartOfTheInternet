namespace Terminal.Core.Data.Entities.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class InitialCreate : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Aliases",
                c => new
                    {
                        Username = c.String(nullable: false, maxLength: 128),
                        Shortcut = c.String(nullable: false, maxLength: 128),
                        Command = c.String(),
                    })
                .PrimaryKey(t => new { t.Username, t.Shortcut })
                .ForeignKey("dbo.Users", t => t.Username, cascadeDelete: true)
                .Index(t => t.Username);
            
            CreateTable(
                "dbo.Users",
                c => new
                    {
                        Username = c.String(nullable: false, maxLength: 128),
                        Password = c.String(),
                        IPAddress = c.String(),
                        JoinDate = c.DateTime(nullable: false),
                        LastLogin = c.DateTime(nullable: false),
                        Email = c.String(),
                        NotifyReplies = c.Boolean(nullable: false),
                        NotifyMessages = c.Boolean(nullable: false),
                        AutoFollow = c.Boolean(nullable: false),
                        Gender = c.String(),
                        FirstName = c.String(),
                        LastName = c.String(),
                        Location = c.String(),
                        Bio = c.String(),
                        Credits = c.Long(nullable: false),
                        TimeZone = c.String(),
                        Sound = c.Boolean(nullable: false),
                        ChatOpen = c.Boolean(nullable: false),
                        ShowTimestamps = c.Boolean(nullable: false),
                        BanInfo_Username = c.String(maxLength: 128),
                    })
                .PrimaryKey(t => t.Username)
                .ForeignKey("dbo.Bans", t => t.BanInfo_Username)
                .Index(t => t.BanInfo_Username);
            
            CreateTable(
                "dbo.Bans",
                c => new
                    {
                        Username = c.String(nullable: false, maxLength: 128),
                        Creator = c.String(),
                        StartDate = c.DateTime(nullable: false),
                        EndDate = c.DateTime(nullable: false),
                        Reason = c.String(),
                        User_Username = c.String(maxLength: 128),
                        BanCreator_Username = c.String(maxLength: 128),
                        User_Username1 = c.String(maxLength: 128),
                    })
                .PrimaryKey(t => t.Username)
                .ForeignKey("dbo.Users", t => t.User_Username)
                .ForeignKey("dbo.Users", t => t.BanCreator_Username)
                .ForeignKey("dbo.Users", t => t.User_Username1)
                .Index(t => t.User_Username)
                .Index(t => t.BanCreator_Username)
                .Index(t => t.User_Username1);
            
            CreateTable(
                "dbo.Ignores",
                c => new
                    {
                        IgnoreID = c.Long(nullable: false, identity: true),
                        InitiatingUser = c.String(),
                        IgnoredUser = c.String(),
                        User_Username = c.String(maxLength: 128),
                        Ignores_Username = c.String(maxLength: 128),
                        User_Username1 = c.String(maxLength: 128),
                        User_Username2 = c.String(maxLength: 128),
                    })
                .PrimaryKey(t => t.IgnoreID)
                .ForeignKey("dbo.Users", t => t.User_Username)
                .ForeignKey("dbo.Users", t => t.Ignores_Username)
                .ForeignKey("dbo.Users", t => t.User_Username1)
                .ForeignKey("dbo.Users", t => t.User_Username2)
                .Index(t => t.User_Username)
                .Index(t => t.Ignores_Username)
                .Index(t => t.User_Username1)
                .Index(t => t.User_Username2);
            
            CreateTable(
                "dbo.LinkClicks",
                c => new
                    {
                        LinkID = c.Long(nullable: false),
                        Username = c.String(nullable: false, maxLength: 128),
                        Count = c.Long(nullable: false),
                    })
                .PrimaryKey(t => new { t.LinkID, t.Username })
                .ForeignKey("dbo.Links", t => t.LinkID, cascadeDelete: true)
                .ForeignKey("dbo.Users", t => t.Username, cascadeDelete: true)
                .Index(t => t.LinkID)
                .Index(t => t.Username);
            
            CreateTable(
                "dbo.Links",
                c => new
                    {
                        LinkID = c.Long(nullable: false, identity: true),
                        Username = c.String(maxLength: 128),
                        Date = c.DateTime(nullable: false),
                        URL = c.String(),
                        Description = c.String(),
                        Title = c.String(),
                    })
                .PrimaryKey(t => t.LinkID)
                .ForeignKey("dbo.Users", t => t.Username)
                .Index(t => t.Username);
            
            CreateTable(
                "dbo.LinkComments",
                c => new
                    {
                        CommentID = c.Int(nullable: false, identity: true),
                        LinkID = c.Long(nullable: false),
                        Username = c.String(maxLength: 128),
                        Date = c.DateTime(nullable: false),
                        Body = c.String(),
                    })
                .PrimaryKey(t => t.CommentID)
                .ForeignKey("dbo.Links", t => t.LinkID, cascadeDelete: true)
                .ForeignKey("dbo.Users", t => t.Username)
                .Index(t => t.LinkID)
                .Index(t => t.Username);
            
            CreateTable(
                "dbo.LinkVotes",
                c => new
                    {
                        LinkID = c.Long(nullable: false),
                        Username = c.String(nullable: false, maxLength: 128),
                        Rating = c.Short(nullable: false),
                    })
                .PrimaryKey(t => new { t.LinkID, t.Username })
                .ForeignKey("dbo.Links", t => t.LinkID, cascadeDelete: true)
                .ForeignKey("dbo.Users", t => t.Username, cascadeDelete: true)
                .Index(t => t.LinkID)
                .Index(t => t.Username);
            
            CreateTable(
                "dbo.Tags",
                c => new
                    {
                        Name = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => t.Name);
            
            CreateTable(
                "dbo.Messages",
                c => new
                    {
                        MessageID = c.Long(nullable: false, identity: true),
                        Sender = c.String(),
                        Recipient = c.String(),
                        MessageRead = c.Boolean(nullable: false),
                        SenderDeleted = c.Boolean(nullable: false),
                        RecipientDeleted = c.Boolean(nullable: false),
                        SentDate = c.DateTime(nullable: false),
                        Subject = c.String(),
                        Body = c.String(),
                        RecipientLocked = c.Boolean(nullable: false),
                        SenderLocked = c.Boolean(nullable: false),
                        FromUser_Username = c.String(maxLength: 128),
                        ToUser_Username = c.String(maxLength: 128),
                        User_Username = c.String(maxLength: 128),
                        User_Username1 = c.String(maxLength: 128),
                    })
                .PrimaryKey(t => t.MessageID)
                .ForeignKey("dbo.Users", t => t.FromUser_Username)
                .ForeignKey("dbo.Users", t => t.ToUser_Username)
                .ForeignKey("dbo.Users", t => t.User_Username)
                .ForeignKey("dbo.Users", t => t.User_Username1)
                .Index(t => t.FromUser_Username)
                .Index(t => t.ToUser_Username)
                .Index(t => t.User_Username)
                .Index(t => t.User_Username1);
            
            CreateTable(
                "dbo.Replies",
                c => new
                    {
                        ReplyID = c.Long(nullable: false, identity: true),
                        TopicID = c.Long(nullable: false),
                        Username = c.String(maxLength: 128),
                        PostedDate = c.DateTime(nullable: false),
                        Body = c.String(),
                        ModsOnly = c.Boolean(nullable: false),
                        LastEdit = c.DateTime(nullable: false),
                        EditedBy = c.String(),
                    })
                .PrimaryKey(t => t.ReplyID)
                .ForeignKey("dbo.Topics", t => t.TopicID, cascadeDelete: true)
                .ForeignKey("dbo.Users", t => t.Username)
                .Index(t => t.TopicID)
                .Index(t => t.Username);
            
            CreateTable(
                "dbo.Topics",
                c => new
                    {
                        TopicID = c.Long(nullable: false, identity: true),
                        BoardID = c.Short(nullable: false),
                        Username = c.String(maxLength: 128),
                        PostedDate = c.DateTime(nullable: false),
                        Title = c.String(),
                        Body = c.String(),
                        ModsOnly = c.Boolean(nullable: false),
                        Locked = c.Boolean(nullable: false),
                        Stickied = c.Boolean(nullable: false),
                        GlobalSticky = c.Boolean(nullable: false),
                        LastEdit = c.DateTime(nullable: false),
                        EditedBy = c.String(),
                    })
                .PrimaryKey(t => t.TopicID)
                .ForeignKey("dbo.Boards", t => t.BoardID, cascadeDelete: true)
                .ForeignKey("dbo.Users", t => t.Username)
                .Index(t => t.BoardID)
                .Index(t => t.Username);
            
            CreateTable(
                "dbo.Boards",
                c => new
                    {
                        BoardID = c.Short(nullable: false),
                        Name = c.String(),
                        Description = c.String(),
                        Hidden = c.Boolean(nullable: false),
                        ModsOnly = c.Boolean(nullable: false),
                        Locked = c.Boolean(nullable: false),
                        Anonymous = c.Boolean(nullable: false),
                        AllTopics = c.Boolean(nullable: false),
                        MembersOnly = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.BoardID);
            
            CreateTable(
                "dbo.TopicFollows",
                c => new
                    {
                        Username = c.String(nullable: false, maxLength: 128),
                        TopicID = c.Long(nullable: false),
                        Saved = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => new { t.Username, t.TopicID })
                .ForeignKey("dbo.Topics", t => t.TopicID, cascadeDelete: true)
                .ForeignKey("dbo.Users", t => t.Username, cascadeDelete: true)
                .Index(t => t.TopicID)
                .Index(t => t.Username);
            
            CreateTable(
                "dbo.TopicVisits",
                c => new
                    {
                        Username = c.String(nullable: false, maxLength: 128),
                        TopicID = c.Long(nullable: false),
                        Count = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.Username, t.TopicID })
                .ForeignKey("dbo.Topics", t => t.TopicID, cascadeDelete: true)
                .ForeignKey("dbo.Users", t => t.Username, cascadeDelete: true)
                .Index(t => t.TopicID)
                .Index(t => t.Username);
            
            CreateTable(
                "dbo.UserActivityLogItems",
                c => new
                    {
                        ID = c.Long(nullable: false, identity: true),
                        Username = c.String(maxLength: 128),
                        Date = c.DateTime(nullable: false),
                        Information = c.String(),
                        Type = c.String(),
                    })
                .PrimaryKey(t => t.ID)
                .ForeignKey("dbo.Users", t => t.Username)
                .Index(t => t.Username);
            
            CreateTable(
                "dbo.Roles",
                c => new
                    {
                        Name = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => t.Name);
            
            CreateTable(
                "dbo.ChannelStatus",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ConnectionId = c.Guid(nullable: false),
                        ChannelName = c.String(),
                        User_Username = c.String(maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Users", t => t.User_Username)
                .Index(t => t.User_Username);
            
            CreateTable(
                "dbo.InviteCodes",
                c => new
                    {
                        Code = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => t.Code);
            
            CreateTable(
                "dbo.LUEsers",
                c => new
                    {
                        Username = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => t.Username);
            
            CreateTable(
                "dbo.MadlibTemplates",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        Title = c.String(),
                        Template = c.String(),
                    })
                .PrimaryKey(t => t.ID);
            
            CreateTable(
                "dbo.Variables",
                c => new
                    {
                        Name = c.String(nullable: false, maxLength: 128),
                        Value = c.String(),
                    })
                .PrimaryKey(t => t.Name);
            
            CreateTable(
                "dbo.TagLinks",
                c => new
                    {
                        Tag_Name = c.String(nullable: false, maxLength: 128),
                        Link_LinkID = c.Long(nullable: false),
                    })
                .PrimaryKey(t => new { t.Tag_Name, t.Link_LinkID })
                .ForeignKey("dbo.Tags", t => t.Tag_Name, cascadeDelete: true)
                .ForeignKey("dbo.Links", t => t.Link_LinkID, cascadeDelete: true)
                .Index(t => t.Tag_Name)
                .Index(t => t.Link_LinkID);
            
            CreateTable(
                "dbo.RoleUsers",
                c => new
                    {
                        Role_Name = c.String(nullable: false, maxLength: 128),
                        User_Username = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => new { t.Role_Name, t.User_Username })
                .ForeignKey("dbo.Roles", t => t.Role_Name, cascadeDelete: true)
                .ForeignKey("dbo.Users", t => t.User_Username, cascadeDelete: true)
                .Index(t => t.Role_Name)
                .Index(t => t.User_Username);
            
        }
        
        public override void Down()
        {
            DropIndex("dbo.RoleUsers", new[] { "User_Username" });
            DropIndex("dbo.RoleUsers", new[] { "Role_Name" });
            DropIndex("dbo.TagLinks", new[] { "Link_LinkID" });
            DropIndex("dbo.TagLinks", new[] { "Tag_Name" });
            DropIndex("dbo.ChannelStatus", new[] { "User_Username" });
            DropIndex("dbo.UserActivityLogItems", new[] { "Username" });
            DropIndex("dbo.TopicVisits", new[] { "Username" });
            DropIndex("dbo.TopicVisits", new[] { "TopicID" });
            DropIndex("dbo.TopicFollows", new[] { "Username" });
            DropIndex("dbo.TopicFollows", new[] { "TopicID" });
            DropIndex("dbo.Topics", new[] { "Username" });
            DropIndex("dbo.Topics", new[] { "BoardID" });
            DropIndex("dbo.Replies", new[] { "Username" });
            DropIndex("dbo.Replies", new[] { "TopicID" });
            DropIndex("dbo.Messages", new[] { "User_Username1" });
            DropIndex("dbo.Messages", new[] { "User_Username" });
            DropIndex("dbo.Messages", new[] { "ToUser_Username" });
            DropIndex("dbo.Messages", new[] { "FromUser_Username" });
            DropIndex("dbo.LinkVotes", new[] { "Username" });
            DropIndex("dbo.LinkVotes", new[] { "LinkID" });
            DropIndex("dbo.LinkComments", new[] { "Username" });
            DropIndex("dbo.LinkComments", new[] { "LinkID" });
            DropIndex("dbo.Links", new[] { "Username" });
            DropIndex("dbo.LinkClicks", new[] { "Username" });
            DropIndex("dbo.LinkClicks", new[] { "LinkID" });
            DropIndex("dbo.Ignores", new[] { "User_Username2" });
            DropIndex("dbo.Ignores", new[] { "User_Username1" });
            DropIndex("dbo.Ignores", new[] { "Ignores_Username" });
            DropIndex("dbo.Ignores", new[] { "User_Username" });
            DropIndex("dbo.Bans", new[] { "User_Username1" });
            DropIndex("dbo.Bans", new[] { "BanCreator_Username" });
            DropIndex("dbo.Bans", new[] { "User_Username" });
            DropIndex("dbo.Users", new[] { "BanInfo_Username" });
            DropIndex("dbo.Aliases", new[] { "Username" });
            DropForeignKey("dbo.RoleUsers", "User_Username", "dbo.Users");
            DropForeignKey("dbo.RoleUsers", "Role_Name", "dbo.Roles");
            DropForeignKey("dbo.TagLinks", "Link_LinkID", "dbo.Links");
            DropForeignKey("dbo.TagLinks", "Tag_Name", "dbo.Tags");
            DropForeignKey("dbo.ChannelStatus", "User_Username", "dbo.Users");
            DropForeignKey("dbo.UserActivityLogItems", "Username", "dbo.Users");
            DropForeignKey("dbo.TopicVisits", "Username", "dbo.Users");
            DropForeignKey("dbo.TopicVisits", "TopicID", "dbo.Topics");
            DropForeignKey("dbo.TopicFollows", "Username", "dbo.Users");
            DropForeignKey("dbo.TopicFollows", "TopicID", "dbo.Topics");
            DropForeignKey("dbo.Topics", "Username", "dbo.Users");
            DropForeignKey("dbo.Topics", "BoardID", "dbo.Boards");
            DropForeignKey("dbo.Replies", "Username", "dbo.Users");
            DropForeignKey("dbo.Replies", "TopicID", "dbo.Topics");
            DropForeignKey("dbo.Messages", "User_Username1", "dbo.Users");
            DropForeignKey("dbo.Messages", "User_Username", "dbo.Users");
            DropForeignKey("dbo.Messages", "ToUser_Username", "dbo.Users");
            DropForeignKey("dbo.Messages", "FromUser_Username", "dbo.Users");
            DropForeignKey("dbo.LinkVotes", "Username", "dbo.Users");
            DropForeignKey("dbo.LinkVotes", "LinkID", "dbo.Links");
            DropForeignKey("dbo.LinkComments", "Username", "dbo.Users");
            DropForeignKey("dbo.LinkComments", "LinkID", "dbo.Links");
            DropForeignKey("dbo.Links", "Username", "dbo.Users");
            DropForeignKey("dbo.LinkClicks", "Username", "dbo.Users");
            DropForeignKey("dbo.LinkClicks", "LinkID", "dbo.Links");
            DropForeignKey("dbo.Ignores", "User_Username2", "dbo.Users");
            DropForeignKey("dbo.Ignores", "User_Username1", "dbo.Users");
            DropForeignKey("dbo.Ignores", "Ignores_Username", "dbo.Users");
            DropForeignKey("dbo.Ignores", "User_Username", "dbo.Users");
            DropForeignKey("dbo.Bans", "User_Username1", "dbo.Users");
            DropForeignKey("dbo.Bans", "BanCreator_Username", "dbo.Users");
            DropForeignKey("dbo.Bans", "User_Username", "dbo.Users");
            DropForeignKey("dbo.Users", "BanInfo_Username", "dbo.Bans");
            DropForeignKey("dbo.Aliases", "Username", "dbo.Users");
            DropTable("dbo.RoleUsers");
            DropTable("dbo.TagLinks");
            DropTable("dbo.Variables");
            DropTable("dbo.MadlibTemplates");
            DropTable("dbo.LUEsers");
            DropTable("dbo.InviteCodes");
            DropTable("dbo.ChannelStatus");
            DropTable("dbo.Roles");
            DropTable("dbo.UserActivityLogItems");
            DropTable("dbo.TopicVisits");
            DropTable("dbo.TopicFollows");
            DropTable("dbo.Boards");
            DropTable("dbo.Topics");
            DropTable("dbo.Replies");
            DropTable("dbo.Messages");
            DropTable("dbo.Tags");
            DropTable("dbo.LinkVotes");
            DropTable("dbo.LinkComments");
            DropTable("dbo.Links");
            DropTable("dbo.LinkClicks");
            DropTable("dbo.Ignores");
            DropTable("dbo.Bans");
            DropTable("dbo.Users");
            DropTable("dbo.Aliases");
        }
    }
}
