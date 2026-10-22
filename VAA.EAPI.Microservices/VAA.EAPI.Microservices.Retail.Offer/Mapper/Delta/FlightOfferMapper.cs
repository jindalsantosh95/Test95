using Microsoft.Extensions.Logging;
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
using Price = VAA.EAPI.Domain.Virgin.Models.Common.Price;

namespace VAA.EAPI.Microservices.Retail.Offer.Mapper.Delta
{
    public static class FlightOfferMapper
    {
        public static OfferSearchCriteria FlightOfferRequestMapper(FlightOfferRequest offerRequest, int pageNumber = default, int resultCount = default)
        {
            return
                 new
                 OfferSearchCriteria()
                 {
                     ProductGroups = new List<ProductCategory>
                     {
                         new ProductCategory { ProductCategoryCode = ProductCategory.ProductCategoryCodeEnum.FLIGHTS },
                     },
                     OffersCriteria = new OffersCriteria
                     {
                         PricingCriteria = new PricingCriteria { PriceableIn = new List<string> { offerRequest.FlightsearchRequest.AwardSearch ? Constants.Application.Miles : Constants.Application.Currency }, CurrencyCode = offerRequest.CurrencyCode },
                         Preferences = new(),
                         ResultsPageNum = pageNumber,
                         ResultsPerRequestNum = resultCount,
                         Qualifiers = offerRequest.FlightsearchRequest.PromoCode.IsNullOrEmpty() ? null : new Qualifiers
                         {
                             DiscountCertificate = new List<Infrastructure.Proxy.DeltaOffer.DiscountCertificate>
                             {
                                 new Infrastructure.Proxy.DeltaOffer.DiscountCertificate
                                 {
                                     CertificateTypeAccountCode = offerRequest.FlightsearchRequest.PromoCode.ToUpper()
                                 }
                             }
                         },
                         FlightRequestCriteria = new FlightOfferCriteria
                         {
                             CurrentTripIndexId = offerRequest.FlightsearchRequest.CurrentTripIndexId,
                             SortByBrandId = offerRequest.FlightsearchRequest.BrandId ?? Constants.Application.VS_ECON_LIGHT,
                             SelectedOfferId = string.IsNullOrEmpty(offerRequest.FlightsearchRequest?.OfferID) ? null : offerRequest.FlightsearchRequest.OfferID.Split("|")[1],
                             SearchOriginDestination = (from Ond in offerRequest.FlightsearchRequest.SearchOriginDestinations
                                                        select new Infrastructure.Proxy.DeltaOffer.SearchOriginDestination
                                                        {
                                                            Destinations = new Airport[] { new Airport { AirportCode = Ond.Destination } },
                                                            Origins = new Airport[] { new Airport { AirportCode = Ond.Origin } },
                                                            ConnectionAirportCode = Ond.ConnectionAirports,
                                                            DepartureLocalTs = DateTime.Parse(Ond.DepartureDate, System.Globalization.CultureInfo.InvariantCulture).ToString(Constants.Application.DateTimeFormat)
                                                        }),
                         }
                     },
                     Sender = (Party)offerRequest.LoyaltyMember,
                     Customers = (from cus in offerRequest.CustomerDetails
                                  select new Passenger
                                  {
                                      CustomerId = cus.CustId,
                                      PassengerTypeCode = cus.Ptc,
                                      PassengerId = cus.CustId
                                  }).ToList(),
                 };
        }

        public static FlightOfferResponse FlightOfferResponseMapper(FlightOfferRequest offerRequest, OffersSetList offerResponse, string docId, IEnumerable<Price> SliceZeroPrice, ILogger logger)
        {
            bool isAwardSearch = offerRequest.FlightsearchRequest.AwardSearch;

            DataList dataList = new()
            {
                CustomerDetails = new(),
                FlightSegments = new(),
                Journey = new(),
                OriginDestinationAssociations = new()
            };

            BuildDataList(offerRequest, dataList, offerResponse.OfferDataList);

            return new FlightOfferResponse()
            {

                Pos = offerRequest.Pos,
                DataList = dataList,
                Offers = GetOffers(offerResponse, docId, SliceZeroPrice, offerRequest.FlightsearchRequest.PromoCode, isAwardSearch, logger)
            };
        }

        #region BuildDataList
        private static void BuildDataList(FlightOfferRequest request, DataList dataList, OfferDataList offerDataList)
        {
            AddCustomerDetails(dataList, offerDataList);
            AddOriginDestinationDetails(request, dataList, offerDataList);
            AddJourneyAndSegmentDetails(dataList, offerDataList);
            AddSegmentDetails(dataList, offerDataList);
        }

        private static void AddCustomerDetails(DataList dataList, OfferDataList offerDataList)
        {
            foreach (var passenger in offerDataList.Customers)
            {
                dataList.CustomerDetails.Add(new()
                {
                    CustId = passenger.CustomerId,
                    Ptc = passenger.PassengerTypeCode
                });
            }
        }

        private static void AddOriginDestinationDetails(FlightOfferRequest request, DataList dataList, OfferDataList offerDataList)
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

        private static List<EAPI.Domain.Virgin.Models.Products.Flight.Legs> AddLegs(IEnumerable<FlightLeg> legs, string tripId, string flightSegmentNum)
        {
            List<EAPI.Domain.Virgin.Models.Products.Flight.Legs> lstLegs = new();

            legs.ToList().ForEach(x =>
            {
                lstLegs.Add(new EAPI.Domain.Virgin.Models.Products.Flight.Legs()
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
                    Distance = new EAPI.Domain.Virgin.Models.Products.Flight.FlightDistance() { Unit = x.Distance?.UnitOfMeasure.GetDisplayName(), Distance = x.Distance?.UnitOfMeasureCnt }
                });
            });
            return lstLegs;
        }
        #endregion

        private static FareDetails GetFareDetails(Infrastructure.Proxy.DeltaOffer.OfferItem offerItem, OfferDataList offerDataList, bool isAwardSearch, Infrastructure.Proxy.DeltaOffer.OfferItem baselineOfferItem)
        {
            var fareInformation = offerItem.RetailItems.First().RetailItemMetaData.FareInformation.FirstOrDefault();

            var fareDetails = new FareDetails()
            {
                DominantFareProduct = fareInformation.DominantSegmentBrandId
            };

            if (string.IsNullOrWhiteSpace(offerItem.OfferSupplier.OfferId)) return fareDetails;

            fareDetails.FareBasisCode = fareInformation.Endorsement.FirstOrDefault()?.ExtendedFareCode; //Logic to calculate fare basis codes to be finalized with business 
            fareDetails.FareSegments = GetFareSegments(fareInformation, offerDataList.FlightSegments, isAwardSearch, baselineOfferItem);

            return fareDetails;
        }

        private static IEnumerable<FareSegments> GetFareSegments(Fare fareInformation, IEnumerable<FlightSegment> flightSegments, bool isAwardSearch, Infrastructure.Proxy.DeltaOffer.OfferItem baselineOfferItem)
        {
            if (!fareInformation.Endorsement.Any()) return null;

            var includeCabinSalePrices = isAwardSearch && ConfigReader.Try<bool>(Constants.Environment.EnableOfferBaselineFareDifferences, false);
            var fareLegs = from leg in fareInformation.BrandByFlightLegs
                           where fareInformation.Endorsement.Any(x => x.BookingCodes.Any(e => e.Contains(leg.CosCode)))
                           let flightSegment = flightSegments.First(fs => fs.FlightSegmentId == leg.FlightSegmentId)
                           select GetFareLeg(
                               leg,
                               fareInformation.Endorsement,
                               flightSegment,
                               isAwardSearch,
                               includeCabinSalePrices ? GetBaselineCabinPoints(baselineOfferItem, leg, flightSegment) : null);

            var filteredSegments = fareLegs.GroupBy(item => item.FlightSegmentRefID)
                                .Select(group =>
                                {
                                    if (group.Count() > 1 && group.DistinctBy(x => x.IsDominantLeg).Count() != 1) { group.First().IsDominantLeg = true; }
                                    return group.First();
                                });
            return filteredSegments.ToList(); // ToList forces the enumeration and so any exceptions are thrown immediately.
        }

        // This is per-leg, even though object is called FareSegments (See #552396)
        public static FareSegments GetFareLeg(BrandByFlightLeg leg, IEnumerable<FareEndorsement> fareEndorsements, FlightSegment flightSegment)
        {
            return GetFareLeg(leg, fareEndorsements, flightSegment, false, null);
        }

        public static FareSegments GetFareLeg(BrandByFlightLeg leg, IEnumerable<FareEndorsement> fareEndorsements, FlightSegment flightSegment, bool isAwardSearch, int? baselineCabinPoints)
        {
            // For multi-leg segments (e.g. LHR-BGI-ANU), sometimes fareEndorsement is for the whole segment (LHR-ANU) and sometimes it is per leg (LHR-BGI and BGI-ANU).
            var fareEndorsement = GetFareEndorsement(leg, fareEndorsements, flightSegment);
            var includeCabinSalePrices = isAwardSearch && ConfigReader.Try<bool>(Constants.Environment.EnableOfferBaselineFareDifferences, false);
            var fareSegment = new FareSegments()
            {
                BookingClass = leg.CosCode,
                CabinName = leg.BrandId,
                FlightSegmentRefID = $"{Constants.Application.FlightSegment}{leg.FlightSegmentId}",
                IsDominantLeg = leg.DominantLeg ?? false,
                FareBasisCode = fareEndorsement?.ExtendedFareCode,
                TicketDesignator = fareEndorsement?.TicketDesignator,
                IsSaverFare = !string.IsNullOrEmpty(fareEndorsement?.TicketDesignator) ? Constants.Application.SaverFareTicketDesignators.Contains(fareEndorsement?.TicketDesignator) : false,
                BaselineCabinSalePrice = includeCabinSalePrices ? GetPointsPrice(baselineCabinPoints) : null,
                CurrentCabinSalePrice = includeCabinSalePrices ? GetPointsPrice(fareEndorsement?.SalesAdjustedPrice?.MilesEquivalentPrice?.MileCnt) : null
            };
            return fareSegment;
        }

        private static int? GetBaselineCabinPoints(Infrastructure.Proxy.DeltaOffer.OfferItem baselineOfferItem, BrandByFlightLeg currentLeg, FlightSegment flightSegment)
        {
            var baselineFareInformation = baselineOfferItem?.RetailItems?.FirstOrDefault()?.RetailItemMetaData?.FareInformation?.FirstOrDefault();
            var baselineLeg = baselineFareInformation?.BrandByFlightLegs?.FirstOrDefault(leg => leg.FlightSegmentId == currentLeg.FlightSegmentId);
            var baselineFareEndorsement = baselineLeg == null ? null : GetFareEndorsement(baselineLeg, baselineFareInformation.Endorsement, flightSegment);

            return baselineFareEndorsement?.SalesAdjustedPrice?.MilesEquivalentPrice?.MileCnt;
        }

        private static FareEndorsement GetFareEndorsement(BrandByFlightLeg leg, IEnumerable<FareEndorsement> fareEndorsements, FlightSegment flightSegment)
        {
            if (fareEndorsements == null) return null;

            return fareEndorsements.FirstOrDefault(x => (x.BookingCodes?.Any(e => e.Contains(leg.CosCode)) ?? false)
                                                         && x.OriginCityCode == flightSegment.OriginCityCode
                                                         && x.DestinationCityCode == flightSegment.DestinationCityCode)
                ?? fareEndorsements.FirstOrDefault(x => (x.BookingCodes?.Any(e => e.Contains(leg.CosCode)) ?? false)
                                                         && (x.OriginCityCode == flightSegment.OriginCityCode
                                                             || x.DestinationCityCode == flightSegment.DestinationCityCode));
        }

        private static Price GetPointsPrice(int? pointsCount)
        {
            return pointsCount.HasValue
                ? new Price
                {
                    PointsAmount = new() { PointsCount = pointsCount.Value },
                    PriceComponentType = Price.PriceComponentTypes.PointsEnum
                }
                : null;
        }

        private static List<OfferDetails> GetOffers(OffersSetList offerResponse, string docId, IEnumerable<Price> sliceZeroPrice, string promoCode, bool isAwardSearch, ILogger logger)
        {
            List<OfferDetails> offers = new();
            var sliceZeroBasePrice = sliceZeroPrice?.FirstOrDefault(x => x.PriceComponentType == Price.PriceComponentTypes.BasePriceEnum)?.Amount.MonetaryAmount ?? null;

            int sliceZeroPointsCount = 0;
            if (isAwardSearch)
            {
                sliceZeroPointsCount = sliceZeroPrice?.FirstOrDefault(x => x.PriceComponentType == Price.PriceComponentTypes.PointsEnum)?.PointsAmount.PointsCount ?? 0;
            }

            // Flex fares from other airlines (non-VS) cause errors later in the booking journey, so are removed from the results.
            var ignoreNonVsFlexFares = ConfigReader.Get<bool>(Constants.Environment.IgnoreFlexFares);
            foreach (var offersSet in offerResponse.OffersSets)
            {
                try
                {
                    var baselineOfferItems = isAwardSearch ? GetBaselineOfferItems(offersSet?.Offers) : null;

                    foreach (var offerGroup in offersSet.Offers.GroupBy(x => x.AdditionalOfferProperties.DominantSegmentBrandId).ToList())
                    {
                        // Delta will sometimes return two copies of the same offer, one marked as
                        // refundable and one marked non-refundable. They have identical prices, and
                        // based on the fare family (e.g. VSCL) we can tell they are non-refundable.
                        // Therefore where duplicates are returned we are preferring the non-refundable
                        // version
                        var offer = offerGroup
                            .OrderBy(o => o.SoldOut)
                            .ThenBy(o => o.AdditionalOfferProperties.Refundable)
                            .First();

                        var flexFaresExist = false;
                        if (ignoreNonVsFlexFares)
                        {
                            flexFaresExist = (from oi in offer.OfferItems
                                              from rs in offerResponse.OfferDataList.FlightSegments
                                              where oi.FlightSegmentIds.Contains(rs.FlightSegmentId)
                                              && (offer.AdditionalOfferProperties.Refundable ?? false)
                                              && (offer.OfferId.IsNullOrEmpty() || rs.OperatingCarrier.CarrierCode != Constants.Application.VsFlightCode)
                                              select rs).Any();
                        }

                        if (!(ignoreNonVsFlexFares && flexFaresExist))
                        {
                            offers.Add(new OfferDetails
                            {
                                OfferId = string.IsNullOrWhiteSpace(offer.OfferId) ? default : $"{docId}{Constants.Application.Pipeline}{offer.OfferId}",
                                SalePrice = string.IsNullOrWhiteSpace(offer.OfferId) ? default : GetSalesPrice(offer.OfferPricing.First(), sliceZeroBasePrice, isAwardSearch, sliceZeroPointsCount),
                                Selected = false,
                                UpsellOffer = false,
                                OfferItems = GetOfferItems(offer.OfferItems, offerResponse.OfferDataList, docId, isAwardSearch, baselineOfferItems),
                                OfferExpirationTimeLimit = offer.OfferExpirationTimeLimitUtcTs,
                                Refundable = offer.AdditionalOfferProperties.Refundable ?? false,
                                PromoCodeApplied = GetPromoCodeApplied(offer, promoCode),
                                IsSaverFare = GetIsSaverFare(offer),
                                PayWithPointsEligible = offer.AdditionalOfferProperties.PayWithMilesEligible ?? false,
                                Availability = string.IsNullOrWhiteSpace(offer.OfferId) ? (offer.AdditionalOfferProperties.SoldOut == true ? AvailabilityStatus.SoldOutEnum : AvailabilityStatus.NotOfferedEnum) : null,
                                AvailableSeatCount = offer.OfferItems.FirstOrDefault()?.RetailItems?.FirstOrDefault()?.RetailItemMetaData?.FareInformation?.FirstOrDefault()?.AvailableSeatCnt
                            });
                        }
                    }
                }
                catch (Exception e)
                {
                    // Log then skip this flight
                    logger.LogError(e, "Flight excluded from results due to exception. {0}", new
                    {
                        segments = string.Join(", ", offersSet?.Offers?.FirstOrDefault()?.OfferItems?.FirstOrDefault()?.FlightSegmentIds)
                    });
                }
            }

            if (offerResponse.SelectedOfferSets != null)
            {
                foreach (var selectedOfferSets in offerResponse.SelectedOfferSets)
                {
                    var selectedBaselineOfferItems = isAwardSearch ? GetBaselineOfferItems(selectedOfferSets?.Offers) : null;

                    foreach (var selectedOffer in selectedOfferSets.Offers)
                    {
                        selectedOffer.OfferPricing.First().TotalAmt.CurrencyEquivalentPrice.CurrencyAmt = Convert.ToDecimal(sliceZeroBasePrice);
                        selectedOffer.OfferPricing.First().TotalAmt.CurrencyEquivalentPrice.FormattedCurrencyAmt = sliceZeroBasePrice;

                        if (isAwardSearch)
                        {
                            selectedOffer.OfferPricing.First().TotalAmt.MilesEquivalentPrice.MileCnt = sliceZeroPointsCount;
                        }

                        offers.Add(new OfferDetails
                        {
                            OfferId = $"{docId}{Constants.Application.Pipeline}{selectedOffer.OfferId}",
                            SalePrice = GetSalesPrice(selectedOffer.OfferPricing.First(), sliceZeroBasePrice, isAwardSearch, sliceZeroPointsCount),
                            Selected = true,
                            UpsellOffer = false,
                            OfferItems = GetOfferItems(selectedOffer.OfferItems, offerResponse.OfferDataList, docId, isAwardSearch, selectedBaselineOfferItems),
                            OfferExpirationTimeLimit = selectedOffer.OfferExpirationTimeLimitUtcTs,
                            Refundable = selectedOffer.AdditionalOfferProperties.Refundable ?? false,
                            IsSaverFare = GetIsSaverFare(selectedOffer),
                            PayWithPointsEligible = selectedOffer.AdditionalOfferProperties.PayWithMilesEligible ?? false
                        });
                    }
                }
            }
            return offers.Count > 0 ? offers : null;
        }

        private static IEnumerable<Infrastructure.Proxy.DeltaOffer.OfferItem> GetBaselineOfferItems(IEnumerable<Infrastructure.Proxy.DeltaOffer.Offer> offers)
        {
            if (offers == null) return null;

            var economyBrands = new[] { "VSCL", "MAIN", "AFST", "E","KLEC" };
            var premiumBrands = new[] { "AFPE", "PE", "DPPS", "KLPE", "VSPE" };

            if (!ConfigReader.Try<bool>(Constants.Environment.EnableOfferBaselineFareDifferences, false))
            {
                return null;
            }

            var economyOffer = offers.FirstOrDefault(o => economyBrands.Contains(o.AdditionalOfferProperties?.DominantSegmentBrandId));
            if (economyOffer?.AdditionalOfferProperties?.SoldOut != true && economyOffer?.OfferItems != null && economyOffer.OfferItems.Any())
            {
                return economyOffer.OfferItems;
            }

            var premiumOffer = offers.FirstOrDefault(o => premiumBrands.Contains(o.AdditionalOfferProperties?.DominantSegmentBrandId));
            return premiumOffer?.AdditionalOfferProperties?.SoldOut != true ? premiumOffer?.OfferItems : null;
        }

        // Also called by OfferPriceRsMapper
        public static bool GetIsSaverFare(Infrastructure.Proxy.DeltaOffer.Offer offer)
        {
            var endorsements = offer.OfferItems.SelectMany(oi => oi.RetailItems).Select(ri => ri.RetailItemMetaData).SelectMany(rimd => rimd.FareInformation).SelectMany(fi => fi.Endorsement);
            return endorsements != null && endorsements.Any(e => Constants.Application.SaverFareTicketDesignators.Contains(e.TicketDesignator));
        }

        public static string GetPromoCodeApplied(Infrastructure.Proxy.DeltaOffer.Offer offer, string promoCode)
        {
            if (promoCode.IsNullOrEmpty()) return null;
            return offer.AdditionalOfferProperties.DiscountAvailable == true ? promoCode : null;
        }

        private static List<Price> GetSalesPrice(Pricing offerPricing, string sliceZeroBasePrice, bool isAwardSearch, int sliceZeroPointsCount)
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
                    PriceComponentType = Price.PriceComponentTypes.TotalTaxEnum
                }
            };



            var currentSalePrice = salePrices.First(x => x.PriceComponentType == Price.PriceComponentTypes.BasePriceEnum).Amount.MonetaryAmount;


            if (!string.IsNullOrEmpty(sliceZeroBasePrice) && !string.IsNullOrEmpty(currentSalePrice))
            {
                var pricediff = Convert.ToDecimal(currentSalePrice) - Convert.ToDecimal(sliceZeroBasePrice);
                var saleprice = salePrices.First(x => x.PriceComponentType == Price.PriceComponentTypes.BasePriceEnum).Amount;
                saleprice.PriceDifference = pricediff >= 0 ? pricediff : -pricediff;
                saleprice.PriceDifferenceSign = Convert.ToDecimal(sliceZeroBasePrice) > Convert.ToDecimal(currentSalePrice) ? "-" : "+";
            }

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

                var currentPointsTotal = salePrices.First(x => x.PriceComponentType == Price.PriceComponentTypes.PointsEnum).PointsAmount.PointsCount;
                var saleprice = salePrices.First(x => x.PriceComponentType == Price.PriceComponentTypes.BasePriceEnum).Amount;
                if (sliceZeroPointsCount != 0 && currentPointsTotal != 0)
                {
                    var pointsdiff = currentPointsTotal - sliceZeroPointsCount;
                    var pointsAmount = salePrices.First(x => x.PriceComponentType == Price.PriceComponentTypes.PointsEnum);
                    pointsAmount.DifferenceDetails = new PointsDifferenceDetails()
                    {
                        PointsDifference = pointsdiff >= 0 ? pointsdiff : -pointsdiff,
                        PointsDifferenceSign = sliceZeroPointsCount > currentPointsTotal ? "-" : "+"
                    };
                }
            }

            return salePrices;
        }

        private static IEnumerable<EAPI.Domain.Virgin.Models.Offer.OfferItem> GetOfferItems(IEnumerable<Infrastructure.Proxy.DeltaOffer.OfferItem> offerItems, OfferDataList offerDataList, string docId, bool isAwardSearch, IEnumerable<Infrastructure.Proxy.DeltaOffer.OfferItem> baselineOfferItems = null)
        {
            var baselineList = baselineOfferItems?.ToList();
            var offerItemsList = offerItems?.ToList() ?? new List<Infrastructure.Proxy.DeltaOffer.OfferItem>();

            return offerItemsList.Select((offerItem, index) => new EAPI.Domain.Virgin.Models.Offer.OfferItem
            {
                OfferItemId = $"{docId}{Constants.Application.Pipeline}{offerItem.OfferItemId}",
                OfferitemPricing = string.IsNullOrWhiteSpace(offerItem.OfferSupplier.OfferId) ? Enumerable.Empty<OfferitemPricing>() : GetOfferItemPricings(offerItem, isAwardSearch, baselineList?.ElementAtOrDefault(index)),
                Services = GetOfferServices(offerItem, offerDataList, isAwardSearch, baselineList?.ElementAtOrDefault(index))
            }).ToList(); // ToList forces the enumeration and so any exceptions are thrown immediately.
        }

        private static IEnumerable<OfferitemPricing> GetOfferItemPricings(Infrastructure.Proxy.DeltaOffer.OfferItem offerItem, bool isAwardSearch, Infrastructure.Proxy.DeltaOffer.OfferItem baselineOfferItem = null)
        {
            List<OfferitemPricing> offerItemPricings = new();
            var offerPricings = offerItem.RetailItems.First().RetailItemMetaData.FareInformation.First().FarePrice;
            var offerPricingsList = offerPricings.ToList();

            for (int i = 0; i < offerPricingsList.Count; i++)
            {
                var pricing = offerPricingsList[i];
                var baseAmt = pricing.BaseFare.CurrencyEquivalentPrice;
                var totalAmt = pricing.TotalFarePrice.CurrencyEquivalentPrice;
                var totalTaxAmt = pricing.TotalTaxAndFees.CurrencyEquivalentPrice;
                var totalPoints = pricing.TotalFarePrice.MilesEquivalentPrice;

                var salePrices = new List<Price>()
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
                            PriceComponentType = Price.PriceComponentTypes.TotalTaxEnum
                        }
                    };

                List<Price> salePriceDifference = null;
                if (isAwardSearch)
                {
                    var currentPoints = totalPoints?.MileCnt ?? 0;
                    salePrices.Add(new()
                    {
                        PointsAmount = new()
                        {
                            PointsCount = currentPoints
                        },
                        PriceComponentType = Price.PriceComponentTypes.PointsEnum
                    });

                    var baselinePoints = baselineOfferItem?.RetailItems?.FirstOrDefault()?.RetailItemMetaData?.FareInformation?.FirstOrDefault()?.FarePrice?.ElementAtOrDefault(i)?.TotalFarePrice?.MilesEquivalentPrice?.MileCnt ?? 0;

                    if (baselineOfferItem != null && baselinePoints != 0 && currentPoints != 0)
                    {
                        var pointsDifference = currentPoints - baselinePoints;
                        if (pointsDifference != 0)
                        {
                            salePriceDifference = new List<Price>
                            {
                                new()
                                {
                                    PointsAmount = new()
                                    {
                                        PointsCount = pointsDifference >= 0 ? pointsDifference : -pointsDifference
                                    },
                                    PriceComponentType = Price.PriceComponentTypes.PointsEnum,
                                    DifferenceDetails = new PointsDifferenceDetails
                                    {
                                        PointsDifference = pointsDifference >= 0 ? pointsDifference : -pointsDifference,
                                        PointsDifferenceSign = pointsDifference < 0 ? "-" : "+"
                                    }
                                }
                            };
                        }
                    }
                }

                offerItemPricings.Add(new OfferitemPricing() { SalePrice = salePrices, SalePriceDifference = salePriceDifference });
            }

            return offerItemPricings;
        }

        private static IEnumerable<OfferService> GetOfferServices(Infrastructure.Proxy.DeltaOffer.OfferItem offerItem, OfferDataList offerDataList, bool isAwardSearch, Infrastructure.Proxy.DeltaOffer.OfferItem baselineOfferItem)
        {
            return new OfferService[]
            {
                new()
                {
                    ServiceBeneficiaries = offerItem.PassengerIds,
                    JourneyRefIds = offerItem.TripIds.Select(t => $"{Constants.Application.Journey}{t}"),
                    FlightSegmentsRefIds = offerItem.FlightSegmentIds.Select(fs => $"{Constants.Application.FlightSegment}{fs}"),
                    FareDetails = GetFareDetails(offerItem, offerDataList, isAwardSearch, baselineOfferItem)
                }
            };
        }

        private static string GetDuration(Duration duration)
        {
            return $"{duration.HourCnt:d2}:{duration.MinuteCnt:d2}";
        }
    }
}
