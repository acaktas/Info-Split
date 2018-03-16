namespace InfoWebApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class InitialCreate : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Article",
                c => new
                    {
                        ArticleID = c.Int(nullable: false, identity: true),
                        Title = c.String(),
                        ShortText = c.String(),
                        Text = c.String(),
                        Link = c.String(),
                        Date = c.DateTime(nullable: false),
                        Hash = c.Binary(),
                        CreatedDateTime = c.DateTime(nullable: false),
                        UpdateDateTime = c.DateTime(nullable: false),
                        IsAlert = c.Boolean(nullable: false),
                        IsSent = c.Boolean(nullable: false),
                        ArticleType = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.ArticleID);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.Article");
        }
    }
}
