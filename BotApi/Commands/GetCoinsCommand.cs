using BotApi.Interfaces;
using BotApi.Services;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace BotApi.Commands
{
    public class GetCoinsCommand: BaseCommandAsync
    {
        private IGetCoinsService _getCoinsService;

        public GetCoinsCommand(ITelegramBotService telegramBotService, IGetCoinsService getCoinsService) :base(telegramBotService) 
        {
            _getCoinsService = getCoinsService;
        }

        public override async Task ExecuteAsync(Update update)
        {
            var number = update.Message.Text.Substring(update.Message.Text.IndexOf(' ') + 1);
            int.TryParse(number, out int currenciesNumber);
            var result = await _getCoinsService.GetPricesAsync(currenciesNumber);
            if (string.IsNullOrEmpty(result.Item1)) return;
            await _telegramBotService.SendTextMessageAsync(update.Message.Chat.Id, result.Item1, parseMode: ParseMode.Html);
        }
    }
}
