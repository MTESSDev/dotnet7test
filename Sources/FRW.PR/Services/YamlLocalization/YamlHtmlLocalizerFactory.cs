using System;
using Microsoft.AspNetCore.Mvc.Localization;

namespace FRW.PR.Extra.Services
{
    public class YamlHtmlLocalizerFactory : IHtmlLocalizerFactory
    {

        private readonly IHtmlLocalizer _htmlLocalizer;

        public YamlHtmlLocalizerFactory(IHtmlLocalizer htmlLocalizer)
        {
            _htmlLocalizer = htmlLocalizer;
        }

        public IHtmlLocalizer Create(string baseName, string location)
        {
            return _htmlLocalizer;
        }

        public IHtmlLocalizer Create(Type resourceSource)
        {
            return _htmlLocalizer;
        }
    }
}
