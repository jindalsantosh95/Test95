using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Infrastructure.MessageBroker.ServiceBusMessagingV2;
using VAA.EAPI.Microservices.Retail.Order.Payment.Domain.Insurance;
using VAA.EAPI.Microservices.Retail.Order.Payment.Infrastructure.Services.InsurancePurchase;
using VAA.EAPI.Microservices.Retail.Order.Payment.Mapper;

namespace VAA.EAPI.Microservices.Retail.Order.Payment.Application.Command.InsurancePurchase
{
    public class InsurancePurchaseEventCommandHandler : IRequestHandler<InsurancePurchaseEventCommand, Result<Unit>>
    {
        private readonly IMediator _mediator;
        private readonly ILogger<InsurancePurchaseEventCommandHandler> _logger;

        public InsurancePurchaseEventCommandHandler(IMediator mediator, ILogger<InsurancePurchaseEventCommandHandler> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Result<Unit>> Handle(InsurancePurchaseEventCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var enableClearCVV = ConfigReader.Try(Constants.Environment.EnableClearCVV, false);

                var insurancePurchaseRequest = Json.Deserialize<PurchaseRequest>(Convert.ToString(request.Data));

                var response = await _mediator.Send(new InsurancePurchaseServiceCommand { InsurancePurchaseRequest = insurancePurchaseRequest }, cancellationToken);

                var cloudEvent = InsuranceResponseMapper.MapInsuranceResponseToOrderDocument(insurancePurchaseRequest, response.Value.Value);
                await _mediator.Send(new ServiceBusTopicMessageEventCommand { Message = cloudEvent, ServiceBusConnectionString = ConfigReader.Get(Constants.Environment.ServiceBusConnectionString), ServiceBusTopicName = Constants.Application.InsurancePurchaseStatusTopicName }, cancellationToken);

                if (response.IsSuccess && response.Value.Value.Status.Code == "SUCCESS" && enableClearCVV)
                {
                    //send event to clear CVV details
                    var paymentEvent = InsuranceResponseMapper.ClearPaymentDetails(insurancePurchaseRequest);
                    await _mediator.Send(new ServiceBusTopicMessageEventCommand { Message = paymentEvent, ServiceBusConnectionString = ConfigReader.Get(Constants.Environment.ServiceBusConnectionString), ServiceBusTopicName = ConfigReader.Get(Constants.Environment.ServiceBusTopicClearPaymentDetails) }, cancellationToken);
                }

                return Result.Success(Unit.Value);
            }
            catch (Exception ex)
            {                
                _logger.LogError(ex, $"Correlation Id - {AsyncContext.Current.CorrelationId} - Exception - {ex}");
                return Result.Failure<Unit>(ex.Message);
            }

        }
    }
}