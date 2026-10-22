using MediatR;
using Microsoft.Extensions.Logging;
using System;
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
    public class AddressLookupEventCommand : CloudEvent, IRequest<bool>
    {

    }
    public class AddresLookupEventCommandHandler : IRequestHandler<AddressLookupEventCommand, bool>
    {
        private readonly ILogger<AddresLookupEventCommandHandler> _logger;
        private readonly IMediator _mediator;

        public AddresLookupEventCommandHandler(ILogger<AddresLookupEventCommandHandler> logger, IMediator mediator)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }

        public async Task<bool> Handle(AddressLookupEventCommand request, CancellationToken cancellationToken)
        {
            bool status = false;
            try
            {
                var req = Json.Deserialize<CreateCustomerEntities>(Convert.ToString(request.Data));
                //Store to event store and publish Event to Address MS
                var eventData = AddressLookupEventMapper.AddressEventMapper(request);
                await _mediator.Send(new EventRequestInsertCommand { Id = request.Id, Type = eventData.Type, Data = eventData.Data, Subject = Constants.Application.CreateCustomerEventSubject }, cancellationToken);
                status = true;
                await _mediator.Publish(eventData, CancellationToken.None);
                return true;
            }

            catch (Exception)
            {
                if (status)
                    await _mediator.Send(new EventStoreRequestDeleteCommand { Id = request.Id, Type = ConfigReader.Get(Constants.Environment.EventTypeAddressLookup), Data = request.Data, Subject = Constants.Application.CreateCustomerEventSubject }, cancellationToken);

                return false;
            }
        }
    }
}
