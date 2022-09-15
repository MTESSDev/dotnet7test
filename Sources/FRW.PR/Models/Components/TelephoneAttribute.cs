using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace FRW.PR.Extra.Models.Components
{
    public class TelephoneAttribute : ValidationAttribute
    {
        private bool _estOnzeChiffresPermis;

        public TelephoneAttribute(bool estOnzeChiffresPermis)
        {
            _estOnzeChiffresPermis = estOnzeChiffresPermis;
        }

        public override bool IsValid(object? value)
        {
            if (value == null || string.IsNullOrWhiteSpace(value?.ToString()))
            {
                return true;
            }

            return EstTelephone(value?.ToString(), _estOnzeChiffresPermis);
        }

        private bool EstTelephone(string? valeur, bool estOnzeChiffresPermis)
        {
            //On enlève les espaces, tirets et parenthèses
            var nombresEtTexte = Regex.Replace(valeur ?? string.Empty, @"(\s|-|\(|\))", "");
            var pattern = estOnzeChiffresPermis ? @"^\d{10,11}$" : @"^\d{10}$";
            return Regex.IsMatch(nombresEtTexte, pattern);
        }
    }
}