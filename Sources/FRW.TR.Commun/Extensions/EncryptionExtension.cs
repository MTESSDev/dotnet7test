using FRW.TR.Commun.Helpers;
using Microsoft.AspNetCore.DataProtection;
using System;

namespace FRW.TR.Commun.Extensions
{
    public static class EncryptionExtension
    {
        public static string RecevoirString(this string str, IDataProtector protector, string? motDePasse = null)
        {
            return SimpleEncryptionHelpers.Decrypt(str, protector, motDePasse);
        }

        public static string EnvoyerString(this string str, IDataProtector protector, string? motDePasse = null)
        {
            return SimpleEncryptionHelpers.Crypt(str, protector, motDePasse);
        }
    }
}
