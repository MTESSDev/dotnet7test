using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FRW.PR.Extra.Pages
{
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public class AccessibiliteModel : PageModel
    {
        public string Language => CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;

        public void OnGet()
        {
        }
    }
}
