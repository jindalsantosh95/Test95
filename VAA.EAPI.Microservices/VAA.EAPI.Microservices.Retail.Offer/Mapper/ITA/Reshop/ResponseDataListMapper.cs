using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Virgin.Models.Common;
using VAA.EAPI.Domain.Virgin.Models.Products.Baggage;
using VAA.EAPI.Domain.Virgin.Models.Products.Flight;
using VAA.EAPI.Microservices.Retail.Offer.Domain;
using VAA.EAPI.Microservices.Retail.Offer.Domain.Reshop;
using VAA.EAPI.Microservices.Retail.Offer.Infrastructure.Helper.Interface;
using VAA.EAPI.Microservices.Retail.Offer.Infrastructure.Proxy;
using VAA.EAPI.Microservices.Retail.Offer.Mapper.Ita.Common;
using PriceClass = VAA.EAPI.Domain.Virgin.Models.Common.PriceClass;

namespace VAA.EAPI.Microservices.Retail.Offer.Mapper.Ita.Reshop
{
    public class ResponseDataListMapper
    {
        public static async Task<string> AddOfferOriginDestination(resultBundledSolutionSummarySolutionPricing pricing, resultBundledSolutionSummarySolutionItinerarySlice slice, DataList dataList, List<string> offerSegmentIds, ICachedCabin cachedCabin, ICachedAirportDetails cachedAirportDetails, OfferRequest offerRequest)
        {
            var originDestinationKey = (slice.origin, slice.destination, slice.departure.date);
            if (!dataList.OriginDestinationsByKey.TryGetValue(originDestinationKey, out var originDestination))
            {
                var originDestRefID = $"{Constants.Application.OriginDestination}{dataList.OriginDestinationAssociations.Count + 1}";
                originDestination = new()
                {
                    OdLabel = originDestRefID,
                    OriginDestination = new()
                    {
                        OriginAirport = slice.origin,
                        DestinationAirport = slice.destination,
                        DepartureDate = slice.departure.date
                    },
                    JourneyRefIds = []
                };
                dataList.OriginDestinationAssociations.Add(originDestination);
                dataList.OriginDestinationsByKey.Add(originDestinationKey, originDestination);
            }

            string journeyRefId = await AddJourneyAndSegmentDetails(pricing, slice, dataList, originDestination.OdLabel, offerSegmentIds, cachedCabin);

            if (!originDestination.JourneyRefIds.Contains(journeyRefId))
            {
                originDestination.JourneyRefIds.Add(journeyRefId);
            }

            return journeyRefId;
        }

        public static async Task<string> AddJourneyAndSegmentDetails(resultBundledSolutionSummarySolutionPricing pricing, resultBundledSolutionSummarySolutionItinerarySlice slice, DataList dataList, string originDestRefID, List<string> offerSegmentIds, ICachedCabin cachedCabin)
        {
            var flightSegmentLst = await AddSegmentDetails(pricing, slice, originDestRefID, dataList, offerSegmentIds, cachedCabin);
            string duration = TimeSpan.FromMinutes(slice.duration).ToString(@"hh\:mm");
            var journeyKey = $"{duration}\u001F{string.Join('\u001F', flightSegmentLst)}";
            if (!dataList.JourneysByKey.TryGetValue(journeyKey, out var journey))
            {
                journey = new()
                {
                    Duration = duration,
                    JourneyId = $"{Constants.Application.Journey}{dataList.Journey.Count + 1}",
                    FlightSegmentRefIds = flightSegmentLst
                };
                dataList.Journey.Add(journey);
                dataList.JourneysByKey.Add(journeyKey, journey);
            }
            return journey.JourneyId;
        }

        private static async Task<List<string>> AddSegmentDetails(resultBundledSolutionSummarySolutionPricing pricing, resultBundledSolutionSummarySolutionItinerarySlice slice, string OriginDestRefID, DataList dataList, List<string> offerSegRefIds, ICachedCabin cachedCabin)
        {
            List<string> flightSegRefIds = new();
            string flightSegRefId = string.Empty;

            foreach (var seg in slice.segment)
            {
                var flightNumber = Convert.ToString(seg.flight.number);
                var segmentKey = (OriginDestRefID, seg.origin, seg.destination, flightNumber);
                if (!dataList.FlightSegmentsByKey.TryGetValue(segmentKey, out var flightSegment))
                {
                    flightSegRefId = $"{Constants.Application.FlightSegment}{dataList.FlightSegments.Count + 1}";
                    flightSegment = new()
                    {
                        SegmentId = flightSegRefId,
                        OdRefId = OriginDestRefID,
                        AirCraft = seg.aircraft != null && seg.aircraft.Any() ? seg.aircraft.FirstOrDefault() : null,
                        ScheduledArrivalTime = new()
                        {
                            LocalDateTime = $"{seg.arrival.date}{Constants.Application.Timespan}{seg.arrival.time}"
                        },
                        ScheduledDepartureTime = new()
                        {
                            LocalDateTime = $"{seg.departure.date}{Constants.Application.Timespan}{seg.departure.time}"
                        },
                        DestinationAirportCode = seg.destination,
                        MarketingCarrier = seg.flight.carrier,
                        MarketingFlightNumber = flightNumber,
                        OriginAirportCode = seg.origin,
                        FlightDuration = TimeSpan.FromMinutes(seg.duration).ToString(@"hh\:mm"),
                        OperatingCarrier = seg.flight.carrier != Constants.Application.VsFlightCode && string.IsNullOrEmpty(seg.operationalFlight?.carrier) ? seg.flight.carrier : seg.operationalFlight?.carrier,
                        OperatingFlightNumber = seg.flight.carrier != Constants.Application.VsFlightCode && Convert.ToString(seg.operationalFlight?.number) == "" ? Convert.ToString(seg.flight.number) : Convert.ToString(seg.operationalFlight?.number) == "" ? null : Convert.ToString(seg.operationalFlight?.number),
                        Legs = seg.leg != null ? GetLegList(seg.leg.ToList(), seg?.flight?.carrier) : null
                    };
                    dataList.FlightSegments.Add(flightSegment);
                    dataList.FlightSegmentsByKey.Add(segmentKey, flightSegment);
                }
                else
                {
                    flightSegRefId = flightSegment.SegmentId;
                }
                var segmentPricing = pricing.fareRestriction.FirstOrDefault(a =>a.segment.origin == seg.origin && a.segment.destination == seg.destination); 
                string offerSegmentId = await AddOfferSegments(segmentPricing, seg, flightSegRefId, offerSegRefIds, cachedCabin, dataList);

                flightSegRefIds.Add(flightSegRefId);
            }
            return flightSegRefIds.Count == 0 ? null : flightSegRefIds;
        }

        private static List<EAPI.Domain.Virgin.Models.Products.Flight.Legs> GetLegList(List<resultBundledSolutionSummarySolutionItinerarySliceSegmentLeg> legList, string operatingCarrier)
        {
            return legList.Select((leg, index) => new EAPI.Domain.Virgin.Models.Products.Flight.Legs()
            {
                ScheduledDepartureTime = new()
                {
                    LocalDateTime = $"{leg.departure.date}{Constants.Application.Timespan}{leg.departure.time}"
                },
                LegSequenceNumber = $"{index++}",
                OriginAirportCode = leg.origin,
                OperatingCarrier = operatingCarrier,
                ScheduledArrivalTime = new()
                {
                    LocalDateTime = $"{leg.arrival.date}{Constants.Application.Timespan}{leg.arrival.time}"
                },
                DestinationAirportCode = leg.destination,
                LegDuration = Convert.ToString(leg.duration),
                StopOverDuration = Convert.ToString(leg?.connection?.duration)
            }).ToList();
        }

        private static async Task<string> AddOfferSegments(resultBundledSolutionSummarySolutionPricingFareRestrictionCouponValidityDateInfo couponValidity, resultBundledSolutionSummarySolutionItinerarySliceSegment segment, string flightSegRefId, List<string> offerSegRefIds, ICachedCabin cachedCabin, DataList dataList)
        {
            string offerSegRefId = null;
            CabinTableEntity cabinData = await cachedCabin.GetCachedCabinData(segment.cabin);
            var offerSegmentKey = (segment.bookingCode, flightSegRefId, cabinData.CabinCode);
            if (!dataList.OfferSegmentsByKey.TryGetValue(offerSegmentKey, out var offerSegment))
            {
                offerSegRefId = Constants.Application.OfferSegment + (dataList.OfferSegments.Count + 1);
                offerSegment = new OfferSegment
                {
                    OfferSegmentId = offerSegRefId,
                    FlightSegmentRefId = flightSegRefId,
                    ClassOfService = segment.bookingCode,
                    CabinType = new()
                    {
                        CabinCode = cabinData.CabinCode,
                        CabinName = cabinData.CabinName
                    },
                    TicketNotValidAfter = couponValidity.notValidAfter,
                    TicketNotValidBefore = couponValidity.notValidBefore
                };
                dataList.OfferSegments.Add(offerSegment);
                dataList.OfferSegmentsByKey.Add(offerSegmentKey, offerSegment);
            }
            else
            {
                offerSegRefId = offerSegment.OfferSegmentId;
            }
            offerSegRefIds.Add(offerSegRefId);
            return offerSegRefId;
        }

        public static async Task<List<string>> AddpriceClassDetails(resultBundledSolutionSummarySolutionPricing pricing, resultBundledSolutionSummarySolutionItinerarySlice slice, List<PriceClass> reshopPriceClass, ICachedPriceClass cachedPriceClass)
        {
            List<string> priceClassRefIds = new();
            string pcRefId = string.Empty;
            foreach (var segment in slice.segment)
            {
                foreach (var bookingInfo in pricing.bookingInfo)
                {
                    if (segment.origin == bookingInfo.segment.origin && segment.destination == bookingInfo.segment.destination && segment.flight.number == bookingInfo.segment.flight.number && segment.departure.date == bookingInfo.segment.departure.date)
                    {
                        PriceClassTableEntity priceClassData = await cachedPriceClass.GetCachedPriceClassData(bookingInfo.fare.record1FareType);

                        if (!reshopPriceClass.Any(s => s.FareBasisCode == bookingInfo.fare.extendedFareCode && s.FareTypeCode == bookingInfo.fare.record1FareType))
                        {
                            int lastPcId = reshopPriceClass.Any() ? Convert.ToInt32(reshopPriceClass.Last().Id[2..]) : 0;
                            pcRefId = $"{Constants.Application.PriceClass}{++lastPcId}";
                            reshopPriceClass.Add(new PriceClass
                            {
                                Id = pcRefId,
                                FareBasisCode = bookingInfo.fare.extendedFareCode,
                                FareTypeCode = priceClassData.RowKey,
                                ProductName = priceClassData.ProductName,
                                CabinType = new() { CabinCode = priceClassData.CabinCode, CabinName = priceClassData.CabinName }
                            });
                        }
                        else
                        {
                            pcRefId = reshopPriceClass.FirstOrDefault(s => s.FareBasisCode == bookingInfo.fare.extendedFareCode).Id;
                        }
                        if (!priceClassRefIds.Any(x => x == reshopPriceClass.FirstOrDefault(s => s.FareBasisCode == bookingInfo.fare.extendedFareCode && s.FareTypeCode == bookingInfo.fare.record1FareType)?.Id))
                            priceClassRefIds.Add(pcRefId);
                    }
                }
            }
            return priceClassRefIds.Count == 0 ? null : priceClassRefIds;
        }

        public static async Task<List<string>> AddpriceClassDetails(resultBundledSolutionSummarySolutionPricing pricing, resultBundledSolutionSummarySolutionItinerarySlice slice, DataList dataList, ICachedPriceClass cachedPriceClass)
        {
            List<string> priceClassRefIds = new();
            foreach (var segment in slice.segment)
            {
                foreach (var bookingInfo in pricing.bookingInfo)
                {
                    if (segment.origin != bookingInfo.segment.origin || segment.destination != bookingInfo.segment.destination || segment.flight.number != bookingInfo.segment.flight.number || segment.departure.date != bookingInfo.segment.departure.date)
                    {
                        continue;
                    }

                    var priceClassKey = (bookingInfo.fare.extendedFareCode, bookingInfo.fare.record1FareType);
                    if (!dataList.PriceClassesByKey.TryGetValue(priceClassKey, out var priceClass))
                    {
                        PriceClassTableEntity priceClassData = await cachedPriceClass.GetCachedPriceClassData(bookingInfo.fare.record1FareType);
                        priceClass = new PriceClass
                        {
                            Id = $"{Constants.Application.PriceClass}{dataList.PriceClass.Count + 1}",
                            FareBasisCode = bookingInfo.fare.extendedFareCode,
                            FareTypeCode = priceClassData.RowKey,
                            ProductName = priceClassData.ProductName,
                            CabinType = new() { CabinCode = priceClassData.CabinCode, CabinName = priceClassData.CabinName }
                        };
                        dataList.PriceClass.Add(priceClass);
                        dataList.PriceClassesByKey.Add(priceClassKey, priceClass);
                    }

                    if (!priceClassRefIds.Contains(priceClass.Id))
                    {
                        priceClassRefIds.Add(priceClass.Id);
                    }
                }
            }
            return priceClassRefIds.Count == 0 ? null : priceClassRefIds;
        }

        public static async Task<string> AddBaggageAllowance(resultBundledSolutionSummarySolutionPricingCheckedBaggageAllowance checkedBaggageAllowances, resultBundledSolutionSummarySolutionPricingCarryOnBaggageAllowance carryOnBaggageAllowances, List<BaggageAllowances> baggageDataList, bool point2pointVS, bool allCodeShare, ICachedPieceAllowance pieceAllowance, ICachedCabinBaggage cabinBaggage)
        {
            if (checkedBaggageAllowances is { freeBaggageAllowance: { } } && checkedBaggageAllowances.freeBaggageAllowance.First().bagDescriptor != null)
            {
                BaggageAllowances baggageAllowance = default;
                var bagDescriptor = checkedBaggageAllowances.freeBaggageAllowance.First().bagDescriptor;
                var piecesCount = checkedBaggageAllowances.freeBaggageAllowance.First().pieces;
                var baggageExists = baggageDataList.Any(bag => bag.BaggageAllowanceTypeCode == Constants.Application.CheckBaggageTypeCode && bag.FreeBaggageAllowance.BagDescriptors.Count == bagDescriptor.Length && bag.FreeBaggageAllowance.AllowedBagCnt == piecesCount && bag.FreeBaggageAllowance.BagDescriptors.Any(x => x.BagDescriptorName == bagDescriptor.First().commercialName));

                if (!baggageExists)
                {
                    int lastBaggageId = baggageDataList.Any() ? Convert.ToInt32(baggageDataList.Last().BaggageAllowanceId[3..]) : 0;
                    string baggageRefId = $"{Constants.Application.Baggage}{++lastBaggageId}";

                    baggageAllowance = new()
                    {
                        BaggageAllowanceId = baggageRefId,
                        BaggageAllowanceTypeCode = Constants.Application.CheckBaggageTypeCode,
                        FreeBaggageAllowance = new()
                        {
                            AllowedBagCnt = piecesCount,
                            BagDescriptors = GetCheckedBagDescriptors(bagDescriptor),
                            DimensionAllowance = await BaggageAllowanceMapper.GetPieceDimensionAllowance(pieceAllowance)
                        }
                    };

                    baggageDataList.Add(baggageAllowance);
                }
                else
                    baggageAllowance = baggageDataList.FirstOrDefault(bag => bag.BaggageAllowanceTypeCode == Constants.Application.CheckBaggageTypeCode && bag.FreeBaggageAllowance.AllowedBagCnt == piecesCount && bag.FreeBaggageAllowance.BagDescriptors.Any(x => x.BagDescriptorName == bagDescriptor.First().commercialName));

                if (baggageAllowance?.FreeBaggageAllowance.WeightAllowance is null)
                    baggageAllowance!.FreeBaggageAllowance.WeightAllowance = BaggageAllowanceMapper.GetWeightAllowance(baggageAllowance);

                return baggageAllowance!.BaggageAllowanceId;
            }

            if (carryOnBaggageAllowances is { freeBaggageAllowance: { bagDescriptor: { } } } && carryOnBaggageAllowances.freeBaggageAllowance?.bagDescriptor.Any(x => x.description != null) == true && carryOnBaggageAllowances.freeBaggageAllowance?.pieces != 0)
            {
                BaggageAllowances baggageAllowance = default;
                var bagDescriptor = carryOnBaggageAllowances.freeBaggageAllowance.bagDescriptor.SelectMany(x => x.description ?? Enumerable.Empty<string>());
                var piecesCount = carryOnBaggageAllowances.freeBaggageAllowance.pieces;
                var safeBagDescriptor = bagDescriptor ?? Enumerable.Empty<string>();

                var baggageExists = baggageDataList.Any(bag =>
                {
                    var descriptors = bag.FreeBaggageAllowance?.BagDescriptors?.SelectMany(x => x.BagDescriptorDesc ?? Enumerable.Empty<string>()).ToList() ?? new List<string>();
                    return bag.BaggageAllowanceTypeCode == Constants.Application.CarryOnBaggageTypeCode && bag.FreeBaggageAllowance?.AllowedBagCnt == piecesCount && descriptors.All(desc => safeBagDescriptor.Contains(desc)) && safeBagDescriptor.Count() == descriptors.Count();
              
                });

                if (!baggageExists)
                {
                    int lastBaggageId = baggageDataList.Any() ? Convert.ToInt32(baggageDataList.Last().BaggageAllowanceId[3..]) : 0;
                    string baggageRefId = $"{Constants.Application.Baggage}{++lastBaggageId}";
                    baggageAllowance = new()
                    {
                        BaggageAllowanceId = baggageRefId,
                        BaggageAllowanceTypeCode = Constants.Application.CarryOnBaggageTypeCode,
                        FreeBaggageAllowance = new()
                        {
                            AllowedBagCnt = carryOnBaggageAllowances.freeBaggageAllowance.pieces,
                            BagDescriptors = GetCarryOnBagDescriptors(carryOnBaggageAllowances.freeBaggageAllowance.bagDescriptor),
                        }
                    };
                    baggageAllowance.FreeBaggageAllowance.WeightAllowance = BaggageAllowanceMapper.GetWeightAllowance(baggageAllowance);
                    baggageDataList.Add(baggageAllowance);

                    if (point2pointVS)
                    {
                        var cabinBaggageAllowance = await cabinBaggage.GetCachedCabinBaggageInformation("CabinBaggage");
                        var weightAllowance = BaggageAllowanceMapper.GetWeightAllowance(baggageAllowance);

                        baggageAllowance.FreeBaggageAllowance.DimensionAllowance = BaggageAllowanceMapper.GetPieceDimensionAllowanceCarryOn(baggageAllowance, cabinBaggageAllowance, weightAllowance);
                        baggageAllowance.FreeBaggageAllowance.WeightAllowance = weightAllowance;
                    }

                    if (!point2pointVS && baggageAllowance.FreeBaggageAllowance.DimensionAllowance == null)
                    {
                        var baggageDescriptionText = Constants.Application.CarryOnBaggageText;
                        baggageAllowance.FreeBaggageAllowance.BagDescriptors.First().BagDescriptorName = baggageDescriptionText;
                    }
                }
                else

                    baggageAllowance = baggageDataList.FirstOrDefault(bag =>
                    {
                        var allDescriptors = bag?.FreeBaggageAllowance?.BagDescriptors?.SelectMany(x => x?.BagDescriptorDesc ?? Enumerable.Empty<string>()).ToList() ?? new List<string>();

                        return bag?.BaggageAllowanceTypeCode == Constants.Application.CarryOnBaggageTypeCode && bag?.FreeBaggageAllowance?.AllowedBagCnt == piecesCount && allDescriptors.All(desc => safeBagDescriptor.Contains(desc)) && safeBagDescriptor.Count() == allDescriptors.Count;

                    });

                return baggageAllowance.BaggageAllowanceId;
            }

            if (carryOnBaggageAllowances is { freeBaggageAllowance: { } } && (carryOnBaggageAllowances.freeBaggageAllowance.bagDescriptor == null || carryOnBaggageAllowances.freeBaggageAllowance?.bagDescriptor.Any(x => x.description == null) == true) && carryOnBaggageAllowances.freeBaggageAllowance.pieces != 0)
            {
                BaggageAllowances baggageAllowance = default;
                var bagDescriptorName = Constants.Application.CarryOnBaggageText;
                var piecesCount = carryOnBaggageAllowances.freeBaggageAllowance.pieces;
                var kilosCount = carryOnBaggageAllowances.freeBaggageAllowance.kilos;
                var baggageExists = baggageDataList.Any(bag => bag.BaggageAllowanceTypeCode == Constants.Application.CarryOnBaggageTypeCode && bag.FreeBaggageAllowance.AllowedBagCnt == piecesCount && bag.FreeBaggageAllowance.BagDescriptors.First().BagDescriptorName.Equals(bagDescriptorName) && (kilosCount <= 0 || bag.FreeBaggageAllowance.WeightAllowance.Any(w => w.MaximumWeightMeasure == kilosCount.ToString())));

                if (!baggageExists)
                {
                    int lastBaggageId = baggageDataList.Any() ? Convert.ToInt32(baggageDataList.Last().BaggageAllowanceId[3..]) : 0;
                    string baggageRefId = $"{Constants.Application.Baggage}{++lastBaggageId}";
                    baggageAllowance = new()
                    {
                        BaggageAllowanceId = baggageRefId,
                        BaggageAllowanceTypeCode = Constants.Application.CarryOnBaggageTypeCode,
                        FreeBaggageAllowance = new()
                        {
                            AllowedBagCnt = carryOnBaggageAllowances.freeBaggageAllowance.pieces,
                            WeightInKilos = carryOnBaggageAllowances.freeBaggageAllowance.kilos
                        }
                    };
                    baggageAllowance.FreeBaggageAllowance.BagDescriptors = [new() { BagDescriptorName = bagDescriptorName}];
                    baggageAllowance.FreeBaggageAllowance.WeightAllowance = BaggageAllowanceMapper.GetWeightAllowance(baggageAllowance);
                    baggageDataList.Add(baggageAllowance);

                }
                else
                    baggageAllowance = baggageDataList.FirstOrDefault(bag => bag.BaggageAllowanceTypeCode == Constants.Application.CarryOnBaggageTypeCode && bag.FreeBaggageAllowance.AllowedBagCnt == piecesCount && bag.FreeBaggageAllowance.BagDescriptors.First().BagDescriptorName.Equals(Constants.Application.CarryOnBaggageText) && (kilosCount <= 0 || bag.FreeBaggageAllowance.WeightAllowance.Any(w => w.MaximumWeightMeasure == kilosCount.ToString())));

                return baggageAllowance.BaggageAllowanceId;
            }
            return null;
        }

        public static PenaltyRef AddPenaltyDetails(resultBundledSolutionSummarySolutionPricingFare[] solnPricingfares, List<Penalty> penaltyDataList, resultBundledSolutionSummarySolutionItinerarySlice slices)
        {
            PenaltyRef penaltyRefIds = new();
            foreach (var fares in solnPricingfares)
            {
                foreach (var bookingInfo in fares.bookingInfo)
                {
                    if (slices.segment.Any(item => item.origin == bookingInfo.segment.origin && item.destination == bookingInfo.segment.destination))
                    {
                        if (fares.ruleSummary is { penalties: { } })
                        {
                            if (fares.ruleSummary.penalties.changePenalty != null && fares.ruleSummary.penalties.changePenalty.Any())
                            {
                                foreach (var changePenalty in fares.ruleSummary.penalties.changePenalty)
                                {
                                    if (changePenalty.maxPrice != null && changePenalty.minPrice != null)
                                    {
                                        penaltyRefIds.ChangePenaltyRefIds ??= new();
                                        if (!penaltyDataList.Any(penalty => penalty.Type == Constants.Application.ChangePenalty && penalty.MaxPrice.Amount.MonetaryAmount == changePenalty.maxPrice[3..] && penalty.Text == changePenalty.application))
                                        {
                                            int lastPenaltyId = penaltyDataList.Any() ? Convert.ToInt32(penaltyDataList.Last().Id[3..]) : 0;
                                            string penaltyRefId = $"{Constants.Application.Penalty}{++lastPenaltyId}";
                                            penaltyDataList.Add(
                                            new Penalty
                                            {
                                                Id = penaltyRefId,
                                                Type = Constants.Application.ChangePenalty,
                                                MaxPrice = new MaxPrice
                                                {
                                                    Amount = GetPenaltyAmount(changePenalty.maxPrice)
                                                },
                                                MinPrice = new MinPrice
                                                {
                                                    Amount = GetPenaltyAmount(changePenalty.minPrice)
                                                },
                                                Text = changePenalty.application
                                            });
                                            penaltyRefIds.ChangePenaltyRefIds.Add(penaltyRefId);
                                        }
                                        else
                                        {
                                            if (!penaltyRefIds.ChangePenaltyRefIds.Contains(penaltyDataList.First(penalty => penalty.Type == Constants.Application.ChangePenalty && penalty.MaxPrice.Amount.MonetaryAmount == changePenalty.maxPrice[3..] && penalty.Text == changePenalty.application).Id))
                                                penaltyRefIds.ChangePenaltyRefIds.Add(penaltyDataList.First(penalty => penalty.Type == Constants.Application.ChangePenalty && penalty.MaxPrice.Amount.MonetaryAmount == changePenalty.maxPrice[3..] && penalty.Text == changePenalty.application).Id);
                                        }
                                    }
                                }
                            }
                            if (fares.ruleSummary.penalties.cancellationAndRefundPenalty != null && fares.ruleSummary.penalties.cancellationAndRefundPenalty.Count() > 0)
                            {
                                foreach (var cancelPenalty in fares.ruleSummary.penalties.cancellationAndRefundPenalty)
                                {
                                    if (cancelPenalty.maxPrice != null && cancelPenalty.minPrice != null)
                                    {
                                        penaltyRefIds.CancellationPenaltyRefIds ??= new();
                                        if (!penaltyDataList.Any(penalty => penalty.Type == Constants.Application.CancellationPenalty && penalty.MaxPrice.Amount.MonetaryAmount == cancelPenalty.maxPrice[3..] && penalty.Text == cancelPenalty.application))
                                        {
                                            int lastPenaltyId = penaltyDataList.Any() ? Convert.ToInt32(penaltyDataList.Last().Id[3..]) : 0;
                                            string penaltyRefId = $"{Constants.Application.Penalty}{++lastPenaltyId}";
                                            penaltyDataList.Add(
                                              new Penalty
                                              {
                                                  Id = penaltyRefId,
                                                  Type = Constants.Application.CancellationPenalty,
                                                  MaxPrice = new MaxPrice
                                                  {
                                                      Amount = GetPenaltyAmount(cancelPenalty.maxPrice)
                                                  },
                                                  MinPrice = new MinPrice
                                                  {
                                                      Amount = GetPenaltyAmount(cancelPenalty.minPrice)
                                                  },
                                                  Text = cancelPenalty.application
                                              });
                                            penaltyRefIds.CancellationPenaltyRefIds.Add(penaltyRefId);
                                        }
                                        else
                                        {
                                            if (!penaltyRefIds.CancellationPenaltyRefIds.Contains(penaltyDataList.First(penalty => penalty.Type == Constants.Application.CancellationPenalty && penalty.MaxPrice.Amount.MonetaryAmount == cancelPenalty.maxPrice[3..] && penalty.Text == cancelPenalty.application).Id))
                                                penaltyRefIds.CancellationPenaltyRefIds.Add(penaltyDataList.First(penalty => penalty.Type == Constants.Application.CancellationPenalty && penalty.MaxPrice.Amount.MonetaryAmount == cancelPenalty.maxPrice[3..] && penalty.Text == cancelPenalty.application).Id);
                                        }
                                    }
                                }
                            }

                        }
                    }
                }
            }
            return penaltyRefIds.CancellationPenaltyRefIds == null && penaltyRefIds.ChangePenaltyRefIds == null ? null : penaltyRefIds;
        }

        public static PenaltyRef AddReshopPenaltyDetails(resultBundledSolutionSummarySolutionPricing pricing, List<Penalty> penaltyDataList, resultBundledSolutionSummarySolutionItinerarySlice slices)
        {
            PenaltyRef penaltyRefIds = new();

            foreach (var penalty in pricing.penaltyPrice)
            {
                penaltyRefIds.ChangePenaltyRefIds ??= new();
                if (!penaltyDataList.Any(p => p.Type == Constants.Application.ChangePenalty && p.MaxPrice.Amount.MonetaryAmount == penalty.amount.ToString()))
                {
                    int lastPenaltyId = penaltyDataList.Any() ? Convert.ToInt32(penaltyDataList.Last().Id.Split("-")[0].Substring(3)) : 0;
                    string penaltyRefId = $"{Constants.Application.Penalty}{++lastPenaltyId}-{Constants.Application.PenaltyChange}";
                    penaltyDataList.Add(
                    new Penalty
                    {
                        Id = penaltyRefId,
                        Type = Constants.Application.ChangePenalty,
                        MaxPrice = new MaxPrice
                        {
                            Amount = new CurrencyAmount() { MonetaryAmount = penalty.amount.ToString(), BaseCurrency = penalty.currency }
                        },
                        Text = "Change Penalty"
                    });
                    penaltyRefIds.ChangePenaltyRefIds.Add(penaltyRefId);
                }
                else
                {
                    if (penaltyDataList.Any(p => p.Type == Constants.Application.ChangePenalty && p.MaxPrice.Amount.MonetaryAmount == penalty.amount.ToString()))
                        penaltyRefIds.ChangePenaltyRefIds.Add(penaltyDataList.First(p => p.Type == Constants.Application.ChangePenalty && p.MaxPrice.Amount.MonetaryAmount == penalty.amount.ToString()).Id);
                }

            }

            return penaltyRefIds.ChangePenaltyRefIds == null ? null : penaltyRefIds;
        }

        private static CurrencyAmount GetPenaltyAmount(string price)
        {
            return new CurrencyAmount
            {
                BaseCurrency = price[..3],
                MonetaryAmount = price[3..]
            };
        }

        private static List<BagDescriptor> GetCheckedBagDescriptors(IEnumerable<resultBundledSolutionSummarySolutionPricingCheckedBaggageAllowanceFreeBaggageAllowanceBagDescriptor> checkedFreeBagDescriptors)
        {
            return checkedFreeBagDescriptors.Any() ? checkedFreeBagDescriptors.Select(checkedFreeBagDescriptor => new BagDescriptor
            {
                BagCnt = checkedFreeBagDescriptor.count,
                BagDescriptorDesc = checkedFreeBagDescriptor.description?.ToList(),
                BagDescriptorName = checkedFreeBagDescriptor.commercialName,
                BagDescriptorSubCode = checkedFreeBagDescriptor.subcode
            }).ToList() : [];
        }

        private static List<BagDescriptor> GetCarryOnBagDescriptors(IEnumerable<resultBundledSolutionSummarySolutionPricingCarryOnBaggageAllowanceFreeBaggageAllowanceBagDescriptor> carryOnFreeBagDescriptors)
        {
            return carryOnFreeBagDescriptors.Any() ? carryOnFreeBagDescriptors.Select(carryOnFreeBagDescriptor => new BagDescriptor
            {
                BagCnt = carryOnFreeBagDescriptor.count,
                BagDescriptorDesc = carryOnFreeBagDescriptor.description?.ToList(),
                BagDescriptorName = carryOnFreeBagDescriptor.commercialName,
                BagDescriptorSubCode = carryOnFreeBagDescriptor.subcode
            }).ToList() : [];
        }
    }
}
