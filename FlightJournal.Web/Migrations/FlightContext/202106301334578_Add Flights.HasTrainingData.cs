namespace FlightJournal.Web.Migrations.FlightContext
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddFlightsHasTrainingData : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Flights", "HasTrainingData", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Flights", "HasTrainingData");
        }
    }
}
