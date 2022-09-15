using System;
using System.Globalization;

namespace FRW.PR
{
    public static class CultureInfoExtensions
    {
        /// <summary>
        /// Permet de vérifier si la langue d'affichage du site est le français.
        /// </summary>
        /// <returns>Si la langue d'affichage du site est le français ou non.</returns>
        public static bool EstSiteFrancais => CultureInfo.CurrentUICulture.EstFrancais();

        /// <summary>
        /// Permet de retourner le code de la langue de l'utilisateur
        /// </summary>
        /// <returns>"fr" ou "en"</returns>
        public static string LangueUtilisateur => CultureInfo.CurrentUICulture.CodeLangueUtilisateur();

        public static string LangueUrl => CultureInfo.CurrentUICulture.EstFrancais() ? "" : "/en";


        /// <summary>
        /// Permet de vérifier si la langue d'une culture est le français.
        /// </summary>
        /// <param name="cultureInfo">Informations d'une culture.</param>
        /// <example>System.Globalization.CultureInfo.CurrentUICulture.EstFrancais();</example>
        /// <returns>Si la langue de la culture est le français.</returns>
        public static bool EstFrancais(this CultureInfo cultureInfo)
        {
            if (cultureInfo is null) { throw new ArgumentNullException(nameof(cultureInfo)); };

            return cultureInfo.TwoLetterISOLanguageName.Equals("fr", StringComparison.InvariantCultureIgnoreCase);
        }

        /// <summary>
        /// Permet de retourner le code de la langue de l'utilisateur
        /// </summary>
        /// <param name="cultureInfo">Informations d'une culture.</param>
        /// <example>System.Globalization.CultureInfo.CurrentUICulture.CodeLangueUtilisateur();</example>
        /// <returns>"fr" ou "en"</returns>
        public static string CodeLangueUtilisateur(this CultureInfo cultureInfo)
        {
            if (cultureInfo is null) { throw new ArgumentNullException(nameof(cultureInfo)); };

            return cultureInfo.TwoLetterISOLanguageName;
        }
    }
}
