using FRW.TR.Commun.Extensions;
using Microsoft.AspNetCore.DataProtection;
using System;
using System.Text;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.WebUtilities;

namespace FRW.TR.Commun.Helpers
{
    public static class SimpleEncryptionHelpers
    {
        public static string EncryptAvecExpiration(DateTime dateExpiration, IDataProtector protector, string textToEncrypt, string? key)
        {
            //14 pos. réservées
            return Crypt(dateExpiration.ToString("yyyyMMddHHmmss") + textToEncrypt, protector, key);
        }

        public static string DecryptAvecExpiration(string textToEncrypt, IDataProtector protector, string? key)
        {
            //14 pos. réservées
            var dataAvecDate = Decrypt(textToEncrypt, protector, key);

            if (!string.IsNullOrEmpty(dataAvecDate))
            {
                var date = DateTime.ParseExact(dataAvecDate.Substring(0, 14), "yyyyMMddHHmmss", null);

                if (DateTime.Now >= date)
                {
                    return default!;
                }
                return dataAvecDate.Remove(0, 14);
            }
            return default!;
        }

        public static string Crypt(this string text, IDataProtector protector, string? key)
        {
            if (protector is null) { throw new ArgumentNullException(nameof(protector)); }

            if (string.IsNullOrWhiteSpace(key))
            {
                return Base64UrlTextEncoder.Encode(protector.Protect(
                    Encoding.UTF8.GetBytes(ProtectionDonneesExtensions.EncoderStringId(text, ""))));
            }
            else
            {
                return Base64UrlTextEncoder.Encode(protector.Protect(
                                    Encoding.UTF8.GetBytes(ProtectionDonneesExtensions.EncoderStringId(text, key.Trim()))));
            }
        }

        public static string Decrypt(this string text, IDataProtector protector, string? key)
        {
            if (protector is null) { throw new ArgumentNullException(nameof(protector)); }

            if (string.IsNullOrWhiteSpace(key))
            {
                return ProtectionDonneesExtensions.DecoderStringId(
                    Encoding.UTF8.GetString(protector.Unprotect(Base64UrlTextEncoder.Decode(text))), "");
            }
            else
            {
                return ProtectionDonneesExtensions.DecoderStringId(
                  Encoding.UTF8.GetString(protector.Unprotect(Base64UrlTextEncoder.Decode(text))), key.Trim());
            }
        }
    }
}
