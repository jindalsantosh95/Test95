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
using VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.Customer;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Helper;

namespace VAA.EAPI.Microservices.Loyalty.Member.Management.Application.Query
{
    public class CustomerDocumentServiceQueryHandler : IRequestHandler<CustomerDocumentServiceQuery, Result<Response<Customer>>>
    {
        private readonly IHttpConnector _apiConnector;
        private readonly ILogger<CustomerDocumentServiceQueryHandler> _logger;

        public CustomerDocumentServiceQueryHandler(IHttpConnector apiConnector, ILogger<CustomerDocumentServiceQueryHandler> logger)
        {
            _apiConnector = apiConnector ?? throw new ArgumentNullException(nameof(apiConnector));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Result<Response<Customer>>> Handle(CustomerDocumentServiceQuery request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(request.CustomerId))
                throw new ArgumentException("Customer ID must be specified");

            string customerUri = ConfigReader.Get(Constants.Environment.CustomerMSUrl).Replace("{1}", request.CustomerId).Replace("{0}", Constants.Application.CustomerDocument);

            string token = await ConfigurationHelper.GetBearerTokenCustomerMS();
            var headers = ConfigurationHelper.GetHeader();
            var result = await _apiConnector.GetAsync<Customer>(uri: customerUri, headers: headers, bearerToken: token);

            return new Response<Customer>
            {
                Status = Status.OK,
                IgnoreNullValues = false,
                Value = result
            };
        }
    }
}
