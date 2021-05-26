namespace FlightJournal.Web.Migrations.FlightContext
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddtablefordefiningAPRSlisteningareas : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.ListenerAreas",
                c => new
                    {
                        ListenerAreaId = c.Int(nullable: false, identity: true),
                        Latitude = c.Double(nullable: false),
                        Longitude = c.Double(nullable: false),
                        Radius = c.Double(nullable: false),
                    })
                .PrimaryKey(t => t.ListenerAreaId);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.ListenerAreas");
        }
    }
}
