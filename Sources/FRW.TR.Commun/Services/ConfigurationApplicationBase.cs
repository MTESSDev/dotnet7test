using FRW.TR.Commun.Utils;
using Microsoft.AspNetCore.Html;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FRW.TR.Commun.Services;

/// <summary>
/// FRW413 - Obtenir les configurations pour un formulaire
/// </summary>
public abstract class ConfigurationApplicationBase : IConfigurationApplication
{
    const string MSG_ERREUR_INITIALISATION = "Impossible de lire la configuration tant que la classe n'a pas été initialisée.";

    private string? _configurationFormulaire;
    private string? _configurationSysteme;
    private string? _configurationGlobale;
    private bool _estInitialise;

    private ObjetInitialise? _configurationFormulaireObjet;
    private ObjetInitialise? _configurationSystemeObjet;
    private ObjetInitialise? _configurationGlobaleObjet;

    private readonly Dictionary<string, string> _cacheScriptsJS = new();

    public string? ConfigurationFormulaire { get { if (!EstInitialise) { throw new NotSupportedException(MSG_ERREUR_INITIALISATION); } return _configurationFormulaire; } set { _configurationFormulaire = value; } }
    public string? ConfigurationSysteme { get { if (!EstInitialise) { throw new NotSupportedException(MSG_ERREUR_INITIALISATION); } return _configurationSysteme; } set { _configurationSysteme = value; } }
    public string? ConfigurationGlobale { get { if (!EstInitialise) { throw new NotSupportedException(MSG_ERREUR_INITIALISATION); } return _configurationGlobale; } set { _configurationGlobale = value; } }

    public int? SystemeAutorise { get; set; }
    public string? TypeFormulaire { get; set; }
    public string? Version { get; set; }

    public ObjetInitialise ConfigurationFormulaireObjet
    {
        get
        {
            if (_configurationFormulaireObjet is null || !_configurationFormulaireObjet.Value.EstInit)
                _configurationFormulaireObjet = new ObjetInitialise() { EstInit = true, Objet = OutilsYaml.DeserializerString<dynamic?>(ConfigurationFormulaire ?? string.Empty) };

            return (ObjetInitialise)_configurationFormulaireObjet;
        }
    }

    public ObjetInitialise ConfigurationSystemeObjet
    {
        get
        {
            if (_configurationSystemeObjet is null || !_configurationSystemeObjet.Value.EstInit)
                _configurationSystemeObjet = new ObjetInitialise() { EstInit = true, Objet = OutilsYaml.DeserializerString<dynamic?>(ConfigurationSysteme ?? string.Empty) };

            return (ObjetInitialise)_configurationSystemeObjet;
        }
    }

    public ObjetInitialise ConfigurationGlobaleObjet
    {
        get
        {
            if (_configurationGlobaleObjet is null || !_configurationGlobaleObjet.Value.EstInit)
                _configurationGlobaleObjet = new ObjetInitialise() { EstInit = true, Objet = OutilsYaml.DeserializerString<dynamic?>(ConfigurationGlobale ?? string.Empty) };

            return (ObjetInitialise)_configurationGlobaleObjet;
        }
    }

    public bool EstInitialise
    {
        get { return _estInitialise; }
        set
        {
            if (!value)
            {
                // Efface le "cache"
                _configurationFormulaire = null;
                _configurationSysteme = null;
                _configurationGlobale = null;
                _configurationFormulaireObjet = null;
                _configurationSystemeObjet = null;
                _configurationGlobaleObjet = null;
                _cacheScriptsJS.Clear();
            }
            _estInitialise = value;
        }
    }

    public ConfigurationApplicationBase() { }

    public async Task<string?> ObtenirValeur(string clee)
    {
        return await ObtenirValeur<string>(clee);
    }

    public async Task<T?> ObtenirValeur<T>(string clee, Niveau niveau = Niveau.Tous)
    {
        await Initialiser();
        dynamic? valeurRetour = null;

        if (niveau.HasFlag(Niveau.Formulaire))
        {
            var cfgFormulaire = ConfigurationFormulaireObjet.Objet;

            // Vérifier si le formulaire est en mode "legacy" (si section config non présente)
            // Pour gérrer le mode légacy VS le nouveau mode, il serait possible d'ajouter une notion d'alias de config
            // ex: accesAnonyme -> securite.accesAnonyme
            if (cfgFormulaire is { })
                if (cfgFormulaire.ContainsKey("config"))
                    valeurRetour = ForerObjet(cfgFormulaire, "config." + clee);
                else
                {
                    // Fallback pour gérer les anciens noms vs les nouveaux noms dans la config
                    var mapClee = new Dictionary<string, string> { { "enregistrement.affichermessageincitatif", "afficherMessageSauvegardeCourrielReprise" },
                                                                   { "securite.accesanonyme", "accesAnonyme" }
                                                                  };

                    if (mapClee.TryGetValue(clee.ToLower(), out var nouvelleClee))
                        valeurRetour = ForerObjet(cfgFormulaire, nouvelleClee);
                    else
                        valeurRetour = ForerObjet(cfgFormulaire, clee);
                }
        }

        if (valeurRetour is null)
        {
            var cfg = new[] { ConfigurationSystemeObjet, ConfigurationGlobaleObjet };
            var niveaux = new[] { Niveau.Systeme, Niveau.Global };
            var niveauActuel = -1;

            foreach (var item in cfg)
            {
                niveauActuel++;

                if (!niveau.HasFlag(niveaux[niveauActuel])) continue;

                var cfgSysteme = item.Objet;

                if (cfgSysteme is { })
                    valeurRetour = ForerObjet(cfgSysteme, "config." + clee);

                if (valeurRetour is { }) break;
            }
        }

        if (valeurRetour is null)
        {
            return default;
        }
        else
        {
            if (typeof(T).Equals(typeof(object)))
                return valeurRetour;
            else
            {
                if (valeurRetour is IDictionary<object, object> test)
                {
                    if (test.ContainsKey("fr"))
                    {
                        var data = ((object)valeurRetour).GetLocalizedString(null, true);
                        return (T?)Convert.ChangeType(data, typeof(T));
                    }
                    else
                    {
                        return (T?)Convert.ChangeType(test, typeof(T));
                    }
                }
                else
                    return (T)Convert.ChangeType(valeurRetour, typeof(T));
            }
        }
    }

    public async Task<Dictionary<object, object>?> ObtenirDictionnaireFusionne(string clee)
    {
        var dictionnaireFinal = new Dictionary<object, object>();

        var dataNiveauFormulaire = await ObtenirValeur<Dictionary<object, object>>(clee, Niveau.Formulaire);
        var dataNiveauSysteme = await ObtenirValeur<Dictionary<object, object>>(clee, Niveau.Systeme);
        var dataNiveauGlobal = await ObtenirValeur<Dictionary<object, object>>(clee, Niveau.Global);

        dataNiveauFormulaire?.ToList().ForEach(x => dictionnaireFinal.Add(x.Key, x.Value));
        dataNiveauSysteme?.ToList().ForEach(x => dictionnaireFinal.TryAdd(x.Key, x.Value));
        dataNiveauGlobal?.ToList().ForEach(x => dictionnaireFinal.TryAdd(x.Key, x.Value));

        return dictionnaireFinal;
    }

    public async Task Initialiser(int? systemeAutorise, string? typeFormulaire, string? version)
    {
        SystemeAutorise = systemeAutorise;
        TypeFormulaire = typeFormulaire;
        Version = version;

        await ObtenirConfigurationApplication();

        EstInitialise = true;
    }

    public abstract Task ObtenirConfigurationApplication();

    public async Task<string> ObtenirScriptsJs(string? type = null)
    {
        var valCache = _cacheScriptsJS.GetValueOrDefault(type ?? "tous");

        if (valCache is { })
            return valCache;

        if (type is { })
        {
            Dictionary<object, object>? objetScript = null;

            switch (type)
            {
                case "method":
                    objetScript = await ObtenirDictionnaireFusionne("InjecterJs.Method");
                    break;
                case "computed":
                    objetScript = await ObtenirDictionnaireFusionne("InjecterJs.Computed");
                    break;
                case "watch":
                    objetScript = await ObtenirDictionnaireFusionne("InjecterJs.Watch");
                    break;
                default:
                    break;
            }

            // Partie ACTUELLE pour le RENDER client side
            var script = string.Empty;

            if (objetScript is { } && objetScript.Any())
            {
                foreach (var method in objetScript)
                {
                    var dictionnaire = (method.Value as IDictionary<object, object>);
                    if (dictionnaire is { } && dictionnaire.TryGetValue("code", out var code))
                        script += $",{method.Key}{code}";
                }
            }

            _cacheScriptsJS.Add(type, script);

            return script;
        }
        else
        {
            var methods = await ObtenirDictionnaireFusionne("InjecterJs.Method");
            var computed = await ObtenirDictionnaireFusionne("InjecterJs.Computed");

            var script = ObtenirFonctionsJs(methods, computed);
            _cacheScriptsJS.Add(type ?? "tous", script);

            return script;
        }

    }



    private dynamic? ForerObjet(dynamic? obj, string clee)
    {
        var arbre = clee.Split('.', StringSplitOptions.RemoveEmptyEntries);

        var newClee = char.ToLowerInvariant(arbre[0][0]) + arbre[0][1..];

        if (obj is { })
            if (obj.ContainsKey(newClee))
            {
                if (arbre.Length == 1)
                    return obj[newClee];
                else
                    return ForerObjet(obj[newClee], string.Join('.', arbre.Skip(1)));
            }

        return null;
    }

    private static string ObtenirFonctionsJs(Dictionary<object, object>? method, Dictionary<object, object>? computed)
    {
        var fonctions = AjouterMethodesJs(method);
        fonctions += AjouterMethodesJs(computed);

        return fonctions;
    }

    private static string AjouterMethodesJs(Dictionary<object, object>? dictInjecterJsElement)
    {
        var code = string.Empty;

        if (dictInjecterJsElement is { })
        {
            foreach (var elementJs in dictInjecterJsElement)
            {
                var dictionnaire = (elementJs.Value as IDictionary<object, object>);
                if (dictionnaire is { } && dictionnaire.TryGetValue("code", out var codeBlock))
                    code += $"function {elementJs.Key}{codeBlock}\r\n";
            }
        }

        return code;
    }

    public abstract Task Initialiser();

    public async Task<HtmlString> ObtenirValeurHtml(string clee)
    {
        return new HtmlString(await ObtenirValeur<string>(clee));
    }
}

public struct ObjetInitialise
{
    public bool EstInit;
    public dynamic? Objet;
}