using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Infrastructure.MessageBroker.ServiceBusMessagingV2;
using VAA.EAPI.Infrastructure.Repository.EventStore;
using VAA.EAPI.Microservices.Retail.Order.Mapper.OrderCreate;

namespace VAA.EAPI.Microservices.Retail.Order.Application.Commands.Insurance
{
    public class InsurancePurchaseCommandHandler : IRequestHandler<InsurancePurchaseCommand, bool>
    {
        private readonly IMediator _mediator;
        private readonly ILogger<InsurancePurchaseCommandHandler> _logger;        

        public InsurancePurchaseCommandHandler(IMediator mediator, ILogger<InsurancePurchaseCommandHandler> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));            
        }

        public async Task<bool> Handle(InsurancePurchaseCommand request, CancellationToken cancellationToken)
        {
            try
            {   
                var cloudEvent = OrderCreateCommandToInsuranceEventMapper.MapOrderToInsurancePurchaseRequest(request.InsurancePurchaseRequest);

                await _mediator.Send(new EventRequestInsertCommand { Id = cloudEvent.Id, Type = Constants.Application.InsurancePurchaseEventType, Data = cloudEvent.Data, Subject = cloudEvent.Subject }, cancellationToken);

                await _mediator.Send(new ServiceBusTopicMessageEventCommand { Message = cloudEvent, ServiceBusConnectionString = ConfigReader.Get(Constants.Environment.ServiceBusConnectionString), ServiceBusTopicName = ConfigReader.Get(Constants.Environment.ServiceBusTopicNameForInsurancePurchase) }, cancellationToken);
                 
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return false;
            }
        }
    }
}
