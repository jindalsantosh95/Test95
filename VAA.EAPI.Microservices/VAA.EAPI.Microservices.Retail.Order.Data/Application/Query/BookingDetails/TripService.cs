using CSharpFunctionalExtensions;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VAA.EAPI.Common;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Microservices.Retail.Order.Data.Application.Interface;
using VAA.EAPI.Microservices.Retail.Order.Data.Domain.Entities.TripDetails;
using VAA.EAPI.Microservices.Retail.Order.Data.Infrastructure.Repositories;

namespace VAA.EAPI.Microservices.Retail.Order.Data.Application.Query.BookingDetails
{
    public class TripService : ITripDetailsService
    {
        private readonly IMediator _mediator;

        public TripService(IMediator mediator)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }

        public async Task<Result<Response<IEnumerable<TripDetailsResponse>>>> RetrieveTripDetails(TripDetailsRequest request)
        {
            try
            {
                var orderIds = request.TripDetails.Select(x => $"{x.PnrLocator}{x.CreatedDate}");

                var orderDocs = await _mediator.Send(new OrdersRepoQuery(orderIds));

                // Match names in request with order docs and return only the matching entries
                var matchingDocs = request.TripDetails.SelectMany(req => orderDocs
                                   .Where(order =>
                                      order.CustomerDetails != null &&
                                      order.CustomerDetails.Any(cust =>
                                         string.Equals(req.FirstName, cust.FirstName, StringComparison.OrdinalIgnoreCase) &&
                                         string.Equals(req.LastName, cust.LastName, StringComparison.OrdinalIgnoreCase)
                                   )));

                return new Response<IEnumerable<TripDetailsResponse>>
                {
                    Status = Status.OK,
                    Value = matchingDocs?.Select(order => (TripDetailsResponse)order) ?? []
                };

            }
            catch (Exception ex)
            {
                return new Response<IEnumerable<TripDetailsResponse>>()
                {
                    Status = Status.BusinessException,
                    Errors = ErrorMapper.MapTechnicalExceptionToEAPIException(EAPI.Infrastructure.Constants.Constants.TechnicalError, AsyncContext.Current.CorrelationId.ToString(), EAPI.Infrastructure.Constants.Constants.ErrorCodesPath)
                };
            }
        }
    }
}