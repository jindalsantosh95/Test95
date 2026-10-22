using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Domain.Exceptions;
using VAA.EAPI.Domain.Virgin.Models.Customer;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Infrastructure.PSS.Mapper;
using VAA.EAPI.Infrastructure.PSS.Proxy.PNRCommonV6Proxy;
using VAA.EAPI.Infrastructure.PSS.Repository;
using VAA.EAPI.Microservices.Retail.Order.Search.Constants;
using VAA.EAPI.Microservices.Retail.Order.Search.Domain;
using VAA.EAPI.Microservices.Retail.Order.Search.Helper.Cache.Interface;
using VAA.EAPI.Microservices.Retail.Order.Search.Mapper;
using VAA.EAPI.Microservices.Retail.Order.Search.Mapper.FlightBookingCustomer;
using VAA.EAPI.Microservices.Retail.Order.Search.Validation;
using TicketRetrieveResponseType = VAA.EAPI.Infrastructure.PSS.Proxy.TicketingProcessV3Proxy.RetrieveResponseType;

namespace VAA.EAPI.Microservices.Retail.Order.Search.Application.Query
{
    public sealed class OrderSearchResponseQueryHandler : IRequestHandler<OrderSearchResponseQuery, Result<Response<OrderSearchResponse>>>
    {
        private readonly IMediator _mediator;
        private readonly ILogger<OrderSearchResponseQueryHandler> _logger;
        private readonly ICachedAirport _cachedAirport;
        private readonly ICachedRFICData _cachedRFICData;
        private readonly IProductName _productName;
        private readonly ICachedSegmentStatusCode _iCachedSegmentStatusCode;
        private readonly ICachedCabinType _iCachedCabinType;
        private readonly ICachedAmenities _iCachedAmenities;

        public OrderSearchResponseQueryHandler(IMediator mediator, ILogger<OrderSearchResponseQueryHandler> logger, ICachedAirport cachedAirport, ICachedRFICData cachedRFICData, IProductName productName, ICachedSegmentStatusCode cachedSegmentStatusCode, ICachedCabinType cachedCabinType, ICachedAmenities cachedAmenities)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _cachedAirport = cachedAirport ?? throw new ArgumentNullException(nameof(cachedAirport));
            _cachedRFICData = cachedRFICData ?? throw new ArgumentNullException(nameof(cachedRFICData));
            _productName = productName ?? throw new ArgumentNullException(nameof(productName));
            _iCachedSegmentStatusCode = cachedSegmentStatusCode ?? throw new ArgumentNullException(nameof(cachedSegmentStatusCode));
            _iCachedCabinType = cachedCabinType ?? throw new ArgumentNullException(nameof(cachedCabinType));
            _iCachedAmenities = cachedAmenities ?? throw new ArgumentNullException(nameof(cachedAmenities));
        }

        public async Task<Result<Response<OrderSearchResponse>>> Handle(OrderSearchResponseQuery requestData, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Order search started for {PnrLocator}", requestData?.OrderSearchRequest?.SearchFilterCriteria?.bookingReferenceCriteria?.BookingReference?.RLOC);

                var result = ValidateOrderSearchRequest.OrderSearchRequestValidator(requestData);

                if (result.Value.Status == Status.OK)
                {
                    try
                    {
                        var serviceResponse = await GetPnrServiceResponse(requestData, cancellationToken);

                        TicketRetrieveResponseType ticketServiceResponse = null;
                        List<TicketRetrieveResponseType> ticketServiceResponseList = new();
                        List<TicketRetrieveResponseType> allTicketServiceResponseList = new();

                        if (!ValidateOrderSearchRequest.VerifyCustomerName(requestData, serviceResponse))
                        {
                            _logger.LogInformation("Passenger name is incorrect");
                            return Result.Success(new Response<OrderSearchResponse>
                            {
                                Errors = ErrorMapper.MapTechnicalExceptionToEAPIException(ErrorCodes.PassengerNameMisMatchErrorCode, Guid.NewGuid().ToString(), EAPI.Infrastructure.Constants.Constants.ErrorCodesPath),
                                Status = Status.BusinessException
                            });
                        }

                        if (serviceResponse.TeardownContextResponse.Pnr?.TicketInformation != null && serviceResponse.TeardownContextResponse.Pnr.TicketInformation.Any())
                        {
                            foreach (var ticketInformmation in serviceResponse.TeardownContextResponse.Pnr.TicketInformation)
                            {
                                TicketRetrieveResponseType ticketingResponse = null;

                                try
                                {
                                    ticketingResponse = await GetTicketServiceResponse(ticketInformmation.TicketNumber, cancellationToken);
                                }
                                catch
                                {
                                    //for GDS PNRs ignore service failure for tickets where status != "HOOKED"
                                    if (serviceResponse.TeardownContextResponse?.Pnr?.RecordLocator?.FirstOrDefault()?.OutsideSystemRecordLocator != null)
                                    {
                                        if (ticketInformmation.TicketStatus == Constants.Application.HookedTicketStatus) throw;
                                    }
                                    else throw;
                                }

                                if (ticketingResponse != null)
                                {
                                    if (ticketInformmation.TicketStatus == Constants.Application.HookedTicketStatus)
                                        ticketServiceResponseList.Add(ticketingResponse);
                                    allTicketServiceResponseList.Add(ticketingResponse);

                                    var feeDocumentIssuedTypes = ticketingResponse.TicketRequestResponse?.FirstOrDefault().TicketResponse?.FirstOrDefault().FeeDocumentIssued;

                                    if (feeDocumentIssuedTypes != null && feeDocumentIssuedTypes.Any())
                                    {
                                        foreach (var feeDocIssuedType in feeDocumentIssuedTypes)
                                        {
                                            var emdTicketResponse = await GetTicketServiceResponse(feeDocIssuedType.FeeDocumentNumber, cancellationToken);

                                            allTicketServiceResponseList.Add(emdTicketResponse);
                                        }
                                    }
                                }
                            }
                        }

                        if (serviceResponse.TeardownContextResponse.Pnr?.TicketNumberData1 != null && serviceResponse.TeardownContextResponse.Pnr.TicketNumberData1.Any())
                        {
                            string emdNumber = serviceResponse.TeardownContextResponse.Pnr.TicketNumberData1.FirstOrDefault().OriginalTicketNumber.Split('-')[0].ToString();

                            if (!allTicketServiceResponseList.Any(x => x.TicketRequestResponse.FirstOrDefault().TicketRequest?.ElectronicDocumentNumber == emdNumber))
                            {
                                try
                                {
                                    allTicketServiceResponseList.Add(await GetTicketServiceResponse(emdNumber, cancellationToken));
                                }
                                catch (Exception)
                                {                                    
                                    _logger.LogError("Delta ticketing service retrieval failed for an EMD while processing order search");
                                }
                            }
                        }

                        if (serviceResponse?.TeardownContextResponse?.Pnr?.DtiSegmentInfo == null)
                        {
                            if (serviceResponse.TeardownContextResponse.Pnr?.TicketInformation?.FirstOrDefault(ticket => ticket.TicketStatus == Constants.Application.HookedTicketStatus).TicketNumber != null)
                            {
                                ticketServiceResponse = await GetTicketServiceResponse(serviceResponse.TeardownContextResponse.Pnr?.TicketInformation?.FirstOrDefault().TicketNumber, cancellationToken);
                            }
                        }

                        var pnrRetriveResponse = PNRRetrieveHistoryRSMapper.MapToPnrRetrieveResponse(serviceResponse);
                        var customersDetails = CustomerDetailMapper.PassengerToCustomerMapper(pnrRetriveResponse?.PnrType?.Passengers, pnrRetriveResponse.PnrType?.InfantInArms ?? new List<EAPI.Domain.Virgin.Models.Customer.InfantInArms>(), pnrRetriveResponse?.PnrType?.Ptcs);
                        List<Customer> customers = new();
                        try
                        {
                            foreach (var customer in customersDetails)
                            {
                                var sessionId = await _mediator.Send(new PNRSetupContextRepoQuery { RecordLocator = requestData.OrderSearchRequest.SearchFilterCriteria?.bookingReferenceCriteria?.BookingReference?.RLOC }, cancellationToken);

                                var passengerV4Response = await _mediator.Send(new PassengerV4ManageMultipleTravelDocsAndSecureFlightRepoQuery() { Customer = customer, SessionId = sessionId, RetrieveFlag = true }, cancellationToken);

                                if (passengerV4Response != null)
                                {
                                    passengerV4Response.CustId = customer.CustId;
                                    passengerV4Response.Ptc = customer.Ptc;
                                    customers.Add(passengerV4Response);
                                }
                            }
                        }
                        catch (ExternalServiceIntegrationException)
                        {
                            _logger.LogError("Delta Passenger V4 gender and date-of-birth retrieval failed while processing order search");
                            customers = null;
                        }

                        var orderSearchResponse = await PNRRetrieveMapper.RetrieveResponseMapper(serviceResponse, _cachedAirport, ticketServiceResponse, ticketServiceResponseList, _productName, _iCachedSegmentStatusCode, _iCachedCabinType, _iCachedAmenities, _mediator, customers);

                        if (orderSearchResponse.OrderItems.Any(x => x.Services.Any(s => s.CurrentSeatAssignment != null)))
                            await GetAccountableDocforSeats(orderSearchResponse, serviceResponse, cancellationToken);

                        GetPaymentDetails(allTicketServiceResponseList, orderSearchResponse);

                        if (!orderSearchResponse.SegmentDetails.Any() && !orderSearchResponse.TimeLimits.IsNullOrEmpty())
                        {
                            orderSearchResponse.OrderItems = orderSearchResponse.OrderItems.Select(c => { c.ItemStatus.Status = Constants.Application.HeldBookingStatus; return c; });
                        }

                        return new Response<OrderSearchResponse>()
                        {
                            Status = Status.OK,
                            Value = orderSearchResponse
                        };
                    }
                    catch (ExternalServiceIntegrationException ex)
                    {
                        _logger.LogError("Delta order-search retrieval failed for {PnrLocator}", requestData?.OrderSearchRequest?.SearchFilterCriteria?.bookingReferenceCriteria?.BookingReference?.RLOC);

                        if (ex.InnerException.Message.Contains(Constants.Application.InvalidPNRErrorMessage))
                            return Result.Failure<Response<OrderSearchResponse>>(Json.Serialize(ErrorMapper.MapTechnicalExceptionToEAPIException(ErrorCodes.PNRExpiredErrorCode, Guid.NewGuid().ToString(), EAPI.Infrastructure.Constants.Constants.ErrorCodesPath)));
                        else
                            return Result.Failure<Response<OrderSearchResponse>>(Json.Serialize(ErrorMapper.MapDeltaExceptionToEAPIException(ErrorCodes.ExceptionErrorCode, Guid.NewGuid().ToString(), EAPI.Infrastructure.Constants.Constants.ErrorCodesPath, ex.InnerException.Message)));
                    }
                }
                else
                {
                    return Result.Success(new Response<OrderSearchResponse>
                    {
                        Errors = result.Value.Errors,
                        Status = Status.BusinessException
                    });
                }
            }
            catch (Exception exception)
            {
                _logger.LogError("Unexpected order-search processing failure for {PnrLocator}; exception type {ExceptionType}", requestData?.OrderSearchRequest?.SearchFilterCriteria?.bookingReferenceCriteria?.BookingReference?.RLOC, exception.GetType().Name);
                return Result.Failure<Response<OrderSearchResponse>>(Json.Serialize(ErrorMapper.MapTechnicalExceptionToEAPIException(EAPI.Infrastructure.Constants.Constants.TechnicalError, Guid.NewGuid().ToString(), EAPI.Infrastructure.Constants.Constants.GlobalExceptionFilePath)));
            }
        }

        private void GetPaymentDetails(List<TicketRetrieveResponseType> ticketServiceResponseList, OrderSearchResponse orderSearchResponse)
        {
            var ticketResponseList = ticketServiceResponseList.SelectMany(x => x.TicketRequestResponse.FirstOrDefault()?.TicketResponse);
            var groupedTicketResponse = ticketResponseList.Where(t => t.FormOfPayment?.FirstOrDefault()?.CreditCardNumber != null).GroupBy(x => new { x?.FormOfPayment?.FirstOrDefault()?.CreditCardNumber })?.Select(y => y);
            List<PaymentDetails> paymentDetailsList = new();
            DateTime lastDateTime = default;
            DateTime currentDateTime = default;

            if (groupedTicketResponse != null && groupedTicketResponse.Count() > 0)
            {
                foreach (var ticketRetrieveResponseType in groupedTicketResponse)
                {
                    var ticketRetrieveResponseByDateTime = ticketRetrieveResponseType.OrderBy(x => x.TimeDayLogicalRecord?.Date).ThenBy(x => x.TimeDayLogicalRecord?.Time);
                    List<EAPI.Infrastructure.PSS.Proxy.TicketingProcessV3Proxy.TicketResponseType> ticketRetrieveResponseList = new List<EAPI.Infrastructure.PSS.Proxy.TicketingProcessV3Proxy.TicketResponseType>();

                    lastDateTime = default;

                    foreach (var ticketRetrieveResponse in ticketRetrieveResponseByDateTime)
                    {
                        if (ticketRetrieveResponse.TimeDayLogicalRecord?.Date != null && ticketRetrieveResponse.TimeDayLogicalRecord?.Time != null)
                            currentDateTime = DateTime.ParseExact(ticketRetrieveResponse.TimeDayLogicalRecord.Date + " " + (ticketRetrieveResponse.TimeDayLogicalRecord.Time.Substring(4,2) == "60" ? $"{ticketRetrieveResponse.TimeDayLogicalRecord.Time[..2]}{(Convert.ToInt32(ticketRetrieveResponse.TimeDayLogicalRecord.Time.Substring(2,2)) + 1).ToString("00")}" + "00" : ticketRetrieveResponse.TimeDayLogicalRecord.Time), "dMMMyy HHmmss", CultureInfo.InvariantCulture);

                        if (lastDateTime != default && currentDateTime.Subtract(lastDateTime).TotalMinutes > 3)
                        {
                            paymentDetailsList.Add(GetPaymentDetailsBlock(ticketRetrieveResponseList, paymentDetailsList.Count + 1, orderSearchResponse));
                            ticketRetrieveResponseList.Clear();
                        }

                        ticketRetrieveResponseList.Add(ticketRetrieveResponse);
                        lastDateTime = currentDateTime;
                    }

                    if (ticketRetrieveResponseList.Count > 0)
                    {
                        paymentDetailsList.Add(GetPaymentDetailsBlock(ticketRetrieveResponseList, paymentDetailsList.Count + 1, orderSearchResponse));
                        ticketRetrieveResponseList.Clear();
                    }
                }
            }

            // Get even exchange tickets
            var groupedTicketRsForEvenExchange = ticketResponseList.Where(t => t.FormOfPayment?.FirstOrDefault()?.CreditCardNumber == null).OrderBy(x => x.TimeDayLogicalRecord?.Date).ThenBy(x => x.TimeDayLogicalRecord?.Time);
            if (groupedTicketRsForEvenExchange != null && groupedTicketRsForEvenExchange.Any())
            {

                List<EAPI.Infrastructure.PSS.Proxy.TicketingProcessV3Proxy.TicketResponseType> ticketRetrieveResponseList = new List<EAPI.Infrastructure.PSS.Proxy.TicketingProcessV3Proxy.TicketResponseType>();

                lastDateTime = default;

                foreach (var ticketRetrieveResponse in groupedTicketRsForEvenExchange)
                {
                    if (ticketRetrieveResponse.TimeDayLogicalRecord != null && !string.IsNullOrEmpty(ticketRetrieveResponse.TimeDayLogicalRecord?.Date))
                        currentDateTime = DateTime.ParseExact(ticketRetrieveResponse.TimeDayLogicalRecord.Date + " " + ticketRetrieveResponse.TimeDayLogicalRecord?.Time ?? "00:00:00", "dMMMyy HHmmss", CultureInfo.InvariantCulture);

                    if (lastDateTime != default && currentDateTime.Subtract(lastDateTime).TotalMinutes > 3)
                    {
                        paymentDetailsList.Add(GetOfflinePaymentDetailsBlock(ticketRetrieveResponseList, paymentDetailsList.Count + 1, orderSearchResponse));
                        ticketRetrieveResponseList.Clear();
                    }

                    ticketRetrieveResponseList.Add(ticketRetrieveResponse);
                    lastDateTime = currentDateTime;
                }

                if (ticketRetrieveResponseList.Count > 0)
                {
                    paymentDetailsList.Add(GetOfflinePaymentDetailsBlock(ticketRetrieveResponseList, paymentDetailsList.Count + 1, orderSearchResponse));
                    ticketRetrieveResponseList.Clear();
                }
            }

            orderSearchResponse.PaymentDetails = paymentDetailsList;
        }

        private PaymentDetails GetPaymentDetailsBlock(IEnumerable<EAPI.Infrastructure.PSS.Proxy.TicketingProcessV3Proxy.TicketResponseType> ticketRetrieveResponseList, int counter, OrderSearchResponse orderSearchResponse)
        {
            ticketRetrieveResponseList = ticketRetrieveResponseList.OrderBy(x => Convert.ToDouble(x.TicketInformation.TotalFare));

            var ticketRetrieveResponse = ticketRetrieveResponseList.FirstOrDefault();
            string cardNumber = ticketRetrieveResponse.FormOfPayment.First().CreditCardNumber;
            IEnumerable<string> documentNumberList = ticketRetrieveResponseList.Select(x => Convert.ToString(x.DebugInfo?.DataSourceRequest?.Split(',')[2]) ?? x.TicketInformation.FirstTicketNumber);

            foreach (var x in orderSearchResponse.OrderItems.Where(oi => oi.AccountableDocumentRefs != null &&
                                                                  (oi.AccountableDocumentRefs.Any(x => documentNumberList.Contains(x.DocumentNumber)) ||
                                                                   oi.AccountableDocumentRefs.Any(x => documentNumberList.Contains(x.OriginalTicketDocInfo?.DocumentNumber)))))
            {
                x.PaymentReferenceIDs = x.PaymentReferenceIDs.Select(p => p.ToString()).Append(counter.ToString());
            }

            return new PaymentDetails()
            {
                Amount = new Amount()
                {
                    BaseCurrency = ticketRetrieveResponse.TicketInformation?.CurrencyCodeOfTotalFare,
                    MonetaryAmount = ticketRetrieveResponse.TicketInformation?.CurrencyCodeOfTotalFare == Constants.Application.IndianCurrency ? ticketRetrieveResponseList.Sum(x => Convert.ToInt32(x.TicketInformation.TotalFare)).ToString() :
                    ticketRetrieveResponseList.Sum(x => Convert.ToDouble(x.TicketInformation.TotalFare)).ToString("F")
                },
                CardMethod = new CardMethod()
                {
                    CardNumber = cardNumber.Substring(cardNumber.Length - 4).PadLeft(cardNumber.Length, '*'),
                    ExpiryDate = ticketRetrieveResponse.FormOfPayment.FirstOrDefault().CCExpirationDate,
                    CardBrandCode = ticketRetrieveResponse.FormOfPayment.FirstOrDefault().CreditCardVendor
                },
                PaymentID = (ticketRetrieveResponse.DebugInfo?.DataSourceRequest?.Split(',')[2] ?? ticketRetrieveResponse.TicketInformation.FirstTicketNumber) + cardNumber.Substring(cardNumber.Length - 4),
                PaymentReferenceID = Convert.ToString(counter),
                FormOfPayment = PaymentDetails.FormOfPayments.CardEnum
            };
        }

        private PaymentDetails GetOfflinePaymentDetailsBlock(IEnumerable<EAPI.Infrastructure.PSS.Proxy.TicketingProcessV3Proxy.TicketResponseType> ticketRetrieveResponseList, int counter, OrderSearchResponse orderSearchResponse)
        {
            ticketRetrieveResponseList = ticketRetrieveResponseList.OrderBy(x => Convert.ToDouble(x.TicketInformation.TotalFare));

            var ticketRetrieveResponse = ticketRetrieveResponseList.FirstOrDefault();
            IEnumerable<string> documentNumberList = ticketRetrieveResponseList.Select(x => Convert.ToString(x.DebugInfo?.DataSourceRequest?.Split(',')[2]) ?? x.TicketInformation.FirstTicketNumber);

            foreach (var x in orderSearchResponse.OrderItems.Where(oi => oi.AccountableDocumentRefs != null && oi.AccountableDocumentRefs.Any(x => documentNumberList.Contains(x.DocumentNumber))))
                x.PaymentReferenceIDs = x.PaymentReferenceIDs.Select(p => p.ToString()).Append(counter.ToString());

            return new PaymentDetails()
            {
                Amount = new Amount()
                {
                    BaseCurrency = ticketRetrieveResponse.TicketInformation?.CurrencyCodeOfTotalFare,
                    MonetaryAmount = ticketRetrieveResponse.TicketInformation?.CurrencyCodeOfTotalFare == Constants.Application.IndianCurrency ? Constants.Application.IndianCurrencyDefaultValue : Constants.Application.OtherCurrencyDefaultValue
                },
                PaymentID = ticketRetrieveResponse.DebugInfo?.DataSourceRequest?.Split(',')[2] ?? ticketRetrieveResponse.TicketInformation.FirstTicketNumber,
                PaymentReferenceID = Convert.ToString(counter),
                FormOfPayment = PaymentDetails.FormOfPayments.OfflinePaymentEnum
            };
        }
        private async Task<RetrieveResponseType> GetPnrServiceResponse(OrderSearchResponseQuery requestData, CancellationToken cancellationToken)
        {
            if (requestData.OrderSearchRequest.SearchFilterCriteria?.bookingReferenceCriteria?.BookingReference?.RLOC == null && requestData.OrderSearchRequest?.SearchFilterCriteria?.accountableDocumentFilterCriteria?.accountableDocumentReference?.DocumentNumber != null)
            {
                var ticketingResponse = await GetTicketServiceResponse(requestData.OrderSearchRequest.SearchFilterCriteria.accountableDocumentFilterCriteria.accountableDocumentReference.DocumentNumber, cancellationToken);
                if (ticketingResponse != null)
                {
                    requestData.OrderSearchRequest.SearchFilterCriteria.bookingReferenceCriteria = new BookingReferenceCriteria()
                    {
                        BookingReference = new BookingReference()
                        {
                            RLOC = ticketingResponse.TicketRequestResponse?.FirstOrDefault().TicketResponse?.FirstOrDefault().TicketInformation?.PNRRecordLocator != null ? ticketingResponse.TicketRequestResponse?.FirstOrDefault().TicketResponse?.FirstOrDefault().TicketInformation?.PNRRecordLocator : ticketingResponse.TicketRequestResponse?.FirstOrDefault().TicketResponse?.FirstOrDefault().TicketInformation?.RecordLocatorForPNRFileAddress
                        },
                        customerName = requestData.OrderSearchRequest.SearchFilterCriteria.accountableDocumentFilterCriteria.customerName
                    };
                }
            }

            return await _mediator.Send(new PNRRetrieveRepoQuery(PNRRetrieveMapper.RetrieveRequestMapper(requestData.OrderSearchRequest)), cancellationToken);
        }

        private async Task GetAccountableDocforSeats(OrderSearchResponse orderSearchResponse, RetrieveResponseType response, CancellationToken cancellationToken)
        {
            RFICTableEntity rficData = await _cachedRFICData.GetCachedRFICData("Seat");

            var orderItems = orderSearchResponse.OrderItems.Where(x => x.Services != null && x.Services.Any(s => s.CurrentSeatAssignment != null));

            List<TicketRetrieveResponseType> ticketRetrieveResponses = new List<TicketRetrieveResponseType>();

            if (orderItems.Any())
            {
                foreach (var orderItem in orderItems)
                {
                    List<string> ticketNumbers = new();
                    bool assignAdf = false;

                    var passenger = response.TeardownContextResponse.Pnr.Passenger.First(psg => psg.CustomerId == orderItem.Services.First().ServiceBeneficiaries.First());
                    ticketNumbers.Add(response.TeardownContextResponse.Pnr.TicketData.First(x => x.LastNameNumber == passenger.LastNameNumber).TicketNumber);

                    foreach (var tktNumber in ticketNumbers)
                    {
                        var ticketingResponse = await GetTicketServiceResponse(tktNumber, cancellationToken);

                        if (ticketingResponse != null)
                        {
                            var feeDocumentIssuedTypes = ticketingResponse.TicketRequestResponse?.FirstOrDefault().TicketResponse?.FirstOrDefault().FeeDocumentIssued;

                            if (feeDocumentIssuedTypes != null && feeDocumentIssuedTypes.Any())
                            {
                                var seatDl = response.TeardownContextResponse.Pnr.SeatDl.FirstOrDefault(s => s.CurrentSeatAssignment == orderItem.Services.First().CurrentSeatAssignment && s.SegmentNumber == orderItem.Services.First().ProductSegmentAssociation.FlightSegmentIds.First());
                                string seatJourney = $"{seatDl?.DeparturePoint}{seatDl?.ArrivalPoint}";

                                foreach (var feeDocIssuedType in feeDocumentIssuedTypes)
                                {
                                    var emdTicketResponse = await GetTicketServiceResponse(feeDocIssuedType.FeeDocumentNumber, cancellationToken);

                                    if (!assignAdf && !orderSearchResponse.IsCabinUpgraded)
                                    {
                                        if (emdTicketResponse != null && emdTicketResponse.TicketRequestResponse != null && emdTicketResponse.TicketRequestResponse.First().TicketResponse != null)
                                        {
                                            var reasonForIssurance = emdTicketResponse.TicketRequestResponse.First().TicketResponse.First().TicketCoupon.First().ReasonForIssuance;

                                            if (reasonForIssurance?.SubCode != null && reasonForIssurance.SubCode.StartsWith("U"))
                                            {
                                                orderSearchResponse.IsCabinUpgraded = true;
                                                orderSearchResponse.UpgradedCabinDesc = reasonForIssurance.Description;
                                            }
                                        }
                                    }

                                    var seatRmks = response.TeardownContextResponse.Pnr.Remark.Where(rmk => rmk.FreeformText != null && rmk.FreeformText.Contains(feeDocIssuedType.FeeDocumentNumber) && rmk.FreeformText.Contains(seatJourney));

                                    if (!assignAdf && seatRmks.Any(rmk => rmk.FreeformText.Contains(seatJourney)))
                                    {
                                        List<AccountableDocumentRef> documents = new();
                                        List<PriceDetail> priceDtls = new();
                                        documents.Add(new AccountableDocumentRef()
                                        {
                                            DocumentNumber = feeDocIssuedType.FeeDocumentNumber,
                                            DocumentType = AccountableDocumentRef.DocumentTypes.YEnum,
                                            CustomerRefId = passenger?.CustomerId,
                                            DocumentIssuer = null,
                                            Coupons = GetCoupon(emdTicketResponse),
                                            IssueDate = DateTimeHelper.ConvertDateToString(feeDocIssuedType.DateOfIssueOfFeeDocument, Constants.Application.DeltaDateFormat, Constants.Application.DateFormat),
                                            FareCalculation = null,
                                            ReasonForIssuance = new ReasonForIssuance()
                                            {
                                                RFIC = rficData?.Code,
                                                RFISC = rficData?.SubCode
                                            }
                                        });
                                        priceDtls.Add(new PriceDetail()
                                        {
                                            PriceComponentType = PriceDetail.PriceComponentTypes.BasePriceEnum,
                                            PriceComponentName = string.Empty,
                                            Amount = new Amount()
                                            {
                                                BaseCurrency = feeDocIssuedType.CurrencyCodeForFeeDocumentTotalAmount,
                                                MonetaryAmount = feeDocIssuedType.FeeDocumentTotalAmount
                                            }
                                        });
                                        orderItem.AccountableDocumentRefs = documents;
                                        orderItem.PriceDetails = priceDtls;
                                        assignAdf = true;
                                    }
                                }
                            }
                        }
                    };
                }
            }
        }

        private async Task<TicketRetrieveResponseType> GetTicketServiceResponse(string ticketNumber, CancellationToken cancellationToken) => await _mediator.Send(new TicketingRetrieveRepoQuery(TicketingRetrieveMapper.TicketingRetrieveRequestMapper(ticketNumber)), cancellationToken);

        private static List<Coupon> GetCoupon(TicketRetrieveResponseType emdTicketResponse)
        {
            List<Coupon> couponList = new();
            var ticketingResult = emdTicketResponse?.TicketRequestResponse?.FirstOrDefault()?.TicketResponse?.FirstOrDefault()?.TicketCoupon?.FirstOrDefault();
            var couponStatus = ticketingResult != null ? ticketingResult.StatusCode : string.Empty;

            if (!string.IsNullOrEmpty(couponStatus))
                couponList.Add(new Coupon()
                {
                    CouponStatus = couponStatus == Constants.Application.FlownStatus ? Constants.Application.FlownStatus : Constants.Application.OpenStatus
                });

            return couponList;
        }
    }
}
