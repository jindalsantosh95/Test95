using MediatR;
using System.Collections.Generic;
using VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Entities;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Repositories.ContactHistoryDocument
{
    public class ContactHistoryRepoQuery : IRequest<IEnumerable<ContactHistory>>
    {
        public string CustomerId { get; set; }

        public ContactHistoryRepoQuery(string customerId)
        {
            CustomerId = customerId;
        }

    }
}