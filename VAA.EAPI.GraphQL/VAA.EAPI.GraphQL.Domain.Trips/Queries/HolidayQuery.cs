using HotChocolate;
using HotChocolate.Resolvers;
using HotChocolate.Types;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Linq;
using NodaTime;
using System.Net;
using VAA.EAPI.GraphQL.Common;
using VAA.EAPI.GraphQL.Common.Entities;
using VAA.EAPI.GraphQL.Common.Exceptions;
using VAA.EAPI.GraphQL.Common.Extensions;
using VAA.EAPI.GraphQL.Common.Serialisation;
using VAA.EAPI.GraphQL.Domain.Trips.Helper;
using VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Models.HolidaysMS.Documents;
using VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Models.HolidaysMS.TripsCore;
using VAA.EAPI.GraphQL.Domain.Trips.Interface;
using VAA.EAPI.GraphQL.Domain.Trips.Models.ReferenceData;
using GraphqlTrips = VAA.EAPI.GraphQL.Domain.Trips.Models.Trips.Virgin;
using HolidayDocumentDetailsRequest = VAA.EAPI.GraphQL.Domain.Trips.Models.Holidays.HolidayDocumentDetailsRequest;
using HolidayDocumentDetailsResponse = VAA.EAPI.GraphQL.Domain.Trips.Models.Holidays.HolidayDocumentDetailsResponse;
using HolidayDocumentDownloadRequest = VAA.EAPI.GraphQL.Domain.Trips.Models.Holidays.HolidayDocumentDownloadRequest;
using HolidayDocumentDownloadResponse = VAA.EAPI.GraphQL.Domain.Trips.Models.Holidays.HolidayDocumentDownloadResponse;


namespace VAA.EAPI.GraphQL.Domain.Trips.Queries
{
    [ExtendObjectType(OperationTypeNames.Query)]
    public class HolidayQuery(ILogger<HolidayQuery> logger, IHolidayProvider holidayProvider, TripDetailsListQuery tripDetailsListQuery, IReferenceServiceProvider referenceServiceProvider)
    {
        private readonly ILogger<HolidayQuery> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        private readonly IHolidayProvider _holidayProvider = holidayProvider ?? throw new ArgumentNullException(nameof(holidayProvider));
        private readonly TripDetailsListQuery _tripDetailsListQuery = tripDetailsListQuery ?? throw new ArgumentNullException(nameof(tripDetailsListQuery));
        private readonly IReferenceServiceProvider _referenceServiceProvider = referenceServiceProvider ?? throw new ArgumentNullException(nameof(referenceServiceProvider));

        [GraphQLName("retrieveHolidayBooking")]
        [GraphQLDescription("Retrieve holiday booking details using booking reference, date of birth, and surname")]
        public async Task<GraphqlTrips.Booking> RetrieveHolidayBooking(GraphqlTrips.TripsDetailsRequest request, IResolverContext context = null)
        {
            ArgumentNullException.ThrowIfNull(request);
            if (request.BookingReference.IsNullOrWhiteSpace()) throw new ArgumentNullException(nameof(request.BookingReference));
            if (request.Surname.IsNullOrWhiteSpace()) throw new ArgumentNullException(nameof(request.Surname));
            if (request.DateOfBirth == default) throw new ArgumentNullException(nameof(request.DateOfBirth));

            try
            {
                var response = await _holidayProvider.RetrieveBooking((TripsDetailsRequest)request);

                if (HasHolidayValidationError(response?.Errors))
                    throw new Exception(Constants.ErrorCodes.RetrieveHolidayBookingValidation);

                IEnumerable<AirportRefData> airportRefData = [];
                IEnumerable<PaymentMethod> paymentMethodRefData = [];
                IEnumerable<AirlineRefData> airlineRefData = [];

                var airportRefDataTask = _referenceServiceProvider.RetrieveAirportDataAsync();
                var paymentMethodRefDataTask = _referenceServiceProvider.RetrievePaymentMethodDetails();
                var airlineRefDataTask = _referenceServiceProvider.RetrieveAirlineDetails();

                await Task.WhenAll(airportRefDataTask, paymentMethodRefDataTask, airlineRefDataTask);

                airportRefData = airportRefDataTask.Result ?? [];
                paymentMethodRefData = paymentMethodRefDataTask.Result ?? [];
                airlineRefData = airlineRefDataTask.Result ?? [];
                var holidayTripRequest = (TripsDetailsRequest)request;

                var tripResourceId = await BookingIdHelper.CreateBookingId(
                    null,
                    response.Booking.Id,
                    holidayTripRequest.GivenNames ?? null,
                    holidayTripRequest.Surname,
                    holidayTripRequest.ConfirmationNum ?? null,
                    holidayTripRequest.TicketNum ?? null,
                    Guid.NewGuid().ToString(),
                    holidayTripRequest.DateOfBirth.ToString("yyyy-MM-dd"));

                var holidayBooking = (GraphqlTrips.Booking)(response, airportRefData, holidayTripRequest, paymentMethodRefData, tripResourceId, airlineRefData);

                if (BookingIdHelper.IsHolidayBookingExpired(holidayBooking))
                {
                    _logger.LogWarning("Holiday Expired | CorrelationId: {CorrelationId} | BookingReference: {BookingReference} | ReturnDate: {ReturnDate} | Reason: Return date is outside 7 days", AsyncContext.Current.CorrelationId, request.BookingReference, holidayBooking?.TravelDates?.ReturnDate);

                    throw new Exception(Constants.ErrorCodes.RetrieveHolidayBooking);
                }

                //get all the air4 products that are supported flights with a pnr locator reference, then get the distinct pnr locators in a case-insensitive manner
                var pnrProducts = holidayBooking.Product?.Where(x => x != null && x.SupportedFlight && !(x.Reference?.PnrLocator ?? string.Empty).IsNullOrWhiteSpace()).ToArray() ?? [];

                // if there are no products with a pnr locator reference, return the holiday booking without attempting to get the trip details
                if (pnrProducts.Length == 0)
                {
                    holidayBooking.Product = GetOrderedProducts(holidayBooking.Product?.Where(p => p != null).ToList() ?? new List<GraphqlTrips.Product>());
                    return holidayBooking;
                }

                var products = GetProductsWithSingleSlice(holidayBooking.Product?.Where(p => p != null)).ToList();

                var pnrs = pnrProducts.Select(x =>
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

                var tripDetailsRequest = new Models.Trips.TripDetailsRequest()
                {
                    TripDetails = pnrs.Select(x =>
                    {
                        return new GraphqlTrips.TripDetails
                        {
                            PnrLocator = x.PnrLocator,
                            GivenNames = x.PassengerFirstName,
                            Surname = x.PassengerLastName,
                        };
                    })
                };

                var trips = await _tripDetailsListQuery.GetTripDetailsList(tripDetailsRequest, context);


                foreach (var pnr in pnrs)
                {
                    // find the trip that contains the product with the matching pnr locator
                    var trip = trips.FirstOrDefault(x => x.Product?.Any(p => p.Reference?.PnrLocator.Is(pnr.PnrLocator) == true) == true);

                    // if no trip is found, continue to the next pnr
                    if (trip is null) continue;

                    // find the indexes of the products with the matching pnr locator
                    var indexes = products.Select((x, i) => new { Product = x, Index = i })
                                          .Where(x => x.Product.Reference?.PnrLocator.Is(pnr.PnrLocator) == true)
                                          .Select(x => x.Index)
                                          .ToArray();

                    var tripAirlineProduct = trip.Product?.FirstOrDefault(p => p.BookingType.Is("AIRLINE") && p.Reference?.PnrLocator.Is(pnr.PnrLocator) == true);

                    if (tripAirlineProduct?.Slices is null) continue;

                    OverwriteHolidaySingleSliceProductsFromTripProduct(indexes.Select(index => products[index]), tripAirlineProduct, trip.Passengers);
                }

                var orderedProducts = GetOrderedProducts(products);

                // Remove duplicate slice-based products that may be created when multiple slices are returned from trip details,
                // while preserving the original product order.
                holidayBooking.Product = RemoveDuplicateProductsByReferenceAndSliceDetails(orderedProducts);

                return holidayBooking;
            }
            catch (HttpRequestException ex)
            {
                var message = ex.Message;

                if (ex.InnerException is HttpResponseException httpResponseException)
                {
                    try
                    {
                        var content = httpResponseException.Response?.Content;

                        if (content is not null)
                            message = await content!.ReadAsStringAsync();
                    }
                    catch (Exception innerEx)
                    {
                        _logger.LogError(innerEx, $"Correlation Id - {AsyncContext.Current.CorrelationId} | Booking reference - {request.BookingReference} | Error message - {innerEx.Message}, Original message - {message}");
                    }
                }

                _logger.LogError(ex, $"Correlation Id - {AsyncContext.Current.CorrelationId} | HttpResponseException for {request.BookingReference} | Error message - {message}");

                if (HasHolidayValidationError(message))
                    throw new Exception(Constants.ErrorCodes.RetrieveHolidayBookingValidation, ex);

                throw new Exception(Constants.ErrorCodes.RetrieveHolidayBooking, ex);
            }
            catch (Exception ex)
            {
                var message = ex.Message;

                _logger.LogError(ex, $"Correlation Id - {AsyncContext.Current.CorrelationId} | Booking reference - {request.BookingReference} | Error message - {message}");

                if (message.Is(Constants.ErrorCodes.RetrieveHolidayBookingValidation))
                    throw;

                throw new Exception(Constants.ErrorCodes.RetrieveHolidayBooking, ex);
            }
        }

        internal static void OverwriteHolidaySingleSliceProductsFromTripProduct(
            IEnumerable<GraphqlTrips.Product> holidaySliceProducts,
            GraphqlTrips.Product tripAirlineProduct,
            IEnumerable<GraphqlTrips.Passenger>? tripPassengers)
        {
            if (holidaySliceProducts is null) return;
            if (tripAirlineProduct?.Slices is null) return;

            var holidayFlights = holidaySliceProducts
                .Where(product =>
                    product is not null
                    && product.BookingType.Is("AIRLINE")
                    && product.SupportedFlight)
                .Where(product =>
                {
                    var slices = product.Slices?.Take(2).ToArray();
                    if (slices is not { Length: 1 } || slices[0] is null)
                        return false;

                    var slice = slices[0];
                    return slice.ScheduledDepartureLocal.HasValue
                        && slice.Origin?.Code.IsNullOrWhiteSpace() == false
                        && slice.Destination?.Code.IsNullOrWhiteSpace() == false;
                })
                .ToList();

            if (holidayFlights.Count == 0) return;

            var tripSlices = tripAirlineProduct.Slices
                .Where(slice =>
                    slice?.ScheduledDepartureLocal.HasValue == true
                    && slice.Origin?.Code.IsNullOrWhiteSpace() == false
                    && slice.Destination?.Code.IsNullOrWhiteSpace() == false)
                .Select(slice => slice!)
                .ToList();

            if (tripSlices.Count == 0) return;

            foreach (var holidayProduct in holidayFlights)
            {
                var holidaySlice = holidayProduct.Slices!.First();
                var holidayDeparture = holidaySlice.ScheduledDepartureLocal!.Value;

                var holidayOrigin = holidaySlice.Origin!.Code!;
                var holidayDestination = holidaySlice.Destination!.Code!;

                var candidateTripSlices = tripSlices
                    .Where(tripSlice => tripSlice.ScheduledDepartureLocal!.Value.Date == holidayDeparture.Date)
                    .Where(tripSlice => tripSlice.Origin?.Code.Is(holidayOrigin) == true)
                    .Where(tripSlice => tripSlice.Destination?.Code.Is(holidayDestination) == true)
                    .ToList();

                if (candidateTripSlices.Count != 1)
                    continue;

                var uniqueTripSlice = candidateTripSlices[0];

                holidayProduct.Slices = new[] { uniqueTripSlice };
                holidayProduct.Reference = tripAirlineProduct.Reference;
                holidayProduct.Passengers = tripPassengers;
                ApplyFlightSortByDateFromFirstSlice(holidayProduct);
            }
        }

        internal static IEnumerable<GraphqlTrips.Product> RemoveDuplicateProductsByReferenceAndSliceDetails(IEnumerable<GraphqlTrips.Product> products)
        {
            var uniqueProducts = new List<GraphqlTrips.Product>();
            var seenSliceProductKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var product in products)
            {
                if (product?.Slices is null || !product.Slices.Any())
                {
                    if (product is not null)
                    {
                        uniqueProducts.Add(product);
                    }

                    continue;
                }

                var productKey = BuildSliceProductKey(product);
                if (seenSliceProductKeys.Add(productKey))
                {
                    uniqueProducts.Add(product);
                }
            }

            return uniqueProducts;
        }

        private static string BuildSliceProductKey(GraphqlTrips.Product product)
        {
            var referenceKey = $"{product.Reference?.PnrLocator}|{product.Reference?.OrderId}";

            var sliceKey = string.Join(";", product.Slices!.Select(slice =>
                $"{slice?.Origin?.Code}>{slice?.Destination?.Code}>{slice?.ScheduledDepartureLocal}>{slice?.ScheduledArrivalLocal}"));

            return $"{product.BookingType}|{referenceKey}|{sliceKey}";
        }

        internal static IEnumerable<GraphqlTrips.Product> GetProductsWithSingleSlice(IEnumerable<GraphqlTrips.Product> products)
        {
            var singleSliceProducts = new List<GraphqlTrips.Product>();
            foreach (var product in products.Where(x => x != null))
            {
                if (product.Slices != null && product.Slices.Count() > 1)
                {
                    var slices = product.Slices?.ToList();
                    for (int i = 0; i < slices.Count; i++)
                    {
                        var newProduct = new GraphqlTrips.Product
                        {
                            BookingType = product.BookingType,
                            Reference = product.Reference,
                            Slices = new[] { slices[i] },
                            Passengers = product.Passengers,
                            TotalPrice = product.TotalPrice,
                            Status = product.Status,
                            CheckInEligible = product.CheckInEligible,
                            AccessibleTravelEligible = product.AccessibleTravelEligible,
                            SortByDate = slices[i].ScheduledDepartureLocal.Value.Date.AtMidnight(),
                            SortByDateInternal = slices[i].ScheduledDepartureLocal.Value,
                            SupportedFlight = product.SupportedFlight,
                            TbxProductType = product.TbxProductType,
                            TbxProductId = product.TbxProductId
                        };
                        singleSliceProducts.Add(newProduct);
                    }
                }
                else
                {
                    ApplyFlightSortByDateFromFirstSlice(product);
                    singleSliceProducts.Add(product);
                }
            }
            return singleSliceProducts;
        }

        internal static void ApplyFlightSortByDateFromFirstSlice(GraphqlTrips.Product? product)
        {
            if (product is null)
                return;

            if (!product.BookingType.Is("AIRLINE"))
                return;

            var scheduledDepartureLocal = product.Slices?.FirstOrDefault()?.ScheduledDepartureLocal;

            if (scheduledDepartureLocal is null)
                return;

            product.SortByDate = scheduledDepartureLocal.Value.Date.AtMidnight();
            product.SortByDateInternal = scheduledDepartureLocal.Value;
        }

        private static IEnumerable<GraphqlTrips.Product> GetSeparateCarProducts(IEnumerable<GraphqlTrips.Product> products)
        {
            var orderedProducts = new List<GraphqlTrips.Product>();
            foreach (var product in products.Where(x => x != null))
            {
                if (product.CarDetails != null)
                {
                    var car = product.CarDetails;
                    var carProductId = product.TbxProductId;

                    var pickupCarDetails = car.Clone();
                    pickupCarDetails.CarProductId = carProductId;
                    pickupCarDetails.EndDateLocal = null;
                    pickupCarDetails.ReturnLocations = null;
                    pickupCarDetails.SelectedReturnLocation = null;

                    var carPickupProduct = new GraphqlTrips.Product
                    {
                        BookingType = product.BookingType,
                        Reference = product.Reference,
                        CarDetails = pickupCarDetails,
                        Passengers = product.Passengers,
                        TotalPrice = product.TotalPrice,
                        Status = product.Status,
                        CheckInEligible = product.CheckInEligible,
                        AccessibleTravelEligible = product.AccessibleTravelEligible,
                        SortByDate = pickupCarDetails.StartDateLocal?.Date.At(new LocalTime(0, 0)) ?? product.SortByDate,
                        SortByDateInternal = pickupCarDetails.StartDateLocal,
                        TbxProductType = product.TbxProductType,
                        TbxProductId = product.TbxProductId
                    };

                    orderedProducts.Add(carPickupProduct);

                    // create a drop-off-only car details instance (preserve end info, remove pickup info)
                    var dropOffCarDetails = car.Clone();
                    dropOffCarDetails.CarProductId = carProductId;
                    dropOffCarDetails.StartDateLocal = null;
                    dropOffCarDetails.PickUpLocations = null;
                    dropOffCarDetails.SelectedPickupLocation = null;

                    var carDropOffProduct = new GraphqlTrips.Product
                    {
                        BookingType = product.BookingType,
                        Reference = product.Reference,
                        CarDetails = dropOffCarDetails,
                        Passengers = product.Passengers,
                        TotalPrice = product.TotalPrice,
                        Status = product.Status,
                        CheckInEligible = product.CheckInEligible,
                        AccessibleTravelEligible = product.AccessibleTravelEligible,
                        SortByDate = dropOffCarDetails.EndDateLocal?.Date.At(new LocalTime(0, 0)) ?? product.SortByDate,
                        SortByDateInternal = dropOffCarDetails.EndDateLocal,
                        TbxProductType = product.TbxProductType,
                        TbxProductId = product.TbxProductId
                    };

                    orderedProducts.Add(carDropOffProduct);
                }
                else
                {
                    orderedProducts.Add(product);
                }
            }
            return orderedProducts;
        }

        internal static IEnumerable<GraphqlTrips.Product> GetOrderedProducts(List<GraphqlTrips.Product> products)
        {
            var orderedProducts = GetProductsWithSingleSlice(products).ToList();

            orderedProducts = GetSeparateCarProducts(orderedProducts).ToList();

            // Ensure hotels are ordered after same-day flights and correct midnight check-in times.
            HotelSortByDateModifier.Apply(orderedProducts);

            // Ensure airport transfers are ordered correctly in both directions: pickup-from-airport between the flight and hotel, and drop-off-at-airport between the hotel and flight.
            TransferSortByDateModifier.Apply(orderedProducts);

            // Ensure Disney/Universal park tickets appear after the first Orlando hotel check-in when their start date is set to the holiday start.
            ParkTicketSortByDateModifier.Apply(orderedProducts);

            return orderedProducts.OrderBy(p => p.SortByDateInternal ?? p.SortByDate).ToList();
        }

        private static bool HasHolidayValidationError(IEnumerable<ErrorDetails>? errors)
        {
            return errors?.Any(e => e?.Code.Is(Constants.ErrorCodes.HolidaysApiRequestValidationErrorCode) == true) == true;
        }

        private static bool HasHolidayValidationError(string? responseContent)
        {
            if (responseContent.IsNullOrWhiteSpace())
                return false;

            try
            {
                return JToken.Parse(responseContent)
                    .SelectTokens("$..code")
                    .Any(code => code.Value<string>().Is(Constants.ErrorCodes.HolidaysApiRequestValidationErrorCode));
            }
            catch
            {
                return false;
            }
        }

        [GraphQLName("retrieveHolidayBookingDocumentDetails")]
        [GraphQLDescription("Retrieve holiday booking document details using trip resource id")]
        public async Task<HolidayDocumentDetailsResponse> RetrieveHolidayBookingDocumentDetails(HolidayDocumentDetailsRequest request, IResolverContext context)
        {
            ArgumentNullException.ThrowIfNull(request);
            if (request.TripResourceId.IsNullOrWhiteSpace())
            {
                throw new ArgumentNullException(nameof(request.TripResourceId));
            }
            try
            {
                var bookingReference = await BookingIdHelper.GetMSBookingId(request.TripResourceId);
                if (bookingReference.IsNullOrWhiteSpace())
                {
                    throw new ArgumentException(
                        "TripResourceId does not contain a valid booking reference.",
                        nameof(request.TripResourceId));
                }
                var documentListMSResponse = await _holidayProvider.RetrieveDocumentList(
                    (RetrieveDocumentRequest)bookingReference);
                var documentDisplayOrder = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
                {
                    ["TRVDOC"] = 1,
                    ["CBKINV"] = 2,
                    ["ATOLCERTIFICATE"] = 3,
                    ["ITINERARYDOCUMENT"] = 4
                };
                var latestDocuments = (documentListMSResponse?.Documents ?? [])
                    .Where(document => !string.IsNullOrWhiteSpace(document.DocumentType))
                    .GroupBy(document => document.DocumentType, StringComparer.OrdinalIgnoreCase)
                    .Select(group => group
                        .OrderByDescending(document => document.CreatedDateTime)
                        .ThenByDescending(document => document.DocumentId)
                        .First())
                    .Where(document => Constants.Environment.AllowedDocumentTypes
                        .Contains(document.DocumentType, StringComparer.OrdinalIgnoreCase))
                    .OrderBy(document =>
                        documentDisplayOrder.TryGetValue(document.DocumentType, out var displayOrder)
                            ? displayOrder
                            : int.MaxValue)
                    .ToArray();
                if (latestDocuments.Length == 0)
                {
                    throw new InvalidOperationException(
                        $"No documents were returned for trip resource id '{request.TripResourceId}'.");
                }
                var linkListRequest = (RetrieveDocumentLinkListRequest)new HolidaysDocumentListResponse
                {
                    Documents = latestDocuments
                };
                var documentLinksMSResponse = await _holidayProvider.RetrieveDocumentDownloadLinkList(
                    linkListRequest);
                var response = (HolidayDocumentDetailsResponse)(
                    new HolidaysDocumentLinkListResponse
                    {
                        HolidaysDocumentLinkResponses =
                            documentLinksMSResponse?.HolidaysDocumentLinkResponses
                    },
                    new HolidaysDocumentListResponse
                    {
                        Documents = latestDocuments
                    });
                if (response?.DocumentInformation != null)
                {
                    foreach (var document in response.DocumentInformation.Where(document => !document.DocumentUrl.IsNullOrWhiteSpace() && !document.DocumentId.IsNullOrWhiteSpace()))
                    {
                        document.DocumentToken = await HolidayDocumentTokenHelper.CreateAsync(request.TripResourceId, document.DocumentId, document.DocumentUrl);
                        document.DocumentUrl = null;
                    }

                    response.DocumentInformation = response.DocumentInformation
                        .OrderBy(document =>
                            documentDisplayOrder.TryGetValue(document.DocumentType, out var displayOrder)
                                ? displayOrder
                                : int.MaxValue)
                        .ToArray();
                }
                return response;
            }
            catch (Exception ex)
            {
                var statusCode = (ex.InnerException as HttpResponseException)?.Response?.StatusCode;

                _logger.LogError(
                    "Correlation Id - {CorrelationId} | Trip resource id - {TripResourceId} | Holiday document retrieval failed with exception type - {ExceptionType} and status code - {StatusCode}",
                    AsyncContext.Current.CorrelationId,
                    request?.TripResourceId,
                    ex.GetType().Name,
                    statusCode);

                throw new GraphQLException(ErrorBuilder.New()
                    .SetMessage("We couldn’t load your documents at the moment. Please try again later.")
                    .SetExtension("Reason", Json.Serialize(new List<ErrorDetails>()
                    {
                        new() {
                            Code = Constants.ErrorCodes.RetrieveTripDetailsErrorCode,
                            DeveloperMessage = $"Error retrieving holiday documents for trip resource id {request?.TripResourceId} | ErrorID: {AsyncContext.Current.CorrelationId} | TimeStamp: {DateTime.UtcNow}",
                            Message = $"We couldn’t load your documents at the moment. Please try again later."
                        }
                    }))
                    .SetExtension("ErrorSource", context.Operation.Name)
                    .SetCode(((int)HttpStatusCode.UnprocessableEntity).ToString())
                    .Build());
            }
        }

        [GraphQLName("retrieveHolidayBookingDocument")]
        [GraphQLDescription("Retrieve a holiday document using a validated document token")]
        public async Task<HolidayDocumentDownloadResponse> RetrieveHolidayBookingDocument(HolidayDocumentDownloadRequest request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            if (request.DocumentId.IsNullOrWhiteSpace() || request.DocumentToken.IsNullOrWhiteSpace())
            {
                throw new ArgumentException("DocumentId and DocumentToken are required.", nameof(request));
            }

            HolidayDocumentTokenContext tokenContext;
            try
            {
                tokenContext = await HolidayDocumentTokenHelper.ValidateAsync(request.DocumentToken);
            }
            catch (Exception ex) when (ex is SecurityTokenException or ArgumentException)
            {
                throw new GraphQLException(ErrorBuilder.New()
                    .SetMessage("The document token is invalid or expired.")
                    .SetCode(((int)HttpStatusCode.BadRequest).ToString())
                    .Build());
            }
            if (!tokenContext.DocumentId.Is(request.DocumentId))
            {
                throw new GraphQLException(ErrorBuilder.New()
                    .SetMessage("The requested document does not match the document token.")
                    .SetCode(((int)HttpStatusCode.BadRequest).ToString())
                    .Build());
            }

            var document = await _holidayProvider.RetrieveDocument(new RetrieveDocumentDownloadRequest
            {
                BookingId = tokenContext.BookingId,
                DocumentId = tokenContext.DocumentId,
                DocumentUrl = tokenContext.DocumentUrl
            }, cancellationToken);

            if (document?.Document is null)
            {
                throw new GraphQLException(ErrorBuilder.New()
                    .SetMessage("The holiday document could not be found.")
                    .SetCode(((int)HttpStatusCode.NotFound).ToString())
                    .Build());
            }

            return new HolidayDocumentDownloadResponse { DocumentId = tokenContext.DocumentId, Document = document.Document };
        }
    }
}

