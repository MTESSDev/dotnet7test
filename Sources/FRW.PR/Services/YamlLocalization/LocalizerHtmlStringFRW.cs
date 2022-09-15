using System.IO;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Localization;

namespace FRW.PR.Extra.Services.YamlLocalization
{
    /// <summary>
    /// Pris en exemple ici : https://github.com/hiiru/LocalizationBugExample
    /// Puisque le localizer de base en HTML plante quand le string contient {0} ...
    /// la solution est dans la branche "workaround" sur le github du dude
    /// </summary>
    public class LocalizerHtmlStringFRW : LocalizedHtmlString, IHtmlContent
    {
        private bool _hasArguments;

        public LocalizerHtmlStringFRW(string name, string value) : base(name, value)
        {
        }

        public LocalizerHtmlStringFRW(string name, string value, bool isResourceNotFound) : base(name, value, isResourceNotFound)
        {
        }

        public LocalizerHtmlStringFRW(string name, string value, bool isResourceNotFound, object[] arguments) : base(name, value, isResourceNotFound, arguments)
        {
            _hasArguments = true;
        }

        void IHtmlContent.WriteTo(TextWriter writer, HtmlEncoder encoder)
        {
            if (_hasArguments)
            {
                base.WriteTo(writer, encoder);
            }
            else
            {
                writer.Write(Value);
            }
        }
    }
}
