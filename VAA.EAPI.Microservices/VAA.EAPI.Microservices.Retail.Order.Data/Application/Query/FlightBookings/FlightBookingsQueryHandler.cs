using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Microservices.Retail.Order.Data.Application.Query.FlightBookings;
using VAA.EAPI.Microservices.Retail.Order.Data.Domain.Entities;
using VAA.EAPI.Microservices.Retail.Order.Data.Domain.Entities.FlightBookings;
using VAA.EAPI.Microservices.Retail.Order.Data.Domain.Entities.FlightBookings.Entity;
using VAA.EAPI.Microservices.Retail.Order.Data.Infrastructure.Repositories;
using VAA.EAPI.Microservices.Retail.Order.Data.Infrastructure.Repositories.FlightBookings;
using VAA.EAPI.Microservices.Retail.Order.Data.Mapper.FlightBookings;

namespace VAA.EAPI.Microservices.Retail.Order.Data.Application.Query.FutureBooking
{
    public class FlightBookingsQueryHandler : IRequestHandler<FlightBookingsQuery, FlightBookingsResponse>
    {
        private readonly IMediator _mediator;

        public FlightBookingsQueryHandler(IMediator mediator, ILogger<FlightBookingsQueryHandler> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }

        public async Task<FlightBookingsResponse> Handle(FlightBookingsQuery request, CancellationToken cancellationToken)
        {
            FlightBookingsResponse result = new();

            if (Boolean.Parse(ConfigReader.Get(Constants.Environment.EnableMockView)))
            {

                return new FlightBookingsResponse()
                {
                    FlightBookingsDetails = FlightBookingsResponseMapper.MockCustomerBookingResponse(),
                };
            }
            if (!string.IsNullOrEmpty(request.FCNumber) || !string.IsNullOrEmpty(request.Email) || !string.IsNullOrEmpty(request.PhoneNo))
            {
                result = FlightBookingsResponseMapper.FilterFuture(await GetCustomerBookings(request, cancellationToken));
            }
            else
            {
                result.Message = $"Either '{nameof(request.FCNumber)} or {nameof(request.Email)} or {nameof(request.PhoneNo)}' must be spefied.";
            }

            return result;

        }

        private async Task<IEnumerable<OrderDoc>> GetCustomerBookings(FlightBookingsQuery request, CancellationToken cancellationToken)
        {

            // Get a list of Order IDs that match the request criteria
            IEnumerable<CustomerOrderAssociation> orders = await _mediator.Send(new CustomerOrderRepoQuery() { CustomerID = request.FCNumber, CustomerEmail = request.Email.ToUpper(), CustomerPhone = request.PhoneNo }, cancellationToken);

            IEnumerable<OrderDoc> orderDocs = Enumerable.Empty<OrderDoc>();

            // If there are any orders, then go get them from cosmos                     
            if (orders.Any())
            {
                orderDocs = await _mediator.Send(new OrdersRepoQuery(orders.Select(x => x.OrderID)), cancellationToken);

            }

            return orderDocs;
        }
    }
}
