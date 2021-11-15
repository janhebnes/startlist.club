namespace FlightJournal.Web.Migrations.FlightContext
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AppliedExercisesextendedwithindexIX_AppliedExercises_FlightId : DbMigration
    {
        public override void Up()
        {
            Sql("CREATE NONCLUSTERED INDEX [IX_AppliedExercises_FlightId] ON [dbo].[AppliedExercises] ([FlightId])");
        }
        
        public override void Down()
        {
            DropIndex("dbo.AppliedExercises", "IX_AppliedExercises_FlightId");
        }
    }
}
