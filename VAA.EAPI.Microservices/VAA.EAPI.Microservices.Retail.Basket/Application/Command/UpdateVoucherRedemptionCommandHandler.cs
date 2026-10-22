using CSharpFunctionalExtensions;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Caching;
using Environment = VAA.EAPI.Microservices.Retail.Basket.Constants.Environment;

namespace VAA.EAPI.Microservices.Retail.Basket.Application.Command
{
    public sealed class UpdateVoucherRedemptionCommandHandler : IRequestHandler<UpdateVoucherRedemptionCommand, Result<Response<Domain.Basket>>>
    {
        private readonly IDistributedCacheService _distributedCacheService;

        public UpdateVoucherRedemptionCommandHandler(IDistributedCacheService distributedCacheService) => _distributedCacheService = distributedCacheService ?? throw new ArgumentNullException(nameof(distributedCacheService));

        public async Task<Result<Response<Domain.Basket>>> Handle(UpdateVoucherRedemptionCommand request, CancellationToken cancellationToken)
        {
            var basket = await _distributedCacheService.Get<Domain.Basket>(request.Id.ToString("N"));
            basket.HasSufficientPoints = request.HasSufficientPoints;
            basket.VoucherPoints = request.VoucherPoints;
            basket.PointsToPay = request.PointsToPay;
            basket.VoucherApplied = request.VoucherApplied;
            basket.VoucherDetails = request.VoucherDetails;
            basket.VoucherRedemptionSelection = request.VoucherRedemptionSelection;

            await _distributedCacheService.Set(request.Id.ToString("N"), basket, true, Environment.RedisExpiryTime, TimeSpan.Zero);
            return new Response<Domain.Basket> { Status = Status.OK, Value = basket };
        }
    }
}
