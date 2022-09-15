using FRW.PR.Extra.Models.Components;
using Jint;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Linq;
using FRW.TR.Commun.Utils;
using FRW.TR.Contrats;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using FRW.TR.Contrats.Composants;
using FRW.TR.Commun.SchemaFormulaire;
using FRW.TR.Commun.Services;

namespace FRW.PR.Extra.Utils
{
    public static class Validateur
    {

        /// <summary>
        /// FRW312 - Valider le contenu d’un formulaire WEB
        /// </summary>
        /// <param name="data">Données du formulaire</param>
        /// <param name="dynamicForm">Configuration du formulaire</param>
        /// <param name="defaultCfg">Configuration de base (défault)</param>
        /// <param name="config"></param>
        /// <returns>Un model state avec les validations en erreur.</returns>
        public static ModelStateDictionary ValiderFormulaire(IDictionary<object, object> data, IConfigurationApplication config, DynamicForm dynamicForm, DynamicForm? defaultCfg)
        {
            var erreurs = new ModelStateDictionary();
            var inputs = new List<ComponentValidation>();
            var formData = new List<ComponentBinding>();
            var compteurDoublons = new Dictionary<string, int>();

            if (dynamicForm is { })
            {
                ObtenirChampsEffectifsSelonConfiguration(data, config, dynamicForm, defaultCfg, ref inputs, ref formData, ref compteurDoublons);

                foreach (var champ in inputs)
                {
                    if (champ is { })
                    {
                        var valeurChamp = ObtenirValeur(champ, data);

                        if (champ.Validations != null)
                        {
                            foreach (var validation in champ.Validations.ToList())
                            {
                                if (!string.IsNullOrEmpty(champ.Name))
                                {
                                    if (!ValiderChamp(validation, valeurChamp))
                                    {
                                        var nomChamp = champ.GenererNomChamp();
                                        erreurs.AddModelError(nomChamp, validation.FormatErrorMessage(champ.FullName));
                                    }
                                }
                            }
                        }

                        // On injecte les champs manquant qui étaient optionnels
                        if (champ.IsOptional)
                        {
                            if (champ.IsGroup)
                            {
                                // Charger le data du group actuel
                                data!.TryGetValue(champ.FullGroupName!, out var dictActuel);

                                // Créer un nouveau group si celui-ci est vide
                                var listeEnfants = dictActuel as Array;

                                if (listeEnfants is { })
                                {
                                    var enfantCourant = listeEnfants.GetValue((int)champ.Index!) as Dictionary<object, object?>;

                                    enfantCourant?.TryAdd(champ.FullName!, null);
                                }
                            }
                            else
                            {
                                data!.TryAdd(champ.FullName!, null);
                            }
                        }
                    }
                }
            }
            else
            {
                erreurs.AddModelError(nameof(dynamicForm), "Le dynamicForm est null");
            }
            return erreurs;
        }

        public static void ObtenirChampsEffectifsSelonConfiguration(IDictionary<object, object> data, IConfigurationApplication config, DynamicForm dynamicForm, DynamicForm? defaultCfg, ref List<ComponentValidation> inputs, ref List<ComponentBinding> formData, ref Dictionary<string, int> compteurDoublons)
        {
            Component.MergeProp(dynamicForm.Form?["sectionsGroup"], defaultCfg?.Form?["defaults"]);
            Component.GetComponents(false, dynamicForm.Form?["sectionsGroup"], ref formData, null, null, null, null, null);

            // Compter les doublons
            foreach (var component in formData)
            {
                if (component.NameValues is null) continue;

                foreach (var item in component.NameValues)
                {
                    if (!compteurDoublons.TryAdd(item, 1))
                    {
                        compteurDoublons[item]++;
                    }
                }
            }
            GetEffectiveComponents(config, data, data, dynamicForm.Form?["sectionsGroup"], ref inputs, ref compteurDoublons, null, null);
        }

        private static object? ObtenirValeur(ComponentValidation item, IDictionary<object, object> data)
        {
            object? val = null;

            if (item.GroupName != null
                && data.TryGetValue(item.FullGroupName!, out var groupData)
                && groupData is Array arrayItem)
            {
                var key = item.FullName;

                if (arrayItem != null && arrayItem.Length > (item.Index ?? 0))
                {
                    (arrayItem.GetValue(item.Index ?? 0) as IDictionary<object, object>)?.TryGetValue(key!, out val);
                }

                return val;
            }
            else
            {
                ObtenirValeurComponent(ref val, item, data);
                return val;
            }
        }

        private static bool ValiderChamp(ValidationAttribute validation, object? val)
        {
            return validation.IsValid(val);
        }

        private static void ObtenirValeurComponent(ref object? val, ComponentValidation item, IDictionary<object, object> data)
        {
            data.TryGetValue(item.FullName, out val);

            // Nous forçons les array "vide" à null pour que les validation requires marche bien.
            if (val is Array arrayVal && arrayVal.Length == 0)
            {
                val = null;
            }
        }

        public static void GetEffectiveComponents(IConfigurationApplication config, IDictionary<object, object> allData, object? subData, object components, ref List<ComponentValidation> inputs, ref Dictionary<string, int> compteurDoublons, string? prefixId, (string? groupName, bool? isRepeatable, int? maxOccur, int? index)? group)
        {
            var composantsCourants = components as IList<object>;

            if (composantsCourants != null)
            {
                foreach (var item in composantsCourants)
                {
                    var dictItem = item as IDictionary<object, object>;

                    if (dictItem != null)
                    {
                        if (dictItem.TryGetValue("sections", out var sections))
                        {
                            if (dictItem.TryGetValue("v-if", out var vifSections) && !CheckVif(config, ref compteurDoublons, vifSections, prefixId, group, allData, dictItem, ref subData))
                            {
                                continue;
                            }

                            dictItem.TryGetValue("prefixId", out var currentPrefixId);
                            GetEffectiveComponents(config, allData, allData, sections, ref inputs, ref compteurDoublons, currentPrefixId?.ToString(), null);
                        }

                        if (dictItem.TryGetValue("v-if", out var vif) && !CheckVif(config, ref compteurDoublons, vif, prefixId, group, allData, dictItem, ref subData))
                        {
                            continue;
                        }

                        if (dictItem.TryGetValue("components", out var innerComponents))
                        {
                            var newGroupName = group?.groupName;
                            var newIsRepeatable = group?.isRepeatable;
                            int? newMaxOccur = group?.maxOccur;
                            var newSubData = subData;

                            if (dictItem.TryGetValue("type", out var componentType))
                            {
                                if (componentType != null && (componentType.ToString()!.EndsWith("group", StringComparison.InvariantCultureIgnoreCase) || componentType.ToString()!.EndsWith("adresse", StringComparison.InvariantCultureIgnoreCase)))
                                {
                                    newGroupName = dictItem["name"].ToString();

                                    if (dictItem.TryGetValue("repeatable", out var estRepetable))
                                    {
                                        newIsRepeatable = bool.Parse(estRepetable.ToString() ?? "false");
                                    }

                                    if (dictItem.TryGetValue("limit", out var limit))
                                    {
                                        newMaxOccur = int.Parse(limit.ToString() ?? "1");
                                    }

                                    if (newGroupName is { })
                                    {
                                        if (subData is IDictionary<object, object> subDataDict)
                                        {
                                            if (subDataDict.TryGetValue(prefixId + newGroupName, out var subGroup))
                                            {
                                                newSubData = subGroup;
                                            }
                                            else
                                            {
                                                if (dictItem.TryGetValue("validations", out var validations))
                                                    if ((validations as Dictionary<object, object>)!.TryGetValue("optional", out var optionalvalidation))
                                                        newSubData = Array.Empty<object>();
                                                    else
                                                        newSubData = new object[1];
                                                else
                                                    newSubData = new object[1];
                                            }
                                        }
                                        else if (subData is IEnumerable<object> subDataEnumerable)
                                        {
                                            newSubData = subDataEnumerable;
                                        }
                                    }

                                    var len = (newSubData as Array)?.Length;

                                    if (newMaxOccur > len)
                                        newMaxOccur = len;

                                }
                            }

                            var indexDebut = group?.index ?? 0;
                            var indexMax = (group?.isRepeatable ?? false) ? group?.index + 1 : newMaxOccur ?? 1;

                            for (int i = indexDebut; i < indexMax; i++)
                            {
                                GetEffectiveComponents(config, allData, newSubData, innerComponents, ref inputs, ref compteurDoublons, prefixId, (newGroupName, newIsRepeatable, newMaxOccur, i));
                            }
                        }
                    }

                    var inputV = new ComponentValidation();
                    inputV.IsRepeatable = group?.isRepeatable ?? false;
                    inputV.ParseAttributes(dictItem);
                    inputV.GroupName = group?.groupName;
                    inputV.Index = group?.index;
                    inputV.PrefixId = prefixId;

                    if (inputV.Type != TypeInput.SKIP)
                    {
                        inputs.Add(inputV);
                    }
                }
            }
        }

        private static bool CheckVif(IConfigurationApplication config, ref Dictionary<string, int> compteurDoublons, object vif, string? prefixId, (string? groupName, bool? isRepeatable, int? maxOccur, int? index)? group, IDictionary<object, object> allData, IDictionary<object, object> dictItem, ref object? subData)
        {
            //Run v-if
            try
            {
                var options = new Options();

                if (Serilog.Log.IsEnabled(Serilog.Events.LogEventLevel.Debug))
                {
                    options.DebugMode(true);
                    options.DebuggerStatementHandling(Jint.Runtime.Debugger.DebuggerStatementHandling.Clr);
                }

                // Dans les cas d'un groupe, il faut boucler
                // Sinon on ne fait pas de boucle
                //var nombreBoucle = maxOccur ?? 0;

                var engine = new Engine(options)
                              .SetValue("index", group?.index)
                              .SetValue("name", prefixId + group?.groupName)
                              .SetValue("form", allData);

                var evaluate = string.Empty;

                if (Serilog.Log.IsEnabled(Serilog.Events.LogEventLevel.Debug))
                {
                    engine.SetValue("log", new Action<object?>(DebugWriteLineCustom));
                }
                else
                {
                    evaluate = "\r\n function log(val) {return;}\r\n";
                }

                var result = engine
                     .Evaluate(config.ObtenirScriptsJs().Result + @"

                               function val(idChamp, indexe) {
                                    const i = indexe || 0
                                    const champs = idChamp.split('.')
                                    let objetAValider = this.form

                                    for (champ of champs) {
                                        if (Array.isArray(objetAValider)) {
                                            if (objetAValider.length === 0) {
                                                return ''
                                            }
                                            objetAValider = objetAValider[i] || objetAValider[0]
                                        }

                                        if (!objetAValider[`${champ}`] && objetAValider[`${champ}`] !== false) {
                                            return ''
                                        } else {
                                            objetAValider = objetAValider[`${champ}`]
                                        }
                                    }
                                    return objetAValider || ''
                                };"
                             + $" ({vif?.ToString()?.Replace("prefixId$", prefixId, StringComparison.InvariantCultureIgnoreCase)} ? true : false)")
                     .AsBoolean();

                /*Jint.Runtime.Debugger..Step += (sender, info) =>
                {
                    Console.WriteLine("{0}: Line {1}, Char {2}", info.CurrentStatement.ToString(), info.Location.Start.Line, info.Location.Start.Char);
                };*/

                if (!result)
                {
                    // Si le compteur de doublon indique qu'il y a encore des éléments avec le même nom à gérer, on continue
                    if (dictItem.TryGetValue("name", out var itemName))
                    {
                        var nomRecherche = Component.GenerateElementName(new ComponentBinding()
                        {
                            GroupName = group?.groupName,
                            IsRepeatable = group?.isRepeatable ?? false,
                            MaxOccur = (group?.index ?? 0) + 1,
                            Name = itemName.ToString(),
                            PrefixId = prefixId
                        }, null, group?.index ?? 0).FirstOrDefault();

                        var nombreRestant = compteurDoublons.GetValueOrDefault(nomRecherche, -1);

                        if (nombreRestant <= 0)
                        {
                            // Pour le conteneurs de components, il est possible que la
                            // clée soit introuvable, pour le moment on skip
                            if (!dictItem.TryGetValue("components", out var compDict))
                                throw new InvalidOperationException($"Il y plus d'un champ nommé '{nomRecherche}' actifs en même temps. Vérifiez vos v-if.");
                        }
                        else if (nombreRestant > 1)
                        {
                            compteurDoublons[nomRecherche]--;
                            // On ne delete pas le contenu tout de suite, on attends au dernier tour
                            return false;
                        }
                    }

                    dictItem.TryGetValue("sectionGroup", out var sectionGroup);
                    dictItem.TryGetValue("name", out var name);
                    dictItem.TryGetValue("type", out var typeElement);
                    dictItem.TryGetValue("components", out var comp);
                    var groupPrefix = string.IsNullOrEmpty(group?.groupName) ? prefixId : string.Empty;

                    // Si c'est un groupe qui était non effectif, on le retire au complet
                    if ((typeElement ?? string.Empty).ToString()!.EndsWith("group", StringComparison.InvariantCultureIgnoreCase) || (typeElement ?? string.Empty).ToString()!.EndsWith("adresse", StringComparison.InvariantCultureIgnoreCase))
                    {
                        (subData as Dictionary<object, object>)?.Remove(prefixId + name);
                    }
                    // Si c'est une sectionGroup qui est invalide, on traite autrement
                    else if (!(sectionGroup is null))
                    {
                        if (dictItem.TryGetValue("sections", out var comp2) && comp2 is List<object> listComp2)
                        {
                            dictItem.TryGetValue("prefixId", out var innerPrefix);

                            foreach (Dictionary<object, object> elementASupprimer in listComp2)
                            {
                                if (elementASupprimer.TryGetValue("components", out var comp3) && comp3 is List<object> listComp3)
                                {
                                    var localPrefix = (innerPrefix ?? string.Empty).ToString();

                                    foreach (Dictionary<object, object> elementASupprimer2 in listComp3)
                                    {
                                        elementASupprimer2.TryGetValue("name", out var nomAsupprimer);

                                        if (!string.IsNullOrWhiteSpace((string?)nomAsupprimer))
                                            (subData as Dictionary<object, object>)?.Remove(localPrefix + nomAsupprimer);

                                        SupprimerDynamics(elementASupprimer2, localPrefix, (subData as Dictionary<object, object>));
                                    }
                                }
                            }
                        }
                    }
                    // Si c'est une liste, on boucle
                    else if (comp is List<object> dictComp)
                    {
                        DeleteEach(groupPrefix, ref subData, dictComp);
                    }
                    //Un champ normal, supprimé normalement
                    else
                    {
                        if (subData is Dictionary<object, object> subDataDict)
                        {
                            subDataDict?.Remove(groupPrefix + name);
                        }
                        else if (subData is Array subDataArray)
                        {
                            (subDataArray.GetValue(group?.index ?? 0) as Dictionary<object, object>)?.Remove(groupPrefix + name);
                        }
                    }

                    return false;
                }
            }
            catch (Exception)
            {
                throw;
            }

            return true;
        }

        private static void SupprimerDynamics(Dictionary<object, object> elementASupprimer, string? prefix, Dictionary<object, object>? subData)
        {
            elementASupprimer.TryGetValue("type", out var type);

            if (type!.ToString()!.Equals("dynamic", StringComparison.InvariantCultureIgnoreCase))
                if (elementASupprimer.TryGetValue("components", out var comp) && comp is List<object> listComp)
                    foreach (Dictionary<object, object> elementEnfantsASupprimer in listComp)
                    {
                        elementEnfantsASupprimer.TryGetValue("name", out var nomAsupprimer);

                        if (!string.IsNullOrWhiteSpace((string?)nomAsupprimer))
                            subData?.Remove(prefix + nomAsupprimer);

                        SupprimerDynamics(elementEnfantsASupprimer, prefix, subData);
                    }
        }

        private static void DeleteEach(string? prefixId, ref object? subData, List<object> dictComp)
        {
            foreach (Dictionary<object, object> elementASupprimer in dictComp)
            {
                if (elementASupprimer.TryGetValue("components", out var comp) && comp is List<object> dictComp2)
                    DeleteEach(prefixId, ref subData, dictComp2);

                elementASupprimer.TryGetValue("name", out var nomAsupprimer);

                if (nomAsupprimer is { })
                    (subData as Dictionary<object, object>)?.Remove(prefixId + nomAsupprimer);
            }
        }

        private static void DebugWriteLineCustom(object? obj)
        {
            Debug.WriteLine(obj);
        }

    }
}
