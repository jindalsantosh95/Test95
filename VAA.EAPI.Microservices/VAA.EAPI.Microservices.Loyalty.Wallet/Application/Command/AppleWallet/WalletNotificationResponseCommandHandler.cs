using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Infrastructure.Repository;
using VAA.EAPI.Microservices.Loyalty.Wallet.Domain;

namespace VAA.EAPI.Microservices.Loyalty.Wallet.Application.Command.AppleWallet
{
    public class WalletNotificationResponseCommandHandler : IRequestHandler<WalletNotificationResponseCommand, Result<Response<WalletResponse>>>
    {
        private readonly IMediator _mediator;
        private readonly ILogger<WalletNotificationResponseCommandHandler> _logger;

        public WalletNotificationResponseCommandHandler(IMediator mediator, ILogger<WalletNotificationResponseCommandHandler> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
        public async Task<Result<Response<WalletResponse>>> Handle(WalletNotificationResponseCommand request, CancellationToken cancellationToken)
        {

            Response<WalletResponse> walletResponse = new();

            try
            {
                int retryCounter = 1;
                bool responseProcessed = false;
                int responseDelayTimeInMS = ConfigReader.Get<int>(Constants.Environment.ResponseDelayTimeInMS);

                while (!responseProcessed && retryCounter <= ConfigReader.Get<int>(Constants.Environment.ResponsePollingCount))
                {
                    var result = await _mediator.Send(new ServiceResponseQuery { CorrelationId = request.CorrelationId, EventType = Constants.Application.WalletNotification });
                    var timespan = TimeSpan.FromMilliseconds(responseDelayTimeInMS * retryCounter);

                    switch (result.Status)
                    {
                        case ServiceResponseProcessStatus.UpdatePending:
                        case ServiceResponseProcessStatus.Processing:
                            await Task.Delay(timespan);
                            break;
                        case ServiceResponseProcessStatus.Completed:
                            walletResponse = WalletResponseMapper(result); 
                            responseProcessed = true;
                            break;

                        case ServiceResponseProcessStatus.Error:
                            responseProcessed = true;
                            walletResponse = WalletResponseMapper(result);
                            break;
                    }
                    retryCounter++;
                }

                return walletResponse != null ? Result.Success(walletResponse) : Result.Failure<Response<WalletResponse>>(Json.Serialize(ErrorMapper.MapTechnicalExceptionToEAPIException(EAPI.Infrastructure.Constants.Constants.TechnicalError, Guid.NewGuid().ToString(), EAPI.Infrastructure.Constants.Constants.GlobalExceptionFilePath)));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error has occured while updating wallet notification response for CorrelationId:{request.CorrelationId}");

                return GetErrorResponse(EAPI.Infrastructure.Constants.Constants.TechnicalError);
            }
        }
        private static Response<WalletResponse> GetErrorResponse(string errorCode)
        {
            return new Response<WalletResponse>()
            {
                Status = Status.BusinessException,
                Errors = new()
                {
                    ErrorMapper.MapBusinessExceptionToEAPIException(new Exception(errorCode), Guid.NewGuid().ToString(), EAPI.Infrastructure.Constants.Constants.ErrorCodesPath).FirstOrDefault()
                }
            };
        }
        
        private static Response<WalletResponse> WalletResponseMapper(ServiceResponse serviceResponse)
        {
            return new Response<WalletResponse>
            {
                Status = Status.OK,
                Value = Json.Deserialize<WalletResponse>(serviceResponse.JsonResponseData)
            };
        }

    }


}
