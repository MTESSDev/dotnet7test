using FRW.PR.Extra.Services;
using FRW.PR.Extra.Utils;
using FRW.TR.Commun;
using FRW.TR.Contrats.Assignateur;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FRW.TR.Commun.Utils;
using FRW.TR.Contrats;
using FRW.TR.Contrats.Composants;
using FRW.TR.Commun.SchemaFormulaire;
using FRW.TR.Contrats.Constantes;

namespace FRW.PR.Extra.Pages
{
    public class BindingIndexModel : PageModel
    {
        private readonly IVueParser _vueParser;
        private readonly ILogger<BindingIndexModel>? _logger;
        private readonly IFormulairesService _formulairesService;

        [BindProperty(Name = "TypeFormulaire", SupportsGet = true)]
        public string? TypeFormulaire { get; set; }

        [BindProperty(Name = "SystemeAutorise", SupportsGet = true)]
        public int SystemeAutorise { get; set; }

        public Dictionary<string, object?> VueData { get; set; } = new Dictionary<string, object?>();

        [VueData("templates")]
        public List<TemplateElement>? Templates { get; set; }

        [VueData("bind")]
        public Dictionary<string, Dictionary<string, BindElement>>? Bind { get; set; }

        [VueData("gabaritEnCours")]
        public string? GabaritEnCours { get; set; }

        public string? Pdf { get; set; }

        [VueData("optionsGroups")]
        public List<string>? OptionsGroups { get; set; }

        [VueData("allOptionsFields")]
        public Dictionary<string, HashSet<string>>? AllOptionsFields { get; set; }

        public BindingIndexModel(ILogger<BindingIndexModel> logger, IVueParser vueParser, IFormulairesService formulairesService)
        {
            _logger = logger;
            _vueParser = vueParser;
            _formulairesService = formulairesService;
        }

        public async Task<IActionResult> OnGet(string? gabarit)
        {
            var cfg = await _formulairesService.ObtenirContenuFichierBinding(SystemeAutorise, TypeFormulaire!);

            if (string.IsNullOrWhiteSpace(cfg))
                return NotFound();

            var mappingObj = OutilsYaml.DeserializerString<Binder>(cfg);

            TemplateElement? gabaritSelection;

            if (gabarit is null)
                gabaritSelection = mappingObj.Templates.First();
            else
                gabaritSelection = mappingObj.Templates.Where(x => x.Id == gabarit).First();

            GabaritEnCours = gabaritSelection.Id;
            Pdf = gabaritSelection.Gabarit.GetLocalizedString(CultureInfoExtensions.LangueUtilisateur, true);
            Templates = mappingObj.Templates;
            Bind = mappingObj.Bind;

            if (Bind is null)
            {
                Bind = new Dictionary<string, Dictionary<string, BindElement>>();
            }

            if (Templates is { })
                foreach (var template in Templates)
                {
                    Bind.TryAdd(template.Id, new Dictionary<string, BindElement>());
                }

            var defaultConfig = await _formulairesService.ObtenirContenuFichierConfiguration(SystemeAutorise, "default", "0", NiveauConfig.GLOBAL);
            DynamicForm? defaultCfg = OutilsYaml.DeserializerString<DynamicForm>(defaultConfig!);

            var cfgForm = await _formulairesService.ObtenirContenuFichierConfiguration(SystemeAutorise, TypeFormulaire, "0", NiveauConfig.FORMULAIRE);

            if (string.IsNullOrWhiteSpace(cfgForm))
                return NotFound();

            var form = OutilsYaml.DeserializerString<DynamicForm>(cfgForm);

            var formData = new List<ComponentBinding>();

            Component.MergeProp(form.Form?["sectionsGroup"], defaultCfg.Form?["defaults"]);
            Component.GetComponents(true, form.Form?["sectionsGroup"], ref formData, null, null, null, null, null);

            OptionsGroups = new List<string>();
            AllOptionsFields = new Dictionary<string, HashSet<string>>();

            foreach (var component in formData)
            {
                if (component.NameValues is null) continue;

                var list = new HashSet<string>();
                foreach (var item in component.NameValues)
                {
                    if (!list.Contains(item))
                    {
                        list.Add(item);
                    }
                }

                if (component.SectionName is null) continue;

                if (AllOptionsFields.ContainsKey(component.SectionName))
                {
                    foreach (var item in list)
                    {
                        if (!AllOptionsFields[component.SectionName].Contains(item))
                        {
                            AllOptionsFields[component.SectionName].Add(item);
                        }
                    }
                }
                else
                {
                    AllOptionsFields.Add(component.SectionName, list);
                }
            }

            AllOptionsFields.Add("Interne", new HashSet<string>() { "<NULL>", "<TOUS-VRAIS>" });

            foreach (var component in AllOptionsFields)
            {
                if (!OptionsGroups.Contains(component.Key))
                {
                    OptionsGroups.Add(component.Key);
                }
            }

            VueData = _vueParser.ParseData(this);

            return Page();
        }
    }
}
