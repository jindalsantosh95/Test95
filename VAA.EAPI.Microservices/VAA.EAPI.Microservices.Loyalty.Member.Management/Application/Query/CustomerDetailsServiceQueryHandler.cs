using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Helper;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Infrastructure.Models;

namespace VAA.EAPI.Microservices.Loyalty.Member.Management.Application.Query
{
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
            if (string.IsNullOrEmpty(request.CustomerId) && string.IsNullOrEmpty(request.MemberId))
                throw new ArgumentException("Either 'Customer ID' or 'Member Number' must be specified");

            string customerUri = !string.IsNullOrEmpty(request.CustomerId)
                ? customerUri = ConfigReader.Get(Constants.Environment.CustomerMSUrlGetbyCustID).Replace("{customerid}", request.CustomerId)
                : customerUri = ConfigReader.Get(Constants.Environment.CustomerMSUrlGetByMemNum).Replace("{memberid}", request.MemberId);

            string token = await ConfigurationHelper.GetBearerTokenCustomerMS();
            var headers = ConfigurationHelper.GetHeader();
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
