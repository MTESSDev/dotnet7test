using SmartFormat.Core.Extensions;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace FRW.TR.Commun.Utils
{
    /// <summary>
    /// Formatteur custom pour SmartFormat pour verifier si un Array de string
    /// ou un string inclue ou vaut une valeur spécifique
    /// </summary>
    public class SmartFormatInclude : IFormatter
    {
        private string[] names = new[] { "include" };
        public string[] Names { get { return names; } set { this.names = value; } }

        public bool TryEvaluateFormat(IFormattingInfo formattingInfo)
        {
            bool exist = false;
            var splitFormatterOptions = formattingInfo.FormatterOptions?.Split('|');    //Un ou l'autre de ces éléments doit être présent
            var splitFormat = formattingInfo.Format?.Split('|');                        //Option 1 si présent, sinon option 2

            foreach (var formatterOption in splitFormatterOptions!)
            {
                if (formattingInfo.CurrentValue is object[] array)
                {
                    exist = Array.Exists(array, element => (element.ToString() ?? string.Empty).Equals(formatterOption));
                }
                else if (!string.IsNullOrWhiteSpace(formatterOption) && formattingInfo.CurrentValue is IDictionary<object, object> dict)
                {
                    exist = dict.TryGetValue(formatterOption, out var value);
                }
                else
                {
                    if ((formattingInfo.CurrentValue ?? string.Empty).ToString()!.Equals(formatterOption, StringComparison.InvariantCultureIgnoreCase))
                    {
                        exist = true;
                    }
                }
                if (exist)
                    break;
            }

            var possedeOptionsMultiples = splitFormat?.Skip(1)?.Any() ?? false;

            if (exist)
            {
                if (possedeOptionsMultiples) //S'il y a plus qu'une option de formattage - pour l'instant true et false seulement
                {
                    formattingInfo.Write(splitFormat?.First()?.RawText ?? string.Empty);
                }
                else
                {
                    formattingInfo.Write(formattingInfo.Format?.RawText ?? string.Empty);
                }
            }
            else if (possedeOptionsMultiples)
            {
                formattingInfo.Write(splitFormat?.Skip(1).First().RawText ?? string.Empty);
            }

            return true;
        }
    }
}