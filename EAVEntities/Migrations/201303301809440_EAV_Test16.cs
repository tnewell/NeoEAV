namespace EAVEntities.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class EAV_Test16 : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.DataTypes",
                c => new
                    {
                        Data_Type_ID = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, maxLength: 16),
                    })
                .PrimaryKey(t => t.Data_Type_ID);
            
            AddColumn("dbo.Attributes", "Data_Type_ID", c => c.Int(nullable: false));
            AddForeignKey("dbo.Attributes", "Data_Type_ID", "dbo.DataTypes", "Data_Type_ID");
            CreateIndex("dbo.Attributes", "Data_Type_ID");
        }
        
        public override void Down()
        {
            DropIndex("dbo.Attributes", new[] { "Data_Type_ID" });
            DropForeignKey("dbo.Attributes", "Data_Type_ID", "dbo.DataTypes");
            DropColumn("dbo.Attributes", "Data_Type_ID");
            DropTable("dbo.DataTypes");
        }
    }
}
