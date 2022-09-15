using System;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace FRW.PR.Extra.Models.Components
{
    public class NumeroAssuranceMaladieAttribute : ValidationAttribute
    {
        public NumeroAssuranceMaladieAttribute()
        {
        }

        public NumeroAssuranceMaladieAttribute(Func<string> errorMessageAccessor) : base(errorMessageAccessor)
        {
        }

        public NumeroAssuranceMaladieAttribute(string errorMessage) : base(errorMessage)
        {
        }

        public override bool IsValid(object? value)
        {
            var valeur = value as string;
            var expression = @"^[A-Z]{4}\d{8}$";
            return string.IsNullOrWhiteSpace(valeur) ? true : Regex.IsMatch(valeur, expression);
        }
    }
}
