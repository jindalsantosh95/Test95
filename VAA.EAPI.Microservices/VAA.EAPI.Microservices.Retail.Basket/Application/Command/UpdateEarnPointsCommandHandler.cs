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
    public class UpdateEarnPointsCommandHandler : IRequestHandler<UpdateEarnPointsCommand, Result<Response<Domain.Basket>>>
    {
        private readonly IDistributedCacheService _distributedCacheService;

        public UpdateEarnPointsCommandHandler(IDistributedCacheService distributedCacheService) => _distributedCacheService = distributedCacheService ?? throw new ArgumentNullException(nameof(distributedCacheService));

        public async Task<Result<Response<Domain.Basket>>> Handle(UpdateEarnPointsCommand request, CancellationToken cancellationToken)
        {
            var basket = await _distributedCacheService.Get<Domain.Basket>(request.Id.ToString("N"));

            if (basket == null)
                return new Response<Domain.Basket>
                {
                    Status = Status.NotFound,
                    Value = null
                };

            var updatedBasket = BasketMapper.UpdateCalculatedEarnPoints(request.EarnPoints, basket);

            await _distributedCacheService.Set(request.Id.ToString("N"), updatedBasket, true, Constants.Environment.RedisExpiryTime, TimeSpan.Zero);

            return new Response<Domain.Basket>
            {
                Status = Status.OK,
                Value = updatedBasket,
            };
        }
    }
}