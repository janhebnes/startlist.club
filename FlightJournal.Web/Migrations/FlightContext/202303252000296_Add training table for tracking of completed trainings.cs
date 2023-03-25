namespace FlightJournal.Web.Migrations.FlightContext
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Addtrainingtablefortrackingofcompletedtrainings : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Trainings",
                c => new
                    {
                        PilotId = c.Int(nullable: false),
                        Training2ProgramId = c.Int(nullable: false),
                        Started = c.DateTime(nullable: false),
                        Finished = c.DateTime(nullable: false),
                        DidComplete = c.Boolean(nullable: false),
                        TrainingProgram_Training2ProgramId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.PilotId, t.Training2ProgramId })
                .ForeignKey("dbo.Pilots", t => t.PilotId)
                .ForeignKey("dbo.Training2Program", t => t.TrainingProgram_Training2ProgramId)
                .Index(t => t.PilotId)
                .Index(t => t.TrainingProgram_Training2ProgramId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Trainings", "TrainingProgram_Training2ProgramId", "dbo.Training2Program");
            DropForeignKey("dbo.Trainings", "PilotId", "dbo.Pilots");
            DropIndex("dbo.Trainings", new[] { "TrainingProgram_Training2ProgramId" });
            DropIndex("dbo.Trainings", new[] { "PilotId" });
            DropTable("dbo.Trainings");
        }
    }
}
