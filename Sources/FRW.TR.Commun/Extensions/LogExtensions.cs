using Serilog;
using Serilog.Context;

namespace FRW.TR.Commun.Extensions
{
    public static class LogExtensions
    {
        public static void Debut(this ILogger log, string nomMethode, object? data)
        {
            if (log != null && log.IsEnabled(Serilog.Events.LogEventLevel.Debug))
            {
                using (LogContext.PushProperty("Donnees", data, true))
                {
                    log?.Debug($"[DEBUT]-{nomMethode}");
                }
            }
        }

        public static T Fin<T>(this ILogger log, string nomMethode, T data)
        {
            if (log != null && log.IsEnabled(Serilog.Events.LogEventLevel.Debug))
            {
                using (LogContext.PushProperty("Donnees", data, true))
                {
                    log?.Debug($"[FIN]-{nomMethode}");
                }
            }

            return data;
        }

        public static void Fin(this ILogger log, string nomMethode)
        {
            if (log != null && log.IsEnabled(Serilog.Events.LogEventLevel.Debug))
            {
                    log.Debug($"[FIN]-{nomMethode}");
            }
        }

        public static void DebutAppelExterne(this ILogger log, string nomMethode, object? data)
        {
            if (log != null && log.IsEnabled(Serilog.Events.LogEventLevel.Debug))
            {
                using (LogContext.PushProperty("Parametres entrees", data, true))
                {
                    log?.Debug($"[DEBUT SERVICE EXTERNE]-{nomMethode}");
                }
            }
        }

        public static T FinAppelExterne<T>(this ILogger log, string nomMethode, T data)
        {
            if (log != null && log.IsEnabled(Serilog.Events.LogEventLevel.Debug))
            {
                using (LogContext.PushProperty("Parametres entrees", data, true))
                {
                    log?.Debug($"[FIN SERVICE EXTERNE]-{nomMethode}");
                }
            }
            return data;
        }

        public static void DebutAppelClicSequr(this ILogger log, string nomMethode, object? data)
        {
            if (log != null && log.IsEnabled(Serilog.Events.LogEventLevel.Debug))
            {
                using (LogContext.PushProperty("Parametres entrees", data, true))
                {
                    log?.Debug($"[DEBUT APPEL SERVICE CLICSEQUR]-{nomMethode}");
                }
            }
        }

        public static T FinAppelClicSequr<T>(this ILogger log, string nomMethode, T data)
        {
            if (log != null && log.IsEnabled(Serilog.Events.LogEventLevel.Debug))
            {
                using (LogContext.PushProperty("Parametres entrees", data, true))
                {
                    log?.Debug($"[FIN SERVICE CLICSEQUR]-{nomMethode}");
                }
            }
            return data;
        }

        /// <summary>
        /// Méthode pour pouvoir journaliser un service en n'affichant pas certaine information
        /// </summary>
        /// <param name="log"></param>
        /// <param name="nomMethode">Le nom du service journaliser</param>
        /// <param name="data"></param>
        public static void JournalisationService(this ILogger log, string nomMethode, object? data)
        {
            if (log != null && log.IsEnabled(Serilog.Events.LogEventLevel.Debug))
            {
                using (LogContext.PushProperty("Parametres entrees", data, true))
                {
                    log?.Debug($"[JOURNALISATION SERVICE]-{nomMethode}");
                }
            }
        }


        public static void Info(this ILogger log, string nomMethode, object? data, string messageTemplate, params object[] propertyValues)
        {
            if (log != null && log.IsEnabled(Serilog.Events.LogEventLevel.Information))
            {
                using (LogContext.PushProperty("Donnees", data, true))
                {
                    log?.Information($"{nomMethode}-{messageTemplate}", propertyValues);
                }
            }
        }

        public static void Erreur(this ILogger log, string nomMethode, object? data, string messageTemplate, params object[] propertyValues)
        {
            if (log != null && log.IsEnabled(Serilog.Events.LogEventLevel.Error))
            {
                using (LogContext.PushProperty("Donnees", data, true))
                {
                    log?.Error($"{nomMethode}-{messageTemplate}", propertyValues);
                }
            }
        }
    }
}
