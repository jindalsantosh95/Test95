using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Caching;
using Environment = VAA.EAPI.Microservices.Retail.Basket.Constants.Environment;

namespace VAA.EAPI.Microservices.Retail.Basket.Application.Command
{
    public sealed class CreateBasketRequestHandler : IRequestHandler<CreateBasketRequest, Result<Response<Domain.Basket>>>
    {
        private readonly IDistributedCacheService _distributedCacheService;
        private readonly ILogger<CreateBasketRequestHandler> _logger;

        public CreateBasketRequestHandler(IDistributedCacheService distributedCacheService, ILogger<CreateBasketRequestHandler> logger)
        {
            _distributedCacheService = distributedCacheService ?? throw new ArgumentNullException(nameof(distributedCacheService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
        public async Task<Result<Response<Domain.Basket>>> Handle(CreateBasketRequest request, CancellationToken cancellationToken)
        {
            try
            {
                var basket = (Domain.Basket)request;

                await _distributedCacheService.Set(basket.BasketId.ToString("N"), basket, true, Environment.RedisExpiryTime, TimeSpan.Zero);

                return new Response<Domain.Basket>
                {
                    Status = Status.OK,
                    Value = basket
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Correlation Id - {AsyncContext.Current.CorrelationId} - Exception - {ex}");
                return Result.Failure<Response<Domain.Basket>>((ex.InnerException ?? ex).ToString());
            }
        }
    }
}
