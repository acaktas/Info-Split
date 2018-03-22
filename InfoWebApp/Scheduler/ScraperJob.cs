using InfoWebApp.DAL;
using InfoWebApp.Hub;
using Microsoft.AspNet.SignalR;
using Quartz;
using System;
using System.Configuration;
using System.Data.Entity;
using System.Linq;
using System.Text;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;

namespace InfoWebApp.Scheduler
{
    public class ScraperJob : IJob
    {
        public void Execute(IJobExecutionContext context)
        {
            using (var articleContext = new ArticleContext())
            {
                var scraper = new Scraper.Scraper();
                var scrapedArticles = scraper.Scrape().GetAwaiter().GetResult();
                var bot = new TelegramBotClient(ConfigurationManager.AppSettings["telegramBotClientAppKey"]);
                var hub = GlobalHost.ConnectionManager.GetHubContext<ArticleHub>();

                var thirtyDaysAgo = DateTime.Now.AddDays(-30);

                var cashedArticles = articleContext.Articles.Where(a =>
                    DbFunctions.TruncateTime(a.UpdateDateTime) >= DbFunctions.TruncateTime(thirtyDaysAgo)
                    || DbFunctions.TruncateTime(a.CreatedDateTime) >= DbFunctions.TruncateTime(thirtyDaysAgo)).ToList();

                foreach (var article in scrapedArticles)
                {
                    if (cashedArticles.Any(a => a.Equals(article))) continue;

                    var sb = new StringBuilder();
                    sb.Append(article.ArticleType + " - " + article.Title);
                    sb.Append(Environment.NewLine + article.Link);

                    if (Convert.ToBoolean(ConfigurationManager.AppSettings["telegramBotEnabled"]))
                    {
                        bot.SendTextMessageAsync(ConfigurationManager.AppSettings["telegramBotClientChanel"],
                            sb.ToString(),
                            ParseMode.Markdown).GetAwaiter().GetResult();

                        article.IsSent = true;
                    }

                    articleContext.Articles.Add(article);
                    
                    hub.Clients.All.AddNewMessageToPage("new article", article);
                }

                articleContext.SaveChanges();
            }
        }
    }
}