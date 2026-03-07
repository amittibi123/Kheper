using System.Text;
using System.Text.Json;

namespace Kheper.Shared.Models;

public class TranslationService
{
    private readonly HttpClient _httpClient = new HttpClient();
    private const string BASE_URL = "http://localhost:5000";

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

            var response = await _httpClient.PostAsync($"{BASE_URL}/translate", content);
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