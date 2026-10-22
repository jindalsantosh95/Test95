using VAA.EAPI.GraphQL.Common.Extensions;
using System.Globalization;
using VAA.EAPI.GraphQL.Common.Helper;
using VAA.EAPI.GraphQL.Domain.Search.Infrastructure.Mapper;
using VAA.EAPI.GraphQL.Domain.Search.Infrastructure.Models.ReferenceMS;
using VAA.EAPI.GraphQL.Domain.Search.Models.BFFSummary;
using VAA.EAPI.GraphQL.Domain.Search.Models.Brand;
using VAA.EAPI.GraphQL.Domain.Search.Models.FlightOffer;
using VAA.EAPI.GraphQL.Domain.Search.Models.FlightOffer.SummaryDeepLink;
using VAA.EAPI.GraphQL.Domain.Search.Models.Reference;
using VAA.EAPI.GraphQL.Infrastructure.Entities.VRedMS;
using Content = VAA.EAPI.GraphQL.Domain.Search.Models.BFFSummary.Content;
using Price = VAA.EAPI.GraphQL.Domain.Search.Models.BFFSummary.Price;
using Segment = VAA.EAPI.GraphQL.Domain.Search.Models.BFFSummary.Segment;

namespace VAA.EAPI.GraphQL.Domain.Search.Mapper
{
    public class OfferMStoBFFSummaryMapper
    {
        private const string InfantPassengerType = "INFANT";
        private const string InfantPtcCode = "INF";

        public static Summary Map(FlightOfferResponse response, int totalOnDs, IEnumerable<AirportInfo> airportInfo, IEnumerable<AircraftInfo> aircraftInfo, IEnumerable<AirlineInfo> airlineInfo, Guid basketId, IEnumerable<Models.Basket.BookingJourneyOfferDetails>? bookingJourneyOfferDetails, Brand? brandRs, string passengerCounts, IEnumerable<CabinInfo> cabinInfo, IEnumerable<UpgradeInfo> upgradeInfo, IEnumerable<TaxCodeInfo> taxCodeInfo, IEnumerable<BaggageDetails> baggageDetails, VRedResponse vRed, Amount deeplinkInitialAmount, EarnPoints earnPoints)
        {
            var selectedOffer = response.Offers.First(offer => offer.Selected == true);

            Price price = (Price)selectedOffer.SalePrice!.ToList();

            var fareBasisCode = selectedOffer.OfferItems.First().Services.First().FareDetails.FareBasisCode;
            var fareCalculations = GetFareCalculations(selectedOffer, response.DataList, taxCodeInfo);

            var summary = new Summary()
            {
                Price = price,
                Flights = GetSummaryFlights(response, selectedOffer, basketId, bookingJourneyOfferDetails, totalOnDs, brandRs, cabinInfo, airportInfo, aircraftInfo, airlineInfo),
                Upgrades = response.IsAwardSearchResult ? Enumerable.Empty<Upgrade>() : GetUpgrades(response.Offers.Where(offer => offer.UpsellOffer == true), response.DataList.FlightSegments, basketId, totalOnDs, totalOnDs, price.AmountIncludingTax, cabinInfo, upgradeInfo),
                FareCalculations = fareCalculations,
                FareCalculationSummary = GetFareCalculationSummary(selectedOffer, response.DataList, fareCalculations),
                Links = GetLinks(selectedOffer.OfferId, passengerCounts),
                PassengerCounts = passengerCounts,
                FareId = fareBasisCode,
                Id = selectedOffer.OfferId,
                BasketId = basketId,
                PromoCodeApplied = selectedOffer.PromoCodeApplied,
                IsSaverFare = selectedOffer.IsSaverFare,
                UpgradeFareId = default,
                FlightPassengerBaggage = GetBaggageDetails(response, brandRs),
                EarnPoints = earnPoints
            };

            if (vRed != null && response.IsAwardSearchResult)
            {
                var requiredAwardPointsText = summary.FareCalculationSummary?.TotalAwardPoints ?? price.AwardPoints;
                summary.HasSufficientPoints =
                    TryParsePoints(requiredAwardPointsText, out var requiredAwardPoints) &&
                    TryParsePoints(vRed.Current, out var availableAwardPoints) &&
                    requiredAwardPoints <= availableAwardPoints;
            }
            if (deeplinkInitialAmount != null && (response.Offers.First().PriceChanged || price.AmountIncludingTax != deeplinkInitialAmount.Value))
            {
                summary.PriceChange = price.AmountIncludingTax - deeplinkInitialAmount.Value;
            }

            summary.FlightPassengerBaggage = ReferenceMSMapper.UpdateBaggageDetails(summary, airportInfo, baggageDetails, response);

            return summary;
        }

        internal static IReadOnlyList<Models.BFFSummary.Fare> ExcludeInfantFares(IEnumerable<Models.BFFSummary.Fare> fares)
        {
            if (fares == null) return Array.Empty<Models.BFFSummary.Fare>();

            return fares
                .Where(f =>
                    !string.Equals(f.PassengerType, InfantPassengerType, StringComparison.OrdinalIgnoreCase) &&
                    !string.Equals(f.PassengerType, InfantPtcCode, StringComparison.OrdinalIgnoreCase))
                .ToArray();
        }

        internal static string? CalculateTotalAwardPoints(IEnumerable<Models.BFFSummary.Fare> fares)
        {
            if (fares == null) return null;

            long totalAwardPoints = 0;

            foreach (var fare in fares)
            {
                if (fare == null || fare.Quantity <= 0) continue;

                if (!TryParsePoints(fare.Price?.AwardPoints, out var awardPointsPerPassenger)) continue;
                if (awardPointsPerPassenger < 0) continue;

                // Avoid overflow and silent wrap.
                if (awardPointsPerPassenger != 0 && awardPointsPerPassenger > long.MaxValue / fare.Quantity)
                {
                    return null;
                }

                totalAwardPoints += awardPointsPerPassenger * fare.Quantity;
            }

            return totalAwardPoints.ToString(CultureInfo.InvariantCulture);
        }

        private static bool TryParsePoints(string? pointsText, out long points)
        {
            return long.TryParse(pointsText, NumberStyles.Integer, CultureInfo.InvariantCulture, out points);
        }

        private static IEnumerable<FlightPassengerBaggage>? GetBaggageDetails(FlightOfferResponse response, Models.Brand.Brand? brandRs)
        {
            var flightPassengerBaggage = response.DataList.OriginDestinationAssociations?.Select(ond => GetFlightPassengerBaggage(response, ond.JourneyRefIds.First(), brandRs));

            return UpdateDimensionAllowances(flightPassengerBaggage);
        }

        private static FlightPassengerBaggage GetFlightPassengerBaggage(FlightOfferResponse response, string journeyRefId, Models.Brand.Brand? brandRs)
        {
            var dominantFareSegment = response.Offers.FirstOrDefault(o => o.Selected)?.OfferItems?.FirstOrDefault()?.Services?.FirstOrDefault(s => s.JourneyRefIds.Contains(journeyRefId))?.FareDetails?.FareSegments?.FirstOrDefault(fs => fs.IsDominantLeg);
            var dominantSegmentOfferMS = response.DataList.FlightSegments.First(s => s.SegmentId == dominantFareSegment?.FlightSegmentRefID);
            var dominantSegmentBrandMS = GetSegmentFromBrandRs(int.Parse(dominantSegmentOfferMS.MarketingFlightNumber), brandRs);

            return new FlightPassengerBaggage()
            {
                FlightRefId = journeyRefId,
                CarryOnBag = new()
                {
                    PassengerBaggageAllowance = (PassengerBaggageAllowance)dominantSegmentBrandMS?.CarryOnBaggage?.FirstOrDefault()
                },
                CheckedBag = new()
                {
                    PassengerBaggageAllowance = (PassengerBaggageAllowance)dominantSegmentBrandMS?.CheckedBaggage?.FirstOrDefault()
                },
            };
        }

        /**
         * Returns a SEGMENT object that represents a LEG of the journey. This is because the ATPCO schema
         * calls these segments, even though they would normally be called legs.
         * 
         * For a multi-leg segment, this will return the first leg.
         */
        private static Models.Brand.Segment GetSegmentFromBrandRs(int flightNumber, Brand? brandRs)
        {
            var itinerary = brandRs?.Itineraries?.FirstOrDefault(x => x.Legs?.Any(l => l.Fares?.FirstOrDefault()?.Segments?.Any(s => s.FlightNumber == flightNumber) ?? false) ?? false);
            // Actually the segment, but ATPCO calls it a leg.
            var leg = itinerary?.Legs?.First(l => l.Fares.First().Segments.Any(s => s.FlightNumber == flightNumber));
            // Actualy the leg, but ATPCO calls it a segment.
            return leg?.Fares?.FirstOrDefault().Segments?.FirstOrDefault(bs => bs?.FlightNumber == flightNumber);
        }

        private static Models.Brand.Fare GetFareFromBrandRs(string sectorOrigin, string sectorDestination, Brand? brandRs)
        {
            var itinerary = brandRs?.Itineraries?.FirstOrDefault(x => x.Legs?.Any(l => l.Fares?.FirstOrDefault()?.Segments?.First().Origin == sectorOrigin && l.Fares?.FirstOrDefault()?.Segments.Last().Destination == sectorDestination) ?? false);
            var leg = itinerary?.Legs?.First(l => l.Fares.First().Segments.First().Origin == sectorOrigin && l.Fares.First().Segments.Last().Destination == sectorDestination);
            return leg?.Fares?.FirstOrDefault();
        }

        private static IEnumerable<Link> GetLinks(string selectedOfferId, string passengerCounts)
        {
            var urlPrefix = ConfigReader.Get(Constants.Environment.SearchPostBackUrl);
            return
            [
                new()
                {
                    Href = $"{urlPrefix}/information/{selectedOfferId}",
                    Rel = "information"
                },
                new()
                {
                    Href = $"{urlPrefix}/carbonindex/{selectedOfferId}/{passengerCounts}",
                    Rel = "carbonIndex"
                }
            ];
        }

        private static FareCalculationSummary GetFareCalculationSummary(OfferDetails selectedOffer, DataList dataList, IEnumerable<FareCalculation> farecalculations)
        {
            var taxes = farecalculations.SelectMany(x => x.Taxes);
            var fares = GetFares(selectedOffer.OfferItems, dataList).ToArray();
            var awardFares = ExcludeInfantFares(fares);

            return new FareCalculationSummary
            {
                Fares = fares,
                Taxes = GetTaxSummary(taxes),
                TotalAwardPoints = CalculateTotalAwardPoints(awardFares)
            };
        }

        private static IEnumerable<Taxes> GetTaxSummary(IEnumerable<Taxes> taxBreakup)
        {
            return taxBreakup
                .GroupBy(g => g.Code)
                .Select(x => new Taxes
                {
                    Code = x.Key,
                    Description = x.First().Description,
                    Price = new Price
                    {
                        Amount = x.Sum(s => s.Price.Amount),
                        Currency = x.First().Price.Currency
                    }
                });
        }

        private static IEnumerable<Models.BFFSummary.Fare> GetFares(IEnumerable<OfferItem> offerItems, DataList dataList)
        {
            return offerItems.Select(offerItem => GetOfferItemFare(dataList, offerItem));
        }

        private static Models.BFFSummary.Fare GetOfferItemFare(DataList dataList, OfferItem offerItem)
        {
            return new Models.BFFSummary.Fare
            {
                PassengerType = GetPTC(dataList, offerItem),
                Quantity = offerItem.Services.First().ServiceBeneficiaries.Count(),
                Price = (Price)offerItem.OfferItemPricing.First().SalePrice.ToList()
            };
        }

        private static string GetPTC(DataList dataList, OfferItem offerItem)
        {
            var ptc = dataList.CustomerDetails.First(customer => customer.CustId == offerItem.Services.First().ServiceBeneficiaries.First()).CustId.Split("_").First();

            switch (ptc)
            {

                case "ADT":
                    ptc = "ADULT";
                    break;

                case "GBE":
                    ptc = "TEEN";
                    break;

                case "CNN":
                    ptc = "CHILD";
                    break;

                case "INF":
                    ptc = "INFANT";
                    break;
            }

            return ptc;
        }

        private static IEnumerable<FareCalculation> GetFareCalculations(OfferDetails selectedOffer, DataList dataList, IEnumerable<TaxCodeInfo> taxCodeInfo)
        {
            var passengers = dataList.CustomerDetails.Select(x => x.CustId);

            return passengers.Select(custId =>
            {
                var offerItem = selectedOffer.OfferItems.First(oi => oi.Services.Select(s => s.ServiceBeneficiaries.Contains(custId)).First());
                var salePrice = offerItem.OfferItemPricing.First().SalePrice;

                return new FareCalculation
                {
                    PassengerType = GetPTC(dataList, offerItem),
                    Fare = (Price)salePrice.ToList(),
                    Taxes = GetTaxes(salePrice.First(price => price.PriceComponentType == Models.FlightOffer.Price.PriceComponentTypes.TotalTaxEnum)!.TaxBreakup!, taxCodeInfo)
                };
            });
        }

        private static IEnumerable<Taxes> GetTaxes(IEnumerable<Tax> taxBreakup, IEnumerable<TaxCodeInfo> taxCodeInfo)
        {
            return taxBreakup.Select(x => new Taxes
            {
                Code = x.Code,
                Description = taxCodeInfo.FirstOrDefault(tci => tci.Id == x.Code)?.Value,
                Price = new Price
                {
                    Amount = decimal.Parse(x.Amount.MonetaryAmount),
                    Currency = x.Amount.BaseCurrency
                }
            });
        }

        private static IEnumerable<Upgrade> GetUpgrades(IEnumerable<OfferDetails> upSellOffers, IEnumerable<Models.FlightOffer.Segment> flightSegments, Guid basketId, int totalOnDs, int currentSlice, decimal selectedOfferPrice, IEnumerable<CabinInfo> cabinInfo, IEnumerable<UpgradeInfo> upgradeInfo)
        {
            return upSellOffers.Select(upSellOffer =>
            {
                var service = upSellOffer.OfferItems.First().Services.Last();
                var dominantCabin = cabinInfo.FirstOrDefault(ci => ci.BrandID == service.FareDetails!.DominantFareProduct);
                var dominantFlightSegment = flightSegments.FirstOrDefault(fs => fs.SegmentId == service.FareDetails?.FareSegments?.FirstOrDefault(fareseg => fareseg.IsDominantLeg)?.FlightSegmentRefID);

                return new Upgrade
                {
                    FareFamilyType = dominantCabin?.FareFamily ?? service?.FareDetails?.DominantFareProduct,
                    Price = new(upSellOffer.SalePrice!, selectedOfferPrice),
                    Content = GetContent(dominantCabin?.FareFamily ?? service?.FareDetails?.DominantFareProduct, dominantFlightSegment?.OperatingCarrier, upgradeInfo),
                    Links = GetUpgradeLinks(basketId.ToString(), totalOnDs, currentSlice, upSellOffer.OfferId),
                    FareId = $"slice{totalOnDs}.{currentSlice}",
                    Id = upSellOffer.OfferId,
                    PromoCodeApplied = upSellOffer.PromoCodeApplied
                };
            });
        }

        public static Content? GetContent(string? dominantFareFamily, string? dominantCarrier, IEnumerable<UpgradeInfo> upgradeInfo)
        {
            var upsell = upgradeInfo?.FirstOrDefault(x => x.FareFamily == dominantFareFamily && x.Carrier == dominantCarrier) ?? upgradeInfo?.FirstOrDefault(x => x.FareFamily == dominantFareFamily && x.Carrier == "OA");

            if (upsell == null) return null;

            return (Content)upsell;
        }

        private static IEnumerable<Link> GetUpgradeLinks(string basketID, int totalOnDs, int currentSlice, string offerID)
        {
            if (basketID is null) return Enumerable.Empty<Link>();

            var urlPrefix = $"{ConfigReader.Get(Constants.Environment.SearchPostBackUrl)}";

            return new Link[]
            {
                new Link()
                {
                    Href = $"{urlPrefix}/{basketID}/summary/slice{totalOnDs}.{currentSlice}/{offerID}",
                    Rel = "upgrade"
                }
            };
        }

        private static IEnumerable<Flight> GetSummaryFlights(FlightOfferResponse offerMsResponse, OfferDetails selectedOffer, Guid basketId, IEnumerable<Models.Basket.BookingJourneyOfferDetails> bookingJourneyOfferDetails, int totalOnDs, Models.Brand.Brand? brandRs, IEnumerable<CabinInfo> cabinInfo, IEnumerable<AirportInfo> airportInfo, IEnumerable<AircraftInfo> aircraftInfo, IEnumerable<AirlineInfo> airlineInfo)
        {
            return offerMsResponse.DataList.Journey.Select(journey =>
            {
                var segments = offerMsResponse.DataList.FlightSegments.Where(fs => journey.FlightSegmentRefIds.Contains(fs.SegmentId));
                var service = selectedOffer.OfferItems.First().Services.FirstOrDefault(ser => ser.JourneyRefIds.Contains(journey.JourneyId));
                int stopCount = 0;

                var currentJourneyOnDLable = $"{offerMsResponse.DataList.OriginDestinationAssociations.FirstOrDefault(ond => journey.JourneyId.Equals(ond.JourneyRefIds.FirstOrDefault())).OdLabel}";
                var currentJourneyOnDepartureDate = DateTime.Parse(segments.FirstOrDefault()?.ScheduledDepartureTime?.LocalDateTime).ToString(Constants.Application.ISODateOnlyFormat);
                var bookingJourneyOffer = bookingJourneyOfferDetails?.FirstOrDefault(sod => sod?.OriginDestinationLabel == currentJourneyOnDLable && sod?.OriginDestinationDepartureDate == currentJourneyOnDepartureDate);

                var originDestination = offerMsResponse.DataList.OriginDestinationAssociations.FirstOrDefault(x => x.JourneyRefIds.Contains(journey.JourneyId)).OriginDestination;
                var dominantCabin = cabinInfo.FirstOrDefault(x => x.BrandID == service.FareDetails.DominantFareProduct);

                return new Flight()
                {
                    FlightId = journey.JourneyId,
                    Segments = segments.Select(x => new Segment(x, service, stopCount, cabinInfo, aircraftInfo, airlineInfo, airportInfo)),
                    Duration = Helpers.DateTimeHelper.ConvertColonDurationToIso(journey.Duration),
                    FareFamilyType = (offerMsResponse.IsAwardSearchResult ? dominantCabin?.AwardFareFamily : dominantCabin?.FareFamily) ?? service.FareDetails.DominantFareProduct,
                    FareFamilyFees = GetFareFamilyFees(segments, brandRs),
                    CabinName = dominantCabin?.BrandName ?? service.FareDetails.DominantFareProduct,
                    Origin = (Origin)(originDestination.OriginAirport, airportInfo),
                    Destination = (Destination)(originDestination.DestinationAirport, airportInfo),
                    Departure = Common.Helper.DateTimeHelper.ParseLocalDateTime(segments.FirstOrDefault()?.ScheduledDepartureTime?.LocalDateTime),
                    Arrival = Common.Helper.DateTimeHelper.ParseLocalDateTime(segments.LastOrDefault()?.ScheduledArrivalTime?.LocalDateTime),
                    FareId = bookingJourneyOffer != null ? $"slice{totalOnDs}.{bookingJourneyOffer?.Slice.Parse<int>()}|{bookingJourneyOffer?.OfferId}" : null,
                    Id = bookingJourneyOffer != null ? basketId.ToString() : null
                };
            });
        }

        private static IEnumerable<FlightPassengerBaggage> UpdateDimensionAllowances(IEnumerable<FlightPassengerBaggage> flightPassengerBaggage)
        {
            if (flightPassengerBaggage is null)
                return null;

            var carryOnBagsDimension = new Models.BFFSummary.DimensionAllowance { Length = "23", Width = "36", Height = "56", UnitOfMeasure = "CM", Linear = "0" };
            var checkedOnBagsDimension = new Models.BFFSummary.DimensionAllowance { Length = "90", Width = "75", Height = "43", UnitOfMeasure = "CM", Linear = "0" };

            var source = flightPassengerBaggage.ToList();

            foreach (var journey in source)
            {
                if (journey.CarryOnBag?.PassengerBaggageAllowance?.FreeBaggageAllowance?.DimensionAllowance != null)
                    journey.CarryOnBag.PassengerBaggageAllowance.FreeBaggageAllowance!.DimensionAllowance = carryOnBagsDimension;

                if (journey.CarryOnBag?.PassengerBaggageAllowance?.PaidBaggage != null)
                {
                    var paidBags = journey.CarryOnBag.PassengerBaggageAllowance.PaidBaggage.ToList();
                    foreach (var bag in paidBags)
                    {
                        bag.DimensionAllowance = carryOnBagsDimension;
                    }
                    journey.CarryOnBag.PassengerBaggageAllowance.PaidBaggage = paidBags;
                }

                if (journey.CheckedBag?.PassengerBaggageAllowance?.FreeBaggageAllowance?.DimensionAllowance != null)
                    journey.CheckedBag.PassengerBaggageAllowance.FreeBaggageAllowance!.DimensionAllowance = checkedOnBagsDimension;

                if (journey.CheckedBag?.PassengerBaggageAllowance?.PaidBaggage != null)
                {
                    var paidBags = journey.CheckedBag.PassengerBaggageAllowance.PaidBaggage.ToList();
                    foreach (var bag in paidBags)
                    {
                        bag.DimensionAllowance = checkedOnBagsDimension;
                    }
                    journey.CheckedBag.PassengerBaggageAllowance.PaidBaggage = paidBags;
                }
            }

            return source;
        }

        private static FareFamilyFees? GetFareFamilyFees(IEnumerable<Models.FlightOffer.Segment> segments, Models.Brand.Brand? brandRs)
        {
            if (brandRs != null)
            {
                var fare = GetFareFromBrandRs(segments.First().OriginAirportCode, segments.Last().DestinationAirportCode, brandRs);

                return fare != null ? new FareFamilyFees
                {
                    Cancellation = fare.Fees.Cancellation.Select(f => new FareFamilyRule
                    {
                        Assessment = f.Assessment,
                        Description = f.Description,
                        Headline = f.Headline,
                        Fee = f.Fee == null ? null : new Price
                        {
                            Currency = f.Fee.Currency,
                            Amount = f.Fee.Amount ?? 9999
                        }
                    }),
                    Change = fare.Fees.Change.Select(f => new FareFamilyRule
                    {
                        Assessment = f.Assessment,
                        Description = f.Description,
                        Headline = f.Headline,
                        Fee = f.Fee == null ? null : new Price
                        {
                            Currency = f.Fee.Currency,
                            Amount = f.Fee.Amount ?? 9999
                        }
                    })
                } : null;
            }
            return null;
        }
    }
}
