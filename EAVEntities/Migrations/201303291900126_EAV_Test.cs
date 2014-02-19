namespace EAVEntities.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class EAV_Test : DbMigration
    {
        public override void Up()
        {
            RenameColumn(table: "dbo.Containers", name: "Has_Fixed_Instance", newName: "Has_Fixed_Instances");
        }
        
        public override void Down()
        {
            RenameColumn(table: "dbo.Containers", name: "Has_Fixed_Instances", newName: "Has_Fixed_Instance");
        }
    }
}
