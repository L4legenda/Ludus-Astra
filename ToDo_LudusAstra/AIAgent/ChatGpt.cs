using System.Text;
using System.Text.Json;

namespace ToDo_LudusAstra.AIAgent;

public class ChatGpt
{
    private const string API_KEY = "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"; // Ваш API ключ
    private const string MODEL = "openai/chatgpt-4o-latest";

    // Метод для обработки контента (удаление тегов <think>)
    private static string ProcessContent(string content)
    {
        return content.Replace("<think>", "").Replace("</think>", "");
    }

    // Основной метод для отправки запроса и получения ответа в потоковом режиме
    public async Task<string> ChatStreamAsync(string prompt)
    {
        using (HttpClient client = new HttpClient())
        {
            // Устанавливаем заголовки
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", API_KEY);
            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

            // Формируем тело запроса
            var data = new
            {
                model = MODEL,
                messages = new[] { new { role = "user", content = prompt } },
                stream = true
            };

            var json = JsonSerializer.Serialize(data);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Отправляем запрос
            using (var response = await client.PostAsync("https://openrouter.ai/api/v1/chat/completions", content))
            {
                if (response.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    throw new Exception("Ошибка API: " + response.StatusCode);
                }

                var fullResponse = new List<string>();

                // Читаем ответ построчно
                using (var stream = await response.Content.ReadAsStreamAsync())
                using (var reader = new System.IO.StreamReader(stream))
                {
                    while (!reader.EndOfStream)
                    {
                        var line = await reader.ReadLineAsync();
                        if (!string.IsNullOrEmpty(line))
                        {
                            var chunkStr = line.Replace("data: ", "");
                            try
                            {
                                var chunkJson = JsonSerializer.Deserialize<JsonElement>(chunkStr);
                                if (chunkJson.TryGetProperty("choices", out var choices))
                                {
                                    var contentDelta = choices[0].GetProperty("delta").GetProperty("content").GetString();
                                    if (!string.IsNullOrEmpty(contentDelta))
                                    {
                                        var cleaned = ProcessContent(contentDelta);
                                        Console.Write(cleaned); // Выводим в консоль
                                        fullResponse.Add(cleaned);
                                    }
                                }
                            }
                            catch
                            {
                                // Игнорируем ошибки парсинга
                            }
                        }
                    }
                }

                Console.WriteLine(); // Перенос строки после завершения потока
                return string.Join("", fullResponse); // Возвращаем полный ответ
            }
        }
    }
}