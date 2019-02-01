using InfoWebApp.DAL;
using InfoWebApp.Hub;
using Info.Models;
using Info.Messengers;
using Microsoft.AspNet.SignalR;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using Info.Scrapers;

namespace InfoWebApp.Scraper
{
    public class Scraper
    {
        static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        static List<IMessenger> _messengersList;

        public async Task Scrape()
        {
            log.Info("Scraper started");

            _messengersList = new List<IMessenger> {
                        new Telegram(log),
                        new Slack(log)
                    };

            var tasks = new List<Task<List<Article>>>();
            tasks.AddRange(new ApnScraper(log).Scrape());
            tasks.AddRange(new NzjzScraper(log).Scrape());
            tasks.AddRange(new VikScraper(log).Scrape());
            tasks.AddRange(new HepScraper(log).Scrape());



            //tasks.AddRange(new DvRadostScraper(log).Scrape());
            //tasks.AddRange(new DvCvitMediteranaScraper(log).Scrape());


            try
            {
                while (tasks.Count > 0)
                {
                    var task = await Task.WhenAny(tasks);
                    tasks.Remove(task);
                    await SaveAndNotifyArticles(task);
                }
            }
            catch (Exception ex)
            {
                log.Error("Scraper error: " + ex.Message + Environment.NewLine + ex.StackTrace);
            }

            log.Info("Scraper ended");
        }

        static async Task SaveAndNotifyArticles(Task<List<Article>> task)
        {
            if (task.Exception != null) return;

            IEnumerable<Article> scrapedArticles = task.Result;

            log.Info("Found " + scrapedArticles.Count() + " from " + scrapedArticles.FirstOrDefault()?.ArticleType);

            using (var articleContext = new ArticleContext())
            {
                var cashedArticles = GetCashedArticles(articleContext);

                foreach (var article in scrapedArticles)
                {
                    if (cashedArticles.Any(a => a.Equals(article)))
                    {
                        log.Info("Skipping article: " + article.Title);
                        continue;
                    }

                    foreach (var messenger in _messengersList)
                    {
                        await messenger.SendMessageAsync(article);
                    }

                    articleContext.Articles.Add(article);

                    log.Info("Added article: " + article.Title);

                    var hub = GlobalHost.ConnectionManager.GetHubContext<ArticleHub>();
                    hub.Clients.All.AddNewMessageToPage("new article", article);
                }

                articleContext.SaveChanges();
            }
        }

        static IList<Article> GetCashedArticles(ArticleContext articleContext)
        {
            var thirtyDaysAgo = DateTime.Now.AddDays(-30);

            var cashedArticles = articleContext.Articles.Where(a =>
                DbFunctions.TruncateTime(a.UpdateDateTime) >= DbFunctions.TruncateTime(thirtyDaysAgo)
                || DbFunctions.TruncateTime(a.CreatedDateTime) >= DbFunctions.TruncateTime(thirtyDaysAgo)).ToList();
            return cashedArticles;
        }
    }
}