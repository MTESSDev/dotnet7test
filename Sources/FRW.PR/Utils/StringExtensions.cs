using CSharpVitamins;
using System;

namespace FRW.PR.Extra
{
    public static class StringExtensions
    {
        public static Guid? GetGuidOrNull(this string? value)
        {
            if (ShortGuid.TryParse(value, out Guid decodedValue))
                return decodedValue;
            else
                return null;
        }

        public static Guid GetGuid(this string? value)
        {
            return GetGuidOrNull(value) ?? throw new ArgumentException("ShortGuid invalide.");
        }

        public static string? GetShortGuid(this Guid value)
        {
            return ShortGuid.Encode(value);
        }
    }

}
