using System.Collections.Generic;
using System.Threading.Tasks;

namespace FRW.TR.Commun.Services
{
    public interface ITexteEditeService
    {
        public Task<string?> ObtenirValeurAsync(string langue, string identifiant, TexteEditeQuery? texteEditeQuery);
        public Task<IDictionary<string, object>?> ObtenirValeursAsync(string langue, TexteEditeQuery? texteEditeQuery);
        public Task<string?> ObtenirValeurAvecRemplacementAsync(string langue, string identifiant, TexteEditeQuery? texteEditeQuery, object? valeursRemplacement = null);
    }
}
