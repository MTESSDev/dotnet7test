using System;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace FRW.TR.Contrats
{
    public interface IDorsale
    {
        Task<TRecevoir> Recevoir<TRecevoir>(string serviceDestination, string addresseApi, [CallerMemberName] string appelant = "");

        Task Envoyer<TEnvoyer>(TEnvoyer envoyer, string serviceDestination, string addresseApi, [CallerMemberName] string appelant = "") where TEnvoyer : class;

        Task Executer(HttpMethod method, string serviceDestination, string addresseApi, [CallerMemberName] string appelant = "");

        Task<TRecevoir> EnvoyerRecevoir<TEnvoyer, TRecevoir>(HttpMethod method, TEnvoyer envoyer, string serviceDestination, string addresseApi, [CallerMemberName] string appelant = "") where TEnvoyer : class;
    }
}
