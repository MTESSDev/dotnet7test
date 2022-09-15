using System;
using System.Collections.Generic;
using System.Globalization;
using FRW.PR.Extra.Services.YamlLocalization;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.Extensions.Localization;

namespace FRW.PR.Extra.Services
{

    public class YamlHtmlLocalizer : IHtmlLocalizer
    {
        private readonly IStringLocalizer _localizer;

        public YamlHtmlLocalizer(IStringLocalizer localizer)
        {
            _localizer = localizer;
        }

        public virtual LocalizedHtmlString this[string name]
        {
            get
            {
                return ToHtmlString(GetString(name));
            }
        }

        /// <summary>
        /// </summary>
        /// <remarks>Appel le GetString pour avoir les valeurs des arguments afficher dans le message et ne pas voir {0}</remarks>
        /// <param name="name"></param>
        /// <param name="arguments"></param>
        /// <returns></returns>
        public virtual LocalizedHtmlString this[string name, params object[] arguments]
        {
            get
            {
                return ToHtmlString(GetString(name, arguments));
            }
        }

        public virtual LocalizedString GetString(string name)
        {
            return _localizer[name];
        }

        public virtual LocalizedString GetString(string name, params object[] arguments)
        {
            return _localizer[name, arguments];
        }

        [Obsolete("Veuillez utiliser l'autre surcharge SVP.")]
        public virtual IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures)
        {
            return _localizer.GetAllStrings(includeParentCultures);
        }

        public virtual IHtmlLocalizer WithCulture(CultureInfo culture)
        {
            return new YamlHtmlLocalizer(_localizer);
        }

        protected virtual LocalizerHtmlStringFRW ToHtmlString(LocalizedString result)
        {
            if (result is null)
            {
                throw new ArgumentNullException(nameof(result));
            }
            return new LocalizerHtmlStringFRW(result.Name, result.Value, result.ResourceNotFound);
        }

        protected virtual LocalizerHtmlStringFRW ToHtmlString(LocalizedString result, object[] arguments)
        {
            if (result is null)
            {
                throw new ArgumentNullException(nameof(result));
            }

            return new LocalizerHtmlStringFRW(result.Name, result.Value, result.ResourceNotFound, arguments);
        }
    }
}
