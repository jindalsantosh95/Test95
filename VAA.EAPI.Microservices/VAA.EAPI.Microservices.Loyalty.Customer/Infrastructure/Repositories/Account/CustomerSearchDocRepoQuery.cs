using MediatR;
using VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Entities;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Repositories.Account
{
    public class CustomerSearchDocRepoQuery : IRequest<CustomerSearch>
    {
        public string MemberNumber { get; set; }

        public CustomerSearchDocRepoQuery(string memberNumber)
        {
            MemberNumber = memberNumber;
        }
    }
}
