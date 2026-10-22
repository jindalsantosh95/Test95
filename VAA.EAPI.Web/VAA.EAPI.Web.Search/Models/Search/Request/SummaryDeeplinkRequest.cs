using MediatR;
using System.Collections.Generic;
using System.IO;
using VAA.EAPI.Web.Common.Extensions;
using VAA.EAPI.Web.Common.Serialisation;
using VAA.EAPI.Web.Search.Models.Search.Response.TravelPlusSummary;

namespace VAA.EAPI.Web.Search.Models.Search.Request
{
    public class SummaryDeeplinkRequest : IRequest<Summary>
    {
        public string? MetaPartnerId { get; set; }
        public IEnumerable<FlightSegment>? FlightSegments { get; set; }
        public Amount? TotalOfferedAmount { get; set; }
        public PaxBreakdown? Customers { get; set; }
        public string? DominantSegmentBrandId { get; set; }
        public string SummaryLink { get; set; }

        public static explicit operator SummaryDeeplinkRequest(Stream source)
        {
            var bodyContent = new StreamReader(source).ReadToEndAsync().GetAwaiter().GetResult();

            return bodyContent.IsNullOrEmpty() ? new() : Json.Deserialize<SummaryDeeplinkRequest>(bodyContent);
        }
    }
}
