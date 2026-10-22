using CSharpFunctionalExtensions;
using MediatR;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Infrastructure.Models;

namespace VAA.EAPI.Microservices.Loyalty.Member.Management.Application.Query
{
    public class CustomerDetailsServiceQuery : IRequest<Result<Response<CustomerDTO>>>
    {
        public string CustomerId { get; set; }
        public string MemberId { get; set; }
    }
}