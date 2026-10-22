using CSharpFunctionalExtensions;
using MediatR;
using System;
using System.Collections.Generic;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Insights.Domain;

namespace VAA.EAPI.Microservices.Insights.Application.Query
{
    public sealed class DeploymentMetricRequest : IRequest<Result<Response<IEnumerable<DeploymentMetricsResult>>>>
    {
        public string[] DefinitionIds { get; set; }
    }
}
