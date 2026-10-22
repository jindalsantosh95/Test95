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
using VAA.EAPI.NDC.Offer.Infrastructure.Model.ValidationMS;

namespace VAA.EAPI.NDC.Offer.Infrastructure.Services.ValidationMS
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

            using var httpResponseMessage = await _httpConnector.SendAsync(ConfigReader.Get(Constants.Environment.ValidationMSUrl), headers, string.Empty, HttpMethod.Post, Json.Serialize(request.AgencyValidateRequest));
            using var responseStream = await httpResponseMessage.Content.ReadAsStreamAsync(cancellationToken);

            if (httpResponseMessage.StatusCode == HttpStatusCode.OK)
                return new ValidationRS() { ValidateResponse = await Json.DeserializeAsync<AgencyValidateResponse>(responseStream, cancellationToken) };
            else
            {
                try
                {
                    return new ValidationRS() { Error = await Json.DeserializeAsync<IEnumerable<ErrorDetails>>(responseStream, cancellationToken) };
                }
                catch (Exception)
                {
                    return new ValidationRS() { Error = new ErrorDetails[] { new ErrorDetails() { Code = httpResponseMessage.StatusCode.ToString(), Message = httpResponseMessage.ReasonPhrase ?? httpResponseMessage.StatusCode.ToString() } } };
                }
            }
        }
    }
}
