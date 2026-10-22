using MediatR;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Events;
using VAA.EAPI.Microservices.Loyalty.Customer.Domain.Entities;
using VAA.EAPI.Microservices.Loyalty.Customer.Domain.Enums;
using VAA.EAPI.Microservices.Loyalty.Customer.Mapper.EnrolmentCustomer;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Triggers.ServiceBus
{
    public class CustomerMessageSubscriber
    {
        private readonly IMediator _mediator;

        public CustomerMessageSubscriber(IMediator mediator)
        {
            _mediator = mediator;
        }

        [Function("EnrolCustomerSubscriber")]
        public async Task Run([ServiceBusTrigger(Constants.Application.AddressEventHandlerTopicName, Constants.Application.AddressEventSubscriptionName, Connection = "ServiceBusConnectionString")] string eventRequestPayload, FunctionContext context)
        {
            var log = context.GetLogger("HttpFunction");
            log.LogInformation("Customer event received");
            var cloudEvent = Json.Deserialize<CloudEvent>(eventRequestPayload);
            VAA.EAPI.Common.AsyncContext.Current?.MapFromCloudEvent(cloudEvent);
            var request = CustomerCommandRequestMapper.EventRequestMapper(cloudEvent);
            var req = Json.Deserialize<CreateCustomerEntities>(Convert.ToString(request.Data));

            if (req.CustomerType == CustomerTypeEnum.Basic)
            {
                log.LogInformation($"Send request to basic handler for channel: {req.EnrollmentChannel}, member: {req?.FcNumber} & customerId:{req?.CustomerId}");
                var basicEnrolmentRequest = BasicEnrolmentMapper.BasicEventMapper(cloudEvent);
                await _mediator.Send(basicEnrolmentRequest);
            }

            else
            {
                string countryCode = req.AddressDetails?.FirstOrDefault(x => x.PreferredFlag.Is("Y"))?.CountryCode;
                if (req.EnrollmentChannel == Constants.Application.PowerAppChannelId || req.EnrollmentChannel == Constants.Application.DeltaChannelID || req.EnrollmentChannel == Constants.Application.WebMyaChannelID || req.EnrollmentChannel == ConfigReader.Get(Constants.Environment.AppMyaChannelId) || countryCode != Convert.ToString(Country.GB) && countryCode != Convert.ToString(Country.US))
                {
                    log.LogInformation($"Send request to handler for channel: {req.EnrollmentChannel}");
                    await _mediator.Send(request);
                }
                else
                {
                    log.LogInformation($"Publish address MS event for channel: {req.EnrollmentChannel}");
                    var addressLookuprequest = AddressLookupEventMapper.CustomerEventDataMapper(cloudEvent);
                    await _mediator.Send(addressLookuprequest);
                }
            }
        }

        [Function("EnrolCustomerWithVerifiedAddressSubscriber")]
        public async Task RunAsync([ServiceBusTrigger(Constants.Application.CustomerEventHandlerTopicName, Constants.Application.CustomerEventSubscriptionName, Connection = "ServiceBusConnectionString")] string eventRequestPayload, FunctionContext context)
        {
            var log = context.GetLogger("CustomerMessageSubscriber");
            log.LogInformation("Customer Event received");
            var cloudEvent = Json.Deserialize<CloudEvent>(eventRequestPayload);
            VAA.EAPI.Common.AsyncContext.Current?.MapFromCloudEvent(cloudEvent);
            var req = CustomerCommandRequestMapper.EventRequestMapper(cloudEvent);
            await _mediator.Send(req);
        }

        [Function("EnrolCustomerForWebMyaSubscriber")]
        public async Task EnrolCustomerForWebMyaSubscriber([ServiceBusTrigger(Constants.Application.CustomerEventHandlerTopicName, Constants.Application.EnrolmentCustomerEventSubscriptionNAme, Connection = "ServiceBusConnectionString")] string eventRequestPayload, FunctionContext context)
        {
            var log = context.GetLogger("EnrolCustomerForWebMyaSubscriber");
            log.LogInformation("Customer Enrolment Event received");
            var cloudEvent = Json.Deserialize<CloudEvent>(eventRequestPayload);
            VAA.EAPI.Common.AsyncContext.Current?.MapFromCloudEvent(cloudEvent);
            await _mediator.Send(WebMYACommandRequestMapper.EventRequestMapper(cloudEvent));
        }
    }
}
