using System.Text;
using System.Text.Json;

namespace Kheper.Shared.Models;

public class NlpService
{
    private readonly HttpClient _httpClient = new HttpClient();
    private const string BASE_URL = "https://kheper.onrender.com/api/translate";
    private const string API_KEY = "Kh3p3r$Tr4nsl@t3#2026!xQmZvR9wYpNkJdF";

    public async Task<List<ExtractedTask>> ProcessTextAsync(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return new List<ExtractedTask>();

        for (int i = 0; i < 3; i++)
        {
            try
            {
                var requestBody = new { q = text, source = "auto", target = "en", format = "text" };
                var json = JsonSerializer.Serialize(requestBody);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                using var request = new HttpRequestMessage(HttpMethod.Post, BASE_URL);
                request.Content = content;
                request.Headers.Add("X-API-Key", API_KEY);

                _httpClient.Timeout = TimeSpan.FromSeconds(60);
                var response = await _httpClient.SendAsync(request);
                var responseJson = await response.Content.ReadAsStringAsync();

                return JsonSerializer.Deserialize<List<ExtractedTask>>(responseJson,
                           new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                       ?? new List<ExtractedTask>();
            }
            catch
            {
                if (i < 2) await Task.Delay(3000);
            }
        }

        return new List<ExtractedTask>();
    }
}