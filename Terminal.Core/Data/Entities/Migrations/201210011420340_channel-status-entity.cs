namespace Terminal.Core.Data.Entities.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class channelstatusentity : DbMigration
    {
        public override void Up()
        {
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
            
        }
        
        public override void Down()
        {
            DropIndex("dbo.ChannelStatus", new[] { "User_Username" });
            DropForeignKey("dbo.ChannelStatus", "User_Username", "dbo.Users");
            DropTable("dbo.ChannelStatus");
        }
    }
}
