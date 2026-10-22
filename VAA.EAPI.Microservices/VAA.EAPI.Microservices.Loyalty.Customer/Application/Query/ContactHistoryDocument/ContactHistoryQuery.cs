using MediatR;
using System.Collections.Generic;
using VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Entities;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Application.Query.ContactHistoryDocument
{
    public class ContactHistoryQuery : IRequest<IEnumerable<ContactHistory>>
    {
        public string CustomerId { get; set; }

        public ContactHistoryQuery(string customerId)
        {
            CustomerId = customerId;
        }
    }
}
