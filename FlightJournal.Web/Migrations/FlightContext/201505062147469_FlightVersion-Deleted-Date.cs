namespace FlightJournal.Web.Migrations.FlightContext
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class FlightVersionDeletedDate : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.FlightVersionHistory", "Deleted", c => c.DateTime());
        }
        
        public override void Down()
        {
            DropColumn("dbo.FlightVersionHistory", "Deleted");
        }
    }
}
