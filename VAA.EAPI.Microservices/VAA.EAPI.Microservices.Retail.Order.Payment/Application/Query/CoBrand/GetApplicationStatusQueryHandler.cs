using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Retail.Order.Payment.Domain.CoBrand;
using VAA.EAPI.Microservices.Retail.Order.Payment.Infrastructure.Services.CoBrand;


namespace VAA.EAPI.Microservices.Retail.Order.Payment.Application.Command.CoBrand
{
    public class GetApplicationStatusQueryHandler : IRequestHandler<GetApplicationStatusQuery, Result<Response<ApplicationStatusResponse>>>
    {
        private readonly IMediator _mediator;
        private readonly ILogger<GetApplicationStatusQueryHandler> _logger;

        public GetApplicationStatusQueryHandler(IMediator mediator, ILogger<GetApplicationStatusQueryHandler> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Result<Response<ApplicationStatusResponse>>> Handle(GetApplicationStatusQuery request, CancellationToken cancellationToken)
        {
            try
            {
                return await _mediator.Send(new ApplicationStatusServiceQuery { SynchronyToken = request.SynchronyToken }, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Correlation Id - {AsyncContext.Current.CorrelationId} - Exception - {ex}");
                return Result.Failure<Response<ApplicationStatusResponse>>((ex.InnerException ?? ex).ToString());
            }
        }
    }
}
