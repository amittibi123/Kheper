using Kheper.Web.Components;
using Kheper.Shared.Components;
using Blazored.LocalStorage;
using Kheper.Web.Data;
using Microsoft.EntityFrameworkCore;
using Kheper.Shared.Models;

var builder = WebApplication.CreateBuilder(args);

// הוספת שירותי Blazor
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// הוספת Blazored LocalStorage ושירותים נוספים
builder.Services.AddBlazoredLocalStorage();
builder.Services.AddScoped<TranslationService>();
builder.Services.AddSingleton<TaskService>();
builder.Services.AddSingleton<LanguageService>();
builder.Services.AddLocalization();
builder.Services.AddScoped<NavigationState>();
builder.Services.AddControllers();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite("Data Source=Kheper.db"));

builder.Services.AddHttpClient("LocalApi", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["BaseAddress"] ?? "http://localhost:5243/");
});

var app = builder.Build();

// הגדרת צינור הטיפול בבקשות (Middleware Pipeline)
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

// --- סדר ה-Middleware קריטי כאן ---

app.UseRouting(); // 1. קודם כל מגדירים ניתוב

app.UseAntiforgery(); // 2. אחרי הניתוב, מפעילים הגנת אנטי-פורג'רי (פעם אחת בלבד)

// 3. עכשיו אפשר למפות את הקצוות (Endpoints)
app.MapControllers(); 

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddAdditionalAssemblies(typeof(Kheper.Shared.Components.Routes).Assembly);

// הוספת משתמש לבדיקה אם ה-DB ריק
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated(); // יוצר את הקובץ אם הוא לא קיים
}

var libreTranslateProcess = new System.Diagnostics.Process
{
    StartInfo = new System.Diagnostics.ProcessStartInfo
    {
        FileName = builder.Configuration["LibreTranslate:ExecutablePath"] ?? "libretranslate",
        Arguments = "--load-only en,he,ar,zh,es,fr,de,ru,pt,ja,ko,it,tr,pl,nl,vi,th,id,uk,fa,hi,sv",
        UseShellExecute = false,
        CreateNoWindow = true
    }
};

try
{
    libreTranslateProcess.Start();
}
catch
{
    Console.WriteLine("⚠️ LibreTranslate לא נמצא - תרגום לא יעבוד. ראה README להוראות התקנה.");
}

// סגירה כשהאפליקציה נסגרת
app.Lifetime.ApplicationStopping.Register(() =>
{
    try { libreTranslateProcess.Kill(); } catch { }
});

app.Run();