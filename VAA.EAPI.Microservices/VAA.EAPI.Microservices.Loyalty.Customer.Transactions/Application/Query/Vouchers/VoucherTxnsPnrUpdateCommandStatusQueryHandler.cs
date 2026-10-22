using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Repository;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.Vouchers;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Infrastructure.Entities;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.Query.Vouchers
{
    public class VoucherTxnsPnrUpdateCommandStatusQueryHandler : IVoucherTxnsPnrUpdateCommandStatusQuery
    {
        private readonly ServiceResponseQueryHandler _serviceResponseQueryHandler;
        private readonly ILogger<VoucherTxnsPnrUpdateCommandStatusQueryHandler> _logger;

        public VoucherTxnsPnrUpdateCommandStatusQueryHandler(ServiceResponseQueryHandler serviceResponseQueryHandler, ILogger<VoucherTxnsPnrUpdateCommandStatusQueryHandler> logger)
        {
            _serviceResponseQueryHandler = serviceResponseQueryHandler ?? throw new ArgumentNullException(nameof(serviceResponseQueryHandler));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Result<Response<VoucherTxnsPnrUpdateCommandQueryResponse>>> Handle(VoucherTxnsPnrUpdateCommandStatusQuery request, CancellationToken cancellationToken)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            try
            {
                var commandEventOutcome = await GetCommandStatusResponse(request.RequestId, cancellationToken);

                if (string.IsNullOrEmpty(commandEventOutcome))
                    return Result.Failure<Response<VoucherTxnsPnrUpdateCommandQueryResponse>>("No response found for the requested ID " + request.RequestId);


                var eventResponse = Json.Deserialize<Result<Response<IEnumerable<TransactionDTO>>>>(commandEventOutcome);

                var result = Result.Success(new Response<VoucherTxnsPnrUpdateCommandQueryResponse>
                {
                    Status = Status.OK,
                    Value = new VoucherTxnsPnrUpdateCommandQueryResponse
                    {
                        RequestId = request.RequestId,
                        CommandStatus = eventResponse.IsSuccess && eventResponse.Value?.Status == Status.OK ? Status.OK : Status.BusinessException,
                        Transactions = eventResponse.IsSuccess ? eventResponse.Value?.Value : null,
                        Errors = eventResponse.IsSuccess ? eventResponse.Value?.Errors : new List<ErrorDetails> { new ErrorDetails { Message = eventResponse.Error, ErrorSource = "Consuming application", Code = Constants.ErrorCodes.VoucherTxnPnrUpdateCommandStatusError } }
                    }
                });

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Unexpected error when checking Command Response Status");
                return Result.Failure<Response<VoucherTxnsPnrUpdateCommandQueryResponse>>(ex.Message);
            }
        }

        private async Task<string> GetCommandStatusResponse(string requestId, CancellationToken cancellationToken)
        {
            string commandEventOutcome = string.Empty;

            int attemptCounter = 1;
            int responseDelayTimeInMS = ConfigReader.Get<int>(Constants.EnvironmentSettings.CommandStatusPollingDelayMS);
            int maxAttempts = ConfigReader.Get<int>(Constants.EnvironmentSettings.CommandStatusPollingRetryCount);

            while (string.IsNullOrEmpty(commandEventOutcome) && attemptCounter <= maxAttempts)
            {
                _logger.LogDebug($"Status request for {requestId} - Attempt  {attemptCounter}");
                await Task.Delay(TimeSpan.FromMilliseconds(responseDelayTimeInMS * (attemptCounter - 1)), cancellationToken);

                ServiceResponse result = await _serviceResponseQueryHandler.Handle(new ServiceResponseQuery { CorrelationId = requestId, EventType = Constants.Application.VoucherRedemptionCommandResponseStatusKey }, cancellationToken);

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
    }
}
