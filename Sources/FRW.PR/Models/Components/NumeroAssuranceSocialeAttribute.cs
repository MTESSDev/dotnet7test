using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace FRW.PR.Extra.Models.Components
{
    /// <summary>
    /// Initialise une nouvelle instance de NasAttribute
    /// </summary>
    public class NumeroAssuranceSocialeAttribute : ValidationAttribute
    {
        public override bool IsValid(object? value)
        {
            var valeur = value as string;
            var expression = @"^\d{9}$";
            return string.IsNullOrWhiteSpace(valeur) ? true : Regex.IsMatch(valeur.Replace(" ", "").Replace("-", ""), expression);
        }
    }
}