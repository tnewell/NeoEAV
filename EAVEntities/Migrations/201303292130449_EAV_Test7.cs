namespace EAVEntities.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class EAV_Test7 : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.ContainerInstances", "Parent_Container_Instance_ID", c => c.Int());
        }
        
        public override void Down()
        {
            AlterColumn("dbo.ContainerInstances", "Parent_Container_Instance_ID", c => c.Int(nullable: false));
        }
    }
}
