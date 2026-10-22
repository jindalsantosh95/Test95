using MediatR;
using Microsoft.Azure.Functions.Worker;
using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Microservices.Loyalty.Identity.Application.Command.ManageAccount;
using VAA.EAPI.Microservices.Loyalty.Identity.Domain;
using VAA.EAPI.Microservices.Loyalty.Identity.Infrastructure.Interfaces;
using Environment = VAA.EAPI.Microservices.Loyalty.Identity.Constants.Environment;


namespace VAA.EAPI.Microservices.Loyalty.Identity.Triggers.ServiceBus
{
    public class B2CUpdateSubscriber
    {
        private readonly IMediator _mediator;
        private readonly IDeleteB2CAccountService _deleteB2CAccountService;
        private readonly IGetAdUserByFcNumberRepository _getADuserByFcNumberRepository;
        private readonly IUpdateAzureB2CProfileCommandHandler _updateAzureB2CProfileCommandHandler;

        public B2CUpdateSubscriber(IMediator mediator, IDeleteB2CAccountService deleteB2CAccountService, IGetAdUserByFcNumberRepository getADuserByFcNumberRepository, IUpdateAzureB2CProfileCommandHandler updateAzureB2CProfileCommandHandler)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _deleteB2CAccountService = deleteB2CAccountService ?? throw new ArgumentNullException(nameof(deleteB2CAccountService));
            _getADuserByFcNumberRepository = getADuserByFcNumberRepository ?? throw new ArgumentNullException(nameof(getADuserByFcNumberRepository));
            _updateAzureB2CProfileCommandHandler = updateAzureB2CProfileCommandHandler ?? throw new ArgumentNullException(nameof(updateAzureB2CProfileCommandHandler));
        }

        [Function("B2cUpdateSubscriber")]
        public async Task Run([ServiceBusTrigger(Constants.Application.IdentityEventHandlerTopicName, Constants.Application.UpdateEmailAzureB2CSubscriber, Connection = "ServiceBusConnectionString")] string eventRequestPayload, FunctionContext context)
        {
            await _mediator.Send(Json.Deserialize<UpdateAzureB2CDetailsCommand>(Convert.ToString(eventRequestPayload)));
        }

        [Function("UpdateAzureB2CManageAccountSubscriber")]
        public async Task UpdateAzureB2CManageAccountSubscriber([ServiceBusTrigger(Constants.Application.IdentityEventHandlerTopicName, Constants.Application.UpdateAzureB2CManageAccountSubscriber, Connection = "ServiceBusConnectionString")] string eventRequestPayload)
        {
            await _mediator.Send(Json.Deserialize<UpdateAzureB2CManageAccountCommand>(Convert.ToString(eventRequestPayload)));
        }

        [Function("DeleteAzureB2CAccountSubscriber")]
        public async Task DeleteAzureB2CAccountSubscriber([ServiceBusTrigger(Constants.Application.ManageAccountUpdateTopicName, Constants.Application.DeleteAzureB2CAccountSubscriber, Connection = "ServiceBusConnectionString")] DeleteAzureB2CAccountMessage message)
        {
            var customerId = await _getADuserByFcNumberRepository.GetCustomerIdByFcNumber(message.FcNumber, CancellationToken.None);
            await _deleteB2CAccountService.DeleteB2CAccount(customerId);
        }

        [Function("UpdateAzureB2CProfileSubscriber")]
        public async Task UpdateAzureB2CProfileSubscriber([ServiceBusTrigger(Constants.Application.UpdateAzureB2CProfileTopicName, Constants.Application.UpdateAzureB2CProfileSubscriber, Connection = "ServiceBusConnectionString")] string eventRequestPayload, CancellationToken cancellationToken)
        {
            await _updateAzureB2CProfileCommandHandler.Handle(Json.Deserialize<UpdateAzureB2CProfileCommand>(Convert.ToString(eventRequestPayload)), cancellationToken);

        }
    }
}
