using CAC.Courriels.Client;
using FRW.TR.Contrats;
using FRW.TR.Contrats.Contexte;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FRW.TR.Commun.Utils
{
    public interface IUtilitaireCourriel
    {
        public Task EnvoyerCourriel(EntrantEnvoyerCourriel entrant, IContexteCAC contexteCAC);
    }
    public class UtilitaireCourriel : IUtilitaireCourriel
    {
        //private  Contexte _contexte;
        public async Task EnvoyerCourriel(EntrantEnvoyerCourriel entrant, IContexteCAC contexteCAC)
        {
            var demande = new DemandeCourriel()
            {
                De = entrant.De,
                NomExpediteur = entrant.NomExpediteur ?? "Mon Dossier",
                A = entrant.A,
                Objet = entrant.Objet,
                Corps = entrant.Corps,
                RetourA = entrant.RetourA,
                OptionsEnvoi = DemandeCourriel.TypeOptionsEnvoi.FormatHTML,
                PiecesJointes = GenererPieceJointe(entrant.PieceJointeCourriel)
            };
                        
            using (var courrielService = new CourrielService(contexteCAC.ToContexte()))
            {
                await courrielService.EnvoyerDemandeCourrielAsync(demande, entrant.Id, entrant.EvenementAffaire, TypeConvoi.SansConvoi);
            }
        }

        private List<PieceJointe>? GenererPieceJointe(List<PieceJointeCourriel>? nomPiecesJointes)
        {
            List<PieceJointe>? piscesJointes = null;

            if (nomPiecesJointes != null && nomPiecesJointes.Any())
            {
                piscesJointes = new List<PieceJointe>();

                nomPiecesJointes.ForEach(x => piscesJointes.Add(new PieceJointe
                {
                    CheminAcces = x.ChemainAccesFichier,
                    NomFichier = x.NomFichier,
                }));
            }

            return piscesJointes;
        }
    }
}
