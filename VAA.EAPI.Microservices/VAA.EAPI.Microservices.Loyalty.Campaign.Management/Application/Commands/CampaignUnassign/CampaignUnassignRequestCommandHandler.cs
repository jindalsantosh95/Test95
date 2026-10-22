using CSharpFunctionalExtensions;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Infrastructure.Repository.EventStore;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Domain.Entities.CampaignUnassign;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Mapper.CampaignUnassign;

namespace VAA.EAPI.Microservices.Loyalty.Campaign.Management.Application.Commands.CampaignUnassign
{
    public class CampaignUnassignRequestCommandHandler : IRequestHandler<CampaignUnassignRequestCommand, Result<Response<string>>>
    {
        private readonly IMediator _mediator;
        private readonly ILogger<CampaignUnassignRequestCommandHandler> _logger;
        private readonly IValidator<CampaignUnassignRequestData> _validator;

        public CampaignUnassignRequestCommandHandler(IMediator mediator, ILogger<CampaignUnassignRequestCommandHandler> logger, IValidator<CampaignUnassignRequestData> validator)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _validator = validator ?? throw new ArgumentNullException(nameof(validator));
        }

        public async Task<Result<Response<string>>> Handle(CampaignUnassignRequestCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Campaign Unassign Request Command Handler - Start");
            try
            {
                var campaignUnassignRequestValidator = await _validator.ValidateAsync(request);
                if (campaignUnassignRequestValidator.IsValid)
                {
                    TargetedMembershipDocDeleteEventData eventData = new TargetedMembershipDocDeleteEventData()
                    {
                        CustomerId = request.CustomerId,
                        TargetedMembershipDocument = new TargetedMembershipDeleteDocument()
                        {
                            Id = request.DocumentId,
                            CustomerId = request.CustomerId
                        }
                    };
                    var cloudEvent = CampaignUnassignCommandToEventMapper.CommandToEventMapper(eventData);
                    var reportingDbCloudEvent = CampaignUnassignCommandToEventMapper.CommandToEventMapperToDeleteFromReportingDb(eventData);

                    reportingDbCloudEvent.Id = cloudEvent.Id;

                    _logger.LogDebug($"Store event in event store - Start");
                    await _mediator.Send(new EventRequestInsertCommand
                    {
                        ConnectionString = ConfigReader.Get(Constants.Environment.CustomerTransactionMicroServiceDBKey),
                        TableName = ConfigReader.Get(Constants.Environment.CustomerTransactionMicroServiceEventStoreTableName),
                        Id = cloudEvent.Id,
                        Type = cloudEvent.Type,
                        Data = cloudEvent,
                        Subject = cloudEvent.Subject
                    });
                    _logger.LogDebug($"Store event in event store - End");
                    try
                    {
                        _logger.LogDebug($"Publish event - Start");
                        await _mediator.Publish(cloudEvent);
                        await _mediator.Publish(reportingDbCloudEvent);
                        _logger.LogDebug($"Publish event - End");
                        var campaignUnassignResponse = CampaignUnassignEventCommandResponseMapper.CampaignUnassignResponseMapper(cloudEvent.Id, Convert.ToString(HttpStatusCode.Accepted));
                        return campaignUnassignResponse != null ? Result.Success<Response<string>>(campaignUnassignResponse) : Result.Success(new Response<string>
                        {
                            Errors = ErrorMapper.MapBusinessExceptionToEAPIException(new Exception(Constants.ErrorCodes.InvalidCampaignId), Guid.NewGuid().ToString(), VAA.EAPI.Infrastructure.Constants.Constants.ErrorCodesPath),
                            Status = Status.BusinessException
                        });
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"Process failed due to Exception");
                        _logger.LogDebug($"Deleting the event from event store");
                        await _mediator.Send(new EventStoreRequestDeleteCommand { Id = cloudEvent.Id, Type = cloudEvent.Type, Data = cloudEvent.Data, Subject = cloudEvent.Subject }, cancellationToken);
                        throw;
                    }
                }
                else
                {
                    return Result.Success(new Response<string>
                    {
                        Errors = ErrorMapper.MapBusinessExceptionToEAPIException(new Exception(campaignUnassignRequestValidator.Errors.FirstOrDefault().ToString()), Guid.NewGuid().ToString(), VAA.EAPI.Infrastructure.Constants.Constants.ErrorCodesPath),
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
                _logger.LogInformation("Campaign Unassign Request Command Handler - End");
            }
        }
    }
}