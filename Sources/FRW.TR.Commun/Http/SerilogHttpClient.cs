using System;
using System.Net.Http;
using System.Threading.Tasks;
using FRW.TR.Commun.Http;
using Microsoft.Extensions.Configuration;
using Serilog.Sinks.Http;

namespace FRW.TR.Commun.SerilogHttpClients
{
    public class SerilogHttpClient : IHttpClient
    {
        private readonly HttpClient _client;
        private readonly HttpMessageHandler _httpMessageHandler;

        public SerilogHttpClient(string? nomCertificatClient)
        {
            _httpMessageHandler = FrwHttpMessageHandler.CreerMessageHandler(nomCertificatClient);
            _client = new HttpClient(_httpMessageHandler);
        }

        public Task<HttpResponseMessage> PostAsync(string requestUri, HttpContent content)
        {
            return _client.PostAsync(new Uri(requestUri), content);
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
                _client.Dispose();
                _httpMessageHandler.Dispose();
            }
            // free native resources if there are any.
        }

        public void Configure(IConfiguration configuration)
        {
        }
    }
}
