namespace OracleProject.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class cluster : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Locations", "cluster", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Locations", "cluster");
        }
    }
}
