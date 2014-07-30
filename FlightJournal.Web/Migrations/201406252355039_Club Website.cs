namespace FlightJournal.Web.Migrations
{
    using System;
    using System.Data.Entity.Migrations;

#if (!DEBUG)
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
#endif
}
