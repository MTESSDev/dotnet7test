using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Core;
using Serilog.Events;

namespace FRW.TR.Commun.Extensions
{
    public static class LoggerConfigurationExtensions
    {
        private const string CleSectionNiveauLogMinimum = "Logging:LogLevel";

        public static LoggerConfiguration SurchargerMinimumLevel(this LoggerConfiguration configurationLogger, HostBuilderContext hostBuilderContext, ref LoggingLevelSwitch loggingLevelSwitch)
        {
            if (configurationLogger is null)
            {
                throw new ArgumentNullException(nameof(configurationLogger));
            }

            if (hostBuilderContext is null)
            {
                throw new ArgumentNullException(nameof(hostBuilderContext));
            }

            foreach (var overrideDirective in hostBuilderContext.Configuration.GetSection(CleSectionNiveauLogMinimum).GetChildren())
            {

                var espaceNom = overrideDirective.Key;
                var niveauMinimum = overrideDirective.Value;

                if (string.IsNullOrWhiteSpace(niveauMinimum))
                {
                    configurationLogger.MinimumLevel.Override(espaceNom, loggingLevelSwitch);
                    continue;
                }

                if (Enum.TryParse(niveauMinimum, out LogEventLevel logEventLevel))
                {
                    configurationLogger.MinimumLevel.Override(espaceNom, logEventLevel);
                }
            }

            return configurationLogger;
        }
    }
}
