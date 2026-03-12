using Microsoft.AspNetCore.Mvc;
using Kheper.Shared.Models;
using System.Text.Json;

[ApiController]
[Route("api/translate")]
public class TranslateProxyController : ControllerBase
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _config;

    public TranslateProxyController(IHttpClientFactory httpClientFactory, IConfiguration config)
    {
        _httpClientFactory = httpClientFactory;
        _config = config;
    }

    [HttpPost]
    public async Task<IActionResult> Translate([FromBody] TranslateRequest request,
                                               [FromHeader(Name = "X-API-Key")] string? apiKey)
    {
        var validKey = Environment.GetEnvironmentVariable("TRANSLATE_API_KEY")
                    ?? _config["TRANSLATE_API_KEY"];

        if (string.IsNullOrEmpty(apiKey) || apiKey != validKey)
            return Unauthorized(new { error = "Invalid or missing API Key" });

        var client = _httpClientFactory.CreateClient();

        // שלב 1 - זיהוי שפה
        Console.WriteLine($"[NLP] Step 1: Detecting language for: '{request.q}'");
        var detectBody = new { q = request.q };
        var detectResponse = await client.PostAsJsonAsync("http://localhost:5000/detect", detectBody);
        var detectJson = await detectResponse.Content.ReadAsStringAsync();
        using var detectDoc = JsonDocument.Parse(detectJson);
        var detectedLang = detectDoc.RootElement[0].GetProperty("language").GetString() ?? "en";
        Console.WriteLine($"[NLP] Step 1: Detected language: {detectedLang}");

        // שלב 2 - תרגום לאנגלית
        Console.WriteLine($"[NLP] Step 2: Translating to English...");
        var translateBody = new { q = request.q, source = detectedLang, target = "en", format = "text" };
        var translateResponse = await client.PostAsJsonAsync("http://localhost:5000/translate", translateBody);
        var translateJson = await translateResponse.Content.ReadAsStringAsync();
        using var translateDoc = JsonDocument.Parse(translateJson);
        var enText = translateDoc.RootElement.GetProperty("translatedText").GetString() ?? request.q;
        Console.WriteLine($"[NLP] Step 2: Translated: '{enText}'");

        // שלב 3 - חילוץ משימות
        Console.WriteLine($"[NLP] Step 3: Extracting tasks...");
        var extractor = new TaskExtractor();
        var tasks = extractor.ExtractTasks(enText);
        Console.WriteLine($"[NLP] Step 3: Found {tasks.Count} tasks");

        return Ok(tasks);
    }

    [HttpGet("health")]
    public async Task<IActionResult> Health()
    {
        try
        {
            var client = _httpClientFactory.CreateClient();
            var response = await client.GetAsync("http://localhost:5000/languages");
            return Ok(new { status = "ok", libreTranslate = response.IsSuccessStatusCode });
        }
        catch (Exception ex)
        {
            return Ok(new { status = "error", message = ex.Message });
        }
    }
}

public class TranslateRequest
{
    public string q { get; set; } = "";
    public string source { get; set; } = "auto";
    public string target { get; set; } = "en";
    public string format { get; set; } = "text";
}