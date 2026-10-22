using Newtonsoft.Json.Linq;
using NodaTime;
using System;
using System.Collections.Generic;
using System.Linq;

namespace VAA.EAPI.Microservices.Retail.Holidays.Domain.Trips
{
    public class Slice
    {
        public string Id { get; set; }
        public int? DistanceMiles { get; set; }
        public int? Ordinal { get; set; }
        public AirportDetails Origin { get; set; }
        public AirportDetails Destination { get; set; }
        public IEnumerable<Segment> Segments { get; set; }
        public string CheckinStatus { get; set; }
        public string TotalSliceDuration { get; set; }
        public bool CheckInEligible { get; set; }
        public DateTime? CheckInOpens { get; set; }
        public DateTime? ScheduledDepartureLocal { get; set; }
        public DateTime? ScheduledArrivalLocal { get; set; }
        public DateTime? ScheduledArrivalBasedOnDepartureLocal { get; set; }
        public DateTime? EstimatedDepartureLocal { get; set; }
        public DateTime? EstimatedArrivalLocal { get; set; }
        public DateTime? ActualDepartureLocal { get; set; }
        public DateTime? ActualArrivalLocal { get; set; }
        public DateTime? CheckInOpensLocal { get; set; }
        public DateTime? ScheduledDepartureUtc { get; set; }
        public DateTime? ScheduledArrivalUtc { get; set; }
        public DateTime? EstimatedDepartureUtc { get; set; }
        public DateTime? EstimatedArrivalUtc { get; set; }
        public DateTime? ActualDepartureUtc { get; set; }
        public DateTime? ActualArrivalUtc { get; set; }

        public static explicit operator Slice(JToken source)
        {
            if (source == null) return null;

            var flightOptions = source["flightOptions"] as JArray;

            if (flightOptions == null) return null;

            var selected = source["selectedFare"]?["selectedFlightOptionDetail"];
            var selectedFlightRef = selected?["flightOptionRef"]?.Value<string>();
            var selectedSegmentRefs = (selected?["segmentDetails"] as JArray)?
                .Select(x => x["segmentRef"]?.Value<string>())
                .Where(r => !string.IsNullOrWhiteSpace(r))
                .ToHashSet(StringComparer.OrdinalIgnoreCase) ?? new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            var segmentsToken = flightOptions
                .Where(fo => selectedFlightRef == null || fo["reference"]?.Value<string>() == selectedFlightRef)
                .SelectMany(fo => (fo["segments"] as JArray) ?? new JArray())
                .Where(seg => selectedSegmentRefs.Count == 0 || selectedSegmentRefs.Contains(seg["reference"]?.Value<string>() ?? string.Empty))
                .ToList();
            if (segmentsToken.Count > 1)
            {
                var segments = segmentsToken.Select(t => (Segment)t).Where(s => s != null).ToList();
                var firstSegment = segments.FirstOrDefault();
                var lastSegment = segments.LastOrDefault();
                var sliceDuration = FormatTbxDuration(segmentsToken.LastOrDefault()?["duration"]);

                return new()
                {
                    Id = ((source["searchSegmentIndex"]?.Value<int>() ?? 0) + 1).ToString(),
                    ScheduledDepartureLocal = firstSegment?.ScheduledDepartureLocal,
                    ScheduledArrivalLocal = lastSegment?.ScheduledArrivalLocal,
                    Origin = firstSegment?.Origin,
                    Destination = lastSegment?.Destination,
                    TotalSliceDuration = sliceDuration,
                    Segments = segments
                };
            }

            var segment = (Segment)segmentsToken.FirstOrDefault();

            var directDuration = FormatTbxDuration(segmentsToken.FirstOrDefault()?["duration"]);

            return new()
            {
                Id = ((source["searchSegmentIndex"]?.Value<int>() ?? 0) + 1).ToString(),
                ScheduledDepartureLocal = segment?.ScheduledDepartureLocal,
                ScheduledArrivalLocal = segment?.ScheduledArrivalLocal,
                Origin = segment?.Origin,
                Destination = segment?.Destination,
                TotalSliceDuration = directDuration,
                Segments = segment != null ? [segment] : []
            };

        }

        internal static string FormatTbxDuration(JToken durationToken)
        {
            var durationMinutes = durationToken?.Value<int?>();
            if (!durationMinutes.HasValue || durationMinutes.Value <= 0)
            {
                return null;
            }

            var duration = TimeSpan.FromMinutes(durationMinutes.Value);
            return $"PT{(int)duration.TotalHours}H{duration.Minutes}M";
        }
    }
}
