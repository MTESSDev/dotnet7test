using System.Collections.Generic;
using System.Linq;

namespace FRW.PR.Extra.Utils
{
    /// <summary>
    /// 
    /// </summary>
    public class VueParser : IVueParser
    {
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TModel"></typeparam>
        /// <param name="model"></param>
        /// <returns></returns>
        public Dictionary<string, object?> ParseData<TModel>(TModel model)
        {
            var props = model?.GetType().GetProperties();
            var result = new Dictionary<string, object?>();

            if (props is null) { return result; }

            foreach (var prop in props)
            {
                var attr = prop.GetCustomAttributes(typeof(VueData), true)?.FirstOrDefault()
                    as VueData;

                if (attr == null)
                {
                    continue;
                }

                result.Add(attr.Name, prop?.GetValue(model) ?? "");
            }

            return result;
        }
    }
}