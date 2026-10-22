using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Caching;
using VAA.EAPI.Microservices.Retail.Basket.Mapper;
using Environment = VAA.EAPI.Microservices.Retail.Basket.Constants.Environment;

namespace VAA.EAPI.Microservices.Retail.Basket.Application.Command
{
    public class UpdatePaymentCommandHandler : IRequestHandler<UpdatePaymentCommand, Result<Response<Domain.Basket>>>
    {
        private readonly IDistributedCacheService _distributedCacheService;
        private readonly ILogger<UpdatePaymentCommandHandler> _logger;

        public UpdatePaymentCommandHandler(IDistributedCacheService distributedCacheService, ILogger<UpdatePaymentCommandHandler> logger)
        {
            _distributedCacheService = distributedCacheService ?? throw new ArgumentNullException(nameof(distributedCacheService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Result<Response<Domain.Basket>>> Handle(UpdatePaymentCommand request, CancellationToken cancellationToken)
        {
            var basket = await _distributedCacheService.Get<Domain.Basket>(request.Id.ToString("N"));

            var updatedBasket = BasketMapper.UpdatePaymentSessionDetails(request.PaymentSessions, basket);

            await _distributedCacheService.Set(request.Id.ToString("N"), updatedBasket, true, Environment.RedisExpiryTime, TimeSpan.Zero);

            return new Response<Domain.Basket>
            {
                Status = Status.OK,
                Value = updatedBasket,
            };
        }
    }
}
