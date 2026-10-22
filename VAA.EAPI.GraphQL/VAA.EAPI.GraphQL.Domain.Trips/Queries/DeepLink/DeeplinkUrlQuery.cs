using HotChocolate;
using HotChocolate.Types;
using VAA.EAPI.GraphQL.Common.Extensions;
using VAA.EAPI.GraphQL.Domain.Trips.Helper;
using VAA.EAPI.GraphQL.Domain.Trips.Interface;
using VAA.EAPI.GraphQL.Domain.Trips.Models.Deeplink;
using VAA.EAPI.GraphQL.Domain.Trips.Models.Enum;

namespace VAA.EAPI.GraphQL.Domain.Trips.Queries.DeepLink
{
    [ExtendObjectType(OperationTypeNames.Query)]
    public class DeeplinkUrlQuery
    {
        private readonly IBasketProvider _basketServiceProvider;
        private ITripsDetailsQueryProvider _tripsDetailsQueryProvider;

        public DeeplinkUrlQuery(ITripsDetailsQueryProvider tripsDetailsQueryProvider, IBasketProvider basketServiceProvider)
        {
            _tripsDetailsQueryProvider = tripsDetailsQueryProvider ?? throw new ArgumentNullException(nameof(tripsDetailsQueryProvider));
            _basketServiceProvider = basketServiceProvider ?? throw new ArgumentNullException(nameof(basketServiceProvider));
        }

        [GraphQLName("Deeplink")]
        [GraphQLDescription("Retrieve deeplink url based on requested option")]
        public async Task<DeeplinkResponse> GetDeeplinkUrl(DeeplinkRequest request)
        {
            ArgumentNullException.ThrowIfNull(request.TripReservationId);
            ArgumentNullException.ThrowIfNull(request.DeeplinkOption);

            Infrastructure.Models.TripMS.DeepLink.DeeplinkRequest msRequest;

            switch (request.DeeplinkOption)
            {
                case DeeplinkOption.TravelInsurance:

                    var basketId = await BookingIdHelper.GetBasketId(request.TripReservationId);
                    var basket = await _basketServiceProvider.RetrieveBasketAsync(basketId);

                    if (string.IsNullOrEmpty(basket?.BookingDetails?.Pos?.CountryCode))
                        throw new ArgumentNullException("POSCountry is required for Travel Insurance Deeplink");

                    msRequest = (Infrastructure.Models.TripMS.DeepLink.DeeplinkRequest)(basket.BookingDetails, request);

                    break;
                case DeeplinkOption.IROP_ViewDetails or DeeplinkOption.IROP_FAF or DeeplinkOption.FlightCancel or DeeplinkOption.FlightChange or DeeplinkOption.FlyReady or DeeplinkOption.BidForUpgrade or DeeplinkOption.BaggageAllowance or DeeplinkOption.FlightDisruption:

                    msRequest = (Infrastructure.Models.TripMS.DeepLink.DeeplinkRequest)request;

                    break;
                case DeeplinkOption.POPSMeal:

                    if (request?.AdditionalData.IsNullOrEmpty() == true)
                        throw new ArgumentNullException("Segment Details Missing for POPSMeal");

                    var basketID = await BookingIdHelper.GetBasketId(request.TripReservationId);
                    var basketResponse = await _basketServiceProvider.RetrieveBasketAsync(basketID);

                    msRequest = (Infrastructure.Models.TripMS.DeepLink.DeeplinkRequest)(basketResponse.BookingDetails, request);

                    break;
                case DeeplinkOption.ManageHoliday:

                    if (request?.TripReservationId.IsNullOrEmpty() == true)
                        throw new ArgumentNullException("Holiday Booking Details Missing for ManageHoliday");

                    msRequest = (Infrastructure.Models.TripMS.DeepLink.DeeplinkRequest)request;

                    break;
                case DeeplinkOption.ConciergeTripAware:

                    if (request?.TripReservationId.IsNullOrEmpty() == true)
                        throw new ArgumentNullException(nameof(request.TripReservationId));

                    msRequest = (Infrastructure.Models.TripMS.DeepLink.DeeplinkRequest)request;

                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(request.DeeplinkOption), request.DeeplinkOption, "Invalid Deeplink Option");
            }

            return (DeeplinkResponse)(await _tripsDetailsQueryProvider.GetDeeplinkUrl(msRequest));
        }
    }
}