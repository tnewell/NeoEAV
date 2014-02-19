namespace EAVEntities.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class remove_fixed_values : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.Attributes", "Has_Fixed_Values");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Attributes", "Has_Fixed_Values", c => c.Boolean(nullable: false));
        }
    }
}
