namespace EAVEntities.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class EAV_Test14 : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.UnitAttributes", "Unit_UnitID", "dbo.Units");
            DropForeignKey("dbo.UnitAttributes", "Attribute_AttributeID", "dbo.Attributes");
            DropIndex("dbo.UnitAttributes", new[] { "Unit_UnitID" });
            DropIndex("dbo.UnitAttributes", new[] { "Attribute_AttributeID" });
            CreateTable(
                "dbo.AttributeUnits",
                c => new
                    {
                        Attribute_AttributeID = c.Int(nullable: false),
                        Unit_UnitID = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.Attribute_AttributeID, t.Unit_UnitID })
                .ForeignKey("dbo.Attributes", t => t.Attribute_AttributeID, cascadeDelete: true)
                .ForeignKey("dbo.Units", t => t.Unit_UnitID, cascadeDelete: true)
                .Index(t => t.Attribute_AttributeID)
                .Index(t => t.Unit_UnitID);
            
            DropTable("dbo.UnitAttributes");
        }
        
        public override void Down()
        {
            CreateTable(
                "dbo.UnitAttributes",
                c => new
                    {
                        Unit_UnitID = c.Int(nullable: false),
                        Attribute_AttributeID = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.Unit_UnitID, t.Attribute_AttributeID });
            
            DropIndex("dbo.AttributeUnits", new[] { "Unit_UnitID" });
            DropIndex("dbo.AttributeUnits", new[] { "Attribute_AttributeID" });
            DropForeignKey("dbo.AttributeUnits", "Unit_UnitID", "dbo.Units");
            DropForeignKey("dbo.AttributeUnits", "Attribute_AttributeID", "dbo.Attributes");
            DropTable("dbo.AttributeUnits");
            CreateIndex("dbo.UnitAttributes", "Attribute_AttributeID");
            CreateIndex("dbo.UnitAttributes", "Unit_UnitID");
            AddForeignKey("dbo.UnitAttributes", "Attribute_AttributeID", "dbo.Attributes", "Attribute_ID", cascadeDelete: true);
            AddForeignKey("dbo.UnitAttributes", "Unit_UnitID", "dbo.Units", "Unit_ID", cascadeDelete: true);
        }
    }
}
