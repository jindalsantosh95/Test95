using CSharpFunctionalExtensions;
using MediatR;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Caching;
using VAA.EAPI.Microservices.Retail.Basket.Mapper;
using Environment = VAA.EAPI.Microservices.Retail.Basket.Constants.Environment;

namespace VAA.EAPI.Microservices.Retail.Basket.Application.Command
{
    public class UpdateOfferIDCommandHandler : IRequestHandler<UpdateOfferIDCommand, Result<Response<Domain.Basket>>>
    {
        private readonly IDistributedCacheService _distributedCacheService;
        public UpdateOfferIDCommandHandler(IDistributedCacheService distributedCacheService) => _distributedCacheService = distributedCacheService ?? throw new ArgumentNullException(nameof(distributedCacheService));

        public async Task<Result<Response<Domain.Basket>>> Handle(UpdateOfferIDCommand request, CancellationToken cancellationToken)
        {
            var basket = await _distributedCacheService.Get<Domain.Basket>(request.Id.ToString("N"));
            var updatedBasket = BasketMapper.UpdateOfferID(request.OfferIDs, basket);
            if (updatedBasket.FlightPassengerSeatMap != null && updatedBasket.FlightPassengerSeatMap.Any())
                updatedBasket.FlightPassengerSeatMap = null;

            await _distributedCacheService.Set(request.Id.ToString("N"), updatedBasket, true, Environment.RedisExpiryTime, TimeSpan.Zero);

            return new Response<Domain.Basket>
            {
                Status = Status.OK,
                Value = updatedBasket
            };
        }
    }
}
