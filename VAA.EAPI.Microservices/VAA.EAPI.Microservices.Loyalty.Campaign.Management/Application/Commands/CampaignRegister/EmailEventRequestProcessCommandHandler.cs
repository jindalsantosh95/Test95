using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Infrastructure.Repository.EventStore;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Domain.Entities.CampaignRegister;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Domain.Enums;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Helper;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Mapper.CampaignRegister;

namespace VAA.EAPI.Microservices.Loyalty.Campaign.Management.Application.Commands.CampaignRegister
{
    public class EmailEventRequestProcessCommandHandler : IRequestHandler<EmailEventRequestProcessCommand, Result<Response<string>>>
    {
        private readonly IMediator _mediator;
        private readonly ILogger<EmailEventRequestProcessCommandHandler> _logger;

        public EmailEventRequestProcessCommandHandler(IMediator mediator, ILogger<EmailEventRequestProcessCommandHandler> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Result<Response<string>>> Handle(EmailEventRequestProcessCommand request, CancellationToken cancellationToken)
        {
            try
            {
                EmailTriggeringEventRequest eventData = new EmailTriggeringEventRequest();
                var registerEventData = Json.Deserialize<CampaignRegisterEventData>(Convert.ToString(request.Data));
                string corporateCampaignTypes = EnumValidator.GetEnumValueByDescription<CorporateCampaignTypes>(registerEventData.CampaignType).ToString();

                if (Enum.IsDefined(typeof(CorporateCampaignTypes), corporateCampaignTypes) && registerEventData.DocumentStatus == Constants.Application.EnrolDocStatus)
                {
                    eventData = EmailTriggeringEventRequestMapper.CreateEventGridSchemaData(registerEventData);
                    var cloudEvent = EmailTriggeringEventGridMapper.EventDataMapper(eventData);

                    await _mediator.Send(new EventRequestInsertCommand { Id = cloudEvent.Id, Type = cloudEvent.Type, Data = cloudEvent, Subject = cloudEvent.Subject });

                    try
                    {
                        await _mediator.Publish(cloudEvent, CancellationToken.None);
                    }
                    catch
                    {
                        await _mediator.Send(new EventStoreRequestDeleteCommand { Id = cloudEvent.Id, Type = cloudEvent.Type, Data = cloudEvent.Data, Subject = cloudEvent.Subject }, cancellationToken);
                        throw;
                    }

                    return Result.Success(new Response<string>
                    {
                        Status = Status.OK,
                        Value = Constants.Application.EmailSentMessage
                    });
                }
                else
                {
                    return Result.Success(new Response<string>
                    {
                        Status = Status.BusinessException,
                        Value = null,
                        Errors = ErrorMapper.MapBusinessExceptionToEAPIException(new Exception(Constants.ErrorCodes.CampaignNotEligibleForEmail), Guid.NewGuid().ToString(), VAA.EAPI.Infrastructure.Constants.Constants.ErrorCodesPath)
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Email Triggering in CampaignRegistration failed due to Exception");
                return Result.Failure<Response<string>>(Json.Serialize(ErrorMapper.MapTechnicalExceptionToEAPIException(Constants.ErrorCodes.MiscellaneousTechnicalError, Guid.NewGuid().ToString(), VAA.EAPI.Infrastructure.Constants.Constants.ErrorCodesPath)));
            }
        }
    }
}
