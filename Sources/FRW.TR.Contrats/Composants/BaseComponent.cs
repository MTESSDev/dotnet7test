using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace FRW.TR.Contrats.Composants
{
    public abstract class BaseComponent
    {
        protected readonly bool _isParseValidationsEnabled;

        public BaseComponent(bool IsParseValidationsEnabled)
        {
            _isParseValidationsEnabled = IsParseValidationsEnabled;
            Validations = new List<ValidationAttribute>();
        }

        public string? Name { get; set; }
        public string? FullName { get { return $"{(IsGroup ? string.Empty : PrefixId)}{Name}"; } }
        public string? PrefixId { get; set; }
        public string? GroupName { get; set; }
        public string? ParentType { get; set; }
        public string? FullGroupName { get { return GroupName is { } ? $"{PrefixId}{GroupName}" : null; } }
        public bool IsGroup { get { return !string.IsNullOrEmpty(GroupName); } }
        public bool IsRepeatable { get; set; }
        public TypeInput Type
        {
            get
            {
                switch (OriginalType.ToUpper())
                {
                    case "HIDDEN":
                    case "TEXT":
                    case "NUMBER":
                    case "TEL":
                    case "EMAIL":
                    case "MONTH":
                    case "WEEK":
                    case "TIME":
                    case "SEARCH":
                    case "URL":
                    case "PASSWORD":
                    case "TEXTAREA":
                    case "NOMBRE":
                        return TypeInput.TEXT;
                    case "DATE":
                    case "DATETIME-LOCAL":
                        return TypeInput.DATE;
                    case "SELECT":
                        return TypeInput.SELECT;
                    case "CHECKBOX":
                        return TypeInput.CHECKBOX;
                    case "RADIO":
                        return TypeInput.RADIO;
                    /*case "REPEATABLEGROUP":
                        return TypeInput.GROUP;*/
                    case "CUSTOMFILE":
                        return TypeInput.FILE;
                    case "ADRESSE":
                        return TypeInput.GROUP;
                    default:
                        return TypeInput.SKIP;
                }
            }
        }
        public string OriginalType { get; set; } = String.Empty;

        public string FlatPath
        {
            get
            {
                if (IsGroup)
                {
                    return $"{FullGroupName}.{FullName}";
                }
                else
                {
                    return $"{FullName}";
                }
            }
        }

        public string Path(int? groupOccurence)
        {
            if (IsGroup)
            {
                return $"{FullGroupName}.{groupOccurence}.{FullName}";
            }
            else
            {
                return $"{FullName}";
            }
        }

        public IDictionary<object, object>? AcceptedValues { get; set; }
        public Dictionary<string, string?>? Additionnals { get; set; }

        public List<ValidationAttribute> Validations { get; set; }
        public object? Label { get; private set; }
        public bool IsOptional { get; private set; }
        public bool HasVIf { get; private set; }

        public abstract IEnumerable<ValidationAttribute>? ConvertRules(List<Rule> rules);

        /// <summary>
        /// Extraire les attributs du fichier de config VueFormulate
        /// </summary>
        /// <param name="attrDict">Dictionnaire d'attributs VueFormulate</param>
        public void ParseAttributes(IDictionary<object, object>? attrDict)
        {
            if (attrDict is null) return;
            Validations.Add(new RequiredAttribute() { });

            foreach (var item in attrDict)
            {
                switch (item.Key.ToString()?.ToUpper())
                {
                    case "NAME":
                        Name = item.Value?.ToString();
                        break;
                    case "LABEL":
                        Label = item.Value;
                        break;
                    case "V-IF":
                        HasVIf = (item.Value is { });
                        break;
                    case "TYPE":
                        OriginalType = item.Value?.ToString()!;
                        break;
                    case "ADDITIONALS":
                        foreach (var additional in (item.Value as Dictionary<object, object>)!)
                        {
                            Additionnals ??= new Dictionary<string, string?>();

                            switch (additional.Key.ToString()?.ToUpper())
                            {
                                case "UNITE":
                                    _ = Additionnals.TryAdd("UNITE", additional.Value.ToString());
                                    break;
                            }
                        }
                        break;
                    case "VALIDATIONS":
                        if (!_isParseValidationsEnabled) { break; };

                        var validationsDict = item.Value as IDictionary<object, object>;
                        if (validationsDict is { } && validationsDict.ContainsKey("optional"))
                        {
                            IsOptional = true;
                            //Enlever le required par défaut
                            Validations.Clear();
                        }
                        var rules = ParseValidations(validationsDict);
                        Validations.AddRange(ConvertRules(rules!)!);
                        break;
                    case "OPTIONS":
                        if ((item.Value?.ToString() ?? string.Empty).Equals("yesno", StringComparison.InvariantCultureIgnoreCase))
                        {
                            AcceptedValues = new Dictionary<object, object>() { { "true", true }, { "false", false }, };
                        }
                        else
                        {
                            if ((item.Value?.GetType()?.Name ?? string.Empty).StartsWith("List"))
                            {
                                AcceptedValues = (item.Value as List<object>).ToDictionary(x => x, x => x);
                            }
                            else
                            {
                                AcceptedValues = item.Value as IDictionary<object, object>;
                            }
                        }
                        break;
                    default:
                        break;
                }
            }
        }

        public abstract List<Rule>? ParseValidations(IDictionary<object, object>? validationsDict);
    }


}

