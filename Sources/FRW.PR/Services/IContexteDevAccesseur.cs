using System.Collections.Generic;

namespace FRW.PR.Services
{
    public interface IContexteDevAccesseur
    {
        IDictionary<object, object>? ConfigDev { get; set; }
        string? ConfigDevBrute { get; set; }
        bool EstActif { get; set; }
    }
}