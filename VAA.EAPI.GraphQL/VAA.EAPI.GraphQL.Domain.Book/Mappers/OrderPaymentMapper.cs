using VAA.EAPI.GraphQL.Common.Extensions;
using VAA.EAPI.GraphQL.Common.Helper;
using VAA.EAPI.GraphQL.Domain.Book.Models.Basket;
using VAA.EAPI.GraphQL.Domain.Book.Models.BookGraphQLModels;
using VAA.EAPI.GraphQL.Domain.Book.Models.Common;
using VAA.EAPI.GraphQL.Domain.Book.Models.Common.Enums;
using VAA.EAPI.GraphQL.Domain.Book.Models.Customer;
using VAA.EAPI.GraphQL.Domain.Book.Models.Offer;
using VAA.EAPI.GraphQL.Domain.Book.Models.Payment;
using VAA.EAPI.GraphQL.Domain.Book.Models.Products.Flight;
using VAA.EAPI.GraphQL.Domain.Book.Models.Reference;
using VAA.EAPI.GraphQL.Domain.Book.Models.Request.PaymentRequest;
using VAA.EAPI.GraphQL.Domain.Book.Models.Response.Loyalty;
using VAA.EAPI.GraphQL.Domain.Book.Models.Response.Offer;
using VAA.EAPI.GraphQL.Domain.Book.Models.Response.Payment;
using VAA.EAPI.GraphQL.Domain.Book.Models.Seats;
using static VAA.EAPI.GraphQL.Domain.Book.Models.Common.Price;

namespace VAA.EAPI.GraphQL.Domain.Book.Mappers
{
    public class OrderPaymentMapper
    {
        public static OrderPaymentRequestMs GetOrderPaymentRequest(OrderPaymentRequest orderPaymentRequest, OfferResponse offerResponse, Basket basket, IEnumerable<CurrencyNumberRefData> currencyNumbers, IEnumerable<SeatMSResponse> seatMSResponse, IEnumerable<CabinRefData> cabinRefData, bool payWithPoints, LoyaltyPointsEquivalentCurrency selectedPayWithPointsOption, double insurancePrice)
        {
            bool disableSeatIntegrationFlag = ConfigReader.Try<bool>(Constants.Environment.SeatIntegrationFlag, false);
            var offerService = offerResponse.Offers.First().OfferItems.First().Services;

            offerResponse.Pos.BrowserInfo = orderPaymentRequest.BrowserInfo;
            offerResponse.Pos.DeviceInfo = orderPaymentRequest.DeviceInfo;
            offerResponse.Pos.IpAddress = orderPaymentRequest.IpAddress;
            offerResponse.Pos.CurrencyNumber = (currencyNumbers.FirstOrDefault(c => c.CurrencyCode == offerResponse.Offers.First().SalePrice.First().Amount.BaseCurrency) ?? currencyNumbers.First(c => c.CurrencyCode == Constants.Application.DefaultCurrencyCode)).CurrencyNumber;

            return new OrderPaymentRequestMs()
            {
                Pos = offerResponse.Pos,
                CustomerDetails = GetCustomerInfo(basket).Select(people =>
                {
                    switch (people.Ptc)
                    {
                        case "ADULT":
                            people.Ptc = Constants.Application.DeltaPtcAdult;
                            break;
                        case "CHILD":
                            people.Ptc = Constants.Application.DeltaPtcChild;
                            break;
                        case "TEEN":
                            people.Ptc = Constants.Application.DeltaPtcTeen;
                            break;
                        case "INFANT":
                            people.Ptc = Constants.Application.DeltaPtcInfant;
                            break;
                        default:
                            throw new ArgumentOutOfRangeException(nameof(people.Ptc));
                    }
                    return people;
                }),
                LoyaltyMemberDetails = (offerResponse.IsAwardSearchResult || payWithPoints) && basket.CustomerProfile != null ? new()
                {
                    FirstName = basket.CustomerProfile.FirstName,
                    LastName = basket.CustomerProfile.FirstName,
                    MemberNumber = basket.CustomerProfile.MembershipDetails.FQTNumber,
                    MemberTierCode = basket.CustomerProfile.MembershipDetails.FQTTierCode
                } : null,
                paymentAmt = new Payment
                {
                    CurrencyEquivalentPrice = new CurrencyEquivalentPrice
                    {
                        Amount = new Amount
                        {
                            MonetaryAmount = payWithPoints ? GetCurrencyAmount(offerResponse, selectedPayWithPointsOption, basket) : offerResponse.Offers.First().SalePrice.FirstOrDefault(sale => sale.PriceComponentType == Price.PriceComponentTypes.BasePriceEnum)?.Amount?.MonetaryAmount,
                            Currency = offerResponse.Offers.First().SalePrice.First().Amount.BaseCurrency,
                            DecimalPrecisionCount = offerResponse.Offers.First().SalePrice.First().Amount.DecimalPrecisionCount,
                        },
                    },
                    MilesEquivalentPrice = (offerResponse.IsAwardSearchResult || payWithPoints) && basket.CustomerProfile != null ? new MilesEquivalentPrice()
                    {
                        CashPlusMiles = !offerResponse.IsAwardSearchResult && payWithPoints,
                        MileCount = payWithPoints ? selectedPayWithPointsOption.LoyaltyPoints : Convert.ToString(offerResponse.Offers.First().SalePrice.First(x => x.PriceComponentType == PriceComponentTypes.PointsEnum).PointsAmount.PointsCount)
                    } : null,
                },
                Selectedoffers = new[]
                {
                    new Offer()
                    {
                        ProductCode = string.Empty,
                        ProductType = EnumOperations.GetEnumDescription(Product.ProductTypes.FlightEnum),
                        Services = new[]
                        {
                            new Service()
                            {
                                FlightSegmentsRefIds = offerService.FirstOrDefault()?.FlightSegmentsRefIds,
                                JourneyRefIds = offerService.FirstOrDefault()?.JourneyRefIds,
                                FareDetails = new FareDetail()
                                {
                                   FareBasisCode = offerService.FirstOrDefault()?.FareDetails.FareBasisCode,
                                   FareFamilyType = offerService.FirstOrDefault()?.FareDetails.FareFamilyType,
                                   FareSegments = offerService.FirstOrDefault()?.FareDetails.FareSegments.Select(seg=> new FareSegment() { BookingClass = seg.BookingClass, CabinName = seg.CabinName })
                                }
                            }
                        },
                        ServiceBeneficiaries =  offerService.FirstOrDefault()?.ServiceBeneficiaries
                    }
                },
                FlightSegments = MapFlightSegments(offerResponse.DataList.FlightSegments, offerService),
                Journey = offerResponse.DataList.Journey,
                TripType = GetTripTypeFromOffer(offerResponse)?.ToString(),
                AdditionalConfigParams = new AdditionalConfigParams()
                {
                    FailureRedirectUrl = orderPaymentRequest.PaymentFailureLink,
                    SuccessRedirectUrl = orderPaymentRequest.PaymentSuccessLink,
                    MultiProductFlag = false,
                    IsInsuranceAdded = basket.IsInsuranceAdded,
                },
                InsuranceDetails = basket.IsInsuranceAdded ?? false ? new() { Amount = new() { MonetaryAmount = Convert.ToString(insurancePrice) } } : null,
                SeatDetails = disableSeatIntegrationFlag ? null : seatMSResponse.Any() ? MapSeatDetails(basket, seatMSResponse, cabinRefData) : null
            };
        }

        public static TripTypeEnum? GetTripTypeFromOffer(OfferResponse offer)
        {
            var ods = offer?.DataList?.OriginDestinationAssociations;

            if (ods?.Count() == 1) return TripTypeEnum.ONEWAY;

            if (ods?.Count() == 2 &&
                ods?.ElementAt(0)?.OriginDestination?.OriginAirport == ods?.ElementAt(1)?.OriginDestination?.DestinationAirport &&
                ods?.ElementAt(0)?.OriginDestination?.DestinationAirport == ods?.ElementAt(1)?.OriginDestination?.OriginAirport)
                return TripTypeEnum.RETURN;
            if (ods?.Count() >= 2)
                return TripTypeEnum.MULTICITY;
            return null;
        }


        public static string GetCurrencyAmount(OfferResponse offer, LoyaltyPointsEquivalentCurrency selectedPayWithPointsOption, Basket basket)
        {
            decimal amount = Convert.ToDecimal(offer.Offers.First().SalePrice.First(sale => sale.PriceComponentType == Price.PriceComponentTypes.BasePriceEnum).Amount?.MonetaryAmount) - Convert.ToDecimal(selectedPayWithPointsOption.Amount.MonetaryAmount);

            if (amount == 0m && !basket.FlightPassengerSeatMap.IsNullOrEmpty() && basket.FlightPassengerSeatMap.Any(seat => !seat.SeatNumber.IsNullOrEmpty() && seat?.Price?.Amount != 0))
                return offer.Offers.First().SalePrice.First(sale => sale.PriceComponentType == Price.PriceComponentTypes.BasePriceEnum)?.Amount?.MonetaryAmount;

            return amount.ToString();
        }

        public static IEnumerable<SeatList> MapSeatDetails(Basket basket, IEnumerable<SeatMSResponse> seatMSResponse, IEnumerable<CabinRefData> cabinRefData)
        {
            return basket?.FlightPassengerSeatMap?.Where(flightpassengerSeatMap => !string.IsNullOrEmpty(flightpassengerSeatMap?.SeatNumber)).Select(flightpassengerSeatMap => new SeatList()
            {
                PassengerSequence = (Convert.ToInt32(flightpassengerSeatMap.PassengerId) + 1).ToString(),
                TripId = flightpassengerSeatMap.SectorId.Split('-').First(),
                TripSeq = flightpassengerSeatMap.SectorId.Split('-').First(),
                PendingSeatNumber = flightpassengerSeatMap.SeatNumber,
                PendingSeatPriceAmount = flightpassengerSeatMap.Price.Amount.ToString(),
                PendingSeatDecimalPrecisionCount = BitConverter.GetBytes(decimal.GetBits(flightpassengerSeatMap.Price.Amount)[3])[2].ToString(),
                PendingSeatCurrencyCode = flightpassengerSeatMap.Price.Currency,
                SeatOfferDetails = GetSeatOfferDetails(seatMSResponse?.First(x => x.SeatOfferId == flightpassengerSeatMap.SeatOfferId), flightpassengerSeatMap, cabinRefData)
            });
        }

        public static SeatOfferDetails GetSeatOfferDetails(SeatMSResponse seatMSResponse, Models.Request.Seat.FlightPassengerSeatMap flightPassengerSeatMap, IEnumerable<CabinRefData> cabinRefData)
        {
            var offerId = seatMSResponse.FlightSegmentSeatMap.First().Seatmap.SelectMany(seatMap => seatMap.SeatRows).SelectMany(row => row.SeatColumns).Where(column => column.SeatIdentifer == flightPassengerSeatMap.SeatNumber).SelectMany(column => column.Eligibility).Where(eligibility => eligibility.CustomerId.Contains(flightPassengerSeatMap.PassengerId)).Select(eligibility => eligibility.CatalogueRefId).FirstOrDefault();

            return new()
            {
                DeltaSeatOfferId = offerId,
                BrandDescription = cabinRefData.First(c => c.BrandID == seatMSResponse.FlightSegmentSeatMap.First().Catalogue.First(catalogue => catalogue.CatalogueId == offerId).BrandId)?.BrandName,
                ProductDescription = seatMSResponse.FlightSegmentSeatMap.First().Catalogue.First(catalogue => catalogue.CatalogueId == offerId).ProductName
            };
        }

        public static IEnumerable<Segment> MapFlightSegments(IEnumerable<Segment> segments, IEnumerable<OfferService> offerServices)
        {
            return segments.Select(seg => new Segment()
            {
                OdRefId = seg.OdRefId,
                SegmentId = seg.SegmentId,
                AirCraft = seg.AirCraft,
                OriginAirportCode = seg.OriginAirportCode,
                OriginCityCode = seg.OriginCityCode,
                DestinationAirportCode = seg.DestinationAirportCode,
                DestinationCityCode = seg.DestinationCityCode,
                ScheduledArrivalTime = seg.ScheduledArrivalTime,
                ScheduledDepartureTime = seg.ScheduledDepartureTime,
                FlightDuration = seg.FlightDuration,
                MarketingFlightNumber = seg.MarketingFlightNumber,
                OperatingFlightNumber = seg.OperatingFlightNumber,
                ClassOfService = offerServices.FirstOrDefault(o => o.FlightSegmentsRefIds.Contains(seg.SegmentId))?.FareDetails?.FareSegments?.First().BookingClass,
                MarketingCarrier = seg.MarketingCarrier,
                OperatingCarrier = seg.OperatingCarrier,
                Legs = seg.Legs,
                ConnectionOrStopOver = seg.ConnectionOrStopOver,
                FareBasis = offerServices.FirstOrDefault(o => o.FlightSegmentsRefIds.Contains(seg.SegmentId))?.FareDetails?.FareBasisCode
            });
        }

        public static PaymentOrderResponse GetOrderPaymentResponse(decimal amount, OfferResponse offerResponse, Basket basket, IEnumerable<AirportRefData> airportData, IEnumerable<AirlineRefData> airlineData, IEnumerable<CabinRefData> cabinData, PaymentSession session = null)
        {
            return new PaymentOrderResponse()
            {
                AmountToPay = amount,
                PaymentSessions = new[] {
                    new PaymentSession()
                    {
                        AmountToPay = amount,
                        PaymentLink = session.PaymentLink,
                        HppPaymentStatus = session.HppPaymentStatus
                    } },
                MinDeposit = amount,
                TotalAmount = amount,
                TotalAmountOnSessions = amount,
                Summary = (Summary)BasketSummaryMapper.MapFlightSummary(offerResponse, basket, airportData, airlineData, null, cabinData),
            };
        }

        private static IEnumerable<CustomerInfo> GetCustomerInfo(Basket basket)
        {

            return basket.People.Select(people => new CustomerInfo()
            {
                CustId = people.Id,
                FirstName = people.FirstName,
                LastName = people.LastName,
                DateOfBirth = people.DateOfBirth?.ToString("yyyy-MM-dd"),
                Ptc = people.Type,
                Gender = people.Gender.Parse<EnumGenderCode>() switch
                {
                    EnumGenderCode.Male => Constants.Application.DeltaGenderCodeMale,
                    EnumGenderCode.Female => Constants.Application.DeltaGenderCodeFemale,
                    EnumGenderCode.Undisclosed => Constants.Application.DeltaGenderCodeUndisclosed,
                    EnumGenderCode.Unspecified => Constants.Application.DeltaGenderCodeUnspecified,
                    _ => throw new ArgumentOutOfRangeException(nameof(people.Gender))
                },
                Contacts = new Contacts
                {
                    Email = basket.ContactDetails.EmailAddress,
                    Phone = basket.ContactDetails.PhoneNumber,
                    PhoneAreaNumber = basket.ContactDetails.PhoneNumber.Substring(0, 3),
                    Countrycode = basket.ContactDetails.CountryCode
                }
            });
        }

        public static IEnumerable<PaymentSessionBasketMSRequest> UpdateBasketPaymentSessionRequest(PaymentSession paymentSession)
        {
            return new[] {
                new PaymentSessionBasketMSRequest
                {
                    AmountToPay = paymentSession.AmountToPay,
                    PaymentLink = paymentSession.PaymentLink,
                    PaymentReferenceId =paymentSession.PaymentReferenceId,
                    ParentPaymentReferenceId = paymentSession.ParentPaymentReferenceId,
                    HppPaymentStatus = paymentSession.HppPaymentStatus,
                    SelectedPayWithPointsOption = paymentSession.SelectedPayWithPointsOption
                }
            };
        }
    }
}
