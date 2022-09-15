using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Serilog;
using Microsoft.AspNetCore.DataProtection;
using FRW.PR.Extra.Services;
using FRW.TR.Commun.Extensions;
using System.Text;
using Microsoft.Extensions.Localization;
using Docnet.Core;
using Docnet.Core.Models;
using Docnet.Core.Converters;

namespace FRW.PR.Extra.Controllers
{
    [Route("/api/v1/[controller]/[action]")]
    [ApiController]
    public class DocumentController : ControllerBase
    {
        private readonly TransmissionDocumentsService _transmissionService;
        private readonly AuthService _authService;
        private readonly ISystemeAutoriseService _systemeAutoriseService;
        private readonly IDataProtector _dataProtector;
        private readonly IConfiguration _config;
        private readonly IStringLocalizer _localizer;
        private readonly IDocLib _docLib;

        private static ILogger FrwLog => Log.ForContext<DocumentController>();

        public DocumentController(TransmissionDocumentsService transmissionService,
                                  AuthService authService,
                                  ISystemeAutoriseService systemeAutorise,
                                  IDataProtectionProvider dataProtectionProvider,
                                  IConfiguration config,
                                  IStringLocalizer localizer,
                                  IDocLib docLib)
        {
            _transmissionService = transmissionService;
            _authService = authService;
            _dataProtector = dataProtectionProvider!.CreateProtector("FRW");
            _config = config;
            _systemeAutoriseService = systemeAutorise;
            _localizer = localizer;
            _docLib = docLib;
        }

        /// <summary>
        /// FRW121 - Télécharger une pièce par nom de fichier protégé
        /// </summary>
        /// <param name="nomFichierProtege">Nom fichier protégé (provient du contenu du formulaire)</param>
        /// <param name="bypassSecurite">Permet de ne pas valider la sécurité.</param>
        /// <param name="noPublicSystemeAuth">Guid publique de système autorisé (facultative)</param>
        /// <param name="apiKey">Clée API (facultative)</param>
        /// <returns></returns>
        [HttpGet("{nomFichierProtege}")]
        [ActionName("Telecharger")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> OnGetTelecharger(string nomFichierProtege,
                                                          bool bypassSecurite = false,
                                                          [FromHeader(Name = "X-NoPublicSystemeAutorise")] string? noPublicSystemeAuth = null,
                                                          [FromHeader(Name = "X-ApiKey")] string? apiKey = null)
        {
            return await Telecharger(nomFichierProtege, null, bypassSecurite, noPublicSystemeAuth, apiKey);
        }

        /// <summary>
        /// FRW121 - Télécharger une pièce avec session active
        /// </summary>
        /// <param name="sessionUtilisateur">Session utilisateur (shortGuid) (provient de la barre adresse)</param>
        /// <param name="nomFichierProtege">Nom fichier protégé (provient du contenu du formulaire)</param>
        /// <param name="bypassSecurite">Permet de ne pas valider la sécurité.</param>
        /// <returns></returns>
        [HttpGet("{sessionUtilisateur}/{nomFichierProtege}")]
        [ActionName("Telecharger")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> OnGetTelecharger(string sessionUtilisateur,
                                                          string nomFichierProtege,
                                                          bool bypassSecurite)
        {
            return await Telecharger(sessionUtilisateur, nomFichierProtege, bypassSecurite, null, null);
        }

        /// <summary>
        /// FRW121 - Téléverser une pièce
        /// </summary>
        /// <param name="formDataCollection"></param>
        /// <param name="shortInstance"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        [HttpPost("{shortInstance}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Upload([FromForm] IFormCollection formDataCollection, string shortInstance)
        {
            if (formDataCollection is null || !formDataCollection.Files.Any() || formDataCollection.Files[0].Length == 0)
            {
                throw new ArgumentNullException(nameof(formDataCollection));
            }

            var infoSession = await _authService.RecupererValiderInformationSession(shortInstance.GetGuidOrNull(), false);

            if (infoSession is null) return Unauthorized();

            var (mimeType, erreurs) = _transmissionService.ValiderFichiers(formDataCollection);

            if (erreurs.Any())
            {
                var erreur = erreurs.FirstOrDefault();
                return ValidationProblem(erreur?.Message, title: erreur?.Message, instance: erreur?.Cible);
            }

            if (ModelState.IsValid)
            {
                byte[] fileArray = Array.Empty<byte>();
                using (MemoryStream ms = new MemoryStream())
                {
                    await formDataCollection.Files[0].CopyToAsync(ms);
                    fileArray = ms.ToArray();
                }

                // La vérification ici est bidon, on sauvegarde le fichier sur le disque juste pour laisser le temps à
                // l'antivirus local de le détecter et supprimer le fichier, une meilleure analyse est faite plus loin
                if (await VerifierPresenceVirus(fileArray, formDataCollection.Files[0].FileName))
                {
                    return Problem(title: "echecUpload", type: "texteEdite", detail: _localizer["champs.customfile.erreurs.echecUpload"]);
                }

                if (!VerifierIntegriteFichier(fileArray, formDataCollection.Files[0].ContentType))
                {
                    var msg = _localizer["champs.customfile.erreurs.fichierCorrompu"];
                    return ValidationProblem(msg, title: msg, instance: formDataCollection.Files[0].FileName);
                }

                // Le fichier est sauvegardé dans le depot backend
                var fichierTransmis = await _transmissionService.Transmettre(fileArray, infoSession?.idFormulaire.ToString()!);

                var ext = Path.GetExtension(formDataCollection.Files[0].FileName)[1..];
                var nomInterne = fichierTransmis.EncoderStringId();
                var nomClient = GetSafeFilename(formDataCollection.Files[0].FileName);

                return Ok(new
                {
                    url = $"{nomInterne}.{ext}",
                    name = nomClient,
                    _type = "customFile"
                });
            }
            else
            {
                string messages = string.Join("; ", ModelState.Values
                                        .SelectMany(x => x.Errors)
                                        .Select(x => x.ErrorMessage));

                return Problem(title: messages, type: "message");
            }
        }

        private bool VerifierIntegriteFichier(byte[] fileArray, string contentType)
        {
            try
            {
                if (contentType == "application/pdf")
                {
                    using (var docReader = _docLib.GetDocReader(fileArray, new PageDimensions(200, 200)))
                    {
                        using (var pageReader = docReader.GetPageReader(0))
                        {
                            var rawBytes = pageReader.GetImage(new NaiveTransparencyRemover(255, 255, 255));
                        }
                    }
                }
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        private static string GetSafeFilename(string filename)
        {
            var nomFichier = Path.GetFileNameWithoutExtension(filename);
            var longeurAvant = nomFichier.Length;
            nomFichier = nomFichier?.Substring(0, Math.Min(longeurAvant, 100)) ?? string.Empty;
            var differenceLongeur = longeurAvant - nomFichier.Length;
            var extension = Path.GetExtension(filename);

            if (differenceLongeur == 0)
            {
                filename = nomFichier + extension;
            }
            else
            {
                filename = $"{nomFichier}~{differenceLongeur}{extension}";
            }

            return string.Join("_", filename.Split(Path.GetInvalidFileNameChars()));
        }

        /// <summary>
        /// Détection des virus
        /// </summary>
        private static async Task<bool> VerifierPresenceVirus(byte[] fichier, string nomFichier)
        {
            var ext = Path.GetExtension(nomFichier);
            var filename = Path.GetFileNameWithoutExtension(nomFichier);

            var tempFilePath = Path.GetTempFileName();

            try
            {
                // Test d'upload de virus
                if (filename.Equals("FRW-DUMMY-LEVEL1-$F$F$F$F$F$F$F$F$F$F$F$F$F$F$F$F$F$F$F$F$F$F$F"))
                {
                    tempFilePath += ".com";
                    fichier = Encoding.UTF8.GetBytes(@"X5O!P%@AP[4\PZX54(P^)7CC)7}$EICAR-STANDARD-ANTIVIRUS-TEST-FILE!$H+H*");
                }
                else
                {
                    tempFilePath += ext;
                }

                // La vérification ici est bidon, on sauvegarde le fichier sur le disque juste pour laisser le temps à
                // l'antivirus local de le détecter et supprimer le fichier, une meilleure analyse est faite plus loin
                await System.IO.File.WriteAllBytesAsync(tempFilePath, fichier);

                // On force la lecture pour que l'antivirus se choque bin raide!
                fichier = await System.IO.File.ReadAllBytesAsync(tempFilePath);

                // On attends un peu au cas...
                await Task.Delay(500);

                if (!System.IO.File.Exists(tempFilePath))
                    return true;

                System.IO.File.Delete(tempFilePath);

            }
            catch (Exception)
            {
                return true;
            }

            return false;
        }

        #region "Private"
        private async Task<IActionResult> Telecharger(string info1,
                                                      string? info2,
                                                      bool bypassSecurite,
                                                      string? noPublicSystemeAuth,
                                                      string? apiKey)
        {
            // En production impossible de by-pass la sécurité
            if (_config.GetValue<bool>("estProduction"))
                bypassSecurite = false;

            var nomFichierDecode = Path.GetFileNameWithoutExtension(info2 ?? info1).DecoderStringId();

            if (!bypassSecurite)
            {
                if (noPublicSystemeAuth is { } && apiKey is { } && info2 is null)
                {
                    // Valider que le système autorisé a une clée API valide
                    if (await _systemeAutoriseService.ValiderSystemeApiKey(noPublicSystemeAuth.GetGuidOrNull(), apiKey) == 0)
                        return Unauthorized();
                }
                else
                {
                    // Traitement pour les appels d'un client qui est en reprise d'un formulaire déjà sauvegardé - protection par cookie
                    var session = await _authService.RecupererValiderInformationSession(info1.GetGuidOrNull(), false);

                    // On vérifie que la session est existante, et que le no. du formulaire dans le fichier est identique à celui de la session
                    if (info2 is null || session is null || !session.HasValue || !nomFichierDecode.StartsWith($"{session.Value.idFormulaire}."))
                        return Unauthorized();
                }
            }


            if (string.IsNullOrWhiteSpace(nomFichierDecode))
                return NotFound();

            var fichier = await _transmissionService.Telecharger(nomFichierDecode);

            if (fichier is null || fichier.Length == 0)
                return NotFound();

            return File(fichier, System.Net.Mime.MediaTypeNames.Application.Octet, info2 ?? info1);
        }
        #endregion
    }
}
