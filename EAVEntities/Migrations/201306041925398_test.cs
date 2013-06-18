namespace EAVEntities.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class test : DbMigration
    {
        public override void Up()
        {
            //RenameColumn(table: "dbo.Containers", name: "DisplayName", newName: "Display_Name");
            //AddColumn("dbo.Containers", "Data_Name", c => c.String(nullable: false, maxLength: 128));
            //AddColumn("dbo.Attributes", "Data_Name", c => c.String(nullable: false, maxLength: 128));
            //DropColumn("dbo.Containers", "Has_Fixed_Instances");
        }
        
        public override void Down()
        {
            //AddColumn("dbo.Containers", "Has_Fixed_Instances", c => c.Boolean(nullable: false));
            //DropColumn("dbo.Attributes", "Data_Name");
            //DropColumn("dbo.Containers", "Data_Name");
            //RenameColumn(table: "dbo.Containers", name: "Display_Name", newName: "DisplayName");
        }
    }
}
