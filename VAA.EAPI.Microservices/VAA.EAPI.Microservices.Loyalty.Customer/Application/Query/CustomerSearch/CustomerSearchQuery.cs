using CSharpFunctionalExtensions;
using MediatR;
using VAA.EAPI.Domain.Entities.Common;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Application.Query.CustomerSearch
{
    public class CustomerSearchQuery : IRequest<Result<Response<Infrastructure.Entities.CustomerSearch>>>
    {
        public string MemberNumber { get; set; }

        public CustomerSearchQuery(string memberNumber)
        {
            MemberNumber = memberNumber;
        }
    }
}
