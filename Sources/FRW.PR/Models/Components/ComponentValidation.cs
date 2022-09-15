using FRW.TR.Contrats.Composants;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace FRW.PR.Extra.Models.Components
{
    public class ComponentValidation : BaseComponent
    {
        public ComponentValidation() : base(true)
        {
            Validations = new List<ValidationAttribute>();
        }

        public int? Index { get; set; }
        public string GenererNomChamp()
        {
            if (!string.IsNullOrWhiteSpace(FullGroupName))
            {
               return FullGroupName + $".{Index}." + Name;
            }

            return FullName!;
        }

        /// <summary>
        /// Permet d'extraire les validations au format "VueFormulate" vers un dictionnaire.
        /// </summary>
        /// <param name="validationsDict">Validations en format VueFormulate</param>
        /// <returns></returns>
        public override List<Rule>? ParseValidations(IDictionary<object, object>? validationsDict)
        {
            var rules = new List<Rule>();
            if (validationsDict is null) { return rules; }
            foreach (var item in validationsDict)
            {
                var newRule = new Rule();

                newRule.Name = item.Key?.ToString()?.Trim('^');

                if (item.Value != null && newRule.Name is { })
                {
                    newRule.Param = item.Value?.ToString();
                    var valueOptions = newRule.Param?.Split(',') ?? Array.Empty<string>();

                    if (valueOptions.Length > 1)
                    {
                        newRule.Param = valueOptions[0];
                        newRule.Option = valueOptions[1];
                    }
                    else
                    {
                        if (newRule.Name == "min" || newRule.Name == "max")
                        {
                            newRule.Option = "value";
                        }
                    }
                }
                rules.Add(newRule);
            }
            return rules;
        }

        /// <summary>
        /// Convertir les règles de validation en ValidationAttribute de .net
        /// </summary>
        /// <param name="rules">Liste des règles à traiter</param>
        /// <returns>Liste de ValidationAttribute</returns>
        public override IEnumerable<ValidationAttribute> ConvertRules(List<Rule> rules)
        {
            foreach (var rule in rules)
            {
                switch (rule.Name?.ToLower())
                {
                    case "required":
                        yield return new RequiredAttribute() { };
                        break;

                    case "max":

                        if (!OriginalType.Equals("customFile", StringComparison.InvariantCultureIgnoreCase))
                        {
                            if (!string.IsNullOrEmpty(rule.Option) && rule.Option.Contains("value"))
                            {
                                var value = rule.Param?.Replace('.', ',') ?? "0";
                                yield return new RangeAttribute(0d, double.Parse(value));
                                break;
                            }
                        }
                        yield return new MaxLengthAttribute(int.Parse(rule.Param ?? "0"));
                        break;

                    case "min":
                        yield return new MinLengthAttribute(int.Parse(rule.Param ?? "0"));
                        break;

                    case "avant":
                        var dateMax = DateTime.Now;
                        if (!string.IsNullOrEmpty(rule.Param))
                        {
                            if (DateTime.TryParse(rule.Param ?? string.Empty, out var parseDate))
                            {
                                dateMax = parseDate;
                            }
                        }
                        yield return new RangeAttribute(typeof(DateTime), DateTime.MinValue.ToString(), dateMax.ToString());
                        break;

                    case "apres":
                        var dateMin = DateTime.Now;
                        if (!string.IsNullOrEmpty(rule.Param))
                        {
                            if (DateTime.TryParse(rule.Param ?? string.Empty, out var parseDate))
                            {
                                dateMin = parseDate;
                            }
                        }
                        yield return new RangeAttribute(typeof(DateTime), dateMin.ToString(), DateTime.MaxValue.ToString());
                        break;

                    case "cp12":
                        yield return new CP12Attribute();
                        break;

                    case "nam":
                        yield return new NumeroAssuranceMaladieAttribute();
                        break;

                    case "nas":
                        yield return new NumeroAssuranceSocialeAttribute();
                        break;

                    case "codepostal":
                        yield return new CodePostalAttribute();
                        break;

                    case "telephone":
                        yield return new TelephoneAttribute(false);
                        break;
                    case "adresseValide":
                        yield return new AdresseAttribute();
                        break;
                    case "optional":
                        break;
                    default:
                        //throw new InvalidOperationException(item.Name + " validation type unknown");
                        break;
                }
            }
        }

    }
}