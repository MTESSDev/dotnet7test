using System;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace FRW.PR.Extra.Models.Components
{
    public class CodePostalAttribute : ValidationAttribute
    {
        public override bool IsValid(object? value)
        {
            var valeur = value as string;
            var expression = @"^[A-Z]\d[A-Z]\s?\d[A-Z]\d$";
            return string.IsNullOrWhiteSpace(valeur) ? true : Regex.IsMatch(valeur, expression);
        }
    }
}
