namespace FlightJournal.Web.Migrations.FlightContext
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class FlightsextendedwithindexonDateStartedFromIdDeleted : DbMigration
    {
        public override void Up()
        {
            // Overriding default Index handling by removing this step that is hit because the StartedFromId is expected to only be present in one index set 
            // DropIndex("dbo.Flights", new[] { "StartedFromId" });

            // Overriding default Index because we want to inlclude fields in the index (the end result is the same)
            //CreateIndex("dbo.Flights", new[] { "Date", "StartedFromId", "Deleted" }, name: "IX_Flights_Date_StartedFromId_Deleted");
            Sql("CREATE NONCLUSTERED INDEX [IX_Flights_Date_StartedFromId_Deleted] ON [dbo].[Flights] ([Date], [StartedFromId], [Deleted]) INCLUDE ([BetalerId], [Departure], [Description], [FlightCost], [LandedOnId], [Landing], [LandingCount], [LastUpdated], [LastUpdatedBy], [PilotBackseatId], [PilotId], [PlaneId], [RecordKey], [StartCost], [StartTypeId], [TachoCost], [TachoDeparture], [TachoLanding], [TaskDistance])");
        }

        public override void Down()
        {
            // Overriding default Index handling by removing this step 
            DropIndex("dbo.Flights", "IX_Flights_Date_StartedFromId_Deleted");
            //CreateIndex("dbo.Flights", "StartedFromId");
        }
    }
}
