using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using FRW.PR.Extra.Models;
using FRW.PR.Extra.Services;
using FRW.PR.Extra.Utils;
using FRW.TR.Commun;
using FRW.TR.Commun.Utils;
using FRW.TR.Contrats;
using FRW.TR.Contrats.Constantes;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FRW.PR.Extra.Pages.Form
{
    public class ConfirmationModel : PageModel
    {
        public string Language => CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;

        private IFormulairesService _formService;
        private readonly IVueParser _vueParser;

        public string? Title { get; set;
        }
        [BindProperty(Name = "noConfirmation", SupportsGet = true)]
        public long NoConfirmation { get; set; }

        [BindProperty(Name = "id", SupportsGet = true)]
        public string? TypeFormulaire { get; set; }

        [BindProperty(Name = "SystemeAutorise", SupportsGet = true)]
        public int? SystemeAutorise { get; set; }

        [BindProperty(Name = "version", SupportsGet = true)]
        public string? Version { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? ShortInstance { get; set; }

        public ConfirmationModel(IFormulairesService formulairesService, IVueParser vueParser)
        {
            _formService = formulairesService;
            _vueParser = vueParser;
        }

        /// <summary>
        /// FRW212 - Afficher la page d'accusé réception de transmission du formulaire
        /// </summary>
        public async Task<IActionResult> OnGet()
        {
            await AssignerTitrePage();

            return Page();
        }

        private async Task AssignerTitrePage()
        {
            var currentConfig = await _formService.ObtenirContenuFichierConfiguration(SystemeAutorise, TypeFormulaire, Version, NiveauConfig.FORMULAIRE);
            if (string.IsNullOrWhiteSpace(currentConfig))
                return;

            var dynamicForm = OutilsYaml.DeserializerString<DynamicForm>(currentConfig);
            Title = (dynamicForm?.Form?["title"] as Dictionary<object, object>)?.GetLocalizedObject();
        }
    }
}
