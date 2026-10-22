using MediatR;
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
using VAA.EAPI.Microservices.Loyalty.Customer.Mapper.EnrolmentCustomer;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Application.Commands.VacEnrolment
{
    public class VaEnrolmentCommandHandler : IRequestHandler<VaEnrolmentCommand, Response<string>>
    {
        private readonly IMediator _mediator;

        public VaEnrolmentCommandHandler(IMediator mediator)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }
        public async Task<Response<string>> Handle(VaEnrolmentCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var correlationIdForEvents = Guid.NewGuid().ToString();

                await PublishEvent(EventGridEventMapper.DeDupDataMapper(request.CustomerDetails, correlationIdForEvents), correlationIdForEvents, cancellationToken);

                return VaEnrolmentResponseMapper(correlationIdForEvents);

            }
            catch (Exception)
            {
                return GetErrorResponse(Constants.ErrorCodes.MiscellaneousTechnicalError);
            }
        }

        private async Task PublishEvent(CloudEvent deDupValidateEvent, string correlationIdForEvents, CancellationToken cancellationToken)
        {

            await _mediator.Send(new EventRequestInsertCommand { Id = correlationIdForEvents, Type = Constants.Application.DeDupValidateEventType, Data = deDupValidateEvent.Data, Subject = deDupValidateEvent.Subject }, cancellationToken);

            try
            {
                await _mediator.Publish(deDupValidateEvent, cancellationToken);
            }

            catch
            {
                await _mediator.Send(new EventStoreRequestDeleteCommand { Id = correlationIdForEvents, Type = Constants.Application.DeDupValidateEventType, Data = deDupValidateEvent.Data, Subject = deDupValidateEvent.Subject }, cancellationToken);
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

        private static Response<string> VaEnrolmentResponseMapper(string correlationid)
        {
            return new Response<string>
            {
                Status = Status.Accepted,
                HeaderParameters = new List<ResponseHeader> { new() { Key = "location", Value = ConfigReader.Get(Constants.Environment.CustMSVaEnrolResponseURL).Replace("{CorrelationId}", correlationid) } },
            };
        }        
    }
}
