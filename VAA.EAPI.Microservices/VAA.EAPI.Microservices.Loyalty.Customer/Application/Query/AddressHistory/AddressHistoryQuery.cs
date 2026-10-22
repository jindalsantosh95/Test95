using MediatR;
using System.Collections.Generic;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Application.Query.AddressHistory
{
    public class AddressHistoryQuery : IRequest<IEnumerable<Infrastructure.Entities.AddressHistory>>
    {
        public string CustomerId { get; set; }

        public AddressHistoryQuery(string customerId)
        {
            CustomerId = customerId;
        }
    }
}
