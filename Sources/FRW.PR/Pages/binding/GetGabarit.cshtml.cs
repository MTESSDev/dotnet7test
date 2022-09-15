using FRW.PR.Extra.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace FRW.PR.Extra.Pages
{
    public class GetGabaritModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        private readonly IFormulairesService _formulairesService;

        public GetGabaritModel(ILogger<IndexModel> logger, IFormulairesService formulairesService)
        {
            _logger = logger;
            _formulairesService = formulairesService;
        }

        public async Task<IActionResult> OnGet(int idSystemeAutorise, string id, string fichier)
        {
            var pdfBytes = await _formulairesService.ObtenirContenuFichierGabarit(idSystemeAutorise, id, fichier);

            if (pdfBytes is null)
                return NotFound();

            return File(pdfBytes, "application/pdf");
        }
    }
}
