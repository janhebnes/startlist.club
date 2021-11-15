namespace FlightJournal.Web.Migrations.FlightContext
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class FlightsextendedwithindexIX_Flights_Deleted_PilotBackseatId_StartTypeId : DbMigration
    {
        public override void Up()
        {
            Sql("CREATE NONCLUSTERED INDEX [IX_Flights_Deleted_PilotBackseatId_StartTypeId] ON[dbo].[Flights]([Deleted], [PilotBackseatId], [StartTypeId]) INCLUDE([BetalerId], [Date], [Departure], [Description], [FlightCost], [HasTrainingData], [LandedOnId], [Landing], [LandingCount], [LastUpdated], [LastUpdatedBy], [PilotId], [PlaneId], [RecordKey], [StartCost], [StartedFromId], [TachoCost], [TachoDeparture], [TachoLanding], [TaskDistance]) ");
        }
        
        public override void Down()
        {
            DropIndex("dbo.Flights", "IX_Flights_Deleted_PilotBackseatId_StartTypeId");
        }
    }
}
