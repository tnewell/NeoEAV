namespace EAVEntities.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class EAV_Test15 : DbMigration
    {
        public override void Up()
        {
            RenameColumn(table: "dbo.AttributeUnits", name: "Attribute_AttributeID", newName: "Attribute_ID");
            RenameColumn(table: "dbo.AttributeUnits", name: "Unit_UnitID", newName: "Unit_ID");
        }
        
        public override void Down()
        {
            RenameColumn(table: "dbo.AttributeUnits", name: "Unit_ID", newName: "Unit_UnitID");
            RenameColumn(table: "dbo.AttributeUnits", name: "Attribute_ID", newName: "Attribute_AttributeID");
        }
    }
}
