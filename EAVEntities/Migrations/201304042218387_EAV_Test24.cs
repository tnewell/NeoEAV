namespace EAVEntities.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class EAV_Test24 : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Values", "Units", c => c.String(maxLength: 8));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Values", "Units", c => c.String(nullable: false, maxLength: 8));
        }
    }
}
