using CSharpFunctionalExtensions;
using MediatR;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.Customer;

namespace VAA.EAPI.Microservices.Loyalty.Member.Management.Application.Query
{
    public class CustomerSearchDocumentServiceQuery : IRequest<Result<Response<CustomerSearch>>>
    {
        public string MemberNumber { get; set; }
    }
}
