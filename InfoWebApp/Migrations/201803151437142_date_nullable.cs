namespace InfoWebApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class date_nullable : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Article", "Date", c => c.DateTime());
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Article", "Date", c => c.DateTime(nullable: false));
        }
    }
}
