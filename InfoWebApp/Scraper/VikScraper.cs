using HtmlAgilityPack;
using InfoWebApp.Models;
using ScrapySharp.Extensions;
using ScrapySharp.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using log4net;

namespace InfoWebApp.Scraper
{
    public class VikScraper : BaseScraper
    {
        private readonly ILog _log;

        public VikScraper(ILog log) : base(new[]
        {
            "http://www.vik-split.hr/aktualnosti/novosti",
            "http://www.vik-split.hr/aktualnosti/obavijesti-o-prekidu-vodoopskrbe",
            "http://www.vik-split.hr/aktualnosti/mutnoca-vode"
        })
        {
            _log = log;
        }

        public override Task<List<Article>> GetArticles(string url)
        {
            _log.Info("VikScraper scraping " + url);

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
                    catch (Exception ex)
                    {
                        _log.Error("VikScraper error: " + ex.Message + Environment.NewLine + ex.StackTrace);
                    }

                    _log.Info("VikScraper Creating " + title);

                    CreateArticle(text, articles, title, shortText, link.Value, ArticleType.Vik, date);
                }

                var documentsTable = homePage.Html.CssSelect("div.DNN_Documents > table > tr").ToArray().Skip(1);
                foreach (var document in documentsTable)
                {
                    var title = document.CssSelect("td.NaslovCell > a").Single().InnerText.Replace("&nbsp;", " ");
                    var text = "";
                    var shortText = title;
                    var link = "http://www.vik-split.hr/" + document.CssSelect("td.DownloadCell > a").Single().Attributes["href"].Value;

                    CreateArticle(text, articles, title, shortText, link, ArticleType.Vik, Convert.ToDateTime(DateTime.Today));
                }

                return articles;
            });
        }
    }
}