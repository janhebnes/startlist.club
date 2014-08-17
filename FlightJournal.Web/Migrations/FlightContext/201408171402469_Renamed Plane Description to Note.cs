namespace FlightJournal.Web.Migrations.FlightContext
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RenamedPlaneDescriptiontoNote : DbMigration
    {
        public override void Up()
        {
            RenameColumn("dbo.Planes","Description","Note");
        }
        
        public override void Down()
        {
            RenameColumn("dbo.Planes", "Note", "Description");
        }
    }
}
