using FRW.TR.Commun;
using FRW.TR.Commun.Services;
using FRW.TR.Commun.Utils;
using FRW.TR.Contrats.Yaml;
using HandlebarsDotNet;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Threading.Tasks;

namespace FRW.TR.Commun.Services
{
    public abstract class TexteEditeServiceBase : ITexteEditeService
    {

        public TexteEditeServiceBase()
        {
        }

        public abstract Task<IDictionary<string, object>?> ObtenirValeursAsync(string langue, TexteEditeQuery? texteEditeQuery);

        public async Task<string?> ObtenirValeurAvecRemplacementAsync(string langue, string identifiant, TexteEditeQuery? texteEditeQuery, object? valeursRemplacement = null)
        {
            Dictionary<string, object> interneRemplacement;

            if (valeursRemplacement is { } && valeursRemplacement is IDictionary<string, object> dict)
            { 
                interneRemplacement = new Dictionary<string, object>(dict);
            }
            else
            {
                interneRemplacement = new Dictionary<string, object>();

                if (valeursRemplacement is { } && valeursRemplacement is Array array)
                    foreach (var item in array)
                    {
                        if (item is IDictionary<string, object> itemDict)
                            foreach (var sousItem in itemDict)
                            {
                                interneRemplacement.Add(sousItem.Key, sousItem.Value);
                            }
                    }
            }

            var textes = await ObtenirValeursAsync(langue, texteEditeQuery);

            interneRemplacement.TryAdd("textes", textes!);

            var texte = ObtenirValeurAsync(textes, langue, identifiant, texteEditeQuery);

            if (texte is null) return identifiant;

            var t = Handlebars.Compile(texte);

            var result = t(interneRemplacement);

            return result;
        }

        public async Task<string?> ObtenirValeurAsync(string langue, string identifiant, TexteEditeQuery? texteEditeQuery)
        {
            //var textes = await ObtenirValeursAsync(langue, texteEditeQuery);

            //return ObtenirValeurAsync(textes, langue, identifiant, texteEditeQuery);

            return identifiant;
        }

        private string? ObtenirValeurAsync(IDictionary<string, object>? textes, string langue, string identifiant, TexteEditeQuery? texteEditeQuery)
        {
            var cles = identifiant.Split('.', StringSplitOptions.RemoveEmptyEntries);

            IDictionary<string, object> dict = textes ?? new Dictionary<string, object>();

            string? resultat = null;

            foreach (var cle in cles)
            {
                dict.TryGetValue(cle, out var valCle);

                if (valCle is null) return identifiant;

                if (valCle is IDictionary<string, object> valCleDict)
                    dict = valCleDict;
                else
                {
                    resultat = valCle.ToString();
                    break;
                }

                if (dict.ContainsKey("fr") || dict.ContainsKey("en"))
                {
                    resultat = dict.GetLocalizedStringFromStringDict(langue, true);
                }
            }

            return resultat;
        }
    }
}