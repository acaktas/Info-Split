using Info.Messengers;
using Info.Models;
using log4net;
using System;
using System.Configuration;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;

namespace Info.Messengers
{
    public class Telegram : IMessenger
    {
        readonly ILog _log;

        static readonly bool _isTelegramEnabled = Convert.ToBoolean(ConfigurationManager.AppSettings["telegramBotEnabled"]);
        static readonly string _telegramAppKey = ConfigurationManager.AppSettings["telegramBotClientAppKey"];
        static readonly string _telegramCahnel = ConfigurationManager.AppSettings["telegramBotClientChanel"];

        public Telegram(ILog log)
        {
            _log = log;
        }

        public async Task SendMessageAsync(Article article)
        {
            if (_isTelegramEnabled)
            {
                var telegram = new TelegramBotClient(_telegramAppKey);

                var sb = new StringBuilder();
                sb.Append(article.ArticleType + " - " + article.Title);
                sb.Append(Environment.NewLine + article.Link);

                await telegram.SendTextMessageAsync(_telegramCahnel,
                    sb.ToString(),
                    ParseMode.Markdown);

                _log.Info("Message to Telegram sent for article: " + article.Title);

                article.IsSent = true;
            }
        }
    }
}