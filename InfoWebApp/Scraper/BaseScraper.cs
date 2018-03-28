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
        private readonly string[] _links;

        protected BaseScraper(string[] links)
        {
            _links = links;
        }

        public abstract Task<List<Article>> GetArticles(string url);

        public Task<List<Article>>[] Scrape()
        {
            var tasks = new Task<List<Article>>[_links.Length];
            for (var i = 0; i < _links.Length; i++)
            {
                tasks[i] = GetArticles(_links[i]);
            }
            return tasks;
        }

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
