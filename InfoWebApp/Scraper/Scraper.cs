using InfoWebApp.DAL;
using InfoWebApp.Hub;
using InfoWebApp.Models;
using Microsoft.AspNet.SignalR;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI.WebControls.Expressions;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;

namespace InfoWebApp.Scraper
{
    public class Scraper
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        public async Task Scrape()
        {

            log.Info("Scraper started");

            var tasks = new List<Task<List<Article>>>();
            tasks.AddRange(new ApnScraper(log).Scrape());
            tasks.AddRange(new NzjzScraper(log).Scrape());
            tasks.AddRange(new VikScraper(log).Scrape());
            tasks.AddRange(new HepScraper(log).Scrape());

            try
            {
                while (tasks.Count > 0)
                {
                    var task = await Task.WhenAny(tasks);
                    tasks.Remove(task);
                    SaveAndNotifyArticles(task);
                }
            }
            catch (Exception ex)
            {
                log.Error("Scraper error: " + ex.Message + Environment.NewLine + ex.StackTrace);
            }

            log.Info("Scraper ended");
        }

        private static async void SaveAndNotifyArticles(Task<List<Article>> task)
        {
            if (task.Exception != null) return;

            IEnumerable<Article> scrapedArticles = task.Result;

            log.Info("Found " + scrapedArticles.Count() + " from " + scrapedArticles.FirstOrDefault()?.ArticleType);

            var bot = new TelegramBotClient(ConfigurationManager.AppSettings["telegramBotClientAppKey"]);
            var hub = GlobalHost.ConnectionManager.GetHubContext<ArticleHub>();

            var thirtyDaysAgo = DateTime.Now.AddDays(-30);

            using (var articleContext = new ArticleContext())
            {
                var cashedArticles = articleContext.Articles.Where(a =>
                    DbFunctions.TruncateTime(a.UpdateDateTime) >= DbFunctions.TruncateTime(thirtyDaysAgo)
                    || DbFunctions.TruncateTime(a.CreatedDateTime) >= DbFunctions.TruncateTime(thirtyDaysAgo)).ToList();

                foreach (var article in scrapedArticles)
                {
                    if (cashedArticles.Any(a => a.Equals(article)))
                    {
                        log.Info("Skipping article: " + article.Title);
                        continue;
                    }

                    var sb = new StringBuilder();
                    sb.Append(article.ArticleType + " - " + article.Title);
                    sb.Append(Environment.NewLine + article.Link);

                    if (Convert.ToBoolean(ConfigurationManager.AppSettings["telegramBotEnabled"]))
                    {
                        await bot.SendTextMessageAsync(ConfigurationManager.AppSettings["telegramBotClientChanel"],
                            sb.ToString(),
                            ParseMode.Markdown);

                        log.Info("Message sent for article: " + article.Title);

                        article.IsSent = true;
                    }

                    articleContext.Articles.Add(article);

                    log.Info("Added article: " + article.Title);

                    hub.Clients.All.AddNewMessageToPage("new article", article);
                }

                articleContext.SaveChanges();
            }
        }
    }
}
