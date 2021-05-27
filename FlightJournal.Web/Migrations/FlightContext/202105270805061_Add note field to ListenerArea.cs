namespace FlightJournal.Web.Migrations.FlightContext
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddnotefieldtoListenerArea : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ListenerAreas", "Note", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.ListenerAreas", "Note");
        }
    }
}
