namespace FlightJournal.Web.Migrations.FlightContext
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class TrainingFlightAnnotationsextendedwithindexIX_TrainingFlightAnnotations_FlightId : DbMigration
    {
        public override void Up()
        {
            // Non-unique: an annotation per flight is not guaranteed
            CreateIndex("dbo.TrainingFlightAnnotations", "FlightId", name: "IX_TrainingFlightAnnotations_FlightId");
        }

        public override void Down()
        {
            DropIndex("dbo.TrainingFlightAnnotations", "IX_TrainingFlightAnnotations_FlightId");
        }
    }
}
