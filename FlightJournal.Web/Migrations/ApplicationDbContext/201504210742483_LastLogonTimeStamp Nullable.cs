namespace FlightJournal.Web.Migrations.ApplicationDbContext
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class LastLogonTimeStampNullable : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.AspNetUsers", "LastLogonTimeStamp", c => c.DateTime());
        }
        
        public override void Down()
        {
            AlterColumn("dbo.AspNetUsers", "LastLogonTimeStamp", c => c.DateTime(nullable: false));
        }
    }
}
