using HotChocolate;
using HotChocolate.AspNetCore.Authorization;
using HotChocolate.Types;
using Microsoft.Extensions.Logging;
using VAA.EAPI.GraphQL.Common.Helper;
using VAA.EAPI.GraphQL.Common.Serialisation;
using VAA.EAPI.GraphQL.Domain.WebForms.Models;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;

namespace VAA.EAPI.GraphQL.Domain.WebForms.Mutations
{
    [ExtendObjectType("Mutation")]
    [GraphQLDescription("Webform Mutation request")]
    public class PaymentRequestFormMutation
    {
        protected readonly IOAuthTokenProvider _tokenProvider;
        protected readonly IHttpConnector _connector;
        protected readonly ILogger<PaymentRequestFormMutation> _logger;
        public PaymentRequestFormMutation(ILogger<PaymentRequestFormMutation> logger, IOAuthTokenProvider tokenProvider, IHttpConnector connector)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _tokenProvider = tokenProvider ?? throw new ArgumentNullException(nameof(tokenProvider));
            _connector = connector ?? throw new ArgumentNullException(nameof(connector));
        }

        [Authorize(Roles = new[] { "WebForm.Submit" })]
        [GraphQLName("webformUpdatePayment")]
        [GraphQLDescription("Updates Bank Details of a customer in CRM dynanamics and returns status of update from CRM Dynamics")]
        public async Task<PaymentRequestFormMutationResponse> UpdatePayment(string formName, Payment payment)
        {
            if (formName is null)
                throw new ArgumentNullException(nameof(formName));
            if (payment is null)
                throw new ArgumentNullException(nameof(payment));
            string uri = ConfigReader.Get(Common.Constants.Environment.CRMMSUrl);
            if (ConfigReader.Get(Common.Constants.Environment.EnableResponseUrl) == "Y")
            {
                var response = await _connector.SendAsync<PaymentRequestFormMutationResponse>(uri, GetHeaders(), await GetToken(), HttpMethod.Patch, Json.Serialize(payment));
                return response.Response;
            }
            else
            {
                await _connector.SendAsync<string>(uri, GetHeaders(), await GetToken(), HttpMethod.Patch, Json.Serialize(payment));
                return new PaymentRequestFormMutationResponse() { Status = StatusEnum.Success };
            }
        }
        private async Task<string> GetToken()
        {
            return await _tokenProvider.AcquireTokenAsync(ConfigReader.Get(Common.Constants.Environment.CRMMSAppId));
        }

        private static Dictionary<string, string> GetHeaders()
        {
            return new Dictionary<string, string>()
            {
                {
                    Common.Constants.Application.OcpKeyHeader, ConfigReader.Get(Common.Constants.Environment.CRMMSOcpKey)
                }
            };
        }
    }
}