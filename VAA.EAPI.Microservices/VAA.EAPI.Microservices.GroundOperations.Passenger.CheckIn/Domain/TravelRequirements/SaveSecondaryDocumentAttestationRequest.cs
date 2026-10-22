using System.ComponentModel.DataAnnotations;
using VAA.EAPI.Microservices.GroundOperations.Passenger.CheckIn.Constants;
using VAA.EAPI.Microservices.GroundOperations.Passenger.CheckIn.DeltaTrm;
using PassengerIdHelper = VAA.EAPI.Microservices.GroundOperations.Passenger.CheckIn.Helper.PassengerId;

namespace VAA.EAPI.Microservices.GroundOperations.Passenger.CheckIn.Domain.TravelRequirements
{
    public sealed class SaveSecondaryDocumentAttestationRequest
    {
        [Required]
        public string RequirementsId { get; set; }

        [Required]
        public string PassengerId { get; set; }

        [Required]
        public string SliceId { get; set; }

        [Required]
        public string SecondaryDocumentId { get; set; }

        [Required]
        public string CountryCode { get; set; }

        public static explicit operator RequirementFormRequest(SaveSecondaryDocumentAttestationRequest source)
        {
            var passengerId = PassengerIdHelper.FromTravelRequirements(source.PassengerId);

            return new RequirementFormRequest
            {
                PassengerResourceId = passengerId.TrmPassengerResourceId,
                TripId = source.SliceId,
                Using = RequirementType.SECONDARY_DOC,
                Consent = true,
                DocumentForm = new DocumentForm
                {
                    DigitalVerificationEligible = false,
                    Link = new Link
                    {
                        LinkId = source.SecondaryDocumentId
                    }
                },
                Country = new Country
                {
                    Code = source.CountryCode
                }
            };
        }
    }
}
