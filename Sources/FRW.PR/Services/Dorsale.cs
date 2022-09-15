using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using FRW.TR.Commun;
using FRW.TR.Commun.Helpers;
using FRW.TR.Commun.Http;
using FRW.TR.Contrats;
using MessagePack;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace FRW.PR.Extra.Services
{
    public class Dorsale : IDorsale
    {
        private readonly HttpClient _httpClient = default!;
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;
        //private readonly IContexteDevAccesseur _contexteDev;

        private static Serilog.ILogger Log => Serilog.Log.ForContext<Dorsale>();

        public Dorsale(HttpClient httpClient,
                        IConfiguration configuration,
                        IHttpContextAccessor httpContextAccessor)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
            //_contexteDev = contexteDev;
            //var cert = new X509Certificate2("C:\\Installation\\ECS.Proxy.un.pfx", "C1password");
            _httpClient?.DefaultRequestHeaders.Add("Accept", "application/x-msgpack");
            //_httpClient?.DefaultRequestHeaders.Add("X-ARR-ClientCert", cert.GetRawCertDataString());
        }

        public async Task<TRecevoir> Recevoir<TRecevoir>(string serviceDestination, string addresseApi, [CallerMemberName] string appelant = "")
        {
            return await EnvoyerRecevoir<Rien, TRecevoir>(HttpMethod.Get, new Rien(), serviceDestination, addresseApi, appelant);
        }

        public async Task Envoyer<TEnvoyer>(TEnvoyer envoyer, string serviceDestination, string addresseApi, [CallerMemberName] string appelant = "") where TEnvoyer : class
        {
            await EnvoyerRecevoir<TEnvoyer, Rien>(HttpMethod.Post, envoyer, serviceDestination, addresseApi, appelant);
        }

        public async Task Executer(HttpMethod method, string serviceDestination, string addresseApi, [CallerMemberName] string appelant = "")
        {
            await EnvoyerRecevoir<Rien, Rien>(method, new Rien(), serviceDestination, addresseApi, appelant);
        }

        public async Task<TRecevoir> EnvoyerRecevoir<TEnvoyer, TRecevoir>(HttpMethod method, TEnvoyer envoyer, string serviceDestination, string addresseApi, [CallerMemberName] string appelant = "") where TEnvoyer : class
        {
            _httpContextAccessor.HttpContext.Request.Headers.TryGetValue(FrwHttpMessageHandler.CleEnteteHttpUrlServiceBackEnd, out var urlDorsaleEnteteHttp);

            var urlDorsale = string.IsNullOrWhiteSpace(urlDorsaleEnteteHttp.ToString()) ? _configuration["FRW:UrlDorsale"] : urlDorsaleEnteteHttp.ToString();
            var uriComplete = urlDorsale + "/" + serviceDestination + addresseApi;

            TRecevoir recevoir = default(TRecevoir);

            Log.Debug("Appel service proxy pour {uriComplete}", uriComplete, appelant);

            try
            {
                using var httpRequestMessage = new HttpRequestMessage(method, uriComplete) { Content = ObtenirContenuMessageEnvoyer(envoyer) };

                //On passe les clées de config de dev reçues
                var listeVals = _httpContextAccessor.HttpContext.Request.Headers.Where(e => e.Key.StartsWith("x-val-"));

                if (listeVals is { })
                    foreach (var config in listeVals)
                    {
                        httpRequestMessage.Headers.TryAddWithoutValidation(config.Key, config.Value.ToString());
                    }

                //On passe les clées de config de dev
                if (_httpContextAccessor.HttpContext.Items.TryGetValue("x-vals", out var xvals) && xvals is IDictionary<object, object> xvalDict)
                {
                    foreach (var config in xvalDict)
                    {
                        if (config.Value is { })
                            httpRequestMessage.Headers.TryAddWithoutValidation($"x-val-{config.Key.ToString()?.ToLower()}", config.Value.ToString());
                    }
                }

                if (!string.IsNullOrWhiteSpace(urlDorsaleEnteteHttp.ToString()))
                {
                    httpRequestMessage.Headers.Add(FrwHttpMessageHandler.CleEnteteHttpUrlServiceBackEnd, urlDorsaleEnteteHttp.ToString());
                }

                //Rediriger l'ip de l'appelant original au backend
                httpRequestMessage.Headers.Add("X-forwarded-for", IP.GetIpAddress(_httpContextAccessor));

                var result = await _httpClient.SendAsync(httpRequestMessage);
                var bytes = await result.Content.ReadAsByteArrayAsync();

                if (result.IsSuccessStatusCode)
                {
                    if (bytes.Length > 0)
                    {
                        if (typeof(TRecevoir).Name.Equals("Byte[]"))
                        {
                            if (bytes is TRecevoir byteArray)
                            {
                                recevoir = byteArray;
                            }
                        }
                        else
                        {
                            recevoir = MessagePackSerializer.Deserialize<TRecevoir>(bytes);
                        }
                    }
                }
                else
                {
                    if (result.Content.Headers != null &&
                        result.Content.Headers.ContentType != null && result.Content.Headers.ContentType.MediaType.Equals("application/json"))
                    {
                        var erreur = System.Text.Json.JsonSerializer.Deserialize<Erreur>(bytes);
                        throw new DorsaleException($"Objet erreur reçu seul", uriComplete, erreur, null);
                    }
                    else
                    {
                        throw new DorsaleException($"Erreur { result.StatusCode } - { result.ReasonPhrase}\r\n body : \r\n { result.Content.ReadAsStringAsync().Result}",
                                           uriComplete, null, null);
                    }

                }
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Erreur avec l'appel à la dorsale {uriComplete}", new { uriComplete, appelant });

                throw;
            }

            return recevoir!;
        }

        /// <summary>
        /// Obtient le contenu du message à envoyer.
        /// </summary>
        /// <typeparam name="TEnvoyer">Type des données à envoyer.</typeparam>
        /// <param name="envoyer">Les données à envoyer.</param>
        /// <returns></returns>
        private ByteArrayContent? ObtenirContenuMessageEnvoyer<TEnvoyer>(TEnvoyer envoyer)
        {

            if (envoyer is { } && !(envoyer is Rien))
            {
                byte[] buffer;
                buffer = MessagePackSerializer.Serialize(envoyer);
                var contenuMessage = new ByteArrayContent(buffer);
                contenuMessage.Headers.Add("Content-Type", "application/x-msgpack");
                return contenuMessage;
            }

            return null;
        }
    }
}
