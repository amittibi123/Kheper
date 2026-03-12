using Microsoft.AspNetCore.Mvc;

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
        var response = await client.PostAsJsonAsync("http://localhost:5000/translate", request);
        var content = await response.Content.ReadAsStringAsync();

        return Content(content, "application/json");
    }
}

public class TranslateRequest
{
    public string q { get; set; } = "";
    public string source { get; set; } = "auto";
    public string target { get; set; } = "en";
    public string format { get; set; } = "text";
}
```

אחרי שתעשה `git push`, Render יעשה deploy אוטומטי והכתובת תהיה:
```
POST https://kheper.onrender.com/api/translate
X-API-Key: Kh3p3r$Tr4nsl@t3#2026!xQmZvR9wYpNkJdF
