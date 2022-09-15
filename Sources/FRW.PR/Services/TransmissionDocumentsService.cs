using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using FRW.TR.Commun.Securite.VerificationFichier;
using FRW.TR.Contrats;
using FRW.TR.Contrats.TransmissionDocuments;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Localization;

namespace FRW.PR.Extra.Services
{
    public class TransmissionDocumentsService
    {
        private readonly IDorsale _dorsale;
        private readonly IStringLocalizer _localizer;
        private readonly int MbTailleMaximaleFichier = 20;
        private const string Service = "FRW.SV.GestionFormulaires";

        public TransmissionDocumentsService(IDorsale dorsale, IStringLocalizer localizer)
        {
            _dorsale = dorsale;
            _localizer = localizer;
        }

        public async Task<string> Transmettre(byte[] fichier, string prefix)
        {
            return await _dorsale.EnvoyerRecevoir<EntrantTransmettre,
                string>(HttpMethod.Post,
                new EntrantTransmettre()
                {
                    Fichier = fichier,
                    Prefix = prefix
                }, Service, $"/api/v1/TransmissionDocuments/Transmettre");
        }

        public async Task<byte[]> Telecharger(string nomFichier)
        {
            return await _dorsale.Recevoir<byte[]>(Service, $"/api/v1/TransmissionDocuments/Telecharger/{nomFichier}");
        }

        public (string mimeType, List<Erreur> erreurs) ValiderFichiers(IFormCollection formDataCollection)
        {
            if (formDataCollection is null) { throw new ArgumentNullException(nameof(formDataCollection)); }

            var erreurs = new List<Erreur>();
            (string mimeType, List<Erreur> erreurs) retour = ("", erreurs);

            var erreurFormatFichier = _localizer["champs.customfile.erreurs.formatInvalide"];
            var erreurTailleFichier = _localizer["champs.customfile.erreurs.excedeTailleMaximale"];

            foreach (var fichier in formDataCollection.Files)
            {
                IEnumerable<string>? extensionsTrouvees = null;

                // Validation du format du fichier
                var ext = Path.GetExtension(fichier.FileName).TrimStart('.');
                bool fichierValide = false;

                using (MemoryStream ms = new MemoryStream())
                {
                    fichier.CopyTo(ms);
                    var fileArray = ms.ToArray();

                    FileTypeVerifyResult result;

                    var filtres = FileType.FormatsFichiers.ToList();

                    foreach (var fileType in filtres)
                    {
                        result = fileType.Verify(ms);

                        if (result.IsVerified)
                        {
                            extensionsTrouvees = result.Extensions;
                            if (extensionsTrouvees.Contains(ext, StringComparer.InvariantCultureIgnoreCase))
                            {
                                retour.mimeType = result.MimeType;
                                fichierValide = true;
                                break;
                            }
                        }
                    }
                }

                if (!fichierValide || (string.IsNullOrEmpty(ext)))
                {
                    erreurs.Add(new Erreur() { Cible = fichier.Name, Message = string.Format(erreurFormatFichier, extensionsTrouvees != null ? string.Join(", ", extensionsTrouvees) : string.Empty) });
                    return retour;
                }

                // Validation de la taille du fichier
                if (fichier.Length > (MbTailleMaximaleFichier * 1024 * 1024))
                {
                    erreurs.Add(new Erreur() { Cible = fichier.Name, Message = string.Format(erreurTailleFichier, MbTailleMaximaleFichier) });
                    return retour;
                }
            }

            return retour;
        }
    }
}
