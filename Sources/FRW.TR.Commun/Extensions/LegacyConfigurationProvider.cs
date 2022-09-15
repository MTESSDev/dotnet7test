using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using System;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;

namespace FRW.TR.Commun.Extensions
{
    public static class LegacyConfigurationProvider
    {
        /// <summary>
        /// Permet de remplacer les variables "token" __VARIABLE1__ dans un fichier XML au startup de l'application.
        /// Il lit un fichier (ex: app.config) et va générer un fichier {default}.config les valeurs de remplacement
        /// sont récupérées à partir du fichier appsettings.json
        /// ({default} équivaut au nom du dll ou exe qui est en train de s'exécuter)
        /// </summary>
        /// <param name="builder">configuration builder</param>
        /// <param name="hostEnvironment">host actif</param>
        /// <param name="inputFilename">Nom du fichier à traiter</param>
        /// <param name="outputFilename">Nom du fichier en sortie avec les valeurs remplacées</param>
        /// <example>Inscrire comme variable: __Service:CAC:Source__dans le XML recherchera la valeur Source dans
        /// l'élément CAC de l'élément Service.</example>
        /// <returns></returns>
        public static IConfigurationBuilder AddLegacyXmlConverter(this IConfigurationBuilder builder,
                                                                    IHostEnvironment hostEnvironment,
                                                                    string inputFilename = "app.config",
                                                                    string outputFilename = "{default}.config")
        {
            if (builder is null) { throw new ArgumentNullException(nameof(builder)); }
            if (hostEnvironment is null) { throw new ArgumentNullException(nameof(hostEnvironment)); }
            if (inputFilename is null) { throw new ArgumentNullException(nameof(inputFilename)); }

            var configuration = builder.Build();
            var fileprov = builder.GetFileProvider();
            var contentPath = hostEnvironment.ContentRootPath;

            foreach (var item in Directory.EnumerateFiles(contentPath, inputFilename))
            {
                var fichier = File.ReadAllText(item);

                fichier = Regex.Replace(fichier, @"__(\w+[:\.\w+]*)__", delegate (Match match)
                {
                    var val = match.Groups[1].Value;

                    return configuration.GetValue<string>(val);

                });

                if (outputFilename is { } && outputFilename.Contains("{default}"))
                {
                    var dll = Assembly.GetEntryAssembly()?.GetName()?.CodeBase?.Replace("file:///", "");
                    outputFilename = outputFilename.Replace("{default}", dll);
                }

                if (outputFilename is { })
                    File.WriteAllText(outputFilename, fichier);
            }

            return builder ?? default!;
        }
    }
}
