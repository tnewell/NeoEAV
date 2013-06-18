namespace EAVEntities.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class eav : DbMigration
    {
        public override void Up()
        {
            CreateIndex("Projects", "Name", true, "IX_Projects_Name");
            CreateIndex("Projects", "Data_Name", true, "IX_Projects_Data_Name");

            CreateIndex("Containers", new string[] { "Name", "Project_ID"}, true, "IX_Containers_Name_Project_ID");
            CreateIndex("Containers", new string[] { "Data_Name", "Project_ID" }, true, "IX_Containers_Data_Name_Project_ID");

            CreateIndex("Attributes", new string[] { "Name", "Container_ID" }, true, "IX_Attributes_Name_Container_ID");
            CreateIndex("Attributes", new string[] { "Data_Name", "Container_ID" }, true, "IX_Attributes_Data_Name_Container_ID");
        }
        
        public override void Down()
        {
            DropIndex("Projects", "IX_Projects_Name");
            DropIndex("Projects", "IX_Projects_Data_Name");
            
            DropIndex("Containers", "IX_Containers_Name_Project_ID");
            DropIndex("Containers", "IX_Containers_Data_Name_Project_ID");

            DropIndex("Attributes", "IX_Attributes_Name_Container_ID");
            DropIndex("Attributes", "IX_Attributes_Data_Name_Container_ID");
        }
    }
}
