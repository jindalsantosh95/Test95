using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Domain.Virgin.Models.Common;
using VAA.EAPI.Domain.Virgin.Models.Products.Baggage;
using VAA.EAPI.Domain.Virgin.Models.Products.Flight;
using VAA.EAPI.Microservices.Retail.Offer.Domain;
using VAA.EAPI.Microservices.Retail.Offer.Domain.Reshop;
using VAA.EAPI.Microservices.Retail.Offer.Infrastructure.Helper.Interface;
using VAA.EAPI.Microservices.Retail.Offer.Infrastructure.Proxy;
using VAA.EAPI.Microservices.Retail.Offer.Mapper.Ita.Common;
using PriceClass = VAA.EAPI.Domain.Virgin.Models.Common.PriceClass;

namespace VAA.EAPI.Microservices.Retail.Offer.Mapper.Ita.Reprice
{
    public class RepriceResponseDataListMapper
    {
        public static async Task<string> AddOriginDestination(resultTripDetailsSolutionPricing pricing, resultTripDetailsSolutionItinerarySlice slice, DataList dataList, List<string> offerSegmentIds, ICachedCabin cachedCabin)
        {

            int lastOriginDestRefID = dataList.OriginDestinationAssociations.Any() ? Convert.ToInt32(dataList.OriginDestinationAssociations.Last().OdLabel[2..]) : 0;
            string originDestRefID = $"{Constants.Application.OriginDestination}{++lastOriginDestRefID}";
            if (!dataList.OriginDestinationAssociations.Any(Od => Od.OriginDestination.OriginAirport == slice.origin && Od.OriginDestination.DestinationAirport == slice.destination))
            {
                dataList.OriginDestinationAssociations.Add(new()
                {
                    OdLabel = originDestRefID,
                    OriginDestination = new()
                    {
                        OriginAirport = slice.origin,
                        DestinationAirport = slice.destination,
                        DepartureDate = slice.departure.date
                    },
                    JourneyRefIds = new()
                });
            }
            else
            {
                originDestRefID = dataList.OriginDestinationAssociations.FirstOrDefault(Od => Od.OriginDestination.OriginAirport == slice.origin && Od.OriginDestination.DestinationAirport == slice.destination).OdLabel;
            }
            string journeyRefId = await AddJourneyAndSegmentDetails(pricing, slice, dataList, originDestRefID, offerSegmentIds, cachedCabin);
            if (!dataList.OriginDestinationAssociations.Any(od => od.JourneyRefIds != null && od.JourneyRefIds.Contains(journeyRefId)))
            {
                dataList.OriginDestinationAssociations.FirstOrDefault(od => od.OdLabel == originDestRefID).JourneyRefIds.Add(journeyRefId);
            }
            return journeyRefId;
        }

        private static async Task<string> AddJourneyAndSegmentDetails(resultTripDetailsSolutionPricing pricing, resultTripDetailsSolutionItinerarySlice slice, DataList dataList, string originDestRefID, List<string> offerSegmentIds, ICachedCabin cachedCabin)
        {
            int lastJourneyId = dataList.Journey.Any() ? Convert.ToInt32(dataList.Journey.Last().JourneyId[4..]) : 0;
            string journeyId = $"{Constants.Application.Journey}{++lastJourneyId}";
            var flightSegmentLst = await AddSegmentDetails(pricing, slice, originDestRefID, dataList, offerSegmentIds, cachedCabin);
            string duration = TimeSpan.FromMinutes(slice.duration).ToString(@"hh\:mm");
            if (!dataList.Journey.Any(x => x.Duration == duration && x.FlightSegmentRefIds.SequenceEqual(flightSegmentLst)))
            {
                dataList.Journey.Add(new()
                {
                    Duration = duration,
                    JourneyId = journeyId,
                    FlightSegmentRefIds = flightSegmentLst
                });
            }
            else
            {
                journeyId = dataList.Journey.FirstOrDefault(x => x.Duration == duration && x.FlightSegmentRefIds.SequenceEqual(flightSegmentLst)).JourneyId;
            }
            return journeyId;
        }

        private static async Task<List<string>> AddSegmentDetails(resultTripDetailsSolutionPricing pricing, resultTripDetailsSolutionItinerarySlice slice, string OriginDestRefID, DataList dataList, List<string> offerSegRefIds, ICachedCabin cachedCabin)
        {
            List<string> flightSegRefIds = new();
            string flightSegRefId = string.Empty;

            foreach (var seg in slice.segment)
            {
                if (!dataList.FlightSegments.Any(s => s.OdRefId == OriginDestRefID && s.OriginAirportCode == seg.origin && s.DestinationAirportCode == seg.destination && s.MarketingFlightNumber == Convert.ToString(seg.flight.number)))
                {
                    int lastSegId = dataList.FlightSegments.Any() ? Convert.ToInt32(dataList.FlightSegments.Last().SegmentId[4..]) : 0;
                    flightSegRefId = $"{Constants.Application.FlightSegment}{++lastSegId}";
                    dataList.FlightSegments.Add(new()
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
                        MarketingFlightNumber = Convert.ToString(seg.flight.number),
                        OriginAirportCode = seg.origin,
                        FlightDuration = TimeSpan.FromMinutes(seg.duration).ToString(@"hh\:mm"),
                        OperatingCarrier = seg.flight.carrier != Constants.Application.VsFlightCode && string.IsNullOrEmpty(seg.operationalFlight?.carrier) ? seg.flight.carrier : seg.operationalFlight?.carrier,
                        OperatingFlightNumber = seg.flight.carrier != Constants.Application.VsFlightCode && Convert.ToString(seg.operationalFlight?.number) == "" ? Convert.ToString(seg.flight.number) : Convert.ToString(seg.operationalFlight?.number) == "" ? null : Convert.ToString(seg.operationalFlight?.number),
                        ConnectionOrStopOver = seg.connection is null ? Constants.Application.StopOver : Constants.Application.ConnectionStop,
                        Legs = seg.leg != null ? GetLegList(seg.leg, seg?.flight?.carrier) : null
                    });
                }
                else
                {
                    flightSegRefId = dataList.FlightSegments.FirstOrDefault(s => s.OdRefId == OriginDestRefID && s.OriginAirportCode == seg.origin && s.DestinationAirportCode == seg.destination && s.MarketingFlightNumber == Convert.ToString(seg.flight.number)).SegmentId;
                }

                var segmentPricing = pricing.fareRestriction.FirstOrDefault(a => a.segment.origin == seg.origin && a.segment.destination == seg.destination);
                await AddOfferSegments(segmentPricing, seg, flightSegRefId, offerSegRefIds, cachedCabin, dataList.OfferSegments);

                flightSegRefIds.Add(flightSegRefId);
            }

            return flightSegRefIds;
        }

        private static IEnumerable<EAPI.Domain.Virgin.Models.Products.Flight.Legs> GetLegList(IEnumerable<resultTripDetailsSolutionItinerarySliceSegmentLeg> legList, string operatingCarrier)
        {
            return legList.Select((leg, index) =>
             {
                 return new EAPI.Domain.Virgin.Models.Products.Flight.Legs()
                 {
                     ScheduledDepartureTime = new()
                     {
                         LocalDateTime = $"{leg.departure.date}{Constants.Application.Timespan}{leg.departure.time}"
                     },
                     LegSequenceNumber = (index + 1).ToString(),
                     OriginAirportCode = leg.origin,
                     OperatingCarrier = operatingCarrier,
                     ScheduledArrivalTime = new()
                     {
                         LocalDateTime = $"{leg.arrival.date}{Constants.Application.Timespan}{leg.arrival.time}"
                     },
                     DestinationAirportCode = leg.destination,
                     LegDuration = Convert.ToString(leg.duration),
                     StopOverDuration = Convert.ToString(leg.connection?.duration),
                     OriginTerminal = leg.originTerminal,
                     DestinationTerminal = leg.destinationTerminal
                 };
             });
        }

        private static async Task<string> AddOfferSegments(resultTripDetailsSolutionPricingFareRestrictionCouponValidityDateInfo couponValidity, resultTripDetailsSolutionItinerarySliceSegment segment, string flightSegRefId, List<string> offerSegRefIds, ICachedCabin cachedCabin, List<OfferSegment> offerSegmentsDatalist)
        {
            string offerSegRefId = null;
            CabinTableEntity cabinData = await cachedCabin.GetCachedCabinData(segment.cabin);
            if (!offerSegmentsDatalist.Any(s => s.ClassOfService == segment.bookingCode && s.FlightSegmentRefId == flightSegRefId && s.CabinType.CabinCode == cabinData.CabinCode))
            {
                int lastOfferSegId = offerSegmentsDatalist.Any() ? Convert.ToInt32(offerSegmentsDatalist.Last().OfferSegmentId[4..]) : 0;
                offerSegRefId = Constants.Application.OfferSegment + ++lastOfferSegId;
                offerSegmentsDatalist.Add(new OfferSegment
                {
                    OfferSegmentId = offerSegRefId,
                    FlightSegmentRefId = flightSegRefId,
                    ClassOfService = segment.bookingCode,
                    CabinType = new()
                    {
                        CabinCode = cabinData.CabinCode,
                        CabinName = cabinData.CabinName
                    },
                    ProductComponentRefIds = Enumerable.Empty<string>(),
                    TicketNotValidAfter = couponValidity.notValidAfter,
                    TicketNotValidBefore = couponValidity.notValidBefore
                });
            }
            else
            {
                offerSegRefId = offerSegmentsDatalist.FirstOrDefault(s => s.ClassOfService == segment.bookingCode && s.FlightSegmentRefId == flightSegRefId && s.CabinType.CabinCode == cabinData.CabinCode).OfferSegmentId;
            }
            offerSegRefIds.Add(offerSegRefId);
            return offerSegRefId;
        }

        public static async Task<List<string>> AddpriceClassDetails(resultTripDetailsSolutionPricing pricing, resultTripDetailsSolutionItinerarySlice slice, List<PriceClass> reshopPriceClass, ICachedPriceClass cachedPriceClass, ICachedCabin cachedCabin)
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
                                TicketDesigCode = bookingInfo.fare.ticketDesignator,
                                FareTypeCode = bookingInfo.fare.record1FareType,
                                ProductName = priceClassData.ProductName,
                                CabinType = new() { CabinCode = priceClassData.CabinCode, CabinName = priceClassData.CabinName }
                            });
                        }
                        else
                        {
                            pcRefId = reshopPriceClass.FirstOrDefault(s => s.FareBasisCode == bookingInfo.fare.extendedFareCode).Id;
                        }
                        if (!priceClassRefIds.Any(x => x == reshopPriceClass.FirstOrDefault(s => s.FareBasisCode == bookingInfo.fare.extendedFareCode && s.FareTypeCode == bookingInfo.fare.record1FareType).Id))
                            priceClassRefIds.Add(pcRefId);
                    }
                }
            }
            return priceClassRefIds.Count == 0 ? null : priceClassRefIds;
        }

        public static async Task<string> AddBaggageAllowance(resultTripDetailsSolutionPricingCheckedBaggageAllowance checkedBaggageAllowances, resultTripDetailsSolutionPricingCarryOnBaggageAllowance carryOnBaggageAllowances, List<BaggageAllowances> baggageDataList, bool point2pointVS, bool allCodeShare, ICachedPieceAllowance pieceAllowance, ICachedCabinBaggage cabinBaggage)
        {
            if (checkedBaggageAllowances is { freeBaggageAllowance: { } } && checkedBaggageAllowances.freeBaggageAllowance.First().bagDescriptor != null)
            {
                BaggageAllowances baggageAllowance = default;
                var piecesCount = checkedBaggageAllowances.freeBaggageAllowance.First().pieces;

                if (!baggageDataList.Any(bag => bag.BaggageAllowanceTypeCode == Constants.Application.CheckBaggageTypeCode && checkedBaggageAllowances.freeBaggageAllowance.First().pieces == piecesCount && bag.FreeBaggageAllowance.BagDescriptors.Count() == checkedBaggageAllowances.freeBaggageAllowance.First().bagDescriptor.Count() && bag.FreeBaggageAllowance.AllowedBagCnt == checkedBaggageAllowances.freeBaggageAllowance.First().pieces && bag.FreeBaggageAllowance.BagDescriptors.Any(x => x.BagDescriptorName == checkedBaggageAllowances.freeBaggageAllowance.First().bagDescriptor.First().commercialName)))
                {
                    int lastBaggageId = baggageDataList.Any() ? Convert.ToInt32(baggageDataList.Last().BaggageAllowanceId[3..]) : 0;
                    string baggageRefId = $"{Constants.Application.Baggage}{++lastBaggageId}";

                    baggageAllowance = new()
                    {
                        BaggageAllowanceId = baggageRefId,
                        BaggageAllowanceTypeCode = Constants.Application.CheckBaggageTypeCode,
                        FreeBaggageAllowance = new()
                        {
                            AllowedBagCnt = checkedBaggageAllowances.freeBaggageAllowance.First().pieces,
                            BagDescriptors = GetCheckedBagDescriptors(checkedBaggageAllowances.freeBaggageAllowance.FirstOrDefault().bagDescriptor),
                            DimensionAllowance = await BaggageAllowanceMapper.GetPieceDimensionAllowance(pieceAllowance)
                        }
                    };

                    baggageDataList.Add(baggageAllowance);
                }
                else
                    baggageAllowance = baggageDataList.FirstOrDefault(bag => bag.BaggageAllowanceTypeCode == Constants.Application.CheckBaggageTypeCode && bag.FreeBaggageAllowance.AllowedBagCnt == checkedBaggageAllowances.freeBaggageAllowance.FirstOrDefault().pieces && bag.FreeBaggageAllowance.BagDescriptors.Any(x => x.BagDescriptorName == checkedBaggageAllowances.freeBaggageAllowance.FirstOrDefault().bagDescriptor.FirstOrDefault().commercialName));

                if (point2pointVS && baggageAllowance?.FreeBaggageAllowance.WeightAllowance is null)
                    baggageAllowance.FreeBaggageAllowance.WeightAllowance = BaggageAllowanceMapper.GetWeightAllowance(baggageAllowance);

                return baggageAllowance?.BaggageAllowanceId;
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
                    baggageAllowance.FreeBaggageAllowance.BagDescriptors = [new() { BagDescriptorName = bagDescriptorName, BagDescriptorDesc = new() { bagDescriptorName } }];
                    baggageAllowance.FreeBaggageAllowance.WeightAllowance = BaggageAllowanceMapper.GetWeightAllowance(baggageAllowance);
                    baggageDataList.Add(baggageAllowance);

                }
                else
                    baggageAllowance = baggageDataList.FirstOrDefault(bag => bag.BaggageAllowanceTypeCode == Constants.Application.CarryOnBaggageTypeCode && bag.FreeBaggageAllowance.AllowedBagCnt == piecesCount && bag.FreeBaggageAllowance.BagDescriptors.First().BagDescriptorName.Equals(Constants.Application.CarryOnBaggageText) && (kilosCount <= 0 || bag.FreeBaggageAllowance.WeightAllowance.Any(w => w.MaximumWeightMeasure == kilosCount.ToString())));

                return baggageAllowance.BaggageAllowanceId;
            }
            return null;
        }

        public static PenaltyRef AddPenaltyDetails(resultTripDetailsSolutionPricingFare[] solnPricingfares, List<Penalty> penaltyDataList, resultTripDetailsSolutionItinerarySlice slices)
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
                            if (fares.ruleSummary.penalties.changePenalty is { Length: > 0 })
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
                            if (fares.ruleSummary.penalties.cancellationAndRefundPenalty is { Length: > 0 })
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
            return penaltyRefIds.ChangePenaltyRefIds == null && penaltyRefIds.CancellationPenaltyRefIds == null ? null : penaltyRefIds;
        }

        public static PenaltyRef AddQuotePenaltyDetails(resultTripDetailsSolutionPricing pricing, List<Penalty> penaltyDataList)
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

        private static List<BagDescriptor> GetCheckedBagDescriptors(IEnumerable<resultTripDetailsSolutionPricingCheckedBaggageAllowanceFreeBaggageAllowanceBagDescriptor> checkedFreeBagDescriptors)
        {
            List<BagDescriptor> bagDescriptorList = new();
            if (checkedFreeBagDescriptors.Any())
            {
                foreach (var checkedFreeBagDescriptor in checkedFreeBagDescriptors)
                {
                    bagDescriptorList.Add(new BagDescriptor
                    {
                        BagCnt = checkedFreeBagDescriptor.count,
                        BagDescriptorDesc = checkedFreeBagDescriptor.description.ToList(),
                        BagDescriptorName = checkedFreeBagDescriptor.commercialName,
                        BagDescriptorSubCode = checkedFreeBagDescriptor.subcode
                    });
                }
            }
            return bagDescriptorList;
        }

        private static List<BagDescriptor> GetCarryOnBagDescriptors(IEnumerable<resultTripDetailsSolutionPricingCarryOnBaggageAllowanceFreeBaggageAllowanceBagDescriptor> carryOnFreeBagDescriptors)
        {
            List<BagDescriptor> bagDescriptorList = new();
            if (carryOnFreeBagDescriptors.Any())
            {
                foreach (var carryOnFreeBagDescriptor in carryOnFreeBagDescriptors)
                {
                    bagDescriptorList.Add(new BagDescriptor
                    {
                        BagCnt = carryOnFreeBagDescriptor.count,
                        BagDescriptorDesc = carryOnFreeBagDescriptor?.description is not null ? carryOnFreeBagDescriptor?.description?.ToList() : null,
                        BagDescriptorName = carryOnFreeBagDescriptor?.commercialName,
                        BagDescriptorSubCode = carryOnFreeBagDescriptor?.subcode
                    });
                }
            }
            return bagDescriptorList;
        }

        private static async Task AddCodeShareAmenities(Segment flightSegmentDetails, OfferSegment offerSegmentDetails, string segmentId, List<PriceClass> priceClasses, ICachedAmenities cachedAmenities, DataList dataList, List<string> productComponentRefIds)
        {
            string productComponentRefId = null;
            string operatingcarrier = flightSegmentDetails.OperatingCarrier;
            if (!string.IsNullOrEmpty(operatingcarrier))
            {
                IEnumerable<AmenitiesTableEntity> amenitiesInfo = await cachedAmenities.GetAmenitiesInformation(Constants.Application.Codeshare);
                foreach (var item in priceClasses)
                {
                    if (operatingcarrier == Constants.Application.DeltaFlights)
                        amenitiesInfo = amenitiesInfo.Where(q => q.PriceClass.Contains(item.ProductName, StringComparison.OrdinalIgnoreCase) && q.OperatingCarrier.Is(operatingcarrier) && q.VAACabins.Is(offerSegmentDetails.CabinType.CabinName));
                    else
                        amenitiesInfo = amenitiesInfo.Where(q => q.PriceClass.Contains(item.ProductName, StringComparison.OrdinalIgnoreCase) && q.OperatingCarrier.Is(Constants.Application.OtherFlights) && q.VAACabins.Is(offerSegmentDetails.CabinType.CabinName));

                    if (amenitiesInfo != null)
                    {
                        foreach (var amenity in amenitiesInfo)
                        {
                            if (!dataList.ProductComponents.Any(x => x.Code == amenity.RowKey && x.Description == amenity.ProductComponentName))
                            {
                                int lastProductComponentId = dataList.ProductComponents.Any() ? Convert.ToInt32(dataList.ProductComponents.Last().Id[4..]) : 0;
                                productComponentRefId = $"{Constants.Application.ProductComponent}{++lastProductComponentId}";
                                dataList.ProductComponents.Add(new ProductComponent
                                {
                                    Id = productComponentRefId,
                                    Code = amenity.RowKey,
                                    Description = amenity.ProductComponentName
                                });

                                offerSegmentDetails.ProductComponentRefIds = offerSegmentDetails.ProductComponentRefIds.Append(productComponentRefId);
                            }
                            else
                            {
                                productComponentRefId = dataList.ProductComponents.First(x => x.Code == amenity.RowKey && x.Description == amenity.ProductComponentName).Id;
                                bool checkProductComponentRefId = offerSegmentDetails.ProductComponentRefIds.Contains(productComponentRefId);
                                if (!checkProductComponentRefId)
                                    offerSegmentDetails.ProductComponentRefIds = offerSegmentDetails.ProductComponentRefIds.Append(productComponentRefId);
                            }
                        }
                    }

                }
            }

        }
        private static List<BagDescriptor> GetCarryOnBagDescriptors(List<resultBundledSolutionSummarySolutionPricingCarryOnBaggageAllowanceFreeBaggageAllowanceBagDescriptor> carryOnFreeBagDescriptors)
        {
            List<BagDescriptor> bagDescriptorList = new();
            if (carryOnFreeBagDescriptors is { Count: > 0 })
            {
                carryOnFreeBagDescriptors.ForEach(carryOnFreeBagDescriptor =>
                {
                    bagDescriptorList.Add(new BagDescriptor
                    {
                        BagCnt = carryOnFreeBagDescriptor.count,
                        BagDescriptorDesc = carryOnFreeBagDescriptor.description?.ToList(),
                        BagDescriptorName = carryOnFreeBagDescriptor.commercialName,
                        BagDescriptorSubCode = carryOnFreeBagDescriptor.subcode
                    });
                });
            }
            return bagDescriptorList;
        }
    }
}

