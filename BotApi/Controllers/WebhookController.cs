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
        private readonly CommandInvoker _commandInvoker;

        public WebhookController(ILogger<WebhookController> logger, 
            IWebhookService webhookService, 
            ITelegramBotService bot,
            CommandInvoker commandInvoker
        )
        {
            _logger = logger;
            _webhookService = webhookService;
            _bot = bot;
            _commandInvoker = commandInvoker;
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Update update)
        {
            try
            {
                await _bot.SendChatActionAsync(update.Message?.Chat?.Id, Telegram.Bot.Types.Enums.ChatAction.Typing);
                var commandTypeTuple = _webhookService.GetCommandType(update);
                
                _logger.LogInformation($"Commnad {commandTypeTuple.command} being executed.");

                Task.Run(async () =>
                {
                    await _commandInvoker.ExecuteCommandAsync(commandTypeTuple.command, update);
                });

                return Ok(update);
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Error On Command Post", null);
                return StatusCode(500);
            }
        }

       
    }
}