using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Caching;
using InfoWebApp.Scraper;
using Quartz;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;

namespace InfoWebApp.Scheduler
{
    public class ScraperJob : IJob
    {
        public async Task Execute(IJobExecutionContext context)
        {
            var cash = new Cache();
            var articlesList = cash.Get("articles") as List<Article>;

            var scraper = new Scraper.Scraper();
            var articles = await scraper.Scrape();
            var bot = new TelegramBotClient(ConfigurationSettings.AppSettings["telegramBotClientAppKey"]);

            foreach (var article in articles)
            {
                if (articlesList.Any(a => a.Equals(article))) continue;

                var sb = new StringBuilder();
                sb.Append(article.Title);
                sb.Append(Environment.NewLine + article.Link);

                await bot.SendTextMessageAsync(ConfigurationSettings.AppSettings["telegramBotClientChanel"], sb.ToString(), ParseMode.Markdown);

                article.IsSent = true;
                articlesList.Add(article);
            }
        }
    }
}