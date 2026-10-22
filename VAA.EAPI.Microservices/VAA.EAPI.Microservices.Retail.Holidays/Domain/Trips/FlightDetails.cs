using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Common.Helpers;

namespace VAA.EAPI.Microservices.Retail.Holidays.Domain.Trips
{
    public class FlightDetails
    {
        private const string AIR4 = "AIR4";
        private const string AMD = "AMD";
        private static readonly HashSet<string> _airlineCodesForAir4 = new HashSet<string>(ConfigReader.Try("Holidays:AirlineCodesForAir4", "VS").Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries), StringComparer.OrdinalIgnoreCase);

        public IEnumerable<Slice> Sectors { get; set; }
        public Reference AirlineLocator { get; set; }
        public bool IsBookingSourceAir4 { get; set; }

        public static explicit operator FlightDetails(JToken source)
        {
            if (source == null)
                return null;

            var fareInfo = (source["fareInfo"] as JArray) ?? new JArray();

            var selectedSegmentTokens = new JArray(fareInfo.SelectMany(ExtractSelectedSegments));

            var crsCode = source["detail"]?["crsCode"]?.Value<string>() ?? string.Empty;

            var isAir4 = crsCode.Is(AIR4) || (crsCode.Is(AMD) && selectedSegmentTokens.Any(s => _airlineCodesForAir4.Contains(s["marketingAirline"]?.Value<string>() ?? string.Empty)));

            return new()
            {
                Sectors = BuildSectors(fareInfo).Where(s => s != null),
                IsBookingSourceAir4 = isAir4,
                AirlineLocator = (Reference)fareInfo.FirstOrDefault()
            };
        }

        private static IEnumerable<Slice> BuildSectors(JArray fareInfo)
        {
            if (fareInfo is null || fareInfo.Count == 0)
                return Enumerable.Empty<Slice>();

            var groups = new List<SectorGroup>();

            var maxConnectionGap = TimeSpan.FromHours(24);
            var orderedFareInfo = fareInfo
                .OfType<JToken>()
                .OrderBy(x => x["searchSegmentIndex"]?.Value<int>() ?? int.MaxValue)
                .ToList();

            foreach (var fareInfoItem in orderedFareInfo)
            {
                var segmentTokens = ExtractSelectedSegments(fareInfoItem).ToList();
                if (segmentTokens.Count == 0)
                    continue;

                var segments = segmentTokens.Select(t => (Segment)t).Where(s => s != null).ToList();
                if (segments.Count == 0)
                    continue;

                var duration = Slice.FormatTbxDuration(segmentTokens.LastOrDefault()?["duration"]);

                var firstSegment = segments.First();
                var lastSegment = segments.Last();

                var direction = segmentTokens.FirstOrDefault()?["direction"]?.Value<string>()?.Trim();
                var groupId = ((fareInfoItem["searchSegmentIndex"]?.Value<int>() ?? 0) + 1).ToString();

                var nextOriginCode = firstSegment.Origin?.Code;
                var nextDestinationCode = lastSegment.Destination?.Code;
                var nextDeparture = firstSegment.ScheduledDepartureLocal;
                var nextArrival = lastSegment.ScheduledArrivalLocal;

                if (groups.Count == 0)
                {
                    groups.Add(SectorGroup.Create(groupId, direction, segments, duration, nextOriginCode, nextDestinationCode, nextDeparture, nextArrival));
                    continue;
                }

                var current = groups[^1];

                var directionMatches = current.Direction.IsNullOrEmpty()
                                       || direction.IsNullOrEmpty()
                                       || current.Direction.Is(direction);

                var connectionGapValid = current.Arrival.HasValue
                                         && nextDeparture.HasValue
                                         && nextDeparture.Value >= current.Arrival.Value
                                         && (nextDeparture.Value - current.Arrival.Value) <= maxConnectionGap;

                var canMerge = directionMatches
                               && !current.DestinationCode.IsNullOrEmpty()
                               && !nextOriginCode.IsNullOrEmpty()
                               && current.DestinationCode.Is(nextOriginCode)
                               && connectionGapValid;

                if (!canMerge)
                {
                    groups.Add(SectorGroup.Create(groupId, direction, segments, duration, nextOriginCode, nextDestinationCode, nextDeparture, nextArrival));
                    continue;
                }

                current.Append(segments, duration, nextDestinationCode, nextArrival);
            }

            return groups.Select(g => g.ToSlice());
        }

        private static IEnumerable<JToken> ExtractSelectedSegments(JToken fareInfoItem)
        {
            if (fareInfoItem is null)
                return Enumerable.Empty<JToken>();

            var flightOptions = fareInfoItem["flightOptions"] as JArray;
            if (flightOptions is null)
                return Enumerable.Empty<JToken>();

            var selected = fareInfoItem["selectedFare"]?["selectedFlightOptionDetail"];
            var selectedFlightRef = selected?["flightOptionRef"]?.Value<string>();

            var selectedSegmentRefs = (selected?["segmentDetails"] as JArray)?
                .Select(x => x["segmentRef"]?.Value<string>())
                .Where(r => !r.IsNullOrEmpty())
                .ToHashSet(StringComparer.OrdinalIgnoreCase)
                ?? new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            return flightOptions
                .Where(fo => selectedFlightRef == null || fo["reference"]?.Value<string>() == selectedFlightRef)
                .SelectMany(fo => (fo["segments"] as JArray) ?? new JArray())
                .Where(seg => selectedSegmentRefs.Count == 0 || selectedSegmentRefs.Contains(seg["reference"]?.Value<string>() ?? string.Empty));
        }

        private sealed class SectorGroup
        {
            private SectorGroup(string id, string? direction, List<Segment> segments, string? duration, string? originCode, string? destinationCode, DateTime? departure, DateTime? arrival)
            {
                Id = id;
                Direction = direction;
                Segments = segments;
                Duration = duration;
                OriginCode = originCode;
                DestinationCode = destinationCode;
                Departure = departure;
                Arrival = arrival;
            }

            public string Id { get; }
            public string? Direction { get; }
            public List<Segment> Segments { get; }
            public string? Duration { get; private set; }
            public string? OriginCode { get; }
            public string? DestinationCode { get; private set; }
            public DateTime? Departure { get; }
            public DateTime? Arrival { get; private set; }

            public static SectorGroup Create(string id, string? direction, List<Segment> segments, string? duration, string? originCode, string? destinationCode, DateTime? departure, DateTime? arrival)
                => new(id, direction, segments, duration, originCode, destinationCode, departure, arrival);

            public void Append(IEnumerable<Segment> segments, string? duration, string? destinationCode, DateTime? arrival)
            {
                Segments.AddRange(segments);
                Duration = duration;
                DestinationCode = destinationCode;
                Arrival = arrival;
            }

            public Slice ToSlice()
            {
                var first = Segments.FirstOrDefault();
                var last = Segments.LastOrDefault();

                return new Slice
                {
                    Id = Id,
                    ScheduledDepartureLocal = first?.ScheduledDepartureLocal,
                    ScheduledArrivalLocal = last?.ScheduledArrivalLocal,
                    Origin = first?.Origin,
                    Destination = last?.Destination,
                    TotalSliceDuration = Duration,
                    Segments = Segments
                };
            }
        }
    }
}
