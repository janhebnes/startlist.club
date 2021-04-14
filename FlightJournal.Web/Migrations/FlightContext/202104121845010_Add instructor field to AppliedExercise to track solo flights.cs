namespace FlightJournal.Web.Migrations.FlightContext
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddinstructorfieldtoAppliedExercisetotracksoloflights : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.AppliedExercises", "Instructor_PilotId", c => c.Int());
            CreateIndex("dbo.AppliedExercises", "Instructor_PilotId");
            AddForeignKey("dbo.AppliedExercises", "Instructor_PilotId", "dbo.Pilots", "PilotId");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.AppliedExercises", "Instructor_PilotId", "dbo.Pilots");
            DropIndex("dbo.AppliedExercises", new[] { "Instructor_PilotId" });
            DropColumn("dbo.AppliedExercises", "Instructor_PilotId");
        }
    }
}
