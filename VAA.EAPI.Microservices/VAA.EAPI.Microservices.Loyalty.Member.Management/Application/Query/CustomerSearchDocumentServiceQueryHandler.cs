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
    public class CustomerSearchDocumentServiceQueryHandler : IRequestHandler<CustomerSearchDocumentServiceQuery, Result<Response<CustomerSearch>>>
    {
        private readonly IHttpConnector _apiConnector;
        private readonly ILogger<CustomerSearchDocumentServiceQueryHandler> _logger;

        public CustomerSearchDocumentServiceQueryHandler(IHttpConnector apiConnector, ILogger<CustomerSearchDocumentServiceQueryHandler> logger)
        {
            _apiConnector = apiConnector ?? throw new ArgumentNullException(nameof(apiConnector));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public virtual async Task<Result<Response<CustomerSearch>>> Handle(CustomerSearchDocumentServiceQuery request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(request.MemberNumber))
                throw new ArgumentException("Member Number must be specified");

            string customerUri = ConfigReader.Get(Constants.Environment.CustomerMSUrl).Replace("{1}", request.MemberNumber).Replace("{0}", Constants.Application.CustomerSearchDocument);

            string token = await ConfigurationHelper.GetBearerTokenCustomerMS();
            var headers = ConfigurationHelper.GetHeader();
            var result = await _apiConnector.GetAsync<CustomerSearch>(uri: customerUri, headers: headers, bearerToken: token);

            return new Response<CustomerSearch>
            {
                Status = Status.OK,
                IgnoreNullValues = false,
                Value = result
            };
        }
    }
}
