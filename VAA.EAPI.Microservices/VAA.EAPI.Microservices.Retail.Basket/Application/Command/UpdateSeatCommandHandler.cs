using CSharpFunctionalExtensions;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Caching;
using VAA.EAPI.Microservices.Retail.Basket.Mapper;
using Environment = VAA.EAPI.Microservices.Retail.Basket.Constants.Environment;

namespace VAA.EAPI.Microservices.Retail.Basket.Application.Command
{
    public class UpdateSeatCommandHandler : IRequestHandler<UpdateSeatCommand, Result<Response<Domain.Basket>>>
    {
        private readonly IDistributedCacheService _distributedCacheService;

        public UpdateSeatCommandHandler(IDistributedCacheService distributedCacheService) => _distributedCacheService = distributedCacheService ?? throw new ArgumentNullException(nameof(distributedCacheService));

        public async Task<Result<Response<Domain.Basket>>> Handle(UpdateSeatCommand request, CancellationToken cancellationToken)
        {
            var basket = await _distributedCacheService.Get<Domain.Basket>(request.Id.ToString("N"));

            var updatedBasket = BasketMapper.UpdateSeatDetails(request.FlightPassengerSeatMap, basket);

            await _distributedCacheService.Set(request.Id.ToString("N"), updatedBasket, true, Environment.RedisExpiryTime, TimeSpan.Zero);

            return new Response<Domain.Basket>
            {
                Status = Status.OK,
                Value = updatedBasket,
            };
        }
    }
}
