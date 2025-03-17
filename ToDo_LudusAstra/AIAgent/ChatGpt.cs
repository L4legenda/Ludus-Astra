using System.Text;
using System.Text.Json;

namespace ToDo_LudusAstra.AIAgent;

public class ChatGpt
{
    private const string API_KEY = "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"; // Ваш API ключ
    private const string MODEL = "openai/chatgpt-4o-latest";

    private readonly HttpClient _httpClient;
    private readonly string _apiKey;

    public ChatGpt()
    {
        _httpClient = new HttpClient();
        _apiKey = API_KEY;
    }
    
    
    public async Task<string> ChatStreamAsync(string userMessage)
    {
        var requestUri = "https://openrouter.ai/api/v1/chat/completions";

        var requestBody = new
        {
            model = MODEL,
            messages = new[]
            {
                new
                {
                    role = "user",
                    content = userMessage
                }
            }
        };
        
        var jsonContent = JsonSerializer.Serialize(requestBody);
        var httpContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");

        _httpClient.DefaultRequestHeaders.Clear();
        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");

        var response = await _httpClient.PostAsync(requestUri, httpContent);

        if (response.IsSuccessStatusCode)
        {
            var responseContent = await response.Content.ReadAsStringAsync();
            return responseContent;
        }
        else
        {
            throw new HttpRequestException($"Request failed with status code {response.StatusCode}");
        }
    }
}