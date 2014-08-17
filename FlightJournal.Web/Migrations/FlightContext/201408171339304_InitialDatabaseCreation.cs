namespace FlightJournal.Web.Migrations.FlightContext
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class InitialDatabaseCreation : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Clubs",
                c => new
                    {
                        ClubId = c.Int(nullable: false, identity: true),
                        ShortName = c.String(),
                        Name = c.String(),
                        LocationId = c.Int(nullable: false),
                        Website = c.String(),
                    })
                .PrimaryKey(t => t.ClubId)
                .ForeignKey("dbo.Locations", t => t.LocationId, cascadeDelete: true)
                .Index(t => t.LocationId);
            
            CreateTable(
                "dbo.Locations",
                c => new
                    {
                        LocationId = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                    })
                .PrimaryKey(t => t.LocationId);
            
            CreateTable(
                "dbo.Pilots",
                c => new
                    {
                        PilotId = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false),
                        UnionId = c.String(),
                        MemberId = c.String(),
                        MobilNumber = c.String(),
                        Email = c.String(),
                        ClubId = c.Int(nullable: false),
                        Note = c.String(),
                        ExitDate = c.DateTime(),
                        PilotStatus_PilotStatusId = c.Int(),
                    })
                .PrimaryKey(t => t.PilotId)
                .ForeignKey("dbo.Clubs", t => t.ClubId, cascadeDelete: true)
                .ForeignKey("dbo.PilotStatusTypes", t => t.PilotStatus_PilotStatusId)
                .Index(t => t.ClubId)
                .Index(t => t.PilotStatus_PilotStatusId);
            
            CreateTable(
                "dbo.FlightVersionHistory",
                c => new
                    {
                        FlightId = c.Guid(nullable: false),
                        Created = c.DateTime(nullable: false),
                        State = c.String(),
                        Date = c.DateTime(nullable: false),
                        Departure = c.DateTime(),
                        Landing = c.DateTime(),
                        LandingCount = c.Int(nullable: false),
                        PlaneId = c.Int(nullable: false),
                        PilotId = c.Int(nullable: false),
                        PilotBackseatId = c.Int(),
                        StartTypeId = c.Int(nullable: false),
                        BetalerId = c.Int(nullable: false),
                        StartedFromId = c.Int(nullable: false),
                        LandedOnId = c.Int(),
                        TachoDeparture = c.Double(),
                        TachoLanding = c.Double(),
                        Description = c.String(),
                        LastUpdated = c.DateTime(nullable: false),
                        LastUpdatedBy = c.String(),
                    })
                .PrimaryKey(t => new { t.FlightId, t.Created })
                .ForeignKey("dbo.Pilots", t => t.BetalerId)
                .ForeignKey("dbo.Locations", t => t.LandedOnId)
                .ForeignKey("dbo.Pilots", t => t.PilotId)
                .ForeignKey("dbo.Pilots", t => t.PilotBackseatId)
                .ForeignKey("dbo.Planes", t => t.PlaneId, cascadeDelete: true)
                .ForeignKey("dbo.Locations", t => t.StartedFromId, cascadeDelete: true)
                .ForeignKey("dbo.StartTypes", t => t.StartTypeId, cascadeDelete: true)
                .Index(t => t.PlaneId)
                .Index(t => t.PilotId)
                .Index(t => t.PilotBackseatId)
                .Index(t => t.StartTypeId)
                .Index(t => t.BetalerId)
                .Index(t => t.StartedFromId)
                .Index(t => t.LandedOnId);
            
            CreateTable(
                "dbo.Planes",
                c => new
                    {
                        PlaneId = c.Int(nullable: false, identity: true),
                        Registration = c.String(nullable: false),
                        CompetitionId = c.String(nullable: false),
                        ShortName = c.String(),
                        Class = c.String(),
                        Type = c.String(nullable: false),
                        Model = c.String(),
                        Owner = c.String(),
                        Description = c.String(),
                        Seats = c.Int(nullable: false),
                        Engines = c.Int(nullable: false),
                        ExitDate = c.DateTime(),
                        StartTypeId = c.Int(),
                    })
                .PrimaryKey(t => t.PlaneId)
                .ForeignKey("dbo.StartTypes", t => t.StartTypeId)
                .Index(t => t.StartTypeId);
            
            CreateTable(
                "dbo.StartTypes",
                c => new
                    {
                        StartTypeId = c.Int(nullable: false, identity: true),
                        ShortName = c.String(),
                        Name = c.String(),
                        ClubId = c.Int(),
                    })
                .PrimaryKey(t => t.StartTypeId)
                .ForeignKey("dbo.Clubs", t => t.ClubId)
                .Index(t => t.ClubId);
            
            CreateTable(
                "dbo.Flights",
                c => new
                    {
                        FlightId = c.Guid(nullable: false),
                        Date = c.DateTime(nullable: false),
                        Departure = c.DateTime(),
                        Landing = c.DateTime(),
                        LandingCount = c.Int(nullable: false),
                        PlaneId = c.Int(nullable: false),
                        PilotId = c.Int(nullable: false),
                        PilotBackseatId = c.Int(),
                        StartTypeId = c.Int(nullable: false),
                        StartedFromId = c.Int(nullable: false),
                        LandedOnId = c.Int(),
                        TachoDeparture = c.Double(),
                        TachoLanding = c.Double(),
                        TaskDistance = c.Double(),
                        Description = c.String(),
                        BetalerId = c.Int(nullable: false),
                        StartCost = c.Double(nullable: false),
                        FlightCost = c.Double(nullable: false),
                        TachoCost = c.Double(nullable: false),
                        LastUpdated = c.DateTime(nullable: false),
                        LastUpdatedBy = c.String(),
                        RecordKey = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.FlightId)
                .ForeignKey("dbo.Pilots", t => t.BetalerId)
                .ForeignKey("dbo.Locations", t => t.LandedOnId)
                .ForeignKey("dbo.Pilots", t => t.PilotId)
                .ForeignKey("dbo.Pilots", t => t.PilotBackseatId)
                .ForeignKey("dbo.Planes", t => t.PlaneId, cascadeDelete: true)
                .ForeignKey("dbo.Locations", t => t.StartedFromId, cascadeDelete: true)
                .ForeignKey("dbo.StartTypes", t => t.StartTypeId, cascadeDelete: true)
                .Index(t => t.PlaneId)
                .Index(t => t.PilotId)
                .Index(t => t.PilotBackseatId)
                .Index(t => t.StartTypeId)
                .Index(t => t.StartedFromId)
                .Index(t => t.LandedOnId)
                .Index(t => t.BetalerId);
            
            CreateTable(
                "dbo.Notes",
                c => new
                    {
                        NoteId = c.Int(nullable: false, identity: true),
                        CreatedOn = c.DateTime(nullable: false),
                        Description = c.String(),
                        Flight_FlightId = c.Guid(),
                    })
                .PrimaryKey(t => t.NoteId)
                .ForeignKey("dbo.Flights", t => t.Flight_FlightId)
                .Index(t => t.Flight_FlightId);
            
            CreateTable(
                "dbo.PilotStatusTypes",
                c => new
                    {
                        PilotStatusId = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                        Description = c.String(),
                        ClubId = c.Int(),
                    })
                .PrimaryKey(t => t.PilotStatusId)
                .ForeignKey("dbo.Clubs", t => t.ClubId)
                .Index(t => t.ClubId);
            
            CreateTable(
                "dbo.PilotLogEntries",
                c => new
                    {
                        PilotLogid = c.Guid(nullable: false),
                        Lesson = c.String(),
                        Description = c.String(),
                        Flight_FlightId = c.Guid(nullable: false),
                        Pilot_PilotId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.PilotLogid)
                .ForeignKey("dbo.Flights", t => t.Flight_FlightId)
                .ForeignKey("dbo.Pilots", t => t.Pilot_PilotId, cascadeDelete: true)
                .Index(t => t.Flight_FlightId)
                .Index(t => t.Pilot_PilotId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.PilotLogEntries", "Pilot_PilotId", "dbo.Pilots");
            DropForeignKey("dbo.PilotLogEntries", "Flight_FlightId", "dbo.Flights");
            DropForeignKey("dbo.Pilots", "PilotStatus_PilotStatusId", "dbo.PilotStatusTypes");
            DropForeignKey("dbo.PilotStatusTypes", "ClubId", "dbo.Clubs");
            DropForeignKey("dbo.FlightVersionHistory", "StartTypeId", "dbo.StartTypes");
            DropForeignKey("dbo.FlightVersionHistory", "StartedFromId", "dbo.Locations");
            DropForeignKey("dbo.FlightVersionHistory", "PlaneId", "dbo.Planes");
            DropForeignKey("dbo.Flights", "StartTypeId", "dbo.StartTypes");
            DropForeignKey("dbo.Flights", "StartedFromId", "dbo.Locations");
            DropForeignKey("dbo.Flights", "PlaneId", "dbo.Planes");
            DropForeignKey("dbo.Flights", "PilotBackseatId", "dbo.Pilots");
            DropForeignKey("dbo.Flights", "PilotId", "dbo.Pilots");
            DropForeignKey("dbo.Notes", "Flight_FlightId", "dbo.Flights");
            DropForeignKey("dbo.Flights", "LandedOnId", "dbo.Locations");
            DropForeignKey("dbo.Flights", "BetalerId", "dbo.Pilots");
            DropForeignKey("dbo.Planes", "StartTypeId", "dbo.StartTypes");
            DropForeignKey("dbo.StartTypes", "ClubId", "dbo.Clubs");
            DropForeignKey("dbo.FlightVersionHistory", "PilotBackseatId", "dbo.Pilots");
            DropForeignKey("dbo.FlightVersionHistory", "PilotId", "dbo.Pilots");
            DropForeignKey("dbo.FlightVersionHistory", "LandedOnId", "dbo.Locations");
            DropForeignKey("dbo.FlightVersionHistory", "BetalerId", "dbo.Pilots");
            DropForeignKey("dbo.Pilots", "ClubId", "dbo.Clubs");
            DropForeignKey("dbo.Clubs", "LocationId", "dbo.Locations");
            DropIndex("dbo.PilotLogEntries", new[] { "Pilot_PilotId" });
            DropIndex("dbo.PilotLogEntries", new[] { "Flight_FlightId" });
            DropIndex("dbo.PilotStatusTypes", new[] { "ClubId" });
            DropIndex("dbo.Notes", new[] { "Flight_FlightId" });
            DropIndex("dbo.Flights", new[] { "BetalerId" });
            DropIndex("dbo.Flights", new[] { "LandedOnId" });
            DropIndex("dbo.Flights", new[] { "StartedFromId" });
            DropIndex("dbo.Flights", new[] { "StartTypeId" });
            DropIndex("dbo.Flights", new[] { "PilotBackseatId" });
            DropIndex("dbo.Flights", new[] { "PilotId" });
            DropIndex("dbo.Flights", new[] { "PlaneId" });
            DropIndex("dbo.StartTypes", new[] { "ClubId" });
            DropIndex("dbo.Planes", new[] { "StartTypeId" });
            DropIndex("dbo.FlightVersionHistory", new[] { "LandedOnId" });
            DropIndex("dbo.FlightVersionHistory", new[] { "StartedFromId" });
            DropIndex("dbo.FlightVersionHistory", new[] { "BetalerId" });
            DropIndex("dbo.FlightVersionHistory", new[] { "StartTypeId" });
            DropIndex("dbo.FlightVersionHistory", new[] { "PilotBackseatId" });
            DropIndex("dbo.FlightVersionHistory", new[] { "PilotId" });
            DropIndex("dbo.FlightVersionHistory", new[] { "PlaneId" });
            DropIndex("dbo.Pilots", new[] { "PilotStatus_PilotStatusId" });
            DropIndex("dbo.Pilots", new[] { "ClubId" });
            DropIndex("dbo.Clubs", new[] { "LocationId" });
            DropTable("dbo.PilotLogEntries");
            DropTable("dbo.PilotStatusTypes");
            DropTable("dbo.Notes");
            DropTable("dbo.Flights");
            DropTable("dbo.StartTypes");
            DropTable("dbo.Planes");
            DropTable("dbo.FlightVersionHistory");
            DropTable("dbo.Pilots");
            DropTable("dbo.Locations");
            DropTable("dbo.Clubs");
        }
    }
}
