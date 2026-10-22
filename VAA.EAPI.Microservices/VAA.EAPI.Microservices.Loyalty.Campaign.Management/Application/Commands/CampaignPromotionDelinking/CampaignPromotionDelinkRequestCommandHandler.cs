using CSharpFunctionalExtensions;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Infrastructure.Repository.EventStore;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Domain.Entities.CampaignPromotionDelinking;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Mapper.CampaignPromotionDelinking;

namespace VAA.EAPI.Microservices.Loyalty.Campaign.Management.Application.Commands.CampaignPromotionDelinking
{
    public class CampaignPromotionDelinkRequestCommandHandler : IRequestHandler<CampaignPromotionDelinkRequestCommand, Result<Response<string>>>
    {
        private readonly IMediator _mediator;
        private readonly ILogger<CampaignPromotionDelinkRequestCommandHandler> _logger;
        private readonly IValidator<CampaignPromotionDelinkRequestData> _validator;

        public CampaignPromotionDelinkRequestCommandHandler(IMediator mediator, ILogger<CampaignPromotionDelinkRequestCommandHandler> logger, IValidator<CampaignPromotionDelinkRequestData> validator)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _validator = validator ?? throw new ArgumentNullException(nameof(validator));
        }
        public async Task<Result<Response<string>>> Handle(CampaignPromotionDelinkRequestCommand request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("CampaignPromotionDelinkRequestCommandHandler started");
                if (_validator.Validate(request).IsValid)
                {
                    var cloudEvent = CampaignPromotionDelinkCommandToEventMapper.CommandToEventMapper(request);
                    _logger.LogDebug($"Store event in event store - Start");
                    await _mediator.Send(new EventRequestInsertCommand { Id = cloudEvent.Id, Type = cloudEvent.Type, Data = cloudEvent.Data, Subject = cloudEvent.Subject });
                    _logger.LogDebug($"Store event in event store - End");
                    try
                    {
                        _logger.LogDebug($"Publish event - Start");
                        await _mediator.Publish(cloudEvent);
                        _logger.LogDebug($"Publish event - End");
                        var campaignPromotionDelinkResponse = CampaignPromotionDelinkEventResponseMapper.CampaignPromotionDelinkResponseMapper(cloudEvent.Id, Convert.ToString(HttpStatusCode.Accepted));
                        return campaignPromotionDelinkResponse != null ? Result.Success<Response<string>>(campaignPromotionDelinkResponse) : Result.Success(new Response<string>
                        {
                            Errors = ErrorMapper.MapBusinessExceptionToEAPIException(new Exception(Constants.ErrorCodes.InvalidCampaignId), Guid.NewGuid().ToString(), VAA.EAPI.Infrastructure.Constants.Constants.ErrorCodesPath),
                            Status = Status.BusinessException
                        });
                    }
                    catch
                    {
                        _logger.LogDebug($"Deleting the event from event store");
                        await _mediator.Send(new EventStoreRequestDeleteCommand { Id = cloudEvent.Id, Type = cloudEvent.Type, Data = cloudEvent.Data, Subject = cloudEvent.Subject }, cancellationToken);
                        throw;
                    }
                }
                else
                {
                    return Result.Success(new Response<string>
                    {
                        Errors = ErrorMapper.MapBusinessExceptionToEAPIException(new Exception(Constants.ErrorCodes.MandatoryInputFieldsMissing), Guid.NewGuid().ToString(), VAA.EAPI.Infrastructure.Constants.Constants.ErrorCodesPath),
                        Status = Status.BusinessException
                    });
                }

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Process failed due to Exception");
                return Result.Failure<Response<string>>(Json.Serialize(ErrorMapper.MapTechnicalExceptionToEAPIException(Constants.ErrorCodes.MiscellaneousTechnicalError, Guid.NewGuid().ToString(), VAA.EAPI.Infrastructure.Constants.Constants.ErrorCodesPath)));

            }
            finally
            {
                _logger.LogInformation("CampaignPromotionDelinkRequestCommandHandler completed");
            }
        }
    }
}
