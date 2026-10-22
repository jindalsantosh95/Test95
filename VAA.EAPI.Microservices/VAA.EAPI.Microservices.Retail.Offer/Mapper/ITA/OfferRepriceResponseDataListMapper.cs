using System;
using System.Collections.Generic;
using System.Linq;
using VAA.EAPI.Domain.Virgin.Models.Common;
using VAA.EAPI.Domain.Virgin.Models.Products.Flight;
using VAA.EAPI.Microservices.Retail.Offer.Infrastructure.Proxy;
using VAA.EAPI.Microservices.Retail.Offer.Domain;

namespace VAA.EAPI.Microservices.Retail.Offer.Mapper.Ita
{
    public class OfferRepriceResponseDataListMapper
    {
        public static void AddSegmentDetails(resultTripDetailsSolutionItinerarySliceSegment seg, string originDestRefID, List<Segment> repriceSegmentDts, List<Segment> segmentsDataList)
        {
            int lastSegId = segmentsDataList.Any() ? Convert.ToInt32(segmentsDataList.Last().SegmentId[3..]) : 0;
            string segRefId = $"{Constants.Application.Segment}{++lastSegId}";

            if (segmentsDataList.Any(s => s.OdRefId == originDestRefID && s.OriginAirportCode == seg.origin && s.DestinationAirportCode == seg.destination))
                segRefId = segmentsDataList.FirstOrDefault(s => s.OdRefId == originDestRefID && s.OriginAirportCode == seg.origin && s.DestinationAirportCode == seg.destination).SegmentId;

            var segment = new Segment()
            {
                SegmentId = segRefId,
                OdRefId = originDestRefID,
                AirCraft = seg.aircraft != null && seg.aircraft.Any() ? seg.aircraft.FirstOrDefault() : null,
                ScheduledArrivalTime = new()
                {
                    LocalDateTime = $"{seg.arrival.date}T{seg.arrival.time}"
                },
                ScheduledDepartureTime = new()
                {
                    LocalDateTime = $"{seg.departure.date}T{seg.departure.time}"
                },
                DestinationAirportCode = seg.destination,
                MarketingCarrier = seg.flight.carrier,
                MarketingFlightNumber = seg.flight.number.ToString(),
                OriginAirportCode = seg.origin,
                FlightDuration = TimeSpan.FromMinutes(seg.duration).ToString(@"hh\:mm"),
                OperatingCarrier = seg.operationalFlight?.carrier,
                OperatingFlightNumber = Convert.ToString(seg.operationalFlight?.number),
                ClassOfService = seg.bookingCode
            };
            if (!segmentsDataList.Any(s => s.OriginAirportCode == seg.origin && s.DestinationAirportCode == seg.destination))
            {
                segmentsDataList.Add(segment);
            }
            repriceSegmentDts.Add(segment);
        }

        public static string AddJourneyAndSegmentDetails(resultTripDetailsSolutionItinerarySlice slice, List<Segment> segmentLst, string originDestRefID, List<Journey> journeyDataList, List<Segment> segmentsDataList)
        {
            foreach (var seg in slice.segment)
            {
                AddSegmentDetails(seg, originDestRefID, segmentLst, segmentsDataList);
            }
            int lastJourneyId = journeyDataList.Any() ? Convert.ToInt32(journeyDataList.Last().JourneyId[4..]) : 0;
            string journeyId = $"{Constants.Application.Journey}{++lastJourneyId}";
            string duration = TimeSpan.FromMinutes(slice.duration).ToString(@"hh\:mm");
            if (!journeyDataList.Any(x => x.Duration == duration && x.FlightSegmentRefIds.SequenceEqual(segmentLst.Select(seg => seg.SegmentId))))
            {
                journeyDataList.Add(new Journey
                {
                    Duration = duration,
                    JourneyId = journeyId,
                    FlightSegmentRefIds = segmentLst.Select(seg => seg.SegmentId).ToList()
                });
            }
            else
            {
                journeyId = journeyDataList.FirstOrDefault(x => x.Duration == duration && x.FlightSegmentRefIds.SequenceEqual(segmentLst.Select(seg => seg.SegmentId))).JourneyId;
            }
            return journeyId;
        }

        public static string AddpriceClassDetails(Segment segment, resultTripDetailsSolutionPricing pricing, resultTripDetailsSolutionExt ext, List<PriceClass> reshopPriceClass, List<Cabins> cabins)
        {
            string priceClassRefId = string.Empty;
            foreach (var fare in pricing.fare)
            {
                if (fare.bookingInfo.Any(fareSegment => fareSegment.segment.origin == segment.OriginAirportCode && fareSegment.segment.destination == segment.DestinationAirportCode))
                {
                    var bookingInfo = fare.bookingInfo.Where(slice => slice.segment.origin == segment.OriginAirportCode && slice.segment.destination == segment.DestinationAirportCode).First();
                    if (!reshopPriceClass.Any(s => s.FareBasisCode == fare.code && s.ClassOfService == bookingInfo.bookingCode))
                    {
                        int lastPcId = reshopPriceClass.Any() ? Convert.ToInt32(reshopPriceClass.Last().Id[2..]) : 0;
                        string pcRefId = $"{Constants.Application.PriceClass}{++lastPcId}";
                        reshopPriceClass.Add(new PriceClass
                        {
                            Id = pcRefId,
                            ClassOfService = bookingInfo.bookingCode,
                            FareBasisCode = fare.code,
                            CabinType = new()
                            {
                                CabinCode = cabins.FirstOrDefault(c => c.ItaCabinName == ext.bestValueFunctionTag).CabinCode,
                                CabinName = cabins.FirstOrDefault(c => c.ItaCabinName == ext.bestValueFunctionTag).CabinName
                            }
                        });
                        priceClassRefId = pcRefId;
                    }
                    else
                    {
                        priceClassRefId = reshopPriceClass.FirstOrDefault(s => s.FareBasisCode == fare.code).Id;
                    }
                }
            }
            return priceClassRefId;
        }

        public static string AddOriginDestination(resultTripDetailsSolutionItinerarySlice slice, List<OriginDestinationAssociation> reshopOriginDestAss, int OdLabelID)
        {
            string OriginDestRefID = $"{Constants.Application.OriginDestination}{OdLabelID}";
            if (!reshopOriginDestAss.Any(Od => Od.OriginDestination.OriginAirport == slice.origin && Od.OriginDestination.DestinationAirport == slice.destination))
            {
                reshopOriginDestAss.Add(new OriginDestinationAssociation
                {
                    OdLabel = OriginDestRefID,
                    OriginDestination = new()
                    {
                        OriginAirport = slice.origin,
                        DestinationAirport = slice.destination
                    }
                });
            }
            else
            {
                OriginDestRefID = reshopOriginDestAss.FirstOrDefault(Od => Od.OriginDestination.OriginAirport == slice.origin && Od.OriginDestination.DestinationAirport == slice.destination).OdLabel;
            }
            return OriginDestRefID;
        }
    }
}
