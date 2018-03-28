using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InfoWebApp.Models;
using ScrapySharp.Extensions;
using ScrapySharp.Network;

namespace InfoWebApp.Scraper
{
    public class HepScraper : IScraper
    {
        private readonly string[] _search;

        public HepScraper(string[] search)
        {
            _search = search;
        }

        public Task<List<Article>>[] Scrape()
        {
            var tasks = new Task<List<Article>>[1];
            tasks[0] = GetArticles("http://www.hep.hr/ods/ostalo/poveznice/bez-struje/19?dp=split");
            return tasks;
        }

        private Task<List<Article>> GetArticles(string url)
        {
            return Task.Run(() =>
            {
                var articles = new List<Article>();

                var browser = new ScrapingBrowser
                {
                    AllowAutoRedirect = true,
                    AllowMetaRedirect = true,
                    Encoding = Encoding.UTF8
                };
                var homePage = browser.NavigateToPage(new Uri(url));

                var pageArticle = homePage.Html.CssSelect("div.radwrap").Single();
                var mjestoList = pageArticle.CssSelect("div.mjesto").ToArray();
                var vrijemeList = pageArticle.CssSelect("div.vrijeme").ToArray();
                string title;
                var link = url;
                var date = Convert.ToDateTime(DateTime.Today);

                for (var i = 0; i < mjestoList.Count(); i++)
                {
                    title = mjestoList[i].CssSelect("div.grad").Single().InnerText.Replace("&nbsp;", " ")
                        .Replace("&quot;", "\"").Replace("Mjesto: ", "");

                    var shortText = mjestoList[i].InnerText.Replace("&nbsp;", " ")
                        .Replace("&quot;", "\"").Replace('\t'.ToString(), "").Replace("    ", "");

                    shortText += vrijemeList[i].InnerText.Replace("&nbsp;", " ")
                        .Replace("&quot;", "\"").Replace('\t'.ToString(), "").Replace("    ", "");

                    var text = shortText;

                    var isAlert = _search.Any(word => text.ToLowerInvariant().Contains(word.ToLowerInvariant()));

                    var hash = Article.GetHash(text);

                    var existingArticle = articles.FirstOrDefault(a => a.Date?.Date == date.Date && a.Title == title);
                    if (existingArticle != null)
                    {
                        if (StructuralComparisons.StructuralEqualityComparer.Equals(existingArticle.Hash, hash))
                            continue;

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
                            Date = date,
                            CreatedDateTime = DateTime.Now,
                            Text = text,
                            UpdateDateTime = DateTime.Now,
                            IsAlert = isAlert,
                            ArticleType = ArticleType.Hep
                        });
                    }
                }

                return articles;
            });
        }
    }
}