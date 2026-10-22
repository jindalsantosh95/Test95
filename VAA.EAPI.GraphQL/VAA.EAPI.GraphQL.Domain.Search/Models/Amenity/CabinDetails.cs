using VAA.EAPI.GraphQL.Common.Extensions;
using VAA.EAPI.GraphQL.Domain.Search.Infrastructure.Models.ReferenceMS;
using VAA.EAPI.GraphQL.Domain.Search.Models.FlightOffer;

namespace VAA.EAPI.GraphQL.Domain.Search.Models.Amenity
{
    public class CabinDetails
    {
        // For backwards compatibility
        [GraphQLDeprecated("Actually a list of segments, use the Segments property instead.")]
        public IEnumerable<SegmentInfo> Legs => Segments;
        public IEnumerable<SegmentInfo> Segments { get; set; }

        public CabinDetails() { }
        public CabinDetails(Brand.Brand brandResponse, IEnumerable<OfferDetails> offers, IEnumerable<Segment> offerResponseSegments,
            bool isAwardSearchResult, IEnumerable<AirlineInfo> airlineInfo, IEnumerable<CabinInfo> cabinInfo)
        {
            // Although BrandMS calls these segments, they are actually legs. e.g. a Direct with Stop segment would have separate entries for each leg of the flight.
            var legs = brandResponse.Itineraries.SelectMany(itinerary => itinerary.Legs)
                                        .SelectMany(fare => fare.Fares)
                                        .SelectMany(segment => segment.Segments);
            // Merge one or more legs into a single segment
            var segments = legs.GroupBy(x => x.FlightNumber);

            Segments = segments.Select((legsOfSegment, index) => GetSegmentInfo(legsOfSegment, index, offers, offerResponseSegments, isAwardSearchResult, airlineInfo, cabinInfo));
        }

        private static SegmentInfo GetSegmentInfo(IEnumerable<Brand.Segment> brandLegs, int index, IEnumerable<OfferDetails> offers, IEnumerable<Segment> flightOfferSegments,
            bool isAwardSearchResult, IEnumerable<AirlineInfo> airlineInfo, IEnumerable<CabinInfo> cabinInfo)
        {
            if (brandLegs.DistinctBy(l => l.FlightNumber).Count() != 1)
            {
                throw new InvalidDataException("All legs must be for the same segment (i.e. a Direct with Stop segment).");
            }

            var flightOfferSegment = flightOfferSegments.First(x => x.OriginAirportCode == brandLegs.First().Origin && x.DestinationAirportCode == brandLegs.Last().Destination);
            var fareSegment = offers.SelectMany(o => o.OfferItems).SelectMany(oi => oi.Services).SelectMany(s => s.FareDetails!.FareSegments!.Where(x => x.FlightSegmentRefID == flightOfferSegment.SegmentId)).First();
            var operatingCarrier = airlineInfo.FirstOrDefault(x => x.CarrierCode == flightOfferSegment.OperatingCarrier);

            var segmentInfo = new SegmentInfo()
            {
                SegmentNum = index,
                Origin = brandLegs.First().Origin,
                Destination = brandLegs.Last().Destination,
                OperatedBy = operatingCarrier?.CarrierName ?? flightOfferSegment.OperatingCarrier,
                OperatedByCode = flightOfferSegment.OperatingCarrier,
                FlightDepartureTime = Common.Helper.DateTimeHelper.ParseLocalDateTime(flightOfferSegment.ScheduledDepartureTime?.LocalDateTime),
                FlightNumber = brandLegs.First().FlightNumber,
                HeroImageURI = brandLegs.First().Upas?.FirstOrDefault(x => x.Categories.Contains("seat") || x.Categories.Contains("fare-informational"))?.Photos?.FirstOrDefault()!.LargeUrl!,
                SeatPitch = brandLegs.First().Amenities?.Seats?.DisplayText!,
                CheckedBags = FormatBaggageDisplayText(brandLegs.First(), Constants.Application.BAGGAGE_TYPE_CHECKED),
                CarryOnBags = FormatBaggageDisplayText(brandLegs.First(), Constants.Application.BAGGAGE_TYPE_CARRYON),
                FreshFoods = brandLegs.First().Amenities?.FreshFood?.DisplayText!,
                Beverages = brandLegs.First().Amenities?.Beverage?.DisplayText!
            };
            // UTAs are loaded per fare basis code, which may include multiple segments. We can be confident these four will apply to all VS-operated segments, but when there are a mix of airlines they may not apply to the non-dominant segment(s).
            if (brandLegs.Any(bl => bl.DominantLeg == true) || flightOfferSegment.OperatingCarrier == "VS")
            {
                segmentInfo.SeatSelection = brandLegs.First().SeatSelection?.FirstOrDefault(s => s.Application != null && s.Application.Equals(Constants.Application.APPLICATION_TYPE, StringComparison.InvariantCultureIgnoreCase))?.ComercialName?.ToTitleCase();
                segmentInfo.BoardingPriority = brandLegs.First().BoardingPriorities?.FirstOrDefault(s => s.Application != null && s.Application.Equals(Constants.Application.APPLICATION_TYPE, StringComparison.InvariantCultureIgnoreCase))?.CommercialName?.ToTitleCase();
                segmentInfo.CheckInPriority = brandLegs.First().CheckInPriority?.FirstOrDefault(s => s.Application != null && s.Application.Equals(Constants.Application.APPLICATION_TYPE, StringComparison.InvariantCultureIgnoreCase))?.ComercialName?.ToTitleCase();
                segmentInfo.LoungeAccess = brandLegs.First().LoungeAccess?.FirstOrDefault(s => s.Application != null && s.Application.Equals(Constants.Application.APPLICATION_TYPE, StringComparison.InvariantCultureIgnoreCase))?.ComercialName?.ToTitleCase();
            }
            if (fareSegment != null)
            {
                var cabin = cabinInfo.FirstOrDefault(x => x.BrandID == fareSegment.CabinName);

                if (cabin != null)
                {
                    segmentInfo.FareFamilyType = isAwardSearchResult ? cabin.AwardFareFamily : cabin.FareFamily;
                    segmentInfo.DominantCabinName = cabin.BrandName;
                }

                segmentInfo.FareBasisCode = fareSegment.FareBasisCode;
                segmentInfo.BookingClass = fareSegment.BookingClass;
                segmentInfo.DominantSegment = fareSegment.IsDominantLeg;
            }
            return segmentInfo;
        }

        private static string FormatBaggageDisplayText(Brand.Segment segment, string bagType)
        {
            string displayText = string.Empty;
            if (bagType == Constants.Application.BAGGAGE_TYPE_CHECKED && segment.CheckedBaggage.Any())
            {
                var bagCount = segment.CheckedBaggage.FirstOrDefault().FreeBags.Pieces;
                var weight = segment.CheckedBaggage.FirstOrDefault().FreeBags.Weight!;
                if (bagCount > 0)
                {
                    if (bagCount > 1)
                    {
                        displayText = weight != null ? $"{bagCount} × {weight.Weight.Metric}kg checked bags" : $"{bagCount} × checked bags";
                    }
                    else
                    {
                        displayText = weight != null ? $"{bagCount} × {weight.Weight.Metric}kg checked bag" : $"{bagCount} × checked bag";
                    }
                }
                else
                {
                    displayText = "Checked baggage not included";
                }
            }
            else if (bagType == Constants.Application.BAGGAGE_TYPE_CARRYON && segment.CarryOnBaggage.Any())
            {
                var bagCount = segment.CarryOnBaggage.FirstOrDefault().FreeBags.Pieces;
                var weight = segment.CarryOnBaggage.FirstOrDefault().FreeBags.Weight!;
                if (bagCount > 0)
                {
                    if (bagCount > 1)
                    {
                        displayText = weight != null ? $"{bagCount} × {weight.Weight.Metric}kg carry on bags" : $"{bagCount} × carry on bags";
                    }
                    else
                    {
                        displayText = weight != null ? $"{bagCount} × {weight.Weight.Metric}kg carry on bag" : $"{bagCount} × carry on bag";
                    }
                }
                else
                {
                    displayText = "No hand baggage";
                }
            }

            return displayText;
        }
    }
}
