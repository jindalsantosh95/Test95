using VAA.EAPI.Microservices.GroundOperations.Passenger.CheckIn.DeltaTrm;
using VAA.EAPI.Microservices.GroundOperations.Passenger.CheckIn.Helper;

namespace VAA.EAPI.Microservices.GroundOperations.Passenger.CheckIn.Domain.ReturnDate
{
    public class ReturnDateRequest
    {
        public string TravelPartyId { get; set; }
        public string RequirementsId { get; set; }
        public string PassengerId { get; set; }
        public DateTime? ReturnDate { get; set; }
        public bool NoReturnDate { get; set; }
        public IEnumerable<string> SelectedPassengersIds { get; set; }

        public static explicit operator RequirementFormRequest(ReturnDateRequest source)
        {
            var passengerId = new PassengerId(source.PassengerId);
            return new RequirementFormRequest()
            {
                PassengerResourceId = passengerId.TrmPassengerResourceId,
                Using = RequirementType.RETURNDATE,
                Consent = true,
                ReturnDateDeclined = source.NoReturnDate,
                ReturnDate = source.ReturnDate.HasValue ? source.ReturnDate.Value.ToString("yyyy-MM-dd") : null
            };
        }
    }
}
