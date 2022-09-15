using System;
using System.ComponentModel.DataAnnotations;

namespace FRW.PR.Extra.Models.Components
{
    public class HelperAccesProfil : IDateHelper
    {
        public DateTime ObtenirDateProduction()
        {
            return DateTime.UtcNow;
        }
    }

    public class DateApresAttribute : ValidationAttribute
    {
        private readonly HelperAccesProfil _profilHelper;

        public DateTime? DateMinimum { get; set; }

        public DateApresAttribute(HelperAccesProfil profilHelper)
        {
            _profilHelper = _profilHelper ?? new HelperAccesProfil();
        }

        public override bool IsValid(object? value)
        {
            var dateMinimum = DateMinimum.HasValue ? DateMinimum.Value : _profilHelper.ObtenirDateProduction();

            if (value is DateTime dateConvertie)
            {
                return dateConvertie >= dateMinimum;
            }

            return false;
        }

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            return base.IsValid(value, validationContext);
        }
    }
}
