using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FRW.PR.Extra.Pages
{
    [AllowAnonymous]
    public class PageIntrouvableModel : PageModel
    {
        public IActionResult OnGet()
        {
            return Page();
        }
    }
}
