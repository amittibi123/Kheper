using System.Text;
using System.Text.Json;

namespace Kheper.Shared.Models;

public class TranslationService
{
    private readonly HttpClient _httpClient = new HttpClient();
    private const string BASE_URL = "https://kheper.onrender.com/api/translate";
    private const string API_KEY = "Kh3p3r$Tr4nsl@t3#2026!xQmZvR9wYpNkJdF";

    public async Task<string> TranslateToEnglishAsync(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return "";

        try
        {
            var requestBody = new
            {
                q = text,
                source = "auto",
                target = "en",
                format = "text"
            };

            var json = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            using var request = new HttpRequestMessage(HttpMethod.Post, BASE_URL);
            request.Content = content;
            request.Headers.Add("X-API-Key", API_KEY);

            var response = await _httpClient.SendAsync(request);
            var responseJson = await response.Content.ReadAsStringAsync();

            using var doc = JsonDocument.Parse(responseJson);
            return doc.RootElement.GetProperty("translatedText").GetString() ?? text;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Translation error: {ex.Message}");
            return text;
        }
    }
}
