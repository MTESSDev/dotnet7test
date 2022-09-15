using System;
using System.ComponentModel.DataAnnotations;

namespace FRW.PR.Extra.Models.Components
{
    public interface IDateHelper
    {
        DateTime ObtenirDateProduction();
    }

    public class DateAvantAttribute : ValidationAttribute
    {
        private readonly IDateHelper _dateHelper;

        public DateTime? DateMaximum { get; set; }

        public DateAvantAttribute(IDateHelper dateHelper)
        {
            _dateHelper = dateHelper ?? new HelperAccesProfil();
        }

        public override bool IsValid(object? value)
        {
            var dateMaximum = DateMaximum.HasValue ? DateMaximum.Value : _dateHelper.ObtenirDateProduction();

            if (value is DateTime dateConvertie)
            {
                return dateConvertie < dateMaximum;
            }

            return false;
        }
    }
}