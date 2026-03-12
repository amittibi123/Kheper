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

        // תרגום לאנגלית
        Console.WriteLine($"[NLP] Translating: '{request.q}'");
        var translateBody = new { q = request.q, source = "auto", target = "en", format = "text" };
        var translateResponse = await client.PostAsJsonAsync("http://localhost:5000/translate", translateBody);
        var translateJson = await translateResponse.Content.ReadAsStringAsync();
        using var translateDoc = JsonDocument.Parse(translateJson);
        var enText = translateDoc.RootElement.GetProperty("translatedText").GetString() ?? request.q;
        Console.WriteLine($"[NLP] Translated: '{enText}'");

        // חילוץ משימות
        Console.WriteLine($"[NLP] Extracting tasks...");
        var extractor = new TaskExtractor();
        var tasks = extractor.ExtractTasks(enText);
        Console.WriteLine($"[NLP] Done! Found {tasks.Count} tasks");

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