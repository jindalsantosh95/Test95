using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
    public class UpdatePhoneNumberEventCommandHandler : IRequestHandler<UpdatePhoneNumberEventCommand, Response<string>>
    {
        private readonly IMediator _mediator;
        private readonly ILogger<UpdatePhoneNumberEventCommandHandler> _logger;
      
        public UpdatePhoneNumberEventCommandHandler(IMediator mediator, ILogger<UpdatePhoneNumberEventCommandHandler> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Response<string>> Handle(UpdatePhoneNumberEventCommand request, CancellationToken cancellationToken)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            try
            {
                var correlationIdForEvents = Guid.NewGuid().ToString();

                var cloudEvent = ManageAccountEventMapper.PhoneNumberUpdateDataMapper(request, correlationIdForEvents);

                await PublishEvent(cloudEvent, correlationIdForEvents, cancellationToken);              

                return UpdatePhoneNumberResponseMapper(correlationIdForEvents);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error occured while updating Phone Number details");

                return GetErrorResponse(Constants.ErrorCodes.MiscellaneousTechnicalError);
            }
            
        }

        private async Task PublishEvent(CloudEvent cloudEvent, string correlationIdForEvents, CancellationToken cancellationToken)
        {

            try
            {
                await _mediator.Publish(cloudEvent, cancellationToken);
                await _mediator.Send(new EventRequestInsertCommand { Id = correlationIdForEvents, Type = cloudEvent.Type, Data = cloudEvent.Data, Subject = cloudEvent.Subject }, cancellationToken);
            }
            catch
            {
                await _mediator.Send(new EventStoreRequestDeleteCommand { Id = correlationIdForEvents, Type = cloudEvent.Type, Data = cloudEvent.Data, Subject = cloudEvent.Subject }, cancellationToken);
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

        public static Response<string> UpdatePhoneNumberResponseMapper(string correlationid)
        {
            return new Response<string>
            {
                Status = Status.Accepted,
                HeaderParameters = new List<ResponseHeader> { new() { Key = "location", Value = ConfigReader.Get(Constants.Environment.CustMSUpdateResponseURL).Replace("{CorrelationId}", correlationid).Replace("{UpdateType}", ManageAccountUpdateType.UpdatePhoneNumber.ToString()) } },
            };
        }
    }
}
