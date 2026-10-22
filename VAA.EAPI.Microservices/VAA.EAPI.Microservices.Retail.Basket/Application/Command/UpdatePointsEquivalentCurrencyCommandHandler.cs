using CSharpFunctionalExtensions;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Caching;
using VAA.EAPI.Microservices.Retail.Basket.Mapper;

namespace VAA.EAPI.Microservices.Retail.Basket.Application.Command
{
    public class UpdatePointsEquivalentCurrencyCommandHandler : IRequestHandler<UpdatePointsEquivalentCurrencyCommand, Result<Response<Domain.Basket>>>
    {
        private readonly IDistributedCacheService _distributedCacheService;

        public UpdatePointsEquivalentCurrencyCommandHandler(IDistributedCacheService distributedCacheService) => _distributedCacheService = distributedCacheService ?? throw new ArgumentNullException(nameof(distributedCacheService));

        public async Task<Result<Response<Domain.Basket>>> Handle(UpdatePointsEquivalentCurrencyCommand request, CancellationToken cancellationToken)
        {
            var basket = await _distributedCacheService.Get<Domain.Basket>(request.Id.ToString("N"));

            var updatedBasket = BasketMapper.UpdatePointsEquivalentCurrencyDetails(request.PtcWisePointsEquivalentCurrencyDetails, basket);

            await _distributedCacheService.Set(request.Id.ToString("N"), updatedBasket, true, Constants.Environment.RedisExpiryTime, TimeSpan.Zero);

            return new Response<Domain.Basket>
            {
                Status = Status.OK,
                Value = updatedBasket,
            };
        }
    }
}