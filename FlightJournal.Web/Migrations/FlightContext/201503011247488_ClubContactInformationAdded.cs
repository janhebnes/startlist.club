namespace FlightJournal.Web.Migrations.FlightContext
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ClubContactInformationAdded : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Clubs", "ContactInformation", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Clubs", "ContactInformation");
        }
    }
}
