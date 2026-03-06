using System.Text.Json;
using System.Web;

namespace set.Shared.Models;

public class TranslationService
{
    private readonly HttpClient _httpClient = new HttpClient();
    public async Task<string> TranslateToEnglishAsync(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return "";

        try
        {
            string url = $"https://translate.googleapis.com/translate_a/single?client=gtx&sl=auto&tl=en&dt=t&q={HttpUtility.UrlEncode(text)}";

            var response = await _httpClient.GetStringAsync(url);

            
            using var doc = JsonDocument.Parse(response);
            var root = doc.RootElement;
            
            
            return root[0][0][0].GetString() ?? text;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Translation error: {ex.Message}");
            return text; 
        }
    }
}