using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Infrastructure.Models;

namespace VAA.EAPI.Microservices.Loyalty.Campaign.Management.Application.Query
{
    public class CustomerDetailsServiceQuery : IRequest<Result<Response<CustomerDTO>>>
    {
        public string CustomerId { get; set; }
        public string MemberNumber { get; set; }
    }

    public class CustomerDetailsServiceQueryHandler : IRequestHandler<CustomerDetailsServiceQuery, Result<Response<CustomerDTO>>>
    {
        private readonly IHttpConnector _apiConnector;
        private readonly ILogger<CustomerDetailsServiceQueryHandler> _logger;

        public CustomerDetailsServiceQueryHandler(IHttpConnector apiConnector, ILogger<CustomerDetailsServiceQueryHandler> logger)
        {
            _apiConnector = apiConnector ?? throw new ArgumentNullException(nameof(apiConnector));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Result<Response<CustomerDTO>>> Handle(CustomerDetailsServiceQuery request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(request.CustomerId) && string.IsNullOrEmpty(request.MemberNumber))
                throw new ArgumentException("Either 'Customer ID' or 'Member Number' must be specified");

            string customerUri = !string.IsNullOrEmpty(request.CustomerId)
                ? customerUri = ConfigReader.Get(Constants.Environment.CustomerMSUrlGetbyCustID).Replace("{customerid}", request.CustomerId)
                : customerUri = ConfigReader.Get(Constants.Environment.CustomerMSUrlGetByMemNum).Replace("{memberid}", request.MemberNumber);

            string clientId = ConfigReader.Get(Constants.Environment.CampaignManagementAppId);
            string clientSecret = ConfigReader.Get(Constants.Environment.CampaignManagementClientSecret);
            string authority = ConfigReader.Get(Constants.Environment.AzureADEndpoint);
            string resource = ConfigReader.Get(Constants.Environment.CustomerMSResourceId);
            string token = await ExternalHelper.GetBearerToken(clientId, clientSecret, authority, resource);

            Dictionary<string, string> headers = new()
            {
                { Constants.Environment.OcpKeyHeader, ConfigReader.Get(Constants.Environment.CampaignManagementOcpKey) }
            };

            var result = await _apiConnector.GetAsync<CustomerDTO>(uri: customerUri, headers: headers, bearerToken: token);

            return new Response<CustomerDTO>
            {
                Status = Status.OK,
                IgnoreNullValues = false,
                Value = result
            };
        }
    }
}
