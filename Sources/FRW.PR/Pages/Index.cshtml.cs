using CSharpVitamins;
using FRW.PR.Extra.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace FRW.PR.Extra.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        private readonly IFormulairesService _formulairesService;
        private readonly IConfiguration _configuration;

        public int? IdFormulaire { get; set; }
        public string? NoPublicSession { get; set; }
        public List<SelectListItem> FormListItems { get; set; } = default!;

        [BindProperty(Name = "SystemeAutorise", SupportsGet = true)]
        public int? SystemeAutorise { get; set; }

        /// <summary>
        /// Page d'accueil de l'application
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="formulairesService"></param>
        /// <param name="configuration"></param>
        public IndexModel(ILogger<IndexModel> logger,
                            IFormulairesService formulairesService,
                            IConfiguration configuration)
        {
            _logger = logger;
            _formulairesService = formulairesService;
            _configuration = configuration;
        }

        public async Task<IActionResult> OnGet()
        {
            if (_configuration.GetValue<bool>("estProduction"))
                return Redirect("https://quebec.ca");

            if (SystemeAutorise is null)
            {
                _ = int.TryParse(Request.Cookies["PageConfigSystemeCourant"], out int nouveauSystemeAutorise);
                SystemeAutorise = nouveauSystemeAutorise;

                if (SystemeAutorise is null || SystemeAutorise == 0) SystemeAutorise = 1;

                Response.Cookies.Append("PageConfigSystemeCourant", SystemeAutorise.ToString()!);

                return Redirect("/" + SystemeAutorise?.ToString());
            }

            Response.Cookies.Append("PageConfigSystemeCourant", SystemeAutorise.ToString()!);

            FormListItems = await CreerListeFormulaires();

            return Page();
        }

        private async Task<List<SelectListItem>> CreerListeFormulaires()
        {
            var listeItems = new List<SelectListItem>();
            var formulaires = await _formulairesService.ObtenirListeFormulaires((int)SystemeAutorise!);

            foreach (var form in formulaires)
            {
                listeItems.Add(new SelectListItem($"{form.Id} - {form.Version} - {form.TitreFrancais}", form.Id));
            }

            return listeItems;
        }

        public async Task<IActionResult> OnPostCreerFormulaireIndividu(int idSystemeAutorise, string typeFormulaire, string? identifiantUtilisateur, string? jsonData)
        {
            // Protection pour la prod
            if (_configuration.GetValue<bool>("estProduction")) return NotFound();

            var a = await _formulairesService.CreerFormulaireIndividu(idSystemeAutorise, typeFormulaire, identifiantUtilisateur, jsonData);

            return new OkObjectResult(a);
        }

        public async Task<IActionResult> OnPostAfficherContenuFormulaire(int idFormulaire)
        {
            // Protection pour la prod
            if (_configuration.GetValue<bool>("estProduction")) return NotFound();

            var donnees = await _formulairesService.ObtenirDonnees(idFormulaire);

            dynamic retour = new { };
            if (!string.IsNullOrEmpty(donnees.ContenuFormulaire)) retour = Newtonsoft.Json.JsonConvert.DeserializeObject(donnees.ContenuFormulaire)!;

            return new OkObjectResult(Newtonsoft.Json.JsonConvert.SerializeObject(retour));
        }

        public IActionResult OnPostConvertirNoSession(string noPublicSession)
        {
            // Protection pour la prod
            if (_configuration.GetValue<bool>("estProduction")) return NotFound();

            return new OkObjectResult(ShortGuid.Encode(noPublicSession));
        }
    }
}
