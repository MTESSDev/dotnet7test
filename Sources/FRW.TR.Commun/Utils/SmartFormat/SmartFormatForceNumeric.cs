using SmartFormat.Core.Extensions;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace FRW.TR.Commun.Utils
{
    /// <summary>
    /// Formatteur custom pour SmartFormat pour verifier si un Array de string
    /// ou un string inclue ou vaut une valeur spécifique
    /// </summary>
    public class SmartFormatForceNumeric : IFormatter
    {
        private string[] names = new[] { "forcenumeric", "forcerNombre" };
        public string[] Names { get { return names; } set { this.names = value; } }

        public bool TryEvaluateFormat(IFormattingInfo formattingInfo)
        {
            if (string.IsNullOrWhiteSpace(formattingInfo.FormatterOptions))
            {
                formattingInfo.Write(Regex.Replace(formattingInfo.CurrentValue?.ToString() ?? string.Empty, "[^0-9]", ""));
                return true;
            }

            var entier = "0";
            var decimales = "00";


            if (formattingInfo.CurrentValue is { })
            {
                var value = string.Empty;

                if (formattingInfo.CurrentValue is string)
                    value = formattingInfo.CurrentValue.ToString();

                if (formattingInfo.CurrentValue is decimal dec)
                    value = dec.ToString(CultureInfo.InvariantCulture);

                if (formattingInfo.CurrentValue is long lng)
                    value = lng.ToString(CultureInfo.InvariantCulture);

                if (formattingInfo.CurrentValue is int intval)
                    value = intval.ToString(CultureInfo.InvariantCulture);

                string[]? nombreComplet = value!.Split('.');
                entier = nombreComplet[0];

                if (nombreComplet.Length == 2)
                {
                    decimales = nombreComplet[1];
                }
            }

            if (formattingInfo.FormatterOptions.Equals("entier"))
            {
                formattingInfo.Write(entier.Trim().PadLeft(1, '0'));
                return true;
            }

            if (formattingInfo.FormatterOptions.Equals("decimales"))
            {
                formattingInfo.Write(decimales.Trim().PadRight(2, '0'));
                return true;
            }

            return false;
        }
    }
}
