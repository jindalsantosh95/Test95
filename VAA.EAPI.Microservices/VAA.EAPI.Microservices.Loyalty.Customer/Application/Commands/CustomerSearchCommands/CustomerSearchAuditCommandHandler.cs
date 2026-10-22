using CSharpFunctionalExtensions;
using MediatR;
using Newtonsoft.Json.Linq;
using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Infrastructure.Repository;
using VAA.EAPI.Infrastructure.Repository.EventStore;
using VAA.EAPI.Microservices.Loyalty.Customer.Application.Query;
using VAA.EAPI.Microservices.Loyalty.Customer.Domain.CustomerSearch;
using VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Entities;
using VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Mapper.CustomerSearch;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Application.Commands.CustomerSearchCommands
{
    public class CustomerSearchAuditCommandHandler : IRequestHandler<CustomerSearchAuditCommand, Result<Unit>>
    {
        private readonly IMediator _mediator;

        public CustomerSearchAuditCommandHandler(IMediator mediator)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }

        public async Task<Result<Unit>> Handle(CustomerSearchAuditCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var docData = Json.DeserializeObject<CustomerProfileAuditCommandBase>((JToken)request.Data);
                CustomerProfileAuditDTO auditDto = (CustomerProfileAuditDTO)docData.CustomerProfileAuditDocument;

                var existingCustomerDoc = (await _mediator.Send(new FetchCustomerDocumentQuery(auditDto.CustomerId), cancellationToken)).Value.Value;
                auditDto = CustomerToCustomerProfileDataMapper.Mapper(auditDto, existingCustomerDoc);

                //CustomerProfileAudit subscriber call
                CustomerProfileAuditEvent auditUpdateEvent = new()
                {
                    CustomerProfileAuditDocument = auditDto,
                };

                var cloudEvent = CustomerProfileEventToCloudEventMapper.EventToCloudEventMapper(auditUpdateEvent);

                try
                {
                    await _mediator.Send(new EventRequestInsertCommand
                    {
                        Id = cloudEvent.Id,
                        Type = cloudEvent.Type,
                        Data = cloudEvent,
                        Subject = cloudEvent.Subject
                    });

                    await _mediator.Publish(cloudEvent);
                }
                catch
                {
                    await _mediator.Send(new EventStoreRequestDeleteCommand { Id = cloudEvent.Id, Type = cloudEvent.Type, Data = cloudEvent.Data, Subject = cloudEvent.Subject }, CancellationToken.None);
                    throw;
                }

                await _mediator.Send(new ServiceResponseInsertCommand { Id = request.Id, JsonResponseData = Json.Serialize(auditUpdateEvent), Status = ServiceResponseProcessStatus.Completed, Type = Constants.Application.CommandResponse }, cancellationToken);
            }
            catch (Exception ex)
            {
                Result<Response<CustomerSearchAuditCommand>> result = Result.Failure<Response<CustomerSearchAuditCommand>>(ex.Message.ToString());
                await _mediator.Send(new ServiceResponseInsertCommand { Id = request.Id, JsonResponseData = Json.Serialize(result), Status = result.IsFailure ? ServiceResponseProcessStatus.Error : ServiceResponseProcessStatus.Completed, Type = Constants.Application.CommandResponse });
            }

            return Unit.Value;
        }
    }
}
