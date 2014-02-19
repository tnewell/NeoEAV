namespace EAVEntities.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class test2 : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Value2",
                c => new
                    {
                        Container_Instance_ID = c.Int(nullable: false),
                        Attribute_ID = c.Int(nullable: false),
                        Raw_Value = c.String(nullable: false),
                        Units = c.String(maxLength: 8),
                        Boolean_Value = c.Boolean(),
                        DateTime_Value = c.DateTime(),
                        Float_Value = c.Single(),
                        Integer_Value = c.Int(),
                    })
                .PrimaryKey(t => new { t.Container_Instance_ID, t.Attribute_ID })
                .ForeignKey("dbo.ContainerInstances", t => t.Container_Instance_ID)
                .ForeignKey("dbo.Attributes", t => t.Attribute_ID)
                .Index(t => t.Container_Instance_ID)
                .Index(t => t.Attribute_ID);
            
        }
        
        public override void Down()
        {
            DropIndex("dbo.Value2", new[] { "Attribute_ID" });
            DropIndex("dbo.Value2", new[] { "Container_Instance_ID" });
            DropForeignKey("dbo.Value2", "Attribute_ID", "dbo.Attributes");
            DropForeignKey("dbo.Value2", "Container_Instance_ID", "dbo.ContainerInstances");
            DropTable("dbo.Value2");
        }
    }
}
