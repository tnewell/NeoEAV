namespace EAVEntities.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class EAV_Test10 : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.Containers", "Project_ID", "dbo.Projects");
            DropForeignKey("dbo.Attributes", "Container_ID", "dbo.Containers");
            DropForeignKey("dbo.Attributes", "Term_ID", "dbo.Terms");
            DropForeignKey("dbo.Values", "Container_Instance_ID", "dbo.ContainerInstances");
            DropForeignKey("dbo.Values", "Attribute_ID", "dbo.Attributes");
            //DropForeignKey("dbo.Subjects", "Project_ID", "dbo.Projects");
            //DropForeignKey("dbo.Subjects", "Entity_ID", "dbo.Entities");
            DropIndex("dbo.Containers", new[] { "Project_ID" });
            DropIndex("dbo.Attributes", new[] { "Container_ID" });
            DropIndex("dbo.Attributes", new[] { "Term_ID" });
            DropIndex("dbo.Values", new[] { "Container_Instance_ID" });
            DropIndex("dbo.Values", new[] { "Attribute_ID" });
            //DropIndex("dbo.Subjects", new[] { "Project_ID" });
            //DropIndex("dbo.Subjects", new[] { "Entity_ID" });
            AddForeignKey("dbo.Containers", "Project_ID", "dbo.Projects", "Project_ID");
            AddForeignKey("dbo.Attributes", "Container_ID", "dbo.Containers", "Container_ID");
            AddForeignKey("dbo.Attributes", "Term_ID", "dbo.Terms", "Term_ID");
            AddForeignKey("dbo.Values", "Container_Instance_ID", "dbo.ContainerInstances", "Container_Instance_ID");
            AddForeignKey("dbo.Values", "Attribute_ID", "dbo.Attributes", "Attribute_ID");
            AddForeignKey("dbo.Subjects", "Project_ID", "dbo.Projects", "Project_ID");
            AddForeignKey("dbo.Subjects", "Entity_ID", "dbo.Entities", "Entity_ID");
            CreateIndex("dbo.Containers", "Project_ID");
            CreateIndex("dbo.Attributes", "Container_ID");
            CreateIndex("dbo.Attributes", "Term_ID");
            CreateIndex("dbo.Values", "Container_Instance_ID");
            CreateIndex("dbo.Values", "Attribute_ID");
            CreateIndex("dbo.Subjects", "Project_ID");
            CreateIndex("dbo.Subjects", "Entity_ID");
        }
        
        public override void Down()
        {
            DropIndex("dbo.Subjects", new[] { "Entity_ID" });
            DropIndex("dbo.Subjects", new[] { "Project_ID" });
            DropIndex("dbo.Values", new[] { "Attribute_ID" });
            DropIndex("dbo.Values", new[] { "Container_Instance_ID" });
            DropIndex("dbo.Attributes", new[] { "Term_ID" });
            DropIndex("dbo.Attributes", new[] { "Container_ID" });
            DropIndex("dbo.Containers", new[] { "Project_ID" });
            DropForeignKey("dbo.Subjects", "Entity_ID", "dbo.Entities");
            DropForeignKey("dbo.Subjects", "Project_ID", "dbo.Projects");
            DropForeignKey("dbo.Values", "Attribute_ID", "dbo.Attributes");
            DropForeignKey("dbo.Values", "Container_Instance_ID", "dbo.ContainerInstances");
            DropForeignKey("dbo.Attributes", "Term_ID", "dbo.Terms");
            DropForeignKey("dbo.Attributes", "Container_ID", "dbo.Containers");
            DropForeignKey("dbo.Containers", "Project_ID", "dbo.Projects");
            CreateIndex("dbo.Subjects", "Entity_ID");
            CreateIndex("dbo.Subjects", "Project_ID");
            CreateIndex("dbo.Values", "Attribute_ID");
            CreateIndex("dbo.Values", "Container_Instance_ID");
            CreateIndex("dbo.Attributes", "Term_ID");
            CreateIndex("dbo.Attributes", "Container_ID");
            CreateIndex("dbo.Containers", "Project_ID");
            AddForeignKey("dbo.Subjects", "Entity_ID", "dbo.Entities", "Entity_ID", cascadeDelete: true);
            AddForeignKey("dbo.Subjects", "Project_ID", "dbo.Projects", "Project_ID", cascadeDelete: true);
            AddForeignKey("dbo.Values", "Attribute_ID", "dbo.Attributes", "Attribute_ID", cascadeDelete: true);
            AddForeignKey("dbo.Values", "Container_Instance_ID", "dbo.ContainerInstances", "Container_Instance_ID", cascadeDelete: true);
            AddForeignKey("dbo.Attributes", "Term_ID", "dbo.Terms", "Term_ID", cascadeDelete: true);
            AddForeignKey("dbo.Attributes", "Container_ID", "dbo.Containers", "Container_ID", cascadeDelete: true);
            AddForeignKey("dbo.Containers", "Project_ID", "dbo.Projects", "Project_ID", cascadeDelete: true);
        }
    }
}
