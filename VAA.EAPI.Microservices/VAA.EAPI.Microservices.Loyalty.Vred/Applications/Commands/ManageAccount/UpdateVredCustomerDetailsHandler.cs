using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Infrastructure.Repository;
using VAA.EAPI.Infrastructure.Repository.EventStore;
using VAA.EAPI.Microservices.Loyalty.Vred.Domain.Enum;
using VAA.EAPI.Microservices.Loyalty.Vred.Entities.ManageAccount;
using VAA.EAPI.Microservices.Loyalty.Vred.Infrastructure.Repository;
using VAA.EAPI.Microservices.Loyalty.Vred.Infrastructure.Services.Interface;

namespace VAA.EAPI.Microservices.Loyalty.Vred.Applications.Commands.ManageAccount
{
    public class UpdateVredCustomerDetailsHandler(EventStoreRequestInsertCommandHandler eventStoreRequestInsertCommandHandler, EventStoreRequestDeleteCommandHandler eventStoreRequestDeleteCommandHandler, UpdateVredRepoHandler updateVredRepoHandler, ServiceResponseInsertCommandHandler serviceResponseInsertCommandHandler) : IVredCustomerDetails
    {
        private readonly EventStoreRequestInsertCommandHandler _eventStoreRequestInsertCommandHandler = eventStoreRequestInsertCommandHandler ?? throw new ArgumentNullException(nameof(eventStoreRequestInsertCommandHandler));
        private readonly EventStoreRequestDeleteCommandHandler _eventStoreRequestDeleteCommandHandler = eventStoreRequestDeleteCommandHandler ?? throw new ArgumentNullException(nameof(eventStoreRequestDeleteCommandHandler));
        private readonly UpdateVredRepoHandler _updateVredRepoHandler = updateVredRepoHandler ?? throw new ArgumentNullException(nameof(updateVredRepoHandler));
        private readonly ServiceResponseInsertCommandHandler _serviceResponseInsertCommandHandler = serviceResponseInsertCommandHandler ?? throw new ArgumentNullException(nameof(serviceResponseInsertCommandHandler));

        public async Task<bool> Handle(UpdateVredMemberDetailsRequestDataPayload command, CancellationToken cancellationToken)
        {
            if (command == null) throw new ArgumentNullException(nameof(command));

            UpdateVredMemberDetailsRequest request = new UpdateVredMemberDetailsRequest
            {
                MemberNumber = string.IsNullOrWhiteSpace(command.Data?.MemberNumber) ? throw new ArgumentNullException("MemberNumber is required") : command.Data.MemberNumber,
                MemberStatus = string.IsNullOrWhiteSpace(command.Data?.MemberStatus) ? throw new ArgumentNullException("MemberStatus is required") : command.Data.MemberStatus,
                Channel = string.IsNullOrWhiteSpace(command.Data?.Channel) ? throw new ArgumentNullException("Channel is required") : command.Data.Channel,
                CreateUpdateTimestamp = string.IsNullOrWhiteSpace(command.Data?.CreateUpdateTimestamp) ? throw new ArgumentNullException("CreateUpdateTimestamp is required") : command.Data.CreateUpdateTimestamp,
                CustomerId = string.IsNullOrWhiteSpace(command.Data?.CustomerId) ? throw new ArgumentNullException("CustomerId is required") : command.Data.CustomerId,
                FirstName = string.IsNullOrWhiteSpace(command.Data?.FirstName) ? throw new ArgumentNullException("FirstName is required") : command.Data.FirstName,
                LastName = string.IsNullOrWhiteSpace(command.Data?.LastName) ? throw new ArgumentNullException("LastName is required") : command.Data.LastName,
                EmailId = string.IsNullOrWhiteSpace(command.Data?.EmailId) ? throw new ArgumentNullException("EmailId is required") : command.Data.EmailId,
            };

            var result = await _updateVredRepoHandler.Handle(new UpdateVredRepoQuery(request), cancellationToken);

            if (result == true)
            {
                await _serviceResponseInsertCommandHandler.Handle(new ServiceResponseInsertCommand { Id = command.Id, JsonResponseData = Json.Serialize(result), Status = ServiceResponseProcessStatus.Completed, Type = EnumOperations.GetEnumDescription((VredEventType)Enum.Parse(typeof(VredEventType), VredEventType.UpdateVredCustomerDetailEvent.ToString())) }, cancellationToken);

                await _eventStoreRequestInsertCommandHandler.Handle(new EventRequestInsertCommand { Id = command.Id, Type = command.Type, Data = command.Data, Subject = command.Subject }, cancellationToken);

            }
            else
            {
                await _serviceResponseInsertCommandHandler.Handle(new ServiceResponseInsertCommand { Id = command.Id, JsonResponseData = Json.Serialize(result), Status = ServiceResponseProcessStatus.Error, Type = EnumOperations.GetEnumDescription((VredEventType)Enum.Parse(typeof(VredEventType), VredEventType.UpdateVredCustomerDetailEvent.ToString())) }, cancellationToken);

                await _eventStoreRequestDeleteCommandHandler.Handle(new EventStoreRequestDeleteCommand { Id = command.Id, Type = command.Type, Data = command.Data, Subject = command.Subject }, cancellationToken);
            }

            return result;
        }
    }
}
