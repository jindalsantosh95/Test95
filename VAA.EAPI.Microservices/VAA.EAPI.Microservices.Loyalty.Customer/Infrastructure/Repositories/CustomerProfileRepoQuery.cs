using MediatR;
using VAA.EAPI.Microservices.Loyalty.Customer.Domain.InflightProfile;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Repositories
{
    public class CustomerProfileRepoQuery : IRequest<CustomerProfile>
    {
        public string CustomerId { get; set; }

        public CustomerProfileRepoQuery(string customerId)
        {
            CustomerId = customerId;
        }
    }
}