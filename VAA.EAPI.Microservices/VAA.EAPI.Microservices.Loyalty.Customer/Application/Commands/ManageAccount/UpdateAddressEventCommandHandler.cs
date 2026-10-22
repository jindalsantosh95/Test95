using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Domain.Events;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Infrastructure.Repository.EventStore;
using VAA.EAPI.Microservices.Loyalty.Customer.Domain.Enums;
using VAA.EAPI.Microservices.Loyalty.Customer.Mapper.ManageAccount;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Application.Commands.ManageAccount
{
    public class UpdateAddressEventCommandHandler : IRequestHandler<UpdateAddressEventCommand, Response<string>>
    {
        private readonly IMediator _mediator;
        private readonly ILogger<UpdateAddressEventCommandHandler> _logger;

        public UpdateAddressEventCommandHandler(IMediator mediator, ILogger<UpdateAddressEventCommandHandler> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Response<string>> Handle(UpdateAddressEventCommand request, CancellationToken cancellationToken)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            try
            {
                var idForEvents = Guid.NewGuid().ToString();

                var cloudEvent = ManageAccountEventMapper.AddressUpdateDataMapper(request.MemberId, request.UpdateAddressRequest.NewAddressDetails, idForEvents);

                await PublishEvent(cloudEvent, idForEvents, cancellationToken);

                request.UpdateAddressRequest.CustomerIds?.ToList().ForEach(customerId =>
                    {
                        var idForLinkedMem = Guid.NewGuid().ToString();
                        var cloudEvent = ManageAccountEventMapper.AddressUpdateDataMapper(customerId, request.UpdateAddressRequest.NewAddressDetails, idForLinkedMem);

                        PublishEvent(cloudEvent, idForLinkedMem, cancellationToken).GetAwaiter().GetResult();
                    });

                return UpdateMemberAddressResponseMapper(idForEvents);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error occured while updating member address details");
                return GetErrorResponse(Constants.ErrorCodes.MiscellaneousTechnicalError);
            }
        }

        private async Task PublishEvent(CloudEvent addressUpdateEvent, string id, CancellationToken cancellationToken)
        {

            await _mediator.Send(new EventRequestInsertCommand { Id = id, Type = Constants.Application.UpdateAddressMAEventType, Data = addressUpdateEvent.Data, Subject = addressUpdateEvent.Subject }, cancellationToken);

            try
            {
                await _mediator.Publish(addressUpdateEvent, cancellationToken);
            }

            catch
            {
                await _mediator.Send(new EventStoreRequestDeleteCommand { Id = id, Type = Constants.Application.UpdateAddressMAEventType, Data = addressUpdateEvent.Data, Subject = addressUpdateEvent.Subject }, cancellationToken);
                throw;
            }
        }

        private static Response<string> GetErrorResponse(string errorCode)
        {
            return new Response<string>()
            {
                Status = Status.BusinessException,
                Errors = new()
                {
                    ErrorMapper.MapBusinessExceptionToEAPIException(new Exception(errorCode), Guid.NewGuid().ToString(), EAPI.Infrastructure.Constants.Constants.ErrorCodesPath).FirstOrDefault()
                }
            };
        }

        public static Response<string> UpdateMemberAddressResponseMapper(string correlationid)
        {
            return new Response<string>
            {
                Status = Status.Accepted,
                HeaderParameters = new List<ResponseHeader> { new() { Key = "location", Value = ConfigReader.Get(Constants.Environment.CustMSUpdateResponseURL).Replace("{CorrelationId}", correlationid).Replace("{UpdateType}", ManageAccountUpdateType.UpdateAddress.ToString()) } },
            };
        }
    }
}
