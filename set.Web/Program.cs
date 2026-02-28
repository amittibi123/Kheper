using set.Web.Components;
using set.Shared.Components; // הוספנו את זה

var builder = WebApplication.CreateBuilder(args);

// הוספת שירותי Blazor
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

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