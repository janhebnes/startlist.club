namespace FlightLog.Migrations
{
    using System.Data.Entity.Migrations;
    
    public partial class InitialCreate : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "Flights",
                c => new
                    {
                        FlightId = c.Guid(nullable: false),
                        Date = c.DateTime(nullable: false),
                        Departure = c.DateTime(),
                        Landing = c.DateTime(),
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
                .ForeignKey("Planes", t => t.PlaneId, cascadeDelete: true)
                .ForeignKey("Pilots", t => t.PilotId)
                .ForeignKey("Pilots", t => t.PilotBackseatId)
                .ForeignKey("StartTypes", t => t.StartTypeId, cascadeDelete: true)
                .ForeignKey("Locations", t => t.StartedFromId, cascadeDelete: true)
                .ForeignKey("Locations", t => t.LandedOnId)
                .ForeignKey("Pilots", t => t.BetalerId)
                .Index(t => t.PlaneId)
                .Index(t => t.PilotId)
                .Index(t => t.PilotBackseatId)
                .Index(t => t.StartTypeId)
                .Index(t => t.StartedFromId)
                .Index(t => t.LandedOnId)
                .Index(t => t.BetalerId);
            
            CreateTable(
                "Planes",
                c => new
                    {
                        PlaneId = c.Int(nullable: false, identity: true),
                        Registration = c.String(nullable: false),
                        CompetitionId = c.String(nullable: false),
                        Seats = c.Double(nullable: false),
                        Engines = c.Double(nullable: false),
                        EntryDate = c.DateTime(nullable: false),
                        ExitDate = c.DateTime(),
                        StartTypeId = c.Int(),
                    })
                .PrimaryKey(t => t.PlaneId)
                .ForeignKey("StartTypes", t => t.StartTypeId)
                .Index(t => t.StartTypeId);
            
            CreateTable(
                "StartTypes",
                c => new
                    {
                        StartTypeId = c.Int(nullable: false, identity: true),
                        ShortName = c.String(),
                        Name = c.String(),
                        ClubId = c.Int(),
                    })
                .PrimaryKey(t => t.StartTypeId)
                .ForeignKey("Clubs", t => t.ClubId)
                .Index(t => t.ClubId);
            
            CreateTable(
                "Clubs",
                c => new
                    {
                        ClubId = c.Int(nullable: false, identity: true),
                        ShortName = c.String(),
                        Name = c.String(),
                        DefaultStartLocationId = c.Int(nullable: false),
                        DefaultStartLocation_LocationId = c.Int(),
                    })
                .PrimaryKey(t => t.ClubId)
                .ForeignKey("Locations", t => t.DefaultStartLocation_LocationId)
                .Index(t => t.DefaultStartLocation_LocationId);
            
            CreateTable(
                "Locations",
                c => new
                    {
                        LocationId = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                    })
                .PrimaryKey(t => t.LocationId);
            
            CreateTable(
                "PilotStatusTypes",
                c => new
                    {
                        PilotStatusId = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                        Description = c.String(),
                        ClubId = c.Int(),
                    })
                .PrimaryKey(t => t.PilotStatusId)
                .ForeignKey("Clubs", t => t.ClubId)
                .Index(t => t.ClubId);
            
            CreateTable(
                "Pilots",
                c => new
                    {
                        PilotId = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false),
                        UnionId = c.String(),
                        MemberId = c.String(),
                        ClubId = c.Int(nullable: false),
                        PilotStatus_PilotStatusId = c.Int(),
                    })
                .PrimaryKey(t => t.PilotId)
                .ForeignKey("Clubs", t => t.ClubId, cascadeDelete: true)
                .ForeignKey("PilotStatusTypes", t => t.PilotStatus_PilotStatusId)
                .Index(t => t.ClubId)
                .Index(t => t.PilotStatus_PilotStatusId);
            
            CreateTable(
                "PilotLogs",
                c => new
                    {
                        PilotLogid = c.Guid(nullable: false),
                        Lesson = c.String(),
                        Description = c.String(),
                        Pilot_PilotId = c.Int(),
                        Flight_FlightId = c.Guid(),
                    })
                .PrimaryKey(t => t.PilotLogid)
                .ForeignKey("Pilots", t => t.Pilot_PilotId)
                .ForeignKey("Flights", t => t.Flight_FlightId)
                .Index(t => t.Pilot_PilotId)
                .Index(t => t.Flight_FlightId);
            
            CreateTable(
                "FlightVersionHistory",
                c => new
                    {
                        FlightId = c.Guid(nullable: false),
                        Created = c.DateTime(nullable: false),
                        State = c.String(),
                        Date = c.DateTime(nullable: false),
                        Departure = c.DateTime(),
                        Landing = c.DateTime(),
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
                .ForeignKey("Planes", t => t.PlaneId, cascadeDelete: true)
                .ForeignKey("Pilots", t => t.PilotId)
                .ForeignKey("Pilots", t => t.PilotBackseatId)
                .ForeignKey("StartTypes", t => t.StartTypeId, cascadeDelete: true)
                .ForeignKey("Pilots", t => t.BetalerId)
                .ForeignKey("Locations", t => t.StartedFromId, cascadeDelete: true)
                .ForeignKey("Locations", t => t.LandedOnId)
                .Index(t => t.PlaneId)
                .Index(t => t.PilotId)
                .Index(t => t.PilotBackseatId)
                .Index(t => t.StartTypeId)
                .Index(t => t.BetalerId)
                .Index(t => t.StartedFromId)
                .Index(t => t.LandedOnId);
            
            CreateTable(
                "Notes",
                c => new
                    {
                        NoteId = c.Int(nullable: false, identity: true),
                        CreatedOn = c.DateTime(nullable: false),
                        Description = c.String(),
                        Flight_FlightId = c.Guid(),
                    })
                .PrimaryKey(t => t.NoteId)
                .ForeignKey("Flights", t => t.Flight_FlightId)
                .Index(t => t.Flight_FlightId);
            
        }
        
        public override void Down()
        {
            DropIndex("Notes", new[] { "Flight_FlightId" });
            DropIndex("FlightVersionHistory", new[] { "LandedOnId" });
            DropIndex("FlightVersionHistory", new[] { "StartedFromId" });
            DropIndex("FlightVersionHistory", new[] { "BetalerId" });
            DropIndex("FlightVersionHistory", new[] { "StartTypeId" });
            DropIndex("FlightVersionHistory", new[] { "PilotBackseatId" });
            DropIndex("FlightVersionHistory", new[] { "PilotId" });
            DropIndex("FlightVersionHistory", new[] { "PlaneId" });
            DropIndex("PilotLogs", new[] { "Flight_FlightId" });
            DropIndex("PilotLogs", new[] { "Pilot_PilotId" });
            DropIndex("Pilots", new[] { "PilotStatus_PilotStatusId" });
            DropIndex("Pilots", new[] { "ClubId" });
            DropIndex("PilotStatusTypes", new[] { "ClubId" });
            DropIndex("Clubs", new[] { "DefaultStartLocation_LocationId" });
            DropIndex("StartTypes", new[] { "ClubId" });
            DropIndex("Planes", new[] { "StartTypeId" });
            DropIndex("Flights", new[] { "BetalerId" });
            DropIndex("Flights", new[] { "LandedOnId" });
            DropIndex("Flights", new[] { "StartedFromId" });
            DropIndex("Flights", new[] { "StartTypeId" });
            DropIndex("Flights", new[] { "PilotBackseatId" });
            DropIndex("Flights", new[] { "PilotId" });
            DropIndex("Flights", new[] { "PlaneId" });
            DropForeignKey("Notes", "Flight_FlightId", "Flights");
            DropForeignKey("FlightVersionHistory", "LandedOnId", "Locations");
            DropForeignKey("FlightVersionHistory", "StartedFromId", "Locations");
            DropForeignKey("FlightVersionHistory", "BetalerId", "Pilots");
            DropForeignKey("FlightVersionHistory", "StartTypeId", "StartTypes");
            DropForeignKey("FlightVersionHistory", "PilotBackseatId", "Pilots");
            DropForeignKey("FlightVersionHistory", "PilotId", "Pilots");
            DropForeignKey("FlightVersionHistory", "PlaneId", "Planes");
            DropForeignKey("PilotLogs", "Flight_FlightId", "Flights");
            DropForeignKey("PilotLogs", "Pilot_PilotId", "Pilots");
            DropForeignKey("Pilots", "PilotStatus_PilotStatusId", "PilotStatusTypes");
            DropForeignKey("Pilots", "ClubId", "Clubs");
            DropForeignKey("PilotStatusTypes", "ClubId", "Clubs");
            DropForeignKey("Clubs", "DefaultStartLocation_LocationId", "Locations");
            DropForeignKey("StartTypes", "ClubId", "Clubs");
            DropForeignKey("Planes", "StartTypeId", "StartTypes");
            DropForeignKey("Flights", "BetalerId", "Pilots");
            DropForeignKey("Flights", "LandedOnId", "Locations");
            DropForeignKey("Flights", "StartedFromId", "Locations");
            DropForeignKey("Flights", "StartTypeId", "StartTypes");
            DropForeignKey("Flights", "PilotBackseatId", "Pilots");
            DropForeignKey("Flights", "PilotId", "Pilots");
            DropForeignKey("Flights", "PlaneId", "Planes");
            DropTable("Notes");
            DropTable("FlightVersionHistory");
            DropTable("PilotLogs");
            DropTable("Pilots");
            DropTable("PilotStatusTypes");
            DropTable("Locations");
            DropTable("Clubs");
            DropTable("StartTypes");
            DropTable("Planes");
            DropTable("Flights");
        }
    }
}
