using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Infrastructure.Repository.EventStore;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Infrastructure.Repositories.CommonRefDB.Interface;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Mapper.CampaignRegister;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Validations;

namespace VAA.EAPI.Microservices.Loyalty.Campaign.Management.Application.Commands.CampaignRegister
{

    public class CampaignRegisterRequestCommandHandler : IRequestHandler<CampaignRegisterRequestCommand, Result<Response<string>>>
    {
        private readonly IMediator _mediator;
        private readonly ICommonRefDBRepository _commonRefDBRepository;
        private readonly ILogger<CampaignRegisterRequestCommandHandler> _logger;

        public CampaignRegisterRequestCommandHandler(IMediator mediator, ICommonRefDBRepository commonRefDBRepository, ILogger<CampaignRegisterRequestCommandHandler> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _commonRefDBRepository = commonRefDBRepository ?? throw new ArgumentNullException(nameof(commonRefDBRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        }
        public async Task<Result<Response<string>>> Handle(CampaignRegisterRequestCommand request, CancellationToken cancellationToken)
        {
            try
            {
                if (new CampaignRegisterRequestValidator().Validate(request).IsValid)
                {
                    var campaignData = await _commonRefDBRepository.GetCampaignDetails(request.CampaignId);
                    if (campaignData is not null)
                    {
                        var eventData = CampaignRegisterEventDataMapper.CampaignRegisterRequestToEventDataMapper(request, campaignData);
                        var cloudEvent = CampaignRegisterEventMapper.CommandToCampaignRegisterEventMapper(eventData);
                        _logger.LogDebug($"Store event in event store - Start");
                        await _mediator.Send(new EventRequestInsertCommand { Id = cloudEvent.Id, Type = cloudEvent.Type, Data = cloudEvent, Subject = cloudEvent.Subject });
                        _logger.LogDebug($"Store event in event store - End");
                        try
                        {
                            _logger.LogDebug($"Publish event - Start");
                            await _mediator.Publish(cloudEvent);
                            _logger.LogDebug($"Publish event - End");
                            var CampaignRegisterResponse = CampaignRegisterEventResponseMapper.CampaignRegisterResponseMapper(cloudEvent.Id, Convert.ToString(HttpStatusCode.Accepted));
                            return CampaignRegisterResponse != null ? Result.Success(CampaignRegisterResponse) : Result.Failure<Response<string>>(Convert.ToString(HttpStatusCode.InternalServerError));
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
                            Status = Status.BusinessException,
                            Value = null,
                            Errors = ErrorMapper.MapBusinessExceptionToEAPIException(new Exception(Constants.ErrorCodes.InvalidCampaignId), Guid.NewGuid().ToString(), VAA.EAPI.Infrastructure.Constants.Constants.ErrorCodesPath)
                        });
                    }

                }
                else
                {
                    return Result.Success(new Response<string>
                    {
                        Status = Status.BusinessException,
                        Value = null,
                        Errors = ErrorMapper.MapBusinessExceptionToEAPIException(new Exception(Constants.ErrorCodes.MandatoryInputFieldsMissing), Guid.NewGuid().ToString(), VAA.EAPI.Infrastructure.Constants.Constants.ErrorCodesPath)
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Rollback from event store for CorrelationId: {AsyncContext.Current.CorrelationId} due to error in CampaignRegisterRequestCommandHandler handler");
                return Result.Failure<Response<string>>(Json.Serialize(ErrorMapper.MapTechnicalExceptionToEAPIException(Constants.ErrorCodes.MiscellaneousTechnicalError, Guid.NewGuid().ToString(), VAA.EAPI.Infrastructure.Constants.Constants.ErrorCodesPath)));
            }
        }
    }
}