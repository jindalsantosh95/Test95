using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Microservices.Retail.Order.Search.Infrastructure.Repositories.SCVReferenceDB.Interface;

namespace VAA.EAPI.Microservices.Retail.Order.Search.Infrastructure.Repositories
{
    public class OrderIdRepoQueryHandler : IRequestHandler<OrderIdRepoQuery, IEnumerable<string>>
    {
        private readonly ISCVRefDBService _refDBService;

        public OrderIdRepoQueryHandler(ISCVRefDBService refDBService)
        {
            _refDBService = refDBService ?? throw new ArgumentNullException(nameof(refDBService));
        }
        public async Task<IEnumerable<string>> Handle(OrderIdRepoQuery repoRequest, CancellationToken cancellationToken)
        {
            var customerOrders = await _refDBService.GetCustomerOrders(new Domain.BookingQuery(repoRequest.FCNumber, repoRequest.PhoneNo));
            return customerOrders.Select(x => x.OrderID);
        }
    }
}
