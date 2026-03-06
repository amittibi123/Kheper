using System.Globalization;

namespace set.Shared.Models;

public class LanguageService
{
    
    public event Action? OnLanguageChanged;

    public void SetLanguage(string cultureCode)
    {
        var culture = new CultureInfo(cultureCode);
        CultureInfo.DefaultThreadCurrentCulture = culture;
        CultureInfo.DefaultThreadCurrentUICulture = culture;

        
        OnLanguageChanged?.Invoke();
    }

    public string GetCurrentLanguage() => CultureInfo.CurrentCulture.Name;
}