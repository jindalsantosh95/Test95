using MediatR;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;
using VAA.Facade.AzureB2C.Domain.Entities;
using VAA.Facade.AzureB2C.Domain.Model;
using VAA.Facade.AzureB2C.Infrastructure.Services;

namespace VAA.Facade.AzureB2C.Infrastructure.Repositories
{
    public class EnrolmentRequestRepoQueryHandler : IRequestHandler<EnrolmentRequestRepoQuery, Response<EnrolmentResponse>>
    {
        private readonly IMediator _mediator;
        private readonly ILogger<EnrolmentRequestRepoQueryHandler> _logger;

        public EnrolmentRequestRepoQueryHandler(IMediator mediator, ILogger<EnrolmentRequestRepoQueryHandler> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        public async Task<Response<EnrolmentResponse>> Handle(EnrolmentRequestRepoQuery enrolment, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Enrolment Request Repo Query handler");

            var serviceResponse = await _mediator.Send(new EnrolmentServiceQuery(enrolment.EnrolmentServieRequest));
            Response<EnrolmentResponse> response = new Response<EnrolmentResponse>();
            
            if (serviceResponse.Key)
            {
                var enrolmentData = serviceResponse.Value.EnrolmentData;
                response.Status = Status.OK;
                response.Value = new EnrolmentResponse() { CustomerId = enrolmentData.CustomerId, Memberdetails = new Domain.Model.Memberdetails() { Status = enrolmentData.Memberdetails.Status } };
            }
            else
            {                
                response.Errors = new System.Collections.Generic.List<FailureResponse>();
                foreach (var error in serviceResponse.Value.EnrolmentFailedData)
                {
                    response.Errors.Add(new FailureResponse() { Code = error.Code, DeveloperMessage = error.DeveloperMessage, Message = error.Message, ErrorSource = error.ErrorSource });
                    if(error.Code == "E001")
                        response.Status = Status.Error;
                    else
                        response.Status = Status.BusinessException;
                }

            }

            return response;
        }
    }
}