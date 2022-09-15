using System;
using Microsoft.Extensions.Localization;

namespace FRW.PR.Extra.Services
{
    public class YamlStringLocalizerFactory : IStringLocalizerFactory
    {
        private readonly IStringLocalizer _stringLocalizer;

        public YamlStringLocalizerFactory(IStringLocalizer stringLocalizer)
        {
            _stringLocalizer = stringLocalizer;
        }

        public IStringLocalizer Create(string baseName, string location)
        {
            return _stringLocalizer;
        }

        public IStringLocalizer Create(Type resourceSource)
        {
            return _stringLocalizer;
        }
    }
}
