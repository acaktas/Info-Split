using HtmlAgilityPack;
using ScrapySharp.Extensions;
using ScrapySharp.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InfoWebApp.Scraper
{
    public class Scraper
    {
        private static readonly string[] Search = { "Sućidar", "Ivaniševićeva", "Drage Ivaniševića" };

        public async Task<List<Article>> Scrape()
        {
            var tasks = new Task<List<Article>>[4];
            tasks[0] = GetVikArticles("http://www.vik-split.hr/aktualnosti/novosti");
            tasks[1] = GetVikArticles("http://www.vik-split.hr/aktualnosti/obavijesti-o-prekidu-vodoopskrbe");
            tasks[2] = GetNzjzArticles("http://www.nzjz-split.hr/zavod/index.php");
            tasks[3] = GetNzjzArticles("http://www.nzjz-split.hr/zavod/index.php/2016-04-26-12-25-06/istakniti-clanci");

            Task.WaitAll(tasks);

            var articles = new List<Article>();
            foreach (var task in tasks)
            {
                articles.AddRange(task.Result);
            }

            return articles;
        }

        private static Task<List<Article>> GetNzjzArticles(string url)
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

                    var isAlert = Search.Any(word => text.ToLowerInvariant().Contains(word.ToLowerInvariant()));

                    var hash = Article.GetHash(text);

                    var existingArticle = articles.FirstOrDefault(a => a.Title == title);
                    if (existingArticle != null)
                    {
                        if (existingArticle.Hash == hash) continue;

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
                            IsAlert = isAlert
                        });
                    }
                }

                return articles;
            });
        }
        private static Task<List<Article>> GetVikArticles(string url)
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

                var pageArticles = homePage.Html.CssSelect("article.latest-news--announce").ToArray();
                foreach (var pageArticle in pageArticles)
                {
                    var ps = pageArticle.CssSelect("div.hentry__content > p");
                    var title = pageArticle.CssSelect("h2.hentry__title > a").Single().InnerText.Replace("&nbsp;", " ")
                        .Replace("&quot;", "\"");
                    var time = pageArticle.CssSelect("time.latest-news__date").Single().Attributes["datetime"];
                    var date = Convert.ToDateTime(time.Value);
                    var htmlNodes = ps as HtmlNode[] ?? ps.ToArray();
                    var shortText = htmlNodes.FirstOrDefault()?.InnerText.Replace("&nbsp;", " ")
                        .Replace("&quot;", "\"");
                    var link = htmlNodes.CssSelect("a.more-link").Single().Attributes["href"];

                    var text = "";

                    try
                    {
                        var homedetailsPage = browser.NavigateToPage(new Uri(link.Value));
                        var psDetails = homedetailsPage.Html.CssSelect("div.hentry__content > p");
                        text = psDetails.Aggregate(text,
                                (current, psDetail) => current + (psDetail.InnerText + Environment.NewLine))
                            .Replace("&nbsp;", " ").Replace("&quot;", "\"");
                    }
                    catch
                    {
                        // ignored
                    }

                    var isAlert = Search.Any(word => text.ToLowerInvariant().Contains(word.ToLowerInvariant()));

                    var hash = Article.GetHash(text);

                    var existingArticle = articles.FirstOrDefault(a => a.Date.Date == date.Date && a.Title == title);
                    if (existingArticle != null)
                    {
                        if (existingArticle.Hash == hash) continue;

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
                            Link = link.Value,
                            Date = date,
                            CreatedDateTime = DateTime.Now,
                            Text = text,
                            UpdateDateTime = DateTime.Now,
                            IsAlert = isAlert
                        });
                    }
                }

                return articles;
            });
        }
    }
}
