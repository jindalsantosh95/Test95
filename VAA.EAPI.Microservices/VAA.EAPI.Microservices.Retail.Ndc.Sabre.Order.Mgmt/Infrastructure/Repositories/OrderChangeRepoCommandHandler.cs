using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Infrastructure.NDC.Sabre.Proxy.EnhancedAirBook;
using VAA.EAPI.Infrastructure.NDC.Sabre.Proxy.EnhancedAirTicket;
using VAA.EAPI.Infrastructure.NDC.Sabre.Proxy.EnhancedEndTransaction;
using VAA.EAPI.Infrastructure.NDC.Sabre.Proxy.GetElectronicDocument;
using VAA.EAPI.Infrastructure.NDC.Sabre.Proxy.GetReservation;
using VAA.EAPI.Infrastructure.NDC.Sabre.Repositories;
using VAA.EAPI.Microservices.Retail.Ndc.Sabre.Order.Mgmt.Domain.OrderChange;
using VAA.EAPI.Microservices.Retail.Ndc.Sabre.Order.Mgmt.Infrastructure.Mapper;
using VAA.EAPI.Microservices.Retail.Ndc.Sabre.Order.Mgmt.Mapper.OrderChange;

namespace VAA.EAPI.Microservices.Retail.Ndc.Sabre.Order.Mgmt.Infrastructure.Repositories
{
    public class OrderChangeRepoCommandHandler(IMediator mediator, ILogger<OrderChangeRepoCommandHandler> logger, SessionCloseCommandHandler sessionCloseCommandHandler, SabreCommandLLSCommandHandler sabreCommandLLSCommandHandler) : IRequestHandler<OrderChangeRepoCommand, Result<Response<OrderDocument>>>
    {
        private readonly IMediator _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        private readonly ILogger<OrderChangeRepoCommandHandler> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        private readonly SessionCloseCommandHandler _sessionCloseCommandHandler = sessionCloseCommandHandler ?? throw new ArgumentNullException(nameof(sessionCloseCommandHandler));
        private readonly SabreCommandLLSCommandHandler _sabreCommandLLSCommandHandler = sabreCommandLLSCommandHandler ?? throw new ArgumentNullException(nameof(sabreCommandLLSCommandHandler));
       
        public async Task<Result<Response<OrderDocument>>> Handle(OrderChangeRepoCommand request, CancellationToken cancellationToken)
        {
            bool isSuccessful = false;
            int retryCount = ConfigReader.Get<int>(Constants.Environment.RetryCount);
            int maxRetries = ConfigReader.Get<int>(Constants.Environment.MaxRetriesCount);
            var keyValuePairs = new Dictionary<string, string>();
            try
            {
                int responseDelayInMS = ConfigReader.Get<int>(Constants.Environment.ResponseDelayForOrderChangeTimeInMS);
                var delay = TimeSpan.FromMilliseconds(responseDelayInMS);
                bool isPastFlownSegments = false;

                var pcc = request.orderChangeRequestData.Parties.Participants.First().PccCode;
                var token = await CreateSession(pcc,cancellationToken);
                var initialReservation = await GetReservation(token, request.orderChangeRequestData.Rloc,pcc, cancellationToken);

                var requestData = request.orderChangeRequestData;
                bool isPartialFlown = requestData.AccountableDocumentRefs.Any(x => x.Coupons.Any(c => c.CouponStatus == "USED"));
                var flownSegmentId = requestData.AccountableDocumentRefs.SelectMany(acc => acc.Coupons).Where(x => x.CouponStatus == "USED").Select(x => x.SegmentRefId);

                if (isPartialFlown)
                {
                    var initialGetReservationAirSegments = (initialReservation.Item as ReservationPNRB).PassengerReservation.Segments.Segment.Where(x => x.Item is SegmentTypePNRBSegmentAir).Select(x => x.Item as SegmentTypePNRBSegmentAir);

                    var reservationFlownSegments = initialGetReservationAirSegments.First(reservationSeg => requestData.DataList.SegmentDetails.Where(x => flownSegmentId.Any(segId => segId.Contains(x.SegmentId))).Any(seg =>
                                                              seg.OriginAirportCode == reservationSeg.DepartureAirport &&
                                                              seg.DestinationAirportCode == reservationSeg.ArrivalAirport &&
                                                              string.Equals(seg.MarketingFlightNumber?.TrimStart('0'), reservationSeg.MarketingFlightNumber?.TrimStart('0'), StringComparison.OrdinalIgnoreCase) &&
                                                              seg.ClassOfService == reservationSeg.ClassOfService &&
                                                              Convert.ToDateTime(seg.ScheduledDepartureTime.LocalDateTime) == Convert.ToDateTime(reservationSeg.DepartureDateTime)
                                                              ));

                    isPastFlownSegments = reservationFlownSegments.isPast;
                }

                if (isPartialFlown && !isPastFlownSegments)
                {
                    await _sessionCloseCommandHandler.Handle(new SessionCloseCommand() { BinaryToken = token, PCC = pcc }, cancellationToken);
                    return Result.Failure<Response<OrderDocument>>(Json.Serialize(ErrorMapper.MapTechnicalExceptionToEAPIException(Constants.ErrorCodes.PartialFlowIsPastIndicator, Guid.NewGuid().ToString(), EAPI.Infrastructure.Constants.Constants.ErrorCodesPath)));
                }

                bool corporateAccountDetails = request?.orderChangeRequestData?.Parties?.CorporateAccount != null;
                bool hasInfant = request.orderChangeRequestData.DataList.CustomerDetails.Any(p => p.Ptc == Constants.Application.Infant);

                await CancelFlightItems(token, request, cancellationToken);

                var enhancedAirBookRs = await BookFlightItems(token, request, cancellationToken);
                
                if(enhancedAirBookRs.EnhancedAirBookRS?.ApplicationResults?.Warning != null)
                {
                    var warningMessages = enhancedAirBookRs.EnhancedAirBookRS.ApplicationResults.Warning.SelectMany(x => x.SystemSpecificResults).SelectMany(x => x.Message).Select(x => x.Value);      
                    if (warningMessages.Any(msg => msg.Contains("FLIGHT NOOP")))
                    {
                        await _sessionCloseCommandHandler.Handle(new SessionCloseCommand() { BinaryToken = token, PCC = pcc }, cancellationToken);
                        return Result.Failure<Response<OrderDocument>>(Json.Serialize(ErrorMapper.MapTechnicalExceptionToEAPIException(Constants.ErrorCodes.FlightNoopError, AsyncContext.Current.CorrelationId.ToString(), EAPI.Infrastructure.Constants.Constants.ErrorCodesPath)));
                    }
                }

                bool actionCode = enhancedAirBookRs.EnhancedAirBookRS.TravelItineraryReadRS.TravelItinerary.ItineraryInfo.ReservationItems.Where(x => x.FlightSegment != null).SelectMany(x => x.FlightSegment).Any(y => y.Status == "UC");

                if (actionCode)
                {
                    await _sessionCloseCommandHandler.Handle(new SessionCloseCommand() { BinaryToken = token, PCC = pcc }, cancellationToken);
                    return Result.Failure<Response<OrderDocument>>(Json.Serialize(ErrorMapper.MapTechnicalExceptionToEAPIException(Constants.ErrorCodes.InValidSegmentError, Guid.NewGuid().ToString(), EAPI.Infrastructure.Constants.Constants.ErrorCodesPath)));
                }

                var enhancedEndTransactionRS = await EndTransaction(token,pcc, cancellationToken, delay);

                if (enhancedEndTransactionRS.ApplicationResults?.Warning != null)
                {
                    var warningMessages = enhancedEndTransactionRS.ApplicationResults.Warning.SelectMany(x => x.SystemSpecificResults).SelectMany(x => x.Message).Select(x => x.Value);
                    if (warningMessages.Any(msg => msg.Contains("IM AND CANCEL UNABLE SEGMENTS")))
                    {
                        await _sessionCloseCommandHandler.Handle(new SessionCloseCommand() { BinaryToken = token, PCC = pcc }, cancellationToken);
                        return Result.Failure<Response<OrderDocument>>(Json.Serialize(ErrorMapper.MapTechnicalExceptionToEAPIException(Constants.ErrorCodes.IAmCancelError, AsyncContext.Current.CorrelationId.ToString(), EAPI.Infrastructure.Constants.Constants.ErrorCodesPath)));
                    }
                }

                var initialReservationPassengers = (initialReservation.Item as ReservationPNRB).PassengerReservation.Passengers.Passenger;

                if (hasInfant || corporateAccountDetails || initialReservationPassengers.Any(p => p.SpecialRequests?.SpecialMealRequest != null) || initialReservationPassengers.Any(p => p.SpecialRequests?.WheelchairRequest != null) || initialReservationPassengers.Any(p => p.SpecialRequests?.GenericSpecialRequest != null))
                {
                    await UpdatePnr(token, request, initialReservation, cancellationToken, false, corporateAccountDetails);
                }

                while (!isSuccessful && retryCount < maxRetries)
                {
                    var getReservationRs = await GetReservation(token, request.orderChangeRequestData.Rloc, pcc,cancellationToken);

                    if (!isPartialFlown)
                    {
                        isSuccessful = await GetPriceQuoteKeys(keyValuePairs, token, request, getReservationRs, cancellationToken);
                        if (isSuccessful)
                        {
                            await ProcessPassengers(token, request, keyValuePairs, getReservationRs, isPartialFlown, cancellationToken);

                            var enhancedEndTransactionRS1 = await EndTransaction(token, pcc, cancellationToken, delay);
                            if (enhancedEndTransactionRS1.ApplicationResults?.Error == null) break;
                        }
                    }
                    else
                    {
                        await ProcessPassengers(token, request, keyValuePairs, getReservationRs, isPartialFlown, cancellationToken);

                        var enhancedEndTransactionRS1 = await EndTransaction(token, pcc, cancellationToken, delay);
                        if (enhancedEndTransactionRS1.ApplicationResults?.Error == null) break;
                    }

                    retryCount++;
                    keyValuePairs.Clear();
                }

                var getReservationRS = await GetReservation(token, request.orderChangeRequestData.Rloc, pcc, cancellationToken, isPartialFlown);

                bool invalidAir4Pnr = ((getReservationRS.Item as ReservationPNRB).PassengerReservation.Segments.Segment.Any(x =>
                {
                    var airSeg = x.Item as SegmentTypePNRBSegmentAir;
                    return airSeg?.AirlineRefId?.Equals("DCVS*", StringComparison.OrdinalIgnoreCase) == true;
                }));

                if (invalidAir4Pnr)
                {
                    await _sessionCloseCommandHandler.Handle(new SessionCloseCommand() { BinaryToken = token, PCC = pcc }, cancellationToken);
                    return Result.Failure<Response<OrderDocument>>(Json.Serialize(ErrorMapper.MapTechnicalExceptionToEAPIException(Constants.ErrorCodes.MissingAir4PNR, AsyncContext.Current.CorrelationId.ToString(), EAPI.Infrastructure.Constants.Constants.ErrorCodesPath)));
                }

                var interLineOperatingSegments = ConfigReader.Get(Constants.Environment.InterLineOperatingSegments);
                var getReservationAirSegments = (getReservationRS.Item as ReservationPNRB).PassengerReservation.Segments.Segment.Where(x => x.Item is SegmentTypePNRBSegmentAir).Select(x => x.Item as SegmentTypePNRBSegmentAir);
                if(getReservationAirSegments.Any(seg => interLineOperatingSegments.Split(",").Contains(seg.MarketingAirlineCode)))
                {
                    await UpdatePnr(token, request, getReservationRS, cancellationToken, true);
                }

                var airTickets = await TicketFlightItems(token, request, getReservationRS, pcc,cancellationToken);

                var orderDocument = OrderDocumentMapper.Mapper(request.orderChangeRequestData, airTickets, getReservationRS);

                await GetElectronicDocuments(token, orderDocument, cancellationToken);

                return await SuccessResult(orderDocument, token, pcc, cancellationToken);
            }
            catch (Exception)
            {
                _logger.LogError("Order change Sabre workflow failed");
                return Result.Failure<Response<OrderDocument>>(Json.Serialize(ErrorMapper.MapTechnicalExceptionToEAPIException(EAPI.Infrastructure.Constants.Constants.TechnicalError, Guid.NewGuid().ToString(), EAPI.Infrastructure.Constants.Constants.GlobalExceptionFilePath)));
            }
        }

        private async Task GetElectronicDocuments(string token, OrderDocument orderDocument, CancellationToken cancellationToken)
        {
            foreach (var ticket in orderDocument.OrderItems.Where(oi => oi.Services.First().Product != null && oi.Services.First().Product.ProductType == EAPI.Domain.Virgin.Models.Common.Product.ProductTypes.FlightEnum).SelectMany(x => x.AccountableDocumentRefs).DistinctBy(x => x.DocumentNumber))
            {
                var couponResponse = await _mediator.Send(new GetElectronicDocumentQuery() { GetElectronicDocumentRQRequest = GetElectronicDocumentRequestMapper.GetElectronicDocumentMapper(token, ticket) }, cancellationToken);
                if (couponResponse?.DocumentDetailsDisplay != null)
                {
                    orderDocument.OrderItems = orderDocument.OrderItems.Where(oi => oi.Services.First().Product != null && oi.Services.First().Product.ProductType == EAPI.Domain.Virgin.Models.Common.Product.ProductTypes.FlightEnum).Select(orderItem =>
                    {
                        var accountableDocumentRefsList = orderItem.AccountableDocumentRefs.ToList();
                        for (int i = 0; i < accountableDocumentRefsList.Count; i++)
                        {
                            if (accountableDocumentRefsList[i].DocumentNumber == ticket.DocumentNumber && ((TicketingDocumentTicketED)couponResponse.DocumentDetailsDisplay.Item).RelatedDocument != null)
                            {
                                accountableDocumentRefsList[i] = IssueTicketDataMapper.AccountableDocumentsMapper(ticket, couponResponse, orderDocument);
                                break;
                            }
                        }
                        orderItem.AccountableDocumentRefs = accountableDocumentRefsList;
                        return orderItem;
                    });
                }
            }
        }

        private async Task<string> CreateSession(string pcc,CancellationToken cancellationToken)
        {
            var token = await _mediator.Send(new CreateSessionCommand() { PCC = pcc}, cancellationToken);
            if (token is null)
            {
                await FailureResult($"{Constants.Application.ErrorMsg} SessionCreateRQ", pcc);
            }

            return token;
        }

        private async Task<GetReservationRS> GetReservation(string token, string rloc, string pcc, CancellationToken cancellationToken, bool isPartialFlown = false)
        {
            var getReservationRQ = GetReservationRequestMapper.GetReservationRequest(token, rloc, isPartialFlown);
            var response = await _mediator.Send(new GetReservationQuery(getReservationRQ), cancellationToken);

            if (response == null || response.Errors != null)
            {
                var errors = string.Join(Environment.NewLine, response.Errors.Select(x => x.Message));
                await FailureResult(errors, pcc,token,cancellationToken);
            }

            return response;
        }

        private async Task CancelFlightItems(string token, OrderChangeRepoCommand request, CancellationToken cancellationToken)
        {
            var oTACancelLLSRQ = OTACancelLLSRequestMapper.GetOTACancelRequest(token, request.orderChangeRequestData.OrderAmendments.First().FlightItems, null);
            var oTACancelLLSRS = await _mediator.Send(new OTACancelLLSCommand() { OtaCancelRequest = oTACancelLLSRQ }, cancellationToken);

            if (oTACancelLLSRS?.OTA_CancelRS.ApplicationResults?.Error != null)
            {
                _logger.LogError("Sabre OTACancelLLS returned an error response");

                var errors = string.Join(Environment.NewLine, oTACancelLLSRS.OTA_CancelRS.ApplicationResults?.Error.SelectMany(x => x.SystemSpecificResults).SelectMany(x => x.Message).Select(x => x.Value));
                await FailureResult(errors, request.orderChangeRequestData.Parties.Participants.First().PccCode, token, cancellationToken);
            }
        }

        private async Task<EnhancedAirBookRQResponse> BookFlightItems(string token, OrderChangeRepoCommand request, CancellationToken cancellationToken)
        {
            var enhancedAirBookRQ = EnhancedAirBookRequestMapper.GetEnhancedAirBookRequest(token, request.orderChangeRequestData, request.orderChangeRequestData.DataList.CustomerDetails);
            var enhancedAirBookRS = await _mediator.Send(new EnhancedAirBookCommand() { EnhancedAirBookRequest = enhancedAirBookRQ }, cancellationToken);

            if (enhancedAirBookRS == null || enhancedAirBookRS.EnhancedAirBookRS.ApplicationResults?.Error != null)
            {
                _logger.LogError("Sabre EnhancedAirBook returned an error response");

                var errors = string.Join(Environment.NewLine, enhancedAirBookRS.EnhancedAirBookRS.ApplicationResults?.Error.SelectMany(x => x.SystemSpecificResults).SelectMany(x => x.Message).Select(x => x.Value));

                await FailureResult(errors, request.orderChangeRequestData.Parties.Participants.First().PccCode, token, cancellationToken);
            }

            return enhancedAirBookRS;
        }

        private async Task UpdatePnr(string token, OrderChangeRepoCommand request, GetReservationRS initialGetReservation, CancellationToken cancellationToken, [Optional] bool isInterLineSegments , [Optional] bool corporateAccountDetails)
        {
            var updatePNRRq = UpdatePassengerNameRecordMapper.UpdatePnrReqMapper(token, request.orderChangeRequestData, initialGetReservation.Item as ReservationPNRB, isInterLineSegments,corporateAccountDetails);

            if ((isInterLineSegments && updatePNRRq.UpdatePassengerNameRecordRQ.SpecialReqDetails.SpecialService.SpecialServiceInfo?.SecureFlight?.Length != 0) || updatePNRRq.UpdatePassengerNameRecordRQ.SpecialReqDetails.SpecialService.SpecialServiceInfo.Service.Length != 0)
            {
                var updatePNRRs = await _mediator.Send(new UpdatePassengerNameRecordCommand() { UpdatePassengerNameRecordRQRequest = updatePNRRq }, cancellationToken);

                if (updatePNRRs == null || updatePNRRs.UpdatePassengerNameRecordRS.ApplicationResults?.Error != null)
                {
                    _logger.LogError("Sabre UpdatePassengerNameRecord returned an error response");

                    var errors = string.Join(Environment.NewLine, updatePNRRs.UpdatePassengerNameRecordRS.ApplicationResults?.Error.SelectMany(x => x.SystemSpecificResults).SelectMany(x => x.Message).Select(x => x.Value));
                    await FailureResult(errors, request.orderChangeRequestData.Parties.Participants.First().PccCode, token, cancellationToken);
                }
            }
        }

        private async Task<EnhancedEndTransactionRS> EndTransaction(string token, string pcc ,CancellationToken cancellationToken, TimeSpan delay)
        {
            await Task.Delay(delay, cancellationToken);

            var enhancedEndTransactionRQ = EnhancedEndTransactionRequestMapper.GetEnhancedEndTransactionRequest(token, true);
            var response = await _mediator.Send(new EnhancedEndTransactionCommand() { EnhancedEndTransactionRQ = enhancedEndTransactionRQ }, cancellationToken);
            if (response == null || response.ApplicationResults.Error != null)
            {
                _logger.LogError("Sabre EndTransaction returned an error response");

                var errors = string.Join(Environment.NewLine, response.ApplicationResults.Error.SelectMany(x => x.SystemSpecificResults).SelectMany(x => x.Message).Select(x => x.Value));
                if (response.ApplicationResults.Warning != null)
                {
                    errors += string.Join(Environment.NewLine, response.ApplicationResults.Warning.SelectMany(x => x.SystemSpecificResults).SelectMany(x => x.Message).Select(x => x.Value));
                }

                if (errors.Contains("SIMULTANEOUS CHANGES TO PNR"))
                {
                    await IgnoreTransaction(token, pcc ,cancellationToken);
                    return response;
                }
                if (errors.Contains("IM AND CANCEL UNABLE SEGMENTS")) { return response; }

                await FailureResult(errors, pcc, token, cancellationToken);
            }

            await Task.Delay(delay, cancellationToken);

            return response;
        }

        private async Task<bool> GetPriceQuoteKeys(Dictionary<string, string> keyValuePairs, string token, OrderChangeRepoCommand request, GetReservationRS getReservationRS2, CancellationToken cancellationToken)
        {
            var passengers = (getReservationRS2.Item as ReservationPNRB).PassengerReservation.Passengers.Passenger;

            foreach (var ptcType in request.orderChangeRequestData.DataList.CustomerDetails.Select((x, i) => new { Value = x.Ptc, Index = i + 1 }).GroupBy(x => x.Value))
            {
                var manualPriceQuoteAddInfoCreateRQ = ManualPriceQuoteAddInfoRQMapper.ManualPriceQuoteAddInfoRQRequest(token, false, request.orderChangeRequestData, ptcType.Key, ptcType.First().Index.ToString(), passengers);
                var manualPriceQuoteAddInfoCreateRS = await _mediator.Send(new ManualPriceQuoteAddInfoCommand() { ManualPriceQuoteRequest = manualPriceQuoteAddInfoCreateRQ }, cancellationToken);

                if (manualPriceQuoteAddInfoCreateRS != null && manualPriceQuoteAddInfoCreateRS.ApplicationResults?.Error == null)
                {
                    var pqValue = manualPriceQuoteAddInfoCreateRS.Text.First().Split(" ")[1];
                    var manualPriceQuoteAddInfoContentRQ = ManualPriceQuoteAddInfoRQMapper.ManualPriceQuoteAddInfoRQRequest(token, true, request.orderChangeRequestData, ptcType.Key, pqValue, passengers);
                    var response = await _mediator.Send(new ManualPriceQuoteAddInfoCommand() { ManualPriceQuoteRequest = manualPriceQuoteAddInfoContentRQ }, cancellationToken);
                    if (response == null || response.ApplicationResults.Error != null)
                    {
                       _logger.LogError("Sabre ManualPriceQuoteAddInfoContent returned an error response");

                        var errors = $" - Content - {string.Join(Environment.NewLine, response.ApplicationResults.Error.SelectMany(x => x.SystemSpecificResults).SelectMany(x => x.Message).Select(x => x.Value))}";
                        await FailureResult(errors, request.orderChangeRequestData.Parties.Participants.First().PccCode, token, cancellationToken);
                    }
                    else
                    {
                        if (!keyValuePairs.ContainsKey(ptcType.Key))
                            keyValuePairs.Add(ptcType.Key, manualPriceQuoteAddInfoCreateRS.Text[0].Substring(3, 1));
                    }

                }
                else
                {
                    _logger.LogError("Sabre ManualPriceQuoteAddInfoCreate returned an error response");

                    var errors = string.Join(Environment.NewLine, manualPriceQuoteAddInfoCreateRS.ApplicationResults.Error.SelectMany(x => x.SystemSpecificResults).SelectMany(x => x.Message).Select(x => x.Value));
                    if (manualPriceQuoteAddInfoCreateRS.ApplicationResults.Warning != null)
                    {
                        errors += string.Join(Environment.NewLine, manualPriceQuoteAddInfoCreateRS.ApplicationResults.Warning.SelectMany(x => x.SystemSpecificResults).SelectMany(x => x.Message).Select(x => x.Value));
                    }

                    if (errors.Contains("IGN AND RETRY"))
                    {
                        await IgnoreTransaction(token, request.orderChangeRequestData.Parties.Participants.First().PccCode, cancellationToken);
                        return false;
                    }
                    await FailureResult(errors, request.orderChangeRequestData.Parties.Participants.First().PccCode, token, cancellationToken);
                }
            }
            return true;
        }

        private async Task ProcessPassengers(string token, OrderChangeRepoCommand request, Dictionary<string, string> keyValuePairs, GetReservationRS getReservationRS, bool isPartialFlown, CancellationToken cancellationToken)
        {
            if (isPartialFlown)
            {
                for (int i = 1; i <= request.orderChangeRequestData.DataList.CustomerDetails.Count(); i++)
                {
                    var pax = request.orderChangeRequestData.DataList.CustomerDetails.ElementAt(i - 1);
                    var offeritem = request.orderChangeRequestData.SelectedOffers.First().OfferItems.First(offerItem => offerItem.Services.Any(service => service.ServiceBeneficiaries.Contains(pax.CustId)));
                    var addCollectPrice = offeritem?.OfferitemPricing?.FirstOrDefault()?.AddCollectPrice?.Amount?.EquivalentMonetaryAmount ?? offeritem?.OfferitemPricing?.FirstOrDefault()?.AddCollectPrice?.Amount?.MonetaryAmount;
                    var nameId = $"{pax.LastNameNumber}.{pax.FirstNameNumber}";

                    var inBoundConnectionFlightSegments = request.orderChangeRequestData.OrderAmendments.SelectMany(amendment => amendment.FlightItems).Where(f => f.ActionContext == FlightItems.ActionContexts.Add).SelectMany(f => f.Segments);
                    var baggageRefIds = offeritem.Services.First(x => inBoundConnectionFlightSegments.Any(seg => x.FlightSegmentsRefIds.Contains(seg.SegmentId))).BaggageRefIds;
                    var carryOnBaggage = request.orderChangeRequestData.DataList.BaggageAllowances.Where(x => x.BaggageAllowanceTypeCode == "CarryOn" && baggageRefIds.Any(bag => bag.Contains(x.BaggageAllowanceId)));
                    var carryOnBaggageCounts = carryOnBaggage.Select(x => $"{x.FreeBaggageAllowance.AllowedBagCnt:D2}P");
                    var fareCaluculationText = request.orderChangeRequestData.SelectedOffers.First().OfferItems.Where(x => x.Services.Any(srv => srv.ServiceBeneficiaries.Contains(pax.CustId))).Select(x => x.OfferitemPricing.First().FareCalculationText).First();
                    var newPriceApplicableTax = offeritem.OfferitemPricing.FirstOrDefault()?.SalePrice.First(x => x.PriceComponentType == EAPI.Domain.Virgin.Models.Common.Price.PriceComponentTypes.TotalTaxEnum).TaxBreakup;
                    List<(string Amount, string FareBasis)> fareBasisandAmount = new List<(string, string)>();

                    if (fareCaluculationText != null)
                    {
                        string[] fareSplits = fareCaluculationText.Split("VS");

                        foreach (var split in fareSplits)
                        {
                            // Regex: capture number (with decimals) followed by letters/numbers
                            var match = RegexCompiled.Match(split, @"(\d+(?:\.\d+)?)([A-Z]{2,}[A-Z0-9]*)");
                            if (match.Success)
                            {
                                string amount = match.Groups[1].Value;
                                string code = match.Groups[2].Value;

                                if (!RegexCompiled.IsMatch(code, @"^[A-Z][A-Z0-9]{3,}$"))
                                    continue;

                                fareBasisandAmount.Add((amount, code));
                            }
                        }
                    }

                    var ticketDetails = ((ReservationPNRB)getReservationRS.Item).PassengerReservation.TicketingInfo?.TicketDetails;
                    var ticketNumber = ticketDetails?.Where(t => t.PassengerName?.nameId == nameId && t.Status?.ToUpper() == "ACTIVE");

                    var changeFeeAmount = (from pen in request.orderChangeRequestData.DataList.Penalty.Where(x => x.Type == "Change" && x.Text.Equals("Change Penalty"))
                                           from offer in request.orderChangeRequestData.SelectedOffers
                                           from offerItem in offer.OfferItems
                                           from service in offerItem.Services
                                           where service.PenaltyRef.ChangePenaltyRefIds != null && service.PenaltyRef.ChangePenaltyRefIds.Any() && service.PenaltyRef.ChangePenaltyRefIds.Contains(pen.Id) && service.ServiceBeneficiaries.Contains(pax.CustId)
                                           select pen)?.FirstOrDefault()?.MaxPrice?.Amount is var amt && amt != null ? (!string.IsNullOrEmpty(amt.EquivalentMonetaryAmount) ? amt?.EquivalentMonetaryAmount : amt?.MonetaryAmount) : amt?.MonetaryAmount;

                    var hostCommands = ticketNumber.SelectMany(x =>
                    {
                        if(changeFeeAmount != null)
                        {
                            return new string[] {  $"WFR{x.TicketNumber}¥AVS¥N{i}.1",
                            string.Empty,
                            string.Empty,
                            string.Empty,
                            string.Empty,
                            string.Empty,
                            string.Empty,
                            string.Empty,
                            string.Empty };
                        }
                        else
                        {
                            return new string[] {  $"WFR{x.TicketNumber}¥AVS¥N{i}.1",
                            string.Empty,
                            string.Empty,
                            string.Empty,
                            string.Empty,
                            string.Empty,
                            string.Empty,
                            string.Empty };
                        }
                    }).ToArray();

                    int index = 0;
                    foreach (var hostCommand in hostCommands)
                    {
                        var sabreCommandLLSRQ = SabreCommandLLSRequestMapper.GetSabreCommandRequest(token, hostCommand); 
                        var sabreCommandLLSRS = await _sabreCommandLLSCommandHandler.Handle(new SabreCommandLLSCommand() { SabreCommandLLSRequest = sabreCommandLLSRQ }, cancellationToken);
                        if (sabreCommandLLSRS != null && sabreCommandLLSRS.ApplicationResults?.Error == null)
                        {
                            var carryOnIndex = changeFeeAmount != null ? 6 : 5;
                            var finalIndex = changeFeeAmount != null ? 7 : 6;
                            if (hostCommand.StartsWith("WFR"))
                            {
                                var sabreCommandResponse = sabreCommandLLSRS.Response.Split([',', '.', ';', ':']).ToArray();
                                var matchCount = sabreCommandResponse.Sum(x => new Regex(Regex.Escape("< >")).Matches(x).Count);
                                var command1 = new StringBuilder();
                                for (int j = 0; j < matchCount; j++)
                                {
                                    command1.Append(j < matchCount - 2 ? "<X>" : "< >");
                                }
                                hostCommands[1] = $"<![CDATA[ WF{command1}]]>";
                            }
                            else if (index == 1)
                            {
                                bool isArunk = sabreCommandLLSRS.Response.Contains("ARNK", StringComparison.OrdinalIgnoreCase);
                                var sabreCommandResponse = Regex.Matches(sabreCommandLLSRS.Response, @"<([^>]*)>");
                                var command1 = new StringBuilder();
                                int finalAirSegmentSelectorIndex = -1;
                                int firstAirSegmentFareBasisIndex = -1;
                                int firstAirSegmentFareAmountIndex = -1;
                                int secondFareAmountIndex = -1;
                                var secondFareBasisIndexes = new List<int>();

                                if (isArunk)
                                {
                                    var airSegments = RegexCompiled.Matches(sabreCommandLLSRS.Response, @"^[ \t]*\d+[ \t]*(?<selector><[^>]*>)?[ \t]*(?!ARNK\b)(?<route>[A-Z]{6})\b", RegexOptions.Multiline);

                                    for (int airSegmentIndex = 0; airSegmentIndex < airSegments.Count; airSegmentIndex++)
                                    {
                                        var airSegment = airSegments[airSegmentIndex];
                                        var selector = airSegment.Groups["selector"];
                                        var route = airSegment.Groups["route"];
                                        int lineEndIndex = sabreCommandLLSRS.Response.IndexOf('\n', route.Index);
                                        var fareFields = new List<int>();

                                        if (selector.Success)
                                            finalAirSegmentSelectorIndex = selector.Index;

                                        if (lineEndIndex < 0)
                                            lineEndIndex = sabreCommandLLSRS.Response.Length;

                                        for (int responseFieldIndex = 0; responseFieldIndex < sabreCommandResponse.Count; responseFieldIndex++)
                                        {
                                            if (sabreCommandResponse[responseFieldIndex].Index > route.Index && sabreCommandResponse[responseFieldIndex].Index < lineEndIndex)
                                                fareFields.Add(responseFieldIndex);
                                        }

                                        if (fareFields.Count >= 2)
                                        {
                                            if (airSegmentIndex == 0)
                                            {
                                                firstAirSegmentFareBasisIndex = fareFields[0];
                                                firstAirSegmentFareAmountIndex = fareFields[1];
                                            }
                                            else
                                            {
                                                secondFareBasisIndexes.Add(fareFields[0]);

                                                if (airSegmentIndex == airSegments.Count - 1)
                                                    secondFareAmountIndex = fareFields[1];
                                            }
                                        }
                                    }
                                }

                                int sabreCommandResponseCount = sabreCommandResponse.Count;

                                for (int j = 0; j < sabreCommandResponseCount; j++)
                                {
                                    var responseField = sabreCommandResponse[j];
                                    var fieldLabel = isArunk ? sabreCommandLLSRS.Response.Substring(0, responseField.Index).TrimEnd() : string.Empty;

                                    if (isArunk && fieldLabel.EndsWith("CXR", StringComparison.OrdinalIgnoreCase))
                                        continue;

                                    string commandValue = responseField.Groups[1].Value.Trim();

                                    if (isArunk)
                                    {
                                        if (string.IsNullOrEmpty(commandValue))
                                        {
                                            if (j == firstAirSegmentFareBasisIndex && fareBasisandAmount.Count > 0)
                                                commandValue = fareBasisandAmount[0].FareBasis;
                                            else if (j == firstAirSegmentFareAmountIndex && fareBasisandAmount.Count > 0)
                                                commandValue = fareBasisandAmount[0].Amount;
                                            else if (secondFareBasisIndexes.Contains(j) && fareBasisandAmount.Count > 1)
                                                commandValue = fareBasisandAmount[1].FareBasis;
                                            else if (j == secondFareAmountIndex && fareBasisandAmount.Count > 1)
                                                commandValue = fareBasisandAmount[1].Amount;
                                        }

                                        if (fieldLabel.EndsWith("Q", StringComparison.OrdinalIgnoreCase))
                                            commandValue = "0";
                                        else if (responseField.Index == finalAirSegmentSelectorIndex)
                                            commandValue = "X";
                                        else if (fieldLabel.EndsWith("PRICE-P/MANUAL-M", StringComparison.OrdinalIgnoreCase))
                                            commandValue = "M";
                                    }
                                    else if (string.IsNullOrEmpty(commandValue))
                                    {
                                        if (j == sabreCommandResponseCount - 2)
                                            commandValue = "M";
                                        else if (j == 9 || j == sabreCommandResponseCount - 4 || j == 15)
                                            commandValue = "0";
                                        else if (j == 20)
                                            commandValue = "X";
                                        else if (j == 7)
                                            commandValue = fareBasisandAmount[0].FareBasis;
                                        else if (j == 8)
                                            commandValue = fareBasisandAmount[0].Amount;
                                        else if (j == 11 || j== 17 && inBoundConnectionFlightSegments.Count() > 1)
                                            commandValue = fareBasisandAmount[1].FareBasis;
                                        else if (j == 12 || j ==18 && inBoundConnectionFlightSegments.Count() > 1)
                                            commandValue = fareBasisandAmount[1].Amount;
                                    }

                                    command1.Append($"<{commandValue}>");
                                }
                                hostCommands[2] = $"<![CDATA[ WF{command1}]]>";
                            }
                            else if (index == 2)
                            {
                                var sabreCommandResponse = Regex.Matches(sabreCommandLLSRS.Response, @"<([^>]*)>").Select(m => m.Groups[1].Value.Trim());
                                var command2 = new StringBuilder();
                                int taxIndex = 0;

                                for (int j = 0; j < sabreCommandResponse.Count(); j++)
                                {
                                    string commandValue = sabreCommandResponse.ElementAtOrDefault(j);

                                    if (newPriceApplicableTax != null && j < newPriceApplicableTax.Count() * 2)
                                    {
                                        if (j % 2 == 0)
                                            commandValue = newPriceApplicableTax.ElementAt(taxIndex).Amount.MonetaryAmount;
                                        else
                                        {
                                            commandValue = newPriceApplicableTax.ElementAt(taxIndex).Code;
                                            taxIndex++;
                                        }
                                    }
                                    else if (string.IsNullOrEmpty(commandValue))
                                    {
                                        commandValue = j == sabreCommandResponse.Count() - 3 ? "X" : " ";
                                    }

                                    command2.Append($"<{commandValue}>");
                                }
                                hostCommands[3] = $"<![CDATA[ WF{command2}]]>";
                            }
                            else if (index == 3)
                            {
                                var sabreCommandResponse = Regex.Matches(sabreCommandLLSRS.Response, @"<([^>]*)>").Select(m => m.Groups[1].Value.Trim());
                                var command1 = new StringBuilder();
                                for (int j = 0; j < sabreCommandResponse.Count(); j++)
                                {
                                    if (j < 1)
                                        command1.Append($"<{sabreCommandResponse.ElementAtOrDefault(j)}>");
                                    else if (j == sabreCommandResponse.Count() - 3)
                                        command1.Append("<X>");
                                    else
                                        command1.Append("< >");
                                }
                                hostCommands[4] = $"<![CDATA[ WF{command1}]]>";
                            }
                            else if (index == 4 && changeFeeAmount != null)
                            {
                                var sabreCommandResponse1 = sabreCommandLLSRS.Response.Split([',', '.', ';', ':']).ToArray();
                                var matchCount = sabreCommandResponse1.Sum(x => new Regex(@"\<\s*\>").Matches(x).Count);
                                var command1 = new StringBuilder();
                                for (int j = 0; j < matchCount; j++)
                                {
                                    if (j == 0)
                                        command1.Append($"<{changeFeeAmount}>");
                                    else if (j == 1)
                                        command1.Append("<XP>");
                                    else if (j == matchCount - 3)
                                        command1.Append("<X>");
                                    else
                                        command1.Append("< >");
                                }
                                hostCommands[5] = $"<![CDATA[ WF{command1}]]>";
                                hostCommands[6] = $"<![CDATA[ WF{command1}]]>";
                            }
                            else if (index == 4 && changeFeeAmount == null)
                            {
                                var sabreCommandResponse1 = sabreCommandLLSRS.Response.Split([',', '.', ';', ':']).ToArray();
                                var matchCount = sabreCommandResponse1.Sum(x => new Regex(@"\<\s*\>").Matches(x).Count);
                                var command1 = new StringBuilder();
                                for (int j = 0; j < matchCount; j++)
                                {
                                    if (j == matchCount - 3)
                                        command1.Append("<X>");
                                    else
                                        command1.Append("< >");
                                }
                                hostCommands[5] = $"<![CDATA[ WF{command1}]]>";
                            }
                            else if (index == carryOnIndex)
                            {
                                var sabreCommandResponse3 = sabreCommandLLSRS.Response.Split([',', '.', ';', ':']).ToArray();
                                var matchCount = sabreCommandResponse3.Sum(x => new Regex(@"\<\s*\>").Matches(x).Count);
                                var command7 = new StringBuilder();
                                for (int j = 0; j < matchCount; j++)
                                { 
                                    if (j ==3 || (j == 5 && addCollectPrice != null))
                                        command7.Append("<X>");
                                    else
                                        command7.Append("< >");
                                }
                                hostCommands[index+1] = $"<![CDATA[ WF{command7}]]>";
                            }
                            else if (index == finalIndex)
                            {
                                var sabreCommandResponse3 = sabreCommandLLSRS.Response.Split([',', '.', ';', ':']).ToArray();
                                var matchCount = sabreCommandResponse3.Sum(x => new Regex(@"\<\s*\>").Matches(x).Count);
                                var command8 = new StringBuilder();
                                for (int j = 0; j < matchCount; j++)
                                {
                                    if(carryOnBaggageCounts != null &&j < inBoundConnectionFlightSegments.Count())
                                        command8.Append($"<{carryOnBaggageCounts.First()}>");
                                    else if (j == matchCount - 3)
                                        command8.Append("<X>");
                                    else
                                        command8.Append("< >");
                                }
                                hostCommands[index+1] = $"<![CDATA[ WF{command8}]]>";
                            }
                            index++;
                        }
                        else
                        {
                            var errors = $" - UnevenExchangeAddCollect - {string.Join(Environment.NewLine, sabreCommandLLSRS.ApplicationResults.Error.SelectMany(x => x.SystemSpecificResults).SelectMany(x => x.Message).Select(x => x.Value))}";
                            await FailureResult(errors, request.orderChangeRequestData.Parties.Participants.First().PccCode, token, cancellationToken);
                        }
                    }
                }
            }
            else
            {
                var differentialAmount = request.orderChangeRequestData.PaymentDetails?.FirstOrDefault()?.Amount?.MonetaryAmount;

                //Uneven Exchange(AddCollect)
                if (differentialAmount.Parse(0D) > 0)
                {
                    for (int i = 1; i <= request.orderChangeRequestData.DataList.CustomerDetails.Count(); i++)
                    {
                        var pax = request.orderChangeRequestData.DataList.CustomerDetails.ElementAt(i - 1);
                        var offeritem = request.orderChangeRequestData.SelectedOffers.First().OfferItems.First(offerItem => offerItem.Services.Any(service => service.ServiceBeneficiaries.Contains(pax.CustId))).OfferitemPricing.FirstOrDefault();
                        var addCollectPrice = offeritem?.AddCollectPrice?.Amount?.EquivalentMonetaryAmount ?? offeritem?.AddCollectPrice?.Amount?.MonetaryAmount;
                        var nameId = $"{pax.LastNameNumber}.{pax.FirstNameNumber}";

                        var ticketDetails = ((ReservationPNRB)getReservationRS.Item).PassengerReservation.TicketingInfo?.TicketDetails;
                        var ticketNumber = ticketDetails?.Where(t => t.PassengerName?.nameId == nameId && t.Status?.ToUpper() == "ACTIVE");

                        var changeFeeAmount = (from pen in request.orderChangeRequestData.DataList.Penalty.Where(x => x.Type == "Change" && x.Text.Equals("Change Penalty"))
                                               from offer in request.orderChangeRequestData.SelectedOffers
                                               from offerItem in offer.OfferItems
                                               from service in offerItem.Services
                                               where service.PenaltyRef.ChangePenaltyRefIds != null && service.PenaltyRef.ChangePenaltyRefIds.Any() && service.PenaltyRef.ChangePenaltyRefIds.Contains(pen.Id) && service.ServiceBeneficiaries.Contains(pax.CustId)
                                               select pen)?.FirstOrDefault()?.MaxPrice?.Amount is var amt && amt != null
                                                    ? (!string.IsNullOrEmpty(amt.EquivalentMonetaryAmount) ? amt.EquivalentMonetaryAmount : amt.MonetaryAmount)
                                                    : null;

                        var hostCommands = ticketNumber.SelectMany(x =>
                        {
                            return new string[] {  $"WFR{x.TicketNumber}¥AVS¥N{i}.1¥PQ{keyValuePairs[pax.Ptc]}",
                        string.Empty,
                        string.Empty,
                        string.Empty,
                        string.Empty
                        };

                        }).ToArray();

                        int index = 0;
                        foreach (var hostCommand in hostCommands)
                        {
                            var sabreCommandLLSRQ = SabreCommandLLSRequestMapper.GetSabreCommandRequest(token, hostCommand);
                            var sabreCommandLLSRS = await _mediator.Send(new SabreCommandLLSCommand() { SabreCommandLLSRequest = sabreCommandLLSRQ }, cancellationToken);
                            if (sabreCommandLLSRS != null && sabreCommandLLSRS.ApplicationResults?.Error == null)
                            {
                                if (hostCommand.StartsWith("WFR"))
                                {
                                    var sabreCommandResponse1 = sabreCommandLLSRS.Response.Split([',', '.', ';', ':']).ToArray();
                                    var matchCount = sabreCommandResponse1.Sum(x => new Regex(Regex.Escape("< >")).Matches(x).Count);
                                    var command1 = new StringBuilder();
                                    for (int j = 0; j < matchCount; j++)
                                    {
                                        command1.Append(j < matchCount - 2 ? "<X>" : "< >");
                                    }
                                    hostCommands[1] = $"<![CDATA[ WF{command1}]]>";
                                }
                                else if (index == 1 && changeFeeAmount != null)
                                {
                                    var sabreCommandResponse1 = sabreCommandLLSRS.Response.Split([',', '.', ';', ':']).ToArray();
                                    var matchCount = sabreCommandResponse1.Sum(x => new Regex(@"\<\s*\>").Matches(x).Count);
                                    var command1 = new StringBuilder();
                                    for (int j = 0; j < matchCount; j++)
                                    {
                                        if (j == 0)
                                            command1.Append($"<{changeFeeAmount}>");
                                        else if (j == 1)
                                            command1.Append("<XP>");
                                        else if (j == matchCount - 2)
                                            command1.Append("<X>");
                                        else
                                            command1.Append("< >");
                                    }
                                    hostCommands[2] = $"<![CDATA[ WF{command1}]]>";
                                    hostCommands[3] = $"<![CDATA[ WF{command1}]]>";
                                }
                                else if (index == 1 && changeFeeAmount == null)
                                {
                                    var sabreCommandResponse1 = sabreCommandLLSRS.Response.Split([',', '.', ';', ':']).ToArray();
                                    var matchCount = sabreCommandResponse1.Sum(x => new Regex(@"\<\s*\>").Matches(x).Count);
                                    var command1 = new StringBuilder();
                                    for (int j = 0; j < matchCount; j++)
                                    {
                                        if (j == matchCount - 2)
                                            command1.Append("<X>");
                                        else
                                            command1.Append("< >");
                                    }
                                    hostCommands[2] = $"<![CDATA[ WF{command1}]]>";
                                    hostCommands[3] = $"<![CDATA[ WF{command1}]]>";
                                }
                                else if (index == 3)
                                {
                                    var sabreCommandResponse3 = sabreCommandLLSRS.Response.Split([',', '.', ';', ':']).ToArray();
                                    var matchCount = sabreCommandResponse3.Sum(x => new Regex(@"\<\s*\>").Matches(x).Count);
                                    var command4 = new StringBuilder();
                                    for (int j = 0; j < matchCount; j++)
                                    {
                                        if (j == matchCount - 3)
                                            command4.Append("<X>");
                                        else if (j == 5 && addCollectPrice != null)
                                            command4.Append("<X>");
                                        else
                                            command4.Append("< >");
                                    }
                                    hostCommands[4] = $"<![CDATA[ WF{command4}]]>";
                                }
                                index++;
                            }
                            else
                            {
                                var errors = $" - UnevenExchangeAddCollect - {string.Join(Environment.NewLine, sabreCommandLLSRS.ApplicationResults.Error.SelectMany(x => x.SystemSpecificResults).SelectMany(x => x.Message).Select(x => x.Value))}";
                                await FailureResult(errors, request.orderChangeRequestData.Parties.Participants.First().PccCode, token, cancellationToken);
                            }
                        }
                    }
                }
                // Even exchange and UnevenExchange(Refund)
                else
                {
                    for (int i = 1; i <= request.orderChangeRequestData.DataList.CustomerDetails.Count(); i++)
                    {
                        var pax = request.orderChangeRequestData.DataList.CustomerDetails.ElementAt(i - 1);
                        var nameId = $"{pax.LastNameNumber}.{pax.FirstNameNumber}";

                        var ticketDetails = ((ReservationPNRB)getReservationRS.Item).PassengerReservation.TicketingInfo?.TicketDetails;
                        var ticketNumber = ticketDetails?.Where(t => t.PassengerName?.nameId == nameId && t.Status?.ToUpper() == "ACTIVE");

                        var hostCommands = ticketNumber.SelectMany(x =>
                        {
                            return new string[]  {$"WFR{x.TicketNumber}¥AVS¥N{i}.1¥PQ{keyValuePairs[pax.Ptc]}",
                        string.Empty,
                        string.Empty,
                        string.Empty };
                        }).ToArray();

                        int index = 0;
                        foreach (var hostCommand in hostCommands)
                        {
                            var sabreCommandLLSRQ = SabreCommandLLSRequestMapper.GetSabreCommandRequest(token, hostCommand);
                            var sabreCommandLLSRS = await _mediator.Send(new SabreCommandLLSCommand() { SabreCommandLLSRequest = sabreCommandLLSRQ }, cancellationToken);
                            if (sabreCommandLLSRS != null && sabreCommandLLSRS.ApplicationResults?.Error == null)
                            {
                                if (hostCommand.StartsWith("WFR"))
                                {
                                    var sabreCommandResponse1 = sabreCommandLLSRS.Response.Split([',', '.', ';', ':']).ToArray();
                                    var matchCount = sabreCommandResponse1.Sum(x => new Regex(Regex.Escape("< >")).Matches(x).Count);
                                    var command1 = new StringBuilder();
                                    for (int j = 0; j < matchCount; j++)
                                    {
                                        command1.Append(j < matchCount - 2 ? "<X>" : "< >");
                                    }
                                    hostCommands[1] = $"<![CDATA[ WF{command1}]]>";
                                }
                                else if (index == 1)
                                {
                                    var sabreCommandResponse2 = sabreCommandLLSRS.Response.Split([',', '.', ';', ':']).ToArray();
                                    var matchCount = sabreCommandResponse2.Sum(x => new Regex(@"\<\s*\>").Matches(x).Count);
                                    var command2 = new StringBuilder();
                                    for (int j = 0; j < matchCount; j++)
                                    {
                                        if (j == matchCount - 2)
                                            command2.Append("<X>");
                                        else
                                            command2.Append("< >");
                                    }
                                    hostCommands[2] = $"<![CDATA[ WF{command2}]]>";
                                }
                                else if (index == 2)
                                {
                                    var sabreCommandResponse3 = sabreCommandLLSRS.Response.Split([',', '.', ';', ':']).ToArray();
                                    var matchCount = sabreCommandResponse3.Sum(x => new Regex(@"\<\s*\>").Matches(x).Count);
                                    var command3 = new StringBuilder();
                                    for (int j = 0; j < matchCount; j++)
                                    {
                                        if (j == matchCount - 3)
                                            command3.Append("<X>");
                                        else
                                            command3.Append("< >");
                                    }
                                    hostCommands[3] = $"<![CDATA[ WF{command3}]]>";
                                }
                                index++;
                            }
                            else
                            {
                                var errors = $" - EvenExchange - {string.Join(Environment.NewLine, sabreCommandLLSRS.ApplicationResults.Error.SelectMany(x => x.SystemSpecificResults).SelectMany(x => x.Message).Select(x => x.Value))}";
                                await FailureResult(errors, request.orderChangeRequestData.Parties.Participants.First().PccCode, token, cancellationToken);
                            }
                        }
                    }
                }

            }
        }

        private async Task<AirTicketRS> TicketFlightItems(string token, OrderChangeRepoCommand request, GetReservationRS getReservationRS,string pcc, CancellationToken cancellationToken)
        {
            var pricedItinerary = ((ReservationPNRB)getReservationRS.Item).PassengerReservation.ItineraryPricing.PricedItinerary;
            var airTicketRQ = EnhancedAirTicketRequestMapper.GetEnhancedAirTicketRequest(token, request.orderChangeRequestData.Rloc, pricedItinerary, pcc);
            var airTicketRS = await _mediator.Send(new EnhancedAirTicketCommand() { EnhancedAirTicketRequest = airTicketRQ }, cancellationToken);
            if (airTicketRS == null || airTicketRS.ApplicationResults?.Error != null)
            {
                var errors = string.Join(Environment.NewLine, airTicketRS.ApplicationResults.Error.SelectMany(x => x.SystemSpecificResults).SelectMany(x => x.Message).Select(x => x.Value));
                if (airTicketRS.ApplicationResults?.Warning != null)
                {
                    errors += string.Join(Environment.NewLine, airTicketRS.ApplicationResults.Warning.SelectMany(x => x.SystemSpecificResults).SelectMany(x => x.Message).Select(x => x.Value));
                }
                await FailureResult(errors, request.orderChangeRequestData.Parties.Participants.First().PccCode, token, cancellationToken);
            }

            return airTicketRS;
        }

        private async Task<Result<Response<OrderDocument>>> FailureResult(string errors,string pcc, [Optional] string token, [Optional] CancellationToken cancellationToken)
        {
            if (!string.IsNullOrEmpty(token))
            {
                var response = await _mediator.Send(new SessionCloseCommand() { BinaryToken = token , PCC = pcc}, cancellationToken);

                if (!response)
                {
                    await FailureResult($"{Constants.Application.ErrorMsg} SessionCloseRQ",pcc, token, cancellationToken);
                }
            }

            throw new Exception(errors);
        }

        private async Task<Result<Response<OrderDocument>>> SuccessResult(OrderDocument orderDocument, string token,string pcc, CancellationToken cancellationToken)
        {
            await _mediator.Send(new SessionCloseCommand() { BinaryToken = token , PCC = pcc}, cancellationToken);

            return Result.Success(new Response<OrderDocument>
            {
                Status = Status.OK,
                Value = orderDocument
            });
        }

        private async Task IgnoreTransaction(string token, string pcc ,CancellationToken cancellationToken)
        {
            var response = await _mediator.Send(new IgnoreTransactionCommand() { BinarySecurityToken = token }, cancellationToken);
            if (!response)
            {
                await FailureResult($"{Constants.Application.ErrorMsg} IgnoreTransactionLLSRQ", pcc,token, cancellationToken);
            }
        }
    }
}
