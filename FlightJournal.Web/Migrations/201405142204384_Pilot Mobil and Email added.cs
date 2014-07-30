namespace FlightJournal.Web.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
#if (!DEBUG)
    public partial class PilotMobilandEmailadded : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Pilots", "MobilNumber", c => c.String());
            AddColumn("dbo.Pilots", "Email", c => c.String());
        }

        public override void Down()
        {
            DropColumn("dbo.Pilots", "Email");
            DropColumn("dbo.Pilots", "MobilNumber");
        }
    }
#endif
}
