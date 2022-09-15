using System;
using System.Collections.Generic;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace FRW.TR.Commun.Helpers
{
    /// <summary>
    /// Ajout de l'entête optionnel CodeNT dans les API exposé par Swagger.
    /// </summary>
    public class CodeNTFilter : IOperationFilter
    {
        /// <summary/>
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            if (operation is null)
            {
                throw new ArgumentNullException(nameof(operation));
            }

            operation.Parameters ??= new List<OpenApiParameter>();

            operation.Parameters.Add(new OpenApiParameter
            {
                Name = "CodeNT",
                In = ParameterLocation.Header,
                Required = false
            });
        }
    }
}
