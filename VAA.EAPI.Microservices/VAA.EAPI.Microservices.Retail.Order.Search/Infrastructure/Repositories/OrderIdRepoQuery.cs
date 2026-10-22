using MediatR;
using System.Collections.Generic;
using VAA.EAPI.Microservices.Retail.Order.Search.Domain;

namespace VAA.EAPI.Microservices.Retail.Order.Search.Infrastructure.Repositories
{
    public class OrderIdRepoQuery : BookingQuery, IRequest<IEnumerable<string>>
    {
        public OrderIdRepoQuery(string fcNumber, string phoneNumber) : base(fcNumber, phoneNumber) { }
    }
}
