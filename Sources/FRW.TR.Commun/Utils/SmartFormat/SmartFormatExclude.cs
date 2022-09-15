using SmartFormat.Core.Extensions;
using System;
using System.Collections.Generic;
using System.Text;

namespace FRW.TR.Commun.Utils
{
    /// <summary>
    /// Formatteur custom pour SmartFormat pour verifier si un Array de string
    /// ou un string n'inclus pas ou vaut pas une valeur spécifique
    /// </summary>
    public class SmartFormatExclude : IFormatter
    {
        private string[] names = new[] { "exclude", "notExist", "neContientPas" };
        public string[] Names { get { return names; } set { this.names = value; } }

        public bool TryEvaluateFormat(IFormattingInfo formattingInfo)
        {
            bool exist = false;

            if (formattingInfo.CurrentValue is object[] array)
            {
                exist = Array.Exists(array, element => (element.ToString() ?? string.Empty).Equals(formattingInfo.FormatterOptions));
            }
            else if (!string.IsNullOrWhiteSpace(formattingInfo.FormatterOptions) && formattingInfo.CurrentValue is IDictionary<object, object> dict)
            {
                exist = dict.TryGetValue(formattingInfo.FormatterOptions, out var value);
            }
            else
            {
                if ((formattingInfo.CurrentValue ?? string.Empty).ToString()!.Equals(formattingInfo.FormatterOptions, StringComparison.InvariantCultureIgnoreCase))
                {
                    exist = true;
                }
            }

            if (!exist)
            {
                formattingInfo.Write(formattingInfo.Format?.RawText ?? string.Empty);
            }

            return true;
        }
    }
}
