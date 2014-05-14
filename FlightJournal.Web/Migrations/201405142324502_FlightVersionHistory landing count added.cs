namespace FlightJournal.Web.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class FlightVersionHistorylandingcountadded : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.FlightVersionHistory", "LandingCount", c => c.Int(nullable: false, defaultValue:1));
        }
        
        public override void Down()
        {
            DropColumn("dbo.FlightVersionHistory", "LandingCount");
        }
    }
}
