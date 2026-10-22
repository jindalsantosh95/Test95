using HotChocolate;
using HotChocolate.Types;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Web;
using VAA.EAPI.GraphQL.Common;
using VAA.EAPI.GraphQL.Common.Extensions;
using VAA.EAPI.GraphQL.Common.Helper;
using VAA.EAPI.GraphQL.Common.Serialisation;
using VAA.EAPI.GraphQL.Domain.Trips.Helper;
using VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Models.BasketMS;
using VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Models.PaymentMS.PaymentDeails;
using VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Models.SeatMS;
using VAA.EAPI.GraphQL.Domain.Trips.Interface;
using VAA.EAPI.GraphQL.Domain.Trips.Models.Enum;
using VAA.EAPI.GraphQL.Domain.Trips.Models.ReferenceData;
using VAA.EAPI.GraphQL.Domain.Trips.Models.Seats;

namespace VAA.EAPI.GraphQL.Domain.Trips.Mutations
{
    [ExtendObjectType(OperationTypeNames.Mutation)]
    public class SeatConfirmationMutation
    {
        private readonly IBasketProvider _basketServiceProvider;
        private readonly ISeatProvider _seatServiceProvider;
        private readonly ILogger<SeatConfirmationMutation> _logger;
        private readonly IPaymentServiceProvider _paymentServiceProvider;
        private readonly IReferenceServiceProvider _referenceServiceProvider;
        private readonly ISeatUpgradeHandler _seatUpgradeHandler;

        public SeatConfirmationMutation(IBasketProvider basketServiceProvider, ISeatProvider seatServiceProvider, ILogger<SeatConfirmationMutation> logger, IPaymentServiceProvider paymentServiceProvider, IReferenceServiceProvider referenceServiceProvider, ISeatUpgradeHandler seatUpgradeHandler)
        {
            _basketServiceProvider = basketServiceProvider ?? throw new ArgumentNullException(nameof(basketServiceProvider));
            _seatServiceProvider = seatServiceProvider ?? throw new ArgumentNullException(nameof(seatServiceProvider));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _paymentServiceProvider = paymentServiceProvider ?? throw new ArgumentNullException(nameof(paymentServiceProvider));
            _referenceServiceProvider = referenceServiceProvider ?? throw new ArgumentNullException(nameof(referenceServiceProvider));
            _seatUpgradeHandler = seatUpgradeHandler ?? throw new ArgumentNullException(nameof(seatUpgradeHandler));
        }


        [GraphQLName("seatConfirmationMutation")]
        [GraphQLDescription("Select seat per passenger post sale for requested segment")]
        public async Task<SeatConfirmationResponse> ConfirmSeats(string tripReservationId)
        {
            try
            {                
                ArgumentNullException.ThrowIfNull(tripReservationId);

                var seatConfirmationResponse = new SeatConfirmationResponse();

                //get basketId and retrieve
                var basketId = await BookingIdHelper.GetBasketId(tripReservationId);

                var basket = await _basketServiceProvider.RetrieveBasketAsync(basketId);

                IEnumerable<CabinRefData> cabinDetails = [];
                IEnumerable<AirlineRefData> airlineRefData = [];
                                
                Parallel.Invoke(
                    new Action(() => cabinDetails = _referenceServiceProvider.RetrieveCabinDataAsync().GetAwaiter().GetResult()),
                    new Action(() => airlineRefData = _referenceServiceProvider.RetrieveAirlineDetails().GetAwaiter().GetResult())
                );

                if (!basket.BookingDetails.IsNullOrDefault())
                {
                    //Get Current SeatDetails from basket
                    //passengerId, SeatNumber, segmentId - LISTA
                    var currentSeatDetails = basket.BookingDetails.SeatDetails.Distinct();

                    //Get FlightPassengerSeatMap from basket for new seat requests
                    //another list of passengerId, seatnumber, segmentId - LISTB
                    //Create a Add request for all the LISTB where seatNumber is not empty - ADDLIST
                    var requestSeats = basket.FlightPassengerSeatMap.Where(f => !f.SeatNumber.IsNullOrEmpty());

                    //Look for passengerid and segmentid from ADDLIST in the LISTA
                    var deleteList = new List<SeatAssignment>();
                    var addList = new List<Infrastructure.Models.BasketMS.FlightPassengerSeatMap>();
                    foreach (var requestSeat in requestSeats)
                    {
                        var existingSeat = currentSeatDetails.FirstOrDefault(s => s.PassengerId == requestSeat.PassengerId && s.SegmentId == requestSeat.SectorId);
                        if (existingSeat != null)
                        {
                            // If found, compare seat number
                            if (existingSeat.SeatNumber != requestSeat.SeatNumber)
                            {
                                // If seat number does not match, add to delete list
                                deleteList.Add(existingSeat);
                                addList.Add(requestSeat);
                            }
                        }
                        else
                        {
                            // If not found, add to add list
                            addList.Add(requestSeat);
                        }
                    }

                    PaymentTransactionDetails paymentDetails = null;
                    if (addList.Sum(seat => seat.Price.Amount) > 0.0m)
                    {
                        paymentDetails = await GetPaymentDetailsAsync(basket) ?? throw new Exception("Payment details not found for the given payment reference id"); ;
                    }

                    #region GetSeatsInformation

                    var seatOfferIds = addList.Select(x => x.SeatOfferId).Distinct();

                    ConcurrentBag<SeatResponse> seatMSResponse = new();

                    if (!seatOfferIds.IsNullOrEmpty())
                    {
                        await Parallel.ForEachAsync(seatOfferIds, async (id, CancellationToken) => seatMSResponse.Add(await _seatServiceProvider.RetrieveSeatOfferDetailsAsync(id)));
                    }
                    #endregion

                    //Validate the basket for seat upgrades if any
                    if (!await _seatUpgradeHandler.IsSeatUpgradeConfirmationValid(basket, seatMSResponse))
                    {
                        seatConfirmationResponse = (SeatConfirmationResponse)(basket, addList, null, seatMSResponse, cabinDetails, airlineRefData);
                        await _seatUpgradeHandler.AddErrorForInvalidUpgradeBasket(basket);
                    }


                    //Create SeatFulfilment Request basis ADDLIST
                    var seatFulfilmentRequest = (SeatFulfilmentRequest)(basket, addList, seatMSResponse, paymentDetails);

                    //Call DELETELIST - Handle failures gracefully as this is not a mandatory step
                    #region Delete Seats
                    if (ConfigReader.Try<bool>(Common.Constants.Environment.EnableSeatDeletion, true))
                    {
                        try
                        {
                            if (!deleteList.IsNullOrEmpty())
                            {
                                var seatDeletionResponse = await _seatServiceProvider.DeleteSeatAsync((SeatDeletionRequest)(basket, deleteList));
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, $"Correlation Id - {AsyncContext.Current.CorrelationId} - Channel - {string.Join(',', AsyncContext.Current.Channel)} No Explicit Seat Deletion Performed - {ex}");
                        }
                    }

                    #endregion

                    var seatFulfilmentResponse = await _seatServiceProvider.SeatFulfilmentAsync(seatFulfilmentRequest);

                  
                    //Delete Basket if seat fulfilment does not fail
                    if (!seatFulfilmentResponse.OrderItems.IsNullOrEmpty() || seatFulfilmentResponse.Error.Any(err => err.Message == "INCOMPLETESEATADDRESPONSE"))
                    {
                        seatConfirmationResponse = (SeatConfirmationResponse)(basket, addList, seatFulfilmentResponse, seatMSResponse, cabinDetails, airlineRefData);
                        await _basketServiceProvider.DeleteBasketAsync(basket.BasketId.ToString());
                    }                    
                    else
                        throw new Exception(string.Join(':', seatFulfilmentResponse?.Error.Select(a => a.Message)));


                    if (seatFulfilmentResponse?.Error?.IsNullOrEmpty() == false)
                    {
                        _logger.LogError($"Correlation Id - {AsyncContext.Current.CorrelationId} - Channel - {string.Join(',', AsyncContext.Current.Channel)} Seat Fulfilment Errors for PNR {basket?.BookingDetails?.PnrLocator} - {Json.Serialize(seatFulfilmentResponse.Error)}");                       
                    }
                }

                return seatConfirmationResponse;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, $"Correlation Id - {AsyncContext.Current.CorrelationId} - Channel - {string.Join(',', AsyncContext.Current.Channel)} Seat Fulfilment Failed For {tripReservationId} Exception - {ErrorHelpers.AggregateHttpConnectorException(ex)}");
                throw new Exception(Constants.ErrorCodes.SelectSeatErrorCode, ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Correlation Id - {AsyncContext.Current.CorrelationId} - Channel - {string.Join(',', AsyncContext.Current.Channel)} Seat Fulfilment Failed For {tripReservationId} Exception - {ex}");
                throw new Exception(Constants.ErrorCodes.SelectSeatErrorCode, ex);
            }

        }

        private async Task<PaymentTransactionDetails> GetPaymentDetailsAsync(Basket basket)
        {
            //check if paid seat                   
            string paymentReferenceId = null;
            string deltaHPPPaymentLink = basket.PaymentSessions?.First().PaymentLink;

            if (!deltaHPPPaymentLink.IsNullOrEmpty() && basket.PaymentSessions?.First().HppPaymentStatus == "SUCCESS")
            {
                var deltaHPPPaymentUri = new Uri(deltaHPPPaymentLink);
                paymentReferenceId = HttpUtility.ParseQueryString(deltaHPPPaymentUri.Query).Get("paymentReferenceId");
                return await _paymentServiceProvider.PaymentDetailsAsync(paymentReferenceId, PaymentType.Card.ToDisplayName());
            }
            throw new NotImplementedException("Payment Not Found");
        }
    }
}
