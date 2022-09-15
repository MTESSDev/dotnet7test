using Microsoft.AspNetCore.Http;
using System;
using System.Linq;

namespace FRW.TR.Commun.Helpers
{
    public static class IP
    {
        public static string GetIpAddress(IHttpContextAccessor httpContextAccessor)
        {
            var ipAddress = httpContextAccessor.HttpContext?.Request.Headers["X-forwarded-for"].FirstOrDefault();

            if (!string.IsNullOrEmpty(ipAddress))
            {
                return GetIpAddressFromProxy(ipAddress);
            }

            return httpContextAccessor.HttpContext!.Connection.RemoteIpAddress!.ToString();
        }

        private static string GetIpAddressFromProxy(string proxiedIpList)
        {
            var addresses = proxiedIpList.Split(',');

            if (addresses.Length != 0)
            {
                // If IP contains port, it will be after the last : (IPv6 uses : as delimiter and could have more of them)
                return addresses[0].Contains(':')
                    ? addresses[0].Substring(0, addresses[0].LastIndexOf(":", StringComparison.Ordinal))
                    : addresses[0];
            }

            return string.Empty;
        }
    }
}
