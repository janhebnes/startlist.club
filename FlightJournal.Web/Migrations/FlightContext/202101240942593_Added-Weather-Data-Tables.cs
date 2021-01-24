namespace FlightJournal.Web.Migrations.FlightContext
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedWeatherDataTables : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Weathers",
                c => new
                    {
                        WeatherId = c.Int(nullable: false, identity: true),
                        WindDirection_WindDirectionId = c.Int(),
                        WindSpeed_WindSpeedId = c.Int(),
                    })
                .PrimaryKey(t => t.WeatherId)
                .ForeignKey("dbo.WindDirections", t => t.WindDirection_WindDirectionId)
                .ForeignKey("dbo.WindSpeeds", t => t.WindSpeed_WindSpeedId)
                .Index(t => t.WindDirection_WindDirectionId)
                .Index(t => t.WindSpeed_WindSpeedId);
            
            CreateTable(
                "dbo.WindDirections",
                c => new
                    {
                        WindDirectionId = c.Int(nullable: false, identity: true),
                        WindDirectionItem = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.WindDirectionId);
            
            CreateTable(
                "dbo.WindSpeeds",
                c => new
                    {
                        WindSpeedId = c.Int(nullable: false, identity: true),
                        WindSpeedItem = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.WindSpeedId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Weathers", "WindSpeed_WindSpeedId", "dbo.WindSpeeds");
            DropForeignKey("dbo.Weathers", "WindDirection_WindDirectionId", "dbo.WindDirections");
            DropIndex("dbo.Weathers", new[] { "WindSpeed_WindSpeedId" });
            DropIndex("dbo.Weathers", new[] { "WindDirection_WindDirectionId" });
            DropTable("dbo.WindSpeeds");
            DropTable("dbo.WindDirections");
            DropTable("dbo.Weathers");
        }
    }
}
