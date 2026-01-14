using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using TaskManagementPlatform.Models;

namespace TaskManagementPlatform.Services
{
    public class AIResponseResult
    {
        public string Content { get; set; }
        public bool Success { get; set; } = false;
        public string? ErrorMessage { get; set; }
    }

    public interface IOpenAIService
    {
        Task<AIResponseResult> GenerateProjectSummaryAsync(Project project);
    }

    public class OpenAIService : IOpenAIService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private readonly ILogger<OpenAIService> _logger;

        public OpenAIService(IConfiguration configuration, ILogger<OpenAIService> logger)
        {
            _httpClient = new HttpClient();
            _apiKey = configuration["OpenAI:ApiKey"] ?? throw new ArgumentNullException("OpenAI:ApiKey not configured");
            _logger = logger;

            _httpClient.BaseAddress = new Uri("https://api.openai.com/v1/");
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public async Task<AIResponseResult> GenerateProjectSummaryAsync(Project project)
        {
            try
            {
                var systemPrompt = @"You are a professional project manager API. 
Summarize the current status of the project based on the provided details.
Identify risks (overdue tasks) and progress.
Keep it concise (max 3-4 sentences).";

                var tasksInfo = string.Join("; ", project.Tasks.Select(t => $"{t.Title} (Status: {t.Status}, Due: {t.EndDate:yyyy-MM-dd})"));
                var userPrompt = $"Project: {project.Title}. Description: {project.Description}. Tasks: {tasksInfo}. Please provide a status summary.";

                var requestBody = new
                {
                    model = "gpt-4o-mini",
                    messages = new[]
                    {
                        new { role = "system", content = systemPrompt },
                        new { role = "user", content = userPrompt }
                    },
                    temperature = 0.3, 
                    max_tokens = 150
                };

                var jsonContent = JsonSerializer.Serialize(requestBody);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                _logger.LogInformation("Sending request to OpenAI API");

                var response = await _httpClient.PostAsync("chat/completions", content);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("OpenAI API error: {StatusCode} - {Content}", response.StatusCode, responseContent);
                    return new AIResponseResult { Success = false, ErrorMessage = $"API Error: {response.StatusCode}" };
                }

                var openAiResponse = JsonSerializer.Deserialize<OpenAiResponse>(responseContent);
                var assistantMessage = openAiResponse?.Choices?.FirstOrDefault()?.Message?.Content;

                if (string.IsNullOrEmpty(assistantMessage))
                {
                    return new AIResponseResult { Success = false, ErrorMessage = "Empty response from API" };
                }

                return new AIResponseResult { Success = true, Content = assistantMessage };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling OpenAI");
                return new AIResponseResult { Success = false, ErrorMessage = ex.Message };
            }
        }
    }

    public class OpenAiResponse
    {
        [JsonPropertyName("choices")]
        public List<Choice>? Choices { get; set; }
    }
    public class Choice
    {
        [JsonPropertyName("message")]
        public Message? Message { get; set; }
    }
    public class Message
    {
        [JsonPropertyName("content")]
        public string? Content { get; set; }
    }
}
