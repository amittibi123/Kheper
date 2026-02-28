using set.Web.Components;
using set.Shared.Components; // הוספנו את זה
using Blazored.LocalStorage;  // הוספנו את השורה הזו

var builder = WebApplication.CreateBuilder(args);

// הוספת שירותי Blazor
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// הוספת Blazored LocalStorage לשירותי התלותים
builder.Services.AddBlazoredLocalStorage();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
}

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddAdditionalAssemblies(typeof(set.Shared.Components.Routes).Assembly); // חשוב: אומר לשרת לחפש דפים ב-Shared

app.Run();