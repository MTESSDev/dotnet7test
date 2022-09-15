using System;
using Microsoft.AspNetCore.Mvc.ApplicationModels;

namespace FRW.PR.Utils.Culture
{
    public class CultureTemplateRouteModelConvention : IPageRouteModelConvention
    {
        public void Apply(PageRouteModel model)
        {
            if (model is null) { throw new ArgumentNullException(nameof(model)); }

            var selectorCount = model.Selectors.Count;
            for (var i = 0; i < selectorCount; i++)
            {
                var selector = model.Selectors[i];
                model.Selectors.Add(new SelectorModel
                {
                    AttributeRouteModel = new AttributeRouteModel
                    {
                        Order = -1,
                        Template = AttributeRouteModel.CombineTemplates(
                      "{culture:regex(^(en|fr)$)}",
                      selector.AttributeRouteModel.Template),
                    }
                });
            }
        }
    }
}
