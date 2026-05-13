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
                    JsonSerializer.Serialize<LLMRequest>(new LLMRequest
                    {
                        model = "qwen/qwen3.5-9b",
                        input = prompt
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


        public class LLMRequest
        {
            public string model { get; set; }
            public string input { get; set; }
            //public Integration[] integrations { get; set; }
            //public int context_length { get; set; }
            //public int temperature { get; set; }
        }

        public class Integration
        {
            public string type { get; set; }
            public string server_label { get; set; }
            public string server_url { get; set; }
            public string[] allowed_tools { get; set; }
            public string id { get; set; }
        }

        public class LLMResponse
        {
            //public string model_instance_id { get; set; }
            public Output[] output { get; set; }
            //public Stats stats { get; set; }
            //public string response_id { get; set; }
        }

        public class Stats
        {
            public int input_tokens { get; set; }
            public int total_output_tokens { get; set; }
            public int reasoning_output_tokens { get; set; }
            public float tokens_per_second { get; set; }
            public float time_to_first_token_seconds { get; set; }
            public float model_load_time_seconds { get; set; }
        }

        public class Output
        {
            public string type { get; set; }
            public string tool { get; set; }
            public Arguments arguments { get; set; }
            public string output { get; set; }
            public Provider_Info provider_info { get; set; }
            public string content { get; set; }
        }

        public class Arguments
        {
            public string sort { get; set; }
            public string query { get; set; }
            public int limit { get; set; }
            public string url { get; set; }
        }

        public class Provider_Info
        {
            public string server_label { get; set; }
            public string type { get; set; }
            public string plugin_id { get; set; }
        }



    }
}