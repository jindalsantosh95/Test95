using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Infrastructure.Repository;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.BaseClasses;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.Base;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.Query.BaseClasses
{
    public abstract class CommandStatusBaseQueryHandler<T> where T : ICommandStatusQueryResponse
    {
        protected readonly IMediator _mediator;
        protected readonly ILogger<CommandStatusBaseQueryHandler<T>> _logger;

        public CommandStatusBaseQueryHandler(IMediator mediator, ILogger<CommandStatusBaseQueryHandler<T>> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public abstract Response<T> CreateResponseFromEvent(string requestId, string commandEventOutcome);

        public async Task<Result<Response<T>>> Handle(CommandStatusQueryBase request, CancellationToken cancellationToken)
        {
            try
            {
                var commandEventOutcome = await GetCommandStatusResponseAsync(request.RequestId);

                // Ran out of attempts to get the response record
                if (string.IsNullOrEmpty(commandEventOutcome))
                    return Result.Success(NotFoundResponse(request.RequestId));

                return Result.Success(CreateResponseFromEvent(request.RequestId, commandEventOutcome));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Unexpected error when checking Command Response Status\n", ex.StackTrace);
                return Result.Failure<Response<T>>(ex.Message);
            }
        }

        protected async Task<string> GetCommandStatusResponseAsync(string requestId)
        {
            string commandEventOutcome = string.Empty;

            int attemptCounter = 1;
            int responseDelayTimeInMS = ConfigReader.Get<int>(Constants.EnvironmentSettings.CommandStatusPollingDelayMS);
            int maxAttempts = ConfigReader.Get<int>(Constants.EnvironmentSettings.CommandStatusPollingRetryCount);

            while (string.IsNullOrEmpty(commandEventOutcome) && attemptCounter <= maxAttempts)
            {
                _logger.LogDebug($"Status request for {requestId} - Attempt  {attemptCounter}");
                await Task.Delay(TimeSpan.FromMilliseconds(responseDelayTimeInMS * (attemptCounter - 1)));

                ServiceResponse result = await _mediator.Send(new ServiceResponseQuery { CorrelationId = requestId, EventType = Constants.Application.CommandResponseStatusKey });

                switch (result.Status)
                {
                    case ServiceResponseProcessStatus.Completed:
                    case ServiceResponseProcessStatus.Error:
                        commandEventOutcome = result.JsonResponseData;
                        break;
                    default:
                        // Go round the loop again
                        break;
                }

                attemptCounter++;
            }

            return commandEventOutcome;
        }

        protected Response<T> NotFoundResponse(string requestId)
        {
            _logger.LogWarning($"Requested status event {requestId} could not be found.");
            return
                new Response<T>()
                {
                    Status = Status.BusinessException,
                    Errors = ErrorMapper.MapTechnicalExceptionToEAPIException(Constants.ErrorCodes.CommandResponseStatusNotReady, Guid.NewGuid().ToString(), EAPI.Infrastructure.Constants.Constants.ErrorCodesPath)
                };
        }

    }
}
