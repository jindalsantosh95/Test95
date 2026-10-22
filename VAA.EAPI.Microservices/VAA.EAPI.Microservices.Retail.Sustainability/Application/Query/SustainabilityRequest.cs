using CSharpFunctionalExtensions;
using MediatR;
using System.Collections.Generic;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Retail.Sustainability.Domain;

namespace VAA.EAPI.Microservices.Retail.Sustainability.Application.Query
{
    public sealed class SustainabilityRequest : IRequest<Result<Response<IEnumerable<SustainMSResponse>>>>
    {
        public SustainMSRequest Request { get; set; }
    }
}
