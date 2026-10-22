using System.ComponentModel.DataAnnotations;
using System.Globalization;
using NodaTime;
using VAA.EAPI.Microservices.GroundOperations.Passenger.CheckIn.DeltaTrm;
using PassengerIdHelper = VAA.EAPI.Microservices.GroundOperations.Passenger.CheckIn.Helper.PassengerId;

namespace VAA.EAPI.Microservices.GroundOperations.Passenger.CheckIn.Domain.TravelRequirements
{
    public class SaveReturnDateRequest
    {
        [Required]
        public string RequirementsId { get; set; }

        [Required]
        public string PassengerId { get; set; }

        public LocalDate? ReturnOn { get; set; }

        public bool NoReturnDate { get; set; }

        public static explicit operator RequirementFormRequest(SaveReturnDateRequest source)
        {
            var passengerId = PassengerIdHelper.FromTravelRequirements(source.PassengerId);

            return new RequirementFormRequest
            {
                PassengerResourceId = passengerId.TrmPassengerResourceId,
                Using = RequirementType.RETURNDATE,
                Consent = true,
                ReturnDateDeclined = source.NoReturnDate,
                ReturnDate = (!source.NoReturnDate && source.ReturnOn.HasValue)
                    ? source.ReturnOn.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)
                    : null
            };
        }
    }
}
