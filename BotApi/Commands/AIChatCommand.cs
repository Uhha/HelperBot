using BotApi.Extensions;
using BotApi.Interfaces;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace BotApi.Commands
{
    public class AIChatCommand : BaseCommandAsync
    {
        private readonly IAIChatService _aiChatService;
        private readonly ILogger<AIChatCommand> _logger;

        public AIChatCommand(ITelegramBotService telegramBotService, IAIChatService aiChatService, ILogger<AIChatCommand> logger) : base(telegramBotService)
        {
            _aiChatService = aiChatService;
            _logger = logger;
        }

        public override async Task ExecuteAsync(Update update)
        {
            try
            {
                _logger.LogInformation("AIChat command being executed. Message text: {MessageText}", update.Message?.Text);

                if (update.Message == null || !update.Message.Text.StartsWith("/p"))
                {
                    _logger.LogDebug("Command does not start with /p");
                    return;
                }

                // Extract the prompt after "/p "
                var prompt = update.Message.Text.Substring(2).Trim();
                _logger.LogInformation("Extracted prompt: {Prompt}", prompt);

                if (string.IsNullOrEmpty(prompt))
                {
                    await _telegramBotService.ReplyAsync(update, "Please provide a prompt after /p. Example: /p What is the capital of France?");
                    return;
                }

                // Check if LM Studio server is available
                var isAvailable = await _aiChatService.IsServerAvailableAsync();

                if (!isAvailable)
                {
                    _logger.LogWarning("LM Studio server is not available");
                    await _telegramBotService.ReplyAsync(update, "The AI chat service is currently unavailable. Please try again later.");
                    return;
                }

                // Send the prompt to LM Studio and get the response
                _logger.LogInformation("Sending prompt to LM Studio: {Prompt}", prompt);
                var response = await _aiChatService.ChatAsync(prompt);

                if (string.IsNullOrEmpty(response))
                {
                    _logger.LogWarning("Empty response from AI service");
                    await _telegramBotService.ReplyAsync(update, "Failed to get a response from the AI service.");
                    return;
                }

                _logger.LogInformation("AI Response: {Response}", response);

                // Split long responses into multiple messages for Telegram
                var messageChunks = response.SplitIntoTelegramMessages();

                if (messageChunks.Count > 1)
                {
                    // Send first chunk immediately
                    await _telegramBotService.SendTextMessageAsync(
                        update.Message.Chat.Id, 
                        messageChunks[0], 
                        parseMode: ParseMode.Html);

                    // Send remaining chunks with a small delay between each
                    for (int i = 1; i < messageChunks.Count; i++)
                    {
                        await Task.Delay(500); // Small delay to avoid rate limiting
                        await _telegramBotService.SendTextMessageAsync(
                            update.Message.Chat.Id, 
                            messageChunks[i], 
                            parseMode: ParseMode.Html);
                    }

                    _logger.LogInformation("Sent {Count} message chunks for AI response", messageChunks.Count);
                }
                else
                {
                    // Single message - send as before
                    await _telegramBotService.SendTextMessageAsync(
                        update.Message.Chat.Id, 
                        response, 
                        parseMode: ParseMode.Html);
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "AI Response Exception");
                throw;
            }
        }
    }
}
