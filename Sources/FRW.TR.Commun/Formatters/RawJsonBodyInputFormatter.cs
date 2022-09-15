using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Formatters;

namespace FRW.TR.Commun
{
    /// <summary/>
    public class RawJsonBodyInputFormatter : InputFormatter
    {
        /// <summary/>
        public RawJsonBodyInputFormatter()
        {
            this.SupportedMediaTypes.Add("application/json");
        }

        /// <summary/>
        public override async Task<InputFormatterResult> ReadRequestBodyAsync(InputFormatterContext context)
        {
            var request = context.HttpContext.Request;
            using (var reader = new StreamReader(request.Body))
            {
                var content = await reader.ReadToEndAsync();
                return await InputFormatterResult.SuccessAsync(content);
            }
        }

        /// <summary/>
        protected override bool CanReadType(Type type)
        {
            return type == typeof(string);
        }
    }
}
