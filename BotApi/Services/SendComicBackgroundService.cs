
using BotApi.Database;
using BotApi.Interfaces;
using Telegram.Bot.Types;
using HtmlAgilityPack;
using System.Net;
using System.Security.Cryptography;
using System.Text;

namespace BotApi.Services
{
    public class SendComicBackgroundService : IHostedService, IDisposable
    {
        private readonly ITelegramBotService _telegramBotService;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<SendComicBackgroundService> _logger;
        private Timer? _timer;

        public SendComicBackgroundService(ITelegramBotService telegramBotService, 
            ILogger<SendComicBackgroundService> logger,
            IServiceScopeFactory scopeFactory
            )
        {
            _telegramBotService = telegramBotService;
            _logger = logger;
            _scopeFactory = scopeFactory;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation($"Checking updated comics.");
            // Set up a timer to call CheckTorrentStatus every 5 minutes
            _timer = new Timer(async state => await CheckForUpdatedComics(), null, TimeSpan.Zero, TimeSpan.FromMinutes(30));

        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _timer?.Change(Timeout.Infinite, 0);
            return Task.CompletedTask;
        }

        private async Task CheckForUpdatedComics()
        {
            try
            {
                _logger.LogInformation($"Checking for an updated comics");
                await SendComicsAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in { nameof(SendComicBackgroundService) }: {ex.Message}");
            }
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }



        private async Task SendComicsAsync()
        {
            await SendComicsAsync(SubscriptionType.Oglaf);
            await SendComicsAsync(SubscriptionType.XKCD);
        }

        private async Task SendComicsAsync(SubscriptionType subscriptionType)
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<IDB>();
                var clients = db.GetClientsWithSubscription(subscriptionType);

                MessageToSend message = null;
                switch (subscriptionType)
                {
                    case SubscriptionType.Oglaf:
                        message = await GetOglafPicture();
                        break;
                    case SubscriptionType.XKCD:
                        message = await GetXKCDPicture();
                        break;
                }

                foreach (var client in clients)
                {
                    var chatId = long.Parse(client);
                    var sub = db.GetSubscription(chatId, subscriptionType);

                    var alreadySent = message?.Title.ToHash().Equals(sub?.LastPostedKey);
                    if (alreadySent ?? false) continue;

                    try
                    {
                        sub.LastPostedKey = message?.Title.ToHash();
                        await _telegramBotService.SendTextMessageAsync(chatId, message?.Title?.ToUpper() ?? "");
                        await _telegramBotService.SendTextMessageAsync(chatId, message?.SubTitle ?? "");
                        await _telegramBotService.SendPhotoAsync(chatId, new InputFileUrl(message?.Image ?? ""));
                    }
                    catch (Exception e)
                    {
                        _logger.LogError(e.Message, e);
                    }
                } 
            }
        }

        private async Task<MessageToSend> GetOglafPicture()
        {
            HttpClient httpclient = new HttpClient();
            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            string html = "";
            try
            {
                html = await httpclient.GetStringAsync("http://www.oglaf.com");
            }
            catch (Exception)
            {
                throw;
            }

            HtmlAttributeCollection attrs = null;
            try
            {
                HtmlDocument doc = new HtmlDocument();
                doc.LoadHtml(html);

                List<HtmlNode> imageNodes = null;
                imageNodes = (from HtmlNode node in doc.DocumentNode.SelectNodes("//img")
                              where node.Name == "img"
                              && node.Attributes["id"]?.Value == "strip"
                              select node).ToList();
                attrs = imageNodes[0]?.Attributes;
            }
            catch (Exception)
            {
                throw;
            }

            return new MessageToSend
            {
                Title = attrs["alt"]?.Value.Replace("&quot;", "\"").Replace("&#39;", "'"),
                SubTitle = attrs["title"]?.Value.Replace("&quot;", "\"").Replace("&#39;", "'"),
                Image = attrs["src"]?.Value
            };
        }

        public async Task<MessageToSend> GetXKCDPicture()
        {
            HttpClient httpclient = new HttpClient();
            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            // Use SecurityProtocolType.Ssl3 if needed for compatibility reasons
            string html = "";
            try
            {
                html = await httpclient.GetStringAsync("https://xkcd.com/");

            }
            catch (Exception e)
            {
                _logger.LogError(e.Message, e); 
            }

            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(html);

            List<HtmlNode> imageNodes = null;
            imageNodes = (from HtmlNode node in doc.DocumentNode.SelectNodes("//img")
                          where node.Name == "img"
                          && !string.IsNullOrEmpty(node.Attributes["title"]?.Value)
                          && node.Attributes["src"].Value.Contains("comics")
                          select node).ToList();

            var attrs = imageNodes[0]?.Attributes;

            return new MessageToSend
            {
                Title = attrs["alt"]?.Value.Replace("&quot;", "\"").Replace("&#39;", "'"),
                SubTitle = attrs["title"]?.Value.Replace("&quot;", "\"").Replace("&#39;", "'"),
                Image = "https:" + attrs["src"]?.Value
            };
        }



        public class MessageToSend
        {
            public string? Title;
            public string? SubTitle;
            public string? Image;
        }

    }
    static class StringExtensions
    {
        internal static string ToHash(this string inputString)
        {
            byte[] data = Encoding.UTF8.GetBytes(inputString);
            byte[] hashBytes;

            using (MD5 md5 = MD5.Create())
            {
                hashBytes = md5.ComputeHash(data);
            }

            return BitConverter.ToString(hashBytes).Replace("-", "");
        }
    }
}
