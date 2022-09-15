using System;
using Microsoft.Extensions.DependencyInjection;
using Serilog.Extensions.Hosting;
using Serilog.Core;
using Serilog.Extensions.Logging;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;
using FRW.TR.Commun.Services;
using Microsoft.Extensions.Hosting;
using Serilog.Exceptions;
using Masking.Serilog;
using FRW.TR.Commun.SerilogHttpClients;

namespace FRW.TR.Commun.Extensions
{

    public static class FrwServicesExtensions
    {

        /// <summary>
        /// Ajoute le log de serilog de façon standard, requiert la valeur FRW:UrlDorsale dans le appSettings.json
        /// </summary>
        /// <param name="builder">Le Builder du webhost</param>
        /// <param name="sourceApp">Nom de l'application dans le log</param>
        /// <param name="appSettingsConfigKeyUrlLog">Nom de la clé config contenant l'url du service log.</param>
        /// <param name="appSettingsConfigKeyCertificat">Nom de la clé config contenant le nom du certificat client.</param>
        /// <param name="utiliserCertificatClient">True/False Certificat pour l'extranet OU Windows Authentification</param>
        /// <param name="loggingLevelSwitch">Propritété statique à passer en entrée.</param>
        /// <param name="preserveStaticLogger">Indicates whether to preserve the value of <see cref="Log.Logger"/>.</param>
        /// <param name="writeToProviders">By default, Serilog does not write events to <see cref="ILoggerProvider"/>s registered through
        /// the Microsoft.Extensions.Logging API. Normally, equivalent Serilog sinks are used in place of providers. Specify
        /// <c>true</c> to write events to all providers.</param>
        public static IHostBuilder UseFrwLog(this IHostBuilder builder,
                        LoggingLevelSwitch loggingLevelSwitch,
                        bool utiliserCertificatClient,
                        string? sourceApp = null,
                        string appSettingsConfigKeyUrlLog = "FRW:UrlDorsale",
                        string appSettingsConfigKeyCertificat = "FRW:NomCertificatClientProxy",
                        bool preserveStaticLogger = false,
                        bool writeToProviders = false)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            builder.ConfigureServices((context, collection) =>
            {
                var loggerConfiguration = new LoggerConfiguration();

                LoggerProviderCollection? loggerProviders = null;
                if (writeToProviders)
                {
                    loggerProviders = new LoggerProviderCollection();
                    loggerConfiguration.WriteTo.Providers(loggerProviders);
                }

                var logger = new LoggerConfiguration()
                                    //Permet d'écrire dans un fichier local si le traitement est au niveau de log "verbose"
                                    //niveau par défaut lors du démarrage de l'application.
                                    .WriteTo.Conditional(evt => loggingLevelSwitch.MinimumLevel == LogEventLevel.Verbose,
                                                         wt => wt.EventLog(sourceApp ?? context.HostingEnvironment.ApplicationName))
                                    .Enrich.FromLogContext()
                                    .Enrich.WithExceptionDetails()
                                    .SurchargerMinimumLevel(context, ref loggingLevelSwitch)
                                    .MinimumLevel.ControlledBy(loggingLevelSwitch)
                                    .Enrich.WithProperty("Serveur", Environment.MachineName)
                                    .Enrich.WithClientIp()
                                    .Enrich.WithClientAgent()
                                    .WriteTo.Http($"{context.Configuration.GetSection(appSettingsConfigKeyUrlLog).Value}/log?sourceApp={sourceApp ?? ((bool.Parse(context.Configuration.GetSection("estProduction").Value) ? Environment.MachineName[^1..] + "." : "") + context.HostingEnvironment.ApplicationName)}&serveur={Environment.MachineName}",
                                                httpClient: new SerilogHttpClient(utiliserCertificatClient ? context.Configuration.GetSection(appSettingsConfigKeyCertificat).Value : null)
                                                ) //, bufferPathFormat: $"logs/Log-{Environment.CurrentManagedThreadId}-{{Date}}.json")
                                                  //S'il y a d'autres informations que l'on veut masquer les ajouter ici et ils vont être masqués 
                                        .Destructure.ByMaskingProperties(opts =>
                                        {
                                            opts.PropertyNames.Add("Identifiant");
                                            opts.PropertyNames.Add("NASComplet");
                                            opts.PropertyNames.Add("NASAncien");
                                            opts.PropertyNames.Add("DateNaissance");
                                            opts.PropertyNames.Add("NAS");
                                            opts.PropertyNames.Add("CP12");
                                            opts.PropertyNames.Add("NAM");
                                        })
                                        .CreateLogger();

                Serilog.ILogger? registeredLogger = null;
                if (preserveStaticLogger)
                {
                    registeredLogger = logger;
                }
                else
                {
                    // Passing a `null` logger to `SerilogLoggerFactory` results in disposal via
                    // `Log.CloseAndFlush()`, which additionally replaces the static logger with a no-op.
                    Log.Logger = logger;
                }

                collection.AddSingleton<ILoggerFactory>(services =>
                {
                    var factory = new SerilogLoggerFactory(registeredLogger, true, loggerProviders);

                    if (writeToProviders)
                    {
                        foreach (var provider in services.GetServices<ILoggerProvider>())
                        {
                            factory.AddProvider(provider);
                        }
                    }

                    return factory;
                });

                ConfigureServices(collection, logger, loggingLevelSwitch);
            });
            return builder;
        }

        static void ConfigureServices(IServiceCollection collection, Serilog.ILogger logger, LoggingLevelSwitch loggingLevelSwitch)
        {
            if (collection == null)
            {
                throw new ArgumentNullException(nameof(collection));
            }

            if (logger != null)
            {
                // This won't (and shouldn't) take ownership of the logger. 
                collection.AddSingleton(logger);
            }
            collection.AddSingleton((c) => loggingLevelSwitch);
            collection.AddHostedService<FrwLogLevelService>();

            // Registered to provide two services...
            var diagnosticContext = new DiagnosticContext(logger);

            // Consumed by e.g. middleware
            collection.AddSingleton(diagnosticContext);

            // Consumed by user code
            collection.AddSingleton<IDiagnosticContext>(diagnosticContext);
        }

    }
}
