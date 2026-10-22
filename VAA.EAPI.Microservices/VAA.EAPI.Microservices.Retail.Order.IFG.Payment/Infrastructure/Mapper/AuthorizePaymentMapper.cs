using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Domain.Virgin.Models.Common;
using VAA.EAPI.Microservices.Retail.Order.IFG.Payment.Domain;
using VAA.EAPI.Microservices.Retail.Order.IFG.Payment.Infrastructure.Mapper.Common;
using VAA.EAPI.Microservices.Retail.Order.IFG.Payment.Infrastructure.Models;
using static VAA.EAPI.Domain.Virgin.Models.Common.Price;

namespace VAA.EAPI.Microservices.Retail.Order.IFG.Payment.Infrastructure.Mapper
{
    public class AuthorizePaymentMapper
    {
        public static PaymentAuthorizeRequest AuthorizePaymentRequest(AuthorizePaymentRequest request)
        {
            return new PaymentAuthorizeRequest
            {
                BasicInformation = new()
                {
                    Channel = Constants.Application.Channel,
                    CountryCode = request.Pos.CountryCode,
                    AgentCode = request.Parties.Participants.First(p => p.PartyType.Value == EAPI.Domain.Virgin.Models.Common.Participant.PartyTypeEnum.Seller && p.Ordinal.StartsWith("1")).AgencyID,
                    NonTicketInd = true,
                    ClientID = Constants.Application.IfgClientID,
                    TASF = false
                },
                FormOfPayment = MapFormOfPayments(request.PaymentDetails, request.Parties),
                Orders = MapOrders(request)
            };
        }
        private static IEnumerable<FormOfPayment> MapFormOfPayments(IEnumerable<VAA.EAPI.Microservices.Retail.Order.IFG.Payment.Domain.Payment> payments, Parties parties)
        {
            var paymentInfo = payments.First();
            bool ifgAdditionalFieldsEnabled = bool.Parse(ConfigReader.Get(Constants.Environment.IfgAdditionalFieldsEnabled));

            return
            [
                 paymentInfo.FormOfPayments ==  EAPI.Domain.Virgin.Models.Payment.PaymentDetails.FormOfPayment.EasyPay? new FormOfPayment()
                 {
                       Amount = int.Parse(paymentInfo.Amount.MonetaryAmount),
                       Currency = paymentInfo.Amount.BaseCurrency,
                       Type = Constants.Application.EasyPayFopType,
                       ExpiryDate = string.Join("", paymentInfo.CardMethod.ExpiryDate.Split('/')),
                       AccountNumber = paymentInfo.CardMethod.CardNumber
                 }:
                paymentInfo.FormOfPayments == Domain.Payment.FormOfPayment.CardEnum
                ? new FormOfPayment()
                {
                    Amount   = int.Parse(paymentInfo.Amount.MonetaryAmount),
                    Currency = paymentInfo.Amount.BaseCurrency,
                    Type     =  $"{Constants.Application.CardType}{paymentInfo.CardMethod.CardType}",
                    Payer = new()
                    {
                        Name = $"{paymentInfo.Payer.LastName} {paymentInfo.Payer.FirstName}",
                        PhoneNumber =$"{paymentInfo.Payer.PayerContacts.CountryDailingCode}-{paymentInfo.Payer.PayerContacts.Phone}",
                        Email = paymentInfo.Payer.PayerContacts.Email,
                    },
                    CardSecurityCode = paymentInfo.CardMethod.Cvv,
                    ExpiryDate = string.Join("", paymentInfo.CardMethod.ExpiryDate.Split('/')),
                    AccountNumber = paymentInfo.CardMethod.CardNumber,
                    Address = new ()
                    {
                        HouseNumberOrName = paymentInfo.Payer.PayerAddress.AddressLine1,
                        Street = paymentInfo.Payer.PayerAddress.AddressLine2,
                        City = paymentInfo.Payer.PayerAddress.City,
                        Country = paymentInfo.Payer.PayerAddress.Country,
                        CountryCode = paymentInfo.Payer.PayerAddress.Country,
                        PostalCode = paymentInfo.Payer.PayerAddress.PostalCode
                    },
                    SecureCard3DAuthenticationInformationRecords = paymentInfo?.PaymentCardAuthentication != null ? new()
                    {
                        cardAuthenticationSequenceNumber = 1,
                        secureCard3DAuthenticationInformation = "CC",
                        secureCard3DAuthenticationInformationDetail = new()
                        {
                            cavv = paymentInfo?.PaymentCardAuthentication?.Cavv,
                            version = "2.2.0",
                            EciFlag = paymentInfo?.PaymentCardAuthentication?.Eci,
                            threeDSServerTransactionId = paymentInfo?.PaymentCardAuthentication?.ServerTransactionId
                        }
                    } : null,
                    MerchantDefinedInformation = ifgAdditionalFieldsEnabled ? HelperMethodMapper.GetMerchantDefinedInformation(parties) : null,
                    MotoPayment = paymentInfo?.PaymentCardAuthentication == null
                } :
                new()
                {
                    Amount   = int.Parse(paymentInfo.Amount.MonetaryAmount),
                    Currency = paymentInfo.Amount.BaseCurrency,
                    Type     = Constants.Application.FopType,
                }
            ];
        }

        private static IEnumerable<Models.Order> MapOrders(AuthorizePaymentRequest request)
        {
            var paymentDetails = request.PaymentDetails.First();
            CurrencyAmount amount = null;
            var offerItems = request?.Offers?.SelectMany(o => o.OfferItems);

            var orders = request.CustomerDetails.Select(cust =>
            {
                amount = request.OrderItems?.Where(x => x.Services.First().ServiceBeneficiaries.Contains(cust.CustId)).Select(x => x.PriceDetails?.FirstOrDefault(s => s.PriceComponentType == PriceComponentTypes.BasePriceEnum)?.Amount).FirstOrDefault();

                if (amount == null && offerItems != null)
                {
                    var offerItem = offerItems.FirstOrDefault(oi => oi.Services.First().ServiceBeneficiaries.Contains(cust.CustId));
                    var pricing = offerItem?.OfferitemPricing.FirstOrDefault();

                    amount = !request.IsChangeReshopHoldBooking ? pricing?.AddCollectPrice?.Amount : pricing?.SalePrice.FirstOrDefault(s => s.PriceComponentType == PriceComponentTypes.BasePriceEnum)?.Amount;
                }

                var segmentIds = request.Segments.Select(x => x.SegmentId.Trim('S', 'E', 'G'));
                var flightRecords = new List<FlightRecord>();

                for (int i = 1; i <= segmentIds.Count(); i += 4)
                {
                    flightRecords.Add(new FlightRecord
                    {
                        ConjunctionTicketIndicator = i == 1 ? false : true,
                        DocumentDetails = new DocumentDetails
                        {
                            Itinerary = MapItineraryDetails(request, i)
                        },
                        PassengerRecord = new PassengerRecord
                        {
                            Name = $"{cust.LastName} {cust.FirstName}",
                            Pnr = request.BookingReferences != null ? request.BookingReferences.RLOC : null,
                            TypeCode = cust.Ptc,
                            DateOfBirth = FormatDateOfBirth(cust.DateOfBirth)
                        }
                    });
                }
                return new Models.Order
                {
                    TransactionCode = request.ProductType.Equals(ProductType.Ticket) ? Constants.Application.IfgTicketTransactionCode : Constants.Application.IfgSeatransactionCode,
                    FlightRecords = flightRecords,
                    ReportedFormOfPayment = request.PaymentDetails.First().FormOfPayments == Domain.Payment.FormOfPayment.CardEnum ? new List<ReportedFormOfPayment>
                    {
                        new ReportedFormOfPayment
                        {
                            Amount   = decimal.TryParse(amount?.MonetaryAmount, out var parsedAmount) ? (int)(parsedAmount * 100) : 0 ,
                            Currency = paymentDetails.Amount.BaseCurrency,
                            Type     =  $"{Constants.Application.CardType}{request.PaymentDetails.First().CardMethod.CardType}",
                            Payer = new Payer()
                            {
                                 Name = $"{paymentDetails.Payer.LastName} {paymentDetails.Payer.FirstName}",
                                 PhoneNumber =$"{paymentDetails.Payer.PayerContacts.CountryDailingCode}{paymentDetails.Payer.PayerContacts.Phone}",
                                 Email = paymentDetails.Payer.PayerContacts.Email
                            },
                            ExpiryDate = string.Join("", paymentDetails.CardMethod.ExpiryDate.Split('/')),
                            AccountNumber = paymentDetails.CardMethod.CardNumber,
                            CardSecurityCode = paymentDetails.CardMethod.Cvv
                        },

                    } : null,
                };
            });

            return orders;
        }

        private static IEnumerable<Itinerary> MapItineraryDetails(AuthorizePaymentRequest request, int segmentIndex)
        {
            var itineraries = new List<Itinerary>();
            int index = 0;

            for (int i = segmentIndex; i <= segmentIndex + 3 && i <= request.Segments.Count(); i++)
            {
                var segment = request.Segments.Where(segId => segId.SegmentId.ToUpper().Replace("SEG", "").Equals(i.ToString())).FirstOrDefault();
                itineraries.Add(new Itinerary
                {
                    OriginCode = segment.OriginAirportCode,
                    DestinationCode = segment.DestinationAirportCode,
                    Carrier = segment.MarketingCarrier,
                    SoldPassengerCabin = GetCabinCode(segment.CabinType?.CabinName ?? ""),
                    ReservationBooking = segment.ClassOfService,
                    FlightDate = DateTime.Parse(segment.ScheduledDepartureTime.LocalDateTime).ToString("yyyyMMdd"),
                    FlightTime = DateTime.Parse(segment.ScheduledDepartureTime.LocalDateTime).ToString("HH:mm:ss"),
                    FlightNumber = segment.MarketingFlightNumber ?? segment.OperatingFlightNumber,
                    FareBasisTicketDesignator = segment.FareBasis,
                    SegmentIdentifier = (++index).ToString(),
                    StopoverCode = segment?.ConnectionOrStopOver,
                    FlightArrivalTime = DateTime.Parse(segment.ScheduledArrivalTime.LocalDateTime).ToString("HH:mm:ss"),
                    FlightArrivalDate = DateTime.Parse(segment.ScheduledArrivalTime.LocalDateTime).ToString("yyyyMMdd")
                });
            }
            return itineraries;
        }

        private static string FormatDateOfBirth(string dateOfBirth)
        {
            if (string.IsNullOrWhiteSpace(dateOfBirth))
            {
                return null;
            }

            DateTime parsedDateOfBirth;
            if (DateTime.TryParseExact(dateOfBirth, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedDateOfBirth) ||
                DateTime.TryParse(dateOfBirth, CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedDateOfBirth) ||
                DateTime.TryParse(dateOfBirth, out parsedDateOfBirth))
            {
                return parsedDateOfBirth.ToString("yyyyMMdd");
            }

            return null;
        }

        private static string GetCabinCode(string CabinName)
        {
            string resultstring = "";

            switch (CabinName?.Trim().ToLower())
            {
                case "upper class":
                case "business":
                    return "C";

                case "premium economy":
                    return "W";

                case "economy":
                    return "Y";

                default:
                    return resultstring;
            }
        }

    }
}
