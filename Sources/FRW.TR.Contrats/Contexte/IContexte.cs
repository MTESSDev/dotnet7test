//using CAC.AccesProfil.Client;

using System.Collections.Generic;

namespace FRW.TR.Contrats.Contexte
{
    public interface IContexte
    {
        IContexteCAC CAC { get; }
        IDictionary<string, string> ConfigDev { get; }

        /*IContexteECS? ECS { get; }
        IContexteECSSession Session { get; }
        IContexteInterne? Interne { get; }
        IAccesProfil Profil { get; }*/
    }
}
