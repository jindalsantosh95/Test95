using MediatR;
using System.Collections.Generic;
using VAA.EAPI.Microservices.Loyalty.Customer.Application.Query.AddressHistory;
using VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Entities;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Repositories.AddressHistoryDocument
{
    public class AddressHistoryRepoQuery : IRequest<IEnumerable<AddressHistory>>
    {
        public string CustomerId { get; set; }

        public AddressHistoryRepoQuery(AddressHistoryQuery request)
        {
            CustomerId = request.CustomerId;
        }
    }
}
