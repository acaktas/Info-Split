using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using InfoWebApp.Models;

namespace InfoWebApp.Scraper
{
    public abstract class BaseScraper
    {
        public readonly string[] Search = { "Sućidar", "Ivaniševićeva", "Drage Ivaniševića" };
        public abstract Task<List<Article>>[] Scrape();

        protected void CreateArticle(string text, List<Article> articles, string title,
            string shortText, string link, ArticleType articleType, DateTime date)
        {
            var isAlert = Search.Any(word => text.ToLowerInvariant().Contains(word.ToLowerInvariant()));
            var hash = Article.GetHash(text);

            var existingArticle = articles.FirstOrDefault(a => a.Date?.Date == date.Date && a.Title == title);
            if (existingArticle != null)
            {
                if (StructuralComparisons.StructuralEqualityComparer.Equals(existingArticle.Hash, hash)) return;

                existingArticle.Text = text;
                existingArticle.UpdateDateTime = DateTime.Now;
                existingArticle.IsAlert = isAlert;
            }
            else
            {
                articles.Add(new Article()
                {
                    Title = title,
                    ShortText = shortText,
                    Link = link,
                    CreatedDateTime = DateTime.Now,
                    Text = text,
                    UpdateDateTime = DateTime.Now,
                    Date = date,
                    IsAlert = isAlert,
                    ArticleType = articleType
                });
            }
        }
    }
}
