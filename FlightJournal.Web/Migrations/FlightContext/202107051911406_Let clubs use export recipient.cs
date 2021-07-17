namespace FlightJournal.Web.Migrations.FlightContext
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Letclubsuseexportrecipient : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Clubs", "ExportRecipientId", c => c.Int());
            CreateIndex("dbo.Clubs", "ExportRecipientId");
            AddForeignKey("dbo.Clubs", "ExportRecipientId", "dbo.ExportRecipients", "ExportRecipientId");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Clubs", "ExportRecipientId", "dbo.ExportRecipients");
            DropIndex("dbo.Clubs", new[] { "ExportRecipientId" });
            DropColumn("dbo.Clubs", "ExportRecipientId");
        }
    }
}
