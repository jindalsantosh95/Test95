using System.Collections.Generic;
using CSharpFunctionalExtensions;
using MediatR;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.Award;

namespace VAA.EAPI.Microservices.Loyalty.Member.Management.Application.Query
{
    public class AwardDocumentServiceQuery : IRequest<Result<Response<IEnumerable<Award>>>>
    {
        public string CustomerId { get; set; }
    }
}
