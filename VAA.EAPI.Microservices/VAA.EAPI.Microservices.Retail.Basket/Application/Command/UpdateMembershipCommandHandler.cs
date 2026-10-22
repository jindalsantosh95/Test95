using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Caching;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Microservices.Retail.Basket.Mapper;
using VAA.EAPI.Microservices.Retail.Basket.Validation;
using Environment = VAA.EAPI.Microservices.Retail.Basket.Constants.Environment;
using System.Linq;

namespace VAA.EAPI.Microservices.Retail.Basket.Application.Command
{
    public class UpdateMembershipCommandHandler : IRequestHandler<UpdateMembershipCommand, Result<Response<Domain.Basket>>>
    {

        private readonly IDistributedCacheService _distributedCacheService;
        private readonly ILogger<UpdateMembershipCommandHandler> _logger;

        public UpdateMembershipCommandHandler(IDistributedCacheService distributedCacheService, ILogger<UpdateMembershipCommandHandler> logger)
        {
            _distributedCacheService = distributedCacheService ?? throw new ArgumentNullException(nameof(distributedCacheService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
        public async Task<Result<Response<Domain.Basket>>> Handle(UpdateMembershipCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var basket = await _distributedCacheService.Get<Domain.Basket>(request.Id.ToString("N"));
                if (basket == null) { throw new ArgumentNullException(); }

                var updatedBasket = BasketMapper.UpdateLoyaltyDetails(request.LoyaltyDetails, basket);               

                await _distributedCacheService.Set(request.Id.ToString("N"), ValidateBasket.ValidateBasketStatus(updatedBasket), true, Environment.RedisExpiryTime, TimeSpan.Zero);

                var basketResponse = new Response<Domain.Basket>
                {
                    Status = Status.OK,
                    Value = updatedBasket
                };
                return Result.Success(basketResponse);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Correlation Id - {AsyncContext.Current.CorrelationId} - Exception - {ex}");
                var errorResponse = ErrorMapper.MapTechnicalExceptionToEAPIException
                                   (
                                       errorCode: Constants.ErrorCodes.UpdateBasketIDMissing,
                                       guid: AsyncContext.Current.CorrelationId.ToString(),
                                       errorFilePath: Infrastructure.Constants.Constants.ErrorCodesPath

                                   );
                return Result.Failure<Response<Domain.Basket>>(Json.Serialize(errorResponse));
            }
        }
    }
}
