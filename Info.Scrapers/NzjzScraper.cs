using Info.Models;
using log4net;
using ScrapySharp.Extensions;
using ScrapySharp.Network;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Info.Scrapers
{
    public class NzjzScraper : BaseScraper
    {
        private readonly ILog _log;

        public NzjzScraper(ILog log) : base(new[]
        {
            "http://www.nzjz-split.hr/"
        })
        {
            _log = log;
        }

        public override Task<List<Article>> GetArticles(string url)
        {
            _log.Info("NzjzScraper scraping " + url);

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
                    var title = pageArticle.CssSelect("div.entry-header > h2").Single().InnerText.Replace("&nbsp;", " ").Replace("\t", "").Replace("\n", "");
                    var link = "http://www.nzjz-split.hr" + pageArticle.CssSelect("div.entry-header > h2 > a").Single().Attributes["href"].Value;

                    var shortText = pageArticle.CssSelect("p").First().InnerText.Replace("&nbsp;", " ").Replace("\t", "").Replace("\n", "");

                    var time = pageArticle.CssSelect("time").Single().Attributes["datetime"].Value;

                    System.Threading.Thread.CurrentThread.CurrentCulture = new CultureInfo("hr-HR");
                    var date = DateTime.Parse(time);

                    var text = "";
                    try
                    {
                        var homedetailsPage = browser.NavigateToPage(new Uri(link));
                        var psDetails = homedetailsPage.Html.CssSelect("article").Single().SelectNodes("//div[@itemprop='articleBody']/p");
                        text = psDetails.Aggregate(text,
                            (current, psDetail) => current + (psDetail.InnerText + Environment.NewLine)).Replace("&nbsp;", " ");
                    }
                    catch (Exception ex)
                    {
                        _log.Error("NzjzScraper error: " + ex.Message + Environment.NewLine + ex.StackTrace);
                    }

                    if (string.IsNullOrWhiteSpace(text))
                    {
                        text = link;
                    }

                    _log.Info("NzjzScraper Creating " + title);

                    CreateArticle(text, articles, title, shortText, link, ArticleType.Nzjz, date);
                }

                return articles;
            });
        }
    }
}
