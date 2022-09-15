using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace FRW.TR.Commun.Utils
{
    public static class FormHelpersExtensions
    {
        public static IDictionary<object, object>? GetComponent(this object form, string componentName)
        {
            var obj = form as IDictionary<object, object>;
            var obj2 = form as IList<object>;

            if (obj != null && obj.ContainsKey("sections"))
            {
                return obj["sections"].GetComponent(componentName);
            }
            else if (obj2 != null)
            {
                foreach (var item in obj2)
                {
                    var dictItem = item as IDictionary<object, object>;
                    if (dictItem is { } && dictItem.TryGetValue("components", out var components))
                    {
                        var returnComp = components.GetComponent(componentName);
                        if (returnComp != null)
                        {
                            return returnComp;
                        }
                    }

                    if (dictItem != null && dictItem.TryGetValue("name", out var name))
                    {
                        if (name.Equals(componentName))
                        {
                            return dictItem;
                        }
                    }

                }
            }

            return null;
        }

        public static dynamic Combine(dynamic item1, IDictionary<object, object> item2)
        {
            var dictionary1 = (IDictionary<object, object>)item1;
            var result = new Dictionary<object, object>();
            var d = result as IDictionary<object, object>;

            foreach (var pair in item2)
            {
                d.Add(pair.Key, pair.Value);
            }
            foreach (var pair in dictionary1)
            {
                d.Add(pair.Key, pair.Value);
            }

            return result;
        }

        public static string Sanitize(this string? value)
        {
            if (value is null) return string.Empty;

            return value.Replace("'", "\\'");
        }

        public static dynamic? GetLocalizedObject(this IDictionary<object, object>? dict)
        {
            if (dict is null) return null;
            dict.TryGetValue(CultureInfo.CurrentUICulture.TwoLetterISOLanguageName, out var value);
            if (value is null)
            {
                var first = dict.FirstOrDefault();
                return $"[{first.Key},{first.Value}]";
            }

            return value;
        }

        public static string? GetLocalizedStringFromStringDict(this object obj, string culture, bool returnFirstIfNotFound)
        {
            if (obj is null) return null;

            if (obj is IDictionary<string, object> dict)
            {
                dict.TryGetValue(culture.ToLower(), out var value);
                if (value is null)
                {
                    var first = dict.FirstOrDefault();

                    if (returnFirstIfNotFound)
                        return first.Value.ToString();
                    else
                        return $"[{first.Key},{first.Value}]";
                }
                return value.ToString();
            }

            return obj.ToString();
        }

        public static string? GetLocalizedString(this object obj, string? culture = null, bool returnFirstIfNotFound = true)
        {
            if (obj is null) return null;

            culture ??= CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;

            if (obj is IDictionary<object, object> dict)
            {
                dict.TryGetValue(culture.ToLower(), out var value);
                if (value is null)
                {
                    var first = dict.FirstOrDefault();

                    if (returnFirstIfNotFound)
                        return first.Value.ToString();
                    else
                        return $"[{first.Key},{first.Value}]";
                }
                return value.ToString();
            }
            else if (obj is Newtonsoft.Json.Linq.JObject jsonObj)
            {
                // Passe passe spéciale pour le JSON
                var props = jsonObj.Properties();

                foreach (var langue in props)
                {
                    if (langue.Name.Equals(culture, StringComparison.InvariantCultureIgnoreCase))
                    {
                        return langue.Value.ToString();
                    }
                }

                return props.FirstOrDefault()?.Value.ToString();
            }

            return obj.ToString();
        }

        public static string? GetLocalizedString(this IDictionary<object, object> obj, string? culture = null, bool returnFirstIfNotFound = true)
        {
            if (obj is null) return null;

            culture ??= CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;

            if (obj is IDictionary<object, object> dict)
            {
                dict.TryGetValue(culture.ToLower(), out var value);
                if (value is null)
                {
                    var first = dict.FirstOrDefault();

                    if (returnFirstIfNotFound)
                        return first.Value.ToString();
                    else
                        return $"[{first.Key},{first.Value}]";
                }
                return value.ToString();
            }
            else if (obj is Newtonsoft.Json.Linq.JObject jsonObj)
            {
                // Passe passe spéciale pour le JSON
                var props = jsonObj.Properties();

                foreach (var langue in props)
                {
                    if (langue.Name.Equals(culture, StringComparison.InvariantCultureIgnoreCase))
                    {
                        return langue.Value.ToString();
                    }
                }

                return props.FirstOrDefault()?.Value.ToString();
            }

            return obj.ToString();
        }

        public static void TryAdd<TKey, TValue>(this IDictionary<TKey, TValue>? dictionary, IDictionary<TKey, TValue>? source) where TKey : notnull
        {
            if (dictionary is null) throw new Exception("Target dict need to be init.");

            if (source is null) return;

            foreach (var item in source)
            {
                dictionary?.TryAdd(item.Key, item.Value);
            }
        }
    }
}