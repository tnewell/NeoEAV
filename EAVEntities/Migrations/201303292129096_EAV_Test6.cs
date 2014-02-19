namespace EAVEntities.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class EAV_Test6 : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Values",
                c => new
                    {
                        Container_Instance_ID = c.Int(nullable: false),
                        Attribute_ID = c.Int(nullable: false),
                        Raw_Value = c.String(nullable: false),
                    })
                .PrimaryKey(t => new { t.Container_Instance_ID, t.Attribute_ID })
                .ForeignKey("dbo.ContainerInstances", t => t.Container_Instance_ID, cascadeDelete: true)
                .ForeignKey("dbo.Attributes", t => t.Attribute_ID, cascadeDelete: true)
                .Index(t => t.Container_Instance_ID)
                .Index(t => t.Attribute_ID);
            
            CreateTable(
                "dbo.ContainerInstances",
                c => new
                    {
                        Container_Instance_ID = c.Int(nullable: false, identity: true),
                        Repeat_Instance = c.Int(nullable: false),
                        Container_ID = c.Int(nullable: false),
                        Subject_ID = c.Int(nullable: false),
                        Parent_Container_Instance_ID = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Container_Instance_ID)
                .ForeignKey("dbo.Containers", t => t.Container_ID)
                .ForeignKey("dbo.Subjects", t => t.Subject_ID)
                .ForeignKey("dbo.ContainerInstances", t => t.Parent_Container_Instance_ID)
                .Index(t => t.Container_ID)
                .Index(t => t.Subject_ID)
                .Index(t => t.Parent_Container_Instance_ID);
            
            CreateTable(
                "dbo.Units",
                c => new
                    {
                        Unit_ID = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, maxLength: 128),
                        Symbol = c.String(nullable: false, maxLength: 8),
                    })
                .PrimaryKey(t => t.Unit_ID);
            
        }
        
        public override void Down()
        {
            DropIndex("dbo.ContainerInstances", new[] { "Parent_Container_Instance_ID" });
            DropIndex("dbo.ContainerInstances", new[] { "Subject_ID" });
            DropIndex("dbo.ContainerInstances", new[] { "Container_ID" });
            DropIndex("dbo.Values", new[] { "Attribute_ID" });
            DropIndex("dbo.Values", new[] { "Container_Instance_ID" });
            DropForeignKey("dbo.ContainerInstances", "Parent_Container_Instance_ID", "dbo.ContainerInstances");
            DropForeignKey("dbo.ContainerInstances", "Subject_ID", "dbo.Subjects");
            DropForeignKey("dbo.ContainerInstances", "Container_ID", "dbo.Containers");
            DropForeignKey("dbo.Values", "Attribute_ID", "dbo.Attributes");
            DropForeignKey("dbo.Values", "Container_Instance_ID", "dbo.ContainerInstances");
            DropTable("dbo.Units");
            DropTable("dbo.ContainerInstances");
            DropTable("dbo.Values");
        }
    }
}
