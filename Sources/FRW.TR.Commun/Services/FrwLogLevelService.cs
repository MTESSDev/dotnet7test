using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using FRW.TR.Commun.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using Serilog;
using Serilog.Core;
using Serilog.Events;

namespace FRW.TR.Commun.Services
{
    public class NiveauLog
    {
        public LogEventLevel MinimumLogLevel { get; set; }
    }

    public class FrwLogLevelService : IHostedService, IDisposable
    {
        protected static readonly ILogger Log = Serilog.Log.ForContext<FrwLogLevelService>();
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;
        private readonly HttpMessageHandler _httpMessageHandler;
        private readonly LoggingLevelSwitch _loggingLevelSwitch;
        //private Timer? _timer;

        public FrwLogLevelService(IConfiguration configuration, LoggingLevelSwitch loggingLevelSwitch)
        {
            _configuration = configuration;
            _loggingLevelSwitch = loggingLevelSwitch;
            _httpMessageHandler = FrwHttpMessageHandler.CreerMessageHandler(configuration.GetValue<string>("FRW:NomCertificatClientProxy"));
            _httpClient = new HttpClient(_httpMessageHandler);
        }

        public async Task StartAsync(CancellationToken stoppingToken)
        {
            //DoWork(null);
            //_timer = new Timer(DoWork, null, TimeSpan.Zero, TimeSpan.FromSeconds(60));

            while (true)
            {
                await DoWork();
                await Task.Delay(60000);
            }

            //return Task.CompletedTask;
        }

        private async Task DoWork()
        {
            var avant = _loggingLevelSwitch.MinimumLevel;

            try
            {
                var niveau = await Recevoir(new NiveauLog() { MinimumLogLevel = LogEventLevel.Verbose }, "logs", "/min-log-level");
                _loggingLevelSwitch.MinimumLevel = niveau.MinimumLogLevel;
            }
#pragma warning disable CA1031 // Ne pas intercepter les types d'exception générale
            catch (Exception ex)
#pragma warning restore CA1031 // Ne pas intercepter les types d'exception générale
            {
                // On remet le log au status de base, puisque nous avons détecter une panne du service de log.
                _loggingLevelSwitch.MinimumLevel = LogEventLevel.Verbose;
                Log.Error(ex, "Impossible de contacter le serveur de log pour obtenir le niveau de log.");
            }

            if (!avant.Equals(_loggingLevelSwitch.MinimumLevel))
            {
                Log.Information("Rafraîchir le niveau de log Avant : {avant}, Après: {MinimumLevel}", avant, _loggingLevelSwitch.MinimumLevel);
            }

        }

        public Task StopAsync(CancellationToken stoppingToken)
        {
            //_timer?.Change(Timeout.Infinite, 0);

            return Task.CompletedTask;
        }

        public async Task<T> Recevoir<T>(T defaultValue, string serviceDestination, string addresseApi)
        {
            var urlDorsale = _configuration["FRW:UrlDorsale"];

            if (string.IsNullOrWhiteSpace(urlDorsale))
            {
                throw new ArgumentException("_configuration[FRW:UrlDorsale] configuration introuvable.");
            }

            var uriComplete = urlDorsale + "/" + serviceDestination + addresseApi;

            T? recevoir = defaultValue;

            using (var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, uriComplete))
            {
                var result = await _httpClient.SendAsync(httpRequestMessage);

                if (result.IsSuccessStatusCode)
                {
                    var bytes = await result.Content.ReadAsStringAsync();

                    recevoir = JsonConvert.DeserializeObject<T>(bytes);
                }
                else
                {
                    Serilog.Log.Logger.Error("Impossible de charger le niveau de log.");
                    return defaultValue;
                }
            }

            if (recevoir is null) { return defaultValue; }

            return recevoir;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                // free managed resources
                //_timer?.Dispose();
                _httpMessageHandler?.Dispose();
                _httpClient?.Dispose();
            }
            // free native resources if there are any.
        }
    }
}
