namespace Terminal.Core.Data.Entities.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddMembersOnlypropertytoboard : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Boards", "MembersOnly", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Boards", "MembersOnly");
        }
    }
}
