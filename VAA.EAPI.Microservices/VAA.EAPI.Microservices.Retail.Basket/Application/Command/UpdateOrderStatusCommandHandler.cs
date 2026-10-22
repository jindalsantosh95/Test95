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
    public class UpdateOrderStatusCommandHandler : IRequestHandler<UpdateOrderStatusCommand, Result<Response<Domain.Basket>>>
    {
        private readonly IDistributedCacheService _distributedCacheService;

        public UpdateOrderStatusCommandHandler(IDistributedCacheService distributedCacheService) => _distributedCacheService = distributedCacheService ?? throw new ArgumentNullException(nameof(distributedCacheService));

        public async Task<Result<Response<Domain.Basket>>> Handle(UpdateOrderStatusCommand request, CancellationToken cancellationToken)
        {
            var basket = await _distributedCacheService.Get<Domain.Basket>(request.Id.ToString("N"));

            BasketMapper.UpdateBasketOrderStatus(basket, request.Status);

            await _distributedCacheService.Set(request.Id.ToString("N"), basket, true, Environment.RedisExpiryTime, TimeSpan.Zero);

            return new Response<Domain.Basket>
            {
                Status = Status.OK,
                Value = basket,
            };
        }
    }
}
