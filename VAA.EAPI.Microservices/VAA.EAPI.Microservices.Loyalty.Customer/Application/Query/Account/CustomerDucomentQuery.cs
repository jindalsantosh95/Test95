using CSharpFunctionalExtensions;
using MediatR;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Entities;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Application.Query.Account
{
    public class CustomerDucomentQuery : IRequest<Result<Response<Customers>>>
    {
        public CustomerDucomentQuery(string memberNumber)
        {
            MemberNumber = memberNumber;
        }
        public string MemberNumber { get; set; }
    }
}
