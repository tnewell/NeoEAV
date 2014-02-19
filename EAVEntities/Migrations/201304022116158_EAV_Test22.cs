namespace EAVEntities.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class EAV_Test22 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Values", "Units", c => c.String(nullable: false, maxLength: 8));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Values", "Units");
        }
    }
}
