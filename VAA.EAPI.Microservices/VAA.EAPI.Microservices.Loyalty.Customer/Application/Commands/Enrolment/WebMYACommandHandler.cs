using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Events;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Infrastructure.Repository.EventStore;
using VAA.EAPI.Microservices.Loyalty.Customer.Domain.Entities;
using VAA.EAPI.Microservices.Loyalty.Customer.Mapper.EnrolmentCustomer;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Application.Commands.Enrolment
{

    public class WebMYACommandHandler : IRequestHandler<WebMYACommand, bool>
    {
        private readonly ILogger<WebMYACommandHandler> _logger;
        private readonly IMediator _mediator;
        private bool microserviceEvent;

        public WebMYACommandHandler(ILogger<WebMYACommandHandler> logger, IMediator mediator)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _mediator = mediator;
        }
        public async Task<bool> Handle(WebMYACommand request, CancellationToken cancellationToken)
        {
            var req = Json.Deserialize<CreateCustomerEntities>(Convert.ToString(request.Data));
            var customerDetails = CreateCustomerEventEntitiesMapper.CustomerEventResponseMapper(req, null);
            var retry = ConfigReader.Get<int>(Constants.Environment.RetryCount);

            if (ConfigReader.Get<bool>(Constants.Environment.VredEnableSignDate))
            {
                customerDetails.MarketingPreferencesSignedAt = customerDetails.TermsAndConditionsSignedAt = DateTime.UtcNow.ToString(Constants.Application.ISODateTimeFormat);
                var preference = req.Preferences?.FirstOrDefault(item => item.PreferenceType == Preferences.PreferenceTypes.VAPartnersMarketingPermissions);
                customerDetails.MarketingAccepted = !string.IsNullOrEmpty(preference?.PreferenceValue) && string.Equals(Constants.Application.TrueFlag, preference?.PreferenceValue, StringComparison.OrdinalIgnoreCase);
            }
            var eventData = CreateCustomerEventDataMapper.CustomerEventDataMapper(customerDetails, req, request.Id);

            bool publishFailsCount = false;
            for (int i = 0; i < retry; i++)
            {
                try
                {
                    await publishVREDandOCDBSync(request, customerDetails, eventData, cancellationToken);
                    publishFailsCount = true;
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"error occurred while publishing VRED and ocdb sync ");
                }
            }
            return publishFailsCount;
        }
        private async Task publishVREDandOCDBSync(WebMYACommand request, CreateCustomerEventResponse customerDetails, CloudEvent eventData, CancellationToken cancellationToken)
        {
            await _mediator.Send(new EventRequestInsertCommand { Id = eventData.Id, Type = eventData.Type, Data = eventData.Data, Subject = eventData.Subject });

            try
            {
                await _mediator.Publish(eventData, cancellationToken);

            }
            catch
            {
                await _mediator.Send(new EventStoreRequestDeleteCommand { Id = request.Id, Type = ConfigReader.Get(Constants.Environment.EventTypeCustomerSync), Data = customerDetails, Subject = Constants.Application.CreateCustomerEventSubject }, cancellationToken);
                throw;
            }
        }
    }

}
