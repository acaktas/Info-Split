using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;
using InfoWebApp.Models;
using ScrapySharp.Extensions;
using ScrapySharp.Network;

namespace InfoWebApp.Scraper
{
    public class VikScraper : BaseScraper
    {
        public override Task<List<Article>>[] Scrape()
        {
            var tasks = new Task<List<Article>>[2];
            tasks[0] = GetArticles("http://www.vik-split.hr/aktualnosti/novosti");
            tasks[1] = GetArticles("http://www.vik-split.hr/aktualnosti/obavijesti-o-prekidu-vodoopskrbe");
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

                    CreateArticle(text, articles, title, shortText, link.Value, ArticleType.Vik, date);
                }

                return articles;
            });
        }
    }
}