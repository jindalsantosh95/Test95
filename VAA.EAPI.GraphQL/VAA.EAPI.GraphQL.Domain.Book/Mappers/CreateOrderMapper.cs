using VAA.EAPI.GraphQL.Common;
using VAA.EAPI.GraphQL.Common.Entities;
using VAA.EAPI.GraphQL.Common.Extensions;
using VAA.EAPI.GraphQL.Common.Helper;
using VAA.EAPI.GraphQL.Common.Serialisation;
using VAA.EAPI.GraphQL.Domain.Book.Constants;
using VAA.EAPI.GraphQL.Domain.Book.Helpers;
using VAA.EAPI.GraphQL.Domain.Book.Models.Basket;
using VAA.EAPI.GraphQL.Domain.Book.Models.BookGraphQLModels;
using VAA.EAPI.GraphQL.Domain.Book.Models.Common;
using VAA.EAPI.GraphQL.Domain.Book.Models.Common.Enums;
using VAA.EAPI.GraphQL.Domain.Book.Models.Customer;
using VAA.EAPI.GraphQL.Domain.Book.Models.Offer;
using VAA.EAPI.GraphQL.Domain.Book.Models.Payment;
using VAA.EAPI.GraphQL.Domain.Book.Models.Reference;
using VAA.EAPI.GraphQL.Domain.Book.Models.Request.Order;
using VAA.EAPI.GraphQL.Domain.Book.Models.Response.Loyalty;
using VAA.EAPI.GraphQL.Domain.Book.Models.Response.Offer;
using VAA.EAPI.GraphQL.Domain.Book.Models.Response.Order;
using VAA.EAPI.GraphQL.Domain.Book.Models.Response.Payment;
using VAA.EAPI.GraphQL.Domain.Book.Models.Seats;
using static VAA.EAPI.GraphQL.Domain.Book.Models.Common.Price;
using static VAA.EAPI.GraphQL.Domain.Book.Models.Payment.CardDetails;
using static VAA.EAPI.GraphQL.Domain.Book.Models.Payment.PaymentDetails;
using BasketFlightPassengerSeatMap = VAA.EAPI.GraphQL.Domain.Book.Models.Request.Seat.FlightPassengerSeatMap;
using Product = VAA.EAPI.GraphQL.Domain.Book.Models.Common.Product;
using SummaryFlightPassengerSeatMap = VAA.EAPI.GraphQL.Domain.Book.Models.BookGraphQLModels.FlightPassengerSeatMap;

namespace VAA.EAPI.GraphQL.Domain.Book.Mappers
{
    public static class CreateOrderMapper
    {
        public static OrderCreateRequestMS GetOrderCreateRequest(OfferResponse offerResponse, Basket basket, PaymentResponse payment, OrderCreateGraphQLRequest graphQLRequest, string paymentReferenceId, IEnumerable<SeatMSResponse> seatMSResponse, IEnumerable<AirportRefData> airportRefData)
        {
            offerResponse.Pos.BrowserInfo = graphQLRequest.BrowserInfo;
            offerResponse.Pos.DeviceInfo = graphQLRequest.DeviceInfo;
            offerResponse.Pos.IpAddress = graphQLRequest.IpAddress;

            graphQLRequest.MetaPartner ??= [];

            graphQLRequest.MetaPartner.AddOrUpdate("MetaPartnerId", basket.MetaPartnerId);

            var voucherDetails = GetVoucherDetails(basket, offerResponse);

            return new OrderCreateRequestMS()
            {
                Pos = offerResponse.Pos,
                CustomerDetails = GetCustomerInfo(basket),
                SelectedOffers = GetSelectedFlightPlusSeatOffers(offerResponse, basket, seatMSResponse),
                PaymentDetails = GetPaymentDetails(offerResponse, basket, payment, paymentReferenceId),
                CreditCardCheckConsent = graphQLRequest.CcCheckConsent,
                IsInsuranceAdded = basket.IsInsuranceAdded,
                InsuranceDetails = MapInsuranceDetails(offerResponse, basket, airportRefData, paymentReferenceId),
                VoucherApplied = voucherDetails is not null,
                VoucherDetails = voucherDetails,
                MetaPartner = graphQLRequest.MetaPartner
            };
        }

        public static OrderVoucherDetails GetVoucherDetails(Basket basket) => GetVoucherDetails(basket, null);

        public static OrderVoucherDetails GetVoucherDetails(Basket basket, OfferResponse offerResponse)
        {
            if (basket?.VoucherApplied != true)
                return null;

            var selectedOptionId = GetVoucherRedemptionSelection(basket)?.SelectedOptionId;
            var selectedOption = basket?.VoucherDetails?.RedemptionOptions?
                .FirstOrDefault(option => option.OptionId == selectedOptionId);

            if (selectedOption is null)
            {
                return null;
            }

            return new OrderVoucherDetails
            {
                VoucherOwnerFcAccount = basket?.CustomerProfile?.MembershipDetails?.FQTNumber,
                RedemptionType = selectedOption.RedemptionType,
                AppliedVouchers = (selectedOption.AppliedVouchers ?? Enumerable.Empty<VAA.EAPI.GraphQL.Domain.Book.Models.FlightSummary.AppliedVoucher>())
                    .Select(voucher => new OrderVoucherRedemption
                    {
                        VoucherId = voucher.VoucherId,
                        PointsUsed = voucher.PointsUsed
                    }),
                TotalPointsUsed = GetTotalAwardPoints(offerResponse) ?? selectedOption.TotalPointsUsed
            };
        }

        private static long? GetTotalAwardPoints(OfferResponse offerResponse) => offerResponse?.Offers?
            .FirstOrDefault()?
            .SalePrice?
            .FirstOrDefault(price => price.PriceComponentType == PriceComponentTypes.PointsEnum)?
            .PointsAmount?
            .PointsCount;

        public static IEnumerable<PaymentDetails> GetPaymentDetails(OfferResponse offerResponse, Basket basket, PaymentResponse payment, string paymentReferenceId)
        {
            var loyaltyPayer = GetLoyaltyPayer(basket);
            var loyaltyPayerFirstName = basket?.CustomerProfile?.FirstName.IsNullOrWhiteSpace() == false
                ? basket.CustomerProfile.FirstName
                : loyaltyPayer?.FirstName;
            var loyaltyPayerLastName = basket?.CustomerProfile?.LastName.IsNullOrWhiteSpace() == false
                ? basket.CustomerProfile.LastName
                : loyaltyPayer?.LastName;

            if (basket.FlightsearchRequest.isAwardSearch && (loyaltyPayerFirstName.IsNullOrWhiteSpace() || loyaltyPayerLastName.IsNullOrWhiteSpace()))
                throw new ArgumentException("Loyalty member first name and last name are required for an award booking.");

            var paymentDetail = payment?.PaymentDetails.Select(paymentDetails => new PaymentDetails()
            {
                FormOfPaymentId = paymentDetails?.FormOfPaymentId,
                FormOfPayments = paymentDetails.FormOfPayments,
                Amount = GetAmount(basket, paymentDetails, offerResponse),
                CardMethod = new CardDetails
                {
                    CardNumber = paymentDetails?.CardMethod?.CardNumber,
                    CardType = paymentDetails.CardMethod?.CardType == null ? null : (CardTypes)System.Enum.Parse(typeof(CardTypes), paymentDetails?.CardMethod?.CardType.ToString()),
                    CardholderName = $"{paymentDetails?.Payer?.FirstName} {paymentDetails?.Payer?.LastName}",
                    ExpiryDate = paymentDetails?.CardMethod?.ExpiryDate,
                    Cvv = paymentDetails?.CardMethod?.Cvv,
                    BillingAddress = new Address
                    {
                        AddressLine1 = paymentDetails?.BillingAddress.AddressLine1,
                        City = paymentDetails?.BillingAddress.City,
                        PostalCode = paymentDetails?.BillingAddress.PostalCode,
                        Region = paymentDetails?.BillingAddress.Region,
                        Country = paymentDetails?.BillingAddress.Country
                    }
                },
                Payer = new PaymentPayer { FirstName = paymentDetails?.Payer.FirstName, LastName = paymentDetails?.Payer.LastName },
                FinancialInstitution = paymentDetails?.FinancialInstitution ?? null,
                PaymentRemarks = paymentDetails.PaymentRemarks,
                PaymentReferenceId = paymentReferenceId,
                PaymentCardAuthorization = paymentDetails?.PaymentCardAuthorization == null ? null : new() { AcquirerReferenceDataText = paymentDetails.PaymentCardAuthorization?.AcquirerReferenceDataText, VendorPaymentTransactionId = paymentDetails.PaymentCardAuthorization?.VendorPaymentTransactionId },
            });

            if (basket.IsInsuranceAdded == true && Convert.ToDouble(payment.PaymentDetails.FirstOrDefault()?.Amount?.MonetaryAmount) == basket.InsuranceQuotes.First().Products.First().TotalPrice)
            {
                paymentDetail = null;
            }

            IEnumerable<PaymentDetails> paymentDetails = paymentDetail ?? Enumerable.Empty<PaymentDetails>();

            if (basket.FlightsearchRequest.isAwardSearch)
            {
                var milesDetail = new PaymentDetails()
                {
                    FormOfPaymentId = (paymentDetails.Count() + 1).ToString(),
                    FormOfPayments = PaymentDetails.FormOfPayment.MilesPoint,
                    Payer = new()
                    {
                        FirstName = loyaltyPayerFirstName,
                        LastName = loyaltyPayerLastName,
                        PayerType = Constants.Application.DeltaPtcAdult,
                        LoyaltyMemberDetails = new()
                        {
                            MemberNumber = basket.CustomerProfile.MembershipDetails.FQTNumber
                        },
                        LoyaltyMemberTravelling = basket.People.Any(p => p.MembershipDetails.FQTNumber == basket.CustomerProfile.MembershipDetails.FQTNumber)
                    },
                    MilesPoints = GetAwardPointsToPay(offerResponse, basket)
                };
                paymentDetails = paymentDetails.Append(milesDetail);
            }

            if (basket.PtcWisePointsEquivalentCurrencyDetails != null && basket.PtcWisePointsEquivalentCurrencyDetails.Any() && basket.PaymentSessions?.First().SelectedPayWithPointsOption != null)
            {
                var milesPlusMoneyDetails = basket.People.GroupBy(x => x.Type).Select((p, i) => new PaymentDetails()
                {
                    FormOfPaymentId = (paymentDetails.Count() + i + 1).ToString("D2"),
                    FormOfPayments = offerResponse.Offers.First().SalePrice.First(sale => sale.PriceComponentType == Price.PriceComponentTypes.BasePriceEnum).Amount?.MonetaryAmount == basket.PaymentSessions?.First().SelectedPayWithPointsOption?.Amount?.MonetaryAmount ? PaymentDetails.FormOfPayment.MoneyPlusMilesFull : PaymentDetails.FormOfPayment.MoneyPlusMilesPartial,
                    Payer = new()
                    {
                        FirstName = loyaltyPayerFirstName,
                        LastName = loyaltyPayerLastName,
                        PayerType = GetPtcType(p.Key),
                        LoyaltyMemberDetails = new()
                        {
                            MemberNumber = basket.CustomerProfile.MembershipDetails.FQTNumber,
                            MemberType = basket.CustomerProfile.MembershipDetails.FQTTierCode switch
                            {
                                "Red" => FQTTier.FF.ToString(),
                                "Silver" => FQTTier.FO.ToString(),
                                "Gold" => FQTTier.GM.ToString(),
                                _ => string.Empty
                            }
                        },
                        LoyaltyMemberTravelling = p.Any(m => m.MembershipDetails?.FQTNumber == basket.CustomerProfile.MembershipDetails.FQTNumber)
                    },
                    MilesPoints = PtcWisePointsEquivalentCurrencyDetail(basket.PtcWisePointsEquivalentCurrencyDetails, basket.PaymentSessions).PtcWisePointsEquivalentCurrency.First(c => c.PassengerTypeCode == GetPtcType(p.Key)).LoyaltyPoints,
                    MilesEquivalentPrice = new()
                    {
                        MonetaryAmount = PtcWisePointsEquivalentCurrencyDetail(basket.PtcWisePointsEquivalentCurrencyDetails, basket.PaymentSessions).PtcWisePointsEquivalentCurrency.First(c => c.PassengerTypeCode == GetPtcType(p.Key)).Amount.MonetaryAmount,
                        BaseCurrency = basket.PaymentSessions.First().SelectedPayWithPointsOption.Amount.BaseCurrency
                    }
                });

                paymentDetails = paymentDetails.Concat(milesPlusMoneyDetails).ToList();
            }

            return paymentDetails;
        }

        private static BasketPerson GetLoyaltyPayer(Basket basket)
        {
            var fqtNumber = basket?.CustomerProfile?.MembershipDetails?.FQTNumber;

            return basket?.People?.FirstOrDefault(person =>
                       !fqtNumber.IsNullOrWhiteSpace() &&
                       (person.MembershipDetails?.FQTNumber).Is(fqtNumber))
                   ?? basket?.People?.FirstOrDefault(person => person.ProfilePassenger == true);
        }

        private static string GetAwardPointsToPay(OfferResponse offerResponse, Basket basket)
        {
            var defaultPoints = offerResponse.Offers.First().SalePrice.First(x => x.PriceComponentType == PriceComponentTypes.PointsEnum).PointsAmount.PointsCount.ToString();
            if (!ConfigReader.Try<bool>(Constants.Application.VoucherRedemptionFeatureFlag, false))
                return defaultPoints;

            var selection = GetVoucherRedemptionSelection(basket);
            var selectedVoucherIds = selection?.SelectedVoucherIds?
                .Where(voucherId => !string.IsNullOrWhiteSpace(voucherId))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray();
            var selectedOption = basket.VoucherDetails?.RedemptionOptions?.FirstOrDefault(option =>
                option.OptionId == selection?.SelectedOptionId &&
                (option.AppliedVouchers ?? Enumerable.Empty<Models.FlightSummary.AppliedVoucher>())
                    .Select(voucher => voucher.VoucherId)
                    .Where(voucherId => !string.IsNullOrWhiteSpace(voucherId))
                    .ToHashSet(StringComparer.OrdinalIgnoreCase)
                    .SetEquals(selectedVoucherIds ?? Array.Empty<string>()));

            return basket.VoucherApplied &&
                   selectedVoucherIds?.Length > 0 &&
                   selectedOption is not null &&
                   long.TryParse(basket.PointsToPay, out var pointsToPay) &&
                   pointsToPay == selectedOption.PointsToPay &&
                   pointsToPay >= 0
                ? pointsToPay.ToString()
                : defaultPoints;
        }

        private static Models.FlightSummary.VoucherRedemptionSelection GetVoucherRedemptionSelection(Basket basket)
        {
            if (basket?.VoucherRedemptionSelection is not null)
                return basket.VoucherRedemptionSelection;

            if (string.IsNullOrWhiteSpace(basket?.FlightSummary))
                return null;

            return Json.Deserialize<Models.FlightSummary.Summary>(basket.FlightSummary)?.VoucherRedemptionSelection;
        }

        public static CurrencyAmount GetAmount(Basket basket, PaymentDetails paymentDetails, OfferResponse offerResponse)
        {
            CurrencyAmount currencyAmount = new();

            if (paymentDetails.FormOfPayments == PaymentDetails.FormOfPayment.CardEnum)
            {
                var insurancePrice = !(basket.IsInsuranceAdded ?? false) ? 0 : basket.InsuranceQuotes.First().Products.First().TotalPrice;

                if (basket.PtcWisePointsEquivalentCurrencyDetails != null && basket.PtcWisePointsEquivalentCurrencyDetails.Any() && basket.PaymentSessions?.First().SelectedPayWithPointsOption != null)
                {
                    var selectedPointsCurrency = basket.PaymentSessions?.First().SelectedPayWithPointsOption?.Amount?.MonetaryAmount;

                    if (paymentDetails.Amount.MonetaryAmount != selectedPointsCurrency)
                    {
                        if (offerResponse.Offers.First().SalePrice.First(sale => sale.PriceComponentType == Price.PriceComponentTypes.BasePriceEnum).Amount?.MonetaryAmount == selectedPointsCurrency)
                        {
                            //full pwp
                            currencyAmount.MonetaryAmount = (Convert.ToDouble(paymentDetails.Amount?.MonetaryAmount) - Convert.ToDouble(selectedPointsCurrency) - insurancePrice).ToString("0.00");
                        }
                        else
                        {
                            //partial pwp
                            currencyAmount.MonetaryAmount = (Convert.ToDouble(paymentDetails.Amount?.MonetaryAmount) - insurancePrice).ToString("0.00");
                        }
                    }
                }
                else
                {
                    currencyAmount.MonetaryAmount = (Convert.ToDouble(paymentDetails.Amount?.MonetaryAmount) - insurancePrice).ToString("0.00");
                }

                currencyAmount.BaseCurrency = paymentDetails.Amount?.BaseCurrency;
                currencyAmount.DecimalPrecisionCount = paymentDetails.Amount.DecimalPrecisionCount;
            }
            else
            {
                currencyAmount.MonetaryAmount = basket.PtcWisePointsEquivalentCurrencyDetails != null && basket.PtcWisePointsEquivalentCurrencyDetails.Any() && basket.PaymentSessions?.First().SelectedPayWithPointsOption != null ? (Convert.ToDouble(offerResponse.Offers.First().SalePrice.FirstOrDefault(sale => sale.PriceComponentType == Price.PriceComponentTypes.BasePriceEnum)?.Amount?.MonetaryAmount) - Convert.ToDouble(basket.PaymentSessions.First().SelectedPayWithPointsOption.Amount.MonetaryAmount)).ToString() : offerResponse.Offers.First().SalePrice.FirstOrDefault(sale => sale.PriceComponentType == Price.PriceComponentTypes.BasePriceEnum)?.Amount?.MonetaryAmount;
                currencyAmount.BaseCurrency = offerResponse.Offers.First().SalePrice.First().Amount.BaseCurrency;
                currencyAmount.DecimalPrecisionCount = offerResponse.Offers.First().SalePrice.First().Amount.DecimalPrecisionCount;
            }

            return currencyAmount;
        }

        public static string GetPtcType(string type)
        {
            return type switch
            {
                "ADULT" => "ADT",
                "CHILD" => "CNN",
                "TEEN" => "GBE",
                "INFANT" => "INF",
                _ => string.Empty
            };
        }

        public static PtcWisePointsEquivalentCurrencyDetails PtcWisePointsEquivalentCurrencyDetail(IEnumerable<PtcWisePointsEquivalentCurrencyDetails> ptcWisePointsEquivalentCurrencyDetails, IEnumerable<PaymentSession> paymentSessions)
        {
            return ptcWisePointsEquivalentCurrencyDetails.First(x => x.TotalPointsEquivalentCurrency.LoyaltyPoints.TrimStart('0') == paymentSessions.First().SelectedPayWithPointsOption.LoyaltyPoints.TrimStart('0'));
        }

        public static InsuranceDetails MapInsuranceDetails(OfferResponse offerResponse, Basket basket, IEnumerable<AirportRefData> airportRefData, string paymentReferenceId)
        {
            var sortedSegments = offerResponse.DataList.FlightSegments.OrderBy(x => x.ScheduledDepartureTime.LocalDateTime);
            var outboundOdRefId = sortedSegments.First().OdRefId;
            var basePrice = offerResponse.Offers.First().SalePrice.First(x => x.PriceComponentType == PriceComponentTypes.BasePriceEnum);

            return new InsuranceDetails()
            {
                TransactionID = AsyncContext.Current.CorrelationId.ToString(),
                BusinessUnit = offerResponse.Pos.CountryCode,
                Purchase = new Purchase()
                {
                    InsuranceBookingDetails = new InsuranceBookingDetails()
                    {
                        PurchaseDateTime = DateTime.UtcNow,
                        Method = "ONLINE",
                        Status = "NEW",
                        Type = "ADD",
                        Currency = basePrice.Amount.BaseCurrency,
                        TotalCoverageAmount = Convert.ToDouble(basePrice.Amount.MonetaryAmount),
                        TotalPrice = (double)((basket.InsuranceQuotes.FirstOrDefault()?.Products.FirstOrDefault()?.TotalPrice) ?? 0),
                        TotalPremiumPrice = (double)((basket.InsuranceQuotes.FirstOrDefault()?.Products.FirstOrDefault()?.TotalPrice) ?? 0),
                        Purchaser = new Purchaser()
                        {
                            FirstName = basket.People.FirstOrDefault(x => x.Id == "0").FirstName,
                            LastName = basket.People.FirstOrDefault(x => x.Id == "0").LastName,
                            EmailAddress = basket.ContactDetails.EmailAddress,
                            PhoneNumber = basket.ContactDetails.PhoneNumber
                        },
                        FulfillmentMethod = "EMAIL",
                        PaymentMethod = "CC"
                    },
                    TravelBookingDetails = new TravelBookingDetails()
                    {
                        Currency = basePrice.Amount.BaseCurrency,
                        TotalTripCost = Convert.ToDouble(basePrice.Amount.MonetaryAmount),
                        Method = "ONLINE",
                        Path = "Domestic"
                    },
                    TravelDetails = new TravelDetails()
                    {
                        DepartureDate = Convert.ToDateTime(sortedSegments.First(s => s.OdRefId == outboundOdRefId).ScheduledDepartureTime.LocalDateTime),
                        ReturnDate = Convert.ToDateTime(sortedSegments.FirstOrDefault(s => s.OdRefId != outboundOdRefId)?.ScheduledDepartureTime.LocalDateTime),
                        Origin = new()
                        {
                            Code = sortedSegments.First(s => s.OdRefId == outboundOdRefId).OriginAirportCode,
                            Country = airportRefData.First(a => a.AirportCode == sortedSegments.First(s => s.OdRefId == outboundOdRefId).OriginAirportCode).CountryCode
                        },
                        Destination = new()
                        {
                            Code = sortedSegments.Last(s => s.OdRefId == outboundOdRefId).DestinationAirportCode,
                            Country = airportRefData.First(a => a.AirportCode == sortedSegments.Last(s => s.OdRefId == outboundOdRefId).DestinationAirportCode).CountryCode
                        }
                    },
                    Providers = new Providers()
                    {
                        Provider =
                        [
                            new Provider()
                            {
                                Name = "Virgin Atlantic"
                            }
                        ]
                    },
                    ProductDetails = new ProductDetails()
                    {
                        Product = basket.People.Select((x, i) => new Models.Request.Order.Product()
                        {
                            Sequence = i,
                            QuoteID = basket.InsuranceQuotes.FirstOrDefault()?.Products.FirstOrDefault()?.QuoteId,
                            ProductID = basket.InsuranceQuotes.FirstOrDefault()?.Products.FirstOrDefault()?.ProductCode,
                            InsuredDetails = new()
                            {
                                Insured = [new Insured()
                                        {
                                           InsuredSequence = i,
                                           InsuredType = InsuranceQuoteMapper.GetInsuredType(x),
                                           FirstName = x.FirstName,
                                           LastName = x.LastName,
                                           Birthdate = (DateTime)x.DateOfBirth,
                                           PhoneNumber = basket.ContactDetails.PhoneNumber,
                                           TripCost = Convert.ToDouble(basePrice.Amount.MonetaryAmount)
                                         }]
                            },
                            CoverageAmount = Convert.ToDouble(basePrice.Amount.MonetaryAmount),
                            Price = (double)((basket.InsuranceQuotes.FirstOrDefault()?.Products.FirstOrDefault()?.TotalPrice) ?? 0),
                            PremiumPrice = (double)((basket.InsuranceQuotes.FirstOrDefault()?.Products.FirstOrDefault()?.TotalPrice) ?? 0),
                            StartDate = Convert.ToDateTime(sortedSegments.First(s => s.OdRefId == outboundOdRefId).ScheduledDepartureTime.LocalDateTime),
                            EndDate = sortedSegments.FirstOrDefault(s => s.OdRefId != outboundOdRefId)?.ScheduledDepartureTime.LocalDateTime == null ? Convert.ToDateTime(sortedSegments.First(s => s.OdRefId == outboundOdRefId).ScheduledDepartureTime.LocalDateTime) : Convert.ToDateTime(sortedSegments.FirstOrDefault(s => s.OdRefId != outboundOdRefId)?.ScheduledDepartureTime.LocalDateTime),
                        })
                    },
                    PaymentDetails = new() { PaymentReferenceId = paymentReferenceId },
                    CxId = basket.InsuranceQuotes.FirstOrDefault()?.TransactionDetails?.Cxid,
                    ResponseId = basket.InsuranceQuotes.FirstOrDefault()?.TransactionDetails?.ResponseId,
                    SupplierCode = basket.InsuranceQuotes.FirstOrDefault()?.Products.FirstOrDefault()?.SupplierCode
                }
            };
        }

        public static IEnumerable<Offers> GetSelectedFlightPlusSeatOffers(OfferResponse offerResponse, Basket basket, IEnumerable<SeatMSResponse> seatMSResponse)
        {
            bool disableSeatIntegrationFlag = ConfigReader.Try<bool>(Constants.Environment.SeatIntegrationFlag, false);

            List<Offers> offers = new();

            //flight offers
            offers.AddRange(offerResponse.Offers.Select(offer => new Offers()
            {
                OfferId = offer.OfferId.Split("|").Last(),
                ProductType = "FLIGHT",
                SalePrice = offer.SalePrice,
                OfferItems = offer.OfferItems.Select(offerItem =>
                {
                    offerItem.OfferItemId = offerItem.OfferItemId.Split("|").Last();
                    return offerItem;
                })
            }));

            if (disableSeatIntegrationFlag)
            {
                return offers;
            }

            foreach (var seatMap in seatMSResponse)
            {
                var selectedSeatOfferIds = basket.FlightPassengerSeatMap.Where(flightPassengerSeatMap => !string.IsNullOrEmpty(flightPassengerSeatMap?.SeatNumber) && flightPassengerSeatMap.SeatOfferId == seatMap.SeatOfferId).Select(selSeat => selSeat.SelectedSeatOfferId.Split('|').First()).Distinct();

                foreach (var selectedSeatOfferId in selectedSeatOfferIds)
                {
                    var passengerSeatMap = basket.FlightPassengerSeatMap.Where(flightPassengerSeatMap => !string.IsNullOrEmpty(flightPassengerSeatMap?.SeatNumber) && flightPassengerSeatMap.SeatOfferId == seatMap.SeatOfferId && flightPassengerSeatMap.SelectedSeatOfferId.StartsWith(string.Concat(selectedSeatOfferId, "|")));
                    var cataloguePriceList = seatMap.FlightSegmentSeatMap.First().Catalogue.Where(cat => passengerSeatMap.Select(selSeatOfferId => selSeatOfferId.SelectedSeatOfferId).Contains(cat.CatalogueId));

                    offers.Add(new Offers()
                    {
                        OfferId = selectedSeatOfferId,
                        ProductType = "SEAT",
                        SalePrice = new List<Price>()
                                {
                                   new Price()
                                   {
                                      Amount = GetTotalSeatAmount(cataloguePriceList, passengerSeatMap)
                                   }
                                },
                        OfferItems = GetOfferItems(passengerSeatMap, seatMap)
                    });

                }
            }

            return offers;
        }

        private static CurrencyAmount GetTotalSeatAmount(IEnumerable<FlightSegmentSeatMapCatalogue> seatMapCatalogue, IEnumerable<Models.Request.Seat.FlightPassengerSeatMap> passengerSeats)
        {
            var priceList = passengerSeats.Select(seat => seatMapCatalogue.First(cat => cat.CatalogueId == seat.SelectedSeatOfferId).SeatPrice);
            return new CurrencyAmount()
            {
                MonetaryAmount = priceList.Sum(catalogueInfo => Convert.ToDouble(catalogueInfo.MonetaryAmount)).ToString(),
                BaseCurrency = priceList.First().BaseCurrency,
                DecimalPrecisionCount = priceList.First().DecimalPrecisionCount
            };
        }

        private static IEnumerable<OfferItem> GetOfferItems(IEnumerable<Models.Request.Seat.FlightPassengerSeatMap> selectedPassengerSeats, SeatMSResponse seatMSResponse)
        {
            List<OfferItem> offerItems = new();

            foreach (var flightPassengerSeatMap in selectedPassengerSeats)
            {
                var seat = GetSeatColumn(seatMSResponse, flightPassengerSeatMap.SeatNumber);
                if (!offerItems.Any(x => x.Services.Any(s => s.ServiceBeneficiaries.Contains(flightPassengerSeatMap.PassengerId))))
                {
                    offerItems.Add(new OfferItem()
                    {
                        OfferItemId = GetSeatOfferItemId(seat, flightPassengerSeatMap),
                        OfferitemPricing = new List<OfferitemPricing>()
                        {
                            new OfferitemPricing()
                            {
                                 SalePrice = new List<Price>()
                                {
                                   new Price()
                                   {
                                      Amount = seatMSResponse.FlightSegmentSeatMap.First().Catalogue.First(cat => cat.CatalogueId == flightPassengerSeatMap.SelectedSeatOfferId).SeatPrice
                                   }
                                }
                            }
                        },
                        Services = new List<OfferService>()
                        {
                           new OfferService()
                           {
                              ServiceBeneficiaries = new string[] { flightPassengerSeatMap.PassengerId },
                              SeatInformation = new()
                              {
                                 FlightLegId = flightPassengerSeatMap.SectorId,
                                 EconomyComfort = seat.EconomyComfort,
                                 ExitOverride = seat.Exit,
                                 ExitRow = seat.Exit,
                                 SeatColumn = seat.SeatIdentifer.Last().ToString(),
                                 SeatProductCode = seatMSResponse.FlightSegmentSeatMap.First().Catalogue.First(cat => cat.CatalogueId == flightPassengerSeatMap.SelectedSeatOfferId).ProductId, //?????????????????? query coming null
                                 SeatRowNumber = Convert.ToInt32(new String(seat.SeatIdentifer.TakeWhile(Char.IsDigit).ToArray())),
                              }
                           }
                        }
                    });
                }
            }

            return offerItems;
        }

        private static SeatColumn GetSeatColumn(SeatMSResponse seatMSResponse, string seatNumber)
        {
            var seatMap = seatMSResponse.FlightSegmentSeatMap.First().Seatmap.Where(seatmap => seatmap.SeatRows.Any(row => !row.RowNumber.IsNullOrEmpty() && row.SeatColumns.Any(col => col.SeatIdentifer == seatNumber))).First();
            var seatRow = seatMap.SeatRows.First(row => row.SeatColumns.Any(seat => seat.SeatIdentifer == seatNumber));

            return seatRow.SeatColumns.First(seat => seat.SeatIdentifer == seatNumber);
        }

        private static string GetSeatOfferItemId(SeatColumn seatColumn, Models.Request.Seat.FlightPassengerSeatMap passengerSeatMap)
        {
            var offerItemIdFromSeat = seatColumn.Eligibility.FirstOrDefault(e => e.CatalogueRefId == passengerSeatMap.SelectedSeatOfferId && e.CustomerId.Contains(passengerSeatMap.PassengerId))?.OfferItemId;
            var offerItemIds = passengerSeatMap.SelectedSeatOfferId.Split('|').Last().Split('_');
            string offerItemId = string.Empty;
            foreach (var offerItem in offerItemIdFromSeat)
            {
                offerItemId = offerItemIds.First(o => o.Equals(offerItem.OfferItemId, StringComparison.OrdinalIgnoreCase));
            }
            return offerItemId;
        }

        private static string GetSeatPosition(IEnumerable<SeatMSResponse> seatMapResponses, string seatOfferId, string seatNumber)
        {
            var seatMap = seatMapResponses.First(msSeatMap => msSeatMap.SeatOfferId == seatOfferId).FlightSegmentSeatMap.First().Seatmap.First(seatmap => seatmap.SeatRows.Any(row => !row.RowNumber.IsNullOrEmpty() && row.SeatColumns.Any(col => col.SeatIdentifer == seatNumber)));
            var seatRow = seatMap.SeatRows.First(row => row.SeatColumns.Any(seat => seat.SeatIdentifer == seatNumber));
            var seatPosition = seatRow.SeatColumns.First(seat => seat.SeatIdentifer == seatNumber).Pos;
            return seatPosition;
        }

        public static OrderCreateGraphQLResponse GetOrderCreateResponse(OrderResponseMS orderResponse, OfferResponse offerResponse, Basket basket,
            IEnumerable<AirportRefData> airportData, IEnumerable<AirlineRefData> airlineData,
            IEnumerable<CabinRefData> cabinData, IEnumerable<SeatMSResponse> seatMSResponse = null, IEnumerable<AircraftRefData> airCraftData = null)
        {
            var basketSummary = BasketSummaryMapper.MapFlightSummary(offerResponse, basket, airportData, airlineData, orderResponse, cabinData, airCraftData);
            IEnumerable<BasketFlightPassengerSeatMap> basketPassengerSeatMap = null;

            if (seatMSResponse is not null && seatMSResponse.Any())
            {
                basketPassengerSeatMap = basket.FlightPassengerSeatMap?.Where(basketSeat => !string.IsNullOrEmpty(basketSeat?.SeatNumber)).Select(basketSeat => new BasketFlightPassengerSeatMap()
                {
                    SectorId = basketSeat.SectorId,
                    SeatOfferId = basketSeat.SeatOfferId,
                    PassengerId = basketSeat.PassengerId,
                    SeatNumber = basketSeat.SeatNumber,
                    SelectedSeatOfferId = basketSeat.SelectedSeatOfferId,
                    LinkedInfantId = basketSeat.LinkedInfantId,
                    Price = basketSeat.Price,
                    Pos = GetSeatPosition(seatMSResponse, basketSeat.SeatOfferId, basketSeat.SeatNumber)
                });

                basket.FlightPassengerSeatMap = basketPassengerSeatMap;
            }

            return new OrderCreateGraphQLResponse
            {
                PointOfSale = new() { CountryCode = offerResponse.Pos?.CountryCode, CityCode = offerResponse.Pos?.CityCode },
                Infants = orderResponse.DataList?.CustomerDetails?.Where(custom => custom.Ptc == Constants.Application.InfantPtc)?.Select(custom => custom.CustId),
                Children = orderResponse.DataList?.CustomerDetails?.Where(custom => custom.Ptc == Constants.Application.ChildPtc)?.Select(custom => custom.CustId),
                Adults = orderResponse.DataList?.CustomerDetails?.Where(custom => custom.Ptc == Constants.Application.AdultPtc)?.Select(custom => custom.CustId),
                People = basket?.People,
                LeadPassenger = GetLeadPassenger(orderResponse, basket),
                Summary = GetConfirmationSummary(basketSummary, orderResponse, basket),
                PaymentInformation = new List<PaymentInformation>
                {
                    new()
                    {
                        CardType = orderResponse.PaymentDetails?.FirstOrDefault(x => x?.FormOfPayments == FormOfPayment.CardEnum)?.CardMethod?.CardType?.ToDisplayName(),
                        PaymentAmount= Convert.ToDouble(orderResponse.PaymentDetails?.FirstOrDefault(x => x?.FormOfPayments == FormOfPayment.CardEnum)?.Amount?.MonetaryAmount)
                    }
                },
                ResourceType = Constants.Application.ResourceType,
                Configuration = new Configuration(),
                Warnings = orderResponse.Warnings,
                CustomerProfile = basket.CustomerProfile != null ? new()
                {
                    FirstName = basket.CustomerProfile.FirstName,
                    LastName = basket.CustomerProfile.LastName,
                    MembershipDetails = new()
                    {
                        FQTProgramme = basket.CustomerProfile.MembershipDetails.FQTProgramme,
                        FQTNumber = basket.CustomerProfile.MembershipDetails.FQTNumber,
                        FQTTierCode = basket.CustomerProfile.MembershipDetails.FQTTierCode,
                        FQTPoints = basketSummary.MembershipPoints != 0 ? Convert.ToString(Convert.ToInt32(basket.CustomerProfile.MembershipDetails?.FQTPoints) - basketSummary.MembershipPoints) : Convert.ToString(Convert.ToInt32(basket.CustomerProfile.MembershipDetails?.FQTPoints) - Convert.ToInt32(string.IsNullOrEmpty(basket.PaymentSessions?.FirstOrDefault()?.SelectedPayWithPointsOption?.LoyaltyPoints.TrimStart('0')) ? "0" : basket.PaymentSessions?.FirstOrDefault()?.SelectedPayWithPointsOption?.LoyaltyPoints.TrimStart('0')))
                    }
                } : new(),
                EarnPoints = basket.EarnPoints
            };
        }

        public static Summary GetConfirmationSummary(BasketSummary basketSummary, OrderResponseMS orderResponse, Basket basket)
        {
            Summary summary = (Summary)basketSummary;

            var orderItemSeats = orderResponse.OrderItems.Where(oi => oi.Services.Any(services => services.Product.ProductType.Equals(Product.ProductTypes.SeatEnum)));

            if (orderItemSeats is not null && orderItemSeats.Any())
            {
                var orderItemSeatsBooked = orderItemSeats.Where(oi => oi.Services.Any(services => services.SeatInformation.SeatStatus != "FAILED" && services.SeatInformation.SeatStatus is not null));
                summary.FlightPassengerSeatMap = !orderItemSeatsBooked.IsNullOrEmpty() ? MapFlightPassengerSeatMapSummary(orderItemSeatsBooked?.Where(oi => oi.Services.Any(services => services.SeatInformation.SeatStatus != "FAILED" && services.SeatInformation.SeatStatus is not null)), orderResponse.DataList.CustomerDetails, basket) : null;
                summary.TotalSeatPrice = !orderItemSeatsBooked.IsNullOrEmpty() ? orderItemSeatsBooked.Select(seat => seat.PriceDetails.First()).Sum(price => Convert.ToDouble(price.Amount.MonetaryAmount)) : 0;
                var orderItemsSeatsFailed = orderItemSeats.Where(oi => oi.Services.Any(services => services.SeatInformation.SeatStatus == "FAILED" || services.SeatInformation.SeatStatus is null));
                var sumFailedSeats = orderItemsSeatsFailed.Select(seat => seat.PriceDetails.First()).Sum(price => Convert.ToDouble(price.Amount.MonetaryAmount));
                summary.TotalPrice = summary.TotalPrice - sumFailedSeats;
            }

            var flightSummary = Json.Deserialize<VAA.EAPI.GraphQL.Domain.Book.Models.FlightSummary.Summary>(basket.FlightSummary);
            VoucherRedemptionFeatureHelper.HydrateBasketFromPersistedSummary(basket, flightSummary);
            summary.FlightPassengerBaggage = flightSummary.FlightPassengerBaggage;
            // Keep the order response aligned with the persisted summary when the
            // Basket root flag has not yet been propagated by BasketMS.
            summary.VoucherApplied = basket.VoucherApplied || flightSummary.VoucherApplied;
            summary.HasSufficientPoints = basket.HasSufficientPoints;
            summary.VoucherPoints = basket.VoucherPoints;
            summary.VoucherAmount = basket.VoucherPoints;
            summary.PointsToPay = basket.PointsToPay;
            summary.VoucherDetails = basket.VoucherDetails;
            summary.VoucherRedemptionSelection = basket.VoucherRedemptionSelection;

            return summary;
        }

        public static IEnumerable<SummaryFlightPassengerSeatMap> MapFlightPassengerSeatMapSummary(IEnumerable<Models.Order.OrderItems> orderItems, IEnumerable<CustomerInfo> orderCustomerDataList, Basket basket)
        {
            return orderCustomerDataList.Where(customer => customer.Ptc != Constants.Application.DeltaPtcInfant).SelectMany(customer =>
            {
                var matchingOrderItems = orderItems.Where(o => o.Services.Any(s => s.CustomerRefIds.Contains(customer.CustId)));
                var basketPassengerSeatDetails = basket.FlightPassengerSeatMap.Where(f => f.PassengerId == customer.CustId);

                if (matchingOrderItems != null && matchingOrderItems.Any())
                {
                    return new[]
                    {
                        new SummaryFlightPassengerSeatMap()
                        {
                          PassengerId = customer.CustId,
                          PassengerFirstName = customer.FirstName,
                          PassengerLastName = customer.LastName,
                          SeatDetails = GetSeatDetails(matchingOrderItems, basketPassengerSeatDetails),
                          LinkedInfant = basketPassengerSeatDetails.Any(b => !b.LinkedInfantId.IsNullOrEmpty()) ? new()
                          {
                             InfantPassengerId = basketPassengerSeatDetails.First().LinkedInfantId,
                             InfantFirstName = basket.People.First(x => x.Id == basketPassengerSeatDetails.First().LinkedInfantId).FirstName,
                             InfantLastName = basket.People.First(x => x.Id == basketPassengerSeatDetails.First().LinkedInfantId).LastName
                          } : null
                       }
                    };
                }
                else
                    return Array.Empty<SummaryFlightPassengerSeatMap>();
            });
        }

        public static IEnumerable<SeatDetails> GetSeatDetails(IEnumerable<Models.Order.OrderItems> orderItems, IEnumerable<Models.Request.Seat.FlightPassengerSeatMap> flightPassengerSeatMaps)
        {
            return flightPassengerSeatMaps.Select(passengerSeat =>
            {
                var orderItem = orderItems.FirstOrDefault(o => o.Services.Any(s => s.SeatInformation.FlightLegId == passengerSeat.SectorId));

                return orderItem != null ? new SeatDetails
                {
                    SectorId = passengerSeat.SectorId,
                    SeatNumber = $"{orderItem.Services.Select(s => s.SeatInformation.SeatRowNum).FirstOrDefault()}{orderItem.Services.Select(s => s.SeatInformation.SeatColumnName).FirstOrDefault()}",
                    Price = passengerSeat.Price,
                    Pos = passengerSeat.Pos
                } : null;
            });
        }

        private static IEnumerable<CustomerInfo> GetCustomerInfo(Basket basket)
        {
            string preferenceValue = basket.ContactDetails.AcceptEmail ? "Y" : "N";
            int infantCount = 0;
            var infantPassengers = basket.People.Where(customer => customer.Type == Constants.Application.InfantPtc).ToList();

            return basket.People.Select(people =>
            {
                var customerInfo = new CustomerInfo()
                {
                    CustId = people.Id,
                    FirstName = people.FirstName,
                    LastName = people.LastName,
                    DateOfBirth = people.DateOfBirth?.ToString("yyyy-MM-dd"),
                    Ptc = people.Type switch
                    {
                        "ADULT" => Constants.Application.DeltaPtcAdult,
                        "CHILD" => Constants.Application.DeltaPtcChild,
                        "TEEN" => Constants.Application.DeltaPtcTeen,
                        "INFANT" => Constants.Application.DeltaPtcInfant,
                        _ => throw new ArgumentOutOfRangeException(nameof(people.Type)),
                    },
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
                        Phone = basket.ContactDetails.PhoneNumber.Substring(3),
                        PhoneAreaNumber = basket.ContactDetails.PhoneNumber.Substring(0, 3),
                        Countrycode = basket.ContactDetails.CountryCode
                    },
                    LoyaltyMemberDetails = new LoyaltyMemberDetails() { MemberNumber = people.MembershipDetails?.FQTNumber, ProgramCode = people.MembershipDetails?.FQTProgramme },
                    LinkedInfantId = (people.Type == Constants.Application.AdultPtc || people.Type == Constants.Application.TeenPtc) && infantPassengers.Count > infantCount ? infantPassengers[infantCount].Id : null,
                };
                customerInfo.Preferences = new Preferences()
                {
                    PreferenceType = PreferenceTypes.VAAMarketingPermissions,
                    PreferenceValue = preferenceValue,
                };

                if (people.Type == Constants.Application.AdultPtc || people.Type == Constants.Application.TeenPtc)
                    infantCount++;

                return customerInfo;
            }).ToArray();
        }

        private static LeadPassenger GetLeadPassenger(OrderResponseMS orderResponse, Basket basket)
        {
            var leadPassenger = orderResponse.DataList.CustomerDetails.FirstOrDefault(customer => customer?.Ptc == Constants.Application.DeltaPtcAdult);
            var leadPassengerBasket = basket.People.FirstOrDefault(customer => customer.Type == Constants.Application.AdultPtc && customer.FirstName.Equals(leadPassenger?.FirstName, StringComparison.OrdinalIgnoreCase) && customer.LastName.Equals(leadPassenger?.LastName, StringComparison.OrdinalIgnoreCase));

            return new LeadPassenger()
            {
                Address = new GQLAddress
                {
                    LineOne = leadPassenger?.Addresses?.AddressLine1,
                    LineTwo = leadPassenger?.Addresses?.AddressLine2,
                    PostCode = leadPassenger?.Addresses?.PostalCode,
                    TownCity = leadPassenger?.Addresses?.City
                },
                Contact = new Models.BookGraphQLModels.ContactDetails
                {
                    PhoneNumber = leadPassenger?.Contacts?.Phone,
                    EmailAddress = !string.IsNullOrEmpty(leadPassenger?.Contacts?.Email) ? leadPassenger?.Contacts?.Email : basket.ContactDetails.EmailAddress,
                    AlternativePhoneNumber = basket.ContactDetails.AlternativePhoneNumber
                },
                FirstName = leadPassenger?.FirstName,
                LastName = leadPassenger?.LastName,
                DateOfBirth = !string.IsNullOrEmpty(leadPassenger?.DateOfBirth) ? leadPassenger?.DateOfBirth : leadPassengerBasket?.DateOfBirth?.ToString("yyyy/MM/dd"),
                Gender = !string.IsNullOrEmpty(leadPassenger?.Gender) ? leadPassenger?.Gender : leadPassengerBasket?.Gender,
                Name = leadPassenger?.FirstName + Constants.Application.StringSpace + leadPassenger?.LastName,
                Type = leadPassengerBasket?.Type
            };
        }

        public static OrderCreateGraphQLResponse OrderCreateErrorMapper(string code, string message)
        {
            string json = ConfigReader.ReadAllText(Common.Constants.Application.FunctionAppDirPath, Constants.Application.ErrorFilePath);

            List<ErrorDetails> errorConfigObject = Json.Deserialize<List<ErrorDetails>>(json);
            ErrorDetails errors = errorConfigObject.FirstOrDefault(e => e.Code == code) != null ? errorConfigObject.FirstOrDefault(e => e.Code == code) : errorConfigObject.FirstOrDefault(e => e.Code == Constants.Application.DeltaGenericErrorCode);

            if (errors != null)
                return new OrderCreateGraphQLResponse()
                {
                    Errors = new ErrorDetails[]
                    {
                        new()
                        {
                            Code = errors?.Code,
                            DeveloperMessage = $"{message} | ErrorID: {AsyncContext.Current.CorrelationId} | TimeStamp: {DateTime.UtcNow}",
                            ErrorSource = errors?.ErrorSource,
                            ErrorTag = errors?.ErrorTag,
                            Message = errors?.Message
                        }
                    }
                };
            else
                return new OrderCreateGraphQLResponse()
                {
                    Errors = new ErrorDetails[]
                    {
                        new()
                        {
                            Code = code,
                            Message =  message
                        }
                    }
                };
        }
    }
}
