using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Serialisation;

namespace VAA.Facade.AzureB2C.Application.Validations
{
    class ValidateB2CRequestHandler : IRequestHandler<ValidateB2CRequest, bool>
    {
        private readonly IMediator _mediator;
        private readonly ILogger<ValidateB2CRequestHandler> _logger;

        public ValidateB2CRequestHandler(IMediator mediator, ILogger<ValidateB2CRequestHandler> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        public async Task<bool> Handle(ValidateB2CRequest request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Validating B2C Request For enrolment API");              

                Dictionary<string, object> b2CRequest = Json.DeserializeObject<Dictionary<string, object>>(request.B2CRequest);
                               
                if (!b2CRequest.ContainsKey("step") || 
                    !b2CRequest.ContainsKey("surname") || 
                    !b2CRequest.ContainsKey("givenName") || 
                    !b2CRequest.ContainsKey("email") ||
                    !b2CRequest.ContainsKey("identities"))
                    return false;
                else
                    return true;
            }
            catch(Exception exception)
            {
                _logger.LogInformation("Validating B2C Request Failed");
                _logger.LogError(exception, "ValidateB2CRequestHandler Failed");
                return false;
            }

        }
    }
}
