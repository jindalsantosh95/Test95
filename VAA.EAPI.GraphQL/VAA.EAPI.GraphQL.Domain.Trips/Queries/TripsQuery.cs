using HotChocolate;
using HotChocolate.Resolvers;
using HotChocolate.Types;
using Microsoft.Extensions.Logging;
using System.Net;
using VAA.EAPI.GraphQL.Common;
using VAA.EAPI.GraphQL.Common.Entities;
using VAA.EAPI.GraphQL.Common.Extensions;
using VAA.EAPI.GraphQL.Common.Helper;
using VAA.EAPI.GraphQL.Common.Serialisation;
using VAA.EAPI.GraphQL.Domain.Trips.Helper;
using VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Models.BasketMS;
using CheckInMS = VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Models.CheckInMS;
using VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Models.PassengerDetails;
using VAA.EAPI.GraphQL.Domain.Trips.Interface;
using VAA.EAPI.GraphQL.Domain.Trips.Interface.Features;
using VAA.EAPI.GraphQL.Domain.Trips.Models.ReferenceData;
using Graphql = VAA.EAPI.GraphQL.Domain.Trips.Models.Trips.Virgin;
using HttpResponseException = VAA.EAPI.GraphQL.Common.Exceptions.HttpResponseException;
using TripMS = VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Models.TripMS;

namespace VAA.EAPI.GraphQL.Domain.Trips.Queries
{
    [ExtendObjectType(OperationTypeNames.Query)]
    public class TripsQuery
    {
        private const string BookingTypeGroup = "group";

        private ITripsDetailsQueryProvider _tripsDetailsQueryProvider;
        private readonly IReferenceServiceProvider _referenceServiceProvider;
        private readonly IPassengerDetailsProvider _passengerDetailsServiceProvider;
        private readonly IBasketProvider _basketServiceProvider;
        private readonly ILogger<TripsQuery> _logger;
        private readonly ITripDetailsFeatureProvider _tripDetailsFeatureProvider;
        private readonly ITravelRequirementsQueryProvider _travelRequirementsQueryProvider;

        public TripsQuery(ITripsDetailsQueryProvider tripsDetailsQueryProvider, IReferenceServiceProvider referenceServiceProvider, IPassengerDetailsProvider passengerDetailsServiceProvider, IBasketProvider basketServiceProvider, ILogger<TripsQuery> logger, ITripDetailsFeatureProvider tripDetailsFeatureProvider, ITravelRequirementsQueryProvider travelRequirementsQueryProvider)
        {
            _tripsDetailsQueryProvider = tripsDetailsQueryProvider ?? throw new ArgumentNullException(nameof(tripsDetailsQueryProvider));
            _referenceServiceProvider = referenceServiceProvider ?? throw new ArgumentNullException(nameof(referenceServiceProvider));
            _passengerDetailsServiceProvider = passengerDetailsServiceProvider ?? throw new ArgumentNullException(nameof(passengerDetailsServiceProvider));
            _basketServiceProvider = basketServiceProvider ?? throw new ArgumentNullException(nameof(basketServiceProvider));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _tripDetailsFeatureProvider = tripDetailsFeatureProvider;
            _travelRequirementsQueryProvider = travelRequirementsQueryProvider ?? throw new ArgumentNullException(nameof(travelRequirementsQueryProvider));
        }

        [GraphQLName("Booking")]
        [GraphQLDescription("Retrieve trip details using PNR or ticket number")]
        public async Task<Graphql.Booking> GetTripsDetails(Graphql.TripsDetailsRequest tripDetailsRequest, IResolverContext context)
        {
            ArgumentNullException.ThrowIfNull(tripDetailsRequest);

            bool excludeGroupBookings = ConfigReader.Try(Common.Constants.Environment.ExcludeGroupBookings, true);

            try
            {
                var tripDetailsTask = _tripsDetailsQueryProvider.GetTripDetails((TripMS.TripsDetailsRequest)tripDetailsRequest);
                var travelRequirementsTask = AreTravelDocumentFieldsSelected(context) && HasPassengerNames(tripDetailsRequest) && !tripDetailsRequest.ConfirmationNum.IsNullOrEmpty()
                    ? _travelRequirementsQueryProvider.GetTravelRequirements((CheckInMS.TravelRequirementsRequest)new Graphql.TripDetails
                    {
                        PnrLocator = tripDetailsRequest.ConfirmationNum,
                        GivenNames = tripDetailsRequest.GivenNames,
                        Surname = tripDetailsRequest.Surname
                    })
                    : Task.FromResult<CheckInMS.TravelRequirementsResponse>(null);

                await Task.WhenAll(tripDetailsTask, travelRequirementsTask);

                var msResponse = await tripDetailsTask;
                var travelRequirementsResponse = await travelRequirementsTask;

                // Validate group bookings if the flag is enabled. This is to prevent unsupported
                // group bookings from being added to the system, which could cause issues downstream.

                if (excludeGroupBookings && msResponse.ReservationType.Is(BookingTypeGroup))
                    throw new InvalidOperationException("Group bookings are not supported");

                IEnumerable<AirportRefData> airportRefData = [];
                IEnumerable<CabinRefData> cabinRefData = [];
                IEnumerable<AirlineRefData> airlineRefData = [];

                var airportRefDataTask = _referenceServiceProvider.RetrieveAirportDataAsync();
                var cabinRefDataTask = _referenceServiceProvider.RetrieveCabinDataAsync();
                var airlineRefDataTask = _referenceServiceProvider.RetrieveAirlineDetails();

                await Task.WhenAll(airportRefDataTask, cabinRefDataTask, airlineRefDataTask);

                airportRefData = airportRefDataTask.Result;
                cabinRefData = cabinRefDataTask.Result;
                airlineRefData = airlineRefDataTask.Result;

                var bookingId = await BookingIdHelper.CreateBookingId(null, msResponse.Id,
                         tripDetailsRequest.GivenNames,
                         tripDetailsRequest.Surname,
                         tripDetailsRequest.ConfirmationNum ?? null,
                         tripDetailsRequest.TicketNum ?? null,
                         Guid.NewGuid().ToString());

                //retrieve booking email
                PassengerDetails? paxInfoResponse = null;
                try
                {
                    paxInfoResponse = await _passengerDetailsServiceProvider.GetPassengerDetails(await BookingIdHelper.GetMSBookingId(bookingId));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to retrieve passenger details for email");
                }

                var basketId = Guid.Parse(await BookingIdHelper.GetBasketId(bookingId));
                var basket = await _basketServiceProvider.CreateBasketAsync((CreateBasketRequest)(msResponse, basketId, airportRefData, paxInfoResponse?.PassengerInformation?.Passengers.FirstOrDefault()?.AdvancePassengerInformation?.PassengerContact?.Email?.Address));

                try
                {
                    if (!_tripDetailsFeatureProvider.IsNullOrDefault())
                    {
                        return await _tripDetailsFeatureProvider.AddTripDetailFeature(msResponse, cabinRefData, bookingId, airportRefData, airlineRefData, travelRequirementsResponse);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Correlation Id - {AsyncContext.Current.CorrelationId} | Booking reference - {tripDetailsRequest.ConfirmationNum ?? tripDetailsRequest.TicketNum} | Error adding trip detail features - {ex.Message}");
                }

                return (Graphql.Booking)(msResponse, cabinRefData, bookingId, airportRefData, airlineRefData, travelRequirementsResponse);
            }

            catch (Exception ex)
            {
                var msError = ex.Message;

                if (ex.InnerException is HttpResponseException httpResponseException)
                    msError = await httpResponseException.Response?.Content?.ReadAsStringAsync();

                _logger.LogError(ex, $"Correlation Id - {AsyncContext.Current.CorrelationId} | Booking reference - {tripDetailsRequest.ConfirmationNum ?? tripDetailsRequest.TicketNum} | Error message - {msError}");

                throw new GraphQLException(ErrorBuilder.New()
                    .SetMessage("We couldn’t add your trip at the moment. Please try again later.")
                    .SetExtension("Reason", Json.Serialize(new List<ErrorDetails>()
                    {
                        new() {
                            Code = Constants.ErrorCodes.RetrieveTripDetailsErrorCode,
                            DeveloperMessage = $"Error retrieving Booking Ref {tripDetailsRequest.ConfirmationNum?? tripDetailsRequest.TicketNum} | ErrorID: {AsyncContext.Current.CorrelationId} with Error Messsage: {ex.Message} | TimeStamp: {DateTime.UtcNow}",
                            Message = $"We couldn’t add your trip at the moment. Please try again later."
                        }
                    }))
                    .SetExtension("ErrorSource", context.Operation.Name)
                    .SetCode(((int)HttpStatusCode.UnprocessableEntity).ToString())
                    .Build());
            }
        }

        private static bool HasPassengerNames(Graphql.TripsDetailsRequest request) =>
            request is not null && !request.GivenNames.IsNullOrEmpty() && !request.Surname.IsNullOrEmpty();

        private static bool AreTravelDocumentFieldsSelected(IResolverContext context)
        {
            if (context is null) return true;

            var rootType = context.Selection.Type.NamedType() as ObjectType;

            return rootType is not null && ContainsTravelDocumentField(context, context.GetSelections(rootType));
        }

        private static bool ContainsTravelDocumentField(IResolverContext context, IReadOnlyList<IFieldSelection> selections)
        {
            foreach (var selection in selections)
            {
                if (selection.Field.Name.Value is "areTravelDocsComplete" or "travelDocsStatusSummary") return true;

                if (selection.Type.NamedType() is not ObjectType childType) continue;

                foreach (var syntaxNode in selection.SyntaxNodes)
                {
                    if (syntaxNode.SelectionSet is not null &&
                        ContainsTravelDocumentField(context, context.GetSelections(childType, syntaxNode.SelectionSet)))
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }
}
