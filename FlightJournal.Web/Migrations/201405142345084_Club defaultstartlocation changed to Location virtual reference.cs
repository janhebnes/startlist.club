namespace FlightJournal.Web.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
#if (!DEBUG)
    public partial class ClubdefaultstartlocationchangedtoLocationvirtualreference : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.Clubs", "DefaultStartLocation_LocationId", "dbo.Locations");
            DropIndex("dbo.Clubs", new[] { "DefaultStartLocation_LocationId" });
            RenameColumn(table: "dbo.Clubs", name: "DefaultStartLocation_LocationId", newName: "LocationId");
            AlterColumn("dbo.Clubs", "LocationId", c => c.Int(nullable: false));
            CreateIndex("dbo.Clubs", "LocationId");
            AddForeignKey("dbo.Clubs", "LocationId", "dbo.Locations", "LocationId", cascadeDelete: false);
            DropColumn("dbo.Clubs", "DefaultStartLocationId");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Clubs", "DefaultStartLocationId", c => c.Int(nullable: false));
            DropForeignKey("dbo.Clubs", "LocationId", "dbo.Locations");
            DropIndex("dbo.Clubs", new[] { "LocationId" });
            AlterColumn("dbo.Clubs", "LocationId", c => c.Int());
            RenameColumn(table: "dbo.Clubs", name: "LocationId", newName: "DefaultStartLocation_LocationId");
            CreateIndex("dbo.Clubs", "DefaultStartLocation_LocationId");
            AddForeignKey("dbo.Clubs", "DefaultStartLocation_LocationId", "dbo.Locations", "LocationId");
        }
    }
#endif
}
