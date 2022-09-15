using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace FRW.PR.Extra.Models.Components
{
    /// <summary>
    /// Initialise une nouvelle instance de CP12Attribute
    /// </summary>
    public class CP12Attribute : ValidationAttribute
    {
        public override bool IsValid(object? value)
        {
            var valeur = value as string;
            var expression = @"^[A-Z]{4}\d{6}[A-Z\d]\d$";
            return string.IsNullOrWhiteSpace(valeur) ? true : Regex.IsMatch(valeur, expression);
        }
    }
}