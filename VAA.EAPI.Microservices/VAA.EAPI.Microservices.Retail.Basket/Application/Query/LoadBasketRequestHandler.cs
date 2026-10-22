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

namespace VAA.EAPI.Microservices.Retail.Basket.Application.Query
{
    public sealed class LoadBasketRequestHandler : IRequestHandler<LoadBasketRequest, Result<Response<Domain.Basket>>>
    {
        private readonly IDistributedCacheService _distributedCacheService;
        private readonly ILogger<LoadBasketRequestHandler> _logger;

        public LoadBasketRequestHandler(IDistributedCacheService distributedCacheService, ILogger<LoadBasketRequestHandler> logger)
        {
            _distributedCacheService = distributedCacheService ?? throw new ArgumentNullException(nameof(distributedCacheService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
        public async Task<Result<Response<Domain.Basket>>> Handle(LoadBasketRequest request, CancellationToken cancellationToken)
        {
            try
            {
                var basket = await _distributedCacheService.Get<Domain.Basket>(request.Id.ToString("N"));
                if (basket == null) { throw new ArgumentNullException(); }
                var basketResponse = new Response<Domain.Basket>
                {
                    Status = basket == null ? Status.BadRequest : Status.OK,
                    Value = basket
                };
                return Result.Success(basketResponse);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Correlation Id - {AsyncContext.Current.CorrelationId} - Exception - {ex}");
                var errorResponse = ErrorMapper.MapTechnicalExceptionToEAPIException
                                   (
                                       errorCode: Constants.ErrorCodes.GetBasketIDMissing,
                                       guid: AsyncContext.Current.CorrelationId.ToString(),
                                       errorFilePath: VAA.EAPI.Infrastructure.Constants.Constants.ErrorCodesPath

                                   );
                return Result.Failure<Response<Domain.Basket>>(Json.Serialize(errorResponse));
            }
        }
    }
}
