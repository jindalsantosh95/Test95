namespace VAA.EAPI.GraphQL.Domain.PaxOps.Infrastructure.Models.SecureFlightInfo
{
    public sealed class UpdatePassengerDetailsRequest
    {
        public string BookingId { get; set; }
        public string PassengerResourceId { get; set; }
        public IEnumerable<TrustedTravellerProgramResponse> TrustedTravellerPrograms { get; set; }

        public static explicit operator PassengerDetailsResponse(UpdatePassengerDetailsRequest source) => new()
        {
            PassengerInformation = new PassengerInformationResponse
            {
                BookingId = source.BookingId,
                Passengers =
                [
                    new PassengerResponse
                    {
                        PassengerResourceId = source.PassengerResourceId,
                        AdvancePassengerInformation = new AdvancePassengerInformationResponse
                        {
                            TrustedTravellerPrograms = source.TrustedTravellerPrograms
                        },
                        ActionType = "ADD"
                    }
                ]
            }
        };
    }
}
