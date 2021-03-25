namespace FlightJournal.Web.Migrations.FlightContext
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Flighttrainingtrackingandinstructorsforflightschool : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.AppliedExercises",
                c => new
                    {
                        AppliedExerciseId = c.Int(nullable: false, identity: true),
                        FlightId = c.Guid(nullable: false),
                        Action = c.Int(nullable: false),
                        Exercise_Training2ExerciseId = c.Int(nullable: false),
                        Grading_GradingId = c.Int(),
                        Lesson_Training2LessonId = c.Int(nullable: false),
                        Program_Training2ProgramId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.AppliedExerciseId)
                .ForeignKey("dbo.Training2Exercise", t => t.Exercise_Training2ExerciseId, cascadeDelete: true)
                .ForeignKey("dbo.Gradings", t => t.Grading_GradingId)
                .ForeignKey("dbo.Training2Lesson", t => t.Lesson_Training2LessonId, cascadeDelete: true)
                .ForeignKey("dbo.Training2Program", t => t.Program_Training2ProgramId, cascadeDelete: true)
                .Index(t => t.Exercise_Training2ExerciseId)
                .Index(t => t.Grading_GradingId)
                .Index(t => t.Lesson_Training2LessonId)
                .Index(t => t.Program_Training2ProgramId);
            
            CreateTable(
                "dbo.Training2Exercise",
                c => new
                    {
                        Training2ExerciseId = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false),
                        Note = c.String(),
                        AcceptanceCriteria = c.String(),
                        IsBriefingOnly = c.Boolean(nullable: false),
                        DisplayOrder = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Training2ExerciseId);
            
            CreateTable(
                "dbo.Training2Lesson",
                c => new
                    {
                        Training2LessonId = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false),
                        Precondition = c.String(),
                        Purpose = c.String(nullable: false),
                        AcceptanceCriteria = c.String(),
                        DisplayOrder = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Training2LessonId);
            
            CreateTable(
                "dbo.Training2Program",
                c => new
                    {
                        Training2ProgramId = c.Int(nullable: false, identity: true),
                        ShortName = c.String(nullable: false),
                        Name = c.String(nullable: false),
                        Notes = c.String(),
                        Url = c.String(),
                    })
                .PrimaryKey(t => t.Training2ProgramId);
            
            CreateTable(
                "dbo.Gradings",
                c => new
                    {
                        GradingId = c.Int(nullable: false, identity: true),
                        GradingIdForExport = c.Guid(nullable: false),
                        Name = c.String(),
                        Value = c.Int(nullable: false),
                        IsOk = c.Boolean(nullable: false),
                        AppliesToBriefingOnlyPartialExercises = c.Boolean(nullable: false),
                        AppliesToPracticalPartialExercises = c.Boolean(nullable: false),
                        DisplayOrder = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.GradingId);
            
            CreateTable(
                "dbo.Commentaries",
                c => new
                    {
                        CommentaryId = c.Int(nullable: false, identity: true),
                        Comment = c.String(nullable: false),
                        IsOk = c.Boolean(nullable: false),
                        DisplayOrder = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.CommentaryId);
            
            CreateTable(
                "dbo.CommentaryTypes",
                c => new
                    {
                        CommentaryTypeId = c.Int(nullable: false, identity: true),
                        CType = c.String(),
                        DisplayOrder = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.CommentaryTypeId);
            
            CreateTable(
                "dbo.TrainingFlightAnnotationCommentCommentTypes",
                c => new
                    {
                        TrainingFlightAnnotationId = c.Int(nullable: false),
                        CommentaryId = c.Int(nullable: false),
                        CommentaryTypeId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.TrainingFlightAnnotationId, t.CommentaryId, t.CommentaryTypeId })
                .ForeignKey("dbo.Commentaries", t => t.CommentaryId, cascadeDelete: true)
                .ForeignKey("dbo.CommentaryTypes", t => t.CommentaryTypeId, cascadeDelete: true)
                .ForeignKey("dbo.TrainingFlightAnnotations", t => t.TrainingFlightAnnotationId, cascadeDelete: true)
                .Index(t => t.TrainingFlightAnnotationId)
                .Index(t => t.CommentaryId)
                .Index(t => t.CommentaryTypeId);
            
            CreateTable(
                "dbo.TrainingFlightAnnotations",
                c => new
                    {
                        TrainingFlightAnnotationId = c.Int(nullable: false, identity: true),
                        FlightId = c.Guid(nullable: false),
                        Note = c.String(),
                        WindSpeed = c.Int(nullable: false),
                        WindDirection = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.TrainingFlightAnnotationId);
            
            CreateTable(
                "dbo.Manouvres",
                c => new
                    {
                        ManouvreId = c.Int(nullable: false, identity: true),
                        ManouvreItem = c.String(),
                        Description = c.String(),
                        IconCssClass = c.String(),
                        Icon = c.Int(nullable: false),
                        DisplayOrder = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.ManouvreId);
            
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
            
            CreateTable(
                "dbo.Training2LessonTraining2Exercise",
                c => new
                    {
                        Training2Lesson_Training2LessonId = c.Int(nullable: false),
                        Training2Exercise_Training2ExerciseId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.Training2Lesson_Training2LessonId, t.Training2Exercise_Training2ExerciseId })
                .ForeignKey("dbo.Training2Lesson", t => t.Training2Lesson_Training2LessonId, cascadeDelete: true)
                .ForeignKey("dbo.Training2Exercise", t => t.Training2Exercise_Training2ExerciseId, cascadeDelete: true)
                .Index(t => t.Training2Lesson_Training2LessonId)
                .Index(t => t.Training2Exercise_Training2ExerciseId);
            
            CreateTable(
                "dbo.Training2ProgramTraining2Lesson",
                c => new
                    {
                        Training2Program_Training2ProgramId = c.Int(nullable: false),
                        Training2Lesson_Training2LessonId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.Training2Program_Training2ProgramId, t.Training2Lesson_Training2LessonId })
                .ForeignKey("dbo.Training2Program", t => t.Training2Program_Training2ProgramId, cascadeDelete: true)
                .ForeignKey("dbo.Training2Lesson", t => t.Training2Lesson_Training2LessonId, cascadeDelete: true)
                .Index(t => t.Training2Program_Training2ProgramId)
                .Index(t => t.Training2Lesson_Training2LessonId);
            
            CreateTable(
                "dbo.TrainingFlightAnnotationsManouvres",
                c => new
                    {
                        TrainingFlightAnnotationRefId = c.Int(nullable: false),
                        ManouvreRefId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.TrainingFlightAnnotationRefId, t.ManouvreRefId })
                .ForeignKey("dbo.TrainingFlightAnnotations", t => t.TrainingFlightAnnotationRefId, cascadeDelete: true)
                .ForeignKey("dbo.Manouvres", t => t.ManouvreRefId, cascadeDelete: true)
                .Index(t => t.TrainingFlightAnnotationRefId)
                .Index(t => t.ManouvreRefId);
            
            CreateTable(
                "dbo.CommentaryCommentaryTypes",
                c => new
                    {
                        CommentaryRefId = c.Int(nullable: false),
                        CommentaryTypeRefId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.CommentaryRefId, t.CommentaryTypeRefId })
                .ForeignKey("dbo.Commentaries", t => t.CommentaryRefId, cascadeDelete: true)
                .ForeignKey("dbo.CommentaryTypes", t => t.CommentaryTypeRefId, cascadeDelete: true)
                .Index(t => t.CommentaryRefId)
                .Index(t => t.CommentaryTypeRefId);
            
            AddColumn("dbo.Pilots", "InstructorId", c => c.String());
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.CommentaryCommentaryTypes", "CommentaryTypeRefId", "dbo.CommentaryTypes");
            DropForeignKey("dbo.CommentaryCommentaryTypes", "CommentaryRefId", "dbo.Commentaries");
            DropForeignKey("dbo.TrainingFlightAnnotationCommentCommentTypes", "TrainingFlightAnnotationId", "dbo.TrainingFlightAnnotations");
            DropForeignKey("dbo.TrainingFlightAnnotationsManouvres", "ManouvreRefId", "dbo.Manouvres");
            DropForeignKey("dbo.TrainingFlightAnnotationsManouvres", "TrainingFlightAnnotationRefId", "dbo.TrainingFlightAnnotations");
            DropForeignKey("dbo.TrainingFlightAnnotationCommentCommentTypes", "CommentaryTypeId", "dbo.CommentaryTypes");
            DropForeignKey("dbo.TrainingFlightAnnotationCommentCommentTypes", "CommentaryId", "dbo.Commentaries");
            DropForeignKey("dbo.AppliedExercises", "Program_Training2ProgramId", "dbo.Training2Program");
            DropForeignKey("dbo.AppliedExercises", "Lesson_Training2LessonId", "dbo.Training2Lesson");
            DropForeignKey("dbo.AppliedExercises", "Grading_GradingId", "dbo.Gradings");
            DropForeignKey("dbo.AppliedExercises", "Exercise_Training2ExerciseId", "dbo.Training2Exercise");
            DropForeignKey("dbo.Training2ProgramTraining2Lesson", "Training2Lesson_Training2LessonId", "dbo.Training2Lesson");
            DropForeignKey("dbo.Training2ProgramTraining2Lesson", "Training2Program_Training2ProgramId", "dbo.Training2Program");
            DropForeignKey("dbo.Training2LessonTraining2Exercise", "Training2Exercise_Training2ExerciseId", "dbo.Training2Exercise");
            DropForeignKey("dbo.Training2LessonTraining2Exercise", "Training2Lesson_Training2LessonId", "dbo.Training2Lesson");
            DropIndex("dbo.CommentaryCommentaryTypes", new[] { "CommentaryTypeRefId" });
            DropIndex("dbo.CommentaryCommentaryTypes", new[] { "CommentaryRefId" });
            DropIndex("dbo.TrainingFlightAnnotationsManouvres", new[] { "ManouvreRefId" });
            DropIndex("dbo.TrainingFlightAnnotationsManouvres", new[] { "TrainingFlightAnnotationRefId" });
            DropIndex("dbo.Training2ProgramTraining2Lesson", new[] { "Training2Lesson_Training2LessonId" });
            DropIndex("dbo.Training2ProgramTraining2Lesson", new[] { "Training2Program_Training2ProgramId" });
            DropIndex("dbo.Training2LessonTraining2Exercise", new[] { "Training2Exercise_Training2ExerciseId" });
            DropIndex("dbo.Training2LessonTraining2Exercise", new[] { "Training2Lesson_Training2LessonId" });
            DropIndex("dbo.TrainingFlightAnnotationCommentCommentTypes", new[] { "CommentaryTypeId" });
            DropIndex("dbo.TrainingFlightAnnotationCommentCommentTypes", new[] { "CommentaryId" });
            DropIndex("dbo.TrainingFlightAnnotationCommentCommentTypes", new[] { "TrainingFlightAnnotationId" });
            DropIndex("dbo.AppliedExercises", new[] { "Program_Training2ProgramId" });
            DropIndex("dbo.AppliedExercises", new[] { "Lesson_Training2LessonId" });
            DropIndex("dbo.AppliedExercises", new[] { "Grading_GradingId" });
            DropIndex("dbo.AppliedExercises", new[] { "Exercise_Training2ExerciseId" });
            DropColumn("dbo.Pilots", "InstructorId");
            DropTable("dbo.CommentaryCommentaryTypes");
            DropTable("dbo.TrainingFlightAnnotationsManouvres");
            DropTable("dbo.Training2ProgramTraining2Lesson");
            DropTable("dbo.Training2LessonTraining2Exercise");
            DropTable("dbo.WindSpeeds");
            DropTable("dbo.WindDirections");
            DropTable("dbo.Manouvres");
            DropTable("dbo.TrainingFlightAnnotations");
            DropTable("dbo.TrainingFlightAnnotationCommentCommentTypes");
            DropTable("dbo.CommentaryTypes");
            DropTable("dbo.Commentaries");
            DropTable("dbo.Gradings");
            DropTable("dbo.Training2Program");
            DropTable("dbo.Training2Lesson");
            DropTable("dbo.Training2Exercise");
            DropTable("dbo.AppliedExercises");
        }
    }
}
