namespace FlightJournal.Web.Migrations.FlightContext
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Addingfieldstosupportautomaticexportofflightdata : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Clubs", "AutoExportConfigName", c => c.String());
            AddColumn("dbo.Flights", "CandidateForExport", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Flights", "CandidateForExport");
            DropColumn("dbo.Clubs", "AutoExportConfigName");
        }
    }
}
