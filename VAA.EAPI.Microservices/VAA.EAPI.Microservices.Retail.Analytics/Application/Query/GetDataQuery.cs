using MediatR;
using System.Collections.Generic;
using VAA.EAPI.Infrastructure.Services.Analytics;

namespace VAA.EAPI.Microservices.Retail.Analytics.Application.Query
{
    public sealed class GetDataQuery : IRequest<IEnumerable<TrackAnalyticsMessage>>
    {
        public string From { get; set; }
        public string To { get; set; }
    }
}
