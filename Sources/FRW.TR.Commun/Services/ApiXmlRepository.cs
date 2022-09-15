using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Xml.Linq;
using FRW.TR.Commun.Http;
using Microsoft.AspNetCore.DataProtection.Repositories;
using Microsoft.Extensions.Configuration;
using Polly;
using Polly.Retry;

namespace FRW.TR.Commun.Services
{
    public class ApiXmlRepository : IXmlRepository, IDisposable
    {
        private readonly HttpMessageHandler _httpMessageHandler;
        private readonly HttpClient _httpClient;

        private const string ServiceExterne = "FRW.SV.GestionFormulaires";
        private RetryPolicy policy = Policy.Handle<Exception>().WaitAndRetryForever(
            sleepDurationProvider: attempt => TimeSpan.FromMilliseconds(500));

        public ApiXmlRepository(IConfiguration configuration)
        {
            if (configuration is null) { throw new ArgumentNullException(nameof(configuration)); }

            _httpMessageHandler = FrwHttpMessageHandler.CreerMessageHandler(configuration.GetValue<string>("FRW:NomCertificatClientProxy"));
            _httpClient = new HttpClient(_httpMessageHandler)
            {
                BaseAddress = new Uri(configuration["FRW:UrlDorsale"])
            };
            _httpClient.DefaultRequestHeaders.Add("Accept", "text/xml");
        }

        public IReadOnlyCollection<XElement> GetAllElements()
        {
            return policy.Execute(() => GetOverHttp());
        }

        private IReadOnlyCollection<XElement> GetOverHttp()
        {
            var uriAPI = ServiceExterne + "/api/v1/DataProtection/GetAllElements";

            using (var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, uriAPI))
            {
                var result = _httpClient.SendAsync(httpRequestMessage).Result;
                if (result.IsSuccessStatusCode)
                {
                    var allElements = result.Content.ReadAsStringAsync().Result;
                    if (string.IsNullOrWhiteSpace(allElements))
                    {
                        return Enumerable.Empty<XElement>().ToList().AsReadOnly();
                    }
                    else
                    {
                        return XDocument.Parse(allElements).Root!.Elements().ToList().AsReadOnly();
                    }
                }
                else
                {
                    throw new DorsaleException($"Erreurs lors de l'obtention des clés de chiffrement.", uriAPI, null, null);
                }
            }
        }

        public void StoreElement(XElement element, string friendlyName)
        {
            if (element is null) { throw new ArgumentNullException(nameof(element)); }

            var uriAPI = ServiceExterne + $"/api/v1/DataProtection/StoreElement/{friendlyName}";

            using
                (var httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, uriAPI)
                {
                    Content = new StringContent(element.ToString(), Encoding.UTF8, "text/xml")
                })
            {
                var result = _httpClient.SendAsync(httpRequestMessage).Result;
                if (!result.IsSuccessStatusCode)
                {
                    throw new DorsaleException($"Erreurs lors du stockage d'une clé de chiffrement.", uriAPI, null, null);
                }
            }
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
                _httpMessageHandler?.Dispose();
                _httpClient?.Dispose();
            }
            // free native resources if there are any.
        }
    }
}
