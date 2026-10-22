using CSharpFunctionalExtensions;
using MediatR;
using System;
using VAA.EAPI.Common;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Retail.Order.Payment.Domain.CoBrand;
using VAA.EAPI.Microservices.Retail.Order.Payment.Infrastructure.Services.CoBrand;
using Microsoft.Extensions.Logging;

namespace VAA.EAPI.Microservices.Retail.Order.Payment.Application.Command.CoBrand
{
    public class GenerateSynchronyTokenCommandHandler : IRequestHandler<GenerateSynchronyTokenCommand, Result<Response<SynchronyTokenResponse>>>
    {
        private readonly IMediator _mediator;
        private readonly ILogger<GenerateSynchronyTokenCommandHandler> _logger;

        public GenerateSynchronyTokenCommandHandler(IMediator mediator, ILogger<GenerateSynchronyTokenCommandHandler> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Result<Response<SynchronyTokenResponse>>> Handle(GenerateSynchronyTokenCommand request, CancellationToken cancellationToken)
        {
            try
            {
                return await _mediator.Send(new GenerateSynchronyTokenServiceCommand { ApplicantDetails = request.TokenRequest.ApplicantDetails, AccountInfo = request.TokenRequest.AccountInfo, InitialPurchaseAmount = request.TokenRequest.InitialPurchaseAmount }, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Correlation Id - {AsyncContext.Current.CorrelationId} - Exception - {ex}");
                return Result.Failure<Response<SynchronyTokenResponse>>((ex.InnerException ?? ex).ToString());
            }
        }
    }
}
