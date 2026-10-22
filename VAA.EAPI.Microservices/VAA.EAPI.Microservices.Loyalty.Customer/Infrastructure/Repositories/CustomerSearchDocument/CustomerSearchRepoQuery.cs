using MediatR;
using VAA.EAPI.Microservices.Loyalty.Customer.Application.Query.CustomerSearch;
using VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Entities;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Repositories.CustomerSearchDocument
{
    public class CustomerSearchRepoQuery : IRequest<CustomerSearch>
    {
        public string MemberNumber { get; set; }

        public CustomerSearchRepoQuery(CustomerSearchQuery request)
        {
            MemberNumber = request.MemberNumber;
        }
    }
}
