using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.DependencyInjection;

namespace FRW.PR.Extra.Extensions
{
    public static class UriExtensions
    {
        public static string AddFileVersionToPath(this HttpContext context, string path)
        {
            if(context is null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            return context
                .RequestServices
                .GetRequiredService<IFileVersionProvider>()
                .AddFileVersionToPath(context.Request.PathBase, path);
        }

        public static string AddFileVersionOfPathToPath(this HttpContext context, string ofPath, string toPath)
        {
            if(context is null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            var query = context
                .RequestServices
                .GetRequiredService<IFileVersionProvider>()
                .AddFileVersionToPath(context.Request.PathBase, ofPath);

            var baseUri = new Uri(context.Request.Scheme + "://" + context.Request.Host.Value);

            var queryString = QueryHelpers.ParseQuery(new Uri(baseUri, query).Query);
            var dictionary = queryString
                .SelectMany(x => x.Value, (col, value) => new KeyValuePair<string, string>(col.Key, value))
                .ToDictionary(x => x.Key, x => x.Value);

            return QueryHelpers.AddQueryString(toPath, dictionary);
        }
    }
}
