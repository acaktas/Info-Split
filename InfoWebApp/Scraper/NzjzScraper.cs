﻿using System;
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
    public class NzjzScraper
    {
        private static string[] _search;

        public NzjzScraper(string[] search)
        {
            _search = search;
        }

        public Task<List<Article>>[] Scrape()
        {
            var tasks = new Task<List<Article>>[2];
            tasks[0] = GetArticles("http://www.nzjz-split.hr/zavod/index.php");
            tasks[1] = GetArticles("http://www.nzjz-split.hr/zavod/index.php/2016-04-26-12-25-06/istakniti-clanci");
            return tasks;
        }

        private static Task<List<Article>> GetArticles(string url)
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

                var pageArticles = homePage.Html.CssSelect("article").ToArray();
                foreach (var pageArticle in pageArticles)
                {
                    var singlepageArticle = pageArticle.CssSelect("h2.article-title > a").Single();
                    var title = singlepageArticle.Attributes["title"].Value;
                    var link = "http://www.nzjz-split.hr" + singlepageArticle.Attributes["href"].Value;

                    var shortText = "";
                    var psShortTexts = pageArticle.CssSelect("section.article-intro > p");
                    shortText = psShortTexts.Aggregate(shortText,
                        (current, psShortText) => current + (psShortText.InnerText + Environment.NewLine));

                    var text = "";
                    try
                    {
                        var homedetailsPage = browser.NavigateToPage(new Uri(link));
                        var psDetails = homedetailsPage.Html.CssSelect("section.article-content > p");
                        text = psDetails.Aggregate(text,
                            (current, psDetail) => current + (psDetail.InnerText + Environment.NewLine)).Replace("&nbsp;", " ");
                    }
                    catch
                    {
                        // ignored
                    }

                    var isAlert = _search.Any(word => text.ToLowerInvariant().Contains(word.ToLowerInvariant()));

                    var hash = Article.GetHash(text);

                    var existingArticle = articles.FirstOrDefault(a => a.Title == title);
                    if (existingArticle != null)
                    {
                        if (StructuralComparisons.StructuralEqualityComparer.Equals(existingArticle.Hash, hash)) continue;

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
                            IsAlert = isAlert,
                            ArticleType = ArticleType.Nzjz
                        });
                    }
                }

                return articles;
            });
        }
    }
}