using InfoWebApp.Scraper;
using Quartz;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;

namespace InfoWebApp.Scheduler
{
    public class ScraperJob : IJob
    {
        public void Execute(IJobExecutionContext context)
        {
            var cashedArticles = HttpRuntime.Cache.Get("articles") as List<Article>;

            var scraper = new Scraper.Scraper();
            var scrapedArticles = scraper.Scrape().GetAwaiter().GetResult();
            var bot = new TelegramBotClient(ConfigurationManager.AppSettings["telegramBotClientAppKey"]);

            foreach (var article in scrapedArticles)
            {
                if (cashedArticles != null && cashedArticles.Any(a => a.Equals(article))) continue;

                var sb = new StringBuilder();
                sb.Append(article.Title);
                sb.Append(Environment.NewLine + article.Link);

                bot.SendTextMessageAsync(ConfigurationManager.AppSettings["telegramBotClientChanel"], sb.ToString(), ParseMode.Markdown).GetAwaiter().GetResult();

                article.IsSent = true;
                cashedArticles?.Add(article);
            }
        }
    }
}