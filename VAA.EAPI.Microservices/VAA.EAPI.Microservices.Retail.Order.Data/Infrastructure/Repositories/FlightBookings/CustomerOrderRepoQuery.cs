using MediatR;
using System.Collections.Generic;
using VAA.EAPI.Microservices.Retail.Order.Data.Domain.Entities;

namespace VAA.EAPI.Microservices.Retail.Order.Data.Infrastructure.Repositories.FlightBookings
{
    public class CustomerOrderRepoQuery : IRequest<List<CustomerOrderAssociation>>
    {
        public string CustomerID { get; set; }
        public string CustomerPhone { get; set; }
        public string CustomerEmail { get; set; }

    }
}
