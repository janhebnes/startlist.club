namespace FlightJournal.Web.Migrations.FlightContext
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Extrafieldsforexportvalidationfortrainingprograms : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Training2Lesson", "CanHaveDualFlightDuration", c => c.Boolean(nullable: false));
            AddColumn("dbo.Training2Lesson", "CanHaveSoloFlightDuration", c => c.Boolean(nullable: false));
            AddColumn("dbo.Training2Program", "RequireUnionIdsForExport", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Training2Program", "RequireUnionIdsForExport");
            DropColumn("dbo.Training2Lesson", "CanHaveSoloFlightDuration");
            DropColumn("dbo.Training2Lesson", "CanHaveDualFlightDuration");
        }
    }
}
