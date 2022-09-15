using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace FRW.TR.Contrats.Composants
{
    public class ComponentBinding : BaseComponent
    {
        public ComponentBinding() : base(false)
        {
        }

        public List<string>? NameValues { get; set; }
        public string? SectionName { get; set; }
        public int? MaxOccur { get; set; }
        public object? SectionGroupNameIntl { get; set; }
        public object? SectionNameIntl { get; set; }
        public object? GroupNameIntl { get; set; }
        public object? GroupInstanceNameIntl { get; set; }

        public override IEnumerable<ValidationAttribute>? ConvertRules(List<Rule> rules)
        {
            throw new System.NotImplementedException();
        }

        public override List<Rule>? ParseValidations(IDictionary<object, object>? validationsDict)
        {
            throw new System.NotImplementedException();
        }
    }
}