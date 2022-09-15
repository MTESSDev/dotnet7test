using Microsoft.AspNetCore.Html;
using System;
using System.Threading.Tasks;

namespace FRW.TR.Commun.Services
{
    public interface IConfigurationApplication
    {
        string? ConfigurationFormulaire { get; set; }
        string? ConfigurationSysteme { get; set; }
        string? ConfigurationGlobale { get; set; }
        string? TypeFormulaire { get; set; }
        int? SystemeAutorise { get; set; }
        string? Version { get; set; }
        bool EstInitialise { get; set; }
        ObjetInitialise ConfigurationFormulaireObjet { get; }
        ObjetInitialise ConfigurationSystemeObjet { get; }
        ObjetInitialise ConfigurationGlobaleObjet { get; }

        /// <summary>
        /// Permet de forcer l'initialisation d'une config spécifique en précisant les valeurs système et autres.
        /// </summary>
        /// <param name="idSystemeAutorise">Id du système autorisé Ex: 1</param>
        /// <param name="typeFormulaire">Type de formulaire Ex: 3003</param>
        /// <param name="version">Version Ex: "0"</param>
        /// <returns></returns>
        Task Initialiser(int? idSystemeAutorise, string? typeFormulaire, string? version);

        /// <summary>
        /// Permet de forcer l'initialisation d'une config
        /// </summary>
        /// <param name="idSystemeAutorise">Id du système autorisé Ex: 1</param>
        /// <param name="typeFormulaire">Type de formulaire Ex: 3003</param>
        /// <param name="version">Version Ex: "0"</param>
        Task Initialiser();

        Task ObtenirConfigurationApplication();

        /// <summary>
        /// Obtenir une valeur en string depuis la config en cascade
        /// </summary>
        /// <param name="clee"></param>
        /// <returns></returns>
        Task<string?> ObtenirValeur(string clee);

        /// <summary>
        /// Obtient une valeur HTML d'une configuration
        /// </summary>
        /// <param name="clee"></param>
        /// <returns></returns>
        Task<HtmlString> ObtenirValeurHtml(string clee);

        /// <summary>
        /// Obtenir une valeur dans le type <typeparamref name="T"/> voulu.
        /// </summary>
        /// <typeparam name="T">Type de retour désiré</typeparam>
        /// <param name="clee"></param>
        /// <param name="niveau">niveau 0 = tous les niveau, niveau 1 = formulaire, niveau 2 = systeme, niveau 3 = global</param>
        /// <returns></returns>
        Task<T?> ObtenirValeur<T>(string clee, Niveau niveau = Niveau.Tous);

        /// <summary>
        /// Permet d'obtenir les scripts Js en cascade d'un formulaire
        /// </summary>
        /// <returns></returns>
        Task<string> ObtenirScriptsJs(string? type = null);

    }

    [Flags]
    public enum Niveau
    {
        Aucun = 0,
        Formulaire = 1,
        Systeme = 2,
        Global = 4,
        Tous = ~0
    }
}