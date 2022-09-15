using System.Collections.Generic;

namespace FRW.TR.Contrats
{
    public class EntrantEnvoyerCourriel
    {
        public string Id { get; set; } = default!;

        public List<string> A { get; set; } = default!;

        public string? RetourA { get; set; }

        public string? NomExpediteur { get; set; } = default!;

        public string De { get; set; } = default!;

        public string Objet { get; set; } = default!;

        public string Corps { get; set; } = default!;

        public List<PieceJointeCourriel>? PieceJointeCourriel { get; set; }

        public string EvenementAffaire { get; set; } = default!;
    }

    public class PieceJointeCourriel
    {
        public string NomFichier { get; set; } = default!;

        public string ChemainAccesFichier { get; set; } = default!;
    }
}
