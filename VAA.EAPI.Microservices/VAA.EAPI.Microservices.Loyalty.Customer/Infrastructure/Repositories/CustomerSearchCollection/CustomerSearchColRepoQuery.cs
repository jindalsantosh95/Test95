using MediatR;
using System.Collections.Generic;
using VAA.EAPI.Microservices.Loyalty.Customer.Application.Query.CustomerSearchCollection;
using VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Entities.CustomerSearchCollection;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Repositories.CustomerSearchCollection
{
    public class CustomerSearchColRepoQuery : IRequest<IEnumerable<CustomerSearch>>
    {
        public string PhoneNumber { get; set; }

        public CustomerSearchColRepoQuery(CustomerSearchByPhoneQuery request)
        {
            PhoneNumber = request.PhoneNumber;
        }
    }
}
