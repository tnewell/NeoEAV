namespace EAVEntities.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class EAV_Test3 : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Entities",
                c => new
                    {
                        Entity_ID = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, maxLength: 512),
                    })
                .PrimaryKey(t => t.Entity_ID);
            
            CreateTable(
                "dbo.Subjects",
                c => new
                    {
                        Subject_ID = c.Int(nullable: false, identity: true),
                        Member_ID = c.String(nullable: false, maxLength: 128),
                        Project_ProjectID = c.Int(nullable: false),
                        Entity_EntityID = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Subject_ID)
                .ForeignKey("dbo.Projects", t => t.Project_ProjectID, cascadeDelete: true)
                .ForeignKey("dbo.Entities", t => t.Entity_EntityID, cascadeDelete: true)
                .Index(t => t.Project_ProjectID)
                .Index(t => t.Entity_EntityID);
            
        }
        
        public override void Down()
        {
            DropIndex("dbo.Subjects", new[] { "Entity_EntityID" });
            DropIndex("dbo.Subjects", new[] { "Project_ProjectID" });
            DropForeignKey("dbo.Subjects", "Entity_EntityID", "dbo.Entities");
            DropForeignKey("dbo.Subjects", "Project_ProjectID", "dbo.Projects");
            DropTable("dbo.Subjects");
            DropTable("dbo.Entities");
        }
    }
}
