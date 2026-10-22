using HotChocolate;
using HotChocolate.Resolvers;
using HotChocolate.Types;
using Microsoft.Extensions.Logging;
using System.Net;
using VAA.EAPI.GraphQL.Common;
using VAA.EAPI.GraphQL.Common.Extensions;
using VAA.EAPI.GraphQL.Common.Helper;
using VAA.EAPI.GraphQL.Common.Serialisation;
using VAA.EAPI.GraphQL.Domain.Trips.Helper;
using VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Models.HolidaysMS.TripsCore;
using VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Models.OrderData;
using VAA.EAPI.GraphQL.Domain.Trips.Interface;
using VAA.EAPI.GraphQL.Domain.Trips.Models.ReferenceData;
using VAA.EAPI.GraphQL.Infrastructure.Entities;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;
using Graphql = VAA.EAPI.GraphQL.Domain.Trips.Models.Trips.Virgin;

namespace VAA.EAPI.GraphQL.Domain.Trips.Queries
{
    [ExtendObjectType(OperationTypeNames.Query)]
    public class TripDetailsListQuery(IReferenceServiceProvider referenceServiceProvider, ITripsDetailsQueryProvider tripService, IHolidayProvider holidayProvider, ILogger<TripDetailsListQuery> logger, ITokenB2CProvider<string> tokenB2CProvider, IOrderDataProvider orderDataProvider, ITripAircraftEligibilityService tripAircraftEligibilityService)
    {
        private const string BoookingTypeGroup = "group";

        private readonly IReferenceServiceProvider _referenceServiceProvider = referenceServiceProvider ?? throw new ArgumentNullException(nameof(referenceServiceProvider));
        private readonly ITripsDetailsQueryProvider _tripService = tripService ?? throw new ArgumentNullException(nameof(tripService));
        private readonly IHolidayProvider _holidayProvider = holidayProvider ?? throw new ArgumentNullException(nameof(holidayProvider));
        private readonly ILogger<TripDetailsListQuery> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        private readonly ITokenB2CProvider<string> _tokenB2CProvider = tokenB2CProvider ?? throw new ArgumentNullException(nameof(tokenB2CProvider));
        private readonly IOrderDataProvider _orderDataProvider = orderDataProvider ?? throw new ArgumentNullException(nameof(orderDataProvider));
        private readonly ITripAircraftEligibilityService _tripAircraftEligibilityService = tripAircraftEligibilityService ?? throw new ArgumentNullException(nameof(tripAircraftEligibilityService));

        [GraphQLName("TripDetails")]
        [GraphQLDescription("Retrieve trip details of multiple pnrs & holidays")]
        public async Task<IEnumerable<Graphql.Booking>> GetTripDetailsList(Models.Trips.TripDetailsRequest request, IResolverContext context = null)
        {
            Models.Trips.TripDetailsRequest tripDetailsRequest = null;

            bool excludeGroupBookings = ConfigReader.Try(Common.Constants.Environment.ExcludeGroupBookings, true);

            try
            {
                var jwtPrincipalFeature = AsyncContext.Current.Context.GetJwtPrincipalFeature();

                if (jwtPrincipalFeature != null)
                {
                    var tokenResponse = await _tokenB2CProvider.ValidateTokenAsync(jwtPrincipalFeature.AccessToken.Token);
                    var claim = Json.Deserialize<TokenB2CResponse>(tokenResponse);

                    if (claim?.FcNumber.IsNullOrEmpty() == true) throw new Exception("No Flying Club number linked to logged-in Customer");

                    var tasks = new List<Task<FlightBookingsResponse>>
                    {
                        _orderDataProvider.RetrieveBookings(claim.FcNumber, null)
                    };

                    if (!string.IsNullOrEmpty(claim?.EmailAddress))
                        tasks.Add(_orderDataProvider.RetrieveBookings(null, claim.EmailAddress));

                    await Task.WhenAll(tasks);

                    var tripDetailsRequests = tasks.Select(x =>
                                                   {
                                                       // In order to avoid returning group bookings, we are filtering out the group bookings at this level itself.
                                                       x.Result.FlightBookingsDetails = x.Result.FlightBookingsDetails?.Where(x => !excludeGroupBookings || (excludeGroupBookings && !x.CustomerDetails.Any(c => c.Bookingtype.Is(BoookingTypeGroup)))).ToArray();

                                                       return x.Result;
                                                   })
                                                   .Where(x => x.FlightBookingsDetails?.Any() == true)
                                                   .Select(x => (Models.Trips.TripDetailsRequest)(x, claim?.FcNumber))
                                                   .ToArray();


                    var tripDetails = tripDetailsRequests.Where(x => x?.TripDetails != null)
                                                         .SelectMany(r => r.TripDetails)
                                                         .DistinctBy(x => x.PnrLocator, StringComparer.OrdinalIgnoreCase);

                    tripDetailsRequest = tripDetails.Any() ? new Models.Trips.TripDetailsRequest { TripDetails = tripDetails } : null;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Correlation Id - {AsyncContext.Current.CorrelationId} | Unexpected error in Fetching future Booking details of logged-in customer.");
            }

            try
            {
                var response = Enumerable.Empty<Graphql.Booking>();
                var combinedRequests = new[] { request?.TripDetails?.Where(x => !x.PnrLocator.IsNullOrEmpty()), tripDetailsRequest?.TripDetails }
                                              .Where(x => x != null)
                                              .SelectMany(x => x!)
                                              .GroupBy(x => x.PnrLocator, StringComparer.OrdinalIgnoreCase)
                                              .Select(g => g.FirstOrDefault(x => !string.IsNullOrEmpty(x.FlyingClubNumber)) ?? g.First());

                var holidayBookingRequest = request?.TripDetails?.Where(x => !x.BookingReference.IsNullOrEmpty()).DistinctBy(x => x.BookingReference);

                if (!combinedRequests.Any() && !holidayBookingRequest.Any()) return response;

                var msRequest = combinedRequests.Select(x => new Infrastructure.Models.TripMS.TripsDetailsRequest
                {
                    Using = Infrastructure.Models.TripMS.RequestOptionCodeType.PNR,
                    ConfirmationNum = x.PnrLocator,
                    GivenNames = x.GivenNames,
                    Surname = x.Surname,
                    LoyaltyNum = x.FlyingClubNumber
                });

                var holidaysMsRequest = holidayBookingRequest.Select(x => new TripsDetailsRequest
                {
                    BookingReference = x.BookingReference,
                    Surname = x.Surname,
                    DateOfBirth = x.DateOfBirth.GetValueOrDefault(),
                    Using = RequestOptionCodeType.HOLIDAYS
                });

                var aircraftEnrichmentOptions = GetAircraftEnrichmentOptions(context);
                var tripTask = msRequest?.Any() == true ? _tripService.GetTripDetailsList(msRequest) : Task.FromResult<Infrastructure.Models.TripMS.TripListResponseStore>(null);
                var holidayTask = holidaysMsRequest?.Any() == true ? _holidayProvider.RetrieveBookingList(holidaysMsRequest) : Task.FromResult<TripListResponseStore>(null);
                await Task.WhenAll(tripTask, holidayTask);

                var tripDetailsResponseList = await tripTask;
                var holidaysResponseList = await holidayTask;

                // Filter out group bookings before returning the response from this query.
                // This filtering happens after trip details have already been retrieved from downstream systems,
                // so it prevents group bookings from being returned here but does not avoid those downstream calls.

                if (tripDetailsResponseList is not null && tripDetailsResponseList.TripListResponse is not null)
                    tripDetailsResponseList.TripListResponse = tripDetailsResponseList.TripListResponse.Where(x => !excludeGroupBookings || (excludeGroupBookings && !(x.TripDetails?.Booking?.ReservationType.Is(BoookingTypeGroup) ?? false))).ToArray();

                IEnumerable<Tuple<string, Infrastructure.Models.TripMS.TripsDetailsResponse, Infrastructure.Models.TripMS.TripsDetailsRequest>> tripMSResponsesByBooking = [];
                IEnumerable<AirportRefData> airportRefData = [];
                IEnumerable<CabinRefData> cabinRefData = [];
                IEnumerable<AirlineRefData> airlineRefData = [];

                var airportRefDataTask = Task.FromResult<IEnumerable<AirportRefData>>([]);
                var cabinRefDataTask = Task.FromResult<IEnumerable<CabinRefData>>([]);
                var airlineRefDataTask = Task.FromResult<IEnumerable<AirlineRefData>>([]);

                if (tripDetailsResponseList?.TripListResponse?.Any() == true && tripDetailsResponseList!.TripListResponse!.All(td => td?.TripDetails?.Status is HttpStatusCode status && (int)status >= (int)HttpStatusCode.InternalServerError) == true)
                {
                    throw new Exception(Constants.ErrorCodes.RetrieveTripRefreshErrorCode);
                }
                else
                {
                    var tasks = tripDetailsResponseList?.TripListResponse.Select(async tripResponse =>
                    {
                        HttpStatusCode? status = tripResponse?.TripDetails?.Status;

                        if (status != HttpStatusCode.OK)
                        {
                            if (status >= HttpStatusCode.BadRequest && status < HttpStatusCode.InternalServerError)
                            {
                                var errorDetails = tripResponse?.TripDetails?.Errors;
                                var errorCodes = errorDetails?.Select(ed => ed.Code).Distinct();
                                if (errorCodes != null && errorCodes.Any(e => e != null && e.Contains(Constants.ErrorCodes.DeltaErrorCode)))
                                {
                                    return null;
                                }
                            }
                            return new Tuple<string, Infrastructure.Models.TripMS.TripsDetailsResponse, Infrastructure.Models.TripMS.TripsDetailsRequest>(
                                null, tripResponse.TripDetails, tripResponse.TripSearchRequest);
                        }

                        string bookingId = await BookingIdHelper.CreateBookingId(
                                null,
                                tripResponse.TripDetails.Booking.Id,
                                tripResponse.TripSearchRequest.GivenNames,
                                tripResponse.TripSearchRequest.Surname,
                                tripResponse.TripSearchRequest.ConfirmationNum ?? null,
                                tripResponse.TripSearchRequest.TicketNum ?? null);


                        return new Tuple<string, Infrastructure.Models.TripMS.TripsDetailsResponse, Infrastructure.Models.TripMS.TripsDetailsRequest>(
                            bookingId, tripResponse.TripDetails, tripResponse.TripSearchRequest);
                    });

                    var results = await Task.WhenAll(tasks ?? Enumerable.Empty<Task<Tuple<string, Infrastructure.Models.TripMS.TripsDetailsResponse, Infrastructure.Models.TripMS.TripsDetailsRequest>>>());

                    tripMSResponsesByBooking = results.Where(x => x != null)!;

                    var hasTripMsResponses = !tripMSResponsesByBooking.IsNullOrEmpty();
                    var hasHolidaysResponses = holidaysResponseList?.TripListResponse?.Any() == true;
                    var aircraftEnrichmentTask = Task.CompletedTask;

                    if (hasTripMsResponses)
                    {
                        airportRefDataTask = _referenceServiceProvider.RetrieveAirportDataAsync();
                        cabinRefDataTask = _referenceServiceProvider.RetrieveCabinDataAsync();
                        airlineRefDataTask = _referenceServiceProvider.RetrieveAirlineDetails();

                        aircraftEnrichmentTask = _tripAircraftEligibilityService.EnrichAircraftDetailsAsync(tripMSResponsesByBooking.Where(x => x.Item2?.Booking != null).Select(x => x.Item2.Booking), aircraftEnrichmentOptions);
                    }
                    else if (hasHolidaysResponses)
                    {
                        airportRefDataTask = _referenceServiceProvider.RetrieveAirportDataAsync();
                        airlineRefDataTask = _referenceServiceProvider.RetrieveAirlineDetails();
                    }

                    await Task.WhenAll(
                        aircraftEnrichmentTask,
                        airportRefDataTask,
                        cabinRefDataTask,
                        airlineRefDataTask);

                    airportRefData = airportRefDataTask.Result ?? [];
                    cabinRefData = cabinRefDataTask.Result ?? [];
                    airlineRefData = airlineRefDataTask.Result ?? [];
                }

                var tripsResponse = tripMSResponsesByBooking.Select(trip =>
                {
                    return (Graphql.Booking)(trip?.Item2, cabinRefData, trip?.Item1, airportRefData, trip.Item3, combinedRequests, airlineRefData);
                }).OrderBy(b => b?.Product?.SelectMany(p => p?.Slices)?.Min(s => s?.ScheduledDepartureLocalDateTime));

                var queryResponse = new List<Graphql.Booking>();

                if (tripsResponse.Any())
                {
                    queryResponse.AddRange(tripsResponse);
                }

                if (holidaysResponseList?.TripListResponse?.Any() == true)
                {
                    IEnumerable<PaymentMethod> paymentMethodRefData = await _referenceServiceProvider.RetrievePaymentMethodDetails();
                    var holidayTasks = holidaysResponseList.TripListResponse.Select(async x =>
                    {
                        string? tripResourceId = null;
                        if (x?.TripDetails?.Booking != null && x.TripSearchRequest != null)
                        {
                            tripResourceId = await BookingIdHelper.CreateBookingId(
                                null,
                                x.TripDetails.Booking.Id,
                                x.TripSearchRequest.GivenNames ?? null,
                                x.TripSearchRequest.Surname,
                                x.TripSearchRequest.ConfirmationNum ?? null,
                                x.TripSearchRequest.TicketNum ?? null,
                                Guid.NewGuid().ToString(),
                                x.TripSearchRequest.DateOfBirth.ToString("yyyy-MM-dd"));
                        }

                        return (Graphql.Booking)(x.TripDetails, airportRefData, x.TripSearchRequest, paymentMethodRefData, tripResourceId, airlineRefData);
                    });

                    var holidays = await Task.WhenAll(holidayTasks);

                    IEnumerable<Graphql.Booking> activeBookings = holidays.Where(h => !BookingIdHelper.IsHolidayBookingExpired(h)).ToArray();

                    var holidayFlightPnrRequests = activeBookings
                        .SelectMany(holiday => holiday.Product?.Where(product => product != null && product.SupportedFlight && !(product.Reference?.PnrLocator ?? string.Empty).IsNullOrWhiteSpace()) ?? [])
                        .Select(product =>
                        {
                            var firstPassenger = product.Passengers?.FirstOrDefault();
                            return new
                            {
                                PnrLocator = product.Reference!.PnrLocator,
                                PassengerFirstName = firstPassenger?.FirstName,
                                PassengerLastName = firstPassenger?.LastName
                            };
                        })
                        .DistinctBy(x => x.PnrLocator, StringComparer.OrdinalIgnoreCase)
                        .ToArray();

                    var holidayFlightTrips = holidayFlightPnrRequests.Any()
                        ? await GetTripDetailsList(new Models.Trips.TripDetailsRequest
                        {
                            TripDetails = [.. holidayFlightPnrRequests.Select(x => new Graphql.TripDetails
                            {
                                PnrLocator = x.PnrLocator,
                                Surname = x.PassengerLastName,
                                GivenNames = x.PassengerFirstName
                            })]
                        }, context)
                        : [];

                    foreach (var holidayBooking in activeBookings)
                    {

                        var supportedFlights = holidayBooking.Product?.Where(x => x != null && x.SupportedFlight && !(x.Reference?.PnrLocator ?? string.Empty).IsNullOrWhiteSpace()).ToArray() ?? [];

                        if (!supportedFlights.Any())
                        {
                            holidayBooking.Product = HolidayQuery.GetOrderedProducts(holidayBooking.Product?.ToList() ?? new List<Graphql.Product>());
                            queryResponse.Add(holidayBooking);

                            continue;
                        }

                        var pnrs = supportedFlights.Select(x =>
                        {
                            var firstPassenger = x.Passengers?.FirstOrDefault();

                            return new
                            {
                                x.Reference!.PnrLocator,
                                PassengerFirstName = firstPassenger?.FirstName,
                                PassengerLastName = firstPassenger?.LastName
                            };
                        })
                        .DistinctBy(x => x.PnrLocator, StringComparer.OrdinalIgnoreCase)
                         .ToArray();

                        var products = HolidayQuery.GetProductsWithSingleSlice(holidayBooking.Product?.Where(p => p != null)).ToList();

                        foreach (var pnr in pnrs)
                        {
                            var trip = holidayFlightTrips.FirstOrDefault(x => x.Product?.Any(p => p.Reference?.PnrLocator.Is(pnr.PnrLocator) == true) == true);

                            if (trip is null) continue;

                            var indexes = products.Select((x, i) => new { Product = x, Index = i })
                                                  .Where(x => x.Product.Reference?.PnrLocator.Is(pnr.PnrLocator) == true)
                                                  .Select(x => x.Index)
                                                  .ToArray();

                            var tripAirlineProduct = trip.Product?.FirstOrDefault(p => p.BookingType.Is("AIRLINE") && p.Reference?.PnrLocator.Is(pnr.PnrLocator) == true);

                            if (tripAirlineProduct?.Slices is null) continue;

                            HolidayQuery.OverwriteHolidaySingleSliceProductsFromTripProduct(indexes.Select(index => products[index]), tripAirlineProduct, trip.Passengers);
                        }

                        var orderedProducts = HolidayQuery.GetOrderedProducts(products);

                        holidayBooking.Product = HolidayQuery.RemoveDuplicateProductsByReferenceAndSliceDetails(orderedProducts);

                        queryResponse.Add(holidayBooking);
                    }

                    queryResponse = [.. queryResponse.OrderBy(b => b?.TravelDates?.DepartureDate != null ? (DateTime?)b.TravelDates.DepartureDate.Value.ToDateTimeUnspecified() : b?.Product?.Where(p => p?.Slices != null).SelectMany(p => p.Slices).Where(s => s?.ScheduledDepartureLocalDateTime != null).Select(s => s.ScheduledDepartureLocalDateTime).Min())];
                }

                return queryResponse;
            }

            catch (Exception ex)
            {
                _logger.LogError(ex, $"Correlation Id - {AsyncContext.Current.CorrelationId} | Unexpected error in TripDetailsListQuery.");
                throw new Exception(Constants.ErrorCodes.RetrieveTripRefreshErrorCode, ex);
            }
        }

        private static TripAircraftEnrichmentOptions GetAircraftEnrichmentOptions(IResolverContext context)
        {
            if (context is null) return new(true, true);

            var rootType = context.Selection.Type.NamedType() as ObjectType;
            if (rootType is null) return new(false, false);

            var fields = new HashSet<string>(StringComparer.Ordinal);
            CollectSelectedFieldNames(context, context.GetSelections(rootType), fields);

            return new(fields.Contains("tailNumber"), fields.Contains("isStarlinkEligible"));
        }

        private static void CollectSelectedFieldNames(IResolverContext context, IReadOnlyList<IFieldSelection> selections, ISet<string> fields)
        {
            foreach (var selection in selections)
            {
                fields.Add(selection.Field.Name.Value);

                if (selection.Type.NamedType() is not ObjectType childType) continue;

                foreach (var syntaxNode in selection.SyntaxNodes)
                {
                    if (syntaxNode.SelectionSet is not null)
                        CollectSelectedFieldNames(context, context.GetSelections(childType, syntaxNode.SelectionSet), fields);
                }
            }
        }

    }
}

