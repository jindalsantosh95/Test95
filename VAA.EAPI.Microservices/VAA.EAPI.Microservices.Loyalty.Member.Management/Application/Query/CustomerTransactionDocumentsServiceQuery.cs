using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.Transaction;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Helper;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Infrastructure.Services.CustomerTransactionMs;

namespace VAA.EAPI.Microservices.Loyalty.Member.Management.Application.Query
{
    public class CustomerTransactionDocumentsServiceQuery : IRequest<Result<Response<IEnumerable<TransactionDocument>>>>
    {
        public string CustomerId { get; set; }
    }

    public class CustomerTransactionDocumentsServiceQueryHandler
        : IRequestHandler<CustomerTransactionDocumentsServiceQuery, Result<Response<IEnumerable<TransactionDocument>>>>
    {
        private readonly IHttpConnector _apiConnector;
        private readonly ICustomerTransactionTokenProvider _customerTransactionTokenProvider;
        private readonly ILogger<CustomerTransactionDocumentsServiceQueryHandler> _logger;

        public CustomerTransactionDocumentsServiceQueryHandler(
            IHttpConnector apiConnector,
            ICustomerTransactionTokenProvider customerTransactionTokenProvider,
            ILogger<CustomerTransactionDocumentsServiceQueryHandler> logger)
        {
            _apiConnector = apiConnector ?? throw new ArgumentNullException(nameof(apiConnector));
            _customerTransactionTokenProvider = customerTransactionTokenProvider ??
                throw new ArgumentNullException(nameof(customerTransactionTokenProvider));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Result<Response<IEnumerable<TransactionDocument>>>> Handle(CustomerTransactionDocumentsServiceQuery request,
            CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Customer Transaction MS method GetTransactionDocumentByCustomerID call");
                string endpoint = ConfigReader.Get(Constants.Environment.CustomerTxnMSDetailsEndpoint).Replace("{0}", request.CustomerId).Replace("{1}", Constants.Application.Transaction);
                string token = await _customerTransactionTokenProvider.AcquireTokenAsync();
                var headers = ConfigurationHelper.GetHeader();
                var result = await _apiConnector.GetAsync<TransactionResponse>(uri: endpoint, headers: headers, bearerToken: token);
                return new Response<IEnumerable<TransactionDocument>>
                {
                    Status = Status.OK,
                    Value = result.TransactionDocuments
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error has occured in Customer Transaction MS method GetTransactionDocumentByCustomerID call with Customer id:{request.CustomerId}");
                return Result.Failure<Response<IEnumerable<TransactionDocument>>>(Json.Serialize(ErrorMapper.MapTechnicalExceptionToEAPIException(EAPI.Infrastructure.Constants.Constants.TechnicalError, Guid.NewGuid().ToString(), EAPI.Infrastructure.Constants.Constants.GlobalExceptionFilePath)));
            }
            finally
            {
                _logger.LogInformation($"CustomerTransactionDocumentsServiceQueryHandler executed with Customer id:{request.CustomerId}");
            }
        }
    }
}
