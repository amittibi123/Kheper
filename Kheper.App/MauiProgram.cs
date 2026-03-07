using Microsoft.Extensions.Logging;
using Kheper.Shared.Components;
using Blazored.LocalStorage;
using System.Net.Http;
using Kheper.Shared.Models; // הוסף כדי ש-AddHttpClient ו-HttpClient יהיו מוכרים

namespace Kheper;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
            });

        // רישום שירותי ה-UI והתשתית
        builder.Services.AddMauiBlazorWebView();
        builder.Services.AddBlazoredLocalStorage();
        builder.Services.AddLocalization();

        // רישום השירותים הייחודיים של הפרויקט
        builder.Services.AddScoped<TranslationService>();
        builder.Services.AddSingleton<TaskService>();
        builder.Services.AddSingleton<LanguageService>();
        builder.Services.AddScoped<NavigationState>();
        builder.Services.AddSingleton<SyncService>();

        builder.Services.AddHttpClient("LocalApi", client =>
        {
            // שנה את זה לפורט שהשרת באמת משתמש בו
            client.BaseAddress = new Uri("http://localhost:5243/"); 
        });

        // רישום HttpClient כללי כ-Scoped (ליתר ביטחון)
        builder.Services.AddScoped(sp => new HttpClient 
        { 
            BaseAddress = new Uri("http://localhost:5243/") 
        });

#if DEBUG
        builder.Services.AddBlazorWebViewDeveloperTools();
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}