using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;
using Info.Models;

namespace InfoWebApp.DAL
{
    public class ArticleContext : DbContext
    {
        public ArticleContext() : base ("ArticleContext")
        {
            
        }

        public DbSet<Article> Articles { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();
        }
    }
}