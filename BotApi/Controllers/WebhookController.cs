using BotApi.Commands;
using BotApi.Interfaces;
using Microsoft.AspNetCore.Mvc;
using QBittorrent.Client;
using Telegram.Bot.Types;

namespace BotApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class WebhookController : ControllerBase
    {
        private readonly ILogger<WebhookController> _logger;
        private readonly IWebhookService _webhookService;
        private readonly ITelegramBotService _bot;
        private readonly ICommandProcessingService _commandProcessingService;
        private readonly CommandInvoker _commandInvoker;

        public WebhookController(ILogger<WebhookController> logger, 
            IWebhookService webhookService, 
            ITelegramBotService bot,
            ICommandProcessingService commandProcessingService,
            CommandInvoker commandInvoker
        )
        {
            _logger = logger;
            _webhookService = webhookService;
            _bot = bot;
            _commandProcessingService = commandProcessingService;
            _commandInvoker = commandInvoker;
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Update update)
        {
            await _bot.SendChatActionAsync(update.Message?.Chat?.Id, Telegram.Bot.Types.Enums.ChatAction.Typing);
            var commandTypeTuple = _webhookService.GetCommandType(update);
            await _commandInvoker.ExecuteCommandAsync("/coins", update);
            return Ok(update);
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            await _bot.SendTextMessageAsync(182328439, "GET!!!");
            return Ok();
        }
    }
}