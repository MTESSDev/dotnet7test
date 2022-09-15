using System;
using System.Collections.Generic;
using System.Text;

namespace FRW.TR.Contrats
{
    public class DonneesFormulaire
    {
        public int NsFormulaire { get; set; }

        public Guid GuidFormulaire { get; set; }

        public long? NoConfirmation { get; set; }

        public string? ContenuFormulaire { get; set; }

        public short IdSystemeAutorise { get; set; }

        public string? IdUtilisateur { get; set; }

        public string TypeFormulaire { get; set; } = default!;
    }
}
