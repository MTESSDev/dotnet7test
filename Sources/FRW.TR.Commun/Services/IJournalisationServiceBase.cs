using FRW.TR.Contrats.Journalisation;
using System;

namespace FRW.TR.Commun.Services
{
    public interface IJournalisationServiceBase
    {
        void JournaliserSIG(CodeOptionTransaction codeOptionTransaction, string codeTransaction, string codePartieVariable, object valeurPartieVariable, int? idFormulaire = null, Guid? idSession = null);
    }
}
