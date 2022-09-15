using SmartFormat.Core.Extensions;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace FRW.TR.Commun.Utils
{
    /// <summary>
    /// Formatteur custom pour SmartFormat pour verifier si un Array de string
    /// ou un string inclue ou vaut une valeur spécifique
    /// </summary>
    public class SmartFormatCodePostal : IFormatter
    {
        private string[] names = new[] { "codePostal" };
        public string[] Names { get { return names; } set { this.names = value; } }

        public bool TryEvaluateFormat(IFormattingInfo formattingInfo)
        {
            bool estFormatter = false;

            if (string.IsNullOrWhiteSpace(formattingInfo.FormatterOptions))
            {
                formattingInfo.Write(Regex.Replace(formattingInfo.CurrentValue?.ToString()?.ToUpper() ?? string.Empty, "\\s+", ""));
                estFormatter = true;
            }

            return estFormatter;
        }
    }
}
