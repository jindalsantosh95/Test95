using CSharpFunctionalExtensions;
using MediatR;
using System;
using System.Collections.Generic;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Insights.Domain;

namespace VAA.EAPI.Microservices.Insights.Application.Query
{
    public sealed class AppInsightsMetricsRequest : IRequest<Result<Response<IEnumerable<AppInsightsMetricsResult>>>>
    {
        public string AppId { get; set; }
        public string ApiKey { get; set; }
        public string Query { get; set; }
    }
}
