namespace FlightJournal.Web.Migrations.FlightContext
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class LocationextendedwithbitformarkingICAOspotasaOGNFlightLogAirfield : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Locations", "RegisteredOgnFlightLogAirfield", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Locations", "RegisteredOgnFlightLogAirfield");
        }
    }
}
