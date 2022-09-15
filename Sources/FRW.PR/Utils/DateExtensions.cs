using CSharpVitamins;
using System;

namespace FRW.PR.Extra
{
    public static class DateExtensions
    {
        public static long ToExpirationClient(this DateTime expiration)
        {
            DateTimeOffset utc = expiration.ToUniversalTime();
            return utc.ToUnixTimeSeconds();
        }
    }

}
