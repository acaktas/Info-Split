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
    public class DvCvitMediteranaScraper : BaseScraper
    {
        private readonly ILog _log;

        public DvCvitMediteranaScraper(ILog log) : base(new[]
        {
            "http://cvit-mediterana.hr/upisi-u-djecji-vrtic/"
        })
        {
            _log = log;
        }

        public override Task<List<Article>> GetArticles(string url)
        {
            _log.Info("DvCvitMediteranaScraper scraping " + url);

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

                var pageArticles = homePage.Html.CssSelect("div.post");
                foreach (var pageArticle in pageArticles)
                {
                    var title = pageArticle.CssSelect("h2 > a").Single().InnerText.Replace("&nbsp;", " ").Replace("&quot;", "\"");

                    var shortText = pageArticle.CssSelect("div.post-text").Single().InnerText;

                    var time = pageArticle.CssSelect("div.post-text > p").First().InnerText;
                    var firstChar = time.Trim().ToCharArray().ElementAt(0);
                    var date = DateTime.Now;
                    if (!string.IsNullOrWhiteSpace(time) && char.IsDigit(firstChar))
                    {
                        date = Convert.ToDateTime(time);
                    }

                    var link = pageArticle.CssSelect("h2 > a").Single().Attributes["href"].Value;

                    var text = "";
                    try
                    {
                        var homedetailsPage = browser.NavigateToPage(new Uri(link));
                        var psDetails = homedetailsPage.Html.CssSelect("div.post");
                        text = psDetails.Aggregate(text,
                            (current, psDetail) => current + (psDetail.InnerText + Environment.NewLine)).Replace("&nbsp;", " ");
                    }
                    catch (Exception ex)
                    {
                        _log.Error("NzjzScraper error: " + ex.Message + Environment.NewLine + ex.StackTrace);
                    }


                    _log.Info("DvCvitMediteranaScraper Creating " + title);

                    CreateArticle(text, articles, title, shortText, link, ArticleType.DvCvitMediterana, date);
                }

                return articles;
            });
        }
    }
}