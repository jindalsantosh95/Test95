using CSharpFunctionalExtensions;
using MediatR;
using System;
using System.Collections.Generic;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Insights.Domain;

namespace VAA.EAPI.Microservices.Insights.Application.Query
{
    public sealed class CatalogueRequest : IRequest<Result<Response<ICollection<Endpoint>>>>
    {
        public Guid SubscriptionId { get; set; }
        public string ApiVersion { get; set; }
        public string? Environment { get; set; }
    }
}
