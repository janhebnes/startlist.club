namespace FlightJournal.Web.Migrations.FlightContext
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddClubflagforusingAPRStakeoffandlanding : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Clubs", "UseAPRSTakeoffAndLanding", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Clubs", "UseAPRSTakeoffAndLanding");
        }
    }
}
