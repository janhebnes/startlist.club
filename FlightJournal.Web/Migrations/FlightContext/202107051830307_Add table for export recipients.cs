namespace FlightJournal.Web.Migrations.FlightContext
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Addtableforexportrecipients : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.ExportRecipients",
                c => new
                    {
                        ExportRecipientId = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false),
                        LastUpdated = c.DateTime(nullable: false),
                        ExporterType = c.Int(nullable: false),
                        Username = c.String(),
                        Password = c.String(),
                        AuthenticationUrl = c.String(),
                        DeliveryUrl = c.String(nullable: false),
                        MaxDeliverySize = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.ExportRecipientId);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.ExportRecipients");
        }
    }
}
