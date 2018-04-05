using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using InfoWebApp.Models;
using ScrapySharp.Extensions;
using ScrapySharp.Network;

namespace InfoWebApp.Scraper
{
    public class APNScraper : BaseScraper
    {
        public APNScraper() : base(new[] { "http://apn.hr/subvencionirani-stambeni-krediti/novosti" })
        { }

        public override Task<List<Article>> GetArticles(string url)
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

                var pageArticles = homePage.Html.CssSelect("section.news").Single().CssSelect("a.news__link").ToArray();
                foreach (var pageArticle in pageArticles)
                {
                    var singlepageArticle = pageArticle.CssSelect("div.news__content").Single();
                    var title = singlepageArticle.CssSelect("p.news__category").Single().InnerText;
                    var link = pageArticle.Attributes["href"].Value;

                    var time = pageArticle.CssSelect("p.news__date").Single();
                    var date = Convert.ToDateTime(time.InnerText);

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
                    catch
                    {
                        // ignored
                    }

                    CreateArticle(text, articles, title, shortText, link, ArticleType.Apn, date);
                }

                return articles;
            });
        }
    }
}