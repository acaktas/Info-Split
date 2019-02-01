using Models;
using log4net;
using Slack.Webhooks;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Threading.Tasks;

namespace Messengers
{
    public class Slack : IMessenger
    {
        readonly ILog _log;

        static readonly bool _isSlackEnabled = Convert.ToBoolean(ConfigurationManager.AppSettings["slackBotEnabled"]);
        static readonly string _slackChanelUrl = ConfigurationManager.AppSettings["slackBotClientChanelUrl"];
        static readonly string _slackChanel = ConfigurationManager.AppSettings["slackBotClientChanel"];

        public Slack(ILog log)
        {
            _log = log;
        }

        public async Task SendMessageAsync(Article article)
        {
            if (_isSlackEnabled)
            {
                var slack = new SlackClient(_slackChanelUrl);

                await slack.PostAsync(new SlackMessage()
                {
                    Channel = _slackChanel,
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

                _log.Info("Message to Slack sent for article: " + article.Title);

                article.IsSent = true;
            }
        }
    }
}
