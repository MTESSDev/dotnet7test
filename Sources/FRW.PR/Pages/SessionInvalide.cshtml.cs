using FRW.PR.Extra.Services;
using FRW.PR.Extra.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using System.Globalization;

namespace FRW.PR.Extra.Pages.Form
{
    public class SessionInvalideModel : PageModel
    {
        public string Language => CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;

        private IFormulairesService _formService;
        private readonly IVueParser _vueParser;
        private readonly ILogger<FormModel> _logger;

        [BindProperty(Name = "id", SupportsGet = true)]
        public string? TypeFormulaire { get; set; }

        [BindProperty(Name = "SystemeAutorise", SupportsGet = true)]
        public int? SystemeAutorise { get; set; }

        [BindProperty(Name = "version", SupportsGet = true)]
        public string? Version { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? ShortInstance { get; set; }

        public SessionInvalideModel(IFormulairesService formulairesService, IVueParser vueParser, ILogger<FormModel> logger)
        {
            _formService = formulairesService;
            _vueParser = vueParser;
            _logger = logger;
        }

        public IActionResult OnGet()
        {
            _logger.LogDebug("SessionInvalide OnGet {0}", new { UrlReferrer = Request.Headers["Referer"].ToString() });
            return Page();
        }
    }
}
