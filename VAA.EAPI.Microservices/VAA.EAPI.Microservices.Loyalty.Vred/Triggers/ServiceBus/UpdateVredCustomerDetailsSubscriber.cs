
using Microsoft.Azure.Functions.Worker;
using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Events;
using VAA.EAPI.Microservices.Loyalty.Vred.Applications.Commands.ManageAccount;
using VAA.EAPI.Microservices.Loyalty.Vred.Entities.ManageAccount;
using VAA.EAPI.Microservices.Loyalty.Vred.Infrastructure.Services.Interface;
namespace VAA.EAPI.Microservices.Loyalty.Vred.Triggers.ServiceBus
{
    public class UpdateVredCustomerDetailsSubscriber
    {
        
        private readonly IVredCustomerDetails _vredCustomerDetails;
        public UpdateVredCustomerDetailsSubscriber(IVredCustomerDetails vredCustomerDetails)
        {
           
            _vredCustomerDetails = vredCustomerDetails ?? throw new ArgumentException(nameof(vredCustomerDetails));
        }

        [Function("UpdateVredCustomerDetailsSubscriber")]
        public async Task Run([ServiceBusTrigger(Constants.Application.ManagaeAccountUpdateTopicName, Constants.Application.UpdateVredCustomerDetailsEventSubscriptionName, Connection = "ServiceBusConnectionString")] string eventRequestPayload, FunctionContext context)
        {
            await _vredCustomerDetails.Handle(Json.Deserialize<UpdateVredMemberDetailsRequestDataPayload> (eventRequestPayload), CancellationToken.None);
        }
    }
}
