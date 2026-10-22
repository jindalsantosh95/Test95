namespace VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Models.TripMS.DeepLink
{
    public class DeeplinkRequest
    {
        public string? TripReservationId { get; set; }
        public Enums.DeeplinkOption DeeplinkOption { get; set; }
        public IEnumerable<string>? AdditionalData { get; set; }
        public string? POSCountryCode { get; set; }
        public string? FlightNumber { get; set; }

        public static explicit operator DeeplinkRequest((BasketMS.Booking bookingDetails, Trips.Models.Deeplink.DeeplinkRequest request) source)
        {
            var (bookingDetails, request) = source;

            var requestedOption = (Enums.DeeplinkOption)request.DeeplinkOption;

            switch (requestedOption)
            {
                case Enums.DeeplinkOption.TravelInsurance:
                    return new()
                    {
                        TripReservationId = request.TripReservationId,
                        DeeplinkOption = (Enums.DeeplinkOption)request.DeeplinkOption,
                        POSCountryCode = bookingDetails.Pos.CountryCode
                    };
                case Enums.DeeplinkOption.POPSMeal:
                    var segments = bookingDetails.Slices.SelectMany(r => r.Segments).Where(seg => seg.Id == request.AdditionalData.First());

                    return new()
                    {
                        TripReservationId = request.TripReservationId,
                        DeeplinkOption = (Enums.DeeplinkOption)request.DeeplinkOption,
                        FlightNumber = segments.First().OperatingCarrier.FlightNumber.Trim('0')
                    };
                default:
                    throw new ArgumentOutOfRangeException(nameof(request.DeeplinkOption), request.DeeplinkOption, "Invalid Deeplink Option for this conversion");
            }
        }

        public static explicit operator DeeplinkRequest(Trips.Models.Deeplink.DeeplinkRequest request)
        {
            return new()
            {
                TripReservationId = request.TripReservationId,
                DeeplinkOption = (Enums.DeeplinkOption)request.DeeplinkOption,
                AdditionalData = request.AdditionalData
            };
        }
    }
}