using Kheper.Web.Components;
using Kheper.Shared.Components;
using Blazored.LocalStorage;
using Kheper.Web.Data;
using Microsoft.EntityFrameworkCore;
using Kheper.Shared.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddBlazoredLocalStorage();
builder.Services.AddScoped<TranslationService>();
builder.Services.AddSingleton<TaskService>();
builder.Services.AddSingleton<LanguageService>();
builder.Services.AddLocalization();
builder.Services.AddScoped<NavigationState>();
builder.Services.AddScoped<NlpService>();
builder.Services.AddControllers();

// ✅ הוספת CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite("Data Source=Kheper.db"));

builder.Services.AddHttpClient("LocalApi", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["BaseAddress"] ?? "http://localhost:5243/");
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// ✅ CORS חייב להיות לפני Antiforgery
app.UseCors("AllowAll");

app.UseAntiforgery();

app.MapControllers();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddAdditionalAssemblies(typeof(Kheper.Shared.Components.Routes).Assembly);

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();
}

var libreTranslatePath = builder.Configuration["LibreTranslate:ExecutablePath"] ?? "libretranslate";
var isDocker = Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER") == "true";

if (!isDocker)
{
    var libreTranslateProcess = new System.Diagnostics.Process
    {
        StartInfo = new System.Diagnostics.ProcessStartInfo
        {
            FileName = libreTranslatePath,
            Arguments = "--load-only en,he,ar,zh,es,fr,de,ru,pt,ja,ko,it,tr,pl,nl,vi,th,id,uk,fa,hi,sv",
            UseShellExecute = false,
            CreateNoWindow = true
        }
    };

    try
    {
        libreTranslateProcess.Start();
        app.Lifetime.ApplicationStopping.Register(() =>
        {
            try { libreTranslateProcess.Kill(); } catch { }
        });
    }
    catch
    {
        Console.WriteLine("⚠️ LibreTranslate לא נמצא.");
    }
}

app.Run();
