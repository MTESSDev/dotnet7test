using Stubble.Core;
using Stubble.Core.Builders;
using Stubble.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace FRW.TR.Commun.Utils
{
    public class FormHelpers
    {

        public static bool IsStubbleInitialized { get; set; }
        private static StubbleVisitorRenderer? _stubble;

        private static readonly Func<HelperContext, dynamic?, string> jsObject = FormHelpers.JsObject;
        private static readonly Func<HelperContext, IDictionary<object, object>?, string> generateValidations = FormHelpers.GenerateValidations;
        private static readonly Func<HelperContext, IDictionary<object, object>?, string> i18n = FormHelpers.I18n;
        private static readonly Func<HelperContext, IEnumerable<dynamic>, string> recursiveComponents = FormHelpers.RecursiveComponents;
        private static readonly Func<HelperContext, IEnumerable<dynamic>, string> recursiveComponentsNoPrefix = FormHelpers.RecursiveComponentsNoPrefix;
        private static readonly Func<HelperContext, string, object, string> generateClasses = FormHelpers.GenerateClasses;
        private static readonly Func<HelperContext, string, object, string> generateDefaults = FormHelpers.GenerateDefaults;
        private static readonly Func<HelperContext, object, string> generateVif = FormHelpers.GenerateVif;

        private static string GenerateClasses(HelperContext context, string type, dynamic component)
        {
            var dictComponent = component as IDictionary<object, object>;

            if (dictComponent is null) return string.Empty;

            var input = InternalGenerateClassesFromObject(dictComponent, "inputClasses", type, ":input-class", component["Form"], "inputDefaultClasses");
            var outer = InternalGenerateClassesFromObject(dictComponent, "outerClasses", type, ":outer-class", component["Form"], "outerDefaultClasses");

            return $"{input} {outer}";
        }

        private static string GenerateDefaults(HelperContext context, string type, dynamic component)
        {
            var dictComponent = component as IDictionary<object, object>;

            if (dictComponent is null) return string.Empty;

            var defaultDict = context.Lookup<Dictionary<object, object>>($"Form.defaults.{type}");

            if (defaultDict != null)
            {
                if (defaultDict.TryGetValue("type", out var typeOverride))
                {
                    component["baseType"] = typeOverride;
                }

                // Copier les attributs defaults
                if (component is IDictionary<object, object> compDict)
                {
                    foreach (var item in defaultDict)
                    {
                        if (!compDict.TryAdd(item.Key, item.Value))
                        {
                            // Si l'élément est déjà présent, mais qu'il s'agit d'un dictionnaire, on tente de copier ses enfants
                            if (item.Value is IDictionary<object, object> itemKeyDict
                                && compDict[item.Key] is IDictionary<object, object> compDictChild)
                            {
                                compDictChild.TryAdd(itemKeyDict);
                            }
                        }
                    }
                }


            }

            return string.Empty;
        }

        private static string GenerateVif(HelperContext context, object vif)
        {
            var prefixId = context.Lookup<object>("vifPrefixId");
            var enableVif = context.Lookup<object>("Form.enableVif");

            if (bool.Parse(enableVif?.ToString() ?? "false"))
            {
                return $" v-if=\"{InternalGenerateVif(vif?.ToString(), prefixId?.ToString())}\"";
            }

            return string.Empty;
        }

        private static string InternalGenerateClassesFromObject(IDictionary<object, object> dictComponent, string yamlKey,
                                                                string type, string attributeName, IDictionary<object, object>? form,
                                                                string defaultDictAttributeName)
        {
            if (form is null) { throw new ArgumentNullException(nameof(form) + " parameter is empty"); }

            form.TryGetValue(defaultDictAttributeName, out var defaultClassesObj);
            var defaultClasses = defaultClassesObj as IDictionary<object, object>;

            if (defaultClasses is null) defaultClasses = new Dictionary<object, object>();

            object? customClasses = null;

            dictComponent?.TryGetValue(yamlKey, out customClasses);

            var listInputCustom = customClasses?.ToString()?.Split(' ') ?? new string[] { };

            if (defaultClasses.TryGetValue(type, out var classesType))
            {
                return InternalGenerateClasses(attributeName, (classesType as string) ?? string.Empty, listInputCustom);
            }
            else if (defaultClasses.TryGetValue("default", out var classesDefault))
            {
                return InternalGenerateClasses(attributeName, (classesDefault as string) ?? string.Empty, listInputCustom);
            }
            else
            {
                return InternalGenerateClasses(attributeName, "", listInputCustom);
            }
        }


        private static string InternalGenerateClasses(string attributeName, string classes, string[] listInputCustom)
        {
            var classesList = (classes ?? "").Split(' ', StringSplitOptions.RemoveEmptyEntries).ToList();
            classesList.AddRange(listInputCustom);

            if (!classesList.Any())
            {
                return string.Empty;
            }

            return $"{attributeName}=\"['{string.Join("', '", classesList.Select(k => k.ToString().Sanitize()))}']\"";
        }

        public static StubbleVisitorRenderer Stubble
        {
            get
            {
                if (!IsStubbleInitialized)
                {
                    var helpers = new Stubble.Helpers.Helpers()
                            .Register("GenerateClasses", generateClasses)
                            .Register("GenerateDefaults", generateDefaults)
                            .Register("GenerateVif", generateVif)
                            .Register("RecursiveComponents", recursiveComponents)
                            .Register("RecursiveComponentsNoPrefix", recursiveComponentsNoPrefix)
                            .Register("i18n", i18n)
                            .Register("JsObject", jsObject)
                            .Register("GenerateValidations", generateValidations);
                    _stubble = new StubbleBuilder()
                                .Configure(conf =>
                                {
                                    conf.AddHelpers(helpers);
                                    conf.SetIgnoreCaseOnKeyLookup(true);
                                })
                                .Build();
                }

                return _stubble ?? throw new Exception("_stubble not init.");
            }
        }

        public static string JsObject(HelperContext context, dynamic? dict)
        {
            if (dict is null) return string.Empty;

            Dictionary<object, object> vueDict = new Dictionary<object, object>();
            if (dict is string)
            {
                if (dict == "yesno")
                {
                    Dictionary<object, object> tempDict = new Dictionary<object, object>();
                    tempDict.Add("fr", "Oui");
                    tempDict.Add("en", "Yes");
                    vueDict.Add("true", I18n(context, tempDict));

                    tempDict.Clear();
                    tempDict.Add("fr", "Non");
                    tempDict.Add("en", "No");
                    vueDict.Add("false", I18n(context, tempDict));
                }
            }
            else if (dict is List<object>)
            {
                return $"['{string.Join("', '", ((List<object>)dict).Select(k => k.ToString().Sanitize()))}']";
            }
            else
            {
                var checkVal = dict as Dictionary<object, object>;

                if (checkVal is null) return string.Empty;

                if (checkVal.ContainsKey("fr"))
                {
                    return JsObject(context, checkVal.GetLocalizedObject());
                }
                else
                {
                    if (checkVal.FirstOrDefault().Value is Dictionary<object, object>)
                    {
                        foreach (var val in checkVal)
                        {
                            vueDict.Add(val.Key, FormHelpersExtensions.GetLocalizedObject(val.Value as Dictionary<object, object>));
                        }
                    }
                }

                if (!vueDict.Any())
                {
                    vueDict = dict;
                }
            }

            return "[" + string.Join(", ", vueDict.Select(kv => $"{{value: '{kv.Key}', label: '{kv.Value?.ToString().Sanitize()}'}}").ToArray()) + "]";
        }

        public static string GenerateValidations(HelperContext context, IDictionary<object, object>? dict)
        {
            if (dict is null) return string.Empty;

            string templateRetour = string.Empty;

            // ------------------------
            // Partie "validation"
            // ------------------------
            Dictionary<object, object>? dictValidations = new Dictionary<object, object>() { { "bail", "true" } };

            if (dict.TryGetValue("validations", out var validations))
            {
                Dictionary<object, object>? dictValidationsElement = validations as Dictionary<object, object>;
                if (dictValidationsElement == null
                    || !(dictValidationsElement.ContainsKey("optional")
                         || dictValidationsElement.ContainsKey("accepted")))
                {
                    dictValidations.TryAdd("required", "trim");
                }
                dictValidations.TryAdd(validations as Dictionary<object, object>);
            }
            else
            {
                dictValidations.TryAdd("required", "trim");
            }

            var element = "[";

            foreach (var item in dictValidations)
            {
                var val = item.Value?.ToString() ?? string.Empty;

                if (val.StartsWith('/') && val.EndsWith('/')) // Gérer les regex
                    element += $"['{item.Key}', {val}], ";
                else
                    element += $"['{item.Key}', '{string.Join("','", val.Split(','))}'], ";

            }

            templateRetour += $":validation=\"{element}]\" ";

            // ------------------------
            // Partie "validation-messages"
            // ------------------------
            if (dict.TryGetValue("validation-messages", out var validationMessages)
                && validationMessages is Dictionary<object, object> validationMessagesDict)
            {
                var messages = "{";

                foreach (var validationMessage in validationMessagesDict)
                {
                    var valeur = validationMessage.Value.GetLocalizedString();
                    valeur = valeur?.Replace("'", "&apos;") ?? string.Empty;

                    if (valeur.Contains('`'))
                    {
                        messages += $"{validationMessage.Key}: {valeur?.Replace("'", "&apos;")}, ";
                    }
                    else
                    {
                        messages += $"{validationMessage.Key}: `{valeur?.Replace("'", "&apos;")}`, ";
                    }
                }

                templateRetour += $":validation-messages=\"{messages.Replace("\"", @"&quot;")}}}\" ";

            }

            return templateRetour;
        }

        public static string I18n(HelperContext context, IDictionary<object, object>? dict)
        {
            return FormHelpersExtensions.GetLocalizedObject(dict)?.ToString() ?? string.Empty;
        }

        public static string? InternalGenerateVif(string? vif, string? prefixId)
        {
            return vif?.Replace("prefixId$", prefixId ?? string.Empty, StringComparison.InvariantCultureIgnoreCase);
        }

        public static string RecursiveComponentsNoPrefix(HelperContext context, IEnumerable<dynamic> components)
        {
            return RecursiveComponentsInterne(context, components, true);
        }

        public static string RecursiveComponents(HelperContext context, IEnumerable<dynamic> components)
        {
            return RecursiveComponentsInterne(context, components, false);
        }

        internal static string RecursiveComponentsInterne(HelperContext context, IEnumerable<dynamic> components, bool noPrefixIds)
        {
            string html = string.Empty;

            var positionBlocCode = context.Lookup<int?>("__positionBlocCode") ?? 0;

            var form = new Dictionary<object, object>();
            form.TryAdd("Form", context.Lookup<object>("Form"));
            form.TryAdd("__positionBlocCode", positionBlocCode);
            form.TryAdd("vifPrefixId", context.Lookup<object>("prefixId"));

            if (noPrefixIds)
                form.Remove("prefixId");
            else
                form.TryAdd("prefixId", context.Lookup<object>("prefixId"));

            foreach (var component in components)
            {
                if (component is null) continue;

                form["__positionBlocCode"] = positionBlocCode++;

                if (component is IDictionary<object, object> compDict && !compDict.TryGetValue("type", out var type))
                    compDict.Add("type", "text"); // Type par défaut

                if (context.Lookup<Dictionary<object, object>>("Form.templates").TryGetValue(component["type"], out object template))
                {
                    html += Stubble.Render(template.ToString(), FormHelpersExtensions.Combine(component, form));
                }
                else if (context.Lookup<Dictionary<object, object>>("Form.templates").TryGetValue("input", out object? inputTemplate))
                {
                    html += Stubble.Render(inputTemplate.ToString(), FormHelpersExtensions.Combine(component, form));
                }

                // Ici on ajoute le bloc "code" pour le P700U
                if ((bool)((dynamic)form["Form"]).ContainsKey("__blocCode"))
                {
                    html += TraiterBlocCode(context, form, component);
                }
            }

            return html;
        }

        private static string TraiterBlocCode(HelperContext context, Dictionary<object, object> form, dynamic component)
        {
            var html = string.Empty;

            if (!context.Lookup<IDictionary<object, object>>(".").TryGetValue("type", out var typeParent)
                || (!typeParent?.ToString()?.EndsWith("group", StringComparison.InvariantCultureIgnoreCase) ?? true))
            {
                context.Lookup<IDictionary<object, object>>(".").TryGetValue("id", out var sectionParent);
                var forr = ((IDictionary<object, object>)form["Form"])["__blocCode"].ToString();

                var skip = true;

                // Si le composant contient un renderCode: false dans les additionnals on le skip, pas de bloc code
                if (((IDictionary<object, object>)component).TryGetValue("afficherBlocCode", out var adds))
                    if (bool.Parse(adds.ToString()!))
                        skip = false;

                if (!skip && sectionParent is { })
                {
                    var spl = forr?.Split($" id: ");

                    var group = spl!.Where(e => e.StartsWith(sectionParent.ToString()!)).FirstOrDefault();
                    int posTiret = -1;

                    foreach (var ligne in group!.Split(Environment.NewLine))
                    {
                        posTiret = ligne.IndexOf("  - ");
                        if (posTiret != -1)
                            break;
                    }

                    posTiret += 4;

                    var splitSearch = "- ".PadLeft(posTiret);

                    List<string> cComponents = new List<string>();

                    bool start = false;

                    foreach (var ligne in group.Split(Environment.NewLine))
                    {
                        var val = ligne.Split(splitSearch);

                        if (val[0].StartsWith(' '))
                        {
                            if (start)
                                cComponents[^1] += ligne + "\r\n";
                            else continue;
                        }
                        else
                        {
                            if (string.IsNullOrWhiteSpace(ligne))
                            {
                                start = false;
                                continue;
                            }

                            cComponents.Add(ligne.Replace(splitSearch, "") + "\r\n");
                            start = true;
                        }
                    }

                    var final = string.Empty;

                    foreach (var ligne in cComponents[int.Parse(form["__positionBlocCode"].ToString()!) + 1].Split(Environment.NewLine))
                    {
                        if (string.IsNullOrWhiteSpace(ligne))
                            break;

                        final += ligne + "\r\n";
                    }

                    var first = true;
                    var newfinal = string.Empty;

                    foreach (var ligne in TrimIndent(final).Split(Environment.NewLine))
                    {
                        if (ligne.Contains("afficherBlocCode"))
                            continue;

                        if (first && ligne[0] != '#')
                        {
                            newfinal += "- ";
                            first = false;
                        }
                        else
                            newfinal += "  ";

                        newfinal += ligne + "\r\n";
                    }

                    newfinal = newfinal.TrimEnd(new char[] { '\r', '\n', ' ' });

                    html += $"<div class=\"bloc-code\">" +
                              $"<span class=\"texte-confirmation-copie\">Copié!</span>" +
                              $"<button type=\"button\" class=\"utd-btn secondaire compact\" title=\"Copier\"><span aria-hidden=\"true\" class=\"utd-icone-svg clipboard md\"></span></button>" +
                               "<pre>" +
                                  $"<code>{WebUtility.HtmlEncode(newfinal)}</code>" +
                              $"</pre>" +
                            $"</div>";
                }
            }

            return html;
        }

        public static string TrimIndent(string s)
        {
            string[] lines = s.Split('\n');

            IEnumerable<int> firstNonWhitespaceIndices = lines
                .Skip(1)
                .Where(it => it.Trim().Length > 0)
                .Select(IndexOfFirstNonWhitespace);

            int firstNonWhitespaceIndex;

            if (firstNonWhitespaceIndices.Any()) firstNonWhitespaceIndex = firstNonWhitespaceIndices.Min();
            else firstNonWhitespaceIndex = -1;

            if (firstNonWhitespaceIndex == -1) return s;

            IEnumerable<string> unindentedLines = lines.Select(it => UnindentLine(it, firstNonWhitespaceIndex));
            return String.Join("\n", unindentedLines);
        }

        private static string UnindentLine(string line, int firstNonWhitespaceIndex)
        {
            if (firstNonWhitespaceIndex < line.Length)
            {
                if (line.Substring(0, firstNonWhitespaceIndex).Trim().Length != 0)
                {
                    return line;
                }

                return line.Substring(firstNonWhitespaceIndex, line.Length - firstNonWhitespaceIndex);
            }

            return line.Trim().Length == 0 ? "" : line;
        }

        private static int IndexOfFirstNonWhitespace(string s)
        {
            char[] chars = s.ToCharArray();
            for (int i = 0; i < chars.Length; i++)
            {
                if (chars[i] != ' ' && chars[i] != '\t') return i;
            }

            return -1;
        }

    }

}