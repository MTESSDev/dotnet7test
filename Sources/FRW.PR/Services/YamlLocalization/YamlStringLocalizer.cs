using System.Collections.Generic;
using System.Globalization;
using FRW.TR.Commun.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Localization;
using Serilog;
using System;
using SmartFormat;
using System.Linq;
using FRW.TR.Commun.Services;
using Microsoft.AspNetCore.Routing;

namespace FRW.PR.Extra.Services
{
    public class YamlStringLocalizer : IStringLocalizer
    {
        private static ILogger FrwLog => Log.ForContext<YamlStringLocalizer>();
        private readonly IHttpContextAccessor _httpContextAccessor;

        private ITexteEditeService TexteEditeService
        {
            get
            {
                return (ITexteEditeService)_httpContextAccessor.HttpContext.RequestServices.GetService(typeof(ITexteEditeService));
            }
        }

        public YamlStringLocalizer(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures)
        {
            throw new NotImplementedException();

            /*var textesEdites = TexteEditeService.ObtenirValeursAsync(CultureInfoExtensions.LangueUtilisateur, null).Result;

            if (textesEdites != null)
            {
                foreach (var texteEdite in textesEdites)
                {
                    yield return new LocalizedString(texteEdite.Key, texteEdite.Value);
                }
            }
            */
        }

        public IStringLocalizer WithCulture(CultureInfo culture)
        {
            return new YamlStringLocalizer(_httpContextAccessor);
        }

        [Obsolete("Veuillez utiliser l'autre surcharge SVP.")]
        public LocalizedString this[string idTexteEdite]
        {
            get
            {
                var texteEdite = TexteEditeService.ObtenirValeurAsync(CultureInfoExtensions.LangueUtilisateur, idTexteEdite, null).Result;
                
                return new LocalizedString(idTexteEdite, texteEdite ?? idTexteEdite);
            }
        }

        public LocalizedString this[string idTexteEdite, params object[] mixedArguments]
        {
            get
            {
                var parameters = SeparerTexteQueryObject(mixedArguments);

                var texteEdite = TexteEditeService.ObtenirValeurAvecRemplacementAsync(CultureInfoExtensions.LangueUtilisateur, idTexteEdite, parameters.texteEditeQuery, parameters.arguments).Result;
                var textFormat = Smart.CreateDefaultSmartFormat();

                textFormat.Settings.FormatErrorAction = SmartFormat.Core.Settings.ErrorAction.MaintainTokens;
                textFormat.Settings.ParseErrorAction = SmartFormat.Core.Settings.ErrorAction.MaintainTokens;

                textFormat.OnFormattingFailure += (sender, args) =>
                {
                    FrwLog.Erreur(nameof(LocalizedString), args, $"Texte édité '{idTexteEdite}' en erreur. Position : {args.ErrorIndex}");
                };
                textFormat.Parser.OnParsingFailure += (sender, args) =>
                {
                    FrwLog.Erreur(nameof(LocalizedString), args, $"Texte édité '{idTexteEdite}' en erreur.");
                };

                return new LocalizedString(idTexteEdite, textFormat.Format(texteEdite, parameters.arguments));
            }
        }

        private (TexteEditeQuery? texteEditeQuery, object[] arguments) SeparerTexteQueryObject(object[] arguments)
        {
            TexteEditeQuery? texteEditeQueryRetour = null;

            var argumentsList = arguments.ToList();

            int max = arguments.Length;

            for (int i = 0; i < max; i++)
            {
                if (argumentsList[i] is TexteEditeQuery texteEditeQuery)
                {
                    texteEditeQueryRetour = texteEditeQuery;
                    argumentsList.RemoveAt(i);
                    max--;
                }
            }

            return (texteEditeQueryRetour, argumentsList.ToArray());
        }
    }
}
