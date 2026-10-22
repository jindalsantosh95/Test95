using Microsoft.OpenApi.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Domain.Virgin.Models.Common;
using VAA.EAPI.Domain.Virgin.Models.Offer;
using VAA.EAPI.Microservices.Retail.Offer.Domain;
using VAA.EAPI.Microservices.Retail.Offer.Domain.FlightOffer;
using VAA.EAPI.Microservices.Retail.Offer.Infrastructure.Proxy.DeltaOffer;
using Legs = VAA.EAPI.Domain.Virgin.Models.Products.Flight.Legs;
using Price = VAA.EAPI.Domain.Virgin.Models.Common.Price;

namespace VAA.EAPI.Microservices.Retail.Offer.Mapper.Delta.FlightOfferPrice
{
    public static class OfferPriceRsMapper
    {
        public static FlightOfferResponse OfferPriceResMapper(Pos pos, OffersSetList offerResponse, string docId, bool isAwardSearch, string promoCode)
        {
            DataList dataList = new()
            {
                CustomerDetails = new(),
                FlightSegments = new(),
                Journey = new(),
                OriginDestinationAssociations = new(),
                Penalty = new()
            };

            BuildDataList(dataList, offerResponse.OfferDataList);

            return new FlightOfferResponse()
            {
                Pos = pos,
                DataList = dataList,
                Offers = GetOffers(
                    offerResponse,
                    docId,
                    isAwardSearch,
                    promoCode
                )
            };
        }

        #region BuildDataList
        private static void BuildDataList(DataList dataList, OfferDataList offerDataList)
        {
            AddCustomerDetails(dataList, offerDataList);
            AddOriginDestinationDetails(dataList, offerDataList);
            AddJourneyAndSegmentDetails(dataList, offerDataList);
            AddSegmentDetails(dataList, offerDataList);
            AddPenalties(dataList, offerDataList);
        }

        private static void AddCustomerDetails(DataList dataList, OfferDataList offerDataList)
        {
            foreach (var passenger in offerDataList.Customers)
            {
                dataList.CustomerDetails.Add(new()
                {
                    CustId = passenger.CustomerId ?? passenger.PassengerId,
                    Ptc = passenger.PassengerTypeCode
                });
            }
        }

        private static void AddOriginDestinationDetails(DataList dataList, OfferDataList offerDataList)
        {
            foreach (var ond in offerDataList.OriginDestinationTrips)
            {
                dataList.OriginDestinationAssociations.Add(new()
                {
                    OdLabel = ond.OriginDestinationId,
                    OriginDestination = new()
                    {
                        OriginAirport = ond.OriginAirportCode,
                        DestinationAirport = ond.DestinationAirportCode,
                    },
                    JourneyRefIds = ond.TripIds.Select(t => $"{Constants.Application.Journey}{t}").ToList()
                });
            }
        }

        public static void AddJourneyAndSegmentDetails(DataList dataList, OfferDataList offerDataList)
        {
            foreach (var trip in offerDataList.Trips)
            {
                dataList.Journey.Add(new()
                {
                    JourneyId = $"{Constants.Application.Journey}{trip.TripId}",
                    Duration = GetDuration(trip.TotalTripTime),
                    FlightSegmentRefIds = trip.FlightSegmentIdList.Select(t => $"{Constants.Application.FlightSegment}{t}")
                });
            }
        }

        private static void AddSegmentDetails(DataList dataList, OfferDataList offerDataList)
        {
            foreach (var seg in offerDataList.FlightSegments)
            {
                var segmentId = $"{Constants.Application.FlightSegment}{seg.FlightSegmentId}";
                var odRefId = (from ond in dataList.OriginDestinationAssociations
                               from j in dataList.Journey
                               where j.FlightSegmentRefIds.Contains(segmentId) && ond.JourneyRefIds.Contains(j.JourneyId)
                               select ond.OdLabel).First();
                var tripId = offerDataList.Trips.First(t => t.FlightSegmentIdList.Contains(seg.FlightSegmentId)).TripId;

                dataList.FlightSegments.Add(new()
                {
                    OdRefId = odRefId,
                    SegmentId = segmentId,
                    AirCraft = seg.AircraftTypeCode,
                    ScheduledArrivalTime = new()
                    {
                        LocalDateTime = seg.ScheduledArrivalLocalTs
                    },
                    ScheduledDepartureTime = new()
                    {
                        LocalDateTime = seg.ScheduledDepartureLocalTs
                    },
                    OriginAirportCode = seg.OriginAirportCode,
                    OriginCityCode = seg.OriginCityCode,
                    DestinationAirportCode = seg.DestinationAirportCode,
                    DestinationCityCode = seg.DestinationCityCode,
                    MarketingCarrier = seg.MarketingCarrier.CarrierCode,
                    MarketingFlightNumber = seg.MarketingCarrier.CarrierNum,
                    OperatingCarrier = seg.OperatingCarrier.CarrierCode,
                    OperatingFlightNumber = seg.OperatingCarrier.CarrierNum,
                    FlightDuration = GetDuration(seg.TotalAirTime),
                    Legs = AddLegs(seg.FlightLeg, tripId, seg.FlightSegmentNum),
                    ConnectionOrStopOver = seg.StopCnt.ToString(),
                    ConnectionDuration = seg.Layover?.LayoverDuration != null ? GetDuration(seg.Layover.LayoverDuration) : null,
                    PendingGovtApproval = seg.PendingGovtApproval
                });
            }
        }

        private static IEnumerable<Legs> AddLegs(IEnumerable<FlightLeg> legs, string tripId, string flightSegmentNum)
        {
            return legs.Select(x => new Legs
            {
                OriginAirportCode = x.OriginAirportCode,
                OperatingCarrier = x.OperatingCarrier.CarrierCode,
                MarketingCarrier = x.MarketingCarrier.CarrierCode,
                DestinationAirportCode = x.DestinationAirportCode,
                ScheduledDepartureTime = new() { LocalDateTime = x.ScheduledDepartureLocalTs },
                ScheduledArrivalTime = new() { LocalDateTime = x.ScheduledArrivalLocalTs },
                LegDuration = GetDuration(x.Duration),
                IsDominantLeg = x.DominantLeg,
                StopOverDuration = x.Layover?.LayoverDuration != null ? GetDuration(x.Layover.LayoverDuration) : null,
                LegId = string.Concat(tripId, "-", flightSegmentNum, "-", x.LegId),
                Distance = new EAPI.Domain.Virgin.Models.Products.Flight.FlightDistance() { Unit = x.Distance.UnitOfMeasure.GetDisplayName(), Distance = x.Distance.UnitOfMeasureCnt }
            });
        }

        private static void AddPenalties(DataList dataList, OfferDataList offerDataList)
        {
            foreach (var penalty in offerDataList.Penalties)
            {
                dataList.Penalty.Add(new()
                {
                    Id = penalty.PenaltyId,
                    Type = Constants.Application.ChangePenalty,
                    MaxPrice = new()
                    {
                        Amount = GetPenaltyAmount(penalty.Fee.PriceRange.Maximum)
                    },
                    MinPrice = new()
                    {
                        Amount = GetPenaltyAmount(penalty.Fee.PriceRange.Minimum)
                    },
                    Text = penalty.Fee.PentaltyPrePostFlightCode
                });
            }
        }

        private static CurrencyAmount GetPenaltyAmount(Infrastructure.Proxy.DeltaOffer.Price price)
        {
            return new CurrencyAmount
            {
                BaseCurrency = price.CurrencyEquivalentPrice.CurrencyCode,
                MonetaryAmount = price.CurrencyEquivalentPrice.FormattedCurrencyAmt
            };
        }
        #endregion

        private static IEnumerable<OfferDetails> GetOffers(OffersSetList offerResponse, string docId, bool isAwardSearch, string promoCode)
        {
            List<OfferDetails> offers = new();
            var baselineOfferItems = GetBaselineOfferItems(GetAllOffers(offerResponse), isAwardSearch);

            foreach (var offer in offerResponse.SelectedOfferSets.First().Offers)
            {
                if (!string.IsNullOrEmpty(offer.OfferId))
                {
                    offers.Add(new OfferDetails
                    {
                        OfferId = $"{docId}{Constants.Application.Pipeline}{offer.OfferId}",
                        SalePrice = GetSalesPrice(offer.OfferPricing.First(), isAwardSearch),
                        Selected = true,
                        UpsellOffer = false,
                        OfferItems = GetOfferItems(offer.OfferItems, offerResponse.OfferDataList, docId, isAwardSearch, baselineOfferItems),
                        OfferExpirationTimeLimit = offer.OfferExpirationTimeLimitUtcTs,
                        Refundable = offer.AdditionalOfferProperties.Refundable ?? false,
                        IsSaverFare = FlightOfferMapper.GetIsSaverFare(offer),
                        PromoCodeApplied = FlightOfferMapper.GetPromoCodeApplied(offer, promoCode),
                        PayWithPointsEligible = offer.AdditionalOfferProperties.PayWithMilesEligible ?? false,
                        PriceChanged = offer.AdditionalOfferProperties.FareChanged ?? false
                    });
                }
            }

            if (offerResponse.RecommendedOfferSets?.Any() ?? false)
            {
                foreach (var upsellOffer in offerResponse.RecommendedOfferSets)
                {
                    foreach (var offer in upsellOffer.Offers)
                    {
                        if (!string.IsNullOrEmpty(offer.OfferId))
                        {
                            offers.Add(new OfferDetails
                            {
                                OfferId = $"{docId}{Constants.Application.Pipeline}{offer.OfferId}",
                                SalePrice = GetSalesPrice(offer.OfferPricing.First(), isAwardSearch),
                                Selected = false,
                                UpsellOffer = true,
                                OfferItems = GetOfferItems(offer.OfferItems, offerResponse.OfferDataList, docId, isAwardSearch, baselineOfferItems),
                                OfferExpirationTimeLimit = offer.OfferExpirationTimeLimitUtcTs,
                                Refundable = offer.AdditionalOfferProperties.Refundable ?? false,
                                IsSaverFare = FlightOfferMapper.GetIsSaverFare(offer),
                                PromoCodeApplied = FlightOfferMapper.GetPromoCodeApplied(offer, promoCode),
                                PayWithPointsEligible = offer.AdditionalOfferProperties.PayWithMilesEligible ?? false
                            });
                        }
                    }
                }
            }

            return offers.Count > 0 ? offers : null;
        }

        private static IEnumerable<Infrastructure.Proxy.DeltaOffer.Offer> GetAllOffers(OffersSetList offerResponse)
        {
            return (offerResponse.SelectedOfferSets ?? Enumerable.Empty<OffersSet>())
                .Concat(offerResponse.RecommendedOfferSets ?? Enumerable.Empty<OffersSet>())
                .SelectMany(offerSet => offerSet.Offers ?? Enumerable.Empty<Infrastructure.Proxy.DeltaOffer.Offer>());
        }

        private static IEnumerable<Infrastructure.Proxy.DeltaOffer.OfferItem> GetBaselineOfferItems(IEnumerable<Infrastructure.Proxy.DeltaOffer.Offer> offers, bool isAwardSearch)
        {
            if (!isAwardSearch || offers == null || !ConfigReader.Try<bool>(Constants.Environment.EnableOfferBaselineFareDifferences, false))
            {
                return null;
            }

            var economyBrands = new[] { "VSCL", "MAIN", "AFST", "E", "KLEC" };
            var premiumBrands = new[] { "AFPE", "PE", "DPPS", "KLPE", "VSPE" };
            var economyOffer = offers.FirstOrDefault(offer => economyBrands.Contains(offer.AdditionalOfferProperties?.DominantSegmentBrandId));

            if (economyOffer?.OfferItems?.Any() == true)
            {
                return economyOffer.OfferItems;
            }

            return offers.FirstOrDefault(offer => premiumBrands.Contains(offer.AdditionalOfferProperties?.DominantSegmentBrandId))?.OfferItems;
        }

        private static IEnumerable<OfferitemPricing> GetOfferItemPricings(IEnumerable<Pricing> offerPricings, bool isAwardSearch, Infrastructure.Proxy.DeltaOffer.OfferItem baselineOfferItem = null)
        {
            return offerPricings.Select((pricing, pricingIndex) =>
            {
                var baseAmt = pricing.BaseAmt.CurrencyEquivalentPrice;
                var totalAmt = pricing.TotalAmt.CurrencyEquivalentPrice;
                var totalTaxAmt = pricing.TaxSummary.TotalTaxAmt.CurrencyEquivalentPrice;
                var totalPoints = pricing.TotalAmt.MilesEquivalentPrice;

                List<Price> salePrice = new()
                    {
                        new()
                        {
                            Amount = new()
                            {
                                BaseCurrency = baseAmt.CurrencyCode,
                                MonetaryAmount = baseAmt.FormattedCurrencyAmt
                            },
                            PriceComponentType = Price.PriceComponentTypes.BaseFareEnum
                        },
                        new()
                        {
                            Amount = new()
                            {
                                BaseCurrency = totalAmt.CurrencyCode,
                                MonetaryAmount = totalAmt.FormattedCurrencyAmt
                            },
                            PriceComponentType = Price.PriceComponentTypes.BasePriceEnum
                        },
                        new()
                        {
                            Amount = new()
                            {
                                BaseCurrency = totalTaxAmt.CurrencyCode,
                                MonetaryAmount = totalTaxAmt.FormattedCurrencyAmt
                            },
                            PriceComponentType = Price.PriceComponentTypes.TotalTaxEnum,
                            TaxBreakup = GetTaxBreakdown(pricing.TaxSummary.Taxes)
                        }
                    };

                if (isAwardSearch)
                {
                    salePrice.Add(new()
                    {
                        PointsAmount = new()
                        {
                            PointsCount = totalPoints?.MileCnt ?? 0
                        },
                        PriceComponentType = Price.PriceComponentTypes.PointsEnum
                    });
                }

                List<Price> salePriceDifference = null;
                if (isAwardSearch)
                {
                    var baselinePoints = baselineOfferItem?.RetailItems?.FirstOrDefault()?.RetailItemPricing?.ElementAtOrDefault(pricingIndex)?.TotalAmt?.MilesEquivalentPrice?.MileCnt ?? 0;
                    var currentPoints = totalPoints?.MileCnt ?? 0;
                    var pointsDifference = currentPoints - baselinePoints;

                    if (currentPoints != 0 && pointsDifference != 0)
                    {
                        salePriceDifference = new List<Price>
                        {
                            new Price
                            {
                                PointsAmount = new() { PointsCount = Math.Abs(pointsDifference) },
                                PriceComponentType = Price.PriceComponentTypes.PointsEnum,
                                DifferenceDetails = new PointsDifferenceDetails
                                {
                                    PointsDifference = Math.Abs(pointsDifference),
                                    PointsDifferenceSign = pointsDifference < 0 ? "-" : "+"
                                }
                            }
                        };
                    }
                }

                return new OfferitemPricing
                {
                    SalePrice = salePrice,
                    SalePriceDifference = salePriceDifference
                };
            });
        }

        private static IEnumerable<Tax> GetTaxBreakdown(IEnumerable<TaxType> taxes)
        {
            return taxes.Select(tax => new Tax
            {
                Amount = new()
                {
                    BaseCurrency = tax.TaxAmt.CurrencyEquivalentPrice.CurrencyCode,
                    MonetaryAmount = tax.TaxAmt.CurrencyEquivalentPrice.FormattedCurrencyAmt
                },
                Code = tax.TaxCode
            });
        }

        private static IEnumerable<EAPI.Domain.Virgin.Models.Offer.OfferItem> GetOfferItems(IEnumerable<Infrastructure.Proxy.DeltaOffer.OfferItem> offerItems, OfferDataList offerDataList, string docId, bool isAwardSearch, IEnumerable<Infrastructure.Proxy.DeltaOffer.OfferItem> baselineOfferItems = null)
        {
            var baselineOfferItemsList = baselineOfferItems?.ToList();

            return offerItems.Select((offerItem, offerItemIndex) => new EAPI.Domain.Virgin.Models.Offer.OfferItem
            {
                OfferItemId = $"{docId}{Constants.Application.Pipeline}{offerItem.OfferItemId}",
                OfferitemPricing = string.IsNullOrWhiteSpace(offerItem.OfferSupplier.OfferId) ? Enumerable.Empty<OfferitemPricing>() : GetOfferItemPricings(offerItem.RetailItems.First().RetailItemPricing, isAwardSearch, baselineOfferItemsList?.ElementAtOrDefault(offerItemIndex)),
                Services = GetOfferServices(offerItem, offerDataList)
            });
        }

        private static IEnumerable<Price> GetSalesPrice(Pricing offerPricing, bool isAwardSearch)
        {
            var baseAmt = offerPricing.BaseAmt.CurrencyEquivalentPrice;
            var totalAmt = offerPricing.TotalAmt.CurrencyEquivalentPrice;
            var totalTaxAmt = offerPricing.TaxSummary.TotalTaxAmt.CurrencyEquivalentPrice;
            var totalPoints = offerPricing.TotalAmt.MilesEquivalentPrice;

            List<Price> salePrices = new()
            {
                new()
                {
                    Amount = new() {
                        BaseCurrency = baseAmt.CurrencyCode,
                        MonetaryAmount = baseAmt.FormattedCurrencyAmt
                    },
                    PriceComponentType = Price.PriceComponentTypes.BaseFareEnum
                },
                new()
                {
                    Amount = new() {
                        BaseCurrency = totalAmt.CurrencyCode,
                        MonetaryAmount = totalAmt.FormattedCurrencyAmt
                    },
                    PriceComponentType = Price.PriceComponentTypes.BasePriceEnum
                },
                new()
                {
                    Amount = new() {
                        BaseCurrency = totalTaxAmt.CurrencyCode,
                        MonetaryAmount = totalTaxAmt.FormattedCurrencyAmt
                    },
                    PriceComponentType = Price.PriceComponentTypes.TotalTaxEnum,
                    TaxBreakup = GetTaxBreakdown(offerPricing.TaxSummary.Taxes)
                }
            };

            if (isAwardSearch)
            {
                salePrices.Add(new()
                {
                    PointsAmount = new()
                    {
                        PointsCount = totalPoints?.MileCnt ?? 0
                    },
                    PriceComponentType = Price.PriceComponentTypes.PointsEnum
                });
            }

            return salePrices;
        }

        private static IEnumerable<OfferService> GetOfferServices(Infrastructure.Proxy.DeltaOffer.OfferItem offerItem, OfferDataList offerDataList)
        {
            var penaltyIds = offerItem.RetailItems.First().RetailItemMetaData.FareInformation.First().Endorsement?.FirstOrDefault()?.PenaltyIds ?? Enumerable.Empty<string>();
            var changePenaltyRefIds = penaltyIds.Where(pid => offerDataList.Penalties.Any(p => pid.Contains(p.PenaltyId) && p.Fee.FeeCode.Is(Constants.Application.ChangePenalty))).ToList();
            var cancellationPenaltyRefIds = penaltyIds.Where(pid => offerDataList.Penalties.Any(p => pid.Contains(p.PenaltyId) && !p.Fee.FeeCode.Is(Constants.Application.ChangePenalty))).ToList();

            return offerDataList.Trips.Select(trip => new OfferService()
            {
                ServiceBeneficiaries = offerItem.PassengerIds,
                JourneyRefIds = trip.TripId.Select(t => $"{Constants.Application.Journey}{t}"),
                FlightSegmentsRefIds = trip.FlightSegmentIdList.Select(fs => $"{Constants.Application.FlightSegment}{fs}"),
                PenaltyRef = new()
                {
                    ChangePenaltyRefIds = changePenaltyRefIds,
                    CancellationPenaltyRefIds = cancellationPenaltyRefIds
                },
                FareDetails = string.IsNullOrWhiteSpace(offerItem.OfferSupplier.OfferId) ? default : GetFareDetails(offerItem, offerDataList, trip.FlightSegmentIdList)
            });
        }

        private static FareDetails GetFareDetails(Infrastructure.Proxy.DeltaOffer.OfferItem offerItem, OfferDataList offerDataList, IEnumerable<string> flightSegmentIds)
        {
            var fareInformation = offerItem.RetailItems.First().RetailItemMetaData.FareInformation.First();

            var fare = from leg in fareInformation.BrandByFlightLegs
                       from fareRestriction in fareInformation.FareRestrictionByFlightSegment
                       from fseg in offerDataList.FlightSegments
                       where fareRestriction.OriginAirportCode == fseg.OriginAirportCode && fareRestriction.DestinationAirportCode == fseg.DestinationAirportCode
                       && leg.FlightSegmentId == fseg.FlightSegmentId
                       && flightSegmentIds.Contains(leg.FlightSegmentId)
                       select new { leg, fareRestriction };

            var dominantLeg = fare.FirstOrDefault(x => x.leg.DominantLeg ?? false);

            return new FareDetails()
            {
                FareBasisCode = dominantLeg?.fareRestriction.FareBasisCode.FirstOrDefault(),
                DominantFareProduct = dominantLeg?.leg.BrandId,
                FareSegments = fare.Select(fs => FlightOfferMapper.GetFareLeg(fs.leg, fareInformation.Endorsement, offerDataList.FlightSegments.First(fls => fls.FlightSegmentId == fs.leg.FlightSegmentId)))
            };
        }

        private static string GetDuration(Duration duration)
        {
            return $"{duration.HourCnt:d2}:{duration.MinuteCnt:d2}";
        }
    }
}
