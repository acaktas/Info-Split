using InfoWebApp.DAL;
using InfoWebApp.Hub;
using InfoWebApp.Models;
using Microsoft.AspNet.SignalR;
using Slack.Webhooks;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

            //tasks.AddRange(new DvRadostScraper(log).Scrape());
            //tasks.AddRange(new DvCvitMediteranaScraper(log).Scrape());

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
            var slack = new SlackClient(ConfigurationManager.AppSettings["slackBotClientChanelUrl"]);
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

                    if (Convert.ToBoolean(ConfigurationManager.AppSettings["telegramBotEnabled"]))
                    {
                        var sb = new StringBuilder();
                        sb.Append(article.ArticleType + " - " + article.Title);
                        sb.Append(Environment.NewLine + article.Link);

                        await bot.SendTextMessageAsync(ConfigurationManager.AppSettings["telegramBotClientChanel"],
                        sb.ToString(),
                        Telegram.Bot.Types.Enums.ParseMode.Markdown);


                        log.Info("Message sent for article: " + article.Title);

                        article.IsSent = true;
                    }

                    if (Convert.ToBoolean(ConfigurationManager.AppSettings["slackBotEnabled"]))
                    {
                        await slack.PostAsync(new SlackMessage()
                        {
                            Channel = ConfigurationManager.AppSettings["slackBotClientChanel"],
                            Text = article.ArticleType.ToString(),
                            Username = "Web Scraping BOT",
                            Attachments = new List<SlackAttachment>()
                            {
                                new SlackAttachment()
                                {
                                    Title =  article.Title,
                                    TitleLink = article.Link,
                                    Color = "warning",
                                    Pretext = article.ShortText
                                }
                            }
                        });
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