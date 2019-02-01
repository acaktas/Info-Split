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
    public class ApnScraper : BaseScraper
    {
        private readonly ILog _log;

        public ApnScraper(ILog log) : base(new[] { "http://apn.hr/subvencionirani-stambeni-krediti/novosti" })
        {
            _log = log;
        }

        public override Task<List<Article>> GetArticles(string url)
        {
            _log.Info("ApnScraper scraping " + url);

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

                var pageArticles = homePage.Html.CssSelect("section.news").Single().CssSelect("a.news__link").ToArray();
                foreach (var pageArticle in pageArticles)
                {
                    var singlepageArticle = pageArticle.CssSelect("div.news__content").Single();
                    var title = singlepageArticle.CssSelect("p.news__category").Single().InnerText;
                    var link = pageArticle.Attributes["href"].Value;

                    var time = pageArticle.CssSelect("p.news__date").Single();

                    System.Threading.Thread.CurrentThread.CurrentCulture = new CultureInfo("hr-HR");
                    var date = DateTime.Parse(time.InnerText);

                    var shortText = "";
                    var psShortTexts = pageArticle.CssSelect("h3.news__title");
                    shortText = psShortTexts.Aggregate(shortText,
                        (current, psShortText) => current + (psShortText.InnerText + Environment.NewLine));

                    var text = "";
                    try
                    {
                        var homedetailsPage = browser.NavigateToPage(new Uri(link));
                        var psDetails = homedetailsPage.Html.CssSelect("article.wysiwyg > p");
                        text = psDetails.Aggregate(text,
                            (current, psDetail) => current + (psDetail.InnerText + Environment.NewLine)).Replace("&nbsp;", " ");
                    }
                    catch (Exception ex)
                    {
                        _log.Error("ApnScraper error: " + ex.Message + Environment.NewLine + ex.StackTrace);
                    }

                    if (string.IsNullOrWhiteSpace(text))
                    {
                        text = link;
                    }

                    _log.Info("ApnScraper Creating " + title);

                    CreateArticle(text, articles, title, shortText, link, ArticleType.Apn, date);
                }

                return articles;
            });
        }
    }
}
