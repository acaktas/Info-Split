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
    public class HepScraper : BaseScraper
    {
        private readonly ILog _log;
        public HepScraper(ILog log) : base(new[] {"http://www.hep.hr/ods/ostalo/poveznice/bez-struje/19?dp=split"})
        {
            _log = log;
        }
        public override Task<List<Article>> GetArticles(string url)
        {
            _log.Info("HepScraper scraping " + url);

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

                var pageArticle = homePage.Html.CssSelect("div.radwrap").Single();
                var mjestoList = pageArticle.CssSelect("div.mjesto").ToArray();
                var vrijemeList = pageArticle.CssSelect("div.vrijeme").ToArray();
                var link = url;

                for (var i = 0; i < mjestoList.Count(); i++)
                {
                    var title = mjestoList[i].CssSelect("div.grad").Single().InnerText.Replace("&nbsp;", " ")
                        .Replace("&quot;", "\"").Replace("Mjesto: ", "");

                    var shortText = mjestoList[i].InnerText.Replace("&nbsp;", " ")
                        .Replace("&quot;", "\"").Replace('\t'.ToString(), "").Replace("    ", "");

                    shortText += vrijemeList[i].InnerText.Replace("&nbsp;", " ")
                        .Replace("&quot;", "\"").Replace('\t'.ToString(), "").Replace("    ", "");

                    var text = shortText;

                    _log.Info("HepScraper Creating " + title);

                    CreateArticle(text, articles, title, shortText, link, ArticleType.Hep, Convert.ToDateTime(DateTime.Today));
                }

                return articles;
            });
        }
    }
}