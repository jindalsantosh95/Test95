using MediatR;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.NDC.Order.Infrastructure.Model.ValidationMS;

namespace VAA.EAPI.NDC.Order.Infrastructure.Services.ValidationMS
{
    public class ValidationMSCommandHandler : IRequestHandler<ValidationMSCommand, ValidationRS>
    {
        private readonly IHttpConnector _httpConnector;

        public ValidationMSCommandHandler(IHttpConnector httpConnector)
        {
            _httpConnector = httpConnector ?? throw new ArgumentNullException(nameof(httpConnector));
        }

        public async Task<ValidationRS> Handle(ValidationMSCommand request, CancellationToken cancellationToken)
        {
            var headers = new Dictionary<string, string>()
            {
                { "Ocp-Apim-Subscription-Key", ConfigReader.Get(Constants.Environment.ValidationMSSubscriptionKey)},
                { "agentSubscriptionKey", request.AgentSubscriptionKey},
                { "Environment", ConfigReader.Get(Constants.Environment.AgentRegistrationEnvironment)}
            };

            var httpResponseMessage = await _httpConnector.SendAsync(ConfigReader.Get(Constants.Environment.ValidationMSUrl), headers, string.Empty, HttpMethod.Post, Json.Serialize(request.AgencyValidateRequest));
            var response = await httpResponseMessage.Content.ReadAsStringAsync(cancellationToken);

            if (httpResponseMessage.StatusCode == HttpStatusCode.OK)
                return new ValidationRS() { ValidateResponse = Json.Deserialize<AgencyValidateResponse>(response) };
            else
                return new ValidationRS() { Error = Json.Deserialize<IEnumerable<ErrorDetails>>(response) };
        }
    }
}
