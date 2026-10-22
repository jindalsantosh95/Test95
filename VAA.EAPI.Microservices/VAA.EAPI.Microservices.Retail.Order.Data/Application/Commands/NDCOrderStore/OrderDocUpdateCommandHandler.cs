using CSharpFunctionalExtensions;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Infrastructure.MessageBroker.ServiceBusMessagingV2;
using VAA.EAPI.Microservices.Retail.Order.Data.Mapper;

namespace VAA.EAPI.Microservices.Retail.Order.Data.Application.Commands.NDCOrderStore
{
    public class OrderDocUpdateCommandHandler(IMediator mediator) : IRequestHandler<OrderDocUpdateCommand, Result<Response<string>>>
    {
        private readonly IMediator _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        private readonly string _correlationId = AsyncContext.Current.CorrelationId.ToString();

        public async Task<Result<Response<string>>> Handle(OrderDocUpdateCommand request, CancellationToken cancellationToken)
        {
            try 
            { 
                var cloudEvent = OrderDocUpdateEventMapper.EventMapper(request.OrderDocument, _correlationId);

                await _mediator.Send(new ServiceBusTopicMessageEventCommand { Message = cloudEvent, ServiceBusTopicName = Constants.Environment.NDCOrderStoreEventHandlerTopicName, ServiceBusConnectionString = ConfigReader.Get(Constants.Environment.OrderServiceBusConnectionString) }, cancellationToken);
            }
            catch
            {
                return Result.Failure<Response<string>>(Json.Serialize(ErrorMapper.MapTechnicalExceptionToEAPIException(EAPI.Infrastructure.Constants.Constants.TechnicalError, _correlationId, EAPI.Infrastructure.Constants.Constants.GlobalExceptionFilePath)));
            }

            return new Response<string>() { Status = Status.Accepted };
        }
    }
}
