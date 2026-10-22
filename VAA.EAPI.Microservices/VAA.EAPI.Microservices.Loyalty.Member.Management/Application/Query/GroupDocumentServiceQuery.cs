using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
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
    public class GroupDocumentServiceQuery : IRequest<Result<Response<HouseHoldGroup>>>
    {
        public string CustomerId { get; set; }
    }

    public class GroupDocumentServiceQueryHandler : IRequestHandler<GroupDocumentServiceQuery, Result<Response<HouseHoldGroup>>>
    {
        private readonly IHttpConnector _apiConnector;
        private readonly ILogger<GroupDocumentServiceQueryHandler> _logger;

        public GroupDocumentServiceQueryHandler(IHttpConnector apiConnector, ILogger<GroupDocumentServiceQueryHandler> logger)
        {
            _apiConnector = apiConnector ?? throw new ArgumentNullException(nameof(apiConnector));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public virtual async Task<Result<Response<HouseHoldGroup>>> Handle(GroupDocumentServiceQuery request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation($"Customer Transaction MS async call to Fetch Group document by HOH customer id started:{request.CustomerId}");

                string endpoint = string.Format(ConfigReader.Get(Constants.Environment.CustomerTxnMSDetailsEndpoint), Constants.Application.FetchGroupDocumentsByHeadCustomerId, request.CustomerId);
                string token = await ConfigurationHelper.GetBearerToken();
                var headers = ConfigurationHelper.GetHeader();
                var result = await _apiConnector.GetAsync<GroupResponse>(uri: endpoint, headers: headers, bearerToken: token);

                return new Response<HouseHoldGroup>
                {
                    Status = Status.OK,
                    IgnoreNullValues = false,
                    Value = result.GroupDocuments?.FirstOrDefault()
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error has occured in Customer Transaction MS async call to Fetch Group document by HOH Customer id:{request.CustomerId}");
                return Result.Failure<Response<HouseHoldGroup>>(Json.Serialize(ErrorMapper.MapTechnicalExceptionToEAPIException(EAPI.Infrastructure.Constants.Constants.TechnicalError, Guid.NewGuid().ToString(), EAPI.Infrastructure.Constants.Constants.GlobalExceptionFilePath)));
            }
            finally
            {
                _logger.LogInformation($"Customer Transaction MS async call to Fetch Group document by HOH customer id ended:{request.CustomerId}");
            }
        }
    }
}
