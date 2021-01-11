namespace FlightJournal.Web.Migrations.FlightContext
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedManouvreTable : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.ManouvreIcons",
                c => new
                    {
                        IconId = c.Int(nullable: false, identity: true),
                        Icon = c.String(),
                    })
                .PrimaryKey(t => t.IconId);
            
            CreateTable(
                "dbo.Manouvres",
                c => new
                    {
                        ManouvreId = c.Int(nullable: false, identity: true),
                        ManouvreItem = c.String(),
                        Icon = c.Int(nullable: false),
                        ManouvreIcon_IconId = c.Int(),
                    })
                .PrimaryKey(t => t.ManouvreId)
                .ForeignKey("dbo.ManouvreIcons", t => t.ManouvreIcon_IconId)
                .Index(t => t.ManouvreIcon_IconId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Manouvres", "ManouvreIcon_IconId", "dbo.ManouvreIcons");
            DropIndex("dbo.Manouvres", new[] { "ManouvreIcon_IconId" });
            DropTable("dbo.Manouvres");
            DropTable("dbo.ManouvreIcons");
        }
    }
}
