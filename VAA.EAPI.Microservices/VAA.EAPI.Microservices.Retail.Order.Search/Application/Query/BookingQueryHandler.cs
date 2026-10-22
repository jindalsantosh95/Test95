using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Microservices.Retail.Order.Search.Infrastructure.Repositories;
using VAA.EAPI.Microservices.Retail.Order.Search.Domain.BookingDetails.Entity;
using VAA.EAPI.Microservices.Retail.Order.Search.Domain.BookingDetails.Response;
using VAA.EAPI.Microservices.Retail.Order.Search.Mapper.GetBooking;

namespace VAA.EAPI.Microservices.Retail.Order.Search.Application.Query
{
    public class BookingQueryHandler : IRequestHandler<LastNextBookingQuery, BookingResponse>,
                                       IRequestHandler<FutureBookingQuery, BookingResponse>
    {
        private readonly IMediator _mediator;
        protected readonly TimeProvider _timeProvider;

        public BookingQueryHandler(IMediator mediator, TimeProvider timeProvider)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _timeProvider = timeProvider ?? throw new ArgumentNullException(nameof(timeProvider));
        }

        public async Task<BookingResponse> Handle(LastNextBookingQuery request, CancellationToken cancellationToken)
        {
            //Response mapping
            return ResponseMapper.FilterLastNext(await GetBookings(request.FCNumber, request.PhoneNo, cancellationToken), _timeProvider);

        }

        public async Task<BookingResponse> Handle(FutureBookingQuery request, CancellationToken cancellationToken)
        {

            int page = int.TryParse(request.Page, out int Page) ? Page : 1;
            int pageSize = int.TryParse(request.PageSize, out int PageSize) ? PageSize : 10;

            return new BookingResponse()
            {
                VaaBookingDetails = ResponseMapper.FilterFuture(await GetBookings(request.FCNumber, request.PhoneNo, cancellationToken), _timeProvider).VaaBookingDetails.Skip((page - 1) * pageSize).Take(pageSize).ToList()
            };
        }

        private async Task<IEnumerable<OrderDoc>> GetBookings(string fcNumber, string phoneNumber, CancellationToken cancellationToken)
        {
            if (fcNumber.IsNullOrEmpty() && phoneNumber.IsNullOrEmpty())
            {
                throw new ArgumentNullException();
            }

            // Get a list of Order IDs that match the request criteria
            IEnumerable<string> orders = await _mediator.Send(new OrderIdRepoQuery(fcNumber, phoneNumber), cancellationToken);

            IEnumerable<OrderDoc> orderDocs = Enumerable.Empty<OrderDoc>();
            // If there are any orders, then go get them from database                    
            if (orders.Any())
            {
                orderDocs = await _mediator.Send(new OrdersRepoQuery(orders), cancellationToken);

            }

            return orderDocs;
        }
    }
}
