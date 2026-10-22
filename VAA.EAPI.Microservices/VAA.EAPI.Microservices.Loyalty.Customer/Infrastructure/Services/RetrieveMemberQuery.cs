using MediatR;
using System.Collections.Generic;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Services
{
    public class RetrieveMemberQuery : IRequest<KeyValuePair<bool, string>>
    {
        public string CustomerId { get; set; }

        public RetrieveMemberQuery(string customerId)
        {
            CustomerId = customerId;
        }
    }
}