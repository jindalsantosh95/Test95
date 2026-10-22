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
using VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.MembershipChange;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Helper;

namespace VAA.EAPI.Microservices.Loyalty.Member.Management.Application.Query
{
    public class MembershipChangeDocumentServiceQuery : IRequest<Result<Response<IEnumerable<MembershipChange>>>>
    {
        public string CustomerId { get; set; }
    }

    public class MembershipChangeDocumentServiceQueryHandler : IRequestHandler<MembershipChangeDocumentServiceQuery, Result<Response<IEnumerable<MembershipChange>>>>
    {
        private readonly IHttpConnector _apiConnector;
        private readonly ILogger<MembershipChangeDocumentServiceQueryHandler> _logger;

        public MembershipChangeDocumentServiceQueryHandler(IHttpConnector apiConnector,
            ILogger<MembershipChangeDocumentServiceQueryHandler> logger)
        {
            _apiConnector = apiConnector;
            _logger = logger;
        }
        public async Task<Result<Response<IEnumerable<MembershipChange>>>> Handle(MembershipChangeDocumentServiceQuery request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Customer Transaction MS method GetMembershipChangeByCustomerID call");
                string endpoint = ConfigReader.Get(Constants.Environment.CustomerTxnMSDetailsEndpoint).Replace("{0}", request.CustomerId).Replace("{1}", Constants.Application.Membershipschange);
                string token = await ConfigurationHelper.GetBearerToken();
                var headers = ConfigurationHelper.GetHeader();
                var result = await _apiConnector.GetAsync<MembershipChangeResponse>(uri: endpoint, headers: headers, bearerToken: token);
                return new Response<IEnumerable<MembershipChange>>
                {
                    Status = Status.OK,
                    Value = result.MembershipChangeDocuments
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error has occured in Customer Transaction MS method GetMembershipChangeByCustomerID call with Customer id:{request.CustomerId}");
                return Result.Failure<Response<IEnumerable<MembershipChange>>>(Json.Serialize(ErrorMapper.MapTechnicalExceptionToEAPIException(EAPI.Infrastructure.Constants.Constants.TechnicalError, Guid.NewGuid().ToString(), EAPI.Infrastructure.Constants.Constants.GlobalExceptionFilePath)));
            }
            finally
            {
                _logger.LogInformation($"MembershipChangeDocumentServiceQueryHandler executed with Customer id:{request.CustomerId}");
            }
        }
    }
}
