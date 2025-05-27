using BotApi.Commands;
using BotApi.Interfaces;
using BotApi.Services;
using Microsoft.AspNetCore.Mvc;
using Telegram.Bot.Types;
using static BotApi.Commands.RegisterCommands;

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
        private readonly ClientReplyStateService _clientReplyStateService;

        public WebhookController(ILogger<WebhookController> logger, 
            IWebhookService webhookService, 
            ITelegramBotService bot,
            CommandInvoker commandInvoker,
            ClientReplyStateService clientReplyStateService
        )
        {
            _logger = logger;
            _webhookService = webhookService;
            _bot = bot;
            _commandInvoker = commandInvoker;
            _clientReplyStateService = clientReplyStateService;
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Update update)
        {
            try
            {
                await _bot.SendChatActionAsync(update.Message?.Chat?.Id, Telegram.Bot.Types.Enums.ChatAction.Typing);
                
                var commandTypeTuple = _webhookService.GetCommandType(update);
                
                if (_clientReplyStateService.GetExpectedReply(update.Message?.Chat?.Id) != ExpectedReplyType.None)
                {
                    switch (_clientReplyStateService.GetExpectedReply(update.Message?.Chat?.Id))
                    {
                        case ExpectedReplyType.BandSearch:
                        case ExpectedReplyType.AddBand:
                            commandTypeTuple = (CommandType.LiveConcerts, false);
                            break;
                    }
                }

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