using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace FRW.PR.Extra
{
    public static class ClaimsExtensions
    {
        public static IEnumerable<Claim> EffacerExpirees(this IEnumerable<Claim> claims)
        {
            foreach (var item in claims)
            {
                if (item.ValueType.Equals("AvecExpiration"))
                {
                    var claim = ObtenirInfos(item);
                    if (claim is { }) yield return item;
                }
                else
                {
                    yield return item;
                }
            }
        }

        public static void AjouterAvecExpiration(this IList<Claim> claims, string nom, string valeur, TimeSpan timeSpan)
        {
            claims.Add(new Claim(nom,
                Newtonsoft.Json.JsonConvert.SerializeObject(new ClaimExpiration()
                {
                    Val = valeur,
                    Exp = DateTime.Now.Add(timeSpan)
                }), "AvecExpiration"));
        }

        public static (Claim? claim, string valeur)? Obtenir(this IList<Claim> claims, string nom)
        {
            var claim = claims?.FirstOrDefault(x => x.Type.Equals(nom, StringComparison.OrdinalIgnoreCase));

            return ObtenirInfos(claim);
        }

        public static (Claim? claim, string valeur)? ObtenirInfos(Claim? claim)
        {
            var infos = Newtonsoft.Json.JsonConvert.DeserializeObject<ClaimExpiration>(claim?.Value ?? string.Empty);

            if (infos is null) return null;

            if (infos?.Exp > DateTime.Now)
                return (claim, infos.Val);
            else
                return null;
        }
    }
}
