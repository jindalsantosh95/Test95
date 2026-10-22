using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using MediatR;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.Award;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Helper;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Infrastructure.Services.CustomerTransactionMs;

namespace VAA.EAPI.Microservices.Loyalty.Member.Management.Application.Query
{
    public class AwardDocumentServiceQueryHandler
        : IRequestHandler<AwardDocumentServiceQuery, Result<Response<IEnumerable<Award>>>>
    {
        private readonly IHttpConnector _apiConnector;
        private readonly ICustomerTransactionTokenProvider _customerTransactionTokenProvider;

        public AwardDocumentServiceQueryHandler(
            IHttpConnector apiConnector,
            ICustomerTransactionTokenProvider customerTransactionTokenProvider)
        {
            _apiConnector = apiConnector ?? throw new ArgumentNullException(nameof(apiConnector));
            _customerTransactionTokenProvider = customerTransactionTokenProvider ??
                throw new ArgumentNullException(nameof(customerTransactionTokenProvider));
        }

        public async Task<Result<Response<IEnumerable<Award>>>> Handle(
            AwardDocumentServiceQuery request,
            CancellationToken cancellationToken)
        {
            try
            {
                var endpoint = ConfigReader.Get(Constants.Environment.CustomerTxnMSDetailsEndpoint)
                    .Replace("{0}", request.CustomerId)
                    .Replace("{1}", Constants.Application.Award);
                var token = await _customerTransactionTokenProvider.AcquireTokenAsync();
                var headers = ConfigurationHelper.GetHeader();
                var result = await _apiConnector.GetAsync<AwardResponse>(
                    uri: endpoint,
                    headers: headers,
                    bearerToken: token);

                return new Response<IEnumerable<Award>>
                {
                    Status = Status.OK,
                    Value = result.Awards
                };
            }
            catch (Exception)
            {
                return Result.Failure<Response<IEnumerable<Award>>>(Json.Serialize(
                    ErrorMapper.MapTechnicalExceptionToEAPIException(
                        EAPI.Infrastructure.Constants.Constants.TechnicalError,
                        Guid.NewGuid().ToString(),
                        EAPI.Infrastructure.Constants.Constants.GlobalExceptionFilePath)));
            }
        }
    }
}
