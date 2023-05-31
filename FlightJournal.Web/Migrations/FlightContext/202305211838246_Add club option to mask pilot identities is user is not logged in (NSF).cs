namespace FlightJournal.Web.Migrations.FlightContext
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddcluboptiontomaskpilotidentitiesisuserisnotloggedinNSF : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Clubs", "MaskPilotIdentityIfUserIsNotLoggedIn", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Clubs", "MaskPilotIdentityIfUserIsNotLoggedIn");
        }
    }
}
