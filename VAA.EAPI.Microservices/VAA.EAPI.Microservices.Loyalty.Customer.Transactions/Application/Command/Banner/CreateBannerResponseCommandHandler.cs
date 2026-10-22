using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Infrastructure.Repository;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.Command.Banner
{
    public class CreateBannerResponseCommandHandler : IRequestHandler<CreateBannerResponseCommand, Result<Response<string>>>
    {
        private readonly IMediator _mediator;
        private readonly ILogger<CreateBannerResponseCommandHandler> _logger;

        public CreateBannerResponseCommandHandler(IMediator mediator, ILogger<CreateBannerResponseCommandHandler> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Result<Response<string>>> Handle(CreateBannerResponseCommand request, CancellationToken cancellationToken)
        {

            Response<string> bannerResponse = new();

            try
            {
                int retryCounter = 1;
                bool responseProcessed = false;
                int responseDelayTimeInMS = ConfigReader.Get<int>(Constants.EnvironmentSettings.ResponseDelayTimeInMS);

                while (!responseProcessed && retryCounter <= ConfigReader.Get<int>(Constants.EnvironmentSettings.ResponsePollingCount))
                {
                    var result = await _mediator.Send(new ServiceResponseQuery { CorrelationId = request.CorrelationId, EventType = Constants.Application.CommandResponseStatusKey });
                    var timespan = TimeSpan.FromMilliseconds(responseDelayTimeInMS * retryCounter);

                    switch (result.Status)
                    {
                        case ServiceResponseProcessStatus.UpdatePending:
                        case ServiceResponseProcessStatus.Processing:
                            await Task.Delay(timespan);
                            break;

                        case ServiceResponseProcessStatus.Completed:
                            bannerResponse = BannerResponseMapper();
                            responseProcessed = true;
                            break;

                        case ServiceResponseProcessStatus.Error:
                            responseProcessed = true;
                            bannerResponse = GetErrorResponse(Constants.ErrorCodes.MiscellaneousTechnicalError);
                            break;
                    }
                    retryCounter++;
                }

                return bannerResponse != null ? Result.Success(bannerResponse) : Result.Failure<Response<string>>(Json.Serialize(ErrorMapper.MapTechnicalExceptionToEAPIException(EAPI.Infrastructure.Constants.Constants.TechnicalError, Guid.NewGuid().ToString(), EAPI.Infrastructure.Constants.Constants.GlobalExceptionFilePath)));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error has occured while creating banner response for CorrelationId:{request.CorrelationId}");

                return GetErrorResponse(Constants.ErrorCodes.MiscellaneousTechnicalError);
            }
        }

        private static Response<string> GetErrorResponse(string errorCode)
        {
            var errorList = new List<ErrorDetails>
            {
                ErrorMapper.MapBusinessExceptionToEAPIException(new Exception(errorCode), Guid.NewGuid().ToString(), EAPI.Infrastructure.Constants.Constants.ErrorCodesPath).FirstOrDefault()
            };

            return new Response<string>
            {
                Status = Status.BusinessException,
                Errors = errorList
            };
        }

        private static Response<string> BannerResponseMapper()
        {
                return new Response<string>
                {
                    Status = Status.OK,
                    Value = "Banner Created Successfully."
                };
        }
    }
}
