namespace FlightJournal.Web.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
#if (!DEBUG)
    public partial class Flightlandingcountadded : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Flights", "LandingCount", c => c.Int(nullable: false, defaultValue:1));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Flights", "LandingCount");
        }
    }
#endif
}
