namespace Terminal.Domain.Migrations
{
    using System.Data.Entity.Migrations;
    
    public partial class InitialCreate : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "Aliases",
                c => new
                    {
                        Username = c.String(nullable: false, maxLength: 128),
                        Shortcut = c.String(nullable: false, maxLength: 128),
                        Command = c.String(),
                    })
                .PrimaryKey(t => new { t.Username, t.Shortcut })
                .ForeignKey("Users", t => t.Username, cascadeDelete: true)
                .Index(t => t.Username);
            
            CreateTable(
                "Users",
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
                .ForeignKey("Bans", t => t.BanInfo_Username)
                .Index(t => t.BanInfo_Username);
            
            CreateTable(
                "Bans",
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
                .ForeignKey("Users", t => t.User_Username)
                .ForeignKey("Users", t => t.BanCreator_Username)
                .ForeignKey("Users", t => t.User_Username1)
                .Index(t => t.User_Username)
                .Index(t => t.BanCreator_Username)
                .Index(t => t.User_Username1);
            
            CreateTable(
                "Ignores",
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
                .ForeignKey("Users", t => t.User_Username)
                .ForeignKey("Users", t => t.Ignores_Username)
                .ForeignKey("Users", t => t.User_Username1)
                .ForeignKey("Users", t => t.User_Username2)
                .Index(t => t.User_Username)
                .Index(t => t.Ignores_Username)
                .Index(t => t.User_Username1)
                .Index(t => t.User_Username2);
            
            CreateTable(
                "InviteCodes",
                c => new
                    {
                        Code = c.String(nullable: false, maxLength: 128),
                        Username = c.String(maxLength: 128),
                    })
                .PrimaryKey(t => t.Code)
                .ForeignKey("Users", t => t.Username)
                .Index(t => t.Username);
            
            CreateTable(
                "LinkClicks",
                c => new
                    {
                        LinkID = c.Long(nullable: false),
                        Username = c.String(nullable: false, maxLength: 128),
                        Count = c.Long(nullable: false),
                    })
                .PrimaryKey(t => new { t.LinkID, t.Username })
                .ForeignKey("Links", t => t.LinkID, cascadeDelete: true)
                .ForeignKey("Users", t => t.Username, cascadeDelete: true)
                .Index(t => t.LinkID)
                .Index(t => t.Username);
            
            CreateTable(
                "Links",
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
                .ForeignKey("Users", t => t.Username)
                .Index(t => t.Username);
            
            CreateTable(
                "LinkComments",
                c => new
                    {
                        CommentID = c.Int(nullable: false, identity: true),
                        LinkID = c.Long(nullable: false),
                        Username = c.String(maxLength: 128),
                        Date = c.DateTime(nullable: false),
                        Body = c.String(),
                    })
                .PrimaryKey(t => t.CommentID)
                .ForeignKey("Links", t => t.LinkID, cascadeDelete: true)
                .ForeignKey("Users", t => t.Username)
                .Index(t => t.LinkID)
                .Index(t => t.Username);
            
            CreateTable(
                "LinkVotes",
                c => new
                    {
                        LinkID = c.Long(nullable: false),
                        Username = c.String(nullable: false, maxLength: 128),
                        Rating = c.Short(nullable: false),
                    })
                .PrimaryKey(t => new { t.LinkID, t.Username })
                .ForeignKey("Links", t => t.LinkID, cascadeDelete: true)
                .ForeignKey("Users", t => t.Username, cascadeDelete: true)
                .Index(t => t.LinkID)
                .Index(t => t.Username);
            
            CreateTable(
                "Tags",
                c => new
                    {
                        Name = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => t.Name);
            
            CreateTable(
                "Messages",
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
                .ForeignKey("Users", t => t.FromUser_Username)
                .ForeignKey("Users", t => t.ToUser_Username)
                .ForeignKey("Users", t => t.User_Username)
                .ForeignKey("Users", t => t.User_Username1)
                .Index(t => t.FromUser_Username)
                .Index(t => t.ToUser_Username)
                .Index(t => t.User_Username)
                .Index(t => t.User_Username1);
            
            CreateTable(
                "Replies",
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
                .ForeignKey("Topics", t => t.TopicID, cascadeDelete: true)
                .ForeignKey("Users", t => t.Username)
                .Index(t => t.TopicID)
                .Index(t => t.Username);
            
            CreateTable(
                "Topics",
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
                .ForeignKey("Boards", t => t.BoardID, cascadeDelete: true)
                .ForeignKey("Users", t => t.Username)
                .Index(t => t.BoardID)
                .Index(t => t.Username);
            
            CreateTable(
                "Boards",
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
                    })
                .PrimaryKey(t => t.BoardID);
            
            CreateTable(
                "TopicFollows",
                c => new
                    {
                        Username = c.String(nullable: false, maxLength: 128),
                        TopicID = c.Long(nullable: false),
                        Saved = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => new { t.Username, t.TopicID })
                .ForeignKey("Topics", t => t.TopicID, cascadeDelete: true)
                .ForeignKey("Users", t => t.Username, cascadeDelete: true)
                .Index(t => t.TopicID)
                .Index(t => t.Username);
            
            CreateTable(
                "TopicVisits",
                c => new
                    {
                        Username = c.String(nullable: false, maxLength: 128),
                        TopicID = c.Long(nullable: false),
                        Count = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.Username, t.TopicID })
                .ForeignKey("Topics", t => t.TopicID, cascadeDelete: true)
                .ForeignKey("Users", t => t.Username, cascadeDelete: true)
                .Index(t => t.TopicID)
                .Index(t => t.Username);
            
            CreateTable(
                "UserActivityLogItems",
                c => new
                    {
                        ID = c.Long(nullable: false, identity: true),
                        Username = c.String(maxLength: 128),
                        Date = c.DateTime(nullable: false),
                        Information = c.String(),
                        Type = c.String(),
                    })
                .PrimaryKey(t => t.ID)
                .ForeignKey("Users", t => t.Username)
                .Index(t => t.Username);
            
            CreateTable(
                "Roles",
                c => new
                    {
                        Name = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => t.Name);
            
            CreateTable(
                "LUEsers",
                c => new
                    {
                        Username = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => t.Username);
            
            CreateTable(
                "MadlibTemplates",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        Title = c.String(),
                        Template = c.String(),
                    })
                .PrimaryKey(t => t.ID);
            
            CreateTable(
                "Variables",
                c => new
                    {
                        Name = c.String(nullable: false, maxLength: 128),
                        Value = c.String(),
                    })
                .PrimaryKey(t => t.Name);
            
            CreateTable(
                "TagLinks",
                c => new
                    {
                        Tag_Name = c.String(nullable: false, maxLength: 128),
                        Link_LinkID = c.Long(nullable: false),
                    })
                .PrimaryKey(t => new { t.Tag_Name, t.Link_LinkID })
                .ForeignKey("Tags", t => t.Tag_Name, cascadeDelete: true)
                .ForeignKey("Links", t => t.Link_LinkID, cascadeDelete: true)
                .Index(t => t.Tag_Name)
                .Index(t => t.Link_LinkID);
            
            CreateTable(
                "RoleUsers",
                c => new
                    {
                        Role_Name = c.String(nullable: false, maxLength: 128),
                        User_Username = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => new { t.Role_Name, t.User_Username })
                .ForeignKey("Roles", t => t.Role_Name, cascadeDelete: true)
                .ForeignKey("Users", t => t.User_Username, cascadeDelete: true)
                .Index(t => t.Role_Name)
                .Index(t => t.User_Username);
            
        }
        
        public override void Down()
        {
            DropIndex("RoleUsers", new[] { "User_Username" });
            DropIndex("RoleUsers", new[] { "Role_Name" });
            DropIndex("TagLinks", new[] { "Link_LinkID" });
            DropIndex("TagLinks", new[] { "Tag_Name" });
            DropIndex("UserActivityLogItems", new[] { "Username" });
            DropIndex("TopicVisits", new[] { "Username" });
            DropIndex("TopicVisits", new[] { "TopicID" });
            DropIndex("TopicFollows", new[] { "Username" });
            DropIndex("TopicFollows", new[] { "TopicID" });
            DropIndex("Topics", new[] { "Username" });
            DropIndex("Topics", new[] { "BoardID" });
            DropIndex("Replies", new[] { "Username" });
            DropIndex("Replies", new[] { "TopicID" });
            DropIndex("Messages", new[] { "User_Username1" });
            DropIndex("Messages", new[] { "User_Username" });
            DropIndex("Messages", new[] { "ToUser_Username" });
            DropIndex("Messages", new[] { "FromUser_Username" });
            DropIndex("LinkVotes", new[] { "Username" });
            DropIndex("LinkVotes", new[] { "LinkID" });
            DropIndex("LinkComments", new[] { "Username" });
            DropIndex("LinkComments", new[] { "LinkID" });
            DropIndex("Links", new[] { "Username" });
            DropIndex("LinkClicks", new[] { "Username" });
            DropIndex("LinkClicks", new[] { "LinkID" });
            DropIndex("InviteCodes", new[] { "Username" });
            DropIndex("Ignores", new[] { "User_Username2" });
            DropIndex("Ignores", new[] { "User_Username1" });
            DropIndex("Ignores", new[] { "Ignores_Username" });
            DropIndex("Ignores", new[] { "User_Username" });
            DropIndex("Bans", new[] { "User_Username1" });
            DropIndex("Bans", new[] { "BanCreator_Username" });
            DropIndex("Bans", new[] { "User_Username" });
            DropIndex("Users", new[] { "BanInfo_Username" });
            DropIndex("Aliases", new[] { "Username" });
            DropForeignKey("RoleUsers", "User_Username", "Users");
            DropForeignKey("RoleUsers", "Role_Name", "Roles");
            DropForeignKey("TagLinks", "Link_LinkID", "Links");
            DropForeignKey("TagLinks", "Tag_Name", "Tags");
            DropForeignKey("UserActivityLogItems", "Username", "Users");
            DropForeignKey("TopicVisits", "Username", "Users");
            DropForeignKey("TopicVisits", "TopicID", "Topics");
            DropForeignKey("TopicFollows", "Username", "Users");
            DropForeignKey("TopicFollows", "TopicID", "Topics");
            DropForeignKey("Topics", "Username", "Users");
            DropForeignKey("Topics", "BoardID", "Boards");
            DropForeignKey("Replies", "Username", "Users");
            DropForeignKey("Replies", "TopicID", "Topics");
            DropForeignKey("Messages", "User_Username1", "Users");
            DropForeignKey("Messages", "User_Username", "Users");
            DropForeignKey("Messages", "ToUser_Username", "Users");
            DropForeignKey("Messages", "FromUser_Username", "Users");
            DropForeignKey("LinkVotes", "Username", "Users");
            DropForeignKey("LinkVotes", "LinkID", "Links");
            DropForeignKey("LinkComments", "Username", "Users");
            DropForeignKey("LinkComments", "LinkID", "Links");
            DropForeignKey("Links", "Username", "Users");
            DropForeignKey("LinkClicks", "Username", "Users");
            DropForeignKey("LinkClicks", "LinkID", "Links");
            DropForeignKey("InviteCodes", "Username", "Users");
            DropForeignKey("Ignores", "User_Username2", "Users");
            DropForeignKey("Ignores", "User_Username1", "Users");
            DropForeignKey("Ignores", "Ignores_Username", "Users");
            DropForeignKey("Ignores", "User_Username", "Users");
            DropForeignKey("Bans", "User_Username1", "Users");
            DropForeignKey("Bans", "BanCreator_Username", "Users");
            DropForeignKey("Bans", "User_Username", "Users");
            DropForeignKey("Users", "BanInfo_Username", "Bans");
            DropForeignKey("Aliases", "Username", "Users");
            DropTable("RoleUsers");
            DropTable("TagLinks");
            DropTable("Variables");
            DropTable("MadlibTemplates");
            DropTable("LUEsers");
            DropTable("Roles");
            DropTable("UserActivityLogItems");
            DropTable("TopicVisits");
            DropTable("TopicFollows");
            DropTable("Boards");
            DropTable("Topics");
            DropTable("Replies");
            DropTable("Messages");
            DropTable("Tags");
            DropTable("LinkVotes");
            DropTable("LinkComments");
            DropTable("Links");
            DropTable("LinkClicks");
            DropTable("InviteCodes");
            DropTable("Ignores");
            DropTable("Bans");
            DropTable("Users");
            DropTable("Aliases");
        }
    }
}
