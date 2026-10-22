using CSharpFunctionalExtensions;
using MediatR;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.Entities.SupplementarySilver;

namespace VAA.EAPI.Microservices.Loyalty.Member.Management.Application.Query
{
    public class SupplementarySilverAccountDetailsQuery : IRequest<Result<Response<SupplementarySilverAccountResponse>>>
    {
        public string CustomerId { get; set; }
    }
}
