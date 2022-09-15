using System;
using System.Collections.Generic;

namespace FRW.PR.Extra.Utils
{
    public static class GestionContenu
    {
        public static string? ObtenirCourriel(IDictionary<object, object>? systeme)
        {
            string? courriel = null;
            if (systeme != null && systeme.TryGetValue("infosEnregistrement", out var infosEnregistrement))
            {
                courriel = ((IDictionary<object, object>)infosEnregistrement).TryGetValue("courriel", out var objCourriel) ? objCourriel as string : null;
            }

            return courriel;
        }
    }
}