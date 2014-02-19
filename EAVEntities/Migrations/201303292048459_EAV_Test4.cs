namespace EAVEntities.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class EAV_Test4 : DbMigration
    {
        public override void Up()
        {
            RenameColumn(table: "dbo.Subjects", name: "Project_ProjectID", newName: "Project_ID");
            RenameColumn(table: "dbo.Subjects", name: "Entity_EntityID", newName: "Entity_ID");
        }
        
        public override void Down()
        {
            RenameColumn(table: "dbo.Subjects", name: "Entity_ID", newName: "Entity_EntityID");
            RenameColumn(table: "dbo.Subjects", name: "Project_ID", newName: "Project_ProjectID");
        }
    }
}
