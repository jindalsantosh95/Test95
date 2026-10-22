using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Events;
using VAA.EAPI.Infrastructure.MessageBroker.ServiceBusMessagingV2;
using VAA.EAPI.Infrastructure.Repository.EventStore;
using VAA.EAPI.Microservices.CRM.CustomerProfile.Domain.Entities;
using VAA.EAPI.Microservices.CRM.CustomerProfile.Domain.Entities.FlightOrderNotification;
using VAA.EAPI.Microservices.CRM.CustomerProfile.Infrastructure.Repositories;
using VAA.EAPI.Microservices.CRM.CustomerProfile.Mapper;

namespace VAA.EAPI.Microservices.CRM.CustomerProfile.Application.Events
{
    public class CustomerProfileEventCommandHandler : ICustomerProfileEventCommand
    {
        private readonly IAmperityApiRepoQuery _amperityApiRepoQueryHandler;
        private readonly ILogger<CustomerProfileEventCommandHandler> _logger;
        private readonly IRequestHandler<EventRequestInsertCommand, bool> _eventStoreRequestInsertCommandHandler;
        private readonly IRequestHandler<EventStoreRequestDeleteCommand, bool> _eventStoreRequestDeleteCommandHandler;
        private readonly IRequestHandler<ServiceBusTopicMessageEventCommand, Unit> _serviceBusTopicMessageEventCommandHandler;
        private readonly string _eventId;

        public CustomerProfileEventCommandHandler(IAmperityApiRepoQuery amperityApiRepoQueryHandler, IRequestHandler<EventRequestInsertCommand, bool> eventStoreRequestInsertCommandHandler, IRequestHandler<EventStoreRequestDeleteCommand, bool> eventStoreRequestDeleteCommandHandler, IRequestHandler<ServiceBusTopicMessageEventCommand, Unit> serviceBusTopicMessageEventCommandHandler, ILogger<CustomerProfileEventCommandHandler> logger)
        {
            _amperityApiRepoQueryHandler = amperityApiRepoQueryHandler ?? throw new ArgumentNullException(nameof(amperityApiRepoQueryHandler));
            _eventStoreRequestInsertCommandHandler = eventStoreRequestInsertCommandHandler ?? throw new ArgumentNullException(nameof(eventStoreRequestInsertCommandHandler));
            _eventStoreRequestDeleteCommandHandler = eventStoreRequestDeleteCommandHandler ?? throw new ArgumentNullException(nameof(eventStoreRequestDeleteCommandHandler));
            _serviceBusTopicMessageEventCommandHandler = serviceBusTopicMessageEventCommandHandler ?? throw new ArgumentNullException(nameof(serviceBusTopicMessageEventCommandHandler));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _eventId = Guid.NewGuid().ToString();
        }

        #region Development is in progress
        public async Task Handle(CloudEvent cloudEventReq, CancellationToken cancellationToken)
        {
            try
            {
                var flightOrders = Json.Deserialize<List<FlightOrderData>>(Convert.ToString(cloudEventReq.Data));

             #region Logic to be decided based on method param (Single / multiple customer) to get amperity id
                List<CustomerProfileRequest> request = flightOrders.SelectMany(flightOrder => flightOrder.Passengers, (flightOrder, passenger) => new CustomerProfileRequest()
                {
                    Passenger = passenger,
                    PnrLocator = flightOrder.BookingReference.PnrLocator
                }).ToList();

                foreach (var customer in request)
                {
                    var response = await _amperityApiRepoQueryHandler.Handle(customer, CancellationToken.None);

                    if (response.Status == EAPI.Domain.Entities.Common.Status.OK)
                    {
                        customer.Passenger.AmperityId = response.Value.AmperityId;
                        //logic to be included to get list of flightorderdata with AmperityID
                    }
                    else
                    {
                        throw new Exception("Exception in Amperity API layer");
                    }
                }
                #endregion

                var cloudEvent = CustomerProfileCloudEventMapper.EventMapper(flightOrders, _eventId);

                await _eventStoreRequestInsertCommandHandler.Handle(new EventRequestInsertCommand { Id = cloudEvent.Id, Type = cloudEvent.Type, Data = cloudEvent.Data, Subject = cloudEvent.Subject }, cancellationToken);

                await _serviceBusTopicMessageEventCommandHandler.Handle(new ServiceBusTopicMessageEventCommand { Message = cloudEvent, ServiceBusConnectionString = ConfigReader.Get(Constants.Environment.OrderServiceBusConnectionString), ServiceBusTopicName = ConfigReader.Get(Constants.Environment.CheckinNotificationTopicName) }, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Rollback event from Event store due to error in AmperityApiRepoQueryHandler: {ex.Message}");
                await _eventStoreRequestDeleteCommandHandler.Handle(new EventStoreRequestDeleteCommand { Id = _eventId, Type = Constants.Constants.CheckinCustProfileResEventSubject }, cancellationToken);
            }
        }
        #endregion
    }

}

