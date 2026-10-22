using CSharpFunctionalExtensions;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Infrastructure.Models.CustProfile;

namespace VAA.EAPI.Microservices.Loyalty.Member.Management.Infrastructure.Application.Query.CustProfileMS
{
    public class CustomerProfileQueryHandler : ICustomerProfileQuery
    {
        private readonly IHttpConnector _httpConnector;
        private readonly IOAuthTokenProvider _tokenProvider;

        public CustomerProfileQueryHandler(IHttpConnector httpConnector,IOAuthTokenProvider tokenProvider)
        {
            _httpConnector = httpConnector ?? throw new ArgumentNullException(nameof(httpConnector));
            _tokenProvider = tokenProvider ?? throw new ArgumentNullException(nameof(tokenProvider));

        }
        public async Task<Result<Response<CustomerProfileResponse>>> Handle(CustomerProfileRequest request, CancellationToken cancellationToken)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (request.CustomerId.IsNullOrWhiteSpace()) throw new ArgumentNullException(nameof(request.CustomerId));

            var headers = new Dictionary<string, string>()
            {
                {"Ocp-Apim-Subscription-Key", ConfigReader.Get(Constants.Environment.CustomerTransactionMSSubscriptionKey)}
            };

            var token = await _tokenProvider.AcquireTokenAsync(ConfigReader.Get(Constants.Environment.CustomerTxnMSResourceId));

            var url = $"{ConfigReader.Get(Constants.Environment.CustomerProfileUrl)}".Replace("{CustomerId}", request.CustomerId);

            var result = await _httpConnector.GetAsync<CustomerProfileResponse>(url, headers, bearerToken: token);

            return new Response<CustomerProfileResponse>
            {
                Status = Status.OK,
                IgnoreNullValues = false,
                Value = result
            };
        }
    }
}
