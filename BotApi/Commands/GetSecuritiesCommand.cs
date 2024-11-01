using BotApi.Interfaces;
using BotApi.Services;
using System.Text;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace BotApi.Commands
{
    public class GetSecuritiesCommand : BaseCommandAsync
    {
        private ISecuritiesService _securitiesService;

        public GetSecuritiesCommand(ITelegramBotService telegramBotService, ISecuritiesService securitiesService) :base(telegramBotService) 
        {
            _securitiesService = securitiesService;
        }

        public override async Task ExecuteAsync(Update update)
        {
            var chatId = update?.Message?.Chat.Id;

            if (chatId == null || chatId == 0)
                return;

            var pricesList = await _securitiesService.GetPricesAsync(chatId ?? 0);
            if (pricesList.Count == 0)
            {
                await _telegramBotService.SendTextMessageAsync(chatId ?? 0, "No tracked securities.");
                return;
            }

            await _telegramBotService.SendTextMessageAsync(chatId ?? 0, string.Join(Environment.NewLine, pricesList), ParseMode.Html);
        }
    }

    public class AddSecurityCommand : BaseCommandAsync
    {
        private ISecuritiesService _securitiesService;

        public AddSecurityCommand(ITelegramBotService telegramBotService, ISecuritiesService securitiesService) : base(telegramBotService)
        {
            _securitiesService = securitiesService;
        }

        public override async Task ExecuteAsync(Update update)
        {
            var chatId = update?.Message?.Chat.Id;
            var symbol = update.Message.Text.Substring(update.Message.Text.IndexOf(' ') + 1);


            if (chatId == null || chatId == 0)
                return;
            if (string.IsNullOrEmpty(symbol))
                return;

            var success = _securitiesService.AddSecurity(chatId ?? 0, symbol);
            if (!success)
            {
                await _telegramBotService.SendTextMessageAsync(chatId ?? 0, "Errorr adding security.");
                return;
            }

            await _telegramBotService.SendTextMessageAsync(chatId ?? 0, $"{symbol} added.");
        }
    }

    public class RemoveSecurityCommand : BaseCommandAsync
    {
        private ISecuritiesService _securitiesService;

        public RemoveSecurityCommand(ITelegramBotService telegramBotService, ISecuritiesService securitiesService) : base(telegramBotService)
        {
            _securitiesService = securitiesService;
        }

        public override async Task ExecuteAsync(Update update)
        {
            var chatId = update?.Message?.Chat.Id;
            var symbol = update.Message.Text.Substring(update.Message.Text.IndexOf(' ') + 1);

            if (chatId == null || chatId == 0)
                return;
            if (string.IsNullOrEmpty(symbol))
                return;

            var success = _securitiesService.RemoveSecurity(chatId ?? 0, symbol);
            if (!success)
            {
                await _telegramBotService.SendTextMessageAsync(chatId ?? 0, "Errorr removing security.");
                return;
            }

            await _telegramBotService.SendTextMessageAsync(chatId ?? 0, $"{symbol} removed.");
        }
    }

    public class ListSecuritiesCommand : BaseCommandAsync
    {
        private ISecuritiesService _securitiesService;

        public ListSecuritiesCommand(ITelegramBotService telegramBotService, ISecuritiesService securitiesService) : base(telegramBotService)
        {
            _securitiesService = securitiesService;
        }

        public override async Task ExecuteAsync(Update update)
        {
            var chatId = update?.Message?.Chat.Id;
            var symbol = update.Message.Text.Substring(update.Message.Text.IndexOf(' ') + 1);

            if (chatId == null || chatId == 0)
                return;
            if (string.IsNullOrEmpty(symbol))
                return;

            var symbols = _securitiesService.ListSecurities(chatId ?? 0);
            if (symbols == null)
            {
                await _telegramBotService.SendTextMessageAsync(chatId ?? 0, "List is Empty.");
            }
            else if (symbols.Count == 0)
            {
                await _telegramBotService.SendTextMessageAsync(chatId ?? 0, "List is Empty.");
            }
            else
            {
                var message = CreateMessage(symbols);
                await _telegramBotService.SendTextMessageAsync(chatId ?? 0, message);
                return;
            }
        }

        private string CreateMessage(IList<string> symbols)
        {
            StringBuilder sb = new StringBuilder();

            foreach (var symbol in symbols)
            {
                sb.Append(symbol + Environment.NewLine);
            }
            return sb.ToString();
        }
    }
}
