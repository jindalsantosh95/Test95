using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.HoHAccount;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Helper;

namespace VAA.EAPI.Microservices.Loyalty.Member.Management.Application.Query
{
    public class GroupHistoryDocumentServiceQuery : IRequest<Result<Response<GroupResponse>>>
    {
        public string CustomerId { get; set; }
    }

    public class GroupHistoryDocumentServiceQueryHandler : IRequestHandler<GroupHistoryDocumentServiceQuery, Result<Response<GroupResponse>>>
    {
        private readonly IHttpConnector _apiConnector;
        private readonly ILogger<GroupHistoryDocumentServiceQueryHandler> _logger;

        public GroupHistoryDocumentServiceQueryHandler(IHttpConnector apiConnector, ILogger<GroupHistoryDocumentServiceQueryHandler> logger)
        {
            _apiConnector = apiConnector ?? throw new ArgumentNullException(nameof(apiConnector));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public virtual async Task<Result<Response<GroupResponse>>> Handle(GroupHistoryDocumentServiceQuery request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation($"Customer Transaction MS async call to Fetch Group document by group history customer id started:{request.CustomerId}");

                string endpoint = string.Format(ConfigReader.Get(Constants.Environment.CustomerTxnMSDetailsEndpoint), Constants.Application.FetchGroupDocumentsFromGroupHistoryCustomerId, request.CustomerId);
                string token = await ConfigurationHelper.GetBearerToken();
                var headers = ConfigurationHelper.GetHeader();
                var result = await _apiConnector.GetAsync<GroupResponse>(uri: endpoint, headers: headers, bearerToken: token);

                return new Response<GroupResponse>
                {
                    Status = Status.OK,
                    IgnoreNullValues = false,
                    Value = result
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error has occured in Customer Transaction MS async call to Fetch Group document by group history Customer id:{request.CustomerId}");
                return Result.Failure<Response<GroupResponse>>(Json.Serialize(ErrorMapper.MapTechnicalExceptionToEAPIException(EAPI.Infrastructure.Constants.Constants.TechnicalError, Guid.NewGuid().ToString(), EAPI.Infrastructure.Constants.Constants.GlobalExceptionFilePath)));
            }
            finally
            {
                _logger.LogInformation($"Customer Transaction MS async call to Fetch Group document by group history customer id ended:{request.CustomerId}");
            }
        }
    }
}
