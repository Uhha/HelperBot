using BotApi.Interfaces;
using BotApi.Services;
using Microsoft.AspNetCore.Mvc;
using Telegram.Bot.Types;

namespace BotApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class WebhookController : ControllerBase
    {
        private readonly ILogger<WebhookController> _logger;
        private readonly IWebhookService _webhookService;
        private readonly IBot _bot;
        private readonly ICommandProcessingService _commandProcessingService;

        public WebhookController(ILogger<WebhookController> logger, 
            IWebhookService webhookService, 
            IBot bot,
            ICommandProcessingService commandProcessingService
            )
        {
            _logger = logger;
            _webhookService = webhookService;
            _bot = bot;
            _commandProcessingService = commandProcessingService;
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Update update)
        {
            await _bot.SendChatActionAsync(update.Message?.Chat?.Id, Telegram.Bot.Types.Enums.ChatAction.Typing);
            var commandTypeTuple = _webhookService.GetCommandType(update);
            await _commandProcessingService.ProcessCommandAsync(commandTypeTuple.command, commandTypeTuple.isCallback, update, _bot);
            return Ok(update);
        }
    }
}