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
using VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.FlyingClubMember;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Helper;

namespace VAA.EAPI.Microservices.Loyalty.Member.Management.Application.Query
{
    public class FlyingClubMemberDocumentServiceQuery : IRequest<Result<Response<FlyingClubMember>>>
    {
        public string CustomerId { get; set; }
    }
    public class FlyingClubMemberDocumentServiceQueryHandler : IRequestHandler<FlyingClubMemberDocumentServiceQuery, Result<Response<FlyingClubMember>>>
    {
        private readonly IHttpConnector _apiConnector;
        private readonly ILogger<FlyingClubMemberDocumentServiceQueryHandler> _logger;

        public FlyingClubMemberDocumentServiceQueryHandler(IHttpConnector apiConnector,
            ILogger<FlyingClubMemberDocumentServiceQueryHandler> logger)
        {
            _apiConnector = apiConnector;
            _logger = logger;
        }

        public virtual async Task<Result<Response<FlyingClubMember>>> Handle(FlyingClubMemberDocumentServiceQuery request,
            CancellationToken cancellationToken)
        {

            try
            {
                _logger.LogInformation("Customer Transaction MS method FetchFlyingClubMemberDocumentByCustomerIdAsync call");
                string endpoint = ConfigReader.Get(Constants.Environment.CustomerTxnMSDetailsEndpoint).Replace("{1}", request.CustomerId).Replace("{0}", Constants.Application.FlyingClubMember);
                string token = await ConfigurationHelper.GetBearerToken();
                var headers = ConfigurationHelper.GetHeader();
                var result = await _apiConnector.GetAsync<FlyingClubMemberResponse>(uri: endpoint, headers: headers, bearerToken: token);
                return new Response<FlyingClubMember>
                {
                    Status = Status.OK,
                    Value = result.FlyingClubMemberDocuments.FirstOrDefault()
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error has occured in Customer Transaction MS method FetchFlyingClubMemberDocumentByCustomerIdAsync call with Customer id:{request.CustomerId}");
                return Result.Failure<Response<FlyingClubMember>>(Json.Serialize(ErrorMapper.MapTechnicalExceptionToEAPIException(EAPI.Infrastructure.Constants.Constants.TechnicalError, Guid.NewGuid().ToString(), EAPI.Infrastructure.Constants.Constants.GlobalExceptionFilePath)));
            }
            finally
            {
                _logger.LogInformation($"FlyingClubMemberDocumentServiceQueryHandler executed with Customer id:{request.CustomerId}");
            }
        }
    }
}
