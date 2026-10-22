using System;
using System.Collections.Generic;
using System.Linq;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Domain.Virgin.Models.Customer;
using VAA.EAPI.Domain.Virgin.Models.Order;
using VAA.EAPI.Domain.Virgin.Models.Payment;
using VAA.EAPI.Domain.Virgin.Models.Products.Flight;
using VAA.EAPI.Domain.Virgin.Models.Ticket;
using VAA.EAPI.Infrastructure.PSS.Constants;
using VAA.EAPI.Infrastructure.PSS.Proxy.TicketingProcessV3Proxy;
using Passenger = VAA.EAPI.Domain.Virgin.Models.Customer.Passenger;

namespace VAA.EAPI.Infrastructure.PSS.Mapper
{
    public class TicketingProcessPerformRevenueMapper
    {
        public static PerformRevenueRequestType RequestMapper(string sessionId, List<PaymentDetails> paymentsDetails, List<Customer> customer)
        {
            var payDetails = paymentsDetails.Any() ? paymentsDetails.First() : throw new ArgumentNullException(nameof(paymentsDetails));

            return new PerformRevenueRequestType()
            {
                RequestInfo = new RequestInfoType
                {
                    ApplicationId = ConfigReader.Get(Constants.Environment.ApplicationId),
                    DebugOptions = ConfigReader.Get(Constants.Environment.DebugOptions),
                    TestLab = ConfigReader.Get(Constants.Environment.TestLab),
                    SessionId = sessionId,
                    HostContextParams = new HostContextParamsType
                    {
                        City = ConfigReader.Get(Constants.Environment.AgentCityCode),
                        PoolType = ConfigReader.Get(Constants.Environment.PoolType)
                    }
                },
                StampingInfo = new StampingInfoType
                {
                    UserAuthorizationData = new UserAuthorizationDataType
                    {
                        AgentCityCode = ConfigReader.Get(Constants.Environment.AgentCityCode),
                        RoleIndicator = ConfigReader.Get(Constants.Environment.RoleIndicator),
                        SecurityId = ConfigReader.Get(Constants.Environment.SecurityId)
                    }
                },
                RequestOptionsType = new RequestOptionType()
                {
                    RequestOption2 = "R"
                },
                CreditCardDataDescription = new CreditCardDataDescriptionType()
                {
                    AuthorizedInd = "N",
                    BillingAddressCtryCd = payDetails.CardMethod.BillingAddress.Region,
                    CCFormatType = "T",
                    CVCCIdNumber = payDetails.CardMethod.Cvv,
                    CreditCardExpiration = payDetails.CardMethod.ExpiryDate,
                    CreditCardNumber = payDetails.CardMethod.CardNumber,
                    CreditCardType = payDetails.CardMethod.CardType.ToString(),
                    CurrencyCode = payDetails.Amount.BaseCurrency,
                    CustomerFirstName = payDetails.Payer.FirstName,
                    CustomerLastName = payDetails.Payer.LastName,
                    PaymentAmount = payDetails.Amount.MonetaryAmount.ToString()
                },
                Email = new EmailType()
                {
                    EmailAddress = customer.First(x => x.Ptc == "ADT").Contacts.Email,
                    SendEmailData = "Y"
                },

                PaymentTransactionInfo = String.IsNullOrEmpty(payDetails.PaymentReferenceId) ? null : new PaymentTransactionInfoType { VendorFormattedPaymentReferenceId = payDetails.PaymentReferenceId, VendorFopTypeDetailText = Constants.Application.VendorFopType },
            };
        }

        public static BookingDetails ResponseMapper(PnrType response)
        {

            return new BookingDetails()
            {
                BookingReference = new BookingReference { RLOC = response.RecordLocator.First().RecordLocator1 },
                SegmentDetails = SegmentInformation(response),
                Passengers = PassengerInformation(response),
                AccountableDocumentRef = AccountableDocumentInfo(response),
                TicketSegments = TicketSegmentInformation(response)
            };
        }

        private static List<Segment> SegmentInformation(PnrType pnrType)
        {
            List<Segment> segmentInfo = new();
            foreach (var segDl in pnrType.SegmentDl)
            {
                segmentInfo.Add(new Segment
                {

                    ClassOfService = segDl.ClassOfService,
                    DestinationAirportCode = segDl.ArrivalPoint,
                    MarketingCarrier = segDl.AirlineCode,
                    MarketingFlightNumber = segDl.FlightNumber,
                    OriginAirportCode = segDl.DeparturePoint,
                    SegmentNumber = segDl.SegmentNumber,
                    ScheduledDepartureTime = new()
                    {
                        LocalDateTime = DateTimeHelper.Air4DepartDateFormatConversion(segDl.DepartureDate, segDl.SegmentDepartureYear, segDl.DepartureTime)?.ToString(Constants.Application.DateTimeFormat),
                        UtcDateTime = string.Empty
                    },
                    Legs = null,
                    FareBasis = pnrType.DtiSegmentInfo.First(x => x.SegmentNumber == segDl.SegmentNumber).FarebasisFareclassCode,
                });
            }
            return segmentInfo;
        }

        private static List<Passenger> PassengerInformation(PnrType pnrType)
        {
            List<Passenger> PassengerIfo = new();
            foreach (var psg in pnrType.Passenger)
            {

                PassengerIfo.Add(new Passenger
                {

                    CustomerId = psg.CustomerId,
                    FirstName = psg.FirstName,
                    FirstNameNumber = psg.FirstNameNumber,
                    LastNameNumber = psg.LastNameNumber,
                    LastName = psg.LastName,
                    Title = psg.Title,
                });
            }

            return GetInfantDetails(pnrType, PassengerIfo);
        }

        private static List<Passenger> GetInfantDetails(PnrType pnrType, List<Passenger> PassengerIfo)
        {
            foreach (var X in pnrType.TicketData)
            {
                if (X.PassengerName.Substring(0, 2).Contains(Application.InfIdentificationChars))
                {
                    string[] PassengerName = X.PassengerName.Split(Application.InfSplitChar);
                    if (PassengerName.Count() > 2)
                    {

                        PassengerIfo.Add(new Passenger { FirstName = PassengerName[2], LastName = PassengerName[1], FirstNameNumber = X.FirstNameNumber, LastNameNumber = X.LastNameNumber });
                    }
                }
            }
            return PassengerIfo;
        }

        private static List<AccountableDocumentRef> AccountableDocumentInfo(PnrType pnrType)
        {
            List<AccountableDocumentRef> accountableDocumentRef = new();
            foreach (var ticketData in pnrType.TicketData)
            {
                var couponData = pnrType?.TicketCouponInfo.Where(x => x.TicketNumber == ticketData.TicketNumber);
                accountableDocumentRef.Add(new AccountableDocumentRef
                {
                    DocumentType = AccountableDocumentRef.DocumentTypes.TEnum,
                    Coupons = CouponInfo(couponData, pnrType),
                    DocumentNumber = ticketData.TicketNumber,
                    IssueDate = ticketData.TicketIssueDate,
                    FirstNameNumber = ticketData.FirstNameNumber,
                    LastNameNumber = ticketData.LastNameNumber,
                });
            }
            return accountableDocumentRef;
        }

        private static IEnumerable<Coupon> CouponInfo(IEnumerable<TicketCouponInfo> ticketCouponInfo, PnrType pnrType)
        {

            return ticketCouponInfo.Select(ticketCoupon =>
            {
                return new Coupon
                {
                    CouponNumber = ticketCoupon.CouponNumber,
                    CheckDigit = ticketCoupon.CheckDigit,
                    CouponStatus = ticketCoupon.CouponStatus,
                    ConnectionTicketNumber = ticketCoupon.ConnectionTicketNumber,
                    SegmentRefId = pnrType.SegmentDl.First(x => x.DeparturePoint == ticketCoupon.DeparturePoint && x.ArrivalPoint == ticketCoupon.ArrivalPoint).SegmentNumber
                };
            });
        }

        private static List<TicketSegment> TicketSegmentInformation(PnrType pnrType)
        {
            List<TicketSegment> segmentInfo = new();
            foreach (var segDl in pnrType.DtiSegmentInfo)
            {
                if (!string.IsNullOrEmpty(segDl.SegmentNumber))
                {
                    var flightSegments = pnrType.SegmentDl.First(x => x.SegmentNumber == segDl.SegmentNumber);
                    segmentInfo.Add(new TicketSegment
                    {

                        ClassOfService = segDl.ClassOfService,
                        MarketingCarrier = segDl.Carrier,
                        MarketingFlightNumber = segDl.FlightNumber,
                        OriginAirportCode = segDl.DeparturePoint,
                        SegmentNumber = segDl.SegmentNumber,
                        ScheduledDepartureTime = new()
                        {
                            LocalDateTime = DateTimeHelper.Air4DepartDateFormatConversion(flightSegments.DepartureDate, flightSegments.SegmentDepartureYear, flightSegments.DepartureTime)?.ToString(Constants.Application.DateTimeFormat),
                        },
                        Legs = null,
                        ExpandedPassengerTypeCode = segDl.ExpandedPassengerTypeCode,
                        PassengerTypeCode = segDl.PassengerTypeCode,
                        FareBasis = segDl.FarebasisFareclassCode,
                    });
                }
            }
            return segmentInfo;
        }
    }

}

