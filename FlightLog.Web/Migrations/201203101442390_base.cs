namespace FlightLog.Migrations
{
    using System.Data.Entity.Migrations;
    
    public partial class _base : DbMigration
    {
        public override void Up()
        {
            DropColumn("PilotStatusTypes", "GoFuckYourself");
        }
        
        public override void Down()
        {
            AddColumn("PilotStatusTypes", "GoFuckYourself", c => c.String());
        }
    }
}
