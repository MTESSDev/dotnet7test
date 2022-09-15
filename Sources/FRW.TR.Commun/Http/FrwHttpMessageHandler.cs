using System.Net;
using System.Net.Http;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

namespace FRW.TR.Commun.Http
{
    public static class FrwHttpMessageHandler
    {
        public static readonly string CleEnteteHttpUrlServiceBackEnd = "X-BACKEND-URL";

        public static HttpMessageHandler CreerMessageHandler(string? nomCertificatClient)
        {
            if (!string.IsNullOrWhiteSpace(nomCertificatClient))
            {
                using (var store = new X509Store(StoreName.My, StoreLocation.LocalMachine, OpenFlags.ReadOnly))
                {
                    var cert = store.Certificates.Find(X509FindType.FindBySubjectName, nomCertificatClient, true);

                    var t = new HttpClientHandler()
                    {
                        UseCookies = false,
                        ClientCertificateOptions = ClientCertificateOption.Manual
                    };

                    t.ServerCertificateCustomValidationCallback = ValidationCertificat;
                    t.ClientCertificates.AddRange(cert);
                    return t;
                }
            }
            else
            {
                var t = new HttpClientHandler()
                {
                    UseCookies = false,
                    Credentials = CredentialCache.DefaultNetworkCredentials,
                    PreAuthenticate = true
                };
                t.ServerCertificateCustomValidationCallback = ValidationCertificat;

                return t;
            }
        }

        private static bool ValidationCertificat(HttpRequestMessage httpRequestMessage, X509Certificate2? certificate, X509Chain? chain, SslPolicyErrors sslPolicyErrors)
        {
            if (sslPolicyErrors == SslPolicyErrors.RemoteCertificateNameMismatch)
            {
                if (httpRequestMessage.Headers.Contains("X-BACKEND-URL"))
                {
                    return true;
                }
            }

            return sslPolicyErrors == SslPolicyErrors.None;
        }
    }
}
