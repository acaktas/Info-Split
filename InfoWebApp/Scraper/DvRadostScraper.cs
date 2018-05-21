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
    public class DvRadostScraper : BaseScraper
    {
        private readonly ILog _log;

        public DvRadostScraper(ILog log) : base(new[]
        {
            "http://www.vrtic-radost.hr/upisi/obavijesti/"
        })
        {
            _log = log;
        }

        public override Task<List<Article>> GetArticles(string url)
        {
            _log.Info("DvRadostScraper scraping " + url);

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

                var pageArticles = homePage.Html.OwnerDocument.GetElementbyId("post-content").CssSelect("p").ToArray();
                var pageLinks = homePage.Html.OwnerDocument.GetElementbyId("post-content").CssSelect("h3 > a").ToArray();
                for (var i = 0; i < pageArticles.Length; i++)
                {
                    var pageArticle = pageArticles[i];
                    if (pageArticle.InnerHtml.StartsWith("<!--")) continue;

                    var title = pageLinks[i].InnerText.Replace("&nbsp;", " ").Replace("&quot;", "\"");

                    var time = pageArticle.CssSelect("cite").Single().InnerText;
                    var date = Convert.ToDateTime(time);
                    var link = "http://www.vrtic-radost.hr" + pageLinks[i].Attributes["href"].Value;

                    _log.Info("DvRadostScraper Creating " + title);

                    CreateArticle(title + " " + link, articles, title, title, link, ArticleType.DvRadost, date);
                }

                return articles;
            });
        }
    }
}