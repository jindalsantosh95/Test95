using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using VAA.EAPI.Common;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Retail.Insurance.Domain;
using VAA.EAPI.Microservices.Retail.Insurance.Infrastructure.Services.OrderCreate;

namespace VAA.EAPI.Microservices.Retail.Insurance.Application.Commands
{
    public class CreateQuoteCommandHandler : IRequestHandler<CreateQuoteCommand, Result<Response<InsuranceQuoteResponse>>>
    {
        private readonly IMediator _mediator;
        private readonly ILogger<CreateQuoteCommandHandler> _logger;       

        public CreateQuoteCommandHandler(IMediator mediator, ILogger<CreateQuoteCommandHandler> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));           
        }

        public async Task<Result<Response<InsuranceQuoteResponse>>> Handle(CreateQuoteCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var quoteResponse = await _mediator.Send(new GetQuoteServieCommand { Request = request.Request }, cancellationToken);

                var quoteResult = new Response<InsuranceQuoteResponse>
                {
                    Status = EAPI.Domain.Entities.Common.Status.OK  ,
                    Value = quoteResponse
                };

                return Result.Success(quoteResult);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Correlation Id - {AsyncContext.Current.CorrelationId} - Exception - {ex}");               
                return Result.Failure<Response<InsuranceQuoteResponse>>((ex.InnerException ?? ex).ToString());
            }
        }
    }
}
