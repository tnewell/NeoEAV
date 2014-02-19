namespace EAVEntities.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class EAV_Test20 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Attributes", "Has_Variable_Units", c => c.Boolean(nullable: false));
            AddColumn("dbo.Attributes", "Has_Fixed_Values", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Attributes", "Has_Fixed_Values");
            DropColumn("dbo.Attributes", "Has_Variable_Units");
        }
    }
}
