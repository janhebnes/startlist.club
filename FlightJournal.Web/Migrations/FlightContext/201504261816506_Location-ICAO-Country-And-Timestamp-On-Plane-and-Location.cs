namespace FlightJournal.Web.Migrations.FlightContext
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class LocationICAOCountryAndTimestampOnPlaneandLocation : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Locations", "ICAO", c => c.String());
            AddColumn("dbo.Locations", "Country", c => c.String());
            AddColumn("dbo.Locations", "CreatedTimestamp", c => c.DateTime(nullable: false));
            AddColumn("dbo.Locations", "CreatedBy", c => c.String());
            AddColumn("dbo.Locations", "LastUpdatedTimestamp", c => c.DateTime(nullable: false));
            AddColumn("dbo.Locations", "LastUpdatedBy", c => c.String());
            AddColumn("dbo.Planes", "CreatedTimestamp", c => c.DateTime(nullable: false));
            AddColumn("dbo.Planes", "CreatedBy", c => c.String());
            AddColumn("dbo.Planes", "LastUpdatedTimestamp", c => c.DateTime(nullable: false));
            AddColumn("dbo.Planes", "LastUpdatedBy", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Planes", "LastUpdatedBy");
            DropColumn("dbo.Planes", "LastUpdatedTimestamp");
            DropColumn("dbo.Planes", "CreatedBy");
            DropColumn("dbo.Planes", "CreatedTimestamp");
            DropColumn("dbo.Locations", "LastUpdatedBy");
            DropColumn("dbo.Locations", "LastUpdatedTimestamp");
            DropColumn("dbo.Locations", "CreatedBy");
            DropColumn("dbo.Locations", "CreatedTimestamp");
            DropColumn("dbo.Locations", "Country");
            DropColumn("dbo.Locations", "ICAO");
        }
    }
}
