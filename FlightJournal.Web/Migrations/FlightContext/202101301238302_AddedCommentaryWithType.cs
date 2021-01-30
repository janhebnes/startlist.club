namespace FlightJournal.Web.Migrations.FlightContext
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedCommentaryWithType : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.Commentaries", "CommentaryTypeId", "dbo.CommentaryTypes");
            DropIndex("dbo.Commentaries", new[] { "CommentaryTypeId" });
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
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.CommentaryCommentaryTypes", "CommentaryTypeRefId", "dbo.CommentaryTypes");
            DropForeignKey("dbo.CommentaryCommentaryTypes", "CommentaryRefId", "dbo.Commentaries");
            DropIndex("dbo.CommentaryCommentaryTypes", new[] { "CommentaryTypeRefId" });
            DropIndex("dbo.CommentaryCommentaryTypes", new[] { "CommentaryRefId" });
            DropTable("dbo.CommentaryCommentaryTypes");
            CreateIndex("dbo.Commentaries", "CommentaryTypeId");
            AddForeignKey("dbo.Commentaries", "CommentaryTypeId", "dbo.CommentaryTypes", "CommentaryTypeId");
        }
    }
}
