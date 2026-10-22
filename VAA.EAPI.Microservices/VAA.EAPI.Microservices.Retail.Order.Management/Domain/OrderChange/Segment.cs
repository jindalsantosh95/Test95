using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using System;
namespace VAA.EAPI.Microservices.Retail.Order.Management.Domain.OrderChange
{
    public class Segment
    {
        [OpenApiProperty(Description = "Origin Destination Ref ID")]
        public string OriginDestId { get; set; }
        [OpenApiProperty(Description = "Segment Id")]
        public string SegmentId { get; set; }
        [OpenApiProperty(Description = "Marketing Airline carrier code Example VS")]
        public string MarketingCarrier { get; set; }
        [OpenApiProperty(Description = "Flight number - an integer value. Example 301,302.")]
        public string MarketingFlightNumber { get; set; }
        [OpenApiProperty(Description = "Operating Airline carrier code e.g. DL,KL ")]
        public string OperatingCarrier { get; set; }
        [OpenApiProperty(Description = "Operating flight number - an integer value. Example 30 ")]
        public string OperatingFlightNumber { get; set; }
        [OpenApiProperty(Description = "The originating airport code for the flight")]
        public string OriginAirportCode { get; set; }
        [OpenApiProperty(Description = "The destination airport code for the flight")]
        public string DestinationAirportCode { get; set; }
        [OpenApiProperty(Description = "The scheduled departure airport for the flight")]
        public DateTime? ScheduledDepartureTime { get; set; }
        [OpenApiProperty(Description = "The scheduled arrival airport for the flight")]
        public DateTime? ScheduledArrivalTime { get; set; }
        [OpenApiProperty(Description = "Class of service")]
        public string ClassOfService { get; set; }
    }
}
