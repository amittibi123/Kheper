using System.Globalization;
using Microsoft.Extensions.Localization;

public class LanguageService
{
    // אירוע שמופעל בכל פעם שהשפה משתנה
    public event Action? OnLanguageChanged;

    public LanguageService()
    {
        // הגדרת אנגלית כברירת מחדל בבנייה של השירות
        SetLanguage("en-US");
    }
    public void SetLanguage(string cultureCode)
    {
        var culture = new CultureInfo(cultureCode);
        CultureInfo.DefaultThreadCurrentCulture = culture;
        CultureInfo.DefaultThreadCurrentUICulture = culture;

        // מודיעים לכל מי שמקשיב שהשפה השתנתה
        OnLanguageChanged?.Invoke();
    }

    public string GetCurrentLanguage() => CultureInfo.CurrentCulture.Name;
}