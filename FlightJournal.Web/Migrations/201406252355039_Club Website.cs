namespace FlightJournal.Web.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ClubWebsite : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Clubs", "Website", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Clubs", "Website");
        }
    }
}
