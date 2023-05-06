namespace FlightJournal.Web.Migrations.FlightContext
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddingPilotInTrainingProgram : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.PilotInTrainingPrograms",
                c => new
                    {
                        PilotInTrainingProgramId = c.Int(nullable: false, identity: true),
                        PilotId = c.Int(nullable: false),
                        Training2ProgramId = c.Int(nullable: false),
                        StartDate = c.DateTime(),
                        EndDate = c.DateTime(),
                        IsCompleted = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.PilotInTrainingProgramId)
                //.ForeignKey("dbo.Pilots", t => t.PilotId, cascadeDelete: true)
                //.ForeignKey("dbo.Training2Program", t => t.Training2ProgramId, cascadeDelete: true)
                .Index(t => t.PilotId)
                .Index(t => t.Training2ProgramId);
            
        }
        
        public override void Down()
        {
            //DropForeignKey("dbo.PilotInTrainingPrograms", "Training2ProgramId", "dbo.Training2Program");
            //DropForeignKey("dbo.PilotInTrainingPrograms", "PilotId", "dbo.Pilots");
            DropIndex("dbo.PilotInTrainingPrograms", new[] { "Training2ProgramId" });
            DropIndex("dbo.PilotInTrainingPrograms", new[] { "PilotId" });
            DropTable("dbo.PilotInTrainingPrograms");
        }
    }
}
