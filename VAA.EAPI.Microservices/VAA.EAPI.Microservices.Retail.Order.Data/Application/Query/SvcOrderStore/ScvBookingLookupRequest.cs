using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace VAA.EAPI.Microservices.Retail.Order.Data.Application.Query.SvcOrderStore
{
    public sealed class ScvBookingLookupRequest : IValidatableObject
    {
        [Required]
        public string Pnr { get; set; }
        [Required]
        public string FlightDate { get; set; }
        [Required]
        public string FirstName { get; set; }
        [Required]
        public string LastName { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (!DateTime.TryParseExact(FlightDate?.Trim(), "ddMMMyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out _))
                yield return new ValidationResult("Flight date must use ddMMMyy format, for example 31AUG25.", [nameof(FlightDate)]);
        }
    }
}
