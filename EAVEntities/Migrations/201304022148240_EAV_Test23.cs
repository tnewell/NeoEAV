namespace EAVEntities.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class EAV_Test23 : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.DataTypes", "Data_Type_ID", c => c.Int(nullable: false));
            AlterColumn("dbo.DataTypes", "Name", c => c.String(nullable: false));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.DataTypes", "Name", c => c.String(nullable: false, maxLength: 16));
            AlterColumn("dbo.DataTypes", "Data_Type_ID", c => c.Int(nullable: false, identity: true));
        }
    }
}
