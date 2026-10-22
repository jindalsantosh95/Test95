using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Domain.Virgin.Models.Common;
using VAA.EAPI.Domain.Virgin.Models.Customer;
using VAA.EAPI.Domain.Virgin.Models.Order;
using VAA.EAPI.Domain.Virgin.Models.Products.Flight;
using VAA.EAPI.Domain.Virgin.Models.Ticket;
using VAA.Facade.NDC.IFGFacade.Constants;
using VAA.Facade.NDC.IFGFacade.Domain;
using VAA.Facade.NDC.IFGFacade.Infrastructure.Models.OrderData;
using VAA.Facade.NDC.IFGFacade.Infrastructure.Models.PaymentMS;
using VAA.Facade.NDC.IFGFacade.Infrastructure.Models.ReferenceMS;

namespace VAA.Facade.NDC.IFGFacade.Infrastructure.Mapper.PaymentMS
{
    public class PaymentRequestMapper
    {
        public static AuthorizePaymentRequest MapAuthorizeRequest(AuthorizeRequest request, IEnumerable<AirportDetail> airportDetails, IEnumerable<Currency> currencies)
        {
            var segments = MapSegments(request.Orders, airportDetails);
            var payment = request.FormOfPayment.First();
            var travelAgentName = GetMerchantDefinedValue(payment, "TravelAgentName");
            var travelAgentEmail = GetMerchantDefinedValue(payment, "TravelAgentEmail");

            return new AuthorizePaymentRequest()
            {
                Pos = new()
                {
                    CountryCode = request.BasicInformation.CountryCode
                },
                Parties = new()
                {
                    Participants = [
                        new()
                        {
                            PartyType = Participant.PartyTypeEnum.Seller,
                            Ordinal = "1",
                            AgencyID = request.BasicInformation.AgentCode,
                            Name = travelAgentName,
                            Contacts = new() { Email = travelAgentEmail }
                        }
                    ]
                },
                BookingReferences = new()
                {
                    RLOC = request.Orders.First().FlightRecords.First().PassengerRecord.Pnr
                },
                ProductType = request.Orders.First().TransactionCode == "TKTT" ? ProductType.Ticket.ToString() : ProductType.Seat.ToString(),
                JourneyType = MapJourneyType(segments),
                OrderId = $"VS932{request.Orders.First().FlightRecords.First().PassengerRecord.Pnr}{DateTime.UtcNow:yy}",
                PaymentDetails = [MapPaymentDetail(payment, currencies, null)],
                CustomerDetails = MapCustomers(request.Orders),
                Segments = segments,
            };
        }

        private static string GetMerchantDefinedValue(FormOfPayment payment, string key)
        {
            return payment?.MerchantDefinedInformation?
                .FirstOrDefault(x => x?.Key?.Equals(key, StringComparison.OrdinalIgnoreCase) == true)?.Value;
        }

        public static string MapJourneyType(IEnumerable<Segment> segments)
        {
            return segments.Count() == 1
                ? "ONEWAY"
                : segments.First().OriginAirportCode == segments.Last().DestinationAirportCode
                    ? "RETURN"
                    : "MULTICITY";
        }

        private static Payment MapPaymentDetail(FormOfPayment paymentRequest, IEnumerable<Currency> currencies, string transactionId)
        {
            var name = paymentRequest.Payer.Name.Split(' ');

            //This is swapped because the name field is formatted as Last Name followed by First Name.
            var firstname = name.Last();
            var lastname = name.First();

            var expiry = $"{paymentRequest.ExpiryDate[..2]}/{paymentRequest.ExpiryDate.Substring(2, 2)}";

            var (countryCode, phone) = ParsePhone(paymentRequest.Payer?.PhoneNumber);

            return new Payment
            {
                PaymentTransactionId = transactionId,
                Amount = new()
                {
                    MonetaryAmount = (paymentRequest.Amount / (decimal)Math.Pow(10, int.Parse(paymentRequest.Currency[^1..]))).ToString(),
                    BaseCurrency = paymentRequest.Currency[..^1],
                    CurrencyId = currencies.First(a => a.CurrencyCode == paymentRequest.Currency[..^1]).CurrencyNumber
                },
                CardMethod = new()
                {
                    CardholderName = $"{firstname} {lastname}",
                    CardNumber = paymentRequest.AccountNumber,
                    ExpiryDate = expiry,
                    Cvv = paymentRequest.CardSecurityCode,
                    CardType = paymentRequest.Type.Substring(2, 2)
                },
                Payer = new()
                {
                    FirstName = firstname,
                    LastName = lastname,
                    PayerContacts = new()
                    {
                        Email = paymentRequest.Payer?.Email,
                        Phone = phone,
                        CountryDailingCode = countryCode
                    },
                    PayerAddress = new()
                    {
                        AddressLine1 = paymentRequest.Address.HouseNumberOrName,
                        AddressLine2 = paymentRequest.Address.Street,
                        City = paymentRequest.Address.City,
                        Country = paymentRequest.Address.CountryCode,
                        PostalCode = paymentRequest.Address.PostalCode
                    }
                },
                PaymentCardAuthentication = paymentRequest.MotoPayment ? null : new()
                {
                    Eci = paymentRequest.CardAuthentication.EcomInd,
                    ServerTransactionId = paymentRequest.CardAuthentication.ServerTransactionId,
                    Cavv = paymentRequest.CardAuthentication.AuthenticationValue
                },
                TransactionTypeId = paymentRequest.MotoPayment ? 7 : 1009
            };
        }

        private static IEnumerable<Customer> MapCustomers(IEnumerable<Order> orders)
        {
            var customers = new List<Customer>();

            foreach (var order in orders)
            {
                var customer = order.FlightRecords.First().PassengerRecord;
                var customerName = customer.Name.Split(" ");

                var firstName = customerName.Last();
                var lastName = customerName.First();

                if (!customers.Any(a=> a.FirstName.Is(firstName) && a.LastName.Is(lastName)))
                {
                    customers.Add(new Customer()
                    {
                        FirstName = firstName,
                        LastName = lastName,
                        Ptc = customer.TypeCode,
                        DateOfBirth = !string.IsNullOrEmpty(customer.DateOfBirth) && DateTime.TryParseExact(customer.DateOfBirth, "yyyyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var dob) ? dob.ToString("yyyy-MM-dd") : null
                    });
                }

            }

            return customers;
        }

        private static IEnumerable<Segment> MapSegments(IEnumerable<Order> orders, IEnumerable<AirportDetail> airportDetails)
        {
            var segments = new List<Segment>();

            var customerName = orders.First().FlightRecords.First().PassengerRecord.Name;

            var itineraries = orders.SelectMany(o => o.FlightRecords)
                                    .Where(fr => fr.PassengerRecord.Name == customerName)
                                    .SelectMany(fr => fr.DocumentDetails.Itinerary)
                                    .Where(x => x.CouponUseIndicator != Constants.Application.ArnkCouponUseIndicator);

            foreach (var itinerary in itineraries)
            {
                var hasArrivalDateTime = !string.IsNullOrWhiteSpace(itinerary.FlightArrivalDate)
                                         && !string.IsNullOrWhiteSpace(itinerary.FlightArrivalTime);
                var arrivalDate = hasArrivalDateTime ? itinerary.FlightArrivalDate : itinerary.FlightDate;
                var arrivalTime = hasArrivalDateTime ? itinerary.FlightArrivalTime : itinerary.FlightTime;

                segments.Add(new Segment
                {
                    OriginCountryCode = airportDetails.First(x => x.AirportCode == itinerary.OriginCode).CountryCode,
                    OriginAirportCode = itinerary.OriginCode,
                    DestinationCountryCode = airportDetails.First(x => x.AirportCode == itinerary.DestinationCode).CountryCode,
                    DestinationAirportCode = itinerary.DestinationCode,
                    ScheduledDepartureTime = new UtcAndLocalDateTime { LocalDateTime = DateTime.ParseExact(itinerary.FlightDate + itinerary.FlightTime, "yyyyMMddHH:mm:ss", null).ToString("yyyy-MM-ddTHH:mm") },
                    ScheduledArrivalTime = new UtcAndLocalDateTime { LocalDateTime = DateTime.ParseExact(arrivalDate + arrivalTime, "yyyyMMddHH:mm:ss", null).ToString("yyyy-MM-ddTHH:mm") },
                    ClassOfService = itinerary.ReservationBooking,
                    MarketingCarrier = itinerary.Carrier,
                    MarketingFlightNumber = itinerary.FlightNumber,
                    OperatingCarrier = itinerary.Carrier,
                    OperatingFlightNumber = itinerary.FlightNumber,
                    FareBasis = itinerary.FareBasisTicketDesignator
                });
            }

            return segments;
        }

        public static CapturePaymentRequest MapCaptureRequest(OrderRequest request, IEnumerable<AirportDetail> airportDetails, IEnumerable<Currency> currencies, OrderRS orderDocument)
        {
            var segments = MapSegments(request.Orders, airportDetails);

            return new CapturePaymentRequest()
            {
                Pos = new()
                {
                    CountryCode = request.BasicInformation.CountryCode
                },
                Parties = new()
                {
                    Participants = [
                        new()
                        {
                            Ordinal = "1",
                            AgencyID = request.BasicInformation.AgentCode,
                            Name = request.BasicInformation.TravelAgencyName ?? request.BasicInformation.AgentCode,
                            Contacts = new() { Email = request.BasicInformation.TravelAgencyEmail }
                        }
                    ]
                },
                BookingReferences = new()
                {
                    RLOC = request.Orders.First().FlightRecords.First().PassengerRecord.Pnr,
                    ExternalRecordLocator = orderDocument?.OrderDocument?.BookingReference?.ExternalRecordLocator
                },
                ProductType = request.Orders.First().TransactionCode == "TKTT" ? ProductType.Ticket.ToString() : ProductType.Seat.ToString(),
                JourneyType = MapJourneyType(segments),
                OrderId = $"VS932{request.Orders.First().FlightRecords.First().PassengerRecord.Pnr}{DateTime.UtcNow:yy}",
                PaymentDetails = [MapCapturePaymentDetail(request.FormOfPayment.First(), currencies, request.TransactionId)],
                CustomerDetails = MapCustomers(request.Orders),
                Segments = segments,
                OrderItems = MapOrderItems(orderDocument?.OrderDocument),
                AccountableDocumentRefs = [.. request.Orders.SelectMany(order => order.FlightRecords).Select(flightRecord => flightRecord.DocumentID.TicketDocumentNumber)
                                            .Where(ticketNumber => !string.IsNullOrEmpty(ticketNumber)).Distinct()
                                            .Select(ticketNumber => new AccountableDocumentRef { DocumentNumber = ticketNumber })],
            };
        }

        private static IEnumerable<Models.PaymentMS.OrderItems> MapOrderItems(OrderDocument orderDoc)
        {
            var orderItems = new List<Models.PaymentMS.OrderItems>();

            if(orderDoc?.OrderItems == null) return orderItems;

            foreach(var oi in orderDoc.OrderItems)
            {
                if (oi == null) continue;
                var firstServices = oi.Services?.FirstOrDefault();

                orderItems.Add(new Models.PaymentMS. OrderItems
                {
                    OrderItemId = oi.OrderItemId,
                    Services = firstServices != null ? [new OrderService() { ServiceBeneficiaries = firstServices?.ServiceBeneficiaries }] : Enumerable.Empty<OrderService>(),
                    PriceDetails = [new Price()
                    {
                        Amount = oi.AddCollectPrice != null ? oi.AddCollectPrice.Amount : oi.PriceDetails?.Where( x => x.PriceComponentType == Price.PriceComponentTypes.BasePriceEnum)?.Select(x => x.Amount)?.FirstOrDefault()
                    }],
                    CustRefIds = oi.Services.SelectMany(s => s.ServiceBeneficiaries).Distinct(),
                }); 
            }
            return orderItems;
        }

        private static Payment MapCapturePaymentDetail(FormOfPayment paymentRequest, IEnumerable<Currency> currencies, string transactionId)
        {
            var name = paymentRequest.Payer?.Name?.Split(' ');

            return new Payment
            {
                PaymentTransactionId = transactionId,
                Amount = new()
                {
                    MonetaryAmount = (paymentRequest.Amount / (decimal)Math.Pow(10, int.Parse(paymentRequest.Currency[^1..]))).ToString(),
                    BaseCurrency = paymentRequest.Currency[..^1],
                    CurrencyId = currencies.First(a => a.CurrencyCode == paymentRequest.Currency[..^1]).CurrencyNumber
                },
                CardMethod = new()
                {
                    CardType = paymentRequest.Type.Substring(2, 2)
                },
                Payer = new()
                {
                    FirstName = name?.Last(),
                    LastName = name?.First(),
                    PayerContacts = new()
                    {
                        Email = paymentRequest.Payer?.Email,
                        Phone = !string.IsNullOrEmpty(paymentRequest.Payer?.PhoneNumber) ? paymentRequest.Payer.PhoneNumber[3..] : null,
                        CountryDailingCode = !string.IsNullOrEmpty(paymentRequest.Payer?.PhoneNumber) ? paymentRequest.Payer.PhoneNumber.Substring(1, 2) : null
                    },
                },
            };
        }

        public static (string? countryCode, string? phone) ParsePhone(string? phoneNumber)
        {
            if (string.IsNullOrWhiteSpace(phoneNumber) || phoneNumber.Length < 4)
                return (null, null);
            if (!phoneNumber.Contains("-"))
                return (null, null);
            
            return (
                phoneNumber.Split("-")[0].TrimStart('0'),
                phoneNumber.Split("-")[1]
            );
        }
    }
}
