namespace FlightLog.Migrations
{
    using System.Data.Entity.Migrations;
    
    public partial class PilotLogRelation : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("PilotLogs", "Pilot_PilotId", "Pilots");
            DropForeignKey("PilotLogs", "Flight_FlightId", "Flights");
            DropIndex("PilotLogs", new[] { "Pilot_PilotId" });
            DropIndex("PilotLogs", new[] { "Flight_FlightId" });
            AddColumn("PilotLogs", "PilotId", c => c.Int(nullable: false));
            AddColumn("PilotLogs", "FlightId", c => c.Guid(nullable: false));
            AddForeignKey("PilotLogs", "PilotId", "Pilots", "PilotId", cascadeDelete: true);
            AddForeignKey("PilotLogs", "FlightId", "Flights", "FlightId", cascadeDelete: true);
            CreateIndex("PilotLogs", "PilotId");
            CreateIndex("PilotLogs", "FlightId");
            DropColumn("PilotLogs", "Pilot_PilotId");
            DropColumn("PilotLogs", "Flight_FlightId");
        }
        
        public override void Down()
        {
            AddColumn("PilotLogs", "Flight_FlightId", c => c.Guid());
            AddColumn("PilotLogs", "Pilot_PilotId", c => c.Int());
            DropIndex("PilotLogs", new[] { "FlightId" });
            DropIndex("PilotLogs", new[] { "PilotId" });
            DropForeignKey("PilotLogs", "FlightId", "Flights");
            DropForeignKey("PilotLogs", "PilotId", "Pilots");
            DropColumn("PilotLogs", "FlightId");
            DropColumn("PilotLogs", "PilotId");
            CreateIndex("PilotLogs", "Flight_FlightId");
            CreateIndex("PilotLogs", "Pilot_PilotId");
            AddForeignKey("PilotLogs", "Flight_FlightId", "Flights", "FlightId");
            AddForeignKey("PilotLogs", "Pilot_PilotId", "Pilots", "PilotId");
        }
    }
}
