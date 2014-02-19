namespace EAVEntities.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class EAV_Test1 : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Projects", "Name", c => c.String(nullable: false, maxLength: 512));
            AlterColumn("dbo.Containers", "Name", c => c.String(nullable: false, maxLength: 512));
            AlterColumn("dbo.Containers", "DisplayName", c => c.String(nullable: false, maxLength: 512));
            AlterColumn("dbo.Attributes", "Name", c => c.String(nullable: false, maxLength: 512));
            AlterColumn("dbo.Attributes", "Display_Name", c => c.String(nullable: false, maxLength: 512));
            AlterColumn("dbo.Terms", "Name", c => c.String(nullable: false, maxLength: 512));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Terms", "Name", c => c.String(nullable: false));
            AlterColumn("dbo.Attributes", "Display_Name", c => c.String(nullable: false));
            AlterColumn("dbo.Attributes", "Name", c => c.String(nullable: false));
            AlterColumn("dbo.Containers", "DisplayName", c => c.String(nullable: false));
            AlterColumn("dbo.Containers", "Name", c => c.String(nullable: false));
            AlterColumn("dbo.Projects", "Name", c => c.String(nullable: false));
        }
    }
}
