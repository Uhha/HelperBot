using BotApi.Interfaces;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Text;
using System.Text.Json;

namespace BotApi.Services
{
    public class AIChatService : IAIChatService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<AIChatService> _logger;
        private const string LM_STUDIO_URL = "http://192.168.86.26:11230/api/v1/models";
        private const string LM_CHAT_URL = "http://192.168.86.26:11230/api/v1/chat";

        public AIChatService(HttpClient httpClient, ILogger<AIChatService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        /// <summary>
        /// Checks if LM Studio server is available at the configured endpoint.
        /// </summary>
        public async Task<bool> IsServerAvailableAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync(LM_STUDIO_URL);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to check LM Studio server availability");
                return false;
            }
        }

        /// <summary>
        /// Sends a chat prompt to the LM Studio server and returns the response.
        /// </summary>
        public async Task<string> ChatAsync(string prompt)
        {
            try
            {
                var requestContent = new StringContent(
                    JsonSerializer.Serialize(new
                    {
                        model = "qwen/qwen3.5-9B",
                        messages = new[]
                        {
                            new { role = "user", content = prompt }
                        },
                        temperature = 0.7f,
                        max_tokens = 1024,
                        top_p = 0.9f,
                        stream = false,
                        presence_penalty = 0.0f,
                        frequency_penalty = 0.0f,
                        repeat_penalty = 1.0f,
                        n = 1,
                        stop = new[] { "\n\n", "\n", " ", "" },
                        max_context_tokens = 4096,
                        ttl = 900
                    }),
                    Encoding.UTF8,
                    "application/json"
                );

                var response = await _httpClient.PostAsync(LM_CHAT_URL, requestContent);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError($"LM Studio API call failed with status: {response.StatusCode}");
                    return null;
                }

                var jsonResponse = await response.Content.ReadAsStringAsync();
                
                // Extract content from JSON response using simple string manipulation
                // The response format is: {"id":"...","choices":[{"finish_reason":"...","index":0,"message":{"content":"..."}}]}
                var startIndex = jsonResponse.IndexOf("\"content\":\"", StringComparison.OrdinalIgnoreCase);
                if (startIndex == -1)
                {
                    _logger.LogWarning("Could not find content in LM Studio response");
                    return null;
                }

                startIndex += 10; // Skip past "content":"
                var endIndex = jsonResponse.IndexOf("\"", startIndex);
                if (endIndex == -1)
                {
                    _logger.LogWarning("Could not find end of content in LM Studio response");
                    return null;
                }

                return jsonResponse.Substring(startIndex, endIndex - startIndex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during chat with LM Studio");
                return null;
            }
        }

    }
}