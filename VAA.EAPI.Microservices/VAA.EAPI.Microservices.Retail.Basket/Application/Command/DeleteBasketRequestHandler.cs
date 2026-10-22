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

namespace VAA.EAPI.Microservices.Retail.Basket.Application.Command
{
    public sealed class DeleteBasketRequestHandler : IRequestHandler<DeleteBasketRequest, Result<Response<bool>>>
    {
        private readonly IDistributedCacheService _distributedCacheService;
        private readonly ILogger<DeleteBasketRequestHandler> _logger;

        public DeleteBasketRequestHandler(IDistributedCacheService distributedCacheService, ILogger<DeleteBasketRequestHandler> logger)
        {
            _distributedCacheService = distributedCacheService ?? throw new ArgumentNullException(nameof(distributedCacheService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
        public async Task<Result<Response<bool>>> Handle(DeleteBasketRequest request, CancellationToken cancellationToken)
        {
            try
            {
                var basket = await _distributedCacheService.Get<Domain.Basket>(request.Id.ToString("N"));
                if (basket == null) { throw new ArgumentNullException(); }

                await _distributedCacheService.Remove(request.Id.ToString("N"));

                return new Response<bool>
                {
                    Status = Status.OK,
                    Value = (await _distributedCacheService.Get<Domain.Basket>(request.Id.ToString("N"))) == null
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Correlation Id - {AsyncContext.Current.CorrelationId} - Exception - {ex}");
                var errorResponse = ErrorMapper.MapTechnicalExceptionToEAPIException
                                   (
                                       errorCode: VAA.EAPI.Microservices.Retail.Basket.Constants.ErrorCodes.DeleteBasketIDMissing,
                                       guid: AsyncContext.Current.CorrelationId.ToString(),
                                       errorFilePath: VAA.EAPI.Infrastructure.Constants.Constants.ErrorCodesPath

                                   );
                return Result.Failure<Response<bool>>(Json.Serialize(errorResponse));
            }
        }
    }
}

