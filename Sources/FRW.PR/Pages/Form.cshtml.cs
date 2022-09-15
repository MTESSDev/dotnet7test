using CSharpVitamins;
using FRW.PR.Extra.Models;
using FRW.PR.Extra.Services;
using FRW.PR.Extra.Utils;
using FRW.PR.Services;
using FRW.TR.Commun;
using FRW.TR.Commun.Services;
using FRW.TR.Commun.Utils;
using FRW.TR.Contrats;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Dynamic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FRW.PR.Extra.Pages
{
    public class FormModel : PageModel
    {
        private readonly AuthService _authService;
        private readonly ILogger<FormModel> _logger;
        private readonly IVueParser _vueParser;
        private readonly IMemoryCache _cache;
        private readonly IConfiguration _config;
        private readonly IFormulairesService _formulairesService;
        private readonly IContexteDevAccesseur _contexteDev;
        private readonly ISystemeAutoriseService _sysAutrService;
        private readonly ISessionService _sessionService;
        private readonly IConfigurationApplication _configApp;
        private static readonly string NOM_NOUVELLE_INSTANCE = "N";

        public string? Layout { get; set; } = "_Layout2ColonnesDroite";
        public string Language => CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
        public string Created { get; set; } = string.Empty;
        public string? FormRaw { get; set; }
        public bool AfficherInfosDebug => _config.GetValue<bool>("FRW:AfficherInfosDebug");
        public bool EstProduction => _config.GetValue<bool>("estProduction");
        public bool AfficherBlocCode { get; set; } = false;

        [BindProperty(Name = "id", SupportsGet = true)]
        public string? TypeFormulaire { get; set; }

        [BindProperty(Name = "SystemeAutorise", SupportsGet = true)]
        public int? SystemeAutorise { get; set; }

        [BindProperty(Name = "version", SupportsGet = true)]
        public string? Version { get; set; }

        [BindProperty(Name = "instance", SupportsGet = true)]
        public string? ShortInstance { get; set; }

        public long? ExpirationSession { get; set; } = 0;

        public long? DelaiAffichagePopUp { get; set; } = 0;

        public Dictionary<string, object?> VueData { get; set; } = new Dictionary<string, object?>();

        [FromQuery]
        public bool ShowAll { get; set; }

        [FromQuery]
        public bool Reprise { get; set; }

        [VueData("formErrors")]
        public object[] FormErrors { get; set; }

        [VueData("inputErrors")]
        public Dictionary<string, string> InputErrors { get; set; }

        [VueData("config")]
        public Dictionary<string, object?> Config { get; set; }

        [VueData("form")]
        public dynamic? Form { get; set; }

        [VueData("noPageCourante")]
        public int NoPageCourante { get; set; } = 0;

        [VueData("title")]
        public string? Title { get; set; }

        [VueData("pagesGroup")]
        public List<Section>? Sections { get; set; } = new List<Section>();
        public bool EstFormulaireAuthentifie { get; private set; }

        public bool EstCourrielRepriseEnvoyer { get; private set; }

        public string? CourrielUtilisateur { get; private set; }

        public FormModel(ILogger<FormModel> logger,
                         IConfiguration config,
                         AuthService authService,
                         IVueParser vueParser,
                         IFormulairesService formulairesService,
                         IMemoryCache memoryCache,
                         IContexteDevAccesseur contexteDev,
                         ISystemeAutoriseService sysAutrService,
                         ISessionService sessionService,
                         IConfigurationApplication configurationApplication)
        {
            _logger = logger;
            _config = config;
            _authService = authService;
            _vueParser = vueParser;
            _formulairesService = formulairesService;
            _cache = memoryCache;
            _contexteDev = contexteDev;
            _sysAutrService = sysAutrService;
            _sessionService = sessionService;
            _configApp = configurationApplication;

            InputErrors = new Dictionary<string, string>();
            FormErrors = new object[0];
            Config = new Dictionary<string, object?>();
            Form = new { };
        }


        /// <summary>
        /// On Get
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> OnGet()
        {
            if (SystemeAutorise == null || TypeFormulaire is null) return Redirect("/");

            var delaiAffichagePopUp = _config.GetValue<double>("FRW:DelaiAffichagePopUp");
            DelaiAffichagePopUp = (long?)TimeSpan.FromMinutes(delaiAffichagePopUp).TotalSeconds;

            await _configApp.Initialiser();

            if (string.IsNullOrWhiteSpace(_configApp.ConfigurationFormulaire))
                return NotFound();

            _logger.LogDebug("Form OnGet {0}", new { Path = HttpContext.Request.Path, Query = HttpContext.Request.QueryString, IsAuthenticated = HttpContext.User.Identity.IsAuthenticated });

            string? contenuFormulaire;
            if (TypeFormulaire!.Equals("devcfg"))
            {
                if (_config.GetValue<bool>("estProduction")) return NotFound();
                contenuFormulaire = _contexteDev.ConfigDevBrute;
                ExpirationSession = DateTime.Now.AddYears(99).ToExpirationClient();
            }
            else
            {
                bool accesAnonyme = await _configApp.ObtenirValeur<bool>("securite.AccesAnonyme");
                bool formulaireUnilingue = await _configApp.ObtenirValeur<bool>("FormulaireUnilingue");

                if (formulaireUnilingue)
                {
                    ViewData["AfficherSelecteurLangue"] = false;
                    var url = Request.Path.ToString();
                    if (url.Contains("/en/"))
                    {
                        url = url.Replace("/en/", "/fr/");
                        return LocalRedirectPreserveMethod(url);
                    }
                }

                bool forcerNouveau = ShortInstance?.Equals(NOM_NOUVELLE_INSTANCE, StringComparison.InvariantCultureIgnoreCase) ?? false;
                Guid? guidSession = ShortInstance.GetGuidOrNull();

                var infoSession = await _authService.RecupererValiderInformationSession(guidSession, true);

                _logger.LogDebug("Form OnGet {0}", new { accesAnonyme, infoSession, forcerNouveau });

                DonneesFormulaire? donneesFormulaire = null;

                if (!HttpContext.User.Identity.IsAuthenticated || forcerNouveau)
                {
                    if (!accesAnonyme)
                    {
                        _logger.LogDebug("Form OnGet RedirigerVersFormulaireInaccessible");
                        //Ce formulaire est accessible seulement par un système ou en reprise
                        return RedirigerVersFormulaireInaccessible();
                    }

                    // Creer le formulaire dans la BD
                    var noPublicForm = Guid.NewGuid();
                    var nsForm = await _formulairesService.Creer((int)SystemeAutorise, TypeFormulaire!, noPublicForm, string.Empty);

                    var entrantObtenirSessionFormulaire = new EntrantObtenirSessionFormulaire
                    {
                        NsFormulaire = nsForm,
                        CodeNatureSession = Constantes.SessionCourriel,
                        IndicateurSessionValide = true
                    };
                    var sessionsCourriel = await _sessionService.ObtenirSessionFormulaire(entrantObtenirSessionFormulaire);
                    EstCourrielRepriseEnvoyer = sessionsCourriel.Any();

                    var (expiration, idSession) = await _authService.ConnecterAsync((int)SystemeAutorise, TypeFormulaire!, nsForm);
                    ExpirationSession = expiration.ToExpirationClient();

                    return RedirigerVersNouveauFormulaire(ShortGuid.Encode(idSession), Version);
                }

                if (infoSession is null)
                {
                    return RedirigerVersNouveauFormulaire(NOM_NOUVELLE_INSTANCE, Version);
                }
                else
                {
                    var entrantObtenirSessionFormulaire = new EntrantObtenirSessionFormulaire
                    {
                        NsFormulaire = (int)infoSession?.idFormulaire!,
                        CodeNatureSession = Constantes.SessionCourriel,
                        IndicateurSessionValide = true
                    };
                    var sessionsCourriel = await _sessionService.ObtenirSessionFormulaire(entrantObtenirSessionFormulaire);
                    EstCourrielRepriseEnvoyer = sessionsCourriel.Any();
                    donneesFormulaire = await _formulairesService.ObtenirDonnees((int)infoSession?.idFormulaire!);
                    ExpirationSession = infoSession?.expiration.ToExpirationClient();
                }

                //Vérifier que l'url contient le même type de formulaire que la BD, sinon on commande un nouveau formulaire
                if (!donneesFormulaire?.TypeFormulaire.Equals(TypeFormulaire) ?? true) return RedirigerVersNouveauFormulaire(NOM_NOUVELLE_INSTANCE, Version);

                contenuFormulaire = donneesFormulaire!.ContenuFormulaire;
                EstFormulaireAuthentifie = (donneesFormulaire!.IdUtilisateur is { });
            }

            //Ici nous avons déjà un cookie avec le no sequentiel de form (donc F5 suite à enregistrement ou mode reprise)
            var retourRender = await RenderPageCache(TypeFormulaire);

            AppliquerRetourRenderPage(retourRender);
            InjecterContenuFormulaire(contenuFormulaire);

            if (retourRender is null) return NotFound();
            else return Page();
        }

        #region Bac-a-sable
        public async Task<IActionResult> OnPost(EntrantBacASable entrantBacASable)
        {
            ViewData["CacherPIV"] = true;
            ViewData["DesactiverGoogleAnalytics"] = true;

            string? readFile = null;
            var cfg = Base64Decode(entrantBacASable.Content);

            var longeur = 1000;

            if (longeur > cfg.Length)
                longeur = cfg.Length;

            var debutFichier = cfg[..longeur];

            var lignes = debutFichier.Split(new char[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
            NameValueCollection? nvc = null;

            foreach (var ligne in lignes)
            {
                var trimligne = ligne.Trim();
                if (ligne.StartsWith('#'))
                {
                    trimligne = trimligne.Trim('#', ' ');

                    if (trimligne.StartsWith("debug", StringComparison.InvariantCultureIgnoreCase))
                    {
                        trimligne = trimligne.Replace("debug", "").Trim(':', ' ');
                        trimligne = trimligne.Replace(",", "&");
                        trimligne = trimligne.Replace(" ", "");
                        nvc = System.Web.HttpUtility.ParseQueryString(trimligne);
                    }
                }
                else
                {
                    break;
                }
            }

            var bc = entrantBacASable.Breadcrumb ?? String.Empty;
            var drill = bc.Split('/', StringSplitOptions.RemoveEmptyEntries);

            var configForm = OutilsYaml.DeserializerString<Dictionary<object, object>>(cfg);

            object partielConfig = configForm;
            string? sectionParent = null;
            bool estAnglais = bc.EndsWith("/en");

            for (int i = 0; i < drill.Length; i++)
            {
                var courant = drill[i];

                // On descend tant que le type est un conteneur de champs formulaire
                if (!(int.TryParse(courant, out var estNum) || courant == "form" || courant == "sectionsGroup" || courant == "sections" || courant == "components"))
                    break;

                object? partielConfigCourant;

                if (int.TryParse(drill[i], out var valint))
                {
                    partielConfigCourant = (partielConfig as List<object>)?[valint];

                    object? curV = null;

                    if (drill[i - 1] == "sections" && ((partielConfigCourant as Dictionary<object, object>)?.TryGetValue("id", out curV) ?? false) && curV is { })
                        sectionParent = (string?)curV;
                }
                else
                {
                    partielConfigCourant = (partielConfig as Dictionary<object, object>)?[drill[i]];
                }

                if (partielConfigCourant is { } && !(partielConfigCourant is string))
                    partielConfig = partielConfigCourant;
            }

            if (nvc is { })
            {
                ShowAll = bool.Parse(nvc["showAll"] ?? "true");

                if (string.IsNullOrWhiteSpace(nvc["langue"]))
                    nvc["langue"] = "auto";

                if (nvc["langue"].Equals("en", StringComparison.InvariantCultureIgnoreCase) || (nvc["langue"] == "auto" && estAnglais))
                {
                    if (CultureInfoExtensions.EstSiteFrancais)
                        return LocalRedirectPreserveMethod("/en" + HttpContext.Request.Path); //On redirige en anglais
                }
            }

            RetourRenderPage? retourRenderPage;

            if (readFile is null)
            {
                retourRenderPage = await RenderPage(TypeFormulaire, cfg);
            }
            else
            {
                retourRenderPage = await RenderPage(readFile);
            }

            object? currName = "undefined";
            (partielConfig as Dictionary<object, object>)?.TryGetValue("label", out currName);

            var labelName = currName?.GetLocalizedString(CultureInfoExtensions.LangueUtilisateur, false) ?? "undefined";

            var finalName = labelName;

            if (string.IsNullOrWhiteSpace(labelName) || labelName.Equals("undefined"))
            {
                finalName = "null";
            }
            else
            {
                finalName = $"\"{finalName}\"";
            }

            var pageNo = retourRenderPage?.Sections.SelectMany(x => x.Pages)
               .Where(x => x.Id == sectionParent).FirstOrDefault()?.No ?? 0;

            Created = $"this.effectuerNavigation({pageNo}, {finalName}, false, true); ";

            AppliquerRetourRenderPage(retourRenderPage);
            InjecterContenuFormulaire(null);
            return Page();
        }
        #endregion

        public string ReplaceFirst(string text, string search, string replace)
        {
            int pos = text.IndexOf(search);
            if (pos < 0)
            {
                return text;
            }
            return text.Substring(0, pos) + replace + text.Substring(pos + search.Length);
        }

        private int GetValOrReturn(NameValueCollection path, string v1, int v2)
        {
            if (int.TryParse(path[v1], out var val))
            {
                return val;
            }
            return v2;
        }

        private IActionResult RedirigerVersFormulaireInaccessible()
        {
            _logger.LogDebug("Form RedirigerVersFormulaireInaccessible");
            var url = @$"/{Language}/SessionInvalide";
            return Redirect(url);
        }

        private int GetValOrReturn(string[] param, int pos, int defaultVal)
        {
            if (param.Length > pos)
                if (int.TryParse(param[pos], out var val))
                {
                    return val;
                }
            return defaultVal;
        }

        private IActionResult RedirigerVersNouveauFormulaire(string noSessionUtil, string? version)
        {
            var expandoObject = new ExpandoObject();
            var expandoDictionary = (IDictionary<string, object>)expandoObject;

            //Ajout des paramètres dans le query string
            foreach (var r in Request.Query)
            {
                expandoDictionary.Add(r.Key, r.Value.ToString() ?? string.Empty);
            }

            expandoDictionary.Add("instance", noSessionUtil);
            expandoDictionary.Add("Version", (version ?? "0").ToString());

            return RedirectToPage("/Form", expandoDictionary);
        }

        private async Task<RetourRenderPage?> RenderPageCache(string? typeFormulaire)
        {
            var render = await _cache.GetOrCreateAsync($"renderpage-{SystemeAutorise}-{typeFormulaire}-{Version}-{ShowAll}-{CultureInfoExtensions.LangueUtilisateur}", entry =>
            {
                int secondesExpiration = _config.GetValue<int>("FRW:DureeCacheGeneral");
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(secondesExpiration);
                return RenderPage(typeFormulaire, null);
            });

            return render;
        }

        private void InjecterContenuFormulaire(string? contenuFormulaire = null)
        {
            if (contenuFormulaire is { })
            {
                string form = contenuFormulaire ?? string.Empty;
                if (string.IsNullOrEmpty(form))
                {
                    //Aucune données à recharger
                    Form = new { validAll = false };
                }
                else
                {
                    //Nous avons des données pour le formulaire, on les charge
                    var contenuForm = DeserialiserRetourFormulaire(form);
                    Form = contenuForm.form ?? new { };
                    if (contenuForm.form?.courriel != null)
                    {
                        CourrielUtilisateur = contenuForm.form?.courriel ?? null;
                    }
                    else
                    {
                        CourrielUtilisateur = contenuForm.systeme?.infosEnregistrement?.courriel ?? null;
                    }
                }
            }
            else
            {
                //C'est un nouveau formulaire, aucune données à recharger
                Form = new { validAll = false };
            }

            // Parse Vue data
            VueData = _vueParser.ParseData(this);
        }

        private void AppliquerRetourRenderPage(RetourRenderPage? retourRenderPage)
        {
            if (retourRenderPage is null) return;

            Config = retourRenderPage.Config;
            Title = retourRenderPage.Title;
            Sections = retourRenderPage.Sections;
            FormRaw = retourRenderPage.FormRaw;
        }

        private async Task<RetourRenderPage?> RenderPage(string? typeFormulaire, string? render = null, string? formConfig = null)
        {
            var retour = new RetourRenderPage();

            var configName = typeFormulaire ?? "default";
            string? currentConfig = null;
            retour.Config.Add("keepData", false);
            retour.Config.Add("configName", configName);

            if (string.IsNullOrWhiteSpace(_configApp.ConfigurationGlobale))
                return null;

            DynamicForm? defaultCfg = OutilsYaml.DeserializerString<DynamicForm>(_configApp.ConfigurationGlobale);

            if (typeFormulaire == "render" && render != null)
            {
                retour.DynamicForm = OutilsYaml.DeserializerString<DynamicForm>(render);
            }
            else
            {
                currentConfig = formConfig ?? _configApp.ConfigurationFormulaire;

                if (string.IsNullOrWhiteSpace(currentConfig))
                    return null;

                retour.DynamicForm = OutilsYaml.DeserializerString<DynamicForm>(currentConfig);
            }

            if (retour.DynamicForm is null) { return null; }

            if (defaultCfg != null && defaultCfg.Form != null)
            {
                foreach (KeyValuePair<object, object> item in defaultCfg?.Form ?? throw new Exception("No form element in YAML default."))
                {
                    if (retour.DynamicForm.Form is IDictionary<object, object> dynamicFormDict && !dynamicFormDict.TryAdd(item.Key, item.Value))
                    {
                        var defaultVal = item.Value as IDictionary<object, object>;
                        var newDict = new Dictionary<object, object>();
                        newDict.TryAdd(defaultVal);
                        (dynamicFormDict[item.Key] as IDictionary<object, object>).TryAdd(newDict);
                    }
                }
            }

            retour.Title = "Formulaire sans titre";

            if (retour.DynamicForm.Form is Dictionary<object, object> formDict
                && formDict.TryGetValue("title", out var titre))
                if (titre.GetLocalizedString() is string titreStr)
                    retour.Title = titreStr;

            if (!ShowAll)
            {
                retour.DynamicForm.Form!["enableVif"] = true;
            }

            if (await _configApp.ObtenirValeur<bool>("AfficherBlocCode"))
            {
                retour.DynamicForm.Form!["__blocCode"] = currentConfig ?? render;
                AfficherBlocCode = true;
            }

            using (StreamReader streamReader = new StreamReader(@"schemas/formTemplate.vue", Encoding.UTF8))
            {
                var content = await streamReader.ReadToEndAsync();
                retour.FormRaw = await FormHelpers.Stubble.RenderAsync(content, retour.DynamicForm);
            }

            //if (dynamicForm?.Form?["sections"] is null) { return NotFound(); }

            var groupNo = 0;
            var sectionNo = 0;

            if (retour.DynamicForm?.Form?["sectionsGroup"] is { })
                foreach (var sectionGroup in retour.DynamicForm.Form["sectionsGroup"])
                {
                    var pageGroupDict = (sectionGroup as Dictionary<object, object>);
                    List<Section> pages = new List<Section>();

                    if (pageGroupDict is null) { continue; }

                    if (pageGroupDict.TryGetValue("sections", out var sections))
                    {
                        var sectionsAsList = (sections as List<object>);

                        if (sectionsAsList is null) { continue; }

                        foreach (Dictionary<object, object>? section in sectionsAsList)
                        {
                            if (section is null) { continue; }

                            pages.Add(new Section()
                            {
                                No = sectionNo++,
                                Id = (pageGroupDict.TryGetValue("prefixId", out var prefixId) ? prefixId?.ToString() ?? string.Empty : string.Empty) + (section.TryGetValue("id", out var pageId) ? pageId?.ToString() ?? string.Empty : string.Empty),
                                Titre = section.TryGetValue("section", out var pageName) ? (pageName as Dictionary<object, object>).GetLocalizedObject() ?? "Title not found" : "Title not found",
                                VIf = !ShowAll ? (section.TryGetValue("v-if", out object? pageVif) ? pageVif?.ToString() ?? string.Empty : string.Empty) : string.Empty,
                                Classes = (section.TryGetValue("classes", out object? pageClasses) ? pageClasses?.ToString() : null),
                                CacherTexteExplicatifChampsObligatoires = section.TryGetValue("cacherTexteExplicatifChampsObligatoires", out object? cacherTexteExplicatifChampsObligatoires) && bool.Parse(cacherTexteExplicatifChampsObligatoires?.ToString() ?? "false")
                            });
                        }

                    }

                    retour.Sections.Add(new Section()
                    {
                        No = groupNo++,
                        Id = pageGroupDict.TryGetValue("prefixId", out var sectionId) ? sectionId?.ToString() ?? string.Empty : string.Empty,
                        Titre = pageGroupDict.TryGetValue("sectionGroup", out var sectionName) ? (sectionName as Dictionary<object, object>).GetLocalizedObject() : null,
                        VIf = !ShowAll ? (pageGroupDict.TryGetValue("v-if", out object? vif) ? vif?.ToString() ?? string.Empty : string.Empty) : string.Empty,
                        Classes = (pageGroupDict.TryGetValue("classes", out object? classes) ? classes?.ToString() : null),
                        Pages = pages
                    });
                }
            else
            {
                retour.Sections.Add(new Section() { Pages = new List<Section>() { new Section() { Id = "idpage", No = 0, Titre = "Page sans titre", CacherTexteExplicatifChampsObligatoires = true } } });
            }

            return retour;
        }

        private (dynamic? form, dynamic? systeme) DeserialiserRetourFormulaire(string formBd)
        {
            var formJson = JsonConvert.DeserializeObject<dynamic>(formBd);
            dynamic? form = null;
            dynamic? systeme = null;

            if (formJson?.ContainsKey("form"))
            {
                form = formJson?.form ?? null;
            }
            if (formJson?.ContainsKey("systeme"))
            {
                systeme = formJson?.systeme ?? null;
            }

            return (form, systeme);
        }

        public static string Base64Decode(string base64EncodedData)
        {
            var base64EncodedBytes = System.Convert.FromBase64String(base64EncodedData);
            return System.Text.Encoding.UTF8.GetString(base64EncodedBytes);
        }
    }
}
