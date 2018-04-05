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
    public class NzjzScraper : BaseScraper
    {
        private readonly ILog _log;

        public NzjzScraper(ILog log) : base(new[]
        {
            "http://www.nzjz-split.hr/zavod/index.php",
            "http://www.nzjz-split.hr/zavod/index.php/2016-04-26-12-25-06/istakniti-clanci"
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
                    catch (Exception ex)
                    {
                        _log.Error("NzjzScraper error: " + ex.Message + Environment.NewLine + ex.StackTrace);
                    }

                    _log.Info("NzjzScraper Creating " + title);

                    CreateArticle(text, articles, title, shortText, link, ArticleType.Nzjz, Convert.ToDateTime(DateTime.Today));
                }

                return articles;
            });
        }
    }
}