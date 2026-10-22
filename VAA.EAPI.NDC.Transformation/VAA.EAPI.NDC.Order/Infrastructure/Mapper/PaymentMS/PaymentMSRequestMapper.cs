using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Domain.Virgin.Models.Common;
using VAA.EAPI.Domain.Virgin.Models.Customer;
using VAA.EAPI.Domain.Virgin.Models.Order;
using VAA.EAPI.Domain.Virgin.Models.Payment;
using VAA.EAPI.Domain.Virgin.Models.Products.Flight;
using VAA.EAPI.Domain.Virgin.Models.Ticket;
using VAA.EAPI.NDC.Order.Constants;
using VAA.EAPI.NDC.Order.Domain.OrderChangeRQ;
using VAA.EAPI.NDC.Order.Domain.OrderCreateRQ;
using VAA.EAPI.NDC.Order.Infrastructure.Model.Air4OrderSearchMS;
using VAA.EAPI.NDC.Order.Infrastructure.Model.ARCPaymentMS;
using VAA.EAPI.NDC.Order.Infrastructure.Model.OfferRS;
using VAA.EAPI.NDC.Order.Infrastructure.Model.OrderMS;
using VAA.EAPI.NDC.Order.Infrastructure.Model.PaymentMS;
using VAA.EAPI.NDC.Order.Infrastructure.Model.ReferenceMS;
using VAA.EAPI.NDC.Order.Infrastructure.Services.PaymentIFGMS;
using VAA.EAPI.NDC.Order.Infrastructure.Services.PaymentMS;
using static VAA.EAPI.Domain.Virgin.Models.Common.Price;

namespace VAA.EAPI.NDC.Order.Infrastructure.Mapper.OrderMS
{
    public static class PaymentMSRequestMapper
    {
        public static PaymentMSAuthorizeCommand MapOrderCreateAuthorizeRequest(OrderRS orderRS, IATA_OrderCreateRQ orderCreateRQ, OfferResponse offerDocResponse, IEnumerable<Currency> currencyReferenceMSResponse, IEnumerable<AirportDetail> listAirportDetails, string amount, ProductType productType, Domain.OrderCreateRQ.PaymentProcessingDetailsType paymentProcessingDetails = null)
        {
            var paymentRq = paymentProcessingDetails ?? orderCreateRQ.Request.PaymentFunctions.First().PaymentProcessingDetails;
            var paymentMethodDetail = ((VAA.EAPI.NDC.Order.Domain.OrderCreateRQ.PaymentProcessingPaymentCardType)paymentRq.PaymentMethod.Item);
            var transactionType = paymentMethodDetail.SecurePaymentVersion2.PaymentTrxChannelCode.ToString();
            var adtContactInfoRefIds = orderCreateRQ?.Request?.DataLists?.PaxList?.Where(x => x.PTC.ToUpper() == "ADT").Select(x => x.ContactInfoRefID);
            var adtConactInfo = orderCreateRQ?.Request?.DataLists?.ContactInfoList?.Where(x => adtContactInfoRefIds.Contains(x.ContactInfoID)).SelectMany(x => GetEmailAddressesForPayment(x).Select(y => new { y.EmailAddressText, Phone = x.Phone?.FirstOrDefault()?.PhoneNumber, CountryDialingCode = x.Phone?.FirstOrDefault()?.CountryDialingCode?.Replace("+", "") })).First(x => x.EmailAddressText != null);
            var cardInfo = (CardInfo)((Domain.OrderCreateRQ.PaymentProcessingPaymentCardType)paymentRq.PaymentMethod.Item);
            var firstName = ((Domain.OrderCreateRQ.PayerIndividualType)paymentRq.Payer.PayerName.Item)?.GivenName.FirstOrDefault();
            var lastName = ((Domain.OrderCreateRQ.PayerIndividualType)paymentRq.Payer.PayerName.Item)?.Surname;
            cardInfo = MapCardHolderName(cardInfo, firstName, lastName);

            var paymentDetails = new List<Model.PaymentMS.PaymentDetails>()
            {
                new()
                {
                    Amount = new CurrencyAmount()
                    {
                         MonetaryAmount = amount,
                         BaseCurrency = paymentRq?.Amount?.CurCode.ToString(),
                         CurrencyId = currencyReferenceMSResponse.First(a=>a.CurrencyCode == paymentRq.Amount.CurCode).CurrencyNumber,
                    },
                    CardMethod = cardInfo,
                    Payer = new PaymentPayer()
                    {
                        FirstName = firstName,
                        LastName = lastName,
                        PayerAddress = new Address()
                        {
                            AddressLine1 = paymentMethodDetail.CardholderAddress?.StreetText.FirstOrDefault(),
                            AddressLine2 = paymentMethodDetail.CardholderAddress?.StreetText.LastOrDefault(),
                            PostalCode = paymentMethodDetail.CardholderAddress?.PostalCode,
                            City = paymentMethodDetail.CardholderAddress?.CityName,
                            Country = paymentMethodDetail.CardholderAddress?.CountryCode,
                        },
                        PayerContacts = new Contacts()
                        {
                           Email =adtConactInfo?.EmailAddressText,
                            Phone = adtConactInfo?.Phone,
                            CountryDailingCode = adtConactInfo?.CountryDialingCode
                        }
                    },
                    PaymentCardAuthentication = (transactionType == "MO") || (transactionType == "TO")? null: (VAA.EAPI.NDC.Order.Infrastructure.Model.PaymentMS.PaymentCardAuthentication)((VAA.EAPI.NDC.Order.Domain.OrderCreateRQ.PaymentProcessingPaymentCardType)paymentRq.PaymentMethod.Item),
                    TransactionTypeId = (transactionType == "MO") || (transactionType == "TO") ? 7 : 1009
                }
            };

            IEnumerable<EAPI.Domain.Virgin.Models.Products.Flight.OriginDestination> OriginDestinations = offerDocResponse?.DataList?.FlightSegments?.Select(x => new EAPI.Domain.Virgin.Models.Products.Flight.OriginDestination { OriginAirport = x.OriginAirportCode, DestinationAirport = x.DestinationAirportCode }).ToList();

            return new PaymentMSAuthorizeCommand()
            {
                Pos = orderRS.OrderDocument.PointOfSale,
                Parties = orderRS.OrderDocument.Parties,
                BookingReferences = orderRS.OrderDocument.BookingReference,
                OrderId = $"VS932{orderRS.OrderDocument.BookingReference?.RLOC}{DateTime.UtcNow:yy}",
                PaymentDetails = paymentDetails,
                ProductType = productType,
                JourneyType = OriginDestinations.Count() == 1 ? "ONEWAY" : ((OriginDestinations.Count() == 2 && OriginDestinations.First().OriginAirport == OriginDestinations.Last().DestinationAirport && OriginDestinations.First().DestinationAirport == OriginDestinations.Last().OriginAirport) ? "RETURN" : "MULTICITY"),
                CustomerDetails = orderCreateRQ?.Request?.DataLists?.PaxList?.Select(x => new Customer
                {
                    CustId = x.PaxID,
                    FirstName = x.Individual.GivenName.FirstOrDefault(),
                    LastName = x.Individual.Surname,
                    Ptc = x.PTC,
                    Contacts = new()
                    {
                        Email = adtConactInfo?.EmailAddressText,
                    },
                    DateOfBirth = x.Individual.Birthdate.ToString("yyyy-MM-dd"),
                }),
                Segments = offerDocResponse?.DataList?.FlightSegments?.Select(x => new Segment
                {
                    ClassOfService = x.ClassOfService,
                    OriginAirportCode = x.OriginAirportCode,
                    DestinationAirportCode = x.DestinationAirportCode,
                    OriginCountryCode = listAirportDetails.FirstOrDefault(y => y.AirportCode == x.OriginAirportCode)?.CountryCode,
                    DestinationCountryCode = listAirportDetails.FirstOrDefault(y => y.AirportCode == x.DestinationAirportCode)?.CountryCode,
                    FareBasis = GetSegmentLevelFareBasis(orderRS.OrderDocument, x.SegmentId),
                    MarketingCarrier = x.MarketingCarrier,
                    MarketingFlightNumber = x.MarketingFlightNumber,
                    OperatingCarrier = x.OperatingCarrier ?? x.MarketingCarrier,
                    OperatingFlightNumber = x.OperatingFlightNumber ?? x.MarketingFlightNumber,
                    ScheduledArrivalTime = x.ScheduledArrivalTime,
                    ScheduledDepartureTime = x.ScheduledDepartureTime,
                    SegmentId = x.SegmentId
                }),
            };
        }

        public static PaymentMSCaptureCommand MapOrderCreateCaptureRequest(PaymentMSAuthorizeCommand authorizeRequest, IEnumerable<AccountableDocumentRef> accountableDocumentRefs, PaymentRS paymentRS, OfferResponse offerDocResponse, ProductType productType, BookingReference bookingReference, OrderDocument orderDocument)
        {
            var payment = authorizeRequest.PaymentDetails.First();

            var paymentDetails = new List<Payment>()
            {
                new()
                {
                    PaymentTransactionId = paymentRS.PaymentDocument.TransactionId,
                    Amount = payment.Amount,
                    CardMethod = payment.CardMethod,
                    Payer = payment.Payer,
                    PaymentCardAuthentication = payment.PaymentCardAuthentication,
                }
            };

            IEnumerable<EAPI.Domain.Virgin.Models.Products.Flight.OriginDestination> OriginDestinations = offerDocResponse?.DataList?.FlightSegments?.Select(x => new EAPI.Domain.Virgin.Models.Products.Flight.OriginDestination { OriginAirport = x.OriginAirportCode, DestinationAirport = x.DestinationAirportCode }).ToList();

            return new PaymentMSCaptureCommand()
            {
                Pos = authorizeRequest.Pos,
                Parties = authorizeRequest.Parties,
                BookingReferences = bookingReference,
                OrderId = $"VS932{authorizeRequest.BookingReferences?.RLOC}{DateTime.UtcNow:yy}",
                PaymentDetails = paymentDetails,
                CustomerDetails = authorizeRequest.CustomerDetails,
                AccountableDocumentRefs = orderDocument.OrderItems.Where(o => o.AccountableDocumentRefs != null && o.AccountableDocumentRefs.Any(a => accountableDocumentRefs.Any(b => b.DocumentNumber == a.DocumentNumber))).SelectMany(a => a.AccountableDocumentRefs),
                ProductType = productType,
                JourneyType = OriginDestinations.Count() == 1 ? "ONEWAY" : ((OriginDestinations.Count() == 2 && OriginDestinations.First().OriginAirport == OriginDestinations.Last().DestinationAirport && OriginDestinations.First().DestinationAirport == OriginDestinations.Last().OriginAirport) ? "RETURN" : "MULTICITY"),
                Segments = authorizeRequest.Segments,
                OrderItems = orderDocument.OrderItems.Where(o => o.AccountableDocumentRefs != null && o.AccountableDocumentRefs.Any(a => accountableDocumentRefs.Any(b => b.DocumentNumber == a.DocumentNumber))).Select(oi =>
                {
                    return new Model.PaymentMS.OrderItems()
                    {
                        OrderItemId = oi.OrderItemId,
                        ProductType = productType,
                        AccountableDocumentRefIds = oi.AccountableDocumentRefs.Select(a => a.DocumentNumber),
                        CustRefIds = oi.Services.SelectMany(s => s.ServiceBeneficiaries).Distinct(),
                        SegmentRefIds = oi.Services.SelectMany(s => s.SegmentRefIds)
                    };
                })
            };
        }

        public static PaymentMSAuthorizeCommand MapHoldBookingAuthorizeRequest(OrderDocument orderDocument, IATA_OrderChangeRQ orderChangeReq, IEnumerable<AirportDetail> listAirportDetails, string amount, IEnumerable<Currency> currencies, ProductType productType)
        {
            var paymentReq = orderChangeReq.Request.PaymentFunctions.First().PaymentProcessingDetails;
            var adtConactInfo = orderDocument.DataList.CustomerDetails.First().Contacts;
            var paymentMethodDetail = ((VAA.EAPI.NDC.Order.Domain.OrderChangeRQ.PaymentProcessingPaymentCardType)paymentReq.PaymentMethod.Item);
            var transactionType = paymentMethodDetail.SecurePaymentVersion2.PaymentTrxChannelCode.ToString();
            var cardInfo = (CardInfo)((Domain.OrderChangeRQ.PaymentProcessingPaymentCardType)paymentReq.PaymentMethod.Item);
            var firstName = ((Domain.OrderChangeRQ.PayerIndividualType)paymentReq.Payer.PayerName.Item)?.GivenName.FirstOrDefault();
            var lastName = ((Domain.OrderChangeRQ.PayerIndividualType)paymentReq.Payer.PayerName.Item)?.Surname;
            cardInfo = MapCardHolderName(cardInfo, firstName, lastName);

            var paymentDetails = new List<Model.PaymentMS.PaymentDetails>()
            {
                new()
                {
                    Amount = new CurrencyAmount()
                    {
                         MonetaryAmount = amount,
                         BaseCurrency = paymentReq?.Amount?.CurCode.ToString(),
                         CurrencyId = currencies.First(a=>a.CurrencyCode == paymentReq.Amount.CurCode).CurrencyNumber,
                    },
                    CardMethod = cardInfo,
                    Payer = new PaymentPayer()
                    {
                        FirstName = firstName,
                        LastName = lastName,
                        PayerAddress = new Address()
                        {
                            AddressLine1 = paymentMethodDetail.CardholderAddress?.StreetText.FirstOrDefault(),
                            AddressLine2 = paymentMethodDetail.CardholderAddress?.StreetText.LastOrDefault(),
                            PostalCode = paymentMethodDetail.CardholderAddress?.PostalCode,
                            City = paymentMethodDetail.CardholderAddress?.CityName,
                            Country = paymentMethodDetail.CardholderAddress?.CountryCode,
                        },
                        PayerContacts = new Contacts()
                        {
                           Email =adtConactInfo?.Email,
                           Phone = adtConactInfo?.Phone,
                           CountryDailingCode = adtConactInfo?.CountryDailingCode?.Replace("+", "")
                        }
                    },
                    PaymentCardAuthentication = (VAA.EAPI.NDC.Order.Infrastructure.Model.PaymentMS.PaymentCardAuthentication)((VAA.EAPI.NDC.Order.Domain.OrderChangeRQ.PaymentProcessingPaymentCardType)paymentReq.PaymentMethod.Item),
                    TransactionTypeId = (transactionType == "MO") || (transactionType == "TO") ? 7 : 1009
                }
            };

            IEnumerable<EAPI.Domain.Virgin.Models.Products.Flight.OriginDestination> OriginDestinations = orderDocument?.DataList?.SegmentDetails?.Select(x => new EAPI.Domain.Virgin.Models.Products.Flight.OriginDestination { OriginAirport = x.OriginAirportCode, DestinationAirport = x.DestinationAirportCode }).ToList();

            return new PaymentMSAuthorizeCommand()
            {
                Pos = orderDocument.PointOfSale,
                Parties = orderDocument.Parties,
                BookingReferences = orderDocument.BookingReference,
                OrderId = orderDocument.OrderNumber.OrderId,
                PaymentDetails = paymentDetails,
                ProductType = productType,
                JourneyType = OriginDestinations.Count() == 1 ? "ONEWAY" : ((OriginDestinations.Count() == 2 && OriginDestinations.First().OriginAirport == OriginDestinations.Last().DestinationAirport && OriginDestinations.First().DestinationAirport == OriginDestinations.Last().OriginAirport) ? "RETURN" : "MULTICITY"),
                CustomerDetails = orderChangeReq?.Request?.DataLists?.PaxList?.Select(x =>
                {
                    var customerDetail = orderDocument.DataList.CustomerDetails.First(c => c.CustId == x.PaxID);

                    return new Customer
                    {
                        FirstName = customerDetail.FirstName,
                        LastName = customerDetail.LastName,
                        CustId = x.PaxID,
                        Ptc = x.PTC,
                        Contacts = new()
                        {
                            Email = adtConactInfo?.Email,
                        },
                        DateOfBirth = customerDetail.DateOfBirth
                    };
                }),
                Segments = orderDocument?.DataList?.SegmentDetails?.Select(x => new Segment
                {
                    ClassOfService = x.ClassOfService,
                    OriginAirportCode = x.OriginAirportCode,
                    DestinationAirportCode = x.DestinationAirportCode,
                    OriginCountryCode = listAirportDetails.FirstOrDefault(y => y.AirportCode == x.OriginAirportCode)?.CountryCode,
                    DestinationCountryCode = listAirportDetails.FirstOrDefault(y => y.AirportCode == x.DestinationAirportCode)?.CountryCode,
                    FareBasis = GetSegmentLevelFareBasis(orderDocument, x.SegmentId),
                    MarketingCarrier = x.MarketingCarrier,
                    MarketingFlightNumber = x.MarketingFlightNumber,
                    OperatingCarrier = x.OperatingCarrier ?? x.MarketingCarrier,
                    OperatingFlightNumber = x.OperatingFlightNumber ?? x.MarketingFlightNumber,
                    ScheduledArrivalTime = x.ScheduledArrivalTime,
                    ScheduledDepartureTime = x.ScheduledDepartureTime,
                    SegmentId = x.SegmentId
                }),
            };
        }

        public static PaymentMSCaptureCommand MapHoldBookingCaptureRequest(PaymentMSAuthorizeCommand authorizeRequest, PaymentRS paymentRS, OrderDocument orderDocument, IEnumerable<AccountableDocumentRef> accountableDocumentRefs, ProductType productType)
        {
            var payment = authorizeRequest.PaymentDetails.First();

            var paymentDetails = new List<Payment>()
            {
                new()
                {
                    PaymentTransactionId = paymentRS.PaymentDocument.TransactionId,
                    Amount = payment.Amount,
                    CardMethod = payment.CardMethod,
                    Payer = payment.Payer,
                    PaymentCardAuthentication = payment.PaymentCardAuthentication,
                }
            };

            IEnumerable<EAPI.Domain.Virgin.Models.Products.Flight.OriginDestination> OriginDestinations = orderDocument?.DataList?.SegmentDetails?.Select(x => new EAPI.Domain.Virgin.Models.Products.Flight.OriginDestination { OriginAirport = x.OriginAirportCode, DestinationAirport = x.DestinationAirportCode }).ToList();

            return new PaymentMSCaptureCommand()
            {
                Pos = authorizeRequest.Pos,
                Parties = authorizeRequest.Parties,
                BookingReferences = authorizeRequest.BookingReferences,
                OrderId = $"VS932{authorizeRequest.BookingReferences?.RLOC}{DateTime.UtcNow:yy}",
                PaymentDetails = paymentDetails,
                AccountableDocumentRefs = accountableDocumentRefs,
                ProductType = productType,
                JourneyType = OriginDestinations.Count() == 1 ? "ONEWAY" : ((OriginDestinations.Count() == 2 && OriginDestinations.First().OriginAirport == OriginDestinations.Last().DestinationAirport && OriginDestinations.First().DestinationAirport == OriginDestinations.Last().OriginAirport) ? "RETURN" : "MULTICITY"),
                CustomerDetails = authorizeRequest.CustomerDetails,
                Segments = authorizeRequest.Segments,
                OrderItems = orderDocument.OrderItems.Where(o => o.AccountableDocumentRefs != null && o.AccountableDocumentRefs.Any(a => accountableDocumentRefs.Any(b => b.DocumentNumber == a.DocumentNumber))).Select(oi =>
                {
                    return new Model.PaymentMS.OrderItems()
                    {
                        OrderItemId = oi.OrderItemId,
                        ProductType = productType,
                        AccountableDocumentRefIds = oi.AccountableDocumentRefs.Select(a => a.DocumentNumber),
                        CustRefIds = oi.Services.SelectMany(s => s.ServiceBeneficiaries).Distinct(),
                        SegmentRefIds = oi.Services.SelectMany(s => s.SegmentRefIds)
                    };
                })
            };
        }

        public static PaymentMSAuthorizeCommand MapOrderChangeAuthorizeRequest(OrderDocument orderDocument, IATA_OrderChangeRQ orderChangeRQ, IEnumerable<Currency> currencyReferenceMSResponse, OfferResponse offerDocResponse, IEnumerable<AirportDetail> airportDetails)
        {
            var paymentProcessingInfo = orderChangeRQ.Request.PaymentFunctions.First().PaymentProcessingDetails;
            var paymentMethodDetail = ((Domain.OrderChangeRQ.PaymentProcessingPaymentCardType)paymentProcessingInfo.PaymentMethod.Item);
            var transactionType = paymentMethodDetail.SecurePaymentVersion2.PaymentTrxChannelCode.ToString();
            var adtConactInfo = orderDocument.DataList.CustomerDetails.First().Contacts;
            var cardInfo = (CardInfo)((Domain.OrderChangeRQ.PaymentProcessingPaymentCardType)paymentProcessingInfo.PaymentMethod.Item);
            var firstName = ((Domain.OrderChangeRQ.PayerIndividualType)paymentProcessingInfo.Payer.PayerName.Item)?.GivenName.FirstOrDefault();
            var lastName = ((Domain.OrderChangeRQ.PayerIndividualType)paymentProcessingInfo.Payer.PayerName.Item)?.Surname;
            cardInfo = MapCardHolderName(cardInfo, firstName, lastName);

            var paymentDetails = new List<Model.PaymentMS.PaymentDetails>()
            {
                new()
                {
                    Amount = new CurrencyAmount()
                    {
                         MonetaryAmount = paymentProcessingInfo?.Amount?.Value.ToString(),
                         BaseCurrency = paymentProcessingInfo?.Amount?.CurCode.ToString(),
                         CurrencyId = currencyReferenceMSResponse.First(a=>a.CurrencyCode == paymentProcessingInfo.Amount.CurCode).CurrencyNumber,
                    },
                    CardMethod = cardInfo,
                    Payer = new PaymentPayer()
                    {
                        FirstName = firstName,
                        LastName = lastName,
                        PayerAddress = new Address()
                        {
                            AddressLine1 = paymentMethodDetail.CardholderAddress?.StreetText.FirstOrDefault(),
                            AddressLine2 = paymentMethodDetail.CardholderAddress?.StreetText.LastOrDefault(),
                            PostalCode = paymentMethodDetail.CardholderAddress?.PostalCode,
                            City = paymentMethodDetail.CardholderAddress?.CityName,
                            Country = paymentMethodDetail.CardholderAddress?.CountryCode,
                        },
                        PayerContacts = new Contacts()
                        {
                           Email =adtConactInfo?.Email,
                           Phone = adtConactInfo?.Phone,
                           CountryDailingCode = adtConactInfo?.CountryDailingCode?.Replace("+", "")
                        }
                    },
                    PaymentCardAuthentication = (transactionType == "MO") || (transactionType == "TO")? null: (Model.PaymentMS.PaymentCardAuthentication)((Domain.OrderChangeRQ.PaymentProcessingPaymentCardType)paymentProcessingInfo.PaymentMethod.Item),
                    TransactionTypeId = (transactionType == "MO") || (transactionType == "TO") ? 7 : 1009
                }
            };

            IEnumerable<EAPI.Domain.Virgin.Models.Products.Flight.OriginDestination> OriginDestinations = orderDocument?.DataList?.SegmentDetails?.Select(x => new EAPI.Domain.Virgin.Models.Products.Flight.OriginDestination { OriginAirport = x.OriginAirportCode, DestinationAirport = x.DestinationAirportCode }).ToList();

            var offerSegmentsByFlightSegmentRefId = offerDocResponse?.DataList?.OfferSegments?
                .Where(os => !string.IsNullOrEmpty(os.FlightSegmentRefId))
                .GroupBy(os => os.FlightSegmentRefId.Trim('F'))
                .ToDictionary(g => g.Key, g => g.First());

            return new PaymentMSAuthorizeCommand()
            {
                Pos = orderDocument.PointOfSale,
                Parties = orderDocument.Parties,
                BookingReferences = new EAPI.Domain.Virgin.Models.Order.BookingReference { RLOC = orderChangeRQ.Request.BookingRef.First().BookingID },
                OrderId = orderChangeRQ.Request.Order.OrderID,
                PaymentDetails = paymentDetails,
                ProductType = Constants.ProductType.Ticket,
                JourneyType = OriginDestinations.Count() == 1 ? "ONEWAY" : ((OriginDestinations.Count() == 2 && OriginDestinations.First().OriginAirport == OriginDestinations.Last().DestinationAirport && OriginDestinations.First().DestinationAirport == OriginDestinations.Last().OriginAirport) ? "RETURN" : "MULTICITY"),
                CustomerDetails = orderChangeRQ?.Request?.DataLists?.PaxList?.Select(x =>
                {
                    var matchingCustomer = orderDocument.DataList.CustomerDetails.First(c => c.CustId == x.PaxID);

                    return new Customer
                    {
                        FirstName = matchingCustomer.FirstName,
                        LastName = matchingCustomer.LastName,
                        CustId = x.PaxID,
                        Ptc = x.PTC,
                        Contacts = new()
                        {
                            Email = adtConactInfo?.Email,
                        },
                        DateOfBirth = matchingCustomer.DateOfBirth

                    };
                }),
                Segments = offerDocResponse?.DataList?.FlightSegments?.Select(flightSegment =>
                {
                    OfferSegment matchingOfferSegment = null;
                    var normalizedSegmentId = flightSegment.SegmentId.Trim('F');
                    offerSegmentsByFlightSegmentRefId?.TryGetValue(normalizedSegmentId, out matchingOfferSegment);

                    return new Segment
                    {
                        SegmentId = normalizedSegmentId,
                        MarketingCarrier = flightSegment.MarketingCarrier,
                        MarketingFlightNumber = flightSegment.MarketingFlightNumber,
                        OperatingCarrier = flightSegment.OperatingCarrier ?? flightSegment.MarketingCarrier,
                        OperatingFlightNumber = flightSegment.OperatingFlightNumber ?? flightSegment.MarketingFlightNumber,
                        OriginAirportCode = flightSegment.OriginAirportCode,
                        DestinationAirportCode = flightSegment.DestinationAirportCode,
                        OriginCountryCode = airportDetails.FirstOrDefault(y => y.AirportCode == flightSegment.OriginAirportCode)?.CountryCode,
                        DestinationCountryCode = airportDetails.FirstOrDefault(y => y.AirportCode == flightSegment.DestinationAirportCode)?.CountryCode,
                        ClassOfService = matchingOfferSegment?.ClassOfService,
                        ScheduledArrivalTime = flightSegment.ScheduledArrivalTime,
                        ScheduledDepartureTime = flightSegment.ScheduledDepartureTime,
                        FareBasis = GetSegmentLevelFareBasis(offerDocResponse, flightSegment.SegmentId),
                        ConnectionOrStopOver = flightSegment.ConnectionOrStopOver,
                        CabinType = matchingOfferSegment?.CabinType
                    };
                }),
            };
        }

        public static PaymentMSCaptureCommand MapOrderChangeCaptureRequest(PaymentMSAuthorizeCommand authorizeRequest, OrderDocument orderDocument, PaymentRS paymentRS, OfferResponse offerDocResponse)
        {
            var paymentInfo = authorizeRequest.PaymentDetails.First();

            var paymentDetails = new List<Payment>()
            {
                new()
                {
                    PaymentTransactionId = paymentRS.PaymentDocument.TransactionId,
                    Amount = paymentInfo.Amount,
                    CardMethod = paymentInfo.CardMethod,
                    Payer = paymentInfo.Payer,
                    PaymentCardAuthentication = paymentInfo.PaymentCardAuthentication,
                }
            };

            IEnumerable<EAPI.Domain.Virgin.Models.Products.Flight.OriginDestination> OriginDestinations = offerDocResponse?.DataList?.FlightSegments?.Select(x => new EAPI.Domain.Virgin.Models.Products.Flight.OriginDestination { OriginAirport = x.OriginAirportCode, DestinationAirport = x.DestinationAirportCode }).ToList();

            return new PaymentMSCaptureCommand()
            {
                Pos = authorizeRequest.Pos,
                Parties = authorizeRequest.Parties,
                BookingReferences = authorizeRequest.BookingReferences,
                OrderId = $"VS932{authorizeRequest.BookingReferences?.RLOC}{DateTime.UtcNow:yy}",
                PaymentDetails = paymentDetails,
                CustomerDetails = authorizeRequest.CustomerDetails,
                AccountableDocumentRefs = orderDocument.OrderItems.SelectMany(o => o.AccountableDocumentRefs),
                ProductType = Constants.ProductType.Ticket,
                JourneyType = OriginDestinations.Count() == 1 ? "ONEWAY" : ((OriginDestinations.Count() == 2 && OriginDestinations.First().OriginAirport == OriginDestinations.Last().DestinationAirport && OriginDestinations.First().DestinationAirport == OriginDestinations.Last().OriginAirport) ? "RETURN" : "MULTICITY"),
                Segments = authorizeRequest.Segments,
                OrderItems = orderDocument.OrderItems.Select(oi =>
                {
                    return new Model.PaymentMS.OrderItems()
                    {
                        OrderItemId = oi.OrderItemId,
                        ProductType = Constants.ProductType.Ticket,
                        AccountableDocumentRefIds = oi.AccountableDocumentRefs.Select(a => a.DocumentNumber),
                        CustRefIds = oi.Services.SelectMany(s => s.ServiceBeneficiaries).Distinct(),
                        SegmentRefIds = oi.Services.SelectMany(s => s.SegmentRefIds)
                    };
                })
            };
        }

        public static PaymentMSRefundCommand MapOrderChangeUnevenRefundRequest(OrderDocument orderDocument, OrderRS orderRS)
        {
            var monetaryAmount = orderDocument.OrderItems.Select(orderItems => orderItems.RefundPrice.First().Amount.MonetaryAmount).Select(value => value.Parse<decimal>()).Sum();
            var RefundpaymentDetails = new List<RefundPayment>()
            {
                new()
                {
                    PaymentTransactionId = orderDocument.OrderItems.First().PaymentDetails.First().PaymentReferenceId,
                    RefundAmount = new RefundAmount()
                    {
                        BaseCurrency = orderDocument.OrderItems.First().RefundPrice.First().Amount.BaseCurrency,
                        MonetaryAmount = Convert.ToString(Math.Abs(monetaryAmount)),
                     },
                }
             };

            return new PaymentMSRefundCommand()
            {
                OrderId = $"VS932{orderDocument.BookingReference?.RLOC}{DateTime.UtcNow:yy}",
                Pos = orderRS.OrderDocument.PointOfSale,
                BookingReferences = new EAPI.Domain.Virgin.Models.Order.BookingReference { RLOC = orderDocument.BookingReference.RLOC },
                PaymentDetails = RefundpaymentDetails,
                AccountableDocumentRefs = orderDocument.OrderItems.Select(orderItems => orderItems.AccountableDocumentRefs).Select(docRef => new AccountableDocumentRef()
                {
                    DocumentNumber = docRef.First().DocumentNumber,

                }),
            };
        }

        private static string GetSegmentLevelFareBasis(OrderDocument orderDocument, string flightSegmentId)
        {
            var priceClassRefId = orderDocument.OrderItems.First().Services.First(x => x.SegmentRefIds.Contains(flightSegmentId)).PriceClassRefIds.First();

            return orderDocument.DataList.PriceClass.First(x => x.Id == priceClassRefId).FareBasisCode;
        }

        private static string GetSegmentLevelFareBasis(OfferResponse offerResponse, string flightSegmentId)
        {
            var journeyId = offerResponse.DataList.Journey.First(x => x.FlightSegmentRefIds.Contains(flightSegmentId)).JourneyId;
            var priceClassRefId = offerResponse.Offers.First().OfferItems.First().Services.First(x => x.JourneyRefIds.Contains(journeyId)).PriceClassRefIds.First();

            return offerResponse.DataList.PriceClass.First(x => x.Id == priceClassRefId).FareBasisCode;
        }

        public static PaymentMSRefundCommand MapPaymentMSRefundRequest(OrderRS orderRS, IATA_OrderCreateRQ OrderCreateRQ, IEnumerable<Currency> currencyReferenceMSResponse, string amount, PaymentRS paymentRS, IEnumerable<AccountableDocumentRef> accountableDocumentRefs,  Domain.OrderCreateRQ.PaymentProcessingDetailsType paymentProcessingDetails = null)
        {
            var paymentRq = paymentProcessingDetails ?? OrderCreateRQ.Request.PaymentFunctions.First().PaymentProcessingDetails;
            var RefundpaymentDetails = new List<RefundPayment>()
            {
                new()
                {
                PaymentTransactionId = paymentRS.PaymentDocument.TransactionId,
                FormOfPayments = (Model.PaymentMS.PaymentDetails.FormOfPayment?)EAPI.Domain.Virgin.Models.Payment.PaymentDetails.FormOfPayment.CardEnum,
                RefundAmount = new RefundAmount()
                {
                    BaseCurrency = paymentRq?.Amount?.CurCode.ToString(),
                    CurrencyId = currencyReferenceMSResponse.First(a=>a.CurrencyCode == paymentRq.Amount.CurCode).CurrencyNumber,
                    MonetaryAmount = amount,
                },
                CardType = ((VAA.EAPI.NDC.Order.Domain.OrderCreateRQ.PaymentProcessingPaymentCardType)paymentRq.PaymentMethod.Item).CardTypeText
                }
            };

            return new PaymentMSRefundCommand()
            {
                Pos = orderRS.OrderDocument.PointOfSale,
                Parties = orderRS.OrderDocument.Parties,
                BookingReferences = orderRS.OrderDocument.BookingReference,
                OrderId = $"VS932{orderRS.OrderDocument.BookingReference?.RLOC}{DateTime.UtcNow:yy}",
                PaymentDetails = RefundpaymentDetails,

                AccountableDocumentRefs = accountableDocumentRefs ?? null,

            };

        }

        public static PaymentMSRefundCommand MapPaymentMSOrderChangeRefundRequest(OrderRS orderRS, IATA_OrderChangeRQ OrderChangeRQ, IEnumerable<Currency> currencyReferenceMSResponse, string amount, PaymentRS paymentRS, IEnumerable<AccountableDocumentRef> accountableDocumentRefs)
        {
            var paymentRq = OrderChangeRQ.Request.PaymentFunctions.First().PaymentProcessingDetails;
            var RefundpaymentDetails = new List<RefundPayment>()
            {
                new()
                {
                PaymentTransactionId = paymentRS.PaymentDocument.TransactionId,
                FormOfPayments = (Model.PaymentMS.PaymentDetails.FormOfPayment?)EAPI.Domain.Virgin.Models.Payment.PaymentDetails.FormOfPayment.CardEnum,
                RefundAmount = new RefundAmount()
                {
                    BaseCurrency = paymentRq?.Amount?.CurCode.ToString(),
                    CurrencyId = currencyReferenceMSResponse.First(a=>a.CurrencyCode == paymentRq.Amount.CurCode).CurrencyNumber,
                    MonetaryAmount = amount,
                },
                CardType = ((VAA.EAPI.NDC.Order.Domain.OrderChangeRQ.PaymentProcessingPaymentCardType)paymentRq.PaymentMethod.Item).CardTypeText
                }
            };

            return new PaymentMSRefundCommand()
            {
                Pos = orderRS.OrderDocument.PointOfSale,
                Parties = orderRS.OrderDocument.Parties,
                BookingReferences = orderRS.OrderDocument.BookingReference,
                OrderId = $"VS932{orderRS.OrderDocument.BookingReference?.RLOC}{DateTime.UtcNow:yy}",
                PaymentDetails = RefundpaymentDetails,

                AccountableDocumentRefs = accountableDocumentRefs ?? null,

            };

        }


        public static PaymentMSAuthorizeCommand MapPostSaleGDSAuthorizeRequest(OrderSearchResponse orderRS, IATA_OrderChangeRQ orderChangeReq, IEnumerable<AirportDetail> listAirportDetails, string amount)
        {

            var paymentReq = orderChangeReq.Request.PaymentFunctions.First().PaymentProcessingDetails;
            var adtContactInfoRefIds = orderChangeReq?.Request?.DataLists?.PaxList?.Where(x => x.PTC.ToUpper() == "ADT").Select(x => x.ContactInfoRefID);
            var adtConactInfo = orderChangeReq?.Request?.DataLists?.ContactInfoList?.Where(x => adtContactInfoRefIds.Contains(x.ContactInfoID)).SelectMany(x => x.EmailAddress.Select(y => new { y.EmailAddressText, Phone = x.Phone?.FirstOrDefault()?.PhoneNumber, CountryDialingCode = x.Phone?.FirstOrDefault()?.CountryDialingCode?.Replace("+", "") })).First(x => x.EmailAddressText != null);
            var paymentMethodDetail = ((VAA.EAPI.NDC.Order.Domain.OrderChangeRQ.PaymentProcessingPaymentCardType)paymentReq.PaymentMethod.Item);
            var transactionType = paymentMethodDetail.SecurePaymentVersion2.PaymentTrxChannelCode.ToString();
            var participant = orderChangeReq.DistributionChain.OrderByDescending(x => x.Ordinal).FirstOrDefault();
            var cardInfo = (CardInfo)((VAA.EAPI.NDC.Order.Domain.OrderChangeRQ.PaymentProcessingPaymentCardType)paymentReq.PaymentMethod.Item);
            var firstName = ((Domain.OrderChangeRQ.PayerIndividualType)paymentReq.Payer.PayerName.Item)?.GivenName.FirstOrDefault();
            var lastName = ((Domain.OrderChangeRQ.PayerIndividualType)paymentReq.Payer.PayerName.Item)?.Surname;
            cardInfo = MapCardHolderName(cardInfo, firstName, lastName);

            var paymentDetails = new List<Model.PaymentMS.PaymentDetails>()
            {
                new()
                {
                    Amount = new CurrencyAmount()
                    {
                         MonetaryAmount = amount,
                         BaseCurrency = paymentReq?.Amount?.CurCode.ToString(),

                    },
                    CardMethod = cardInfo,
                    Payer = new PaymentPayer()
                    {
                        FirstName = firstName,
                        LastName = lastName,
                        PayerAddress = new Address()
                        {
                            AddressLine1 = paymentMethodDetail.CardholderAddress?.StreetText.FirstOrDefault(),
                            AddressLine2 = paymentMethodDetail.CardholderAddress?.StreetText.LastOrDefault(),
                            PostalCode = paymentMethodDetail.CardholderAddress?.PostalCode,
                            City = paymentMethodDetail.CardholderAddress?.CityName,
                            Country = paymentMethodDetail.CardholderAddress?.CountryCode,
                        },
                        PayerContacts = new Contacts()
                        {
                           Email =adtConactInfo?.EmailAddressText,
                            Phone = adtConactInfo?.Phone,
                            CountryDailingCode = adtConactInfo?.CountryDialingCode
                        }
                    },
                    PaymentCardAuthentication = (VAA.EAPI.NDC.Order.Infrastructure.Model.PaymentMS.PaymentCardAuthentication)((VAA.EAPI.NDC.Order.Domain.OrderChangeRQ.PaymentProcessingPaymentCardType)paymentReq.PaymentMethod.Item),
                    TransactionTypeId = (transactionType == "MO") || (transactionType == "TO") ? 7 : 1009
                }
            };

            IEnumerable<EAPI.Domain.Virgin.Models.Products.Flight.OriginDestination> OriginDestinations = orderRS?.SegmentDetails?.Select(x => new EAPI.Domain.Virgin.Models.Products.Flight.OriginDestination { OriginAirport = x.OriginAirportCode, DestinationAirport = x.DestinationAirportCode }).ToList();

            return new PaymentMSAuthorizeCommand()
            {
                Pos = new Pos()
                {
                    CountryCode = orderRS.PointOfSale.CountryCode,
                    CurrencyCode = orderRS.PointOfSale.CurrencyCode
                },
                Parties = new Model.Parties()
                {
                    Participants = new List<VAA.EAPI.NDC.Order.Infrastructure.Model.Participant>()
                    {
                        new VAA.EAPI.NDC.Order.Infrastructure.Model.Participant()
                        {
                            PartyType = (Model.Participant.PartyTypeEnum?)Participant.PartyTypeEnum.Seller,
                            Name = participant.ParticipatingOrg.Name,
                            AgencyID = participant.ParticipatingOrg.OrgID,
                            Ordinal = participant.Ordinal.ToString()
                        }
                    }
                },
                BookingReferences = orderRS.BookingReferences,
                OrderId = $"VS932{orderRS.BookingReferences?.RLOC}{DateTime.UtcNow:yy}",
                PaymentDetails = paymentDetails,
                ProductType = Constants.ProductType.Ticket,
                JourneyType = OriginDestinations.Count() == 1 ? "ONEWAY" : ((OriginDestinations.Count() == 2 && OriginDestinations.First().OriginAirport == OriginDestinations.Last().DestinationAirport && OriginDestinations.First().DestinationAirport == OriginDestinations.Last().OriginAirport) ? "RETURN" : "MULTICITY"),
                CustomerDetails = orderChangeReq?.Request?.DataLists?.PaxList?.Select(x => new Customer
                {
                    FirstName = x.Individual.GivenName.FirstOrDefault(),
                    LastName = x.Individual.Surname,
                    Ptc = x.PTC,
                    Contacts = new()
                    {
                        Email = adtConactInfo?.EmailAddressText,
                    },
                    DateOfBirth = orderRS.CustomerDetails.First(c => c.CustId == x.PaxID).DateOfBirth
                }),
                Segments = orderRS?.SegmentDetails?.Select(x => new Segment
                {
                    ClassOfService = x.ClassOfService,
                    OriginAirportCode = x.OriginAirportCode,
                    DestinationAirportCode = x.DestinationAirportCode,
                    OriginCountryCode = listAirportDetails.FirstOrDefault(y => y.AirportCode == x.OriginAirportCode)?.CountryCode,
                    DestinationCountryCode = listAirportDetails.FirstOrDefault(y => y.AirportCode == x.DestinationAirportCode)?.CountryCode,
                    MarketingCarrier = x.MarketingCarrier,
                    MarketingFlightNumber = x.MarketingFlightNumber,
                    OperatingCarrier = x.OperatingCarrier ?? x.MarketingCarrier,
                    OperatingFlightNumber = x.OperatingFlightNumber ?? x.MarketingFlightNumber,
                    ScheduledArrivalTime = x.ScheduledArrivalTime,
                    ScheduledDepartureTime = x.ScheduledDepartureTime,
                }),
            };
        }

        public static PaymentMSCaptureCommand MapPostSaleGDSCaptureRequest(PaymentMSAuthorizeCommand authorizeRequest, PaymentRS paymentRS, OrderSearchResponse orderRS, IEnumerable<AccountableDocumentRef> accountableDocumentRefs)
        {
            var payment = authorizeRequest.PaymentDetails.First();

            var paymentDetails = new List<Payment>()
            {
                new()
                {
                    PaymentTransactionId = paymentRS.PaymentDocument.TransactionId,
                    Amount = payment.Amount,
                    CardMethod = payment.CardMethod,
                    Payer = payment.Payer,
                    PaymentCardAuthentication = payment.PaymentCardAuthentication,
                }
            };

            IEnumerable<EAPI.Domain.Virgin.Models.Products.Flight.OriginDestination> OriginDestinations = orderRS?.SegmentDetails?.Select(x => new EAPI.Domain.Virgin.Models.Products.Flight.OriginDestination { OriginAirport = x.OriginAirportCode, DestinationAirport = x.DestinationAirportCode }).ToList();

            return new PaymentMSCaptureCommand()
            {
                Pos = authorizeRequest.Pos,
                Parties = authorizeRequest.Parties,
                BookingReferences = authorizeRequest.BookingReferences,
                OrderId = $"VS932{authorizeRequest.BookingReferences?.RLOC}{DateTime.UtcNow:yy}",
                PaymentDetails = paymentDetails,
                AccountableDocumentRefs = accountableDocumentRefs,
                ProductType = Constants.ProductType.Ticket,
                JourneyType = OriginDestinations.Count() == 1 ? "ONEWAY" : ((OriginDestinations.Count() == 2 && OriginDestinations.First().OriginAirport == OriginDestinations.Last().DestinationAirport && OriginDestinations.First().DestinationAirport == OriginDestinations.Last().OriginAirport) ? "RETURN" : "MULTICITY"),
                CustomerDetails = authorizeRequest.CustomerDetails,
                Segments = authorizeRequest.Segments,
            };
        }

        public static PaymentMSRefundCommand MapPostSaleGDSRefundRequest(OrderSearchResponse orderRS, IATA_OrderChangeRQ orderChangeRQ, IEnumerable<AccountableDocumentRef> accountableDocumentRefs, PaymentRS paymentRS, PaymentMSAuthorizeCommand authorizeRequest)
        {
            var paymentRq = orderChangeRQ.Request.PaymentFunctions?.FirstOrDefault()?.PaymentProcessingDetails;
            var RefundpaymentDetails = new List<RefundPayment>()
            {
                new()
                {
                PaymentTransactionId = paymentRS.PaymentDocument.TransactionId,
                FormOfPayments = (Model.PaymentMS.PaymentDetails.FormOfPayment?)EAPI.Domain.Virgin.Models.Payment.PaymentDetails.FormOfPayment.CardEnum,
                RefundAmount = new RefundAmount()
                {
                    BaseCurrency = Convert.ToString(paymentRq?.Amount?.CurCode),
                    MonetaryAmount = paymentRS.PaymentDocument.Amount.ToString(),
                    CurrencyId = authorizeRequest.PaymentDetails.First().Amount.CurrencyId
                },
                CardType = ((VAA.EAPI.NDC.Order.Domain.OrderChangeRQ.PaymentProcessingPaymentCardType)paymentRq.PaymentMethod.Item).CardTypeText
                }
            };

            return new PaymentMSRefundCommand()
            {
                Pos = new()
                {
                    CityCode = orderRS.PointOfSale?.CityCode,
                    CurrencyCode = orderRS.PointOfSale?.CurrencyCode,
                    CountryCode = orderRS.PointOfSale?.CountryCode
                },
                BookingReferences = orderRS.BookingReferences,
                OrderId = $"VS932{orderRS.BookingReferences?.RLOC}{DateTime.UtcNow:yy}",
                PaymentDetails = RefundpaymentDetails,
                AccountableDocumentRefs = accountableDocumentRefs ?? null,
            };
        }

        public static PaymentMSRefundCommand MapOrderChangeRefundRequest(OrderRS orderRS, IATA_OrderChangeRQ orderChangeRQ, IEnumerable<AccountableDocumentRef> accountableDocumentRefs, PaymentRS paymentRS, PaymentMSAuthorizeCommand authorizeRequest)
        {
            var paymentRq = orderChangeRQ.Request.PaymentFunctions?.FirstOrDefault()?.PaymentProcessingDetails;
            var RefundpaymentDetails = new List<RefundPayment>()
            {
                new()
                {
                PaymentTransactionId = paymentRS.PaymentDocument.TransactionId,
                FormOfPayments = (Model.PaymentMS.PaymentDetails.FormOfPayment?)EAPI.Domain.Virgin.Models.Payment.PaymentDetails.FormOfPayment.CardEnum,
                RefundAmount = new RefundAmount()
                {
                    BaseCurrency = Convert.ToString(paymentRq?.Amount?.CurCode),
                    MonetaryAmount = paymentRS.PaymentDocument.Amount.ToString(),
                    CurrencyId = authorizeRequest.PaymentDetails.First().Amount.CurrencyId
                },
                CardType = ((VAA.EAPI.NDC.Order.Domain.OrderChangeRQ.PaymentProcessingPaymentCardType)paymentRq.PaymentMethod.Item).CardTypeText
                }
            };

            return new PaymentMSRefundCommand()
            {
                Pos = orderRS.OrderDocument.PointOfSale,
                Parties = orderRS.OrderDocument.Parties,
                BookingReferences = orderRS.OrderDocument.BookingReference,
                OrderId = $"VS932{orderRS.OrderDocument.BookingReference?.RLOC}{DateTime.UtcNow:yy}",
                PaymentDetails = RefundpaymentDetails,
                AccountableDocumentRefs = accountableDocumentRefs ?? null,
            };
        }

        public static PaymentMSRefundCommand MapOrderChangeCancelRefundRequest(OrderDocument orderDocument, CancelReshopResponse offerDoc, decimal refundAmount, string PaymentReferenceId, string orderID)
        {
            var refundpaymentDetails = new List<RefundPayment>()
            {
                new()
                {
                    RefundAmount = new RefundAmount()
                    {
                        BaseCurrency = orderDocument.PointOfSale.CurrencyCode,
                        MonetaryAmount = refundAmount.ToString()
                    },
                    PaymentTransactionId = PaymentReferenceId
                }
            };

            return new PaymentMSRefundCommand()
            {
                Pos = new Pos()
                {
                    CountryCode = orderDocument.PointOfSale.CountryCode,
                    CurrencyCode = orderDocument.PointOfSale.CurrencyCode
                },
                BookingReferences = new EAPI.Domain.Virgin.Models.Order.BookingReference()
                {
                    RLOC = orderID[5..^2],
                },
                OrderId = orderID,
                PaymentDetails = refundpaymentDetails
            };
        }

        public static SplitPnrPaymentRequest MapSplitPnrPaymentRequest(OrderDocument parentOrderDocument, OrderDocument childOrderDocument, IEnumerable<string> splitPaxIds)
        {
            return new SplitPnrPaymentRequest()
            {
                OrderId = parentOrderDocument.Id,
                ParentBookingReference = parentOrderDocument.BookingReference,
                ChildBookingReference = childOrderDocument.BookingReference,
                SplitPaxIds = splitPaxIds
            };
        }

        public static PaymentIFGMSAuthorizeCommand MapOrderCreateIFGAuthorizeRequest(IATA_OrderCreateRQ OrderCreateRQ, OfferResponse offerDocResponse, IEnumerable<AirportDetail> listAirportDetails, string amount, ProductType productType, [Optional] bool isEasyPay)
        {
            return new PaymentIFGMSAuthorizeCommand()
            {
                Pos = offerDocResponse.Pos,
                ProductType = productType,
                Parties = new VAA.EAPI.NDC.Order.Infrastructure.Model.Parties
                {
                    Participants = offerDocResponse.Parties.Participants.Select(p => new VAA.EAPI.NDC.Order.Infrastructure.Model.Participant
                    {
                        PartyType = p.PartyType,
                        Name = p.Name,
                        AgencyID = !string.IsNullOrEmpty(p.AgencyID) && p.AgencyID.Length > 1 ? p.AgencyID.Substring(0, p.AgencyID.Length - 1) : p.AgencyID,
                        Ordinal = p.Ordinal,
                        AccountCode = p.AccountCode,
                        PccCode = p.PccCode,
                        IsCreditCardAccepted = p.IsCreditCardAccepted
                    })
                },
                PaymentDetails = new List<Model.PaymentMS.PaymentDetails>()
                {
                   new ()
                   {
                       FormOfPayments = isEasyPay? Model.PaymentMS.PaymentDetails.FormOfPayment.EasyPay : Model.PaymentMS.PaymentDetails.FormOfPayment.CashEnum,
                       Amount = new()
                       {
                            MonetaryAmount = decimal.TryParse(amount, out var parsedAmount) ? ((int)(parsedAmount * 100)).ToString(): null,
                            BaseCurrency = offerDocResponse.Offers?.FirstOrDefault()?.SalePrice.FirstOrDefault()?.Amount?.BaseCurrency+ "2",
                       },
                       CardMethod = isEasyPay? GetCardDetails(OrderCreateRQ) : null
                   }
                },
                Segments = offerDocResponse?.DataList?.FlightSegments?.Select(flightSegment =>
                {
                    var matchingOfferSegment = offerDocResponse.DataList.OfferSegments?.FirstOrDefault(os => os.FlightSegmentRefId.Trim('F') == flightSegment.SegmentId.Trim('F'));

                    return new Segment
                    {
                        SegmentId = flightSegment.SegmentId.Trim('F'),
                        MarketingCarrier = flightSegment.MarketingCarrier,
                        MarketingFlightNumber = flightSegment.MarketingFlightNumber,
                        OperatingCarrier = flightSegment.OperatingCarrier ?? flightSegment.MarketingCarrier,
                        OperatingFlightNumber = flightSegment.OperatingFlightNumber ?? flightSegment.MarketingFlightNumber,
                        OriginAirportCode = flightSegment.OriginAirportCode,
                        DestinationAirportCode = flightSegment.DestinationAirportCode,
                        OriginCountryCode = listAirportDetails?.FirstOrDefault(y => y.AirportCode == flightSegment.OriginAirportCode)?.CountryCode,
                        DestinationCountryCode = listAirportDetails?.FirstOrDefault(y => y.AirportCode == flightSegment.DestinationAirportCode)?.CountryCode,
                        ClassOfService = matchingOfferSegment?.ClassOfService,
                        ScheduledArrivalTime = flightSegment.ScheduledArrivalTime,
                        ScheduledDepartureTime = flightSegment.ScheduledDepartureTime,
                        FareBasis = GetSegmentLevelFareBasis(offerDocResponse, flightSegment.SegmentId),
                        ConnectionOrStopOver = flightSegment.ConnectionOrStopOver,
                        CabinType = matchingOfferSegment.CabinType
                    };
                }),
                CustomerDetails = OrderCreateRQ?.Request?.DataLists?.PaxList?.Select(x => new Customer
                {
                    FirstName = x.Individual.GivenName.FirstOrDefault(),
                    LastName = x.Individual.Surname,
                    Ptc = x.PTC,
                    CustId = x.PaxID,
                    DateOfBirth = x.Individual.Birthdate.ToString("yyyy-MM-dd")
                }),
            };
        }

        private static CardInfo GetCardDetails(IATA_OrderCreateRQ orderCreateRQ)
        {
            var easyPayRq = orderCreateRQ.Request?.PaymentFunctions?.FirstOrDefault()?.PaymentProcessingDetails?.PaymentMethod?.Item as VAA.EAPI.NDC.Order.Domain.OrderCreateRQ.PaymentProcessingIATA_EasyPayType;

            if (easyPayRq == null)
            {
                // Handle case where it's not the expected type
                return null;
            }

            return new CardInfo
            {
                CardNumber = easyPayRq.AccountNumber,
                ExpiryDate = easyPayRq.ExpirationDate
            };
        }

        public static PaymentIFGMSOrderCommand MapOrderCreateIFGOrderRequest(PaymentIFGMSAuthorizeCommand authorizeIFGRequest, IEnumerable<AccountableDocumentRef> accountableDocumentRefs, PaymentRS authorizeIFGResponse, OfferResponse offerDocResponse, IEnumerable<AirportDetail> listAirportDetails, OrderDocument orderDocument, BookingReference bookingReference, ProductType productType, [Optional] bool isEasyPay)
        {
            var currenyCode = offerDocResponse.Offers?.FirstOrDefault()?.SalePrice.FirstOrDefault()?.Amount?.BaseCurrency;

            return new PaymentIFGMSOrderCommand()
            {
                Pos = offerDocResponse.Pos,
                TrackId = authorizeIFGResponse.PaymentDocument.TrackId,
                ProductType = productType,
                Parties = authorizeIFGRequest.Parties,
                PaymentDetails = authorizeIFGRequest.PaymentDetails,
                Segments = authorizeIFGRequest.Segments,
                CustomerDetails = authorizeIFGRequest.CustomerDetails,
                BookingReferences = bookingReference,
                OrderId = $"VS932{bookingReference.RLOC}{DateTime.UtcNow:yy}",
                DataList = orderDocument.DataList,
                AccountableDocumentRef = orderDocument.OrderItems.Where(o => o.AccountableDocumentRefs != null && o.AccountableDocumentRefs.Any(a => accountableDocumentRefs.Any(b => b.DocumentNumber == a.DocumentNumber))).SelectMany(a => a.AccountableDocumentRefs),
                OrderItems = orderDocument.OrderItems
            };
        }

        public static PaymentIFGMSAuthorizeCommand MapOrderChangeExchangeIFGAuthorizeRequest(IATA_OrderChangeRQ request, OfferResponse offerDocResponse, IEnumerable<AirportDetail> airportDetails, OrderDocument orderDocument, [Optional] bool IsCardPayment, [Optional] bool IsEasyPay, [Optional] bool IsChangeReshopHoldBooking)
        {
            var adtConactInfo = orderDocument.DataList.CustomerDetails.First().Contacts;
            var paymentReq = request.Request.PaymentFunctions?.FirstOrDefault()?.PaymentProcessingDetails;
            var paymentMethodDetail = paymentReq?.PaymentMethod.Item as VAA.EAPI.NDC.Order.Domain.OrderChangeRQ.PaymentProcessingPaymentCardType;
            var easyPayRq = request.Request?.PaymentFunctions?.FirstOrDefault()?.PaymentProcessingDetails?.PaymentMethod?.Item as VAA.EAPI.NDC.Order.Domain.OrderChangeRQ.PaymentProcessingIATA_EasyPayType;
            var transactionType = paymentMethodDetail?.SecurePaymentVersion2?.PaymentTrxChannelCode.ToString();

            return new PaymentIFGMSAuthorizeCommand()
            {
                Pos = offerDocResponse.Pos,
                ProductType = Constants.ProductType.Ticket,
                Parties = new VAA.EAPI.NDC.Order.Infrastructure.Model.Parties
                {
                    Participants = offerDocResponse.Parties.Participants.Select(p => new VAA.EAPI.NDC.Order.Infrastructure.Model.Participant
                    {
                        PartyType = p.PartyType,
                        Name = p.Name,
                        AgencyID = !string.IsNullOrEmpty(p.AgencyID) && p.AgencyID.Length > 1 ? p.AgencyID.Substring(0, p.AgencyID.Length - 1) : p.AgencyID,
                        Ordinal = p.Ordinal,
                        AccountCode = p.AccountCode,
                        PccCode = p.PccCode,
                        IsCreditCardAccepted = p.IsCreditCardAccepted,
                        RequestAgentType = orderDocument.Parties.Participants.First().RequestAgentType,
                        Contacts = new()
                        {
                            Email = orderDocument.Parties.Participants.First(x => x.PartyType == Model.Participant.PartyTypeEnum.Seller).Contacts.Email,
                        }
                    })
                },
                PaymentDetails = new List<Model.PaymentMS.PaymentDetails>()
                {
                   new ()
                   {
                       FormOfPayments =  IsCardPayment ? Model.PaymentMS.PaymentDetails.FormOfPayment.CardEnum : IsEasyPay? Model.PaymentMS.PaymentDetails.FormOfPayment.EasyPay: Model.PaymentMS.PaymentDetails.FormOfPayment.CashEnum,
                       Amount = new()
                       {
                            MonetaryAmount = ((int)(request.Request.PaymentFunctions.First().PaymentProcessingDetails.Amount.Value * 100)).ToString(),
                            BaseCurrency = request.Request.PaymentFunctions.First().PaymentProcessingDetails.Amount?.CurCode + "2",
                       },
                       CardMethod = IsCardPayment? (CardInfo)((VAA.EAPI.NDC.Order.Domain.OrderChangeRQ.PaymentProcessingPaymentCardType)paymentReq.PaymentMethod.Item) : IsEasyPay? new CardInfo() { ExpiryDate = easyPayRq.ExpirationDate , CardNumber = easyPayRq.AccountNumber} : null,
                       Payer = IsCardPayment? new PaymentPayer()
                       {
                            FirstName = ((Domain.OrderChangeRQ.PayerIndividualType)paymentReq.Payer.PayerName.Item)?.GivenName.FirstOrDefault(),
                            LastName = ((Domain.OrderChangeRQ.PayerIndividualType)paymentReq.Payer.PayerName.Item)?.Surname,
                            PayerAddress = new Address()
                            {
                                AddressLine1 = paymentMethodDetail.CardholderAddress?.StreetText.FirstOrDefault(),
                                AddressLine2 = paymentMethodDetail.CardholderAddress?.StreetText.LastOrDefault(),
                                PostalCode = paymentMethodDetail.CardholderAddress?.PostalCode,
                                City = paymentMethodDetail.CardholderAddress?.CityName,
                                Country = paymentMethodDetail.CardholderAddress?.CountryCode,
                            },
                            PayerContacts = new Contacts()
                            {
                               Email =adtConactInfo?.Email,
                               Phone = adtConactInfo?.Phone,
                               CountryDailingCode = adtConactInfo?.CountryDailingCode?.Replace("+", "")

                            },
                       }:null,
                       PaymentCardAuthentication = IsCardPayment? (transactionType == "MO") || (transactionType == "TO")? null: (VAA.EAPI.NDC.Order.Infrastructure.Model.PaymentMS.PaymentCardAuthentication)((VAA.EAPI.NDC.Order.Domain.OrderChangeRQ.PaymentProcessingPaymentCardType)paymentReq.PaymentMethod.Item) : null,
                       TransactionTypeId = IsCardPayment ?(transactionType == "MO") || (transactionType == "TO") ? 7 : 1009 :null
                   }
                },
                Segments = offerDocResponse?.DataList?.FlightSegments?.Select(flightSegment =>
                {
                    var matchingOfferSegment = offerDocResponse.DataList.OfferSegments?.FirstOrDefault(os => os.FlightSegmentRefId.Trim('F') == flightSegment.SegmentId.Trim('F'));

                    return new Segment
                    {
                        SegmentId = flightSegment.SegmentId.Trim('F'),
                        MarketingCarrier = flightSegment.MarketingCarrier,
                        MarketingFlightNumber = flightSegment.MarketingFlightNumber,
                        OperatingCarrier = flightSegment.OperatingCarrier ?? flightSegment.MarketingCarrier,
                        OperatingFlightNumber = flightSegment.OperatingFlightNumber ?? flightSegment.MarketingFlightNumber,
                        OriginAirportCode = flightSegment.OriginAirportCode,
                        DestinationAirportCode = flightSegment.DestinationAirportCode,
                        OriginCountryCode = airportDetails.FirstOrDefault(y => y.AirportCode == flightSegment.OriginAirportCode)?.CountryCode,
                        DestinationCountryCode = airportDetails.FirstOrDefault(y => y.AirportCode == flightSegment.DestinationAirportCode)?.CountryCode,
                        ClassOfService = matchingOfferSegment?.ClassOfService,
                        ScheduledArrivalTime = flightSegment.ScheduledArrivalTime,
                        ScheduledDepartureTime = flightSegment.ScheduledDepartureTime,
                        FareBasis = GetSegmentLevelFareBasis(offerDocResponse, flightSegment.SegmentId),
                        ConnectionOrStopOver = flightSegment.ConnectionOrStopOver,
                        CabinType = matchingOfferSegment?.CabinType
                    };
                }),
                CustomerDetails = orderDocument.DataList.CustomerDetails.Select(x => new Customer
                {
                    FirstName = x.FirstName,
                    LastName = x.LastName,
                    Ptc = x.Ptc,
                    CustId = x.CustId,
                    DateOfBirth = x.DateOfBirth
                }),
                BookingReferences = new BookingReference()
                {
                    RLOC = orderDocument.BookingReference.RLOC
                },
                Offers = offerDocResponse?.Offers,
                IsChangeReshopHoldBooking = IsChangeReshopHoldBooking
            };
        }

        public static PaymentIFGMSExchangeCommand MapOrderChangeExchangeIFGOrderExchangeRequest(IATA_OrderChangeRQ request, OfferResponse offerDocResponse, IEnumerable<AirportDetail> airportDetails, OrderDocument orderManagementRS, OrderDocument orderRS, [Optional] PaymentRS paymentIFGAuthorizeRS, [Optional] bool isRefund, bool isCardPayment, bool isEasyPay)
        {
            var currenyCode = offerDocResponse.Offers?.FirstOrDefault()?.SalePrice.FirstOrDefault()?.Amount?.BaseCurrency;
            var baseCurrency = offerDocResponse.Offers?.FirstOrDefault()?.OfferItems.FirstOrDefault()?.OfferitemPricing.FirstOrDefault().OriginalSalePrice?.FirstOrDefault(x => x.PriceComponentType == PriceComponentTypes.BaseFareEnum).Amount.BaseCurrency + "2";
            var latestTrackId = orderRS.OrderItems.First().PaymentDetails.OrderByDescending(x => x.PaymentCommitmentDateTime);
            var NameCorrectedTrackID = latestTrackId.FirstOrDefault()?.TransactionType == "NameCorrection" ? orderRS.OrderItems.First().PaymentDetails.Count(x => x.TransactionType == "NameCorrection") : 0;
            var paxCount = orderManagementRS.DataList.CustomerDetails.Count();
            var trackIDHistories = paxCount == NameCorrectedTrackID ? orderRS.OrderItems.First().PaymentDetails.Where(x => x.TransactionType == "NameCorrection") : orderRS.OrderItems.First().PaymentDetails.OrderByDescending(x => int.Parse(x.SequenceId)).Take(NameCorrectedTrackID + 1);
            var paymentReq = request.Request.PaymentFunctions?.FirstOrDefault()?.PaymentProcessingDetails;
            var paymentMethodDetail = paymentReq?.PaymentMethod.Item as VAA.EAPI.NDC.Order.Domain.OrderChangeRQ.PaymentProcessingPaymentCardType;

            return new PaymentIFGMSExchangeCommand()
            {
                Pos = offerDocResponse.Pos,
                TrackId = (paymentIFGAuthorizeRS != null) ? paymentIFGAuthorizeRS?.PaymentDocument?.TrackId : null,
                TrackIdsHistory = trackIDHistories.Select(x => 
                {
                    return new TrackIdHistory()
                    {
                        TrackId = x.TrackId,
                        PaxRefIds = x.PaxRefIds,
                    };
                }),
                Parties = new VAA.EAPI.NDC.Order.Infrastructure.Model.Parties
                {
                    Participants = offerDocResponse.Parties.Participants.Select(p => new VAA.EAPI.NDC.Order.Infrastructure.Model.Participant
                    {
                        PartyType = p.PartyType,
                        Name = p.Name,
                        AgencyID = !string.IsNullOrEmpty(p.AgencyID) && p.AgencyID.Length > 1 ? p.AgencyID.Substring(0, p.AgencyID.Length - 1) : p.AgencyID,
                        Ordinal = p.Ordinal,
                        AccountCode = p.AccountCode,
                        PccCode = p.PccCode,
                        IsCreditCardAccepted = p.IsCreditCardAccepted
                    })
                },
                PaymentDetails = new List<Model.PaymentMS.PaymentDetails>()
                {
                   new ()
                   {
                       FormOfPayments = isCardPayment ? Model.PaymentMS.PaymentDetails.FormOfPayment.CardEnum : isEasyPay? Model.PaymentMS.PaymentDetails.FormOfPayment.EasyPay:  Model.PaymentMS.PaymentDetails.FormOfPayment.CashEnum,
                       Amount = new()
                       {
                            MonetaryAmount = request.Request.PaymentFunctions != null ? ((int)(request.Request.PaymentFunctions.First().PaymentProcessingDetails.Amount.Value * 100)).ToString() : "000",
                            BaseCurrency = isRefund ? baseCurrency : offerDocResponse.Offers?.FirstOrDefault()?.SalePrice.FirstOrDefault()?.Amount?.BaseCurrency+ "2",
                       },
                       CardMethod = isCardPayment && paymentMethodDetail != null && paymentReq != null ? (CardInfo)((VAA.EAPI.NDC.Order.Domain.OrderChangeRQ.PaymentProcessingPaymentCardType)paymentReq.PaymentMethod.Item) : isCardPayment ? new CardInfo() {CardType = latestTrackId.FirstOrDefault( x => x.CardMethod != null)?.CardMethod?.CardBrandCode}  : null,

                   }
                },
                Segments = offerDocResponse?.DataList?.FlightSegments?.Select(flightSegment =>
                {
                    var matchingOfferSegment = offerDocResponse.DataList.OfferSegments?.FirstOrDefault(os => os.FlightSegmentRefId.Trim('F') == flightSegment.SegmentId.Trim('F'));

                    return new Segment
                    {
                        SegmentId = flightSegment.SegmentId.Trim('F'),
                        MarketingCarrier = flightSegment.MarketingCarrier,
                        MarketingFlightNumber = flightSegment.MarketingFlightNumber,
                        OperatingCarrier = flightSegment.OperatingCarrier ?? flightSegment.MarketingCarrier,
                        OperatingFlightNumber = flightSegment.OperatingFlightNumber ?? flightSegment.MarketingFlightNumber,
                        OriginAirportCode = flightSegment.OriginAirportCode,
                        DestinationAirportCode = flightSegment.DestinationAirportCode,
                        OriginCountryCode = airportDetails.FirstOrDefault(y => y.AirportCode == flightSegment.OriginAirportCode)?.CountryCode,
                        DestinationCountryCode = airportDetails.FirstOrDefault(y => y.AirportCode == flightSegment.DestinationAirportCode)?.CountryCode,
                        ClassOfService = matchingOfferSegment?.ClassOfService,
                        ScheduledArrivalTime = flightSegment.ScheduledArrivalTime,
                        ScheduledDepartureTime = flightSegment.ScheduledDepartureTime,
                        FareBasis = GetSegmentLevelFareBasis(offerDocResponse, flightSegment.SegmentId),
                        ConnectionOrStopOver = flightSegment.ConnectionOrStopOver,
                        CabinType = matchingOfferSegment?.CabinType
                    };
                }),
                CustomerDetails = orderManagementRS.DataList.CustomerDetails.Select(x => new Customer
                {
                    FirstName = x.FirstName,
                    LastName = x.LastName,
                    Ptc = x.Ptc,
                    CustId = x.CustId,
                    DateOfBirth = x.DateOfBirth
                }),
                BookingReferences = orderManagementRS.BookingReference,
                OrderId = $"VS932{orderManagementRS.BookingReference?.RLOC}{DateTime.UtcNow:yy}",
                OrderItems = orderManagementRS.OrderItems,
                DataList = orderManagementRS.DataList,
                OrderHistory = new OrderHistory[]
                {
                    new()
                    {
                        OrderItems = orderRS.OrderItems,
                        DataList = orderRS.DataList
                    }   
                }
            };
        }

        public static PaymentIFGMSAuthorizeCommand MapOrderChangeIFGAuthorizeRequest(IATA_OrderChangeRQ request, OrderDocument orderDoc, IEnumerable<AirportDetail> airportDetails, string amount, ProductType productType, [Optional] bool IsCardPayment, bool IsEasyPay, [Optional] OfferResponse offerResponse)
        {
            var currencyCode = request.Request.PaymentFunctions.First().PaymentProcessingDetails.Amount.CurCode;
            var adtConactInfo = orderDoc.DataList.CustomerDetails.First().Contacts;
            var paymentReq = request.Request.PaymentFunctions?.FirstOrDefault()?.PaymentProcessingDetails;
            var paymentMethodDetail = IsCardPayment ? ((VAA.EAPI.NDC.Order.Domain.OrderChangeRQ.PaymentProcessingPaymentCardType)paymentReq.PaymentMethod.Item) : null;
            var transactionType = IsCardPayment ? paymentMethodDetail.SecurePaymentVersion2.PaymentTrxChannelCode.ToString() : null;
            var easyPayRq = IsEasyPay ? request.Request?.PaymentFunctions?.FirstOrDefault()?.PaymentProcessingDetails?.PaymentMethod?.Item as VAA.EAPI.NDC.Order.Domain.OrderChangeRQ.PaymentProcessingIATA_EasyPayType : null;

            return new PaymentIFGMSAuthorizeCommand()
            {
                Pos = orderDoc.PointOfSale,
                ProductType = productType,
                OrderItems = IsCardPayment ? orderDoc.OrderItems : null,
                BookingReferences = orderDoc.BookingReference,
                Parties = new VAA.EAPI.NDC.Order.Infrastructure.Model.Parties
                {
                    Participants = orderDoc.Parties.Participants.Select(p => new VAA.EAPI.NDC.Order.Infrastructure.Model.Participant
                    {
                        PartyType = p.PartyType,
                        Name = p.Name,
                        AgencyID = !string.IsNullOrEmpty(p.AgencyID) && p.AgencyID.Length > 1 ? p.AgencyID.Substring(0, p.AgencyID.Length - 1) : p.AgencyID,
                        Ordinal = p.Ordinal,
                        AccountCode = p.AccountCode,
                        PccCode = p.PccCode,
                        IsCreditCardAccepted = p.IsCreditCardAccepted
                    })
                },
                PaymentDetails = new List<Model.PaymentMS.PaymentDetails>()
                {
                   new ()
                   {
                       FormOfPayments =  IsCardPayment ? Model.PaymentMS.PaymentDetails.FormOfPayment.CardEnum :IsEasyPay ? Model.PaymentMS.PaymentDetails.FormOfPayment.EasyPay: Model.PaymentMS.PaymentDetails.FormOfPayment.CashEnum ,
                       Amount = new()
                       {
                            MonetaryAmount = ((int)(request.Request.PaymentFunctions.First().PaymentProcessingDetails.Amount.Value * 100)).ToString(),
                            BaseCurrency = currencyCode + "2",
                       },
                       CardMethod = IsCardPayment? (CardInfo)((VAA.EAPI.NDC.Order.Domain.OrderChangeRQ.PaymentProcessingPaymentCardType)paymentReq.PaymentMethod.Item) : IsEasyPay? new CardInfo() { ExpiryDate = easyPayRq.ExpirationDate,CardNumber = easyPayRq.AccountNumber} : null,
                       Payer = IsCardPayment? new PaymentPayer()
                       {
                            FirstName = ((Domain.OrderChangeRQ.PayerIndividualType)paymentReq.Payer.PayerName.Item)?.GivenName.FirstOrDefault(),
                            LastName = ((Domain.OrderChangeRQ.PayerIndividualType)paymentReq.Payer.PayerName.Item)?.Surname,
                            PayerAddress = new Address()
                            {
                                AddressLine1 = paymentMethodDetail.CardholderAddress?.StreetText.FirstOrDefault(),
                                AddressLine2 = paymentMethodDetail.CardholderAddress?.StreetText.LastOrDefault(),
                                PostalCode = paymentMethodDetail.CardholderAddress?.PostalCode,
                                City = paymentMethodDetail.CardholderAddress?.CityName,
                                Country = paymentMethodDetail.CardholderAddress?.CountryCode,
                            },
                            PayerContacts = new Contacts()
                            {
                               Email =adtConactInfo?.Email,
                               Phone = adtConactInfo?.Phone,
                               CountryDailingCode = adtConactInfo?.CountryDailingCode?.Replace("+", "")
                            }
                       } :null,
                       PaymentCardAuthentication = IsCardPayment ?(transactionType == "MO") || (transactionType == "TO")? null: (VAA.EAPI.NDC.Order.Infrastructure.Model.PaymentMS.PaymentCardAuthentication)((VAA.EAPI.NDC.Order.Domain.OrderChangeRQ.PaymentProcessingPaymentCardType)paymentReq.PaymentMethod.Item) : null,
                   }
                },
                Segments = orderDoc?.DataList?.SegmentDetails?.Select(flightSegment =>
                {
                    return new Segment
                    {
                        SegmentId = flightSegment.SegmentId.Trim('F'),
                        MarketingCarrier = flightSegment.MarketingCarrier,
                        MarketingFlightNumber = flightSegment.MarketingFlightNumber,
                        OperatingCarrier = flightSegment.OperatingCarrier ?? flightSegment.MarketingCarrier,
                        OperatingFlightNumber = flightSegment.OperatingFlightNumber ?? flightSegment.MarketingFlightNumber,
                        OriginAirportCode = flightSegment.OriginAirportCode,
                        DestinationAirportCode = flightSegment.DestinationAirportCode,
                        OriginCountryCode = airportDetails.FirstOrDefault(y => y.AirportCode == flightSegment.OriginAirportCode)?.CountryCode,
                        DestinationCountryCode = airportDetails.FirstOrDefault(y => y.AirportCode == flightSegment.DestinationAirportCode)?.CountryCode,
                        ClassOfService = flightSegment?.ClassOfService,
                        ScheduledArrivalTime = flightSegment.ScheduledArrivalTime,
                        ScheduledDepartureTime = flightSegment.ScheduledDepartureTime,
                        FareBasis = GetSegmentLevelFareBasis(orderDoc, flightSegment.SegmentId),
                        ConnectionOrStopOver = flightSegment.ConnectionOrStopOver,
                        CabinType = flightSegment.CabinType,
                    };
                }),
                CustomerDetails = orderDoc.DataList.CustomerDetails.Select(x => new Customer
                {
                    FirstName = x.FirstName,
                    LastName = x.LastName,
                    Ptc = x.Ptc,
                    CustId = x.CustId,
                    DateOfBirth = x.DateOfBirth
                }),
            };
        }

        public static PaymentIFGMSOrderCommand MapOrderChangeHoldBookingIFGOrderRequest(PaymentIFGMSAuthorizeCommand authorizeIFGRequest, IEnumerable<AccountableDocumentRef> accountableDocumentRefs, PaymentRS authorizeIFGResponse, OrderDocument orderDocument, ProductType productType)
        {
            return new PaymentIFGMSOrderCommand()
            {
                Pos = authorizeIFGRequest.Pos,
                ProductType = productType,
                TrackId = authorizeIFGResponse.PaymentDocument.TrackId,
                Parties = authorizeIFGRequest.Parties,
                PaymentDetails = authorizeIFGRequest.PaymentDetails,
                Segments = authorizeIFGRequest.Segments,
                CustomerDetails = authorizeIFGRequest.CustomerDetails,
                BookingReferences = orderDocument.BookingReference,
                OrderId = $"VS932{orderDocument.BookingReference?.RLOC}{DateTime.UtcNow:yy}",
                AccountableDocumentRef = accountableDocumentRefs,
                OrderItems = orderDocument.OrderItems,
                DataList = orderDocument.DataList
            };
        }

        public static PaymentIFGMSOrderRefundCommand MapOrderChangeIFGRefundRequest(OrderDocument orderManagementDoc, OfferResponse offerDocResponse, OrderDocument orderDocument, IEnumerable<AirportDetail> airportDetails, bool isCardPayment, bool isEasyPay)
        {
            var refundPrice = offerDocResponse.Offers?.FirstOrDefault()?.OfferItems.Select(x => (decimal.TryParse(x.OfferitemPricing.FirstOrDefault()?.RefundPrice?.FirstOrDefault()?.Amount.MonetaryAmount, out var monetaryAmount) ? monetaryAmount : 0m) * (x.Services.FirstOrDefault()?.ServiceBeneficiaries.Count())).Sum() ?? 0m;

            return new PaymentIFGMSOrderRefundCommand()
            {
                Pos = offerDocResponse.Pos,
                TrackId = orderDocument.OrderItems.First().PaymentDetails.OrderByDescending(x => x.PaymentCommitmentDateTime).LastOrDefault()?.TrackId,
                ExchangeRefund = true,
                Parties = new VAA.EAPI.NDC.Order.Infrastructure.Model.Parties
                {
                    Participants = offerDocResponse.Parties.Participants.Where(x => x.PartyType == Model.Participant.PartyTypeEnum.Seller).Select(p => new VAA.EAPI.NDC.Order.Infrastructure.Model.Participant
                    {
                        PartyType = p.PartyType,
                        Name = p.Name,
                        AgencyID = !string.IsNullOrEmpty(p.AgencyID) && p.AgencyID.Length > 1 ? p.AgencyID.Substring(0, p.AgencyID.Length - 1) : p.AgencyID,
                        Ordinal = p.Ordinal,
                        AccountCode = p.AccountCode,
                        PccCode = p.PccCode,
                        IsCreditCardAccepted = p.IsCreditCardAccepted
                    })
                },
                PaymentDetails = new List<Model.PaymentMS.PaymentDetails>()
                {
                   new ()
                   {
                       FormOfPayments = isCardPayment ? Model.PaymentMS.PaymentDetails.FormOfPayment.CardEnum : isEasyPay? Model.PaymentMS.PaymentDetails.FormOfPayment.EasyPay: Model.PaymentMS.PaymentDetails.FormOfPayment.CashEnum,
                       Amount = new()
                       {
                            MonetaryAmount = ((int)(Math.Abs(refundPrice) * 100)).ToString(),
                            BaseCurrency = offerDocResponse.Offers?.FirstOrDefault()?.OfferItems.FirstOrDefault()?.OfferitemPricing.FirstOrDefault()?.RefundPrice.FirstOrDefault()?.Amount.BaseCurrency+ "2",
                       }
                   }
                },
                Segments = offerDocResponse?.DataList?.FlightSegments?.Select(flightSegment =>
                {
                    var matchingOfferSegment = offerDocResponse.DataList.OfferSegments?.FirstOrDefault(os => os.FlightSegmentRefId.Trim('F') == flightSegment.SegmentId);

                    return new Segment
                    {
                        SegmentId = flightSegment.SegmentId.Trim('F'),
                        MarketingCarrier = flightSegment.MarketingCarrier,
                        MarketingFlightNumber = flightSegment.MarketingFlightNumber,
                        OperatingCarrier = flightSegment.OperatingCarrier ?? flightSegment.MarketingCarrier,
                        OperatingFlightNumber = flightSegment.OperatingFlightNumber ?? flightSegment.MarketingFlightNumber,
                        OriginAirportCode = flightSegment.OriginAirportCode,
                        DestinationAirportCode = flightSegment.DestinationAirportCode,
                        OriginCountryCode = airportDetails.FirstOrDefault(y => y.AirportCode == flightSegment.OriginAirportCode)?.CountryCode,
                        DestinationCountryCode = airportDetails.FirstOrDefault(y => y.AirportCode == flightSegment.DestinationAirportCode)?.CountryCode,
                        ClassOfService = matchingOfferSegment?.ClassOfService,
                        ScheduledArrivalTime = flightSegment.ScheduledArrivalTime,
                        ScheduledDepartureTime = flightSegment.ScheduledDepartureTime,
                        FareBasis = GetSegmentLevelFareBasis(offerDocResponse, flightSegment.SegmentId),
                        ConnectionOrStopOver = flightSegment.ConnectionOrStopOver
                    };
                }),
                CustomerDetails = orderManagementDoc.DataList.CustomerDetails.Select(x => new Customer
                {
                    FirstName = x.FirstName,
                    LastName = x.LastName,
                    Ptc = x.Ptc,
                    CustId = x.CustId,
                    DateOfBirth = x.DateOfBirth
                }),
                BookingReferences = orderManagementDoc.BookingReference,
                OrderId = $"VS932{orderManagementDoc.BookingReference?.RLOC}{DateTime.UtcNow:yy}",
                OrderItems = orderManagementDoc.OrderItems,
            };
        }

        public static PaymentIFGMSOrderRefundCommand MapOrderCancelIFGRefundRequest(CancelReshopResponse offerDocResponse, OrderDocument orderDocument, IEnumerable<AirportDetail> airportDetails, [Optional] bool isEasyPay)
        {
            var refundPrice = offerDocResponse.Offers?.FirstOrDefault()?.OfferItems.Select(x => (decimal.TryParse(x.OfferitemPricing.FirstOrDefault()?.RefundPrice?.FirstOrDefault()?.Amount.MonetaryAmount, out var monetaryAmount) ? monetaryAmount : 0m) * (x.Services.FirstOrDefault()?.ServiceBeneficiaries.Count())).Sum() ?? 0m;

            return new PaymentIFGMSOrderRefundCommand()
            {
                Pos = orderDocument.PointOfSale,
                TrackId = orderDocument.OrderItems.First().PaymentDetails.OrderByDescending(x => x.PaymentCommitmentDateTime).FirstOrDefault().TrackId,
                IsRfc = (orderDocument.BookingReference?.SplitRLOC?.ParentRLOC != null || orderDocument.BookingReference?.SplitRLOC?.ChildRLOCDetails?.Any(d => d?.ChildRLOC != null) == true)? false: offerDocResponse.IsRfc,
                Parties = new VAA.EAPI.NDC.Order.Infrastructure.Model.Parties
                {
                    Participants = offerDocResponse.Parties.Participants.Select(p => new VAA.EAPI.NDC.Order.Infrastructure.Model.Participant
                    {
                        PartyType = p.PartyType,
                        Name = p.Name,
                        AgencyID = !string.IsNullOrEmpty(p.AgencyID) && p.AgencyID.Length > 1 ? p.AgencyID.Substring(0, p.AgencyID.Length - 1) : p.AgencyID,
                        Ordinal = p.Ordinal,
                        AccountCode = p.AccountCode,
                        PccCode = p.PccCode,
                        IsCreditCardAccepted = p.IsCreditCardAccepted
                    })
                },
                PaymentDetails = new List<Model.PaymentMS.PaymentDetails>()
                {
                   new ()
                   {
                       FormOfPayments = isEasyPay ? Model.PaymentMS.PaymentDetails.FormOfPayment.EasyPay: Model.PaymentMS.PaymentDetails.FormOfPayment.CashEnum,
                       Amount = new()
                       {
                            MonetaryAmount = ((int)(Math.Abs(refundPrice) * 100)).ToString(),
                            BaseCurrency = offerDocResponse.Offers?.FirstOrDefault()?.OfferItems.FirstOrDefault()?.OfferitemPricing.FirstOrDefault()?.RefundPrice.FirstOrDefault()?.Amount.BaseCurrency+ "2",
                       }
                   }
                },
                Segments = offerDocResponse?.DataList?.FlightSegments?.Select(flightSegment =>
                {
                    var matchingOfferSegment = offerDocResponse.DataList.OfferSegments?.FirstOrDefault(os => os.FlightSegmentRefId.Trim('F') == flightSegment.SegmentId);

                    return new Segment
                    {
                        SegmentId = flightSegment.SegmentId.Trim('F'),
                        MarketingCarrier = flightSegment.MarketingCarrier,
                        MarketingFlightNumber = flightSegment.MarketingFlightNumber,
                        OperatingCarrier = flightSegment.OperatingCarrier ?? flightSegment.MarketingCarrier,
                        OperatingFlightNumber = flightSegment.OperatingFlightNumber ?? flightSegment.MarketingFlightNumber,
                        OriginAirportCode = flightSegment.OriginAirportCode,
                        DestinationAirportCode = flightSegment.DestinationAirportCode,
                        OriginCountryCode = airportDetails.FirstOrDefault(y => y.AirportCode == flightSegment.OriginAirportCode)?.CountryCode,
                        DestinationCountryCode = airportDetails.FirstOrDefault(y => y.AirportCode == flightSegment.DestinationAirportCode)?.CountryCode,
                        ClassOfService = matchingOfferSegment?.ClassOfService,
                        ScheduledArrivalTime = flightSegment.ScheduledArrivalTime,
                        ScheduledDepartureTime = flightSegment.ScheduledDepartureTime,
                        FareBasis = GetSegmentLevelFareBasis(orderDocument, flightSegment.SegmentId),
                        ConnectionOrStopOver = flightSegment.ConnectionOrStopOver
                    };
                }),
                CustomerDetails = orderDocument.DataList.CustomerDetails.Select(x => new Customer
                {
                    FirstName = x.FirstName,
                    LastName = x.LastName,
                    Ptc = x.Ptc,
                    CustId = x.CustId,
                    DateOfBirth = x.DateOfBirth
                }),
                BookingReferences = orderDocument.BookingReference,
                OrderId = $"VS932{orderDocument.BookingReference?.RLOC}{DateTime.UtcNow:yy}",
                OrderItems = orderDocument.OrderItems,
                
            };
        }

        public static PaymentIFGMSOrderRefundCommand MapOrderCancelCCIFGRefundRequest(CancelReshopResponse offerDocResponse, OrderDocument orderDocument, OrderHistory latestHistories, PaymentTransactionDocument paymentDoc)
        {
            var refundPrice = offerDocResponse.IsRfc && orderDocument.BookingReference?.SplitRLOC == null ? (decimal.TryParse(paymentDoc.PaymentDetails.First().Amount.MonetaryAmount, out var monetaryAmount) ? monetaryAmount : 0m): offerDocResponse.Offers?.FirstOrDefault()?.OfferItems.Select(x => (decimal.TryParse(x.OfferitemPricing.FirstOrDefault()?.RefundPrice?.FirstOrDefault()?.Amount.MonetaryAmount, out var monetaryAmount) ? monetaryAmount : 0m) * (x.Services.FirstOrDefault()?.ServiceBeneficiaries.Count())).Sum() ?? 0m;
            var paymentRef = orderDocument.DataList.CustomerDetails.First(x => x.Ptc.ToUpper() == "ADT" && x.Contacts != null);
            var latestPaymentFunc = latestHistories.OrderItems.First().PaymentDetails.OrderByDescending(x => x.PaymentCommitmentDateTime);

            return new PaymentIFGMSOrderRefundCommand()
            {
                Pos = orderDocument.PointOfSale,
                TrackId = latestPaymentFunc?.FirstOrDefault()?.TrackId,
                IsRfc = offerDocResponse.IsRfc && orderDocument.BookingReference?.SplitRLOC == null && latestHistories.ChangeType == ChangeTypeEnum.OrderItemAdded ? true : false,
                Parties = new VAA.EAPI.NDC.Order.Infrastructure.Model.Parties
                {
                    Participants = offerDocResponse.Parties.Participants.Select(p => new VAA.EAPI.NDC.Order.Infrastructure.Model.Participant
                    {
                        PartyType = p.PartyType,
                        Name = p.Name,
                        AgencyID = !string.IsNullOrEmpty(p.AgencyID) && p.AgencyID.Length > 1 ? p.AgencyID.Substring(0, p.AgencyID.Length - 1) : p.AgencyID,
                        Ordinal = p.Ordinal,
                        AccountCode = p.AccountCode,
                        PccCode = p.PccCode,
                        IsCreditCardAccepted = p.IsCreditCardAccepted
                    })
                },
                PaymentDetails = new List<Model.PaymentMS.PaymentDetails>()
                {
                   new ()
                   {
                       FormOfPayments = Model.PaymentMS.PaymentDetails.FormOfPayment.CardEnum,
                       Amount = new()
                       {
                            MonetaryAmount = ((int)(Math.Abs(refundPrice) * 100)).ToString(),
                            BaseCurrency = offerDocResponse.Offers?.FirstOrDefault()?.OfferItems.FirstOrDefault()?.OfferitemPricing.FirstOrDefault()?.RefundPrice.FirstOrDefault()?.Amount.BaseCurrency+ "2",
                       },
                       CardMethod = new CardInfo() {CardType = latestPaymentFunc?.FirstOrDefault( x => x.CardMethod != null)?.CardMethod?.CardBrandCode},
                       Payer = new PaymentPayer()
                       {
                           FirstName = paymentRef.FirstName,
                           LastName = paymentRef.LastName,
                           PayerContacts = new Contacts()
                           {
                                Email =paymentRef?.Contacts.Email,
                           }
                       }
                   }
                },
                CustomerDetails = orderDocument.DataList.CustomerDetails.Select(x => new Customer
                {
                    FirstName = x.FirstName,
                    LastName = x.LastName,
                    Ptc = x.Ptc,
                    CustId = x.CustId
                }),
                BookingReferences = orderDocument.BookingReference,
                OrderId = $"VS932{orderDocument.BookingReference?.RLOC}{DateTime.UtcNow:yy}",
                OrderItems = latestHistories.OrderItems,
                OrderHistory = new OrderHistory[]
                {
                    new()
                    {
                        OrderItems =  offerDocResponse.IsRfc ? paymentDoc.OrderItems.Select(x => new EAPI.Domain.Virgin.Models.Order.OrderItems
                        {
                            OrderItemId = x.OrderItemId,
                            Services = x.Services,
                            PriceDetails = x.PriceDetails
                        }) : orderDocument.OrderItems
                    }
                }
            };
        }

        public static PaymentIFGMSExchangeCommand MapOrderChangeNameCorrectionIFGOrderExchangeRequest(IATA_OrderChangeRQ request, IEnumerable<AirportDetail> airportDetails, OrderDocument orderDocument, OrderRS orderDataRS, bool IsEasyPay, bool isCardPayment)
        {
            var paymentDetail = orderDataRS.OrderDocument.OrderItems.First().PaymentDetails.Where(x => x.TransactionType != "NameCorrection").OrderByDescending(x => x.PaymentCommitmentDateTime).FirstOrDefault();
            var latestPaymentFunc = orderDataRS.OrderDocument.OrderItems.First().PaymentDetails.OrderByDescending(x => x.PaymentCommitmentDateTime);

            return new PaymentIFGMSExchangeCommand()
            {
                Pos = orderDataRS.OrderDocument.PointOfSale,
                TrackIdsHistory = new TrackIdHistory[] 
                {
                    new()
                    {
                        TrackId = paymentDetail.TrackId,
                        PaxRefIds = paymentDetail.PaxRefIds,
                    }
                },
                Parties = new VAA.EAPI.NDC.Order.Infrastructure.Model.Parties
                {
                    Participants = orderDataRS.OrderDocument.Parties.Participants.Select(p => new VAA.EAPI.NDC.Order.Infrastructure.Model.Participant
                    {
                        PartyType = p.PartyType,
                        Name = p.Name,
                        AgencyID = !string.IsNullOrEmpty(p.AgencyID) && p.AgencyID.Length > 1 ? p.AgencyID.Substring(0, p.AgencyID.Length - 1) : p.AgencyID,
                        Ordinal = p.Ordinal,
                        AccountCode = p.AccountCode,
                        PccCode = p.PccCode,
                        IsCreditCardAccepted = p.IsCreditCardAccepted
                    })
                },
                PaymentDetails = new List<Model.PaymentMS.PaymentDetails>()
                {
                   new ()
                   {
                       FormOfPayments = isCardPayment ? Model.PaymentMS.PaymentDetails.FormOfPayment.CardEnum : IsEasyPay? Model.PaymentMS.PaymentDetails.FormOfPayment.EasyPay:  Model.PaymentMS.PaymentDetails.FormOfPayment.CashEnum,
                       Amount = new()
                       {
                            MonetaryAmount = ((int)(request.Request.PaymentFunctions.First().PaymentProcessingDetails.Amount.Value * 100)).ToString(),
                            BaseCurrency = request.Request.PaymentFunctions.First().PaymentProcessingDetails.Amount.CurCode + "2",
                       },
                       CardMethod = isCardPayment ? new CardInfo() {CardType = latestPaymentFunc.FirstOrDefault( x => x.CardMethod != null)?.CardMethod?.CardBrandCode}  : null,

                   }
                },
                Segments = orderDataRS?.OrderDocument?.DataList?.SegmentDetails?.Select(flightSegment =>
                {
                    return new Segment
                    {
                        SegmentId = flightSegment.SegmentId.Trim('F'),
                        MarketingCarrier = flightSegment.MarketingCarrier,
                        MarketingFlightNumber = flightSegment.MarketingFlightNumber,
                        OperatingCarrier = flightSegment.OperatingCarrier ?? flightSegment.MarketingCarrier,
                        OperatingFlightNumber = flightSegment.OperatingFlightNumber ?? flightSegment.MarketingFlightNumber,
                        OriginAirportCode = flightSegment.OriginAirportCode,
                        DestinationAirportCode = flightSegment.DestinationAirportCode,
                        OriginCountryCode = airportDetails.FirstOrDefault(y => y.AirportCode == flightSegment.OriginAirportCode)?.CountryCode,
                        DestinationCountryCode = airportDetails.FirstOrDefault(y => y.AirportCode == flightSegment.DestinationAirportCode)?.CountryCode,
                        ClassOfService = flightSegment?.ClassOfService,
                        ScheduledArrivalTime = flightSegment.ScheduledArrivalTime,
                        ScheduledDepartureTime = flightSegment.ScheduledDepartureTime,
                        FareBasis = GetSegmentLevelFareBasis(orderDataRS.OrderDocument, flightSegment.SegmentId),
                        ConnectionOrStopOver = flightSegment.ConnectionOrStopOver,
                        CabinType = flightSegment.CabinType
                    };
                }),
                CustomerDetails = orderDocument.DataList.CustomerDetails.Where(x => x.CustId == request.Request.DataLists.PaxList.First().PaxID).Select(x => new Customer
                {
                    FirstName = x.FirstName,
                    LastName = x.LastName,
                    Ptc = x.Ptc,
                    CustId = x.CustId,
                    DateOfBirth = x.DateOfBirth
                }),
                BookingReferences = orderDocument.BookingReference,
                OrderId = $"VS932{orderDocument.BookingReference?.RLOC}{DateTime.UtcNow:yy}",
                OrderItems = orderDocument.OrderItems,
                DataList = orderDocument.DataList,
                OrderHistory = new OrderHistory[]
                {
                    new()
                    {
                        OrderItems = orderDataRS.OrderDocument.OrderItems,
                        DataList = orderDataRS.OrderDocument.DataList
                    }
                }
            };
        }

        public static PaymentIFGMSAuthorizeCommand MapOrderCreateCCIFGAuthorizeRequest(OrderRS orderRS, IATA_OrderCreateRQ OrderCreateRQ, OfferResponse offerDocResponse, IEnumerable<Currency> currencyReferenceMSResponse, IEnumerable<AirportDetail> listAirportDetails, string amount, ProductType productType)
        {
            var paymentRq = OrderCreateRQ.Request.PaymentFunctions.First().PaymentProcessingDetails;
            var paymentMethodDetail = ((VAA.EAPI.NDC.Order.Domain.OrderCreateRQ.PaymentProcessingPaymentCardType)paymentRq.PaymentMethod.Item);
            var transactionType = paymentMethodDetail.SecurePaymentVersion2.PaymentTrxChannelCode.ToString();
            var adtContactInfoRefIds = OrderCreateRQ?.Request?.DataLists?.PaxList?.Where(x => x.PTC.ToUpper() == "ADT").Select(x => x.ContactInfoRefID);
            var adtConactInfo = OrderCreateRQ?.Request?.DataLists?.ContactInfoList?.Where(x => adtContactInfoRefIds.Contains(x.ContactInfoID)).SelectMany(x => GetEmailAddressesForPayment(x).Select(y => new { y.EmailAddressText, Phone = x.Phone?.FirstOrDefault()?.PhoneNumber, CountryDialingCode = x.Phone?.FirstOrDefault()?.CountryDialingCode?.Replace("+", "")})).First(x => x.EmailAddressText != null);

            var paymentDetails = new List<Model.PaymentMS.PaymentDetails>()
            {
                new()
                {
                    FormOfPayments = Model.PaymentMS.PaymentDetails.FormOfPayment.CardEnum,
                    Amount = new CurrencyAmount()
                    {
                        MonetaryAmount = decimal.TryParse(amount, out var parsedAmount) ? ((int)(parsedAmount * 100)).ToString(): null,
                        BaseCurrency = offerDocResponse.Offers?.FirstOrDefault()?.SalePrice.FirstOrDefault()?.Amount?.BaseCurrency+ "2",
                        CurrencyId = currencyReferenceMSResponse.First(a=>a.CurrencyCode == paymentRq.Amount.CurCode).CurrencyNumber,
                    },
                    CardMethod = (VAA.EAPI.NDC.Order.Infrastructure.Model.PaymentMS.CardInfo)((VAA.EAPI.NDC.Order.Domain.OrderCreateRQ.PaymentProcessingPaymentCardType)paymentRq.PaymentMethod.Item),
                    Payer = new PaymentPayer()
                    {
                        FirstName = ((Domain.OrderCreateRQ.PayerIndividualType)paymentRq.Payer.PayerName.Item)?.GivenName.FirstOrDefault(),
                        LastName = ((Domain.OrderCreateRQ.PayerIndividualType)paymentRq.Payer.PayerName.Item)?.Surname,
                        PayerAddress = new Address()
                        {
                            AddressLine1 = paymentMethodDetail.CardholderAddress?.StreetText.FirstOrDefault(),
                            AddressLine2 = paymentMethodDetail.CardholderAddress?.StreetText.LastOrDefault(),
                            PostalCode = paymentMethodDetail.CardholderAddress?.PostalCode,
                            City = paymentMethodDetail.CardholderAddress?.CityName,
                            Country = paymentMethodDetail.CardholderAddress?.CountryCode,
                        },
                        PayerContacts = new Contacts()
                        {
                           Email =adtConactInfo?.EmailAddressText,
                           Phone = adtConactInfo?.Phone,
                           CountryDailingCode = adtConactInfo.CountryDialingCode
                        }
                    },
                    PaymentCardAuthentication = (transactionType == "MO") || (transactionType == "TO")? null: (VAA.EAPI.NDC.Order.Infrastructure.Model.PaymentMS.PaymentCardAuthentication)((VAA.EAPI.NDC.Order.Domain.OrderCreateRQ.PaymentProcessingPaymentCardType)paymentRq.PaymentMethod.Item),
                    TransactionTypeId = (transactionType == "MO") || (transactionType == "TO") ? 7 : 1009
                }
            };

            IEnumerable<EAPI.Domain.Virgin.Models.Products.Flight.OriginDestination> OriginDestinations = offerDocResponse?.DataList?.FlightSegments?.Select(x => new EAPI.Domain.Virgin.Models.Products.Flight.OriginDestination { OriginAirport = x.OriginAirportCode, DestinationAirport = x.DestinationAirportCode }).ToList();

            var currenyCode = offerDocResponse.Offers?.FirstOrDefault()?.SalePrice.FirstOrDefault()?.Amount?.BaseCurrency;

            return new PaymentIFGMSAuthorizeCommand()
            {
                Pos = orderRS.OrderDocument.PointOfSale,
                Parties = new VAA.EAPI.NDC.Order.Infrastructure.Model.Parties
                {
                    Participants = offerDocResponse.Parties.Participants.Where(x => x.PartyType == Model.Participant.PartyTypeEnum.Seller ).Select(p => new VAA.EAPI.NDC.Order.Infrastructure.Model.Participant
                    {
                        PartyType = p.PartyType,
                        Name = p.Name,
                        AgencyID = !string.IsNullOrEmpty(p.AgencyID) && p.AgencyID.Length > 1 ? p.AgencyID.Substring(0, p.AgencyID.Length - 1) : p.AgencyID,
                        Ordinal = p.Ordinal,
                        AccountCode = p.AccountCode,
                        PccCode = p.PccCode,
                        IsCreditCardAccepted = p.IsCreditCardAccepted,
                        RequestAgentType = p.RequestAgentType,
                        Contacts = new()
                        {
                             Email = p.Contacts.Email
                        }
                    })
                },
                BookingReferences = orderRS.OrderDocument.BookingReference,
                OrderId = $"VS{ConfigReader.Get(Constants.Environment.TicketStockNumber)}{orderRS.OrderDocument.BookingReference?.RLOC}{DateTime.UtcNow:yy}",
                PaymentDetails = paymentDetails,
                ProductType = productType,
                OrderItems = orderRS.OrderDocument.OrderItems,
                JourneyType = OriginDestinations.Count() == 1 ? "ONEWAY" : ((OriginDestinations.Count() == 2 && OriginDestinations.First().OriginAirport == OriginDestinations.Last().DestinationAirport && OriginDestinations.First().DestinationAirport == OriginDestinations.Last().OriginAirport) ? "RETURN" : "MULTICITY"),
                CustomerDetails = OrderCreateRQ?.Request?.DataLists?.PaxList?.Select(x => new Customer
                {
                    CustId = x.PaxID,
                    FirstName = x.Individual.GivenName.FirstOrDefault(),
                    LastName = x.Individual.Surname,
                    Ptc = x.PTC,
                    Contacts = new()
                    {
                        Email = adtConactInfo?.EmailAddressText,
                    },
                    DateOfBirth = x.Individual.Birthdate.ToString("yyyy-MM-dd")
                }),
                Segments = offerDocResponse?.DataList?.FlightSegments?.Select(flightSegment =>
                {
                    var matchingOfferSegment = offerDocResponse.DataList.OfferSegments?.FirstOrDefault(os => os.FlightSegmentRefId.Trim('F') == flightSegment.SegmentId.Trim('F'));

                    return new Segment
                    {
                        ClassOfService = flightSegment.ClassOfService,
                        OriginAirportCode = flightSegment.OriginAirportCode,
                        DestinationAirportCode = flightSegment.DestinationAirportCode,
                        OriginCountryCode = listAirportDetails.FirstOrDefault(y => y.AirportCode == flightSegment.OriginAirportCode)?.CountryCode,
                        DestinationCountryCode = listAirportDetails.FirstOrDefault(y => y.AirportCode == flightSegment.DestinationAirportCode)?.CountryCode,
                        FareBasis = GetSegmentLevelFareBasis(orderRS.OrderDocument, flightSegment.SegmentId),
                        MarketingCarrier = flightSegment.MarketingCarrier,
                        MarketingFlightNumber = flightSegment.MarketingFlightNumber,
                        OperatingCarrier = flightSegment.OperatingCarrier ?? flightSegment.MarketingCarrier,
                        OperatingFlightNumber = flightSegment.OperatingFlightNumber ?? flightSegment.MarketingFlightNumber,
                        ScheduledArrivalTime = flightSegment.ScheduledArrivalTime,
                        ScheduledDepartureTime = flightSegment.ScheduledDepartureTime,
                        SegmentId = flightSegment.SegmentId,
                        CabinType = matchingOfferSegment.CabinType
                    };
                })
            };
        }

        public static PaymentIFGMSOrderCommand MapOrderCreateCCIFGOrderRequest(PaymentIFGMSAuthorizeCommand authorizeRequest, IEnumerable<AccountableDocumentRef> accountableDocumentRefs, PaymentRS paymentRS, ProductType productType, BookingReference bookingReference, OrderDocument orderDocument, [Optional] string trackId)
        {

            return new PaymentIFGMSOrderCommand()
            {
                Pos = authorizeRequest.Pos,
                TrackId = trackId,
                Parties = authorizeRequest.Parties,
                BookingReferences = bookingReference,
                OrderId = $"VS{Constants.Environment.TicketStockNumber}{authorizeRequest.BookingReferences?.RLOC}{DateTime.UtcNow:yy}",
                PaymentDetails = authorizeRequest.PaymentDetails,
                CustomerDetails = authorizeRequest.CustomerDetails,
                AccountableDocumentRef = orderDocument.OrderItems.Where(o => o.AccountableDocumentRefs != null && o.AccountableDocumentRefs.Any(a => accountableDocumentRefs.Any(b => b.DocumentNumber == a.DocumentNumber))).SelectMany(a => a.AccountableDocumentRefs),
                ProductType = productType,
                JourneyType = authorizeRequest.JourneyType,
                Segments = authorizeRequest.Segments,
                OrderItems = orderDocument.OrderItems,
                DataList = orderDocument.DataList,
                
            };
        }

        public static PaymentDocRequest MapPaymentDocumentRequest(PaymentRS ifgAuthorizeRS, PaymentIFGMSAuthorizeCommand ifgAuthorizeRequest, OrderDocument orderDocument, string docType, [Optional] bool isaddCollOrderChange, [Optional] bool isOrderRefund, [Optional] bool isEasyPay, [Optional] TransactionServiceRequest transactionServiceRequest, [Optional] TransactionServiceResponse transactionServiceResponse, [Optional] string easyPayCardNumber)
        {
            var latestPaymentFunction = orderDocument.OrderItems.FirstOrDefault()?.PaymentDetails?.OrderByDescending(x => x.PaymentCommitmentDateTime);
            var latestEasyPayPaymentdetails = latestPaymentFunction?.FirstOrDefault(x => x.CardMethod != null && x.ApprovalCode != null);
            var amount = isaddCollOrderChange ? orderDocument.OrderItems.Sum(x => Convert.ToDecimal(x.AddCollectPrice?.Amount?.MonetaryAmount) * x.Services.First().ServiceBeneficiaries.Count()) : orderDocument.OrderItems.Sum(x => Convert.ToDecimal(x.PriceDetails.First().Amount.MonetaryAmount) * x.Services.First().ServiceBeneficiaries.Count());

            return new PaymentDocRequest()
            {
                Type = docType,
                ProductType = isOrderRefund ? Constants.ProductType.Ticket : (ifgAuthorizeRequest != null ? ifgAuthorizeRequest.ProductType : transactionServiceRequest?.ProductType),
                Pos = orderDocument.PointOfSale,
                Parties = orderDocument.Parties,
                BookingReferences = orderDocument.BookingReference,
                OrderId = orderDocument.OrderNumber.OrderId,
                PaymentDetails =
                [
                    new()
                    {
                        FormOfPayments = isEasyPay ? Model.PaymentMS.PaymentDetails.FormOfPayment.EasyPay : Model.PaymentMS.PaymentDetails.FormOfPayment.CashEnum,
                        Amount = new CurrencyAmount()
                        {
                            MonetaryAmount = isOrderRefund ? orderDocument.OrderItems.Sum(x => Convert.ToDecimal(x.RefundPrice.First().Amount?.MonetaryAmount) * x.Services.First().ServiceBeneficiaries.Count()).ToString() : amount.ToString(),
                            BaseCurrency = orderDocument.OrderItems.First().PriceDetails.First().Amount.BaseCurrency
                        },
                        TrackId = ifgAuthorizeRS != null ? ifgAuthorizeRS.PaymentDocument.TrackId : (transactionServiceResponse != null) ? transactionServiceResponse.RequestId : latestPaymentFunction?.FirstOrDefault()?.TrackId,
                        ApprovalCode = isOrderRefund && isEasyPay ? latestEasyPayPaymentdetails?.ApprovalCode : isEasyPay ? ifgAuthorizeRS.PaymentDocument.ApprovalCode : null,
                        CardMethod = isOrderRefund && isEasyPay ? new() { CardNumber = latestEasyPayPaymentdetails?.CardMethod?.CardNumber } : isEasyPay ? new() { CardNumber = MaskCardNumber(easyPayCardNumber) } : null
                    }
                ]            
            };
        }

        private static string MaskCardNumber(string cardNumber)
        {
            if (string.IsNullOrEmpty(cardNumber) || cardNumber.Length <= 4)
                return cardNumber;

            return $"EP{new string('X', cardNumber.Length - 4)}{cardNumber.Substring(cardNumber.Length - 4)}";
        }

        private static CardInfo MapCardHolderName(CardInfo cardInfo, string firstName, string lastName)
        {
            cardInfo.CardholderName = $"{firstName} {lastName}";
            return cardInfo;
        }

        private static IEnumerable<Domain.OrderCreateRQ.EmailAddressType> GetEmailAddressesForPayment(Domain.OrderCreateRQ.ContactInfoType contactInfo)
        {
            if (contactInfo.ContactRefusedInd && (contactInfo.EmailAddress.IsNullOrEmpty() || contactInfo.Phone.IsNullOrEmpty()))
            {
                return [];
            }

            return contactInfo.EmailAddress;
        }

        public static PaymentIFGMSOrderCommand MapIFGOrderChangeReshopHoldBooking(PaymentIFGMSAuthorizeCommand authorizeIFGRequest, PaymentRS authorizeIFGResponse, OrderDocument orderDocument, OfferResponse offerResponse,bool IsChangeReshopHoldBooking)
        {
            return new PaymentIFGMSOrderCommand()
            {
                Pos = offerResponse.Pos,
                TrackId = authorizeIFGResponse.PaymentDocument.TrackId,
                ProductType = authorizeIFGRequest.ProductType,
                Parties = authorizeIFGRequest.Parties,
                PaymentDetails = authorizeIFGRequest.PaymentDetails,
                Segments = authorizeIFGRequest.Segments,
                CustomerDetails = authorizeIFGRequest.CustomerDetails,
                BookingReferences = orderDocument.BookingReference,
                OrderId = $"VS932{orderDocument.BookingReference.RLOC}{DateTime.UtcNow:yy}",
                DataList = orderDocument.DataList,
                OrderItems = orderDocument.OrderItems,
                IsChangeReshopHoldBooking = IsChangeReshopHoldBooking
            };
        }

    }
}
