using CSharpVitamins;
using FRW.PR.Extra.Services;
using FRW.PR.Extra.Utils;
using FRW.TR.Commun;
using FRW.TR.Commun.Services;
using FRW.TR.Commun.Utils;
using FRW.TR.Contrats;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace FRW.PR.Extra.Pages
{

    public class PageRepriseModel : PageModel
    {
        private readonly ILogger<PageRepriseModel> _logger;
        private readonly AuthService _authService;
        private readonly IFormulairesService _formulairesService;
        private readonly ISessionService _sessionService;
        private readonly IVueParser _vueParser;
        private readonly IConfiguration _configuration;
        private readonly ISuiviEtatService _suiviService;
        private readonly IConfigurationApplication _configApp;

        public string? Layout { get; set; } = "_Layout2Colonnes";
        public string Language => CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
        public bool EstValide { get; set; } = false;
        public bool DejaTransmis { get; set; } = false;
        public string? Title { get; set; }
        public string? TypeFormulaire { get; set; }
        public int? SystemeAutorise { get; set; }
        public string? ShortInstance { get; set; }
        public string? Version { get; set; }

        public Dictionary<string, object?> VueData { get; set; } = new Dictionary<string, object?>();

        [VueData("formErrors")]
        public object[] FormErrors { get; set; }

        [VueData("inputErrors")]
        public Dictionary<string, string> InputErrors { get; set; }

        [VueData("config")]
        public Dictionary<string, object?> Config { get; set; }

        [VueData("form")]
        public dynamic? Form { get; set; }

        public static IDeserializer deserializer = new DeserializerBuilder()
                             .WithNamingConvention(CamelCaseNamingConvention.Instance)
                             .IgnoreUnmatchedProperties()
                             .Build();

        public PageRepriseModel(
            ILogger<PageRepriseModel> logger,
            IVueParser vueParser,
            AuthService authService,
            IFormulairesService formulairesService,
            ISessionService sessionService,
            IConfiguration configuration,
            ISuiviEtatService suiviService,
            IConfigurationApplication configurationApplication)
        {
            _logger = logger;
            _vueParser = vueParser;
            _authService = authService;
            _formulairesService = formulairesService;
            _sessionService = sessionService;
            _configuration = configuration;
            _suiviService = suiviService;
            _configApp = configurationApplication;

            InputErrors = new Dictionary<string, string>();
            FormErrors = new object[0];
            Config = new Dictionary<string, object?>();
            Form = new { };
        }

        public async Task<IActionResult> OnGet([FromQuery(Name = "no")] string? noPublicSession, [FromQuery(Name = "utm_source")] string? utmSource, [FromQuery(Name = "debug")] string? debug, string? codent)
        {
            if (!ShortGuid.TryParse(noPublicSession, out Guid guidSession)) return RedirigerVersSessionInvalide();
            Version ??= "0";

            // Données de base pour trouver la cellule de BD
            HttpContext.Items.TryAdd("x-vals", new Dictionary<object, object>() { { "codent", codent! } });

            var session = await _sessionService.Obtenir(guidSession);

            if (session is null || session.SessionConsommee) return RedirigerVersSessionInvalide();

            var formulaireBD = await ObtenirDonneesFormulaire(session.NsFormulaire);

            await _configApp.Initialiser((int)SystemeAutorise!, TypeFormulaire!, "0");

            if (session.CodeNatureSession == Constantes.SessionSysteme && !DejaTransmis)
            {
                var delaiValidite = _configuration.GetValue<int>("FRW:DelaiValiditeSessionSysteme");
                var estValide = !session.SessionConsommee && session.DateCreation.AddSeconds(delaiValidite) >= DateTime.Now;

                if (estValide)
                {
                    await _sessionService.Consommer(guidSession);
                    await _suiviService.CreerSuiviEtatFormulaire(session.NsFormulaire, Constantes.EtatReprise);
                    var retourConnecter = await _authService.ConnecterAsync((int)SystemeAutorise!, TypeFormulaire!, session.NsFormulaire);                 
                    return RedirigerVersFormulaire(ShortGuid.Encode(retourConnecter.idSession), utmSource, debug);
                }
                else
                {
                    return RedirigerVersSessionInvalide();
                }
            }

            ShortInstance = ShortGuid.Encode(formulaireBD!.GuidFormulaire);

            AssignerTitrePage();

            return Page();
        }

        public async Task<IActionResult> OnPostValiderReprise(string noPublicSession, string cle, string motPasse)
        {
            if (!ShortGuid.TryParse(noPublicSession, out Guid guidSession)) return BadRequest("Invalid");

            var session = await _sessionService.Obtenir(guidSession);

            var formulaireBD = await ObtenirDonneesFormulaire(session.NsFormulaire);

            var estValide = await _formulairesService.ValiderReprise(guidSession, cle, motPasse, session.NsFormulaire);

            Guid idSession = Guid.Empty;

            if (estValide && formulaireBD is { } && formulaireBD.NoConfirmation is null)
            {
                await _suiviService.CreerSuiviEtatFormulaire(session.NsFormulaire, Constantes.EtatReprise);
                var retourConnecter = await _authService.ConnecterAsync((int)SystemeAutorise!, TypeFormulaire!, session.NsFormulaire);
                idSession = retourConnecter.idSession;
            }

            return new OkObjectResult(new { estValide, fpk = ShortGuid.Encode(idSession) });

            //            return new OkObjectResult(new SortantValiderReprise { EstValide = false}); POUR TESTS
        }
        private IActionResult RedirigerVersFormulaire(string noFormulaire, string? utmSource, string? debug)
        {
            string query = "";

            if (!string.IsNullOrEmpty(utmSource))
            {
                query = $"?utm_source={utmSource}";
            }

            if (!string.IsNullOrEmpty(debug))
            {
                query = $"?debug={debug}";
            }

            return Redirect(@$"{Url.Page("form", new { culture = Language })}/{SystemeAutorise!}/{TypeFormulaire!}/{Version ?? "0"}/{noFormulaire}{query}");
        }
        private IActionResult RedirigerVersSessionInvalide(string? version = default!)
        {
            var url = @$"/{Language}/SessionInvalide";
            return Redirect(url);
        }

        private void AssignerTitrePage()
        {
            var currentConfig = _configApp.ConfigurationFormulaireObjet;

            if (currentConfig.Objet is null)
                return;

            Title = (currentConfig.Objet?["form"]["title"] as Dictionary<object, object>)?.GetLocalizedObject();
            ViewData["Title"] = Title;
        }

        private async Task<DonneesFormulaire?> ObtenirDonneesFormulaire(int nsFormulaire)
        {
            var formulaire = await _formulairesService.ObtenirDonnees(nsFormulaire);
            if (formulaire != null)
            {
                //Vérifier si formulaire transmis
                if (formulaire.NoConfirmation != null)
                {
                    DejaTransmis = true;
                }

                TypeFormulaire = formulaire.TypeFormulaire;
                SystemeAutorise = formulaire.IdSystemeAutorise;
            }

            return formulaire;
        }
    }
}
