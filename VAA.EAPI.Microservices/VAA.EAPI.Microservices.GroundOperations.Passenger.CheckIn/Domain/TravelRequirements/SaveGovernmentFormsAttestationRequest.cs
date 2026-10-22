using System.ComponentModel.DataAnnotations;
using VAA.EAPI.Microservices.GroundOperations.Passenger.CheckIn.DeltaTrm;
using PassengerIdHelper = VAA.EAPI.Microservices.GroundOperations.Passenger.CheckIn.Helper.PassengerId;

namespace VAA.EAPI.Microservices.GroundOperations.Passenger.CheckIn.Domain.TravelRequirements
{
    public sealed class SaveGovernmentFormsAttestationRequest
    {
        [Required]
        public string RequirementsId { get; set; }

        [Required]
        public string PassengerId { get; set; }

        [Required]
        public string SliceId { get; set; }

        [Required]
        public string Id { get; set; }

        [Required]
        public string LinkId { get; set; }

        [Required]
        public string Text { get; set; }

        [Required]
        public string Href { get; set; }

        public static explicit operator RequirementFormRequest(SaveGovernmentFormsAttestationRequest source)
        {
            var passengerId = PassengerIdHelper.FromTravelRequirements(source.PassengerId);

            return new RequirementFormRequest
            {
                PassengerResourceId = passengerId.TrmPassengerResourceId,
                TripId = source.SliceId,
                Using = RequirementType.SELF_DECLARATION_FORM,
                Consent = true,
                Modified = false,
                SaveToProfile = false,
                DocumentForm = new DocumentForm
                {
                    DigitalVerificationEligible = false,
                    Link = new Link
                    {
                        Id = source.Id,
                        LinkId = source.LinkId,
                        Text = source.Text,
                        Href = source.Href
                    }
                }
            };
        }
    }
}
