namespace FlightJournal.Web.Migrations.FlightContext
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Addedexportidfiedstotrainingmodels : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Training2Exercise", "ExerciseIdForExport", c => c.Guid(nullable: false));
            AddColumn("dbo.Training2Lesson", "LessonIdForExport", c => c.Guid(nullable: false));
            AddColumn("dbo.Training2Program", "ProgramIdForExport", c => c.Guid(nullable: false));
            AddColumn("dbo.Training2Program", "Version", c => c.String());
            AddColumn("dbo.Commentaries", "CommentaryIdForExport", c => c.Guid(nullable: false));
            AddColumn("dbo.CommentaryTypes", "CommentaryTypeIdForExport", c => c.Guid(nullable: false));
            AddColumn("dbo.Manouvres", "ManouvreIdForExport", c => c.Guid(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Manouvres", "ManouvreIdForExport");
            DropColumn("dbo.CommentaryTypes", "CommentaryTypeIdForExport");
            DropColumn("dbo.Commentaries", "CommentaryIdForExport");
            DropColumn("dbo.Training2Program", "Version");
            DropColumn("dbo.Training2Program", "ProgramIdForExport");
            DropColumn("dbo.Training2Lesson", "LessonIdForExport");
            DropColumn("dbo.Training2Exercise", "ExerciseIdForExport");
        }
    }
}
