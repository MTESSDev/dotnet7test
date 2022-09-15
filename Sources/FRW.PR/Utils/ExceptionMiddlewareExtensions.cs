using System.Net;
using System.Text.Encodings.Web;
using System.Text.Json;
using FRW.TR.Contrats;
using FRW.TR.Contrats.Contexte;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Serilog;

namespace FRW.PR.Extra.Extensions
{
    /// <summary>
    /// Gstion du retour JSON des erreurs
    /// </summary>
    public static class ExceptionMiddlewareExtensions
    {
        /// <summary>
        /// Gestion des exceptions dans les API
        /// </summary>
        /// <param name="app"></param>
        public static void UseFrwApiExceptionHandler(this IApplicationBuilder app)
        {
            app.UseExceptionHandler(appError =>
            {
                appError.Run(async context =>
                {
                    context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    context.Response.ContentType = "application/json";

                    var contextFeature = context.Features.Get<IExceptionHandlerFeature>();
                    if (contextFeature != null)
                    {
                        using (Serilog.Context.LogContext.PushProperty("Branche", context.Request.Host, true))
                        {
                            Log.Logger.Error(contextFeature.Error, "Erreur lors de l'appel à {Path}", context.Request.Path);
                        }

                        var msg = new Erreur()
                        {
                            No = context.Response.StatusCode.ToString(),
                            Message = contextFeature.Error.Message,
                            Cible = context.Request.Path
                        };

                        //Test pour alléger les unicodes
                        await context.Response.WriteAsync(JsonSerializer.Serialize(msg, new JsonSerializerOptions
                        {
                            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                        }));

                    }
                });
            });
        }


    }
}
