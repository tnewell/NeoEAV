namespace EAVEntities.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class EAV_Test13 : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.UnitAttributes",
                c => new
                    {
                        Unit_UnitID = c.Int(nullable: false),
                        Attribute_AttributeID = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.Unit_UnitID, t.Attribute_AttributeID })
                .ForeignKey("dbo.Units", t => t.Unit_UnitID, cascadeDelete: true)
                .ForeignKey("dbo.Attributes", t => t.Attribute_AttributeID, cascadeDelete: true)
                .Index(t => t.Unit_UnitID)
                .Index(t => t.Attribute_AttributeID);
            
        }
        
        public override void Down()
        {
            DropIndex("dbo.UnitAttributes", new[] { "Attribute_AttributeID" });
            DropIndex("dbo.UnitAttributes", new[] { "Unit_UnitID" });
            DropForeignKey("dbo.UnitAttributes", "Attribute_AttributeID", "dbo.Attributes");
            DropForeignKey("dbo.UnitAttributes", "Unit_UnitID", "dbo.Units");
            DropTable("dbo.UnitAttributes");
        }
    }
}
