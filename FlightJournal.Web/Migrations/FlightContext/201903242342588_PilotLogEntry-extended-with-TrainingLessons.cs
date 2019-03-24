namespace FlightJournal.Web.Migrations.FlightContext
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class PilotLogEntryextendedwithTrainingLessons : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.Notes", "Flight_FlightId", "dbo.Flights");
            DropForeignKey("dbo.PilotLogEntries", "Pilot_PilotId", "dbo.Pilots");
            DropIndex("dbo.Notes", new[] { "Flight_FlightId" });
            DropIndex("dbo.PilotLogEntries", new[] { "Flight_FlightId" });
            RenameColumn(table: "dbo.PilotLogEntries", name: "Pilot_PilotId", newName: "PilotId");
            RenameIndex(table: "dbo.PilotLogEntries", name: "IX_Pilot_PilotId", newName: "IX_PilotId");
            CreateTable(
                "dbo.Instructors",
                c => new
                    {
                        InstructorId = c.Int(nullable: false, identity: true),
                    })
                .PrimaryKey(t => t.InstructorId);
            
            CreateTable(
                "dbo.TrainingLessons",
                c => new
                    {
                        TrainingLessonId = c.Int(nullable: false, identity: true),
                        TrainingLessonCategoryId = c.Int(nullable: false),
                        Identifier = c.String(),
                        Description = c.String(),
                        RequiresFlight = c.Boolean(nullable: false),
                        RequiresFlightInstructorApproval = c.Boolean(nullable: false),
                        Enabled = c.Boolean(nullable: false),
                        SortOrder = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.TrainingLessonId)
                .ForeignKey("dbo.TrainingLessonCategories", t => t.TrainingLessonCategoryId, cascadeDelete: true)
                .Index(t => t.TrainingLessonCategoryId);
            
            CreateTable(
                "dbo.TrainingLessonCategories",
                c => new
                    {
                        TrainingLessonCategoryId = c.Int(nullable: false, identity: true),
                        TrainingLessonCategoryName = c.String(),
                        Enabled = c.Boolean(nullable: false),
                        SortOrder = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.TrainingLessonCategoryId);
            
            AddColumn("dbo.PilotLogEntries", "FlightId", c => c.Int(nullable: false));
            AddColumn("dbo.PilotLogEntries", "Position", c => c.Int(nullable: false));
            AddColumn("dbo.PilotLogEntries", "TrainingLessonId", c => c.Int(nullable: false));
            AddColumn("dbo.PilotLogEntries", "TrainingLessonApproved", c => c.DateTime());
            AddColumn("dbo.PilotLogEntries", "TrainingLessonApprovedByFlightInstructorId", c => c.Int());
            AddColumn("dbo.PilotLogEntries", "Deleted", c => c.DateTime());
            AddColumn("dbo.PilotLogEntries", "LastUpdated", c => c.DateTime(nullable: false));
            AddColumn("dbo.PilotLogEntries", "LastUpdatedBy", c => c.String());
            AddColumn("dbo.PilotLogEntries", "TrainingLessonApprovedByFlightInstructor_InstructorId", c => c.Int());
            AlterColumn("dbo.PilotLogEntries", "Flight_FlightId", c => c.Guid());
            CreateIndex("dbo.PilotLogEntries", "TrainingLessonId");
            CreateIndex("dbo.PilotLogEntries", "Flight_FlightId");
            CreateIndex("dbo.PilotLogEntries", "TrainingLessonApprovedByFlightInstructor_InstructorId");
            AddForeignKey("dbo.PilotLogEntries", "TrainingLessonId", "dbo.TrainingLessons", "TrainingLessonId", cascadeDelete: true);
            AddForeignKey("dbo.PilotLogEntries", "TrainingLessonApprovedByFlightInstructor_InstructorId", "dbo.Instructors", "InstructorId");
            AddForeignKey("dbo.PilotLogEntries", "PilotId", "dbo.Pilots", "PilotId");
            DropColumn("dbo.PilotLogEntries", "Lesson");
            DropTable("dbo.Notes");
        }
        
        public override void Down()
        {
            CreateTable(
                "dbo.Notes",
                c => new
                    {
                        NoteId = c.Int(nullable: false, identity: true),
                        CreatedOn = c.DateTime(nullable: false),
                        Description = c.String(),
                        Flight_FlightId = c.Guid(),
                    })
                .PrimaryKey(t => t.NoteId);
            
            AddColumn("dbo.PilotLogEntries", "Lesson", c => c.String());
            DropForeignKey("dbo.PilotLogEntries", "PilotId", "dbo.Pilots");
            DropForeignKey("dbo.PilotLogEntries", "TrainingLessonApprovedByFlightInstructor_InstructorId", "dbo.Instructors");
            DropForeignKey("dbo.PilotLogEntries", "TrainingLessonId", "dbo.TrainingLessons");
            DropForeignKey("dbo.TrainingLessons", "TrainingLessonCategoryId", "dbo.TrainingLessonCategories");
            DropIndex("dbo.TrainingLessons", new[] { "TrainingLessonCategoryId" });
            DropIndex("dbo.PilotLogEntries", new[] { "TrainingLessonApprovedByFlightInstructor_InstructorId" });
            DropIndex("dbo.PilotLogEntries", new[] { "Flight_FlightId" });
            DropIndex("dbo.PilotLogEntries", new[] { "TrainingLessonId" });
            AlterColumn("dbo.PilotLogEntries", "Flight_FlightId", c => c.Guid(nullable: false));
            DropColumn("dbo.PilotLogEntries", "TrainingLessonApprovedByFlightInstructor_InstructorId");
            DropColumn("dbo.PilotLogEntries", "LastUpdatedBy");
            DropColumn("dbo.PilotLogEntries", "LastUpdated");
            DropColumn("dbo.PilotLogEntries", "Deleted");
            DropColumn("dbo.PilotLogEntries", "TrainingLessonApprovedByFlightInstructorId");
            DropColumn("dbo.PilotLogEntries", "TrainingLessonApproved");
            DropColumn("dbo.PilotLogEntries", "TrainingLessonId");
            DropColumn("dbo.PilotLogEntries", "Position");
            DropColumn("dbo.PilotLogEntries", "FlightId");
            DropTable("dbo.TrainingLessonCategories");
            DropTable("dbo.TrainingLessons");
            DropTable("dbo.Instructors");
            RenameIndex(table: "dbo.PilotLogEntries", name: "IX_PilotId", newName: "IX_Pilot_PilotId");
            RenameColumn(table: "dbo.PilotLogEntries", name: "PilotId", newName: "Pilot_PilotId");
            CreateIndex("dbo.PilotLogEntries", "Flight_FlightId");
            CreateIndex("dbo.Notes", "Flight_FlightId");
            AddForeignKey("dbo.PilotLogEntries", "Pilot_PilotId", "dbo.Pilots", "PilotId", cascadeDelete: true);
            AddForeignKey("dbo.Notes", "Flight_FlightId", "dbo.Flights", "FlightId");
        }
    }
}
