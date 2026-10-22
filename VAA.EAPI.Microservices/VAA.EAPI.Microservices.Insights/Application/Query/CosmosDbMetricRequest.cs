using CSharpFunctionalExtensions;
using MediatR;
using System;
using System.Collections.Generic;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Insights.Domain;

namespace VAA.EAPI.Microservices.Insights.Application.Query
{
    public sealed class CosmosDbMetricRequest : IRequest<Result<Response<IEnumerable<CosmosDbMetricResult>>>>
    {
        public IEnumerable<string> ResourceNames { get; set; }
    }
}
