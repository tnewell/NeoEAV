namespace EAVEntities.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class EAV_Test25 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Projects", "Data_Name", c => c.String(nullable: false, maxLength: 64));
            AlterColumn("dbo.Containers", "Data_Name", c => c.String(nullable: false, maxLength: 64));
            AlterColumn("dbo.Attributes", "Data_Name", c => c.String(nullable: false, maxLength: 64));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Attributes", "Data_Name", c => c.String(nullable: false, maxLength: 128));
            AlterColumn("dbo.Containers", "Data_Name", c => c.String(nullable: false, maxLength: 128));
            DropColumn("dbo.Projects", "Data_Name");
        }
    }
}
