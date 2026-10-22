using VAA.EAPI.Microservices.GroundOperations.Passenger.CheckIn.DeltaTrm;

namespace VAA.EAPI.Microservices.GroundOperations.Passenger.CheckIn.Domain.TravelRequirements
{
    public class TravelRequirementsRequest
    {
        public string PnrLocator { get; set; }
        public string GivenNames { get; set; }
        public string Surname { get; set; }


        public static explicit operator RequirementsRequest(TravelRequirementsRequest source)
        {
            return new RequirementsRequest()
            {
                Using = RequestOptionCodeType.CONFIRMATION,
                GivenNames = source.GivenNames,
                Surname = source.Surname,
                ConfirmationNum = source.PnrLocator
            };
        }
    }
}
