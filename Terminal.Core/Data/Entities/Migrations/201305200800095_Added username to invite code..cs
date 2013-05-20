namespace Terminal.Core.Data.Entities.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Addedusernametoinvitecode : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.InviteCodes", "Username", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.InviteCodes", "Username");
        }
    }
}
