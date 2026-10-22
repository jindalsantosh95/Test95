using CSharpFunctionalExtensions;
using MediatR;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Caching;
using Environment = VAA.EAPI.Microservices.Retail.Basket.Constants.Environment;

namespace VAA.EAPI.Microservices.Retail.Basket.Application.Command
{
    public class UpdateSelectedOfferCommandHandler : IRequestHandler<UpdateSelectedOfferCommand, Result<Response<Domain.Basket>>>
    {
        private readonly IDistributedCacheService _distributedCacheService;

        public UpdateSelectedOfferCommandHandler(IDistributedCacheService distributedCacheService) => _distributedCacheService = distributedCacheService ?? throw new ArgumentNullException(nameof(distributedCacheService));

        public async Task<Result<Response<Domain.Basket>>> Handle(UpdateSelectedOfferCommand request, CancellationToken cancellationToken)
        {
            var basket = await _distributedCacheService.Get<Domain.Basket>(request.Id.ToString("N"));

            basket.BookingJourneyOfferDetails = basket.BookingJourneyOfferDetails == null ? new Domain.BookingJourneyOfferDetails[] { request.BookingJourneyOfferDetails } : basket.BookingJourneyOfferDetails.Append(request.BookingJourneyOfferDetails);

            if (basket.FlightPassengerSeatMap != null && basket.FlightPassengerSeatMap.Any())
                basket.FlightPassengerSeatMap = null;

            await _distributedCacheService.Set(request.Id.ToString("N"), basket, true, Environment.RedisExpiryTime, TimeSpan.Zero);

            return new Response<Domain.Basket>
            {
                Status = Status.OK,
                Value = basket
            };
        }
    }
}
