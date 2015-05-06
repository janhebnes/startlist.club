namespace FlightJournal.Web.Migrations.FlightContext
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class FlightDeletedDate : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Flights", "Deleted", c => c.DateTime());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Flights", "Deleted");
        }
    }
}
