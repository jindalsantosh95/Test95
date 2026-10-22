using MediatR;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using VAA.Facade.AzureB2C.Domain.Model;
using VAA.Facade.AzureB2C.Domain.Entities;
using VAA.Facade.AzureB2C.Shared;
using VAA.Facade.AzureB2C.Mapper;
using VAA.Facade.AzureB2C.Infrastructure.Repositories;
using System;
using VAA.Facade.AzureB2C.Application.Validations;

namespace VAA.Facade.AzureB2C.Application.Command
{
    public class B2CRequestCommandHandler : IRequestHandler<B2CRequestCommand, Result<Response<B2CResponse>>>
    {
        private readonly IMediator _mediator;
        private readonly ILogger<B2CRequestCommandHandler> _logger;

        public B2CRequestCommandHandler(IMediator mediator, ILogger<B2CRequestCommandHandler> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }
        public async Task<Result<Response<B2CResponse>>> Handle(B2CRequestCommand request, CancellationToken cancellationToken)
        {

            _logger.LogInformation("B2C Request Command handler");
            Response<B2CResponse> response = new Response<B2CResponse>();
            try
            {        
                if (request.B2CRequest.Step == Constants.B2CStepPostAttributeCollection)
                {
                    var enrolmentResponse = await _mediator.Send(new EnrolmentRequestRepoQuery(request.B2CRequest));                   
                    response.Status = enrolmentResponse.Status;
                    if (enrolmentResponse.Value != null)
                    {
                        _logger.LogInformation("B2C Request Command Set Success response handler");
                        dynamic requestData = request.B2CRequest;
                        response.Value = new B2CResponse()
                        {
                            extension_customerId = enrolmentResponse.Value.CustomerId,
                            displayName = request.B2CRequest.Identities[0].SignInType == "federated" ? requestData.displayName : requestData.givenName + " " + requestData.surname,
                            extension_staffMember = request.B2CRequest.Identities[0].SignInType == "federated" ? true : false,
                            extension_MemberStatus = enrolmentResponse.Value.Memberdetails.Status                            
                        };
                    }
                    else
                    {
                        _logger.LogInformation("B2C Request Command Set Failed response handler");
                        response.Value = EnrolmentErrorCodeMapping.MapEnrolmentErrorToB2CResponse(enrolmentResponse.Errors);
                        response.Errors = enrolmentResponse.Errors;
                    }                   
                }
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "B2C Request Command handler Failed");
                response.Value = new B2CResponse() { action = Constants.B2CActionBlockPage, userMessage = "Miscellenous Technical Error Occured." };
                response.Status = Status.Error;
            }
            return response;
        }
    }
}
