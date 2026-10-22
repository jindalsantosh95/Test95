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
using VAA.EAPI.Domain.Events;
using VAA.EAPI.Infrastructure.Repository.EventStore;
using VAA.EAPI.Infrastructure.Services.Analytics;
using VAA.EAPI.Microservices.Loyalty.Identity.Domain;
using VAA.EAPI.Microservices.Loyalty.Identity.Domain.RegisterToB2C;
using VAA.EAPI.Microservices.Loyalty.Identity.Infrastructure.Mapper;
using VAA.EAPI.Microservices.Loyalty.Identity.Infrastructure.Repositories;

namespace VAA.EAPI.Microservices.Loyalty.Identity.Application.Command
{
    public class AccountCreationCommandHandler : IRequestHandler<AccountCreationCommand, bool>
    {
        private readonly IMediator _mediator;
        private readonly ILogger<AccountCreationCommandHandler> _logger;
        private readonly ITrackAnalyticsEventCommandHandler _trackAnalyticsEventCommandHandler;

        public AccountCreationCommandHandler(
            IMediator mediator,
            ILogger<AccountCreationCommandHandler> logger,
            ITrackAnalyticsEventCommandHandler trackAnalyticsEventCommandHandler)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _trackAnalyticsEventCommandHandler = trackAnalyticsEventCommandHandler ?? throw new ArgumentNullException(nameof(trackAnalyticsEventCommandHandler));
        }

        public async Task<bool> Handle(AccountCreationCommand request, CancellationToken cancellationToken)
        {
            var trackAnalyticsEventCommand = new TrackAnalyticsEventCommand
            {
                Type = nameof(TrackingAnalyticsType.Microservice),
                Subject = "AccountCreation"
            };

            try
            {
                var createCustomerEntities = Json.Deserialize<CreateCustomerEntities>(Convert.ToString(request.Data));
                var correlationIdForEvents = Guid.NewGuid().ToString();

                B2CUserResponse response = await _mediator.Send(new RegisterRepoCommand((MemberRegisterRequest)createCustomerEntities), cancellationToken);
                 
                if(response.IsB2COperationSuccess)
                {
                    await PublishToVaAccEmail(AccountCreationEmailTriggerEventMapper.EmailTriggerEventMapper(createCustomerEntities, correlationIdForEvents), correlationIdForEvents, cancellationToken);
                }

                trackAnalyticsEventCommand.Track("AccountCreated", response.IsB2COperationSuccess);
            }                   
            catch(Exception ex)
            {
                _logger.LogError(ex,"AzureB2C Creation Failed");
                return false;
            }
            finally
            {
                await _trackAnalyticsEventCommandHandler.Handle(trackAnalyticsEventCommand, cancellationToken);
            }

            return true;
        }

        private async Task PublishToVaAccEmail(CloudEvent VaAccEmailSyncCloudEvent, string correlationIdForEvents, CancellationToken cancellationToken)
        {
            await _mediator.Send(new EventRequestInsertCommand { Id = correlationIdForEvents, Type = Constants.Application.VaAccEmailEvent, Data = VaAccEmailSyncCloudEvent.Data, Subject = VaAccEmailSyncCloudEvent.Subject }, cancellationToken);

            try
            {
                await _mediator.Publish(VaAccEmailSyncCloudEvent, cancellationToken);
            }
            catch (Exception)
            {
                await _mediator.Send(new EventStoreRequestDeleteCommand { Id = correlationIdForEvents, Type = Constants.Application.VaAccEmailEvent, Data = VaAccEmailSyncCloudEvent.Data, Subject = VaAccEmailSyncCloudEvent.Subject }, cancellationToken);
                throw;
            }
        }
    }
}
